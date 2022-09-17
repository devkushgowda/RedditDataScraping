using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace RedditDataScraping.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Submission
    {
        [DefaultValue("")]
        [JsonProperty("id")]
        public string Id { get; set; }

        [DefaultValue("")]
        [JsonProperty("subreddit")]
        public string Subreddit { get; set; }

        [DefaultValue("")]
        [JsonProperty("full_link")]
        public string FullLink { get; set; }

        [DefaultValue("")]
        [JsonProperty("title")]
        public string Title { get; set; }

        [DefaultValue("")]
        [JsonProperty("selftext")]
        public string Selftext { get; set; }

        [DefaultValue("")]
        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("is_submission")]
        public bool IsSubmission { get; set; } = true;

        [DefaultValue("")]
        [JsonProperty("topic")]
        public string Topic { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("created_utc")]
        public string CreatedUtc
        {
            set
            {
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(long.Parse(value)).LocalDateTime.ToUniversalTime();
            }
        }
    }


}
