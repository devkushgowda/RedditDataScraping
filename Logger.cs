using RedditDataScraping;
using System;

public static class Logger
{
    public static void Log(string message)
    {
        Console.WriteLine($"{DateTime.Now}\t{Program.Submissions.Count}\t{Program.Comments.Count}\t{message}");
    }
}
