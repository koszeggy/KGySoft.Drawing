using System.Collections;
using System.Collections.Generic;

namespace KGySoft.Drawing
{
    internal static class EnumerableExtensions
    {
        internal static bool TryGetCount<T>(this IEnumerable<T> collection, out int count)
        {
            switch (collection)
            {
                case ICollection<T> genericCollection:
                    count = genericCollection.Count;
                    return true;
                case IReadOnlyCollection<T> readOnlyCollection:
                    count = readOnlyCollection.Count;
                    return true;
                case ICollection nonGenericCollection:
                    count = nonGenericCollection.Count;
                    return true;
                default:
                    int? result = collection.GetListProviderCount();
                    count = result >= 0 ? result.Value : default;
                    return result >= 0;
            }
        }
    }
}
