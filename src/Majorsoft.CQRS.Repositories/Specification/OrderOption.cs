using System;
using System.Linq.Expressions;

namespace Majorsoft.CQRS.Repositories.Specification
{
	/// <summary>
	/// Order by option to specify entity property and direction.
	/// </summary>
	/// <typeparam name="T">Entity Type</typeparam>
	public record OrderOption<T>(Expression<Func<T, object>> OrderBy, bool Descending = false)
	{
		public override string ToString()
		{
			return $"{nameof(OrderOption<T>)}_{nameof(OrderBy)}:{OrderBy}_Descending:{Descending}";
		}
	}
}