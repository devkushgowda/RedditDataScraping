using Newtonsoft.Json;
using System.Collections.Generic;

namespace RedditDataScraping.Models
{
    public class RedditResponse<T> where T : class, new()
    {
        [JsonProperty("data")]
        public List<T> Data { get; set; }
    }
}
