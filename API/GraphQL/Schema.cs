using GQL = GraphQL;

namespace API.GraphQL
{
    public class Schema : GQL.Types.Schema
    {
        public Schema(GQL.IDependencyResolver resolver) : base(resolver)
        {
            Query = resolver.Resolve<Query>();
            Mutation = resolver.Resolve<Mutation>();
        }
    }
}
