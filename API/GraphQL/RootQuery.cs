using Features.User;
using GraphQL.Types;

namespace API.GraphQL
{
    public class RootQuery : ObjectGraphType
    {
        public RootQuery()
        {
            Name = "Query";

            Field<NonNullGraphType<UserQuery>>("user", resolve: context => new { });
        }
    }
}
