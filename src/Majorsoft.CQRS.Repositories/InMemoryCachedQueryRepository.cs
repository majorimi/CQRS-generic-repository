using Majorsoft.CQRS.Repositories.Interfaces;
using Majorsoft.CQRS.Repositories.Specification;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Majorsoft.CQRS.Repositories
{
	/// <summary>
	/// InMemory object cached implementation of <see cref="IQueryRepository{TContext, TEntity}"/>
	/// </summary>
	/// <typeparam name="TContext">DbContext Type</typeparam>
	/// <typeparam name="TEntity">Entity Type</typeparam>
	public class InMemoryCachedQueryRepository<TContext, TEntity> : IQueryRepository<TContext, TEntity>
		where TContext : DbContext where TEntity : class
	{
		private const int DefaultCacheExpiryInSec = 1200;
		public int CacheExpiryInSec { get; private set; }

		private readonly IMemoryCache _memoryCache;
		private readonly IQueryRepository<TContext, TEntity> _queryRepository;

		public InMemoryCachedQueryRepository(IMemoryCache memoryCache,
			IQueryRepository<TContext, TEntity> queryRepository,
			IInMemoryCacheExpiryProvider expiryProvider)
		{
			_memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
			_queryRepository = queryRepository ?? throw new ArgumentNullException(nameof(queryRepository));

			if (expiryProvider is null)
			{
				throw new ArgumentNullException(nameof(expiryProvider));
			}

			CacheExpiryInSec = expiryProvider.CacheExpiryInSec ?? DefaultCacheExpiryInSec;
		}

		public async Task<TEntity> FindAsync(IQuerySpecification<TEntity> querySpecifications = null, CancellationToken cancellationToken = default)
		{
			return await GetFromCache<TEntity>(querySpecifications,
				() => _queryRepository.FindAsync(querySpecifications, cancellationToken));
		}

		public async Task<TSelect> FindAsync<TSelect>(IQuerySpecification<TEntity> querySpecifications, Expression<Func<TEntity, TSelect>> select, CancellationToken cancellationToken = default)
		{
			return await GetFromCache<TSelect>(querySpecifications,
				() => _queryRepository.FindAsync<TSelect>(querySpecifications, select, cancellationToken),
				select?.ToString());
		}

		public async Task<TResult> FindAsync<TResult>(IQuerySpecification<TEntity> querySpecifications = null, CancellationToken cancellationToken = default) where TResult : class
		{
			return await GetFromCache<TResult>(querySpecifications,
				() => _queryRepository.FindAsync<TResult>(querySpecifications, cancellationToken));
		}

		public DataTable GetList(string sqlQuery, params DbParameter[] parameters)
		{
			var key = $"{nameof(InMemoryCachedQueryRepository<TContext, TEntity>)}<{typeof(DataTable)}>_SqlQuery:{sqlQuery}_Parameters:{string.Join(",", parameters.Select(x => $"{x.ParameterName}:{x.Value}"))}";

			if (_memoryCache.TryGetValue(key, out DataTable entity) && entity is not null)
			{
				return entity;
			}

			var ret = _queryRepository.GetList(sqlQuery, parameters);
			_memoryCache.Set(key, ret);

			return ret;
		}

		public async Task<IEnumerable<TEntity>> GetListAsync(IQuerySpecification<TEntity> querySpecifications = null, CancellationToken cancellationToken = default)
		{
			return await GetFromCache<IEnumerable<TEntity>>(querySpecifications,
				() => _queryRepository.GetListAsync(querySpecifications, cancellationToken));
		}

		public async Task<IEnumerable<TSelect>> GetListAsync<TSelect>(IQuerySpecification<TEntity> querySpecifications, Expression<Func<TEntity, TSelect>> select, CancellationToken cancellationToken = default)
		{
			return await GetFromCache<IEnumerable<TSelect>>(querySpecifications,
				() => _queryRepository.GetListAsync<TSelect>(querySpecifications, select, cancellationToken),
				select?.ToString());
		}

		public async Task<IEnumerable<TResult>> GetListAsync<TResult>(IQuerySpecification<TEntity> querySpecifications = null, CancellationToken cancellationToken = default) where TResult : class
		{
			return await GetFromCache<IEnumerable<TResult>>(querySpecifications,
				async () => await _queryRepository.GetListAsync<TResult>(querySpecifications, cancellationToken));
		}

		public async Task<PagedData<TEntity>> GetListAsync(IQuerySpecification<TEntity> querySpecifications = null, PagingOptions pagingOptions = null, CancellationToken cancellationToken = default)
		{
			return await GetFromCache<PagedData<TEntity>>(querySpecifications,
				async () => await _queryRepository.GetListAsync(querySpecifications, pagingOptions, cancellationToken),
				pagingOptions.ToString());
		}

		public async Task<PagedData<TResult>> GetListAsync<TResult>(IQuerySpecification<TEntity> querySpecifications = null, PagingOptions pagingOptions = null, CancellationToken cancellationToken = default) where TResult : class
		{
			return await GetFromCache<PagedData<TResult>>(querySpecifications,
				async () => await _queryRepository.GetListAsync<TResult>(querySpecifications, pagingOptions, cancellationToken),
				pagingOptions.ToString());
		}

		private async Task<TRet> GetFromCache<TRet>(IQuerySpecification<TEntity> querySpecifications, Func<Task<TRet>> dbFunction, string keyExtension = null)
		{
			var key = querySpecifications?.ToString() ?? $"{nameof(IQuerySpecification<TRet>)}<{typeof(TRet)}>";
			key = $"{key}{keyExtension}";

			if (_memoryCache.TryGetValue(key, out TRet entity) && entity is not null)
			{
				return entity;
			}

			var ret = await dbFunction();
			_memoryCache.Set(key, ret, TimeSpan.FromSeconds(CacheExpiryInSec));

			return ret;
		}
	}
}