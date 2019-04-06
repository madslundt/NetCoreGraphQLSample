using Features.User;
using GraphQL.Types;

namespace API.GraphQL
{
    public class Query : ObjectGraphType
    {
        public Query()
        {
            Name = "Query";

            Field<UserQuery>("user", resolve: context => new { });
        }
    }
}
