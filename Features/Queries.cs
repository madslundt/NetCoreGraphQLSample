using Features.User;
using Features.User.Types;
using GraphQL.Types;
using System;

namespace Features
{
    public class Queries : ObjectGraphType
    {
        public Queries(IUserContext userContext)
        {
            Name = "Query";

            FieldAsync<UserType>(
                name: "user",
                arguments: new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = "userId" }
                ),
                resolve: async context => await userContext.GetUser(context.GetArgument<Guid>("userId"))
            );
        }
    }
}
