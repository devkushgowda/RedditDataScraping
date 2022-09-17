using System;
using System.Web;

public static class UrlExtension
{
    public static string AddSearchKeyQueryParameter(this string url, string key)
    {
        return url.AddQueryParameter("q", key);
    }

    public static string AddSizeQueryParameter(this string url, string size)
    {
        return url.AddQueryParameter("size", size);
    }

    public static string AddSubRedditQueryParameter(this string url, string subreddit)
    {
        return url.AddQueryParameter("subreddit", subreddit);
    }

    public static string AddQueryParameter(this string url,string paramName, string value)
    {
        var q = $"{paramName}={value}";
        return url += url.EndsWith("?") ? q : "&" + q;
    }
}
