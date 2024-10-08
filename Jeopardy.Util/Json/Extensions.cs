﻿using Newtonsoft.Json;

namespace Jeopardy.Util.Json
{
    public static class Extensions
    {
        /// <summary>
        /// Ensures that a JSON string is formatted with indentation.
        /// </summary>
        /// <param name="json">The JSON to format.</param>
        /// <returns>The JSON formatted with indentation.</returns>
        public static string FormatJson(this string json)
        {
            object? parsedJson = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented);
        }
    }
}
