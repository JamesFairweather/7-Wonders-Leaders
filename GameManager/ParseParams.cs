using System;
using System.Collections.Generic;

namespace SevenWonders
{
    public static class UriExtensions
    {
        private static readonly System.Text.RegularExpressions.Regex _regex =
            new System.Text.RegularExpressions.Regex(@"[?|&]([^?|^&|^=]+)=([^?|^&]+)");

        // Similar to HttpUtility.ParseQueryString, which returns a NameValueCollection, but this
        // version allows for non-unique names (a 7 Wonders hand can have two or even three of the
        // same card) and it allows spaces in the Names.
        public static IList<KeyValuePair<string, string>> ParseQueryString(string s)
        {
            // var match = _regex.Match(uri.PathAndQuery);
            var match = _regex.Match(s);
            var parameters = new List<KeyValuePair<string, string>>();

            while (match.Success)
            {
                parameters.Add(new KeyValuePair<string, string>(match.Groups[1].Value, match.Groups[2].Value));
                match = match.NextMatch();
            }

            return parameters;
        }
    }
}
