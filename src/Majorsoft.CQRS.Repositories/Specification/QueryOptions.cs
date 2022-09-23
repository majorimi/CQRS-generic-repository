using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Majorsoft.CQRS.Repositories.Specification
{
	public class QueryOptions<T> where T : class
	{
		/// <summary>
		/// Filter condition for where
		/// </summary>
		public Expression<Func<T, bool>> FilterCondition { get; set; }

		/// <summary>
		/// Include Entities
		/// </summary>
		public List<Expression<Func<T, object>>> Includes { get; set; }

		/// <summary>
		/// Order result
		/// </summary>
		public List<OrderOption<T>> OrderBy { get; set; }
	}
}