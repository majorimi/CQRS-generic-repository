using AutoMapper;

using Microsoft.EntityFrameworkCore;

using System;

namespace Majorsoft.CQRS.Repositories
{
	/// <summary>
	/// Base class for <see cref="QueryRepository{TContext,TEntity}" and <see cref="ICommandRepository{TContext,TEntity}"/>/>
	/// </summary>
	/// <typeparam name="TContext">DbContext Type</typeparam>
	/// <typeparam name="TEntity">Entity Type</typeparam>
	public abstract class BaseRepository<TContext, TEntity> where TContext : DbContext where TEntity : class
	{
		protected readonly TContext _dbContext;
		protected readonly DbSet<TEntity> _dbSet;
		protected readonly IMapper _mapper;

		public BaseRepository(TContext context, IMapper mapper)
		{
			_dbContext = context ?? throw new ArgumentNullException(nameof(context));
			_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

			_dbSet = _dbContext.Set<TEntity>();
		}
	}
}