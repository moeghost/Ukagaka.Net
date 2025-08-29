using System.Collections.Generic;

namespace Ukagaka
{
    public class SCHLSensorResponse
    {
        public string StatusMessage { get; }
        public Dictionary<string, string> Headers { get; }
        public string Content { get; }

        public SCHLSensorResponse(string statusMessage, Dictionary<string, string> headers, string content)
        {
            StatusMessage = statusMessage;
            Headers = headers ?? new Dictionary<string, string>();
            Content = content;
        }

        public string GetStatusMessage()
        {
            return StatusMessage;
        }


        public string GetHeader(string key)
        {
            return Headers.TryGetValue(key, out var value) ? value : null;
        }

        public string GetContent()
        {
            return Content;
        }
    }
}