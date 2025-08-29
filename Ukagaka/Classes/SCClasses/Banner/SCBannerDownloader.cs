using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ukagaka
{
    public class SCBannerDownloader
    {
        private const bool DEBUG = false;
        private const int TIMEOUT_CONNECTING = 15 * 1000; // 15 seconds

        private volatile bool flagTerm = false;
        private readonly string bannerUrl;
        private readonly SCBannerServer bserver;
        private Thread thread;

        public SCBannerDownloader(SCBannerServer bserver, string url)
        {
            this.bserver = bserver;
            bannerUrl = url;
        }

        public void Start()
        {
            if (thread == null)
            {
                thread = new Thread(Run);
                thread.Start();
            }
        }

        private async void Run()
        {
            if (!bannerUrl.StartsWith("http://") && !bannerUrl.StartsWith("https://"))
                return;

            try
            {
                debugmsg("Trying to establish a connection...");

                using (HttpClientHandler handler = new HttpClientHandler()
                {
                    AllowAutoRedirect = true, // 允许重定向
                    MaxAutomaticRedirections = 10
                })
                using (HttpClient client = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromMilliseconds(TIMEOUT_CONNECTING)
                })
                {
                    HttpResponseMessage response = await client.GetAsync(bannerUrl, HttpCompletionOption.ResponseHeadersRead);

                    debugmsg($"Connection established. Response code was {(int)response.StatusCode}");

                    if (!response.IsSuccessStatusCode)
                    {
                        return; // 非200退出
                    }

                    // 读取全部内容到内存
                    using (MemoryStream ms = new MemoryStream())
                    using (Stream stream = await response.Content.ReadAsStreamAsync())
                    {
                        byte[] buffer = new byte[1024];
                        int bytesRead;

                        while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            if (flagTerm)
                            {
                                return; // 强制退出
                            }
                            ms.Write(buffer, 0, bytesRead);
                        }

                        debugmsg($"Finished downloading. Total size is {ms.Length} bytes.");

                        // 保存到磁盘
                        ms.Position = 0;
                        bserver.SaveToDisk(bannerUrl, ms);
                        debugmsg("Saved to disk.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            bserver.Interrupt(); // 通知服务器任务已完成
        }

        public string GetBannerUrl()
        {
            return bannerUrl;
        }

        public void Terminate()
        {
            if (thread == null || !thread.IsAlive) return;

            flagTerm = true;
            thread.Interrupt();
            thread.Join();
        }

        public bool IsAlive()
        {

            return thread != null && thread.IsAlive;

        }

        private void debugmsg(string msg)
        {
            if (DEBUG)
            {
                Console.WriteLine($"{bannerUrl} : {msg}");
            }
        }
    }
}
