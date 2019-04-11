using Features.User;
using GraphQL.Types;

namespace API.GraphQL
{
    public class RootMutation : ObjectGraphType
    {
        public RootMutation()
        {
            Name = "Mutation";

            Field<NonNullGraphType<UserMutation>>("user", resolve: context => new { });
        }
    }
}
