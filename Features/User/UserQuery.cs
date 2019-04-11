using Features.User.Types;
using GraphQL.Types;
using System;

namespace Features.User
{
    public class UserQuery : ObjectGraphType
    {
        public UserQuery(IUserContext userContext)
        {
            FieldAsync<UserType>(
                name: "user",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IdGraphType>> { Name = "userId" }
                ),
                resolve: async context => await userContext.GetUser(context.GetArgument<Guid>("userId"))
            );

            Field<NonNullGraphType<ListGraphType<NonNullGraphType<UserStatusType>>>>(
                name: "statuses",
                resolve: _ => userContext.GetUserStatuses()
            );
        }
    }
}
