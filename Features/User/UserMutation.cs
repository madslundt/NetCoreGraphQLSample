using DataModel;
using Features.User.Types;
using Features.User.Types.Inputs;
using GraphQL.Types;
using System;

namespace Features.User
{
    public class UserMutation : ObjectGraphType
    {
        public UserMutation(IUserContext userContext)
        {
            FieldAsync<NonNullGraphType<UserType>>(
                name: "create",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<CreateUserInputType>> { Name = "input" }
                ),
                resolve: async context => {
                    var user = context.GetArgument<DataModel.Models.User>("input");

                    return await userContext.CreateUser((firstName: user.FirstName, lastName: user.LastName));
                }
            );
        }
    }
}
