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
			Assert.IsNotNull(querySpecifications.FilterConditions);
			Assert.AreEqual(0, querySpecifications.FilterConditions.Count());
			Assert.IsNotNull(querySpecifications.Includes);
			Assert.AreEqual(0, querySpecifications.Includes.Count());
			Assert.IsNotNull(querySpecifications.IncludeStrings);
			Assert.AreEqual(0, querySpecifications.IncludeStrings.Count());
			Assert.AreEqual(false, querySpecifications.IsNonTrackableQuery);
			Assert.IsNotNull(querySpecifications.OrderOptions);
			Assert.AreEqual(0, querySpecifications.OrderOptions.Count());
		}

		[TestMethod]
		public void QuerySpecification_should_set_filter_from_constructor()
		{
			Expression<Func<Event, bool>> filterCondition = x => x.EventId == Guid.Empty;
			var querySpecifications = new QuerySpecification<Event>(filterCondition);

			Assert.AreEqual(filterCondition, querySpecifications.FilterCondition);
			Assert.IsNotNull(querySpecifications.FilterConditions);
			Assert.AreEqual(0, querySpecifications.FilterConditions.Count());
			Assert.IsNotNull(querySpecifications.Includes);
			Assert.AreEqual(0, querySpecifications.Includes.Count());
			Assert.IsNotNull(querySpecifications.IncludeStrings);
			Assert.AreEqual(0, querySpecifications.IncludeStrings.Count()); 
			Assert.AreEqual(false, querySpecifications.IsNonTrackableQuery);
			Assert.IsNotNull(querySpecifications.OrderOptions);
			Assert.AreEqual(0, querySpecifications.OrderOptions.Count());
		}

		[TestMethod]
		public void QuerySpecification_should_set_QueryOptions_from_constructor()
		{
			var expressions = new List<Expression<Func<Event, object>>>
			{
				x => x.EventAccessControls
			};
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
			Assert.IsNotNull(querySpecifications.IncludeStrings);
			Assert.AreEqual(0, querySpecifications.IncludeStrings.Count()); 
			Assert.AreEqual(false, querySpecifications.IsNonTrackableQuery);
			Assert.AreEqual(options.OrderBy, querySpecifications.OrderOptions);
		}

		[TestMethod]
		public void QuerySpecification_should_have_ApplyFilter()
		{
			var querySpecifications = new QuerySpecification<Event>();
			querySpecifications.ApplyFilter(x => x.EventId == Guid.Empty);

			Assert.IsNotNull(querySpecifications.FilterCondition);
			Assert.IsNotNull(querySpecifications.FilterConditions);
			Assert.AreEqual(0, querySpecifications.FilterConditions.Count());
			Assert.IsNotNull(querySpecifications.Includes);
			Assert.AreEqual(0, querySpecifications.Includes.Count());
			Assert.IsNotNull(querySpecifications.IncludeStrings);
			Assert.AreEqual(0, querySpecifications.IncludeStrings.Count()); 
			Assert.AreEqual(false, querySpecifications.IsNonTrackableQuery);
			Assert.IsNotNull(querySpecifications.OrderOptions);
			Assert.AreEqual(0, querySpecifications.OrderOptions.Count());
		}

		[TestMethod]
		public void QuerySpecification_should_have_ApplyIncludes()
		{
			var querySpecifications = new QuerySpecification<Event>();
			querySpecifications.ApplyIncludes(x => x.Documents, x => x.Links);

			Assert.AreEqual(null, querySpecifications.FilterCondition);
			Assert.IsNotNull(querySpecifications.FilterConditions);
			Assert.AreEqual(0, querySpecifications.FilterConditions.Count());
			Assert.IsNotNull(querySpecifications.Includes);
			Assert.AreEqual(2, querySpecifications.Includes.Count());
			Assert.IsNotNull(querySpecifications.IncludeStrings);
			Assert.AreEqual(0, querySpecifications.IncludeStrings.Count()); 
			Assert.AreEqual(false, querySpecifications.IsNonTrackableQuery);
			Assert.IsNotNull(querySpecifications.OrderOptions);
			Assert.AreEqual(0, querySpecifications.OrderOptions.Count());
		}

		[TestMethod]
		public void QuerySpecification_should_have_ApplyIncludes_multiple_times()
		{
			var querySpecifications = new QuerySpecification<Event>();
			querySpecifications.ApplyIncludes(x => x.Documents, x => x.Links);
			querySpecifications.ApplyIncludes(x => x.EventAccessControls);

			Assert.AreEqual(null, querySpecifications.FilterCondition);
			Assert.IsNotNull(querySpecifications.FilterConditions);
			Assert.AreEqual(0, querySpecifications.FilterConditions.Count());
			Assert.IsNotNull(querySpecifications.Includes);
			Assert.AreEqual(3, querySpecifications.Includes.Count());
			Assert.IsNotNull(querySpecifications.IncludeStrings);
			Assert.AreEqual(0, querySpecifications.IncludeStrings.Count());
			Assert.AreEqual(false, querySpecifications.IsNonTrackableQuery);
			Assert.IsNotNull(querySpecifications.OrderOptions);
			Assert.AreEqual(0, querySpecifications.OrderOptions.Count());
		}

		[TestMethod]
		public void QuerySpecification_should_have_ApplyIncludes_with_string()
		{
			var querySpecifications = new QuerySpecification<Event>();
			querySpecifications
				.ApplyIncludes($"{nameof(Event.Documents)}", $"{nameof(Event.Links)}");

			Assert.AreEqual(null, querySpecifications.FilterCondition);
			Assert.IsNotNull(querySpecifications.FilterConditions);
			Assert.AreEqual(0, querySpecifications.FilterConditions.Count());
			Assert.IsNotNull(querySpecifications.Includes);
			Assert.AreEqual(0, querySpecifications.Includes.Count());
			Assert.IsNotNull(querySpecifications.IncludeStrings);
			Assert.AreEqual(2, querySpecifications.IncludeStrings.Count());
			Assert.AreEqual(false, querySpecifications.IsNonTrackableQuery);
			Assert.IsNotNull(querySpecifications.OrderOptions);
			Assert.AreEqual(0, querySpecifications.OrderOptions.Count());
		}

		[TestMethod]
		public void QuerySpecification_should_have_ApplyIncludes_with_string_multiple_times()
		{
			var querySpecifications = new QuerySpecification<Event>();

			querySpecifications.ApplyIncludes($"{nameof(Event.Documents)}", $"{nameof(Event.Links)}");
			querySpecifications.ApplyIncludes("NotExisting");

			Assert.AreEqual(null, querySpecifications.FilterCondition);
			Assert.IsNotNull(querySpecifications.FilterConditions);
			Assert.AreEqual(0, querySpecifications.FilterConditions.Count());
			Assert.IsNotNull(querySpecifications.Includes);
			Assert.AreEqual(0, querySpecifications.Includes.Count());
			Assert.IsNotNull(querySpecifications.IncludeStrings);
			Assert.AreEqual(3, querySpecifications.IncludeStrings.Count());
			Assert.AreEqual(false, querySpecifications.IsNonTrackableQuery);
			Assert.IsNotNull(querySpecifications.OrderOptions);
			Assert.AreEqual(0, querySpecifications.OrderOptions.Count());
		}

		[TestMethod]
		public void QuerySpecification_should_have_ApplyOrderBy()
		{
			var querySpecifications = new QuerySpecification<Event>();
			querySpecifications.ApplyOrderBy(new OrderOption<Event>(x => x.EventId, false));

			Assert.AreEqual(null, querySpecifications.FilterCondition);
			Assert.IsNotNull(querySpecifications.FilterConditions);
			Assert.AreEqual(0, querySpecifications.FilterConditions.Count());
			Assert.IsNotNull(querySpecifications.OrderOptions);
			Assert.IsNotNull(querySpecifications.Includes);
			Assert.AreEqual(0, querySpecifications.Includes.Count());
			Assert.IsNotNull(querySpecifications.IncludeStrings);
			Assert.AreEqual(0, querySpecifications.IncludeStrings.Count()); 
			Assert.AreEqual(false, querySpecifications.IsNonTrackableQuery);
		}

		[TestMethod]
		public void QuerySpecification_should_have_set_AsNonTracking()
		{
			var querySpecifications = new QuerySpecification<Event>();
			querySpecifications.AsNonTracking();

			Assert.AreEqual(null, querySpecifications.FilterCondition);
			Assert.IsNotNull(querySpecifications.FilterConditions);
			Assert.AreEqual(0, querySpecifications.FilterConditions.Count());
			Assert.IsNotNull(querySpecifications.Includes);
			Assert.AreEqual(0, querySpecifications.Includes.Count());
			Assert.IsNotNull(querySpecifications.IncludeStrings);
			Assert.AreEqual(0, querySpecifications.IncludeStrings.Count()); 
			Assert.AreEqual(true, querySpecifications.IsNonTrackableQuery);
			Assert.IsNotNull(querySpecifications.OrderOptions);
			Assert.AreEqual(0, querySpecifications.OrderOptions.Count());
		}

		[TestMethod]
		public void QuerySpecification_should_have_ApplyFilters()
		{
			var querySpecifications = new QuerySpecification<Event>();
			Expression<Func<Event, bool>> filterByEventId = x => x.EventId == Guid.Empty;
			Expression<Func<Event, bool>> isPublic = x => x.PublicFlag.HasValue && x.PublicFlag.Value;
			
			querySpecifications.ApplyFilters(filterByEventId, isPublic);

			Assert.AreEqual(null, querySpecifications.FilterCondition);
			Assert.IsNotNull(querySpecifications.FilterConditions);
			Assert.AreEqual(2, querySpecifications.FilterConditions.Count());
			Assert.IsNotNull(querySpecifications.Includes);
			Assert.AreEqual(0, querySpecifications.Includes.Count());
			Assert.IsNotNull(querySpecifications.IncludeStrings);
			Assert.AreEqual(0, querySpecifications.IncludeStrings.Count());
			Assert.AreEqual(false, querySpecifications.IsNonTrackableQuery);
			Assert.IsNotNull(querySpecifications.OrderOptions);
			Assert.AreEqual(0, querySpecifications.OrderOptions.Count());
		}

		[TestMethod]
		public void QuerySpecification_should_have_ApplyFilters_multiple_times()
		{
			var querySpecifications = new QuerySpecification<Event>();
			Expression<Func<Event, bool>> filterByEventId = x => x.EventId == Guid.Empty;
			Expression<Func<Event, bool>> isPublic = x => x.PublicFlag.HasValue && x.PublicFlag.Value;
			querySpecifications.ApplyFilters(filterByEventId, isPublic);

			querySpecifications.ApplyFilters(x => x.EventDate == DateTime.Now);

			Assert.AreEqual(null, querySpecifications.FilterCondition);
			Assert.IsNotNull(querySpecifications.FilterConditions);
			Assert.AreEqual(3, querySpecifications.FilterConditions.Count());
			Assert.IsNotNull(querySpecifications.Includes);
			Assert.AreEqual(0, querySpecifications.Includes.Count());
			Assert.IsNotNull(querySpecifications.IncludeStrings);
			Assert.AreEqual(0, querySpecifications.IncludeStrings.Count());
			Assert.AreEqual(false, querySpecifications.IsNonTrackableQuery);
			Assert.IsNotNull(querySpecifications.OrderOptions);
			Assert.AreEqual(0, querySpecifications.OrderOptions.Count());
		}

		[TestMethod]
		public void QuerySpecification_ToString_should_return_custom_value()
		{
			var querySpecifications = new QuerySpecification<Event>(x => x.CreatedBy == "user");
			Expression<Func<Event, bool>> filterByEventId = x => x.EventId == Guid.Parse("{0057214B-8C2E-4341-BF3B-33091B18DEFA}");
			Expression<Func<Event, bool>> isPublic = x => x.PublicFlag.HasValue && x.PublicFlag.Value;
			querySpecifications.ApplyFilters(filterByEventId, isPublic);
			
			querySpecifications.ApplyIncludes(x => x.EventAccessControls);
			querySpecifications.ApplyIncludes("FakeInclude");

			querySpecifications.ApplyOrderBy(new OrderOption<Event>(x => x.EventDate, true), new OrderOption<Event>(x => x.EventId, false));

			Expression<Func<Event, bool>> filterEventDate = x => x.EventDate == DateTime.Now;
			querySpecifications.ApplyFilters(filterEventDate);

			var str = querySpecifications.ToString();

			Assert.AreEqual("IQuerySpecification<Majorsoft.CQRS.Repositories.Tests.TestDb.Event>_FilterCondition:x => (x.CreatedBy == \"user\")_FilterConditions:x => (x.EventId == Parse(\"{0057214B-8C2E-4341-BF3B-33091B18DEFA}\")),x => (x.PublicFlag.HasValue AndAlso x.PublicFlag.Value),x => (x.EventDate == Convert(DateTime.Now, Nullable`1))_OrderOptions:OrderOption_OrderBy:x => Convert(x.EventDate, Object)_Descending:True,OrderOption_OrderBy:x => Convert(x.EventId, Object)_Descending:False_Includes:x => x.EventAccessControls_IncludeStrings:FakeInclude_IsNonTrackableQuery:False", str);
		}
	}
}