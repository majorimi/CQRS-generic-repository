using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Majorsoft.CQRS.Repositories.Specification
{
	/// <summary>
	/// Default implementation of <see cref="IQuerySpecification{T}"/>
	/// </summary>
	/// <typeparam name="T">Entity Type</typeparam>
	public class QuerySpecification<T> : IQuerySpecification<T> where T : class
	{
		public QuerySpecification() { }

		public QuerySpecification(QueryOptions<T> options)
		{
			FilterCondition = options.FilterCondition;
			Includes = options.Includes;
			OrderOptions = options.OrderBy;
		}

		public QuerySpecification(Expression<Func<T, bool>> filterCondition)
		{
			FilterCondition = filterCondition;
		}

		public Expression<Func<T, bool>> FilterCondition { get; private set; }
		public IEnumerable<Expression<Func<T, bool>>> FilterConditions { get; init; } = new List<Expression<Func<T, bool>>>();
		public IEnumerable<OrderOption<T>> OrderOptions { get; init; } = new List<OrderOption<T>>();
		public IEnumerable<Expression<Func<T, object>>> Includes { get; init; } = new List<Expression<Func<T, object>>>();
		public IEnumerable<string> IncludeStrings { get; init; } = new List<string>();
		public bool IsNonTrackableQuery { get; private set; }

		public IQuerySpecification<T> ApplyFilter(Expression<Func<T, bool>> predicate)
		{
			FilterCondition = predicate;
			return this;
		}

		public IQuerySpecification<T> ApplyIncludes(params Expression<Func<T, object>>[] includes)
		{
			if (includes is not null)
			{
				(Includes as List<Expression<Func<T, object>>>).AddRange(includes);
			}

			return this;
		}

		public IQuerySpecification<T> ApplyIncludes(params string[] includeStrings)
		{
			if (includeStrings is not null)
			{
				(IncludeStrings as List<string>).AddRange(includeStrings);
			}

			return this;
		}

		public IQuerySpecification<T> ApplyOrderBy(params OrderOption<T>[] orderBy)
		{
			if (orderBy is not null)
			{
				(OrderOptions as List<OrderOption<T>>).AddRange(orderBy);
			}

			return this;
		}

		public IQuerySpecification<T> AsNonTracking()
		{
			IsNonTrackableQuery = true;
			return this;
		}

		public IQuerySpecification<T> ApplyFilters(params Expression<Func<T, bool>>[] predicate)
		{
			if (predicate is not null)
			{
				(FilterConditions as List<Expression<Func<T, bool>>>).AddRange(predicate);
			} 
			return this;
		}

		public override string ToString()
		{
			var sb = new StringBuilder($"{nameof(IQuerySpecification<T>)}<{typeof(T)}>");
			if (FilterCondition is not null)
			{
				sb.Append($"_FilterCondition:{FilterCondition}");
			}
			if (FilterConditions?.Any() ?? false)
			{
				sb.Append($"_FilterConditions:{string.Join(",", FilterConditions.Select(x => x.ToString()))}");
			}
			if (OrderOptions?.Any() ?? false)
			{
				sb.Append($"_OrderOptions:{string.Join(",", OrderOptions.Select(x => x.ToString()))}");
			}
			if (Includes?.Any() ?? false)
			{
				sb.Append($"_Includes:{string.Join(",", Includes.Select(x => x.ToString()))}");
			}
			if (IncludeStrings?.Any() ?? false)
			{
				sb.Append($"_IncludeStrings:{string.Join(",", IncludeStrings.Select(x => x.ToString()))}");
			}
			sb.Append($"_IsNonTrackableQuery:{IsNonTrackableQuery}");

			return sb.ToString();
		}
	}
}