using GraphQL;
using GraphQL.Types;

namespace Features
{
    public class Schemas : Schema
    {
        public Schemas(IDependencyResolver resolver) : base(resolver)
        {
            Query = resolver.Resolve<Queries>();
            Mutation = resolver.Resolve<Mutations>();
        }
    }
}
