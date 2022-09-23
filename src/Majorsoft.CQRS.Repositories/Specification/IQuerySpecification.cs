using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Majorsoft.CQRS.Repositories.Specification
{
    /// <summary>
    /// Rule set for Generic repository.
    /// Specification design pattern allows us to check whether our objects meet certain requirements.
    /// </summary>
    /// <typeparam name="T">Entity Type</typeparam>
    public interface IQuerySpecification<T>
    {
        /// <summary>
        /// Gets if query result change tracked if set with <see cref="AsNonTracking"/>
        /// </summary>
        bool IsNonTrackableQuery { get; }

        /// <summary>
        /// Gets filter option if set with <see cref="ApplyFilter(Expression{Func{T, bool}})"/>
        /// </summary>
        Expression<Func<T, bool>> FilterCondition { get; }

        /// <summary>
        /// Gets order by options if set with <see cref="ApplyOrderBy(Expression{Func{T, object}}[])"/>
        /// </summary>
        IEnumerable<OrderOption<T>> OrderOptions { get; }

        /// <summary>
        /// Gets include options if set with <see cref="ApplyIncludes(Expression{Func{T, object}}[])"/>
        /// </summary>
        IEnumerable<Expression<Func<T, object>>> Includes { get; }

        /// <summary>
        /// Gets include options if set with string navigation properties
        /// </summary>
        IEnumerable<string> IncludeStrings { get; }

        /// <summary>
        /// Gets filter options if set with <see cref="ApplyFilters(Expression{Func{T, bool}}[])"/>
        /// </summary>
        IEnumerable<Expression<Func<T, bool>>> FilterConditions { get; }

        /// <summary>
        /// Applies filter on query (where) condition method can be called multiple times but value will be overridden
        /// </summary>
        /// <param name="predicate">Filter options</param>
        /// <returns>Instance of <see cref="IQuerySpecification{T}"/> for chainalble config</returns>
        IQuerySpecification<T> ApplyFilter(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Applies multiple filters on query (where) conditions method can be called multiple times filters will be unioned
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>Instance of <see cref="IQuerySpecification{T}"/> for chainalble config</returns>
        IQuerySpecification<T> ApplyFilters(params Expression<Func<T, bool>>[] predicate);

        /// <summary>
        /// Applies include operation (eager loading)
        /// </summary>
        /// <param name="Includes">Navigation property to include Entity types</param>
        /// <returns>Instance of <see cref="IQuerySpecification{T}"/> for chainalble config</returns>
        IQuerySpecification<T> ApplyIncludes(params Expression<Func<T, object>>[] Includes);

        /// <summary>
        /// Applies include operation (eager loading)
        /// </summary>
        /// <example>
        /// For example:   
        /// <code>
        /// IQuerySpecification<T> querySpecification = new QuerySpecification<T>();
        /// querySpecification.ApplyIncludes($"{nameof(T.NavigationProperty)}.{nameof(T.NavigationProperty)}")
        /// </code>
        /// </example>
        /// <param name="Includes">Strings to include Entity types</param>
        /// <returns>Instance of <see cref="IQuerySpecification{T}"/> for chainalble config</returns>
        IQuerySpecification<T> ApplyIncludes(params string[] includeStrings);

        /// <summary>
        /// Marks query to orderable
        /// </summary>
        /// <param name="orderBy">Order by option</param>
        /// <returns>Instance of <see cref="IQuerySpecification{T}"/> for chainalble config</returns>
        IQuerySpecification<T> ApplyOrderBy(params OrderOption<T>[] orderBy);

        /// <summary>
        /// Marks query as NonTracking (entity objects changes are not detected)
        /// </summary>
        /// <returns>Instance of <see cref="IQuerySpecification{T}"/> for chainalble config</returns>
        IQuerySpecification<T> AsNonTracking();

        /// <summary>
        /// Provide override of <see cref="ToString"/> method for caching key.
        /// </summary>
        /// <returns>Return value as string</returns>
        string ToString();
    }
}