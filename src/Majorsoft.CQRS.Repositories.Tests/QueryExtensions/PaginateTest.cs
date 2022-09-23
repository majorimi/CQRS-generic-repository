using System.Threading.Tasks;
using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.EntityFrameworkCore;
using Majorsoft.CQRS.Repositories.Specification;
using Majorsoft.CQRS.Repositories.QueryExtensions;
using Majorsoft.CQRS.Repositories.Tests.TestDb;

namespace Majorsoft.CQRS.Repositories.Tests.QueryExtensions
{
	[TestClass]
	public class PaginateTest
	{
		private TestDbContext _usieDbContext;

		[TestInitialize]
		public async Task Init()
		{
			var options = new DbContextOptionsBuilder<TestDbContext>()
			   .UseInMemoryDatabase($"UsieDatabase_{DateTime.Now.ToString()}")
			   .Options;

			_usieDbContext = new TestDbContext(options);

			for (int i = 0; i < 15; i++)
			{
				_usieDbContext.Links.Add(new Link() { LinkId = Guid.NewGuid(), CategoryId = i });
			}

			await _usieDbContext.SaveChangesAsync();
		}

		[TestCleanup]
		public async Task Cleanup()
		{
			await _usieDbContext.Database.EnsureDeletedAsync();
			await _usieDbContext.DisposeAsync();
		}

		[TestMethod]
		public async Task Paginate_extension_should_handle_null()
		{
			var response = await _usieDbContext.Links.ToPagedResponseAsync(null, default);
			var result = await response.Query.ToListAsync();

			Assert.IsNotNull(response);
			Assert.IsNotNull(result);
			Assert.AreEqual(10, result.Count);
			Assert.AreEqual(0, result[0].CategoryId);
			Assert.AreEqual(15, response.TotalCount);
		}

		[TestMethod]
		public async Task Paginate_extension_should_Count()
		{
			var response = await _usieDbContext.Links.ToPagedResponseAsync(new PagingOptions(1, 5), default);
			var result = await response.Query.ToListAsync();

			Assert.IsNotNull(response);
			Assert.IsNotNull(result);
			Assert.AreEqual(5, result.Count);
			Assert.AreEqual(5, result[0].CategoryId);
			Assert.AreEqual(15, response.TotalCount);
		}

		[TestMethod]
		public async Task Paginate_extension_should_handle_negative_pageNumber()
		{
			var response = await _usieDbContext.Links.ToPagedResponseAsync(new PagingOptions(-1, 5), default);
			var result = await response.Query.ToListAsync();

			Assert.IsNotNull(response);
			Assert.IsNotNull(result);
			Assert.AreEqual(5, result.Count);
			Assert.AreEqual(0, result[0].CategoryId);
			Assert.AreEqual(15, response.TotalCount);
		}

		[TestMethod]
		public async Task Paginate_extension_should_handle_over_paging()
		{
			var response = await _usieDbContext.Links.ToPagedResponseAsync(new PagingOptions(10, 5), default);
			var result = await response.Query.ToListAsync();

			Assert.IsNotNull(response);
			Assert.IsNotNull(result);
			Assert.AreEqual(0, result.Count);
			Assert.AreEqual(15, response.TotalCount);
		}
	}
}