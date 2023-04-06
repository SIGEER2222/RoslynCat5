
using System.Collections.Immutable;

namespace OneDas.DataManagement.Explorer.Omnisharp
{
    internal static class ImmutableArrayExtensions
    {
        public static ImmutableArray<TOut> SelectAsArray<TIn, TOut>(this ImmutableArray<TIn> array, Func<TIn, TOut> mapper)
        {
            if (array.IsDefaultOrEmpty)
            {
                return ImmutableArray<TOut>.Empty;
            }

            var builder = ImmutableArray.CreateBuilder<TOut>(array.Length);
            foreach (var e in array)
            {
                builder.Add(mapper(e));
            }

            return builder.MoveToImmutable();
        }

        public static ImmutableArray<T> ToImmutableAndClear<T>(this ImmutableArray<T>.Builder builder)
        {
            if (builder.Capacity == builder.Count)
            {
                return builder.MoveToImmutable();
            }
            else
            {
                var result = builder.ToImmutable();
                builder.Clear();
                return result;
            }
        }
    }
}
