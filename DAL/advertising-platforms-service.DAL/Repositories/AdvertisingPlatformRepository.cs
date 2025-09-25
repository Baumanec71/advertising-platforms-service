using advertising_platforms_service.DAL.Interfaces;
using System.Collections.Concurrent;

namespace advertising_platforms_service.DAL.Repositories
{
    public class AdvertisingPlatformRepository : IAdvertisingPlatformRepository
    {
        private ConcurrentDictionary<string, List<string>> _platformsWithLocations;
        public AdvertisingPlatformRepository()
        {
            _platformsWithLocations = new ConcurrentDictionary<string, List<string>>();
        }

        public void Clear()
        {
            _platformsWithLocations.Clear();
        }

        public ConcurrentDictionary<string, List<string>> GetPlatformsWithLocations()
        {
            return _platformsWithLocations;
        }

        public void Update(ConcurrentDictionary<string, List<string>> newPlatforms)
        {
            if (newPlatforms == null) throw new ArgumentNullException(nameof(newPlatforms));

            var copy = new ConcurrentDictionary<string, List<string>>();

            foreach (var kvp in newPlatforms)
            {
                copy[kvp.Key] = new List<string>(kvp.Value);
            }

            _platformsWithLocations = copy;
        }
    }
}
