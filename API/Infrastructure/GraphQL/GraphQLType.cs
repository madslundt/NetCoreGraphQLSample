using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Infrastructure.GraphQL
{
    public abstract class GraphQLType<T> : ObjectGraphType<T> 
    {
        public static string TypeName { get; private set; }

        public GraphQLType()
        {
            Name = typeof(T).FullName;
            TypeName = Name;
        }
    }
}
