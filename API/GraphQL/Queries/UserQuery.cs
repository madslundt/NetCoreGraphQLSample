using API.Features.User;
using API.GraphQL.Types;
using GraphQL.Types;
using MediatR;
using System;

namespace API.GraphQL.Queries
{
    public class UserQuery : ObjectGraphType
    {
        public UserQuery(IMediator mediator)
        {
            FieldAsync<UserType>(
                name: "user",
                arguments: new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = nameof(GetUser.Query.Id) }
                ),
                resolve: async context => await mediator.Send(new GetUser.Query
                {
                    Id = context.GetArgument<Guid>(nameof(GetUser.Query.Id))
                })
            );
        }
    }
}
