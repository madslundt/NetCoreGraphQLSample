using GraphQL.Types;

namespace Features.User.Types
{
    public class UserType : ObjectGraphType<DataModel.Models.User>
    {
        public UserType()
        {
            Field(x => x.Id, type: typeof(NonNullGraphType<IdGraphType>)).Description($"ID of {nameof(DataModel.Models.User)}");
            Field(x => x.FirstName, type: typeof(NonNullGraphType<StringGraphType>)).Description($"First name of {nameof(DataModel.Models.User)}");
            Field(x => x.LastName, type: typeof(NonNullGraphType<StringGraphType>)).Description($"Last name of {nameof(DataModel.Models.User)}");
            Field(x => x.Status, type: typeof(NonNullGraphType<UserStatusType>)).Description($"Status of {nameof(DataModel.Models.User)}");
        }
    }
}
