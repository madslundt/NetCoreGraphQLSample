using GraphQL;
using GraphQL.Types;

namespace API.GraphQL
{
    public class Schemas : Schema
    {
        public Schemas(IDependencyResolver resolver) : base(resolver)
        {
            Query = resolver.Resolve<Query>();
            Mutation = resolver.Resolve<Mutation>();
        }
    }
}
