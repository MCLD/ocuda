using System.Linq;
using Ocuda.Ops.Service.Filters;

namespace Ocuda.Ops.Data.Extensions
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> queryable,
            BaseFilter filter)
        {
            if (filter?.Skip.HasValue == true && filter?.Take.HasValue == true)
            {
                return queryable.Skip(filter.Skip.Value)
                    .Take(filter.Take.Value);
            }
            else
            {
                return queryable;
            }
        }
    }
}
