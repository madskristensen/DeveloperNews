﻿namespace DeveloperNews
{
    public class FeedInfo
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public bool IsSelected { get; set; }

        public string DisplayName
        {
            get { return Name.TrimStart('!', '?'); }
        }

        public override string ToString()
        {
            return $"{Name}:{IsSelected}";
        }
    }
}
