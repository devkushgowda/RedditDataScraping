using System.Collections.Generic;

namespace RedditDataScraping.Models
{
    public class Configuration
    {
        public List<Entry> SubReddits { get; set; } = new List<Entry>() { };
        public List<Entry> Topics { get; set; } = new List<Entry>() { };
    }

    public class Entry
    {
        public string Name { get; set; }
        public uint Count { get; set; }
    }
}
