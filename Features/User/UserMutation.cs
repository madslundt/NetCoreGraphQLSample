using Features.User.Types;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Features.User
{
    public class UserMutation : ObjectGraphType
    {
        public UserMutation(IUserContext userContext)
        {
            Name = "User";

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
