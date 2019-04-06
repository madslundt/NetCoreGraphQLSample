using DataModel;
using Features.User.Types;
using GraphQL.Types;
using System;

namespace Features.User
{
    public class UserMutation : ObjectGraphType
    {
        public UserMutation(DatabaseContext db)
        {
            Name = "User";

            Field<UserType>(
                name: "user",
                arguments: new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = "userId" }
                )
            );
        }
    }
}
