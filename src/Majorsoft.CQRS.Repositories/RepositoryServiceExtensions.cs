using Majorsoft.CQRS.Repositories.Interfaces;

using Microsoft.Extensions.DependencyInjection;

namespace Majorsoft.CQRS.Repositories
{
	public static class RepositoryServiceExtensions
	{
		public static IServiceCollection AddRepositories(this IServiceCollection services)
		{
			services.AddMemoryCache();

			services.AddTransient(typeof(IQueryRepository<,>), typeof(QueryRepository<,>));
			services.AddTransient(typeof(ICommandRepository<,>), typeof(CommandRepository<,>));

			//Cached repositories config
			services.AddSingleton<IInMemoryCacheExpiryProvider, DefaultConfigInMemoryCacheExpiryProvider>();

			return services;
		}
	}
}