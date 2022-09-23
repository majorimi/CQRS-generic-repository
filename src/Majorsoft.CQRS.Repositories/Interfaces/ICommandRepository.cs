using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Majorsoft.CQRS.Repositories
{
	/// <summary>
	/// Command repository for Create, Update and Delete operations
	/// </summary>
	/// <typeparam name="TContext">DbContext Type</typeparam>
	/// <typeparam name="TEntity">Entity Type</typeparam>
	public interface ICommandRepository<TContext, TEntity> where TContext : DbContext where TEntity : class
	{
		/// <summary>
		/// Adds an Entity object to DBSet (not saved to DB)
		/// </summary>
		/// <param name="entity">Entity object</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>async ValueTask</returns>
		ValueTask<EntityEntry<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken);

		/// <summary>
		/// Adds multiple Entity objects to DBSet (not saved to DB)
		/// </summary>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <param name="entities">Entity objects</param>
		/// <returns>async Task</returns>
		Task AddAsync(CancellationToken cancellationToken, params TEntity[] entities);

		/// <summary>
		/// Adds multiple Entity objects to DBSet (not saved to DB)
		/// </summary>
		/// <param name="entities">Entity objects</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>async Task</returns>
		Task AddAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);

		/// <summary>
		/// Removes an Entity objects from DBSet (not saved to DB)
		/// </summary>
		/// <param name="entity">Entity object</param>
		void Delete(TEntity entity);

		/// <summary>
		/// Removes multiple Entity object from DBSet (not saved to DB)
		/// </summary>
		/// <param name="entities">Entity objects</param>
		void Delete(params TEntity[] entities);

		/// <summary>
		/// Removes multiple Entity object from DBSet (not saved to DB)
		/// </summary>
		/// <param name="entities">Entity objects</param>
		void Delete(IEnumerable<TEntity> entities);

		/// <summary>
		/// Removes multiple Entity object from DBSet by given Query (not saved to DB)
		/// </summary>
		/// <param name="predicate">Filter predicate</param>
		void Delete(Expression<Func<TEntity, bool>> predicate);

		/// <summary>
		///Update Entity object from DBSet (not saved to DB)
		/// </summary>
		/// <param name="entities">Entity objects</param>
		void Update(TEntity entity);

		/// <summary>
		/// Saves every changes (Add, Delete and Update) made on EVERY Entity objects in this DB Contexts (should be called once per operation)
		/// </summary>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>async Task</returns>
		Task<int> SaveChangesAsync(CancellationToken cancellationToken);
	}
}