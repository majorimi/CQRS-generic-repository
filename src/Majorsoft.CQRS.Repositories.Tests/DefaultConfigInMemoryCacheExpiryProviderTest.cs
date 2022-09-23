using Majorsoft.CQRS.Repositories;

using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Moq;

using System;

namespace Majorsoft.CQRS.Usie.Repositories.Tests
{
	[TestClass]
	public class DefaultConfigInMemoryCacheExpiryProviderTest
	{
		private Mock<IConfiguration> _mockConfig;

		private DefaultConfigInMemoryCacheExpiryProvider _cacheExpiryProvider;

		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		public void DefaultConfigInMemoryCacheExpiryProvider_constructor_should_check_null()
		{
			_cacheExpiryProvider = new DefaultConfigInMemoryCacheExpiryProvider(null);
		}

		[TestMethod]
		public void DefaultConfigInMemoryCacheExpiryProvider_should_read_config()
		{
			var section = new Mock<IConfigurationSection>();
			section.SetupGet(g => g.Value).Returns("9999");

			_mockConfig = new Mock<IConfiguration>();
			_mockConfig.Setup(s => s.GetSection("GenericRepository:InMemoryCacheExpiryInSec")).Returns(section.Object);

			_cacheExpiryProvider = new DefaultConfigInMemoryCacheExpiryProvider(_mockConfig.Object);

			Assert.AreEqual(9999, _cacheExpiryProvider.CacheExpiryInSec);
		}
	}
}