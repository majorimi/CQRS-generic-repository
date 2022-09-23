using Majorsoft.CQRS.Repositories.Specification;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Majorsoft.CQRS.Repositories.Tests.Specification
{
	[TestClass]
	public class PagingOptionsTest
	{
		[TestMethod]
		public void PagingOptions_should_set_properties()
		{
			var pagingOptions = new PagingOptions() { PageIndex = 5, PageSize = 20 };

			Assert.AreEqual(5, pagingOptions.PageIndex);
			Assert.AreEqual(20, pagingOptions.PageSize);
		}

		[TestMethod]
		public void PagingOptions_should_set_values_from_contstructor()
		{
			var pagingOptions = new PagingOptions(5, 20);

			Assert.AreEqual(5, pagingOptions.PageIndex);
			Assert.AreEqual(20, pagingOptions.PageSize);
		}

		[TestMethod]
		public void PagingOptions_should_set_values_from_contstructor_Tuple()
		{
			var pagingOptions = new PagingOptions(new(5, 20));

			Assert.AreEqual(5, pagingOptions.PageIndex);
			Assert.AreEqual(20, pagingOptions.PageSize);
		}

		[TestMethod]
		public void PagingOptions_ToString_should_return_custom_value()
		{
			var pagingOptions = new PagingOptions(new(5, 20));

			var str = pagingOptions.ToString();

			Assert.AreEqual("PagingOptions_Index:5_Size:20", str);
		}
	}
}