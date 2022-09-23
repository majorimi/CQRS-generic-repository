using Majorsoft.CQRS.Repositories.Specification;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Collections.Generic;
using System.Linq;

namespace Majorsoft.CQRS.Repositories.Tests.Specification
{
	[TestClass]
	public class QuerySpecificationEvaluatorTest
	{
		[TestMethod]
		public void QuerySpecificationEvaluator_should_return_same_query_when_no_spec()
		{
			var data = new List<object>() { 1, 2, 3, 4, 5 };
			var input = data.AsQueryable();

			var resQuery = QuerySpecificationEvaluator<object>.GetQuery(input, null);

			Assert.AreEqual(resQuery, input);
		}

		[TestMethod]
		public void QuerySpecificationEvaluator_should_return_query()
		{
			var data = new List<object>() { 1, 2, 3, 4, 5 };
			var input = data.AsQueryable();

			var resQuery = QuerySpecificationEvaluator<object>.GetQuery(input,
				new QuerySpecification<object>().AsNonTracking());

			Assert.AreEqual(resQuery.Expression, input.Expression);
		}
	}
}