using Features.User;
using GraphQL.Types;

namespace API.GraphQL
{
    public class Mutation : ObjectGraphType
    {
        public Mutation()
        {
            Name = "Mutation";

            Field<NonNullGraphType<UserMutation>>("user", resolve: context => new { });
        }
    }
}
