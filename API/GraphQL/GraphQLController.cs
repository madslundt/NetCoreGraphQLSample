using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using API.Infrastructure.GraphQL;
using GraphQL;
using GraphQL.Http;
using GraphQL.Instrumentation;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;

namespace API.GraphQL
{
    [Route("api/graphql")]
    public class GraphQLController : Controller
    {
        private readonly ISchema _schema;
        private readonly IDocumentExecuter _executer;
        private readonly IDocumentWriter _writer;

        public GraphQLController(
            IDocumentExecuter executer,
            IDocumentWriter writer,
            ISchema schema)
        {
            _executer = executer;
            _writer = writer;
            _schema = schema;
        }

        [HttpGet]
        public Task<HttpResponseMessage> GetAsync(HttpRequestMessage request)
        {
            return PostAsync(request, new GraphQLRequest { Query = "query user { id }", Variables = null });
        }

        [HttpPost]
        public async Task<HttpResponseMessage> PostAsync(HttpRequestMessage request, GraphQLRequest query)
        {
            var inputs = query.Variables.ToInputs();

            var result = await _executer.ExecuteAsync(_ =>
            {
                _.Schema = _schema;
                _.Query = query.Query;
                _.OperationName = query.OperationName;
                _.Inputs = inputs;

                _.FieldMiddleware.Use<InstrumentFieldsMiddleware>();

            }).ConfigureAwait(false);

            var httpResult = result.Errors?.Count > 0
                ? HttpStatusCode.BadRequest
                : HttpStatusCode.OK;

            var json = _writer.Write(result);

            var response = request.CreateResponse(httpResult);
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");

            return response;
        }
    }
}
