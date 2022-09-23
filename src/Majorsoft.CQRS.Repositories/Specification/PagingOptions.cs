namespace Majorsoft.CQRS.Repositories.Specification
{
	/// <summary>
	/// Paging option data.
	/// </summary>
	public record PagingOptions
	{
		public int PageIndex { get; init; } = 0;

		public int PageSize { get; init; } = 10;

		public PagingOptions()
		{ }

		public PagingOptions(int pageIndex, int pageSize)
		{
			PageIndex = pageIndex;
			PageSize = pageSize;
		}

		public PagingOptions((int pageIndex, int pageSize) options)
		{
			PageIndex = options.pageIndex;
			PageSize = options.pageSize;
		}

		public override string ToString()
		{
			return $"{nameof(PagingOptions)}_Index:{PageIndex}_Size:{PageSize}";
		}
	}
}