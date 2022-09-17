using Newtonsoft.Json;
using System;
using System.ComponentModel;

namespace RedditDataScraping.Models
{
    public class Comment
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
        [JsonProperty("body")]
        public string Body { get; set; }

        [DefaultValue("")]
        [JsonProperty("author")]
        public string Author { get; set; }

        [DefaultValue("")]
        [JsonProperty("parent_id")]
        public string ParentId { get; set; }
        
        [DefaultValue("")]
        [JsonProperty("parent_body")]
        public string ParentBody { get; set; }

        [JsonProperty("is_submission")]
        public bool IsSubmission { get; set; } = false;

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

        [JsonProperty("permalink")]
        public string PermanentLink
        {
            set
            {
                FullLink = @"https://www.reddit.com" + value;
            }
        }
    }
}
