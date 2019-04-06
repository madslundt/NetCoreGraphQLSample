using System.Security.Claims;

namespace API.Infrastructure.GraphQL
{
    public class GraphQLUserContext
    {
        public ClaimsPrincipal User { get; set; }
    }
}
