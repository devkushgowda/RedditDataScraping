using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RedditDataScraping.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace RedditDataScraping
{
    internal static class Program
    {
        public static List<Comment> Comments = new List<Comment>();
        public static List<Submission> Submissions = new List<Submission>();

        static void Main(string[] args)
        {
            //Test();
            var config = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText("configuration.json"));
            var parser = new SubmissionParser();
            parser.GetBySubReddit(config.SubReddits, Submissions, Comments);
            parser.GetByTopic(config.Topics, Submissions, Comments);
            Console.WriteLine($"Total Submissions: {Submissions.Count}\nTotal Comments: {Comments.Count}");
            Save("comments.json", Comments);
            Save("submissions.json", Submissions);
            Console.ReadKey();
        }

        private static void Test()
        {
            var comments = JsonConvert.DeserializeObject<List<Comment>>(File.ReadAllText("comments.json"));
            var sumbissions = JsonConvert.DeserializeObject<List<Submission>>(File.ReadAllText("submissions.json"));
            Console.WriteLine($"Submissions: {sumbissions.Count}\nComments: {comments.Count}");
            comments.ForEach(x =>
            {
                x.CreatedAt = x.CreatedAt.ToUniversalTime();
            });
            sumbissions.ForEach(x =>
            {
                x.CreatedAt = x.CreatedAt.ToUniversalTime();
            });
            Save("comments_indented.json", comments);
            Save("submissions_indented.json", sumbissions);

        }
        private static void Save(string path, object obj, Formatting f = Formatting.Indented)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(obj, f, new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-dd'T'HH:mm:ssZ" }));
        }
    }
}
