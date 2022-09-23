using Majorsoft.CQRS.Repositories;
using Majorsoft.CQRS.Repositories.Interfaces;
using Majorsoft.CQRS.Repositories.Specification;
using Majorsoft.CQRS.Repositories.Tests.TestDb;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Majorsoft.CQRS.Usie.Repositories.Tests
{
	[TestClass]
	public class InMemoryCachedQueryRepositoryTest
	{
		private Mock<IQueryRepository<TestDbContext, Link>> _mockLinkQueryRepository;
		private Mock<IMemoryCache> _mockMemoryCache;
		private Mock<IInMemoryCacheExpiryProvider> _mockInMemoryCacheExpiryProvider;

		private InMemoryCachedQueryRepository<TestDbContext, Link> _linkCachedQueryRepository;

		[TestInitialize]
		public void Init()
		{
			_mockMemoryCache = new Mock<IMemoryCache>();

			_mockLinkQueryRepository = new Mock<IQueryRepository<TestDbContext, Link>>();

			_mockInMemoryCacheExpiryProvider = new Mock<IInMemoryCacheExpiryProvider>();

			_linkCachedQueryRepository = new InMemoryCachedQueryRepository<TestDbContext, Link>(_mockMemoryCache.Object,
				_mockLinkQueryRepository.Object,
				_mockInMemoryCacheExpiryProvider.Object);
		}

		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		public async Task InMemoryCachedQueryRepository_constructor_should_check_null_IMemoryCache()
		{
			_linkCachedQueryRepository = new InMemoryCachedQueryRepository<TestDbContext, Link>(null,
				_mockLinkQueryRepository.Object,
				_mockInMemoryCacheExpiryProvider.Object);
		}

		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		public async Task InMemoryCachedQueryRepository_constructor_should_check_null_IQueryRepository()
		{
			_linkCachedQueryRepository = new InMemoryCachedQueryRepository<TestDbContext, Link>(_mockMemoryCache.Object,
				null,
				_mockInMemoryCacheExpiryProvider.Object);
		}

		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		public async Task InMemoryCachedQueryRepository_constructor_should_check_null_IInMemoryCacheExpiryProvider()
		{
			_linkCachedQueryRepository = new InMemoryCachedQueryRepository<TestDbContext, Link>(_mockMemoryCache.Object,
				_mockLinkQueryRepository.Object,
				null);
		}

		[TestMethod]
		public async Task InMemoryCachedQueryRepository_constructor_should_have_Default_Cache_time()
		{
			_mockInMemoryCacheExpiryProvider.SetupGet(g => g.CacheExpiryInSec).Returns(new Nullable<int>());

			_linkCachedQueryRepository = new InMemoryCachedQueryRepository<TestDbContext, Link>(_mockMemoryCache.Object,
				_mockLinkQueryRepository.Object,
				_mockInMemoryCacheExpiryProvider.Object);

			Assert.AreEqual(1200, _linkCachedQueryRepository.CacheExpiryInSec);
		}

		[TestMethod]
		public async Task InMemoryCachedQueryRepository_constructor_should_use_Cache_time_provider()
		{
			_mockInMemoryCacheExpiryProvider.SetupGet(g => g.CacheExpiryInSec).Returns(new Nullable<int>(7777));

			_linkCachedQueryRepository = new InMemoryCachedQueryRepository<TestDbContext, Link>(_mockMemoryCache.Object,
				_mockLinkQueryRepository.Object,
				_mockInMemoryCacheExpiryProvider.Object);

			Assert.AreEqual(7777, _linkCachedQueryRepository.CacheExpiryInSec);
		}

		[TestMethod]
		public async Task InMemoryCachedQueryRepository_should_read_config()
		{
			_mockInMemoryCacheExpiryProvider.SetupGet(g => g.CacheExpiryInSec).Returns(9999);

			_linkCachedQueryRepository = new InMemoryCachedQueryRepository<TestDbContext, Link>(_mockMemoryCache.Object,
				_mockLinkQueryRepository.Object,
				_mockInMemoryCacheExpiryProvider.Object);

			Assert.AreEqual(9999, _linkCachedQueryRepository.CacheExpiryInSec);
		}

		[TestMethod]
		public async Task InMemoryCachedQueryRepository_GetListAsync_TEntity_should_handle_cache_miss()
		{
			object cache = null;
			_mockMemoryCache.Setup(s => s.TryGetValue(It.IsAny<object>(), out cache)).Returns(false);

			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyFilters(x => x.CategoryId > 3,
									  x => x.Organization == "org 1",
									  x => x.Message != null);

			IEnumerable<Link> data = new List<Link>() { new Link(), new Link() };
			_mockLinkQueryRepository.Setup(s => s.GetListAsync(spec, default))
				.ReturnsAsync(data);

			var cacheEntryMock = new Mock<ICacheEntry>();
			_mockMemoryCache.Setup(s => s.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

			var res = await _linkCachedQueryRepository.GetListAsync(spec, default);

			Assert.AreEqual(2, res.Count());

			_mockMemoryCache.Verify(v => v.TryGetValue(It.IsAny<object>(), out cache), Times.Once);
			_mockLinkQueryRepository.Verify(v => v.GetListAsync(spec, default), Times.Once);
			_mockMemoryCache.Verify(v => v.CreateEntry(It.IsAny<object>()), Times.Once);
		}

		[TestMethod]
		public async Task InMemoryCachedQueryRepository_GetListAsync_TEntity_should_return_from_cache()
		{
			object cache = new List<Link>() { new Link(), new Link() };
			_mockMemoryCache.Setup(s => s.TryGetValue(It.IsAny<object>(), out cache)).Returns(true);

			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyFilters(x => x.CategoryId > 3,
									  x => x.Organization == "org 1",
									  x => x.Message != null);

			var res = await _linkCachedQueryRepository.GetListAsync(spec, default);

			Assert.AreEqual(2, res.Count());

			_mockMemoryCache.Verify(v => v.TryGetValue(It.IsAny<object>(), out cache), Times.Once);
			_mockLinkQueryRepository.Verify(v => v.GetListAsync(spec, default), Times.Never);
			_mockMemoryCache.Verify(v => v.CreateEntry(It.IsAny<object>()), Times.Never);
		}

		[TestMethod]
		public async Task InMemoryCachedQueryRepository_GetListAsync_TResult_should_handle_cache_miss()
		{
			object cache = null;
			_mockMemoryCache.Setup(s => s.TryGetValue(It.IsAny<object>(), out cache)).Returns(false);

			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyFilters(x => x.CategoryId > 3,
									  x => x.Organization == "org 1",
									  x => x.Message != null);

			IEnumerable<TestLink> data = new List<TestLink>() { new TestLink(), new TestLink() };
			_mockLinkQueryRepository.Setup(s => s.GetListAsync<TestLink>(spec, default))
				.ReturnsAsync(data);

			var cacheEntryMock = new Mock<ICacheEntry>();
			_mockMemoryCache.Setup(s => s.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

			var res = await _linkCachedQueryRepository.GetListAsync<TestLink>(spec, default);

			Assert.AreEqual(2, res.Count());

			_mockMemoryCache.Verify(v => v.TryGetValue(It.IsAny<object>(), out cache), Times.Once);
			_mockLinkQueryRepository.Verify(v => v.GetListAsync<TestLink>(spec, default), Times.Once);
			_mockMemoryCache.Verify(v => v.CreateEntry(It.IsAny<object>()), Times.Once);
		}

		[TestMethod]
		public async Task InMemoryCachedQueryRepository_GetListAsync_TResult_should_return_from_cache()
		{
			object cache = new List<TestLink>() { new TestLink(), new TestLink() };
			_mockMemoryCache.Setup(s => s.TryGetValue(It.IsAny<object>(), out cache)).Returns(true);

			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyFilters(x => x.CategoryId > 3,
									  x => x.Organization == "org 1",
									  x => x.Message != null);

			var res = await _linkCachedQueryRepository.GetListAsync<TestLink>(spec, default);

			Assert.AreEqual(2, res.Count());

			_mockMemoryCache.Verify(v => v.TryGetValue(It.IsAny<object>(), out cache), Times.Once);
			_mockLinkQueryRepository.Verify(v => v.GetListAsync<TestLink>(spec, default), Times.Never);
			_mockMemoryCache.Verify(v => v.CreateEntry(It.IsAny<object>()), Times.Never);
		}

		[TestMethod]
		public async Task InMemoryCachedQueryRepository_GetListAsync_TEntity_paged_should_handle_cache_miss()
		{
			var pagingOpt = new PagingOptions(0, 5);
			object cache = null;
			_mockMemoryCache.Setup(s => s.TryGetValue(It.IsAny<object>(), out cache)).Returns(false);

			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyFilters(x => x.CategoryId > 3,
									  x => x.Organization == "org 1",
									  x => x.Message != null);

			IEnumerable<Link> data = new List<Link>() { new Link(), new Link() };
			_mockLinkQueryRepository.Setup(s => s.GetListAsync(spec, pagingOpt, default))
				.ReturnsAsync(new PagedData<Link>(data, 20));

			var cacheEntryMock = new Mock<ICacheEntry>();
			_mockMemoryCache.Setup(s => s.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

			var res = await _linkCachedQueryRepository.GetListAsync(spec, pagingOpt, default);

			Assert.AreEqual(2, res.Data.Count());

			_mockMemoryCache.Verify(v => v.TryGetValue(It.IsAny<object>(), out cache), Times.Once);
			_mockLinkQueryRepository.Verify(v => v.GetListAsync(spec, pagingOpt, default), Times.Once);
			_mockMemoryCache.Verify(v => v.CreateEntry(It.IsAny<object>()), Times.Once);
		}

		[TestMethod]
		public async Task InMemoryCachedQueryRepository_GetListAsync_TEntity_paged_should_return_from_cache()
		{
			var pagingOpt = new PagingOptions(0, 5);
			object cache = new PagedData<Link>(new List<Link>() { new Link(), new Link() }, 20);
			_mockMemoryCache.Setup(s => s.TryGetValue(It.IsAny<object>(), out cache)).Returns(true);

			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyFilters(x => x.CategoryId > 3,
									  x => x.Organization == "org 1",
									  x => x.Message != null);

			var res = await _linkCachedQueryRepository.GetListAsync(spec, pagingOpt, default);

			Assert.AreEqual(2, res.Data.Count());

			_mockMemoryCache.Verify(v => v.TryGetValue(It.IsAny<object>(), out cache), Times.Once);
			_mockLinkQueryRepository.Verify(v => v.GetListAsync(spec, pagingOpt, default), Times.Never);
			_mockMemoryCache.Verify(v => v.CreateEntry(It.IsAny<object>()), Times.Never);
		}

		[TestMethod]
		public async Task InMemoryCachedQueryRepository_GetListAsync_TResult_paged_should_handle_cache_miss()
		{
			var pagingOpt = new PagingOptions(0, 5);
			object cache = null;
			_mockMemoryCache.Setup(s => s.TryGetValue(It.IsAny<object>(), out cache)).Returns(false);

			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyFilters(x => x.CategoryId > 3,
									  x => x.Organization == "org 1",
									  x => x.Message != null);

			IEnumerable<TestLink> data = new List<TestLink>() { new TestLink(), new TestLink() };
			_mockLinkQueryRepository.Setup(s => s.GetListAsync<TestLink>(spec, pagingOpt, default))
				.ReturnsAsync(new PagedData<TestLink>(data, 20));

			var cacheEntryMock = new Mock<ICacheEntry>();
			_mockMemoryCache.Setup(s => s.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

			var res = await _linkCachedQueryRepository.GetListAsync<TestLink>(spec, pagingOpt, default);

			Assert.AreEqual(2, res.Data.Count());

			_mockMemoryCache.Verify(v => v.TryGetValue(It.IsAny<object>(), out cache), Times.Once);
			_mockLinkQueryRepository.Verify(v => v.GetListAsync<TestLink>(spec, pagingOpt, default), Times.Once);
			_mockMemoryCache.Verify(v => v.CreateEntry(It.IsAny<object>()), Times.Once);
		}

		[TestMethod]
		public async Task InMemoryCachedQueryRepository_GetListAsync_TResult_paged_should_return_from_cache()
		{
			var pagingOpt = new PagingOptions(0, 5);
			object cache = new PagedData<TestLink>(new List<TestLink>() { new TestLink(), new TestLink() }, 20);
			_mockMemoryCache.Setup(s => s.TryGetValue(It.IsAny<object>(), out cache)).Returns(true);

			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyFilters(x => x.CategoryId > 3,
									  x => x.Organization == "org 1",
									  x => x.Message != null);

			var res = await _linkCachedQueryRepository.GetListAsync<TestLink>(spec, pagingOpt, default);

			Assert.AreEqual(2, res.Data.Count());

			_mockMemoryCache.Verify(v => v.TryGetValue(It.IsAny<object>(), out cache), Times.Once);
			_mockLinkQueryRepository.Verify(v => v.GetListAsync<TestLink>(spec, pagingOpt, default), Times.Never);
			_mockMemoryCache.Verify(v => v.CreateEntry(It.IsAny<object>()), Times.Never);
		}

		[TestMethod]
		public async Task InMemoryCachedQueryRepository_GetListAsync_TSelect_should_handle_cache_miss()
		{
			object cache = null;
			_mockMemoryCache.Setup(s => s.TryGetValue(It.IsAny<object>(), out cache)).Returns(false);

			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyFilters(x => x.CategoryId > 3,
									  x => x.Organization == "org 1",
									  x => x.Message != null);

			IEnumerable<Link> data = new List<Link>() { new Link(), new Link() };
			_mockLinkQueryRepository.Setup(s => s.GetListAsync(spec, x=> x.Description, default))
				.ReturnsAsync(new string[] { "desc 1", "desc 2" });

			var cacheEntryMock = new Mock<ICacheEntry>();
			_mockMemoryCache.Setup(s => s.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

			var res = await _linkCachedQueryRepository.GetListAsync(spec, x => x.Description, default);

			Assert.AreEqual(2, res.Count());

			_mockMemoryCache.Verify(v => v.TryGetValue(It.IsAny<object>(), out cache), Times.Once);
			_mockLinkQueryRepository.Verify(v => v.GetListAsync(spec, x => x.Description, default), Times.Once);
			_mockMemoryCache.Verify(v => v.CreateEntry(It.IsAny<object>()), Times.Once);
		}

		[TestMethod]
		public async Task InMemoryCachedQueryRepository_GetListAsync_TSelect_should_return_from_cache()
		{
			object cache = new List<string>() { "desc 1", "desc 2" };
			_mockMemoryCache.Setup(s => s.TryGetValue(It.IsAny<object>(), out cache)).Returns(true);

			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyFilters(x => x.CategoryId > 3,
									  x => x.Organization == "org 1",
									  x => x.Message != null);

			var res = await _linkCachedQueryRepository.GetListAsync(spec, x => x.Description, default);

			Assert.AreEqual(2, res.Count());

			_mockMemoryCache.Verify(v => v.TryGetValue(It.IsAny<object>(), out cache), Times.Once);
			_mockLinkQueryRepository.Verify(v => v.GetListAsync(spec, x => x.Description, default), Times.Never);
			_mockMemoryCache.Verify(v => v.CreateEntry(It.IsAny<object>()), Times.Never);
		}

		[TestMethod]
		public async Task InMemoryCachedQueryRepository_FindAsync_TEntity_should_handle_cache_miss()
		{
			object cache = null;
			_mockMemoryCache.Setup(s => s.TryGetValue(It.IsAny<object>(), out cache)).Returns(false);

			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyFilters(x => x.CategoryId > 3,
									  x => x.Organization == "org 1",
									  x => x.Message != null);

			Link data = new Link();
			_mockLinkQueryRepository.Setup(s => s.FindAsync(spec, default)).ReturnsAsync(data);

			var cacheEntryMock = new Mock<ICacheEntry>();
			_mockMemoryCache.Setup(s => s.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

			var res = await _linkCachedQueryRepository.FindAsync(spec, default);

			Assert.IsNotNull(res);

			_mockMemoryCache.Verify(v => v.TryGetValue(It.IsAny<object>(), out cache), Times.Once);
			_mockLinkQueryRepository.Verify(v => v.FindAsync(spec, default), Times.Once);
			_mockMemoryCache.Verify(v => v.CreateEntry(It.IsAny<object>()), Times.Once);
		}

		[TestMethod]
		public async Task InMemoryCachedQueryRepository_FindAsync_TEntity_should_return_from_cache()
		{
			object cache = new Link();
			_mockMemoryCache.Setup(s => s.TryGetValue(It.IsAny<object>(), out cache)).Returns(true);

			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyFilters(x => x.CategoryId > 3,
									  x => x.Organization == "org 1",
									  x => x.Message != null);

			var res = await _linkCachedQueryRepository.FindAsync(spec, default);

			Assert.IsNotNull(res);

			_mockMemoryCache.Verify(v => v.TryGetValue(It.IsAny<object>(), out cache), Times.Once);
			_mockLinkQueryRepository.Verify(v => v.FindAsync(spec, default), Times.Never);
			_mockMemoryCache.Verify(v => v.CreateEntry(It.IsAny<object>()), Times.Never);
		}

		[TestMethod]
		public async Task InMemoryCachedQueryRepository_FindAsync_TResult_should_handle_cache_miss()
		{
			object cache = null;
			_mockMemoryCache.Setup(s => s.TryGetValue(It.IsAny<object>(), out cache)).Returns(false);

			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyFilters(x => x.CategoryId > 3,
									  x => x.Organization == "org 1",
									  x => x.Message != null);

			Link data = new Link();
			_mockLinkQueryRepository.Setup(s => s.FindAsync<Link>(spec, default)).ReturnsAsync(data);

			var cacheEntryMock = new Mock<ICacheEntry>();
			_mockMemoryCache.Setup(s => s.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

			var res = await _linkCachedQueryRepository.FindAsync<Link>(spec, default);

			Assert.IsNotNull(res);

			_mockMemoryCache.Verify(v => v.TryGetValue(It.IsAny<object>(), out cache), Times.Once);
			_mockLinkQueryRepository.Verify(v => v.FindAsync<Link>(spec, default), Times.Once);
			_mockMemoryCache.Verify(v => v.CreateEntry(It.IsAny<object>()), Times.Once);
		}

		[TestMethod]
		public async Task InMemoryCachedQueryRepository_FindAsync_TResult_should_return_from_cache()
		{
			object cache = new TestLink();
			_mockMemoryCache.Setup(s => s.TryGetValue(It.IsAny<object>(), out cache)).Returns(true);

			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyFilters(x => x.CategoryId > 3,
									  x => x.Organization == "org 1",
									  x => x.Message != null);

			var res = await _linkCachedQueryRepository.FindAsync<TestLink>(spec, default);

			Assert.IsNotNull(res);

			_mockMemoryCache.Verify(v => v.TryGetValue(It.IsAny<object>(), out cache), Times.Once);
			_mockLinkQueryRepository.Verify(v => v.FindAsync<TestLink>(spec, default), Times.Never);
			_mockMemoryCache.Verify(v => v.CreateEntry(It.IsAny<object>()), Times.Never);
		}

		[TestMethod]
		public async Task InMemoryCachedQueryRepository_FindAsync_TSelect_should_handle_cache_miss()
		{
			object cache = null;
			_mockMemoryCache.Setup(s => s.TryGetValue(It.IsAny<object>(), out cache)).Returns(false);

			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyFilters(x => x.CategoryId > 3,
									  x => x.Organization == "org 1",
									  x => x.Message != null);

			Link data = new Link();
			_mockLinkQueryRepository.Setup(s => s.FindAsync<string>(spec, x=> x.Description, default)).ReturnsAsync("desc");

			var cacheEntryMock = new Mock<ICacheEntry>();
			_mockMemoryCache.Setup(s => s.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

			var res = await _linkCachedQueryRepository.FindAsync<string>(spec, x => x.Description, default);

			Assert.AreEqual("desc", res);

			_mockMemoryCache.Verify(v => v.TryGetValue(It.IsAny<object>(), out cache), Times.Once);
			_mockLinkQueryRepository.Verify(v => v.FindAsync<string>(spec, x => x.Description, default), Times.Once);
			_mockMemoryCache.Verify(v => v.CreateEntry(It.IsAny<object>()), Times.Once);
		}

		[TestMethod]
		public async Task InMemoryCachedQueryRepository_FindAsync_TSelect_should_return_from_cache()
		{
			object cache = "desc";
			_mockMemoryCache.Setup(s => s.TryGetValue(It.IsAny<object>(), out cache)).Returns(true);

			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyFilters(x => x.CategoryId > 3,
									  x => x.Organization == "org 1",
									  x => x.Message != null);

			var res = await _linkCachedQueryRepository.FindAsync<string>(spec, x => x.Description, default);

			Assert.AreEqual("desc", res);

			_mockMemoryCache.Verify(v => v.TryGetValue(It.IsAny<object>(), out cache), Times.Once);
			_mockLinkQueryRepository.Verify(v => v.FindAsync<string>(spec, x => x.Description, default), Times.Never);
			_mockMemoryCache.Verify(v => v.CreateEntry(It.IsAny<object>()), Times.Never);
		}


		[TestMethod]
		public void InMemoryCachedQueryRepository_GetList_should_handle_cache_miss()
		{
			var query = "select";
			var sqlParams = new SqlParameter[] { };
			object cache = null;
			_mockMemoryCache.Setup(s => s.TryGetValue(It.IsAny<object>(), out cache)).Returns(false);

			var data = new DataTable();
			_mockLinkQueryRepository.Setup(s => s.GetList(query, sqlParams)).Returns(data);

			var cacheEntryMock = new Mock<ICacheEntry>();
			_mockMemoryCache.Setup(s => s.CreateEntry(It.IsAny<object>())).Returns(cacheEntryMock.Object);

			var res = _linkCachedQueryRepository.GetList(query, sqlParams);

			Assert.IsNotNull(res);

			_mockMemoryCache.Verify(v => v.TryGetValue(It.IsAny<object>(), out cache), Times.Once);
			_mockLinkQueryRepository.Verify(v => v.GetList(query, sqlParams), Times.Once);
			_mockMemoryCache.Verify(v => v.CreateEntry(It.IsAny<object>()), Times.Once);
		}

		[TestMethod]
		public void InMemoryCachedQueryRepository_GetList_should_return_from_cache()
		{
			var query = "select";
			object cache = new DataTable();
			var sqlParams = new SqlParameter[] { };
			_mockMemoryCache.Setup(s => s.TryGetValue(It.IsAny<object>(), out cache)).Returns(true);

			var res = _linkCachedQueryRepository.GetList(query, sqlParams);

			Assert.IsNotNull(res);

			_mockMemoryCache.Verify(v => v.TryGetValue(It.IsAny<object>(), out cache), Times.Once);
			_mockLinkQueryRepository.Verify(v => v.GetList(query, sqlParams), Times.Never);
			_mockMemoryCache.Verify(v => v.CreateEntry(It.IsAny<object>()), Times.Never);
		}
	}
}