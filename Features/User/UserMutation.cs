using DataModel;
using Features.User.Types;
using Features.User.Types.Inputs;
using GraphQL.Types;

namespace Features.User
{
    public class UserMutation : ObjectGraphType
    {
        public UserMutation(DatabaseContext db)
        {
            FieldAsync<NonNullGraphType<UserType>>(
                name: "create",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<CreateUserInputType>> { Name = "input" }
                ),
                resolve: async context => {
                    var input = context.GetArgument<DataModel.Models.User>("input");

                    var user = new DataModel.Models.User
                    {
                        FirstName = input.FirstName,
                        LastName = input.LastName
                    };

                    db.Users.Add(user);

                    await db.SaveChangesAsync();

                    return user;
                }
            );
        }
    }
}
