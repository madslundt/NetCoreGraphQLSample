using GraphQL.Types;

namespace Features.User.Types.Inputs
{
    public class CreateUserInputType : InputObjectGraphType<DataModel.Models.User>
    {
        public CreateUserInputType()
        {
            Name = "CreateUserInput";

            Field(x => x.FirstName, type: typeof(NonNullGraphType<StringGraphType>)).Description($"First name of {nameof(DataModel.Models.User)}");
            Field(x => x.LastName, type: typeof(NonNullGraphType<StringGraphType>)).Description($"Last name of {nameof(DataModel.Models.User)}");
        }
    }
}
