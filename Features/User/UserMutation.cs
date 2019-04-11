using DataModel;
using Features.User.Types;
using GraphQL.Types;
using System;

namespace Features.User
{
    public class UserMutation : ObjectGraphType
    {
        public UserMutation(IUserContext userContext)
        {
            FieldAsync<NonNullGraphType<UserType>>(
                name: "user",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "firstName" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "lastName" }
                ),
                resolve: async context => await userContext.CreateUser(context.GetArgument<string>("firstName"), context.GetArgument<string>("lastName"))
            );
        }
    }
}
