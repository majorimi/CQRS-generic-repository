using Majorsoft.CQRS.Repositories.Specification;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Linq;

namespace Majorsoft.CQRS.Repositories.Tests.Specification
{
	[TestClass]
	public class PagedDataTest
	{
		[TestMethod]
		public void PagedData_should_not_have_nulls()
		{
			var data = new PagedData<object>(null, 20);

			Assert.IsNotNull(data.Data);
			Assert.AreEqual(20, data.TotalItems);
		}

		[TestMethod]
		public void PagedData_should_set_values()
		{
			var data = new PagedData<object>(new object[] { new object(), new object() }, 5);

			Assert.IsNotNull(data.Data);
			Assert.AreEqual(2, data.Data.Count());
			Assert.AreEqual(5, data.TotalItems);
		}
	}
}