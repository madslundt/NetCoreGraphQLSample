using System;
using System.Linq;
using System.Reflection;
using App.Metrics;
using App.Metrics.Reporting.InfluxDB;
using API.Infrastructure.Filter;
using CorrelationId;
using DataModel;
using DataModel.Models;
using Hangfire;
using Hangfire.Dashboard;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StructureMap;
using GQL = GraphQL;
using GraphQL.Http;
using GraphQL.Types;
using GraphQL.Server;
using GraphQL.Server.Ui.Playground;
using GraphQL;
using Microsoft.AspNetCore.Http;
using Features.User;
using API.Infrastructure.GraphQL;
using Microsoft.EntityFrameworkCore;

namespace API
{
    public class Startup
    {
        public Startup(IHostingEnvironment env, ILogger<Startup> logger)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile(path: $"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true) 
                .AddEnvironmentVariables();

            Configuration = builder.Build();
            _logger = logger;
            _env = env;
        }

        public IConfigurationRoot Configuration { get; }
        private readonly ILogger<Startup> _logger;
        private readonly IHostingEnvironment _env;

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            // GraphQL
            services.AddSingleton<IDependencyResolver>(s => new FuncDependencyResolver(s.GetRequiredService));

            services.AddSingleton<IDocumentExecuter, DocumentExecuter>();
            services.AddSingleton<IDocumentWriter, DocumentWriter>();

            _ = services.AddGraphQL(o =>
              {
                  o.ExposeExceptions = true;
                  o.ComplexityConfiguration = new GQL.Validation.Complexity.ComplexityConfiguration { MaxDepth = 15 };
              })
            .AddGraphTypes(ServiceLifetime.Singleton);
            services.AddSingleton<ISchema, GraphQL.Schemas>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


            services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(Configuration.GetConnectionString(ConnectionStringKeys.App)));

            services.AddHangfire(x => x.UseSqlServerStorage(Configuration.GetConnectionString(ConnectionStringKeys.Hangfire)));

            services.AddCorrelationId();

            services.AddOptions();

            var metricsConfigSection = Configuration.GetSection(nameof(MetricsOptions));
            var influxOptions = new MetricsReportingInfluxDbOptions();
            Configuration.GetSection(nameof(MetricsReportingInfluxDbOptions)).Bind(influxOptions);

            var metrics = AppMetrics.CreateDefaultBuilder()
                .Configuration.Configure(metricsConfigSection.AsEnumerable())
                .Report.ToInfluxDb(influxOptions)
                .Build();

            services.AddMetrics(metrics);
            services.AddMetricsTrackingMiddleware();
            services.AddMetricsEndpoints();
            services.AddMetricsReportingHostedService();

            // Context
            services.AddSingleton<IUserContext, Features.User.UserContext>();

            // Pipeline
            services.AddMvc(opt => { opt.Filters.Add(typeof(ExceptionFilter)); })
                .AddMetrics()
                .AddControllersAsServices()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // Identity
            var identityOptions = new Infrastructure.Identity.IdentityOptions();
            Configuration.GetSection(nameof(Infrastructure.Identity.IdentityOptions)).Bind(identityOptions);

            services.AddIdentity<User, Role>(options =>
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequiredLength = 6;
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<DatabaseContext>()
                .AddDefaultTokenProviders();

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = identityOptions.Authority;
                    options.ApiName = identityOptions.ApiName;
                    options.ApiSecret = identityOptions.ApiSecret;
                    options.RequireHttpsMetadata = _env.IsProduction();
                    options.EnableCaching = true;
                    options.CacheDuration = TimeSpan.FromMinutes(10);
                });

            IContainer container = new Container();
            container.Configure(config =>
            {
                config.Populate(services);
            });

            metrics.ReportRunner.RunAllAsync();

            // Check for missing dependencies
            var controllers = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(ControllerBase).IsAssignableFrom(type))
                .ToList();

            var sp = services.BuildServiceProvider();
            foreach (var controllerType in controllers)
            {
                _logger.LogInformation($"Found {controllerType.Name}");
                try
                {
                    sp.GetService(controllerType);
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, $"Cannot create instance of controller {controllerType.FullName}, it is missing some services");
                }
            }

            services.AddLogging(builder => builder
                .AddConfiguration(Configuration)
                .AddConsole()
                .AddDebug()
                .AddEventSourceLogger()
                .AddSentry());

            return container.GetInstance<IServiceProvider>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app, 
            IHostingEnvironment env,
            ILoggerFactory loggerFactory)
        {
            app.UseCorrelationId(new CorrelationIdOptions
            {
                UseGuidForCorrelationId = true
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();

                app.UseGraphQLPlayground(new GraphQLPlaygroundOptions());
            }

            app.UseMiddleware<GraphQLMiddleware>(new GraphQLSettings
            {
                BuildUserContext = ctx => new GraphQLUserContext
                {
                    User = ctx.User
                }
            });

            app.UseStaticFiles();
            app.UseMetricsAllEndpoints();
            app.UseMetricsAllMiddleware();

            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                SchedulePollingInterval = TimeSpan.FromSeconds(30),
                ServerCheckInterval = TimeSpan.FromMinutes(1),
                ServerName = $"{Environment.MachineName}.{Guid.NewGuid()}",
                WorkerCount = Environment.ProcessorCount * 5
            });

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                IsReadOnlyFunc = (DashboardContext context) => true,
                Authorization = new[] { new MyAuthorizationFilter() }
            });
            app.UseAuthentication();

            app.UseGraphQL<Schema>();
        }

        public class MyAuthorizationFilter : IDashboardAuthorizationFilter
        {
            public bool Authorize(DashboardContext context)
            {
                var httpContext = context.GetHttpContext();

                // Allow all authenticated users to see the Dashboard (potentially dangerous).
                //return httpContext.User.Identity.IsAuthenticated;
                return true;
            }
        }
    }
}
