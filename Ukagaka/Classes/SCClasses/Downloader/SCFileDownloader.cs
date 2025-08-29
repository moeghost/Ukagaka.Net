using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Ukagaka
{

    public interface ISCStatusWindowController
    {
        void PbArtype_SetVal(double val);     // 进度值 0.0~1.0，-1表示未知大小
        void PbArtype_SetText2(string text);  // 显示的文本
    }

    public class SCTaraimawasiException : Exception { }
    public class SCCannotDownloadException : Exception
    {
        public int StatusCode { get; }
        public SCCannotDownloadException(int code) : base($"Cannot download. HTTP Status: {code}")
        {
            StatusCode = code;
        }
    }

    public static class SCFileDownloader
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task DownloadAsync(string urlStr, string outFilePath, int timeoutMs, ISCStatusWindowController prog = null)
        {
            _httpClient.Timeout = TimeSpan.FromMilliseconds(timeoutMs);

            Uri currentUri = new Uri(urlStr);
            HttpResponseMessage response = null;
            int redirectCount = 0;

            while (redirectCount < 10)
            {
                response = await _httpClient.GetAsync(currentUri, HttpCompletionOption.ResponseHeadersRead);
                if (response.StatusCode == System.Net.HttpStatusCode.Redirect ||
                    response.StatusCode == System.Net.HttpStatusCode.MovedPermanently ||
                    response.StatusCode == System.Net.HttpStatusCode.TemporaryRedirect ||
                    (int)response.StatusCode == 308) // Permanent Redirect
                {
                    if (response.Headers.Location != null)
                    {
                        currentUri = new Uri(currentUri, response.Headers.Location);
                        redirectCount++;
                        response.Dispose();
                        continue;
                    }
                }
                break;
            }

            if (redirectCount >= 10)
            {
                throw new SCTaraimawasiException();
            }

            if (!response.IsSuccessStatusCode)
            {
                response.Dispose();
                throw new SCCannotDownloadException((int)response.StatusCode);
            }

            long? contentLength = response.Content.Headers.ContentLength;

            if (prog != null && !contentLength.HasValue)
            {
                prog.PbArtype_SetVal(-1.0);
                prog.PbArtype_SetText2("Content-Length not given.");
            }

            using (var contentStream = await response.Content.ReadAsStreamAsync())
            using (var fileStream = new FileStream(outFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                byte[] buffer = new byte[1024];
                long totalRead = 0;
                int read;
                int posPercentage = 0;
                int posKilobytes = 0;

                while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, read);
                    totalRead += read;

                    if (prog != null)
                    {
                        if (contentLength.HasValue)
                        {
                            double pos = (double)totalRead / contentLength.Value;
                            int kbDownloaded = (int)(totalRead / 1024);

                            if (posPercentage + 3 <= (int)(pos * 100)) // 每3%更新
                            {
                                posPercentage = (int)(pos * 100);
                                prog.PbArtype_SetVal(pos);
                                prog.PbArtype_SetText2($"[{totalRead}/{contentLength}] {posPercentage}%");
                            }
                        }
                        else
                        {
                            int kbDownloaded = (int)(totalRead / 1024);
                            if (posKilobytes + 30 <= kbDownloaded) // 每30KB更新
                            {
                                posKilobytes = kbDownloaded;
                                prog.PbArtype_SetText2($"{posKilobytes}kb");
                            }
                        }
                    }
                }

                if (prog != null)
                {
                    prog.PbArtype_SetVal(1.0);
                    if (contentLength.HasValue)
                    {
                        prog.PbArtype_SetText2($"[{totalRead}/{contentLength}] 100%");
                    }
                    else
                    {
                        prog.PbArtype_SetText2($"{totalRead / 1024}kb");
                    }
                }
            }

            response.Dispose();
        }
    }
}
