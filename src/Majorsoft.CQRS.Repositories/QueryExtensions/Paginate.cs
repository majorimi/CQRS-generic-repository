using Majorsoft.CQRS.Repositories.Specification;

using Microsoft.EntityFrameworkCore;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Majorsoft.CQRS.Repositories.QueryExtensions
{
	public static class Paginate
	{
		private static IQueryable<T> Page<T>(this IQueryable<T> query, PagingOptions pagingOptions)
		{
			pagingOptions = pagingOptions ?? new PagingOptions();

			var skip = (pagingOptions.PageIndex < 0 ? 0 : pagingOptions.PageIndex )  * pagingOptions.PageSize;
			return query.Skip(skip).Take(pagingOptions.PageSize);
		}

		public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(this IQueryable<T> query, PagingOptions pagingOptions, CancellationToken cancellationToken)
			where T : class
		{
			return new PagedResponse<T>()
			{
				Query = query.Page(pagingOptions),
				TotalCount = await query.CountAsync(cancellationToken)
			};
		}
	}
}