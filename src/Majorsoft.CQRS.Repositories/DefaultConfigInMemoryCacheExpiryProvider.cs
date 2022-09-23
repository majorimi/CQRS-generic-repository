using Majorsoft.CQRS.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

using System;

namespace Majorsoft.CQRS.Repositories
{
	/// <summary>
	/// Default InMemory cache expiration time provider which is reading config values.
	/// NOTE: new implementations can be declared and registered per repo type, etc. in order to specify different expiration times.
	/// </summary>
	public class DefaultConfigInMemoryCacheExpiryProvider : IInMemoryCacheExpiryProvider
	{
		public int? CacheExpiryInSec { get; init; } = null;

		public DefaultConfigInMemoryCacheExpiryProvider(IConfiguration configuration)
		{
			if (configuration is null)
			{
				throw new ArgumentNullException(nameof(configuration));
			}

			var expiry = configuration?.GetValue<int?>("GenericRepository:InMemoryCacheExpiryInSec");
			CacheExpiryInSec = expiry;
		}
	}
}