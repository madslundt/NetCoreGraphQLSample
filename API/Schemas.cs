using GraphQL;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API
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
