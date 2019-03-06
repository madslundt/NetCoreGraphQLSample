using API.Features.User;
using API.Types;
using DataModel.Models;
using GraphQL.Types;
using MediatR;

namespace API
{
    public class Mutations : ObjectGraphType
    {
        public Mutations(IMediator mediator)
        {
            Field<UserType, CreateUser.Result>()
                .Name("user")
                .Argument<NonNullGraphType<UserType>>("user", "User information")
                .ResolveAsync(context =>
                {
                    var user = context.GetArgument<User>("user");
                    return mediator.Send(new CreateUser.Command
                    {
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Password = user.PasswordHash.ToString()
                    });
                });
                
        }
    }
}
