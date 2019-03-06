using API.Infrastructure.GraphQL;
using DataModel.Models;
using GraphQL.Types;

namespace API.Types
{
    public class UserType : GraphQLType<User>
    {
        public UserType()
        {
            Field(x => x.Id, type: typeof(IdGraphType)).Description($"ID of {Name}");
            Field(x => x.FirstName).Description($"First name of {Name}");
            Field(x => x.LastName).Description($"Last name of {Name}");
            Field(x => x.Status, type: typeof(ListGraphType<UserStatusType>)).Description($"Status of {Name}");
        }
    }
}
