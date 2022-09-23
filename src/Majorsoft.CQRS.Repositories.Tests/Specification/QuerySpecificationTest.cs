using Majorsoft.CQRS.Repositories.Specification;
using Majorsoft.CQRS.Repositories.Tests.TestDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Majorsoft.CQRS.Repositories.Tests.Specification
{
	[TestClass]
	public class QuerySpecificationTest
	{
		[TestMethod]
		public void QuerySpecification_should_have_default_values()
		{
			var querySpecifications = new QuerySpecification<Event>();

			Assert.AreEqual(null, querySpecifications.FilterCondition);
			Assert.AreEqual(null, querySpecifications.Includes);
			Assert.AreEqual(null, querySpecifications.IncludeStrings);
			Assert.AreEqual(false, querySpecifications.IsNonTrackableQuery);
			Assert.AreEqual(null, querySpecifications.OrderOptions);
		}

		[TestMethod]
		public void QuerySpecification_should_set_filter_from_constructor()
		{
			Expression<Func<Event, bool>> filterCondition = x => x.EventId == Guid.Empty;
			var querySpecifications = new QuerySpecification<Event>(filterCondition);

			Assert.AreEqual(filterCondition, querySpecifications.FilterCondition);
			Assert.AreEqual(null, querySpecifications.Includes);
			Assert.AreEqual(null, querySpecifications.IncludeStrings);
			Assert.AreEqual(false, querySpecifications.IsNonTrackableQuery);
			Assert.AreEqual(null, querySpecifications.OrderOptions);
		}

		[TestMethod]
		public void QuerySpecification_should_set_QueryOptions_from_constructor()
		{
			var expressions = new List<Expression<Func<Event, object>>>();
			expressions.Add(x => x.EventAccessControls);
			var orderExpressions = new OrderOption<Event>[]
				{
					new OrderOption<Event>(x => x.CreatedDate, false)
				};

			var options = new QueryOptions<Event>()
			{
				FilterCondition = x => x.EventId == Guid.Empty,
				Includes = expressions,
				OrderBy = orderExpressions.ToList(),
			};

			var querySpecifications = new QuerySpecification<Event>(options);

			Assert.AreEqual(options.FilterCondition, querySpecifications.FilterCondition);
			Assert.AreEqual(options.Includes, querySpecifications.Includes);
			Assert.AreEqual(null, querySpecifications.IncludeStrings);
			Assert.AreEqual(false, querySpecifications.IsNonTrackableQuery);
			Assert.AreEqual(options.OrderBy, querySpecifications.OrderOptions);
		}

		[TestMethod]
		public void QuerySpecification_should_have_ApplyFilter()
		{
			var querySpecifications = new QuerySpecification<Event>();
			querySpecifications.ApplyFilter(x => x.EventId == Guid.Empty);

			Assert.IsNotNull(querySpecifications.FilterCondition);
			Assert.AreEqual(null, querySpecifications.Includes);
			Assert.AreEqual(null, querySpecifications.IncludeStrings);
			Assert.AreEqual(false, querySpecifications.IsNonTrackableQuery);
			Assert.AreEqual(null, querySpecifications.OrderOptions);
		}

		[TestMethod]
		public void QuerySpecification_should_have_ApplyIncludes()
		{
			var querySpecifications = new QuerySpecification<Event>();
			querySpecifications.ApplyIncludes(x => x.Documents, x => x.Links);

			Assert.AreEqual(null, querySpecifications.FilterCondition);
			Assert.IsNotNull(querySpecifications.Includes);
			Assert.AreEqual(2, querySpecifications.Includes.Count());
			Assert.AreEqual(null, querySpecifications.IncludeStrings);
			Assert.AreEqual(false, querySpecifications.IsNonTrackableQuery);
			Assert.AreEqual(null, querySpecifications.OrderOptions);
		}

		[TestMethod]
		public void QuerySpecification_should_have_ApplyIncludes_with_string()
		{
			var querySpecifications = new QuerySpecification<Event>();
			querySpecifications
				.ApplyIncludes($"{nameof(Event.Documents)}", $"{nameof(Event.Links)}");

			Assert.AreEqual(null, querySpecifications.FilterCondition);
			Assert.AreEqual(null, querySpecifications.Includes);
			Assert.IsNotNull(querySpecifications.IncludeStrings);
			Assert.AreEqual(2, querySpecifications.IncludeStrings.Count());
			Assert.AreEqual(false, querySpecifications.IsNonTrackableQuery);
			Assert.AreEqual(null, querySpecifications.OrderOptions);
		}

		[TestMethod]
		public void QuerySpecification_should_have_ApplyOrderBy()
		{
			var querySpecifications = new QuerySpecification<Event>();
			querySpecifications.ApplyOrderBy(new OrderOption<Event>(x => x.EventId, false));

			Assert.AreEqual(null, querySpecifications.FilterCondition);
			Assert.IsNotNull(querySpecifications.OrderOptions);
			Assert.AreEqual(null, querySpecifications.Includes);
			Assert.AreEqual(null, querySpecifications.IncludeStrings);
			Assert.AreEqual(false, querySpecifications.IsNonTrackableQuery);
		}

		[TestMethod]
		public void QuerySpecification_should_have_set_AsNonTracking()
		{
			var querySpecifications = new QuerySpecification<Event>();
			querySpecifications.AsNonTracking();

			Assert.AreEqual(null, querySpecifications.FilterCondition);
			Assert.AreEqual(null, querySpecifications.Includes);
			Assert.AreEqual(null, querySpecifications.IncludeStrings);
			Assert.AreEqual(true, querySpecifications.IsNonTrackableQuery);
			Assert.AreEqual(null, querySpecifications.OrderOptions);
		}

		[TestMethod]
		public void QuerySpecification_should_have_ApplyFilters()
		{
			var querySpecifications = new QuerySpecification<Event>();
			Expression<Func<Event, bool>> filterByEventId = x => x.EventId == Guid.Empty;
			Expression<Func<Event, bool>> isPublic = x => x.PublicFlag.HasValue && x.PublicFlag.Value;
			querySpecifications.ApplyFilters(filterByEventId, isPublic);

			Assert.AreEqual(querySpecifications.FilterConditions.Count(), 2);
		}

		[TestMethod]
		public void QuerySpecification_should_have_ApplyFilters_multiple_times()
		{
			var querySpecifications = new QuerySpecification<Event>();
			Expression<Func<Event, bool>> filterByEventId = x => x.EventId == Guid.Empty;
			Expression<Func<Event, bool>> isPublic = x => x.PublicFlag.HasValue && x.PublicFlag.Value;
			querySpecifications.ApplyFilters(filterByEventId, isPublic);

			Expression<Func<Event, bool>> filterEventDate = x => x.EventDate == DateTime.Now;
			querySpecifications.ApplyFilters(filterEventDate);

			Assert.AreEqual(querySpecifications.FilterConditions.Count(), 3);
		}

		[TestMethod]
		public void QuerySpecification_ToString_should_return_custom_value()
		{
			var querySpecifications = new QuerySpecification<Event>();
			Expression<Func<Event, bool>> filterByEventId = x => x.EventId == Guid.Parse("{0057214B-8C2E-4341-BF3B-33091B18DEFA}");
			Expression<Func<Event, bool>> isPublic = x => x.PublicFlag.HasValue && x.PublicFlag.Value;
			querySpecifications.ApplyFilters(filterByEventId, isPublic);
			
			querySpecifications.ApplyIncludes(x => x.EventAccessControls);
			querySpecifications.ApplyIncludes("FakeInclude");

			querySpecifications.ApplyOrderBy(new OrderOption<Event>(x => x.EventDate, true), new OrderOption<Event>(x => x.EventId, false));

			Expression<Func<Event, bool>> filterEventDate = x => x.EventDate == DateTime.Now;
			querySpecifications.ApplyFilters(filterEventDate);

			var str = querySpecifications.ToString();

			Assert.AreEqual("IQuerySpecification<Majorsoft.CQRS.Repositories.Tests.TestDb.Event>_FilterConditions:x => (x.EventDate == Convert(DateTime.Now, Nullable`1)),x => (x.EventId == Parse(\"{0057214B-8C2E-4341-BF3B-33091B18DEFA}\")),x => (x.PublicFlag.HasValue AndAlso x.PublicFlag.Value)_OrderOptions:OrderOption_OrderBy:x => Convert(x.EventDate, Object)_Descending:True,OrderOption_OrderBy:x => Convert(x.EventId, Object)_Descending:False_Includes:x => x.EventAccessControls_IncludeStrings:FakeInclude_IsNonTrackableQuery:False", str);
		}
	}
}