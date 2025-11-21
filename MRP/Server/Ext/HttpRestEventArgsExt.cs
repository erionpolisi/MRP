using System.Net;
using System.Text.Json.Nodes;
using MRP.System;

namespace MRP.Server.Ext
{
    public static class HttpRestEventArgsExt
    {
        public static bool VerifyAuthentication(this HttpRestEventArgs e)
        {
            if (e.Session is not null) return true;

            e.Respond(HttpStatusCode.Unauthorized,
                new JsonObject() { ["success"] = false, ["reason"] = "Authentication required." });

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{nameof(UserHandler)} No valid session. {e.Method} {e.Path}.");
            return false;
        }

        public static IReadOnlyDictionary<string, string?> ParseQuery(this HttpListenerRequest request)
        {
            var dict = new Dictionary<string, string?>();

            string? rawQuery = request.Url?.Query;
            if (string.IsNullOrEmpty(rawQuery))
                return dict;

            // Remove leading ?
            string q = rawQuery.StartsWith("?") ? rawQuery[1..] : rawQuery;

            var pairs = q.Split('&', StringSplitOptions.RemoveEmptyEntries);

            foreach (var pair in pairs)
            {
                int idx = pair.IndexOf('=');
                string key;
                string? value;

                if (idx >= 0)
                {
                    key = pair[..idx];
                    value = pair.Length > idx + 1 ? pair[(idx + 1)..] : string.Empty;
                }
                else
                {
                    key = pair;
                    value = string.Empty;
                }

                try
                {
                    key = Uri.UnescapeDataString(key).ToLowerInvariant();
                    value = Uri.UnescapeDataString(value ?? "");
                }
                catch
                {
                    key = key.ToLowerInvariant();
                }

                if (!dict.ContainsKey(key))
                    dict[key] = value;
            }

            return dict;
        }
        public static string? GetQuery(this HttpListenerRequest request, string key, string? defaultValue = null)
        {
            var map = request.ParseQuery();
            return map.TryGetValue(key.ToLowerInvariant(), out var val) ? val : defaultValue;
        }
    }
}