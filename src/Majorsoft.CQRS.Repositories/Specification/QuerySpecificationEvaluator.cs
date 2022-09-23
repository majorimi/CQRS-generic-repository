using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Majorsoft.CQRS.Repositories.Specification
{
	public class QuerySpecificationEvaluator<T> where T : class
	{
		public static IQueryable<T> GetQuery(IQueryable<T> query, IQuerySpecification<T> specifications)
		{
			if (specifications is null)
			{
				return query;
			}

			if (specifications.IsNonTrackableQuery)
			{
				query = query.AsNoTracking();
			}

			if (specifications.FilterCondition is not null)
			{
				query = query.Where(specifications.FilterCondition);
			}

			if (specifications.FilterConditions is not null)
			{
				specifications.FilterConditions.ToList().ForEach(q =>
				query = query.Where(q));
			}

			if (specifications.Includes is not null && specifications.Includes.Any())
			{
				query = specifications.Includes.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));
			}
			
			if (specifications.IncludeStrings is not null && specifications.IncludeStrings.Any())
			{
				query = specifications.IncludeStrings.Aggregate(query,
								(current, include) => current.Include(include));
			}

			if (specifications.OrderOptions is not null && specifications.OrderOptions.Any())
			{
				var first = specifications.OrderOptions.First();
				var orderedQuery = first.Descending ? query.OrderByDescending(first.OrderBy) : query.OrderBy(first.OrderBy);

				foreach (var nextOrder in specifications.OrderOptions.Skip(1))
				{
					orderedQuery = nextOrder.Descending ? orderedQuery.ThenByDescending(nextOrder.OrderBy) : orderedQuery.ThenBy(nextOrder.OrderBy);
				}

				query = orderedQuery;
			}

			return query;
		}
	}
}