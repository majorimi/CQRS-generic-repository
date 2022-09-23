namespace Majorsoft.CQRS.Repositories.Interfaces
{
    public interface IInMemoryCacheExpiryProvider
    {
        int? CacheExpiryInSec { get; }
    }
}