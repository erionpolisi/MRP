using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using MRP.System;
using Xunit.Sdk;

namespace MRP.Server.Ext
{
    public static class HttpRestEventArgsExt
    {
        private static string _currentHandler = string.Empty;

        public static void SetCurrentHandler(this HttpRestEventArgs e, string currentHandler) => _currentHandler = currentHandler;

        public static bool VerifySession(this HttpRestEventArgs e)
        {
            if (e.Session is not null) 
                return true; 
            
            e.RespondForbidden("No valid session.");
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

        public static bool EnsureAccess(this HttpRestEventArgs e, string userId, string errorMessage, out Guid guid)
        {
            guid = Guid.Empty;

            // 1. Validate GUID format
            if (!Guid.TryParse(userId, out guid))
            {
                e.RespondBadRequest("Invalid userId format.");
                return false;
            }

            // 2. Validate existing session
            if (e.Session is null)
            {
                e.RespondUnauthorized();
                return false;
            }

            // 3. Check access rights
            if (!e.Session.CanAccessUser(guid))
            {
                e.RespondForbidden(errorMessage);
                return false;
            }

            return true;
        }
        public static string? GetQuery(this HttpListenerRequest request, string key, string? defaultValue = null)
        {
            var map = request.ParseQuery();
            return map.GetValueOrDefault(key.ToLowerInvariant(), defaultValue);
        }

        // ----------------------------------------------------------
        //                  RESPONSE HELPERS
        // ----------------------------------------------------------

        //console
        public static void ConsoleResponse(this HttpRestEventArgs e, bool success, string message, Exception? ex = null)
        {
            if (success)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }

            Console.WriteLine($"[{_currentHandler} {message} {e.Method} {e.Path} {ex?.Message}]");
        }

        //with json object//
        public static void RespondOk(this HttpRestEventArgs e, JsonObject jsonObject)
        {
            e.Respond(HttpStatusCode.OK, jsonObject);
            e.ConsoleResponse(true, "Handled.");
        }

        public static void RespondCreated(this HttpRestEventArgs e, JsonObject jsonObject)
        {
            e.Respond(HttpStatusCode.Created, jsonObject);
            e.ConsoleResponse(true, "Handled, content added.");
        }
        
        public static void RespondAccepted(this HttpRestEventArgs e, JsonObject jsonObject)
        {
            e.Respond(HttpStatusCode.Accepted, jsonObject);
            e.ConsoleResponse(true, "Handled, content edited.");
        }

        //with message//
        public static void RespondForbidden(this HttpRestEventArgs e, string message)
        {
            e.Respond(HttpStatusCode.Forbidden, new JsonObject
            {
                ["success"] = false,
                ["reason"] = message
            });
            e.ConsoleResponse(false, message);
        }

        public static void RespondConflict(this HttpRestEventArgs e, string message)
        {
            e.Respond(HttpStatusCode.Conflict, new JsonObject
            {
                ["success"] = false,
                ["reason"] = message
            });
            e.ConsoleResponse(false, message);
        }

        public static void RespondBadRequest(this HttpRestEventArgs e, string message)
        {
            e.Respond(HttpStatusCode.BadRequest, new JsonObject
            {
                ["success"] = false,
                ["reason"] = message
            });
            e.ConsoleResponse(false, message);
        }
        public static void RespondNotFound(this HttpRestEventArgs e, string message)
        {
            e.Respond(HttpStatusCode.NotFound, new JsonObject
            {
                ["success"] = false,
                ["reason"] = message
            });
            e.ConsoleResponse(false, message);
        }

        public static void RespondInternalServerError(this HttpRestEventArgs e, Exception ex)
        {
            e.Respond(HttpStatusCode.InternalServerError, new JsonObject
            {
                ["success"] = false,
                ["reason"] = ex.Message
            });
            e.ConsoleResponse(false, "Error occured in Server", ex);
        }

        //general responses//
        public static void RespondNoContent(this HttpRestEventArgs e)
        {
            e.Respond(HttpStatusCode.NoContent, new JsonObject());
            e.ConsoleResponse(true, "Handled, no content.");
        }

        public static void RespondUnauthorized(this HttpRestEventArgs e)
        {
            e.Respond(HttpStatusCode.Unauthorized, new JsonObject
            {
                ["success"] = false,
                ["reason"] = "Authentication required."
            });
            e.ConsoleResponse(false, "Authentication required.");
        }

        public static void RespondInvalidEndpoint(this HttpRestEventArgs e)
        {
            e.Respond(HttpStatusCode.BadRequest, new JsonObject
            {
                ["success"] = false,
                ["reason"] = "Invalid endpoint."
            });
            e.ConsoleResponse(false, "Invalid endpoint.");
        }
    }
}