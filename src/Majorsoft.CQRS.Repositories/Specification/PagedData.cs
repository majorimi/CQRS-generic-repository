using System.Collections.Generic;

namespace Majorsoft.CQRS.Repositories.Specification
{
	public record PagedData<T> where T : class
	{
		public PagedData(IEnumerable<T> data, int totalItems)
		{
			Data = data ?? new List<T>();
			TotalItems = totalItems;
		}

		public IEnumerable<T> Data { get; init; }

		public int TotalItems { get; init; }
	}
}