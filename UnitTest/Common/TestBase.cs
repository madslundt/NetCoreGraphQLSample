using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using API.GraphQL;
using AutoFixture;
using DataModel;
using FluentAssertions;
using GraphQL;
using GraphQL.Conversion;
using GraphQL.Http;
using GraphQL.Server;
using GraphQL.Types;
using GraphQL.Validation;
using GraphQLParser.Exceptions;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StructureMap;

namespace UnitTest.Common
{
    public class TestBase : IDisposable
    {
        protected readonly DatabaseContext _db;
        protected readonly Mock<IBackgroundJobClient> _jobClientMock;
        protected readonly Fixture _fixture;

        private readonly IDocumentExecuter _executer;
        private readonly IDocumentWriter _writer;
        private readonly ISchema _schema;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public TestBase()
        {
            var services = new ServiceCollection();

            // Services
            services.AddMvc();


            // GraphQL
            services.AddSingleton<IDependencyResolver>(s => new FuncDependencyResolver(s.GetRequiredService));

            _executer = new DocumentExecuter();
            _writer = new DocumentWriter(indent: true);

            services.AddGraphQL(o =>
            {
                o.ExposeExceptions = true;
                o.ComplexityConfiguration = new GraphQL.Validation.Complexity.ComplexityConfiguration { MaxDepth = 15 };
            })
            .AddGraphTypes(ServiceLifetime.Singleton);
            services.AddSingleton<ISchema, RootSchema>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


            // Database
            _db = new DatabaseContext(DatabaseContextMock<DatabaseContext>.InMemoryDatabase());


            // Global objects
            _jobClientMock = new Mock<IBackgroundJobClient>();
            _jobClientMock.Setup(x => x.Create(It.IsAny<Job>(), It.IsAny<EnqueuedState>()));

            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                     .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());


            IContainer container = new Container(cfg =>
            {
                cfg.For<IBackgroundJobClient>().Use(_jobClientMock.Object);
                cfg.For<ISchema>().Use<RootSchema>();
                cfg.For<DatabaseContext>().Use(_db);
                cfg.For(typeof(ILogger<>)).Use(typeof(NullLogger<>));
                cfg.Populate(services);
            });

            _schema = container.GetInstance<ISchema>();

            _jsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.Indented
            };
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        public string SerializeObject(object obj)
        {
            var result = JsonConvert.SerializeObject(obj, _jsonSerializerSettings);

            return result;
        }

        public object DeserializeObject(string json)
        {
            var result = JsonConvert.DeserializeObject(json, _jsonSerializerSettings);

            return result;
        }

        public T DeserializeObject<T>(string json)
        {
            var result = JsonConvert.DeserializeObject<T>(json, _jsonSerializerSettings);

            return result;
        }

        public async Task<ExecutionResult> AssertQuerySuccess(
            string query,
            string expected,
            Inputs inputs = null,
            object root = null,
            object userContext = null,
            CancellationToken cancellationToken = default(CancellationToken),
            IEnumerable<IValidationRule> rules = null)
        {
            var queryResult = CreateQueryResult(expected);
            return await AssertQuery(query, queryResult, inputs, root, userContext, cancellationToken, rules);
        }

        public async Task<ExecutionResult> AssertQueryWithErrors(
            string query,
            string expected,
            int expectedErrorCount = 0,
            Inputs inputs = null,
            object root = null,
            object userContext = null,
            CancellationToken cancellationToken = default(CancellationToken),
            bool renderErrors = false)
        {
            var queryResult = CreateQueryResult(expected);
            return await AssertQueryIgnoreErrors(
                query,
                queryResult,
                inputs,
                root,
                userContext,
                cancellationToken,
                expectedErrorCount,
                renderErrors);
        }

        public async Task<ExecutionResult> AssertQueryIgnoreErrors(
            string query,
            ExecutionResult expectedExecutionResult,
            Inputs inputs = null,
            object root = null,
            object userContext = null,
            CancellationToken cancellationToken = default(CancellationToken),
            int expectedErrorCount = 0,
            bool renderErrors = false)
        {
            var runResult = await _executer.ExecuteAsync(_ =>
            {
                _.Schema = _schema;
                _.Query = query;
                _.Root = root;
                _.Inputs = inputs;
                _.UserContext = userContext;
                _.CancellationToken = cancellationToken;
            });

            var renderResult = renderErrors ? runResult : new ExecutionResult { Data = runResult.Data };

            var writtenResult = await _writer.WriteToStringAsync(renderResult);
            var expectedResult = await _writer.WriteToStringAsync(expectedExecutionResult);

            // #if DEBUG
            //             Console.WriteLine(writtenResult);
            // #endif

            writtenResult.Should().Be(expectedResult);

            var errors = runResult.Errors ?? new ExecutionErrors();

            errors.Count().Should().Be(expectedErrorCount);

            return runResult;
        }

        private async Task<ExecutionResult> AssertQuery(
            string query,
            ExecutionResult expectedExecutionResult,
            Inputs inputs,
            object root,
            object userContext = null,
            CancellationToken cancellationToken = default(CancellationToken),
            IEnumerable<IValidationRule> rules = null)
        {
            var runResult = await _executer.ExecuteAsync(_ =>
            {
                _.Schema = _schema;
                _.Query = query;
                _.Root = root;
                _.Inputs = inputs;
                _.UserContext = userContext;
                _.CancellationToken = cancellationToken;
                _.ValidationRules = rules;
                _.FieldNameConverter = new CamelCaseFieldNameConverter();
            });

            var writtenResult = await _writer.WriteToStringAsync(runResult);
            var expectedResult = await _writer.WriteToStringAsync(expectedExecutionResult);

            // #if DEBUG
            //             Console.WriteLine(writtenResult);
            // #endif

            string additionalInfo = null;

            if (runResult.Errors?.Any() == true)
            {
                additionalInfo = string.Join(Environment.NewLine, runResult.Errors
                    .Where(x => x.InnerException is GraphQLSyntaxErrorException)
                    .Select(x => x.InnerException.Message));
            }

            writtenResult.Should().Be(expectedResult, additionalInfo);

            return runResult;
        }

        private ExecutionResult CreateQueryResult(string result, ExecutionErrors errors = null)
        {
            object data = null;
            if (!string.IsNullOrWhiteSpace(result))
            {
                data = DeserializeObject(result);
            }

            return new ExecutionResult { Data = data, Errors = errors };
        }
    }
}
