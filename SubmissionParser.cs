using Newtonsoft.Json;
using RedditDataScraping.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace RedditDataScraping
{
    public class SubmissionParser
    {
        public const string BaseUrl = @"https://api.pushshift.io/reddit/search/submission?";
        public const string RequiredFields = "id,subreddit,full_link,title,selftext,author,is_submission,topic,created_utc";

        public void GetBySubReddit(List<Entry> subreddits, List<Submission> submissions, List<Comment> commentList)
        {
            subreddits.ForEach(
                subreddit =>
                {
                    List<Submission> curResult = new List<Submission>();
                    Logger.Log($"Collecting data for subreddit: {subreddit.Name}, target: {subreddit.Count} entries.");
                    var url = BaseUrl.AddSubRedditQueryParameter(subreddit.Name);
                    if (subreddit.Count != 0)
                        url = url.AddSizeQueryParameter(subreddit.Count.ToString());
                    GetSubmission(url, subreddit.Count, curResult);
                    submissions.AddRange(curResult);
                    curResult.ForEach(x =>
                    {
                        CommentsParser.GetComments(x.Id, x.Selftext, commentList);
                        x.Topic = subreddit.Name;
                    });
                    Logger.Log($"Collected {curResult.Count} entries for subreddit: {subreddit.Name}");
                });
        }

        public void GetByTopic(List<Entry> topics, List<Submission> submissions, List<Comment> commentList)
        {
            topics.ForEach(
                q =>
                {
                    List<Submission> curResult = new List<Submission>();
                    Logger.Log($"Collecting data for search-key: {q.Name}, target: {q.Count} entries.");
                    var url = BaseUrl.AddSearchKeyQueryParameter(q.Name);
                    if (q.Count != 0)
                        url = url.AddSizeQueryParameter(q.Count.ToString());
                    GetSubmission(url, q.Count, curResult);
                    submissions.AddRange(curResult);
                    curResult.ForEach(x =>
                    {
                        CommentsParser.GetComments(x.Id, x.Selftext, commentList);
                        x.Topic = q.Name;
                    });
                    Logger.Log($"Collected {curResult.Count} entries for search-key: {q.Name}");
                });
        }

        private void GetSubmission(string url, uint size, List<Submission> submissions)
        {
            List<Submission> curRes = new List<Submission>();
            const int T = 90;
            int i = T;
            //Get up to one week old data
            while (i <= 12 * T && curRes.Count < size)
            {
                var curURL = url;
                curURL = curURL.AddQueryParameter("fields", RequiredFields);
                curURL = curURL.AddQueryParameter("after", i.ToString() + "d");
                curURL = curURL.AddQueryParameter("before", (i - T).ToString() + "d");
                var responseStr = GetRequest(curURL);
                var data = JsonConvert.DeserializeObject<RedditResponse<Submission>>(responseStr).Data;
                var res = data.Where(x => IsValid(x.Selftext)).ToList();
                curRes.AddRange(res);
                submissions.AddRange(res);
                Logger.Log($"Extracted {res.Count} entries...");
                i += T;
                Thread.Sleep(1000);
            }
        }

        private bool IsValid(string selftext)
        {
            if (selftext == null)
                return false;
            var removedMessage = "your submission has been removed for the following reason";
            return !selftext.Equals("[removed]") && !selftext.Equals("[deleted]") && !selftext.Contains(removedMessage);
        }

        private string GetRequest(string uri)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Logger.Log("Error: " + e.Message);
            }
            return "{\"data\":[]}";
        }
    }
}
