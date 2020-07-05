namespace DeveloperNews
{
    public class FeedInfo
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public bool IsSelected { get; set; }

        public override string ToString()
        {
            return $"{Name}:{IsSelected}";
        }
    }
}
