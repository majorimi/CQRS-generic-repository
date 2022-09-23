using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Majorsoft.CQRS.Repositories.Specification;

using Microsoft.EntityFrameworkCore;

namespace Majorsoft.CQRS.Repositories
{
	/// <summary>
	/// Query repository for only Read operations
	/// </summary>
	/// <typeparam name="TContext">DbContext Type</typeparam>
	/// <typeparam name="TEntity">Entity Type</typeparam>
	public interface IQueryRepository<TContext, TEntity> where TContext : DbContext where TEntity : class
	{
		/// <summary>
		/// Gets a single object by <see cref="IQuerySpecification{T}"/> and returns object <see cref="{T}"/> or NULL
		/// </summary>
		/// <param name="querySpecifications">Specification object</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>async Task</returns>
		Task<TEntity> FindAsync(IQuerySpecification<TEntity> querySpecifications = null, CancellationToken cancellationToken = default);

		/// <summary>
		/// Gets a single object by <see cref="IQuerySpecification{T}"/> and returns only a single or few columns from DB not the full object.
		/// If multiple columns need to be returned probably use <see cref="FindAsync{TResult}(IQuerySpecification{TEntity}, CancellationToken)"/> which uses AutoMapper with projection.
		/// </summary>
		/// <typeparam name="TSelect">Selected column type or anonymous for multiple columns</typeparam>
		/// <param name="querySpecifications">Specification object</param>
		/// <param name="select">Lambda expression to select one or more columns</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Selected column value or anonymous type</returns>
		Task<TSelect> FindAsync<TSelect>(IQuerySpecification<TEntity> querySpecifications, Expression<Func<TEntity, TSelect>> select, CancellationToken cancellationToken = default);

		/// <summary>
		/// Gets a single object by <see cref="IQuerySpecification{T}"/> and returns a transformed (ProjectTo) object to <see cref="{TResult}"/> or NULL
		/// </summary>
		/// <typeparam name="TResult">Result type</typeparam>
		/// <param name="querySpecifications">Specification object</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>async Task</returns>
		Task<TResult> FindAsync<TResult>(IQuerySpecification<TEntity> querySpecifications = null, CancellationToken cancellationToken = default) where TResult : class;

		/// <summary>
		/// Gets a list of objects by <see cref="IQuerySpecification{T}"/> and returns as <see cref="IEnumerable{T}"/>
		/// </summary>
		/// <param name="querySpecifications">Specification object</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>async Task with result set</returns>
		Task<IEnumerable<TEntity>> GetListAsync(IQuerySpecification<TEntity> querySpecifications = null, CancellationToken cancellationToken = default);

		/// <summary>
		/// Gets a list of objects by <see cref="IQuerySpecification{T}"/> and returns only a single or few columns from DB not the full object.
		/// If multiple columns need to be returned probably use <see cref="GetListAsync{TResult}(IQuerySpecification{TEntity}, CancellationToken)"/> which uses AutoMapper with projection.
		/// </summary>
		/// <typeparam name="TSelect">Selected column type or anonymous for multiple columns</typeparam>
		/// <param name="querySpecifications">Specification object</param>
		/// <param name="select">Lambda expression to select one or more columns</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>List of selected column value or anonymous types</returns>
		Task<IEnumerable<TSelect>> GetListAsync<TSelect>(IQuerySpecification<TEntity> querySpecifications, Expression<Func<TEntity, TSelect>> select, CancellationToken cancellationToken = default);

		/// <summary>
		/// Gets a list of objects by <see cref="IQuerySpecification{T}"/> and returns a transformed (ProjectTo) object list to <see cref="PagedData{TResult}"/>
		/// </summary>
		/// <typeparam name="TResult">Result type</typeparam>
		/// <param name="querySpecifications">Specification object</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>async Task with result set</returns>
		Task<IEnumerable<TResult>> GetListAsync<TResult>(IQuerySpecification<TEntity> querySpecifications = null, CancellationToken cancellationToken = default) where TResult : class;

		/// <summary>
		/// Gets a list of objects by <see cref="IQuerySpecification{T}"/> and returns as <see cref="IEnumerable{T}"/>
		/// </summary>
		/// <param name="querySpecifications">Specification object</param>
		/// <param name="pagingOptions">Page size and index options for Paging</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>async Task with paged data</returns>
		Task<PagedData<TEntity>> GetListAsync(IQuerySpecification<TEntity> querySpecifications = null, PagingOptions pagingOptions = null, CancellationToken cancellationToken = default);

		/// <summary>
		/// Gets a list of objects by <see cref="IQuerySpecification{T}"/> and returns a transformed (ProjectTo) object list to <see cref="PagedData{TResult}"/>
		/// </summary>
		/// <typeparam name="TResult">Result type</typeparam>
		/// <param name="querySpecifications">Specification object</param>
		/// <param name="pagingOptions">Page size and index options for Paging</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>async Task with paged data</returns>
		Task<PagedData<TResult>> GetListAsync<TResult>(IQuerySpecification<TEntity> querySpecifications = null, PagingOptions pagingOptions = null, CancellationToken cancellationToken = default) where TResult : class;
		
		/// <summary>
		/// Execute SP or sql query & return data in DataTable
		/// </summary>
		/// <param name="sqlQuery">Raw sql query</param>
		/// <param name="parameters">Any parameters for the sql query or procedures</param>
		/// <returns></returns>
		DataTable GetList(string sqlQuery, params DbParameter[] parameters);
	}
}