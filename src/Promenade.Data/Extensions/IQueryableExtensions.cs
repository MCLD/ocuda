using System.Linq;
using Ocuda.Promenade.Service.Filters;

namespace Ocuda.Promenade.Data.Extensions
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> queryable, BaseFilter filter)
        {
            if (filter.Skip != null && filter.Take != null)
            {
                return queryable.Skip((int)filter.Skip).Take((int)filter.Take);
            }
            else
            {
                return queryable;
            }
        }
    }
}
