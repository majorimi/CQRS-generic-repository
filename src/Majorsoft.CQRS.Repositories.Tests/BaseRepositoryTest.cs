using AutoMapper;

using Majorsoft.CQRS.Repositories.Tests.TestDb;

using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using System;

namespace Majorsoft.CQRS.Repositories.Tests
{
	[TestClass]
	public class BaseRepositoryTest
	{
		private class FakeRepo<TContext, TEntity> : BaseRepository<TContext, TEntity> where TContext : DbContext where TEntity : class
		{
			public FakeRepo(TContext context, IMapper mapper) : base(context, mapper)
			{ }
		}

		private Mock<IMapper> _mapperMock;

		[TestInitialize]
		public void Init()
		{
			_mapperMock = new Mock<IMapper>();
		}

		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		public void BaseRepository_should_not_allow_null_context()
		{
			var repo = new FakeRepo<TestDbContext, object>(null, _mapperMock.Object);
		}

		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		public void BaseRepository_should_not_allow_null_mapper()
		{
			var repo = new FakeRepo<TestDbContext, object>(new TestDbContext(null), null);
		}
	}
}