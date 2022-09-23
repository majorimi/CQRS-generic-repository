using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Majorsoft.CQRS.Repositories
{
	/// <summary>
	/// Default implementation of <see cref="ICommandRepository{TContext, TEntity}"/>
	/// </summary>
	/// <typeparam name="TContext">DbContext Type</typeparam>
	/// <typeparam name="TEntity"></typeparam>
	public class CommandRepository<TContext, TEntity> : BaseRepository<TContext, TEntity>, ICommandRepository<TContext, TEntity> where TContext : DbContext where TEntity : class
	{
		public CommandRepository(TContext context, IMapper mapper) : base(context, mapper)
		{ }

		public async ValueTask<EntityEntry<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken)
		{
			return await _dbSet.AddAsync(entity, cancellationToken);
		}

		public async Task AddAsync(CancellationToken cancellationToken, params TEntity[] entities)
		{
			await AddAsync(entities, cancellationToken);
		}

		public async Task AddAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
		{
			if (entities is null)
			{
				return;
			}

			await _dbSet.AddRangeAsync(entities, cancellationToken);
		}

		public void Delete(TEntity entity)
		{
			if (entity is null)
			{
				return;
			}

			_dbSet.Remove(entity);
		}

		public void Delete(params TEntity[] entities)
		{
			if (entities is null)
			{
				return;
			}

			_dbSet.RemoveRange(entities);
		}

		public void Delete(IEnumerable<TEntity> entities)
		{
			if (entities is null)
			{
				return;
			}

			_dbSet.RemoveRange(entities);
		}

		public void Delete(Expression<Func<TEntity, bool>> predicate)
		{
			var entities = _dbSet.Where(predicate);

			if (entities is not null)
			{
				_dbSet.RemoveRange(entities);
			}
		}

		public void Update(TEntity entity)
		{
			_dbSet.Attach(entity);
			_dbContext.Entry(entity).State = EntityState.Modified;
		}

		public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
		{
			return await _dbContext.SaveChangesAsync(cancellationToken);
		}
	}
}