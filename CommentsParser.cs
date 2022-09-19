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
    public static class CommentsParser
    {
        public const string BaseUrl = @"https://api.pushshift.io/reddit/search/comment/?q=*";
        public const string RequiredFields = "id,subreddit,permalink,body,author,parent_id,created_utc";

        public static void GetComments(string id, string parentBody, List<Comment> commentList, int size = 30)
        {
            var curURL = BaseUrl.AddQueryParameter("link_id", id);
            curURL = curURL.AddQueryParameter("fields", RequiredFields);
            //Get 5 times the size so that filtering will not reduce the output
            curURL = curURL.AddSizeQueryParameter((size * 5).ToString());
            var responseStr = GetRequest(curURL);
            var data = JsonConvert.DeserializeObject<RedditResponse<Comment>>(responseStr).Data;
            var res = data.Where(x => IsValid(x.Body)).ToList();
            //Limit comments to size
            res = res.GetRange(0, res.Count < size ? res.Count : size);
            //Remove duplicates.
            res = res.Where(x => !commentList.Any(y => y.Id == x.Id)).ToList();
            Logger.Log($"Extracted {res.Count} comments...");
            res.ForEach(x => { x.ParentBody = parentBody; });
            commentList.AddRange(res);
            Thread.Sleep(1000);
        }

        private static bool IsValid(string body)
        {
            if (body == null)
                return false;
            body = body.ToLower().Trim();
            var removedMessage = "your submission has been removed for the following reason";
            return !string.IsNullOrEmpty(body) && !body.Equals("[removed]") && !body.Equals("[deleted]") && !body.Contains(removedMessage);

        }

        private static string GetRequest(string uri)
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
