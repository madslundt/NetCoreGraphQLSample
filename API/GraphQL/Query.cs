using API.GraphQL.Queries;
using GraphQL.Types;

namespace API.GraphQL
{
    public class Query : ObjectGraphType
    {
        public Query()
        {
            Field<UserQuery>("user", resolve: context => new { });
        }
    }
}
