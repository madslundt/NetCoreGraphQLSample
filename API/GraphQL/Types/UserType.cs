using DataModel.Models;
using GraphQL.Types;

namespace API.GraphQL.Types
{
    public class UserType : ObjectGraphType<User>
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
