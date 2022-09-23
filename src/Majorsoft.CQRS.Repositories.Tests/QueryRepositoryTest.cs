using AutoMapper;
using Majorsoft.CQRS.Repositories;
using Majorsoft.CQRS.Repositories.Specification;
using Majorsoft.CQRS.Repositories.Tests.TestDb;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Majorsoft.CQRS.Usie.Repositories.Tests
{
	internal class TestLink
	{
		public Guid Id { get; set; }
		public string Organization { get; set; }
		public int CategoryId { get; set; }

		public TestMessage Message { get; set; }
	}
	internal class TestMessage
	{
	}

	[TestClass]
	public class QueryRepositoryTest
	{
		private TestDbContext _testDbContext;
		private QueryRepository<TestDbContext, Link> _linkQueryRepository;
		private Mock<IMapper> _mapperMock;

		[TestInitialize]
		public async Task Init()
		{
			var options = new DbContextOptionsBuilder<TestDbContext>()
			   .UseInMemoryDatabase($"UsieDatabase_{DateTime.Now.ToString()}")
			   .Options;

			_testDbContext = new TestDbContext(options);
			await _testDbContext.Database.EnsureCreatedAsync();

			for (int i = 0; i < 15; i++)
			{
				short bookNumber = (short)(15 - i);
				var msgid = Guid.NewGuid();
				_testDbContext.Links.Add(new Link() { LinkId = Guid.NewGuid(), CategoryId = i+1, MessageId = msgid , Organization =i < 5 ? "org 1" : "org 2", Title= i.ToString()});
				_testDbContext.Categories.Add(new Category() { CategoryId = i+1, CategoryName = $"Category " + i.ToString() });
				_testDbContext.Messages.Add(new Message() { MessageId = msgid, BookNo = bookNumber });
				_testDbContext.InformationCategories.Add(new InformationCategory() { CategoryId =  i+1, CategoryText = ""});
			}

			await _testDbContext.SaveChangesAsync();
			_testDbContext.ChangeTracker.Clear();

			var config = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<Link, TestLink>()
					.ForMember(m => m.Id, o => o.MapFrom(m => m.LinkId));

				cfg.CreateMap<Message, TestMessage>();
			});

			_mapperMock = new Mock<IMapper>();
			_mapperMock.SetupGet(g => g.ConfigurationProvider).Returns(config);

			_linkQueryRepository = new QueryRepository<TestDbContext, Link>(_testDbContext, _mapperMock.Object);
		}

		[TestCleanup]
		public async Task Cleanup()
		{
			await _testDbContext.Database.EnsureDeletedAsync();
			await _testDbContext.DisposeAsync();
		}

		#region Constructor tests

		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod()]
		[TestCategory("Constructor tests")]
		public void QueryRepository_should_not_allow_null_context()
		{
			var repo = new QueryRepository<TestDbContext, object>(null, _mapperMock.Object);
		}

		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		[TestCategory("Constructor tests")]
		public void QueryRepository_should_not_allow_null_mapper()
		{
			var repo = new QueryRepository<TestDbContext, object>(new TestDbContext(null), null);
		}

		#endregion

		#region Task<IEnumerable<T>> GetListAsync

		[TestMethod]
		public async Task QueryRepository_GetListAsync_should_allow_null_spec()
		{
			QuerySpecification<Link> spec = null;
			var res = await _linkQueryRepository.GetListAsync(spec, default);

			Assert.IsNotNull(res);
			Assert.AreEqual(15, res.Count());
			Assert.AreEqual(1, res.First().CategoryId);
			Assert.IsNull(res.First().Message);
			Assert.AreEqual(EntityState.Unchanged, _testDbContext.Entry(res.First()).State);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_should_allow_empty_spec()
		{
			var res = await _linkQueryRepository.GetListAsync(new QuerySpecification<Link>(), default);

			Assert.IsNotNull(res);
			Assert.AreEqual(15, res.Count());
			Assert.AreEqual(1, res.First().CategoryId);
			Assert.IsNull(res.First().Message);
			Assert.AreEqual(EntityState.Unchanged, _testDbContext.Entry(res.First()).State);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_should_not_find_entities()
		{
			var res = await _linkQueryRepository.GetListAsync(new QuerySpecification<Link>(x => x.CategoryId > 200), default);

			Assert.IsNotNull(res);
			Assert.AreEqual(0, res.Count());
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_should_apply_all_query_params()
		{
			var res = await _linkQueryRepository.GetListAsync(new QuerySpecification<Link>()
				.AsNonTracking()
				.ApplyFilter(x => x.CategoryId > 10)
				.ApplyOrderBy(new OrderOption<Link>(x => x.CategoryId, true), new OrderOption<Link>(x => x.Message.BookNo, true))
				.ApplyIncludes(x => x.Message)
				.ApplyIncludes($"{nameof(Link.InformationCategory)}")
				, default);

			Assert.IsNotNull(res);
			Assert.AreEqual(5, res.Count());
			Assert.AreEqual(15, res.First().CategoryId);
			Assert.IsNotNull(res.First().Message);
			Assert.IsNotNull(res.First().InformationCategory);
			Assert.AreEqual(EntityState.Detached, _testDbContext.Entry(res.First()).State);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_should_include_multiple_spec()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyIncludes(x => x.Message, x=> x.InformationCategory);
							
			var res = await _linkQueryRepository.GetListAsync(spec, default);

			var messagesCount = res.Count(x => x.Message != null);
			var informationCategoryCount = res.Count(x => x.InformationCategory != null);

			Assert.AreEqual(15, messagesCount);
			Assert.AreEqual(15, informationCategoryCount);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_should_order_multiple_spec()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyOrderBy(new OrderOption<Link>(x => x.Organization, false), new OrderOption<Link>(x => x.CategoryId, false));


			var res = await _linkQueryRepository.GetListAsync(spec, default);

			Assert.AreEqual(1, res.First().CategoryId);
			Assert.AreEqual(15, res.Last().CategoryId);
			Assert.AreEqual("org 1", res.First().Organization);
			Assert.AreEqual("org 2", res.Skip(5).First().Organization);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_should_orderDesc_multiple_spec()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyOrderBy(new OrderOption<Link>(x => x.Organization, true), new OrderOption<Link>(x => x.CategoryId, true));

			var res = await _linkQueryRepository.GetListAsync(spec, default);

			Assert.AreEqual(15, res.First().CategoryId);
			Assert.AreEqual(1, res.Last().CategoryId);
			Assert.AreEqual("org 2", res.First().Organization);
			Assert.AreEqual("org 1", res.Skip(10).First().Organization);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_should_order_multiple_directions()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyOrderBy(new OrderOption<Link>(x => x.Organization, false), new OrderOption<Link>(x => x.CategoryId, true));

			var res = await _linkQueryRepository.GetListAsync(spec, default);

			Assert.AreEqual(5, res.First().CategoryId);
			Assert.AreEqual(6, res.Last().CategoryId);
			Assert.AreEqual("org 1", res.First().Organization);
			Assert.AreEqual("org 2", res.Skip(5).First().Organization);
		}


		[TestMethod]
		public async Task QueryRepository_GetListAsync_should_include_multiple_Filters()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyFilters(x => x.CategoryId > 3, 
									  x => x.Organization == "org 1",
									  x => x.Message != null);

			var res = await _linkQueryRepository.GetListAsync(spec, default);

			Assert.AreEqual(2, res.Count());
		}

		#endregion

		#region Task<IEnumerable<T>> GetListAsync<TResult>

		[TestMethod]
		public async Task QueryRepository_GetListAsync_Mapped_should_allow_null_spec()
		{
			QuerySpecification<Link> spec = null;
			var res = await _linkQueryRepository.GetListAsync<TestLink>(spec, default);

			Assert.IsNotNull(res);
			Assert.AreEqual(15, res.Count());
			Assert.AreEqual(1, res.First().CategoryId);
			Assert.IsNotNull(res.First().Message);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_Mapped_should_allow_empty_spec()
		{
			var res = await _linkQueryRepository.GetListAsync<TestLink>(new QuerySpecification<Link>(), default);

			Assert.IsNotNull(res);
			Assert.AreEqual(15, res.Count());
			Assert.AreEqual(1, res.First().CategoryId);
			Assert.IsNotNull(res.First().Message);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_Mapped_should_not_find_entities()
		{
			var res = await _linkQueryRepository.GetListAsync<TestLink>(new QuerySpecification<Link>(x => x.CategoryId > 200), default);

			Assert.IsNotNull(res);
			Assert.AreEqual(0, res.Count());
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_Mapped_should_apply_all_query_params()
		{
			var res = await _linkQueryRepository.GetListAsync<TestLink>(new QuerySpecification<Link>()
				.AsNonTracking()
				.ApplyFilter(x => x.CategoryId > 10)
				.ApplyOrderBy(new OrderOption<Link>(x => x.CategoryId, true), new OrderOption<Link>(x => x.Message.BookNo, true))
				.ApplyIncludes(x => x.Message)
				, default);

			Assert.IsNotNull(res);
			Assert.AreEqual(5, res.Count());
			Assert.AreEqual(15, res.First().CategoryId);
			Assert.IsNotNull(res.First().Message);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_Mapped_should_include_multiple_spec()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyIncludes(x => x.Message, x => x.InformationCategory);

			var res = await _linkQueryRepository.GetListAsync<TestLink>(spec, default);

			var messagesCount = res.Count(x => x.Message != null);

			Assert.AreEqual(15, messagesCount);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_Mapped_should_order_multiple_spec()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyOrderBy(new OrderOption<Link>(x => x.Organization, false), new OrderOption<Link>(x => x.CategoryId, false));

			var res = await _linkQueryRepository.GetListAsync<TestLink>(spec, default);

			Assert.AreEqual(1, res.First().CategoryId);
			Assert.AreEqual(15, res.Last().CategoryId);
			Assert.AreEqual("org 1", res.First().Organization);
			Assert.AreEqual("org 2", res.Skip(5).First().Organization);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_Mapped_should_orderDesc_multiple_spec()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyOrderBy(new OrderOption<Link>(x => x.Organization, true), new OrderOption<Link>(x => x.CategoryId, true));

			var res = await _linkQueryRepository.GetListAsync<TestLink>(spec, default);

			Assert.AreEqual(15, res.First().CategoryId);
			Assert.AreEqual(1, res.Last().CategoryId);
			Assert.AreEqual("org 2", res.First().Organization);
			Assert.AreEqual("org 1", res.Skip(10).First().Organization);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_Mapped_should_order_multiple_directions()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyOrderBy(new OrderOption<Link>(x => x.Organization, false), new OrderOption<Link>(x => x.CategoryId, true));

			var res = await _linkQueryRepository.GetListAsync<TestLink>(spec, default);

			Assert.AreEqual(5, res.First().CategoryId);
			Assert.AreEqual(6, res.Last().CategoryId);
			Assert.AreEqual("org 1", res.First().Organization);
			Assert.AreEqual("org 2", res.Skip(5).First().Organization);
		}


		[TestMethod]
		public async Task QueryRepository_GetListAsync_Mapped_should_include_multiple_Filters()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyFilters(x => x.CategoryId > 3,
									  x => x.Organization == "org 1",
									  x => x.Message != null);

			var res = await _linkQueryRepository.GetListAsync<TestLink>(spec, default);

			Assert.AreEqual(2, res.Count());
		}

		#endregion

		#region Task<IEnumerable<T>> GetListAsync<TSelect>

		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		public async Task QueryRepository_GetListAsync_TSelect_should_allow_null_spec()
		{
			QuerySpecification<Link> spec = null;
			Expression<Func<Link, string>> select = null;
			var res = await _linkQueryRepository.GetListAsync<string>(spec, select, default);
		}

		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		public async Task QueryRepository_GetListAsync_TSelect_should_allow_empty_spec()
		{
			Expression<Func<Link, string>> select = null;
			var res = await _linkQueryRepository.GetListAsync<string>(new QuerySpecification<Link>(), select, default);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_TSelect_should_not_find_entities()
		{
			var res = await _linkQueryRepository.GetListAsync<TestLink>(new QuerySpecification<Link>(x => x.CategoryId > 200), default);

			Assert.IsNotNull(res);
			Assert.AreEqual(0, res.Count());
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_TSelect_should_apply_all_query_params()
		{
			var res = await _linkQueryRepository.GetListAsync(new QuerySpecification<Link>()
				.AsNonTracking()
				.ApplyFilter(x => x.CategoryId > 10)
				.ApplyOrderBy(new OrderOption<Link>(x => x.CategoryId, true), new OrderOption<Link>(x => x.Message.BookNo, true))
				.ApplyIncludes(x => x.Message),
				x => new { x.Message, x.CategoryId },
				default);

			Assert.IsNotNull(res);
			Assert.AreEqual(5, res.Count());
			Assert.AreEqual(15, res.First().CategoryId);
			Assert.IsNotNull(res.First().Message);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_TSelect_should_include_multiple_spec()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyIncludes(x => x.Message, x => x.InformationCategory);

			var res = await _linkQueryRepository.GetListAsync<Message>(spec, x => x.Message, default);

			var messagesCount = res.Count(x => x != null);

			Assert.AreEqual(15, messagesCount);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_TSelect_should_order_multiple_spec()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyOrderBy(new OrderOption<Link>(x => x.Organization, false), new OrderOption<Link>(x => x.CategoryId, false));

			var res = await _linkQueryRepository.GetListAsync(spec, x => new { x.Organization, x.CategoryId }, default);

			Assert.AreEqual(1, res.First().CategoryId);
			Assert.AreEqual(15, res.Last().CategoryId);
			Assert.AreEqual("org 1", res.First().Organization);
			Assert.AreEqual("org 2", res.Skip(5).First().Organization);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_TSelect_should_orderDesc_multiple_spec()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyOrderBy(new OrderOption<Link>(x => x.Organization, true), new OrderOption<Link>(x => x.CategoryId, true));

			var res = await _linkQueryRepository.GetListAsync(spec, x => new { x.Organization, x.CategoryId }, default);

			Assert.AreEqual(15, res.First().CategoryId);
			Assert.AreEqual(1, res.Last().CategoryId);
			Assert.AreEqual("org 2", res.First().Organization);
			Assert.AreEqual("org 1", res.Skip(10).First().Organization);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_TSelect_should_order_multiple_directions()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyOrderBy(new OrderOption<Link>(x => x.Organization, false), new OrderOption<Link>(x => x.CategoryId, true));

			var res = await _linkQueryRepository.GetListAsync(spec, x => new { x.Organization, x.CategoryId }, default);

			Assert.AreEqual(5, res.First().CategoryId);
			Assert.AreEqual(6, res.Last().CategoryId);
			Assert.AreEqual("org 1", res.First().Organization);
			Assert.AreEqual("org 2", res.Skip(5).First().Organization);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_TSelect_should_include_multiple_Filters()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyFilters(x => x.CategoryId > 3,
									  x => x.Organization == "org 1",
									  x => x.Message != null);

			var res = await _linkQueryRepository.GetListAsync<string>(spec, x => x.Organization, default);

			Assert.AreEqual(2, res.Count());
			Assert.AreEqual("org 1", res.First());
			Assert.AreEqual("org 1", res.ElementAt(1));
		}

		#endregion


		#region Task<IEnumerable<T>> GetListAsync Paged

		[TestMethod]
		public async Task QueryRepository_GetListAsync_paged_should_allow_null_spec()
		{
			QuerySpecification<Link> spec = null;
			var res = await _linkQueryRepository.GetListAsync(spec, null, default);

			Assert.IsNotNull(res);
			Assert.AreEqual(10, res.Data.Count());
			Assert.AreEqual(1, res.Data.First().CategoryId);
			Assert.AreEqual(15, res.TotalItems);
			Assert.IsNull(res.Data.First().Message);
			Assert.AreEqual(EntityState.Unchanged, _testDbContext.Entry(res.Data.First()).State);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_paged_should_allow_empty_spec()
		{
			var res = await _linkQueryRepository.GetListAsync(new QuerySpecification<Link>(), null, default);

			Assert.IsNotNull(res);
			Assert.AreEqual(10, res.Data.Count());
			Assert.AreEqual(1, res.Data.First().CategoryId);
			Assert.AreEqual(15, res.TotalItems);
			Assert.IsNull(res.Data.First().Message);
			Assert.AreEqual(EntityState.Unchanged, _testDbContext.Entry(res.Data.First()).State);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_paged_should_not_find_entities()
		{
			var res = await _linkQueryRepository.GetListAsync(new QuerySpecification<Link>(x => x.CategoryId > 200), null, default);

			Assert.IsNotNull(res);
			Assert.AreEqual(0, res.Data.Count());
			Assert.AreEqual(0, res.TotalItems);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_paged_should_apply_all_query_params()
		{
			var res = await _linkQueryRepository.GetListAsync(new QuerySpecification<Link>()
				.AsNonTracking()
				.ApplyFilter(x => x.CategoryId > 10)
				.ApplyOrderBy(new OrderOption<Link>(x => x.CategoryId, true), new OrderOption<Link>(x => x.Message.BookNo, true))
				.ApplyIncludes(x => x.Message),
				new PagingOptions()
				, default);

			Assert.IsNotNull(res);
			Assert.AreEqual(5, res.Data.Count());
			Assert.AreEqual(15, res.Data.First().CategoryId);
			Assert.IsNotNull(res.Data.First().Message);
			Assert.AreEqual(EntityState.Detached, _testDbContext.Entry(res.Data.First()).State);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_paged_should_apply_paging()
		{
			var res = await _linkQueryRepository.GetListAsync(new QuerySpecification<Link>()
				.AsNonTracking()
				, new PagingOptions(2, 3)
				, default);

			Assert.IsNotNull(res);
			Assert.AreEqual(3, res.Data.Count());
			Assert.AreEqual(7, res.Data.First().CategoryId);
			Assert.AreEqual(EntityState.Detached, _testDbContext.Entry(res.Data.First()).State);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_paged_should_ApplyPaging()
		{
			var res = await _linkQueryRepository.GetListAsync(new QuerySpecification<Link>()
				.AsNonTracking()
				, new PagingOptions(2, 3)
				, default);

			Assert.IsNotNull(res);
			Assert.AreEqual(3, res.Data.Count());
			Assert.AreEqual(7, res.Data.First().CategoryId);
			Assert.AreEqual(EntityState.Detached, _testDbContext.Entry(res.Data.First()).State);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_paged_should_include_multiple_spec()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyIncludes(x => x.Message, x => x.InformationCategory);

			var res = await _linkQueryRepository.GetListAsync(spec, null);

			var messagesCount = res.Data.Count(x => x.Message != null);
			var informationCategoryCount = res.Data.Count(x => x.InformationCategory != null);

			Assert.AreEqual(10, messagesCount);
			Assert.AreEqual(10, informationCategoryCount);
			Assert.AreEqual(15, res.TotalItems);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_paged_should_order_multiple_spec()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyOrderBy(new OrderOption<Link>(x => x.Organization, false), new OrderOption<Link>(x => x.CategoryId, false));

			var res = await _linkQueryRepository.GetListAsync(spec, new PagingOptions());

			Assert.AreEqual(1, res.Data.First().CategoryId);
			Assert.AreEqual(10, res.Data.Last().CategoryId);
			Assert.AreEqual("org 1", res.Data.First().Organization);
			Assert.AreEqual("org 2", res.Data.Skip(5).First().Organization);
			Assert.AreEqual(15, res.TotalItems);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_paged_should_orderDesc_multiple_spec()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyOrderBy(new OrderOption<Link>(x => x.Organization, true), new OrderOption<Link>(x => x.CategoryId, true));

			var res = await _linkQueryRepository.GetListAsync(spec, new PagingOptions());

			Assert.AreEqual(15, res.Data.First().CategoryId);
			Assert.AreEqual(6, res.Data.Last().CategoryId);
			Assert.AreEqual("org 2", res.Data.First().Organization);
			Assert.AreEqual("org 2", res.Data.Last().Organization);
			Assert.AreEqual(15, res.TotalItems);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_paged_should_order_multiple_directions()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyOrderBy(new OrderOption<Link>(x => x.Organization, false), new OrderOption<Link>(x => x.CategoryId, true));

			var res = await _linkQueryRepository.GetListAsync(spec, new PagingOptions());

			Assert.AreEqual(5, res.Data.First().CategoryId);
			Assert.AreEqual(11, res.Data.Last().CategoryId);
			Assert.AreEqual("org 1", res.Data.First().Organization);
			Assert.AreEqual("org 2", res.Data.Skip(5).First().Organization);
			Assert.AreEqual(15, res.TotalItems);
		}


		[TestMethod]
		public async Task QueryRepository_GetListAsync_paged_should_include_multiple_Filters()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyFilters(x => x.CategoryId > 3,
									  x => x.Organization == "org 1",
									  x => x.Message != null);

			var res = await _linkQueryRepository.GetListAsync<TestLink>(spec, new PagingOptions());

			Assert.AreEqual(2, res.Data.Count());
		}

		#endregion

		#region GetList

		[TestMethod]
		public void QueryRepository_GetList_should_execute_sql()
		{
			var dataTable = _linkQueryRepository.GetList("select * from Link");
		}

		#endregion

		#region Task<IEnumerable<T>> GetListAsync<TResult> Paged

		[TestMethod]
		public async Task QueryRepository_GetListAsync_paged_mapped_should_allow_null_spec()
		{
			QuerySpecification<Link> spec = null;
			var res = await _linkQueryRepository.GetListAsync<TestLink>(spec, null as PagingOptions, default);

			Assert.IsNotNull(res);
			Assert.AreEqual(10, res.Data.Count());
			Assert.AreEqual(1, res.Data.First().CategoryId);
			Assert.AreEqual(15, res.TotalItems);
			Assert.IsNotNull(res.Data.First().Message);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_paged_mapped_should_allow_empty_spec()
		{
			var res = await _linkQueryRepository.GetListAsync<TestLink>(new QuerySpecification<Link>(), null as PagingOptions, default);

			Assert.IsNotNull(res);
			Assert.AreEqual(10, res.Data.Count());
			Assert.AreEqual(1, res.Data.First().CategoryId);
			Assert.AreEqual(15, res.TotalItems);
			Assert.IsNotNull(res.Data.First().Message);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_paged_mapped_should_not_find_entities()
		{
			var res = await _linkQueryRepository.GetListAsync<TestLink>(new QuerySpecification<Link>(x => x.CategoryId > 200),
				new PagingOptions(),
				default);

			Assert.IsNotNull(res);
			Assert.AreEqual(0, res.Data.Count());
			Assert.AreEqual(0, res.TotalItems);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_paged_mapped_should_apply_all_query_params()
		{
			var res = await _linkQueryRepository.GetListAsync<TestLink>(new QuerySpecification<Link>()
				.AsNonTracking()
				.ApplyFilter(x => x.CategoryId > 10)
				.ApplyOrderBy(new OrderOption<Link>(x => x.CategoryId, true), new OrderOption<Link>(x => x.Message.BookNo, true))
				.ApplyIncludes(x => x.Message)
				, new PagingOptions()
				, default);

			Assert.IsNotNull(res);
			Assert.AreEqual(5, res.Data.Count());
			Assert.AreEqual(15, res.Data.First().CategoryId);
			Assert.IsNotNull(res.Data.First().Message);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_paged_mapped_should_apply_paging()
		{
			var res = await _linkQueryRepository.GetListAsync<TestLink>(new QuerySpecification<Link>()
				.AsNonTracking()
				, new PagingOptions(2, 3)
				, default);

			Assert.IsNotNull(res);
			Assert.AreEqual(3, res.Data.Count());
			Assert.AreEqual(7, res.Data.First().CategoryId);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_paged_mapped_should_ApplyPaging()
		{
			var res = await _linkQueryRepository.GetListAsync<TestLink>(new QuerySpecification<Link>()
				.AsNonTracking()
				, new PagingOptions(2, 3)
				, default);

			Assert.IsNotNull(res);
			Assert.AreEqual(3, res.Data.Count());
			Assert.AreEqual(7, res.Data.First().CategoryId);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_paged_mapped_should_include_multiple_spec()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyIncludes(x => x.Message, x => x.InformationCategory);

			var res = await _linkQueryRepository.GetListAsync<TestLink>(spec, new PagingOptions());

			var messagesCount = res.Data.Count(x => x.Message != null);

			Assert.AreEqual(10, messagesCount);
			Assert.AreEqual(15, res.TotalItems);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_paged_mapped_should_order_multiple_spec()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyOrderBy(new OrderOption<Link>(x => x.Organization, false), new OrderOption<Link>(x => x.CategoryId, false));

			var res = await _linkQueryRepository.GetListAsync<TestLink>(spec, new PagingOptions());

			Assert.AreEqual(1, res.Data.First().CategoryId);
			Assert.AreEqual(10, res.Data.Last().CategoryId);
			Assert.AreEqual("org 1", res.Data.First().Organization);
			Assert.AreEqual("org 2", res.Data.Skip(5).First().Organization);
			Assert.AreEqual(15, res.TotalItems);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_paged_mapped_should_orderDesc_multiple_spec()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyOrderBy(new OrderOption<Link>(x => x.Organization, true), new OrderOption<Link>(x => x.CategoryId, true));

			var res = await _linkQueryRepository.GetListAsync<TestLink>(spec, new PagingOptions());

			Assert.AreEqual(15, res.Data.First().CategoryId);
			Assert.AreEqual(6, res.Data.Last().CategoryId);
			Assert.AreEqual("org 2", res.Data.First().Organization);
			Assert.AreEqual("org 2", res.Data.Last().Organization);
			Assert.AreEqual(15, res.TotalItems);
		}

		[TestMethod]
		public async Task QueryRepository_GetListAsync_paged_mapped_should_order_multiple_directions()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyOrderBy(new OrderOption<Link>(x => x.Organization, false), new OrderOption<Link>(x => x.CategoryId, true));

			var res = await _linkQueryRepository.GetListAsync<TestLink>(spec, new PagingOptions());

			Assert.AreEqual(5, res.Data.First().CategoryId);
			Assert.AreEqual(11, res.Data.Last().CategoryId);
			Assert.AreEqual("org 1", res.Data.First().Organization);
			Assert.AreEqual("org 2", res.Data.Skip(5).First().Organization);
			Assert.AreEqual(15, res.TotalItems);
		}

		#endregion

		#region Task<T> FindAsync

		[TestMethod]
		public async Task QueryRepository_FindAsync_should_handle_null_spec()
		{
			var res = await _linkQueryRepository.FindAsync(null);

			Assert.IsNotNull(res);
		}

		[TestMethod]
		public async Task QueryRepository_FindAsync_should_handle_default_spec()
		{
			var spec = new QuerySpecification<Link>();
			var res = await _linkQueryRepository.FindAsync(spec);

			Assert.IsNotNull(res);
		}

		[TestMethod]
		public async Task QueryRepository_FindAsync_should_not_find_entity()
		{
			var spec = new QuerySpecification<Link>(x => x.CategoryId > 200);
			var res = await _linkQueryRepository.FindAsync(spec);

			Assert.IsNull(res);
		}

		[TestMethod]
		public async Task QueryRepository_FindAsync_should_include_multiple_spec()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyIncludes(x => x.Message, x => x.InformationCategory);

			var res = await _linkQueryRepository
				.FindAsync(spec);

			Assert.IsTrue(res.Message != null);
			Assert.IsTrue(res.InformationCategory != null);
			Assert.AreEqual(EntityState.Detached, _testDbContext.Entry(res).State);
		}

		[TestMethod]
		public async Task QueryRepository_FindAsync_should_orderby_multiple_spec()
		{
			var spec = new QuerySpecification<Link>()
						.ApplyOrderBy(new OrderOption<Link>(x => x.Organization, false), new OrderOption<Link>(x => x.CategoryId, false));

			var res = await _linkQueryRepository.FindAsync(spec);

			Assert.AreEqual(1, res.CategoryId);
			Assert.AreEqual("org 1", res.Organization);
			Assert.AreEqual(EntityState.Unchanged, _testDbContext.Entry(res).State);
		}

		[TestMethod]
		public async Task QueryRepository_FindAsync_should_orderbyDesc_multiple_spec()
		{
			var spec = new QuerySpecification<Link>()
						.ApplyOrderBy(new OrderOption<Link>(x => x.Organization, true), new OrderOption<Link>(x => x.CategoryId, true));

			var res = await _linkQueryRepository.FindAsync(spec);
			
			Assert.AreEqual(15, res.CategoryId);
			Assert.AreEqual("org 2", res.Organization);
		}

		#endregion

		#region Task<T> FindAsync<TResult>

		[TestMethod]
		public async Task QueryRepository_FindAsync_Mapped_should_handle_null_spec()
		{
			var res = await _linkQueryRepository.FindAsync<TestLink>(null, default);

			Assert.IsNotNull(res);
		}

		[TestMethod]
		public async Task QueryRepository_FindAsync_Mapped_should_handle_default_spec()
		{
			var spec = new QuerySpecification<Link>();
			var res = await _linkQueryRepository.FindAsync<TestLink>(spec, default);

			Assert.IsNotNull(res);
			Assert.IsNotNull(res.Message);
		}

		[TestMethod]
		public async Task QueryRepository_FindAsync_Mapped_should_not_find_entity()
		{
			var spec = new QuerySpecification<Link>(x => x.CategoryId > 200);
			var res = await _linkQueryRepository.FindAsync<TestLink>(spec, default);

			Assert.IsNull(res);
		}

		[TestMethod]
		public async Task QueryRepository_FindAsync_Mapped_should_include_multiple_spec()
		{
			var spec = new QuerySpecification<Link>()
						.AsNonTracking()
						.ApplyIncludes(x => x.Message, x => x.InformationCategory);

			var res = await _linkQueryRepository.FindAsync<TestLink>(spec, default);

			Assert.IsNotNull(res);
			Assert.IsNotNull(res.Message);
		}

		[TestMethod]
		public async Task QueryRepository_FindAsync_Mapped_should_orderby_multiple_spec()
		{
			var spec = new QuerySpecification<Link>()
						.ApplyOrderBy(new OrderOption<Link>(x => x.CategoryId, false), new OrderOption<Link>(x => x.Organization, false));

			var res = await _linkQueryRepository.FindAsync<TestLink>(spec, default);

			Assert.AreEqual(1, res.CategoryId);
			Assert.AreEqual("org 1", res.Organization);
		}

		[TestMethod]
		public async Task QueryRepository_FindAsync_Mapped_should_orderbyDesc_multiple_spec()
		{
			var spec = new QuerySpecification<Link>()
						.ApplyOrderBy(new OrderOption<Link>(x => x.CategoryId, true), new OrderOption<Link>(x => x.Organization, true));

			var res = await _linkQueryRepository.FindAsync<TestLink>(spec, default);

			Assert.AreEqual(15, res.CategoryId);
			Assert.AreEqual("org 2", res.Organization);
		}

		#endregion

		#region Task<T> FindAsync<TSelect>

		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		public async Task QueryRepository_FindAsync_TSelect_should_handle_null_spec()
		{
			var res = await _linkQueryRepository.FindAsync<string>(null, null, default);
		}

		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		public async Task QueryRepository_FindAsync_TSelect_should_handle_default_spec()
		{
			var spec = new QuerySpecification<Link>();
			var res = await _linkQueryRepository.FindAsync<string>(spec, null, default);
		}

		[TestMethod]
		public async Task QueryRepository_FindAsync_TSelect_should_handle_simple_selector()
		{
			var spec = new QuerySpecification<Link>();
			var res = await _linkQueryRepository.FindAsync<string>(spec, x => x.Organization, default);

			Assert.IsNotNull(res);
			Assert.AreEqual("org 1", res);
		}

		[TestMethod]
		public async Task QueryRepository_FindAsync_TSelect_should_handle_anonymous_selector()
		{
			var spec = new QuerySpecification<Link>();
			var res = await _linkQueryRepository.FindAsync(spec, x => new { x.Organization, x.CategoryId}, default);

			Assert.IsNotNull(res);
			Assert.AreEqual("org 1", res.Organization);
			Assert.AreEqual(1, res.CategoryId);
		}

		#endregion
	}
}