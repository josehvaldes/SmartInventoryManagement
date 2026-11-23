
namespace SmartInventory.Application.Common.Cache
{
    public interface ICacheKeyFactory
    {
        string ById<T>(Guid id);

        string ByProperty<T>(string name, string property);
    }
}
