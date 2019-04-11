using DataModel;
using DataModel.Models;
using Features.User.Types;
using GraphQL.Types;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Features.User
{
    public class UserQuery : ObjectGraphType
    {
        public UserQuery(DatabaseContext db)
        {
            FieldAsync<UserType>(
                name: "find",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "userId" }
                ),
                resolve: async context => {
                    var userId = context.GetArgument<Guid>("userId");
                    var result = await db.Users.FirstOrDefaultAsync(user => user.Id == userId);

                    return result;
                }
            );

            Field<NonNullGraphType<ListGraphType<NonNullGraphType<UserStatusType>>>>(
                name: "statuses",
                resolve: _ => Enum.GetValues(typeof(UserStatus)).Cast<UserStatus>().Select(userStatus => ((int)userStatus, userStatus.ToString()))
            );
        }
    }
}
