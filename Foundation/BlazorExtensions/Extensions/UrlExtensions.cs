﻿using Microsoft.AspNetCore.Components;
using System;


namespace Foundation.BlazorExtensions.Extensions
{

    public static class UrlExtensions
    {

        public static string AddBaseUrl(this string url, NavigationManager navigationManager)
        {

            if (!string.IsNullOrWhiteSpace(url) && url.StartsWith("/"))
                url = url.Substring(1);

            return new Uri(new Uri(navigationManager.BaseUri), url).ToString();
        }

        //TODO Not sure where to put this one...
        public static bool HasRouteError(this string url)
        {
            return !string.IsNullOrWhiteSpace(url) && url.Contains(Constants.Route.RouteError);
        }

        public static string RemoveFilePrefix(this string url)
        {

            url = url.Replace(Constants.UrlFixes.FilePrefix, "");

            return url;
        }

        //public static string GetLanguageSegment(this string url, string baseUrl)
        //{

        //    Uri uri = new Uri(new Uri(baseUrl), url);

        //    string segment = uri?.Segments?.ElementAt(1);

        //    if (string.IsNullOrWhiteSpace(segment))
        //        return false;

        //    url = url.Replace(Constants.UrlFixes.FilePrefix, "");

        //    return url;
        //}


    }
}
