
using System.Linq;

namespace Majorsoft.CQRS.Repositories.QueryExtensions
{
	public class PagedResponse<T> where T : class
	{
		public IQueryable<T> Query { get; set; }

		public int TotalCount { get; set; }
	}
}