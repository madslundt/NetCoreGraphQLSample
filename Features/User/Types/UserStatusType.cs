using DataModel.Models;
using GraphQL.Types;

namespace Features.User.Types
{
    public class UserStatusType : ObjectGraphType<(int Id, string Name)>
    {
        public UserStatusType()
        {
            Name = "UserStatusReference";

            Field(x => x.Id, type: typeof(NonNullGraphType<IdGraphType>)).Name("Id").Description($"Id of {Name}");
            Field(x => x.Name, type: typeof(NonNullGraphType<StringGraphType>)).Name("Name").Description($"Name of {Name}");
        }
    }
}
