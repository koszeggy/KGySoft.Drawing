#if NET35 || NET40 || NET45 || NET46
using System.Collections.Generic;

using KGySoft.Reflection;

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.ReturnValue | AttributeTargets.Class | AttributeTargets.Struct)]
    internal sealed class TupleElementNamesAttribute : Attribute
    {
        private readonly string[] transformNames;
        public TupleElementNamesAttribute(string[] transformNames) => this.transformNames = transformNames ?? throw new ArgumentNullException(nameof(transformNames));
        public TupleElementNamesAttribute() => transformNames = Reflector.EmptyArray<string>();
        public IList<string> TransformNames => transformNames;
    }
}
#endif