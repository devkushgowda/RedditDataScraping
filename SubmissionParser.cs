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

        private const int MaxRecords = 500;

        public void GetBySubReddit(List<Entry> subreddits, List<Submission> submissionsList, List<Comment> commentList)
        {
            subreddits.ForEach(
                subreddit =>
                {
                    List<Submission> curResult = new List<Submission>();
                    Logger.Log($"Collecting data for subreddit: {subreddit.Value}, target: {subreddit.Count} entries.");
                    var url = BaseUrl.AddSubRedditQueryParameter(subreddit.Value);
                    if (!subreddit.CanBeEmpty)
                    {
                        url = url.AddQueryParameter("selftext", "I");
                    }
                    GetSubmission(url, subreddit.Count, curResult);
                    //Remove duplicates
                    curResult = curResult.Where(x => !submissionsList.Any(y => y.Id == x.Id)).ToList();
                    submissionsList.AddRange(curResult);
                    curResult.ForEach(x =>
                    {
                        CommentsParser.GetComments(x.Id, x.Selftext, commentList);
                        x.Topic = subreddit.Value;
                    });
                    Logger.Log($"Collected {curResult.Count} entries for subreddit: {subreddit.Value}");
                });
        }

        public void GetByTopic(List<Entry> topics, List<Submission> submissionsList, List<Comment> commentList)
        {
            topics.ForEach(
                q =>
                {

                    List<Submission> curResult = new List<Submission>();
                    Logger.Log($"Collecting data for search-key: {q.Value}, target: {q.Count} entries.");
                    var url = BaseUrl.AddSearchKeyQueryParameter(q.Value);
                    if (!q.CanBeEmpty)
                    {
                        url = url.AddQueryParameter("selftext", "I");
                    }
                    GetSubmission(url, q.Count, curResult, false);
                    submissionsList.AddRange(curResult);
                    curResult.ForEach(x =>
                    {
                        CommentsParser.GetComments(x.Id, x.Selftext, commentList);
                        x.Topic = q.Topic;
                    });
                    Logger.Log($"Collected {curResult.Count} entries for search-key: {q.Value}");
                });
        }

        private void GetSubmission(string url, uint size, List<Submission> submissionsList, bool allowEmpty = true)
        {
            url = url.AddQueryParameter("selftext:not", "[removed]");
            //To filter out empty self text.
            url = url.AddSizeQueryParameter(MaxRecords.ToString());
            List<Submission> curRes = new List<Submission>();
            const int T = 30;
            const int MaxTries = 50;
            int i = T;
            //Get up to one week old data
            while (i <= MaxTries * T && curRes.Count < size)
            {
                var curURL = url;
                curURL = curURL.AddQueryParameter("fields", RequiredFields);
                curURL = curURL.AddQueryParameter("after", i.ToString() + "d");
                curURL = curURL.AddQueryParameter("before", (i - T).ToString() + "d");
                var responseStr = GetRequest(curURL);
                var data = JsonConvert.DeserializeObject<RedditResponse<Submission>>(responseStr).Data;
                var res = data.Where(x => IsValid(x.Selftext, allowEmpty)).ToList();
                var insertCount = res.Count + curRes.Count <= size ? res.Count : size - curRes.Count;
                res = res.GetRange(0, (int)insertCount);
                curRes.AddRange(res);
                submissionsList.AddRange(res);
                Logger.Log($"Extracted {res.Count} entries...");
                i += T;
                Thread.Sleep(1000);
            }
        }

        private bool IsValid(string selftext, bool allowEmpty)
        {
            if (selftext == null)
                return false;
            if (!allowEmpty && string.IsNullOrEmpty(selftext))
                return false;
            var removedMessage = "your submission has been removed for the following reason";
            return !selftext.Equals("[deleted]") && !selftext.Contains(removedMessage);
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
