namespace advertising_platforms_service.Domain.Models
{
    public class AdvertisingPlatform
    {
        public string Name { get; set; } = string.Empty;
        public List<string> Locations { get; set; } = new();
    }
}
