using DataModel.Models;
using GraphQL.Types;

namespace API.Types
{
    public class UserStatusType : ObjectGraphType<UserStatusRef>
    {
        public UserStatusType()
        {
            Name = "UserStatusReference";

            Field(x => x.Id, type: typeof(IdGraphType)).Description($"Id of {Name}");
            Field(x => x.Name).Description($"Name of {Name}");
        }
    }
}
