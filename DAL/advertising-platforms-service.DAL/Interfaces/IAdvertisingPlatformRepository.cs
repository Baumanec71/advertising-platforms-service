using System.Collections.Concurrent;

namespace advertising_platforms_service.DAL.Interfaces
{
    public interface IAdvertisingPlatformRepository
    {
        void Clear();
        ConcurrentDictionary<string, List<string>> GetPlatformsWithLocations();
        void Update(ConcurrentDictionary<string, List<string>> newPlatforms);
    }
}
