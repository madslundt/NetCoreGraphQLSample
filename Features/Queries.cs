using Features.User;
using Features.User.Types;
using GraphQL.Types;
using MediatR;
using System;

namespace Features
{
    public class Queries : ObjectGraphType
    {
        public Queries(IMediator mediator)
        {
            Name = "Query";

            FieldAsync<UserType>(
                name: "user",
                arguments: new QueryArguments(
                    new QueryArgument<IdGraphType> { Name = nameof(GetUser.Query.Id).ToLowerCamelCase() }
                ),
                resolve: async context => await mediator.Send(new GetUser.Query
                {
                    Id = context.GetArgument<Guid>(nameof(GetUser.Query.Id).ToLowerCamelCase())
                })
            );
        }
    }
}
