using AutoMapper;

using Majorsoft.CQRS.Repositories;
using Majorsoft.CQRS.Repositories.Tests.TestDb;

using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Majorsoft.CQRS.Usie.Repositories.Tests
{
	[TestClass]
	public class CommandRepositoryTest
	{
		private TestDbContext _testDbContext;
		private CommandRepository<TestDbContext, Link> _linkCommandRepository;
		private Mock<IMapper> _mapperMock;

		[TestInitialize]
		public async Task Init()
		{
			var options = new DbContextOptionsBuilder<TestDbContext>()
			   .UseInMemoryDatabase($"UsieDatabase_{DateTime.Now.ToString()}")
			   .Options;

			_testDbContext = new TestDbContext(options);
			await _testDbContext.Database.EnsureCreatedAsync();

			_mapperMock = new Mock<IMapper>();
			_linkCommandRepository = new CommandRepository<TestDbContext, Link>(_testDbContext, _mapperMock.Object);
		}

		[TestCleanup]
		public async Task Cleanup()
		{
			await _testDbContext.Database.EnsureDeletedAsync();
			await _testDbContext.DisposeAsync();
		}

		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		public void CommandRepository_should_not_allow_null_context()
		{
			var repo = new CommandRepository<TestDbContext, object>(null, _mapperMock.Object);
		}

		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		public void CommandRepository_should_not_allow_null_mapper()
		{
			var repo = new CommandRepository<TestDbContext, object>(new TestDbContext(null), null);
		}

		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		public async Task CommandRepository_AddAsync_should_not_allow_null()
		{
			Link link = null;
			var res = await _linkCommandRepository.AddAsync(link, default);
		}

		[TestMethod]
		public async Task CommandRepository_AddAsync_should_add_entity()
		{
			var link = new Link();
			var res = await _linkCommandRepository.AddAsync(link, default);

			Assert.IsNotNull(res);
			Assert.AreEqual(0, _testDbContext.Links.Count()); //Not saved
			Assert.AreEqual(1, _testDbContext.ChangeTracker.Entries().Count());
			Assert.AreEqual(EntityState.Added, res.State);
			Assert.AreEqual(EntityState.Added, _testDbContext.Entry(res.Entity).State);
		}

		[TestMethod]
		public async Task CommandRepository_AddAsync_list_should_not_allow_null()
		{
			Link[] links = null;
			await _linkCommandRepository.AddAsync(links, default);

			Assert.AreEqual(0, _testDbContext.Links.Count());
		}

		[TestMethod]
		public async Task CommandRepository_AddAsync_list_should_add_entity()
		{
			Link[] links = new Link[] { new Link() { LinkId = Guid.NewGuid() }, new Link() { LinkId = Guid.NewGuid() } };

			await _linkCommandRepository.AddAsync(links, default);

			Assert.AreEqual(0, _testDbContext.Links.Count()); //Not saved
			Assert.AreEqual(2, _testDbContext.ChangeTracker.Entries().Count());
			Assert.AreEqual(EntityState.Added, _testDbContext.Entry(links[0]).State);
			Assert.AreEqual(EntityState.Added, _testDbContext.Entry(links[1]).State);
		}

		[TestMethod]
		public async Task CommandRepository_AddAsync_param_should_not_allow_null()
		{
			Link[] links = null;
			await _linkCommandRepository.AddAsync(default, links);

			Assert.AreEqual(0, _testDbContext.Links.Count());
		}

		[TestMethod]
		public async Task CommandRepository_AddAsync_param_should_add_entity()
		{
			Link[] links = new Link[] { new Link() { LinkId = Guid.NewGuid() }, new Link() { LinkId = Guid.NewGuid() } };

			await _linkCommandRepository.AddAsync(default, links);

			Assert.AreEqual(0, _testDbContext.Links.Count()); //Not saved
			Assert.AreEqual(2, _testDbContext.ChangeTracker.Entries().Count());
			Assert.AreEqual(EntityState.Added, _testDbContext.Entry(links[0]).State);
			Assert.AreEqual(EntityState.Added, _testDbContext.Entry(links[1]).State);
		}

		[TestMethod]
		public async Task CommandRepository_Delete_should_handle_null()
		{
			await CreateLinks();

			Link link = null;
			_linkCommandRepository.Delete(link);
			await _linkCommandRepository.SaveChangesAsync(default);

			Assert.AreEqual(5, _testDbContext.Links.Count());
		}

		[TestMethod]
		public async Task CommandRepository_Delete_should_not_remove_not_attached_entity()
		{
			await CreateLinks();

			var link = new Link();
			_linkCommandRepository.Delete(link);

			Assert.AreEqual(5, _testDbContext.Links.Count());
			Assert.AreEqual(1, _testDbContext.ChangeTracker.Entries().Count());
		}

		[TestMethod]
		public async Task CommandRepository_Delete_should_remove_attached_entity()
		{
			await CreateLinks();

			var link = await _testDbContext.Links.FirstAsync();
			_linkCommandRepository.Delete(link);

			Assert.AreEqual(5, _testDbContext.Links.Count());
			Assert.AreEqual(1, _testDbContext.ChangeTracker.Entries().Count());
			Assert.AreEqual(EntityState.Deleted, _testDbContext.Entry(link).State);
		}

		[TestMethod]
		public async Task CommandRepository_Delete_list_should_not_allow_null()
		{
			await CreateLinks();

			List<Link> links = null;
			_linkCommandRepository.Delete(links);

			Assert.AreEqual(5, _testDbContext.Links.Count());
			Assert.AreEqual(0, _testDbContext.ChangeTracker.Entries().Count());
		}

		[TestMethod]
		public async Task CommandRepository_Delete_list_should_not_remove_not_attached_entity()
		{
			await CreateLinks();

			var links = new List<Link>() { new Link() { LinkId = Guid.NewGuid() }, new Link() { LinkId = Guid.NewGuid() }};
			_linkCommandRepository.Delete(links);

			Assert.AreEqual(5, _testDbContext.Links.Count());
			Assert.AreEqual(2, _testDbContext.ChangeTracker.Entries().Count()); //Will track non existing objects...
		}

		[TestMethod]
		public async Task CommandRepository_Delete_list_should_remove_attached_entity()
		{
			await CreateLinks();

			var links = await _testDbContext.Links.Take(2).ToListAsync();
			_linkCommandRepository.Delete(links);

			Assert.AreEqual(5, _testDbContext.Links.Count());
			Assert.AreEqual(2, _testDbContext.ChangeTracker.Entries().Count());
			Assert.AreEqual(EntityState.Deleted, _testDbContext.Entry(links[0]).State);
			Assert.AreEqual(EntityState.Deleted, _testDbContext.Entry(links[1]).State);
		}

		[TestMethod]
		public async Task CommandRepository_Delete_params_should_not_allow_null()
		{
			await CreateLinks();

			Link[] links = null;
			_linkCommandRepository.Delete(links);

			Assert.AreEqual(5, _testDbContext.Links.Count());
			Assert.AreEqual(0, _testDbContext.ChangeTracker.Entries().Count());
		}

		[TestMethod]
		public async Task CommandRepository_Delete_params_should_not_remove_not_attached_entity()
		{
			await CreateLinks();

			var links = new Link[] { new Link() { LinkId = Guid.NewGuid() }, new Link() { LinkId = Guid.NewGuid() } };
			_linkCommandRepository.Delete(links);

			Assert.AreEqual(5, _testDbContext.Links.Count());
			Assert.AreEqual(2, _testDbContext.ChangeTracker.Entries().Count()); //Will track non existing objects...
		}

		[TestMethod]
		public async Task CommandRepository_Delete_params_should_remove_attached_entity()
		{
			await CreateLinks();

			var links = await _testDbContext.Links.Take(2).ToArrayAsync();
			_linkCommandRepository.Delete(links);

			Assert.AreEqual(5, _testDbContext.Links.Count());
			Assert.AreEqual(2, _testDbContext.ChangeTracker.Entries().Count());
			Assert.AreEqual(EntityState.Deleted, _testDbContext.Entry(links[0]).State);
			Assert.AreEqual(EntityState.Deleted, _testDbContext.Entry(links[1]).State);
		}

		[TestMethod]
		public async Task CommandRepository_Delete_predicate_should_remove_attached_entity()
		{
			await CreateLinks();
			var links = await _testDbContext.Links.Where(x => x.CategoryId == 1).ToArrayAsync();

			_linkCommandRepository.Delete(x => x.CategoryId == 1);

			Assert.AreEqual(5, _testDbContext.Links.Count());
			Assert.AreEqual(1, _testDbContext.ChangeTracker.Entries().Count());
			Assert.AreEqual(EntityState.Deleted, _testDbContext.Entry(links[0]).State);
		}

		[TestMethod]
		public async Task CommandRepository_SaveChangesAsync_should_save_AddAsync_entity()
		{
			await CreateLinks();

			var links = new Link[] { new Link() { LinkId = Guid.NewGuid() }, new Link() { LinkId = Guid.NewGuid() } };
			await _linkCommandRepository.AddAsync(links, default);

			Assert.AreEqual(5, _testDbContext.Links.Count());
			Assert.AreEqual(2, _testDbContext.ChangeTracker.Entries().Count());
			Assert.AreEqual(EntityState.Added, _testDbContext.Entry(links[0]).State);
			Assert.AreEqual(EntityState.Added, _testDbContext.Entry(links[1]).State);

			var res = await _linkCommandRepository.SaveChangesAsync(default);

			Assert.AreEqual(2, res);
			Assert.AreEqual(7, _testDbContext.Links.Count());
			Assert.AreEqual(2, _testDbContext.ChangeTracker.Entries().Count());
			Assert.AreEqual(EntityState.Unchanged, _testDbContext.Entry(links[0]).State);
			Assert.AreEqual(EntityState.Unchanged, _testDbContext.Entry(links[1]).State);
		}

		[TestMethod]
		public async Task CommandRepository_SaveChangesAsync_should_save_modify_entity()
		{
			await CreateLinks();

			var link = await _testDbContext.Links.FirstAsync();
			link.Description = "updated desc";

			Assert.AreEqual(5, _testDbContext.Links.Count());
			Assert.AreEqual(1, _testDbContext.ChangeTracker.Entries().Count());
			Assert.AreEqual(EntityState.Modified, _testDbContext.Entry(link).State);

			var res = await _linkCommandRepository.SaveChangesAsync(default);
			
			Assert.AreEqual(1, res);
			Assert.AreEqual(5, _testDbContext.Links.Count());
			Assert.AreEqual(1, _testDbContext.ChangeTracker.Entries().Count());
			Assert.AreEqual(EntityState.Unchanged, _testDbContext.Entry(link).State);
		}

		[TestMethod]
		public async Task CommandRepository_SaveChangesAsync_should_save_Delete_attached_entity()
		{
			await CreateLinks();

			var links = await _testDbContext.Links.Take(2).ToArrayAsync();
			_linkCommandRepository.Delete(links);

			var res = await _linkCommandRepository.SaveChangesAsync(default);

			Assert.AreEqual(2, res);
			Assert.AreEqual(3, _testDbContext.Links.Count());
			Assert.AreEqual(0, _testDbContext.ChangeTracker.Entries().Count());
			Assert.AreEqual(EntityState.Detached, _testDbContext.Entry(links[0]).State);
			Assert.AreEqual(EntityState.Detached, _testDbContext.Entry(links[1]).State);
		}

		private async Task CreateLinks()
		{
			for (int i = 0; i < 5; i++)
			{
				_testDbContext.Links.Add(new Link() { LinkId = Guid.NewGuid(), CategoryId = i });
			}

			await _testDbContext.SaveChangesAsync();
			_testDbContext.ChangeTracker.Clear();
		}
	}
}