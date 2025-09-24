namespace advertising_platforms_service.Domain.Helpers
{
    public class Node
    {
        public Dictionary<string, Node> Children { get; set; } = new();
        public HashSet<Node> Platforms { get; set; } = new();
    }
}
