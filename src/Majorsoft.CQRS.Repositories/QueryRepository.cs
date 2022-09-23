using AutoMapper;
using AutoMapper.QueryableExtensions;

using Majorsoft.CQRS.Repositories.QueryExtensions;
using Majorsoft.CQRS.Repositories.Specification;

using Microsoft.EntityFrameworkCore;

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
	/// Default implementation of <see cref="IQueryRepository{TContext, TEntity}"/>
	/// </summary>
	/// <typeparam name="TContext">DbContext Type</typeparam>
	/// <typeparam name="TEntity">Entity Type</typeparam>
	public class QueryRepository<TContext, TEntity> : BaseRepository<TContext, TEntity>, IQueryRepository<TContext, TEntity>
		where TContext : DbContext where TEntity : class
	{
		public QueryRepository(TContext context, IMapper mapper) : base(context, mapper)
		{ }

		public async Task<TEntity> FindAsync(IQuerySpecification<TEntity> querySpecifications = null, CancellationToken cancellationToken = default)
		{
			var _query = _dbSet;

			return await QuerySpecificationEvaluator<TEntity>.GetQuery(_query.AsQueryable(), querySpecifications)
									.FirstOrDefaultAsync(cancellationToken);
		}

		public async Task<TSelect> FindAsync<TSelect>(IQuerySpecification<TEntity> querySpecifications, Expression<Func<TEntity, TSelect>> select, CancellationToken cancellationToken = default)
		{
			if (select is null)
			{
				throw new ArgumentNullException(nameof(select));
			}

			var _query = _dbSet;

			return await QuerySpecificationEvaluator<TEntity>.GetQuery(_query.AsQueryable(), querySpecifications)
									.Select(select)
									.FirstOrDefaultAsync(cancellationToken);
		}

		public async Task<TResult> FindAsync<TResult>(IQuerySpecification<TEntity> querySpecifications = null, CancellationToken cancellationToken = default) where TResult : class
		{
			var _query = _dbSet;

			return await QuerySpecificationEvaluator<TEntity>.GetQuery(_query.AsQueryable(), querySpecifications)
									.ProjectTo<TResult>(_mapper.ConfigurationProvider)
									.FirstOrDefaultAsync(cancellationToken);
		}

		public async Task<IEnumerable<TEntity>> GetListAsync(IQuerySpecification<TEntity> querySpecifications = null, CancellationToken cancellationToken = default)
		{
			var query = FilteredQuery(querySpecifications);

			return await query.ToListAsync(cancellationToken);
		}

		public async Task<IEnumerable<TSelect>> GetListAsync<TSelect>(IQuerySpecification<TEntity> querySpecifications, Expression<Func<TEntity, TSelect>> select, CancellationToken cancellationToken = default)
		{
			if (select is null)
			{
				throw new ArgumentNullException(nameof(select));
			}

			var query = FilteredQuery(querySpecifications);

			return await query.Select(select).ToListAsync(cancellationToken);
		}

		public async Task<IEnumerable<TResult>> GetListAsync<TResult>(IQuerySpecification<TEntity> querySpecifications = null, CancellationToken cancellationToken = default) where TResult : class
		{
			var _query = _dbSet;

			return await QuerySpecificationEvaluator<TEntity>.GetQuery(_query.AsQueryable(), querySpecifications)
									.ProjectTo<TResult>(_mapper.ConfigurationProvider)
									.ToListAsync(cancellationToken);
		}

		public async Task<PagedData<TResult>> GetListAsync<TResult>(IQuerySpecification<TEntity> querySpecifications = null, PagingOptions pagingOptions = null, CancellationToken cancellationToken = default) where TResult : class
		{
			var query = FilteredQuery(querySpecifications);
			return await GetListMappedInternalAsync<TResult>(query, pagingOptions, cancellationToken);
		}

		/// <summary>
		/// Method for overridden repositories to call for Paging
		/// </summary>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="query">Custom query to be paged</param>
		/// <param name="pagingOptions">Page size and index options for Paging</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		protected async Task<PagedData<TResult>> GetListMappedInternalAsync<TResult>(IQueryable<TEntity> query, PagingOptions pagingOptions = null, CancellationToken cancellationToken = default) where TResult : class
		{
			var pagedResult = await query.ToPagedResponseAsync(pagingOptions, cancellationToken);

			var data = await pagedResult.Query
						.ProjectTo<TResult>(_mapper.ConfigurationProvider)
						.ToListAsync(cancellationToken);

			return new PagedData<TResult>(data, pagedResult.TotalCount);
		}
		/// <summary>
		/// Get list result from SQl function
		/// </summary>
		/// <typeparam name="TResult">Generic result type</typeparam>
		/// <param name="query">Custom sql query to be executed</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		protected async Task<IEnumerable<TResult>> GetListAsync<TResult>(IQueryable<TEntity> query, CancellationToken cancellationToken = default) where TResult : class
		{
			var data = await query.ProjectTo<TResult>(_mapper.ConfigurationProvider)
									.ToListAsync(cancellationToken);

			return data;
		}

		public async Task<PagedData<TEntity>> GetListAsync(IQuerySpecification<TEntity> querySpecifications = null, PagingOptions pagingOptions = null, CancellationToken cancellationToken = default)
		{
			var query = FilteredQuery(querySpecifications);
			return await GetListInternalAsync(query, pagingOptions, cancellationToken);
		}

		/// <summary>
		/// Method for overridden repositories to call for Paging
		/// </summary>
		/// <param name="query">Custom query to be paged</param>
		/// <param name="pagingOptions">Page size and index options for Paging</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns></returns>
		protected async Task<PagedData<TEntity>> GetListInternalAsync(IQueryable<TEntity> query, PagingOptions pagingOptions = null, CancellationToken cancellationToken = default)
		{
			var pagedResult = await query.ToPagedResponseAsync(pagingOptions, cancellationToken);
			var data = await pagedResult.Query.ToListAsync(cancellationToken);

			return new PagedData<TEntity>(data, pagedResult.TotalCount);
		}

		private IQueryable<TEntity> FilteredQuery(IQuerySpecification<TEntity> querySpecifications = null)
		{
			var _query = _dbSet;
			return QuerySpecificationEvaluator<TEntity>.GetQuery(_query.AsQueryable(), querySpecifications);
		}

		public DataTable GetList(string sqlQuery, params DbParameter[] parameters)
		{
			DataTable dataTable = new DataTable();
			DbConnection connection = _dbContext.Database.GetDbConnection();
			DbProviderFactory dbFactory = DbProviderFactories.GetFactory(connection);
			using (var cmd = dbFactory.CreateCommand()) 
			{
				cmd.Connection = connection;
				cmd.CommandType = CommandType.Text;
				cmd.CommandText = sqlQuery;
				if (parameters != null)
				{
					foreach (var item in parameters)
					{
						cmd.Parameters.Add(item);
					}
				}
				using (DbDataAdapter adapter = dbFactory.CreateDataAdapter())
				{
					adapter.SelectCommand = cmd;
					adapter.Fill(dataTable);
				}
				cmd.Parameters.Clear();
			}
			return dataTable;
		}
	}
}