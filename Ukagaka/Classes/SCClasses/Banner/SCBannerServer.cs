using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Windows.Media.Imaging;
using System.Security.Cryptography.X509Certificates;
namespace Ukagaka
{


    /// <summary>
    /// C# 版 Banner 下载管理器
    /// </summary>
    public class SCBannerServer
    {
        private const int MAX_CONNECTIONS = 3; // 同时最大连接数

        private readonly string bannerDir;
        private readonly List<string> waitingQueue;                // 等待队列
        private readonly List<SCBannerDownloader> downloadingQueue; // 下载中队列
        private readonly List<SavingQueueElement> saveWaitingQueue; // 保存等待队列

        private volatile bool flagTerm = false;
        private Thread thread;

        public SCBannerServer()
        {
            bannerDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "home", "_system", "banner");
            Directory.CreateDirectory(bannerDir);

            waitingQueue = new List<string>();
            downloadingQueue = new List<SCBannerDownloader>();
            saveWaitingQueue = new List<SavingQueueElement>();

            ConvertOldType();
        }

        public void Start()
        {
            if (thread == null)
            {
                flagTerm = false;
                thread = new Thread(Run) { IsBackground = true };
                thread.Start();
            }
        }

        private void Run()
        {
            while (!flagTerm)
            {
                // 保存等待队列的数据
                lock (saveWaitingQueue)
                {
                    foreach (var elem in saveWaitingQueue)
                    {
                        _SaveToDisk(elem.Url, elem.DataStream);
                    }
                    saveWaitingQueue.Clear();
                }

                // 清理已结束的下载线程
                lock (downloadingQueue)
                {
                    for (int i = 0; i < downloadingQueue.Count; i++)
                    {
                        if (!downloadingQueue[i].IsAlive())
                        {
                            downloadingQueue.RemoveAt(i);
                            i--;
                        }
                    }
                }

                // 启动新的下载线程
                while (downloadingQueue.Count < MAX_CONNECTIONS && waitingQueue.Count > 0)
                {
                    string url;
                    lock (waitingQueue)
                    {
                        url = waitingQueue[0];
                        waitingQueue.RemoveAt(0);
                    }

                    var downloader = new SCBannerDownloader(this, url);
                    downloader.Start();
                    downloadingQueue.Add(downloader);
                }

                try
                {
                    Thread.Sleep(30 * 1000);
                }
                catch (ThreadInterruptedException)
                {
                    continue;
                }
            }
        }


        public BitmapImage GetBanner(string bannerUrl)
        {
            if (string.IsNullOrEmpty(bannerUrl))
            { 
                return null;
            }
            if (!bannerUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            var validExtensions = new[] { ".png", ".gif", ".jpg" };
            if (!validExtensions.Any(ext => bannerUrl.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
            {
                return null;
            }

            // Check if already downloaded
            string filename = SCMiscUtils.GetMD5AsString(bannerUrl);
            var bannerFile = new FileInfo(Path.Combine(bannerDir, filename));

            if (bannerFile.Exists)
            {
                try
                {
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = new Uri(bannerFile.FullName);
                    image.EndInit();
                    image.Freeze(); // Make it cross-thread safe
                    return image;
                }
                catch
                {
                    // Corrupted image file, delete it
                    bannerFile.Delete();
                }
            }

            if (thread == null || !thread.IsAlive)
                return null;

            lock (waitingQueue)
            {
                if (waitingQueue.Contains(bannerUrl))
                    return null;
            }

            lock (downloadingQueue)
            {
                if (downloadingQueue.Any(d => d.GetBannerUrl() == bannerUrl))
                    return null;
            }

            // 加入等待队列
            lock (waitingQueue)
            {
                waitingQueue.Add(bannerUrl);
            }

            thread.Interrupt();
            return null;
        }



        /// <summary>
        /// 获取 Banner 图像文件，如果不存在则加入下载队列。
        /// 返回文件路径或 null。
        /// </summary>
        public string GetBannerPath(string bannerUrl)
        {
            if (string.IsNullOrEmpty(bannerUrl) ||
                !(bannerUrl.StartsWith("http://") || bannerUrl.StartsWith("https://")))
                return null;

            if (!(bannerUrl.EndsWith(".png") || bannerUrl.EndsWith(".gif") || bannerUrl.EndsWith(".jpg")))
                return null;

            string bannerFile = Path.Combine(bannerDir, SCMiscUtils.GetMD5AsString(bannerUrl));
            if (File.Exists(bannerFile))
            {
                return bannerFile; // 已经存在
            }

            if (thread == null || !thread.IsAlive)
                return null;

            lock (waitingQueue)
            {
                if (waitingQueue.Contains(bannerUrl))
                    return null;
            }

            lock (downloadingQueue)
            {
                if (downloadingQueue.Any(d => d.GetBannerUrl() == bannerUrl))
                    return null;
            }

            // 加入等待队列
            lock (waitingQueue)
            {
                waitingQueue.Add(bannerUrl);
            }

            thread.Interrupt();
            return null;
        }

        public void Close()
        {
            if (thread == null || !thread.IsAlive) return;

            flagTerm = true;
            thread.Interrupt();
            thread.Join();
        }

        public void SaveToDisk(string url, Stream dataStream)
        {
            lock (saveWaitingQueue)
            {
                saveWaitingQueue.Add(new SavingQueueElement(url, dataStream));
            }
            thread.Interrupt();
        }

        private void _SaveToDisk(string url, Stream dataStream)
        {
            string outFile = Path.Combine(bannerDir, SCMiscUtils.GetMD5AsString(url));
            try
            {
                using (var fs = new FileStream(outFile, FileMode.Create, FileAccess.Write))
                {
                    dataStream.CopyTo(fs);
                }
            }
            catch { }
            finally
            {
                dataStream.Dispose();
            }
        }

        private void ConvertOldType()
        {
            string idxFile = Path.Combine(bannerDir, "index");
            if (!File.Exists(idxFile)) return;

            try
            {
                using (var sr = new StreamReader(idxFile))
                {
                    while (!sr.EndOfStream)
                    {
                        string url = null, filename = null;
                        while (true)
                        {
                            string line = sr.ReadLine();
                            if (line == null) break;
                            if (line.Length == 0 || !line.Contains("=")) break;

                            var parts = line.Split('=');
                            if (parts.Length < 2) continue;

                            string key = parts[0].Trim();
                            string value = parts[1].Trim();

                            if (key == "url")
                                url = value;
                            else if (key == "filename")
                                filename = value;
                        }

                        if (url != null && filename != null)
                        {
                            string before = Path.Combine(bannerDir, filename);
                            if (!File.Exists(before)) continue;

                            string after = Path.Combine(bannerDir, SCMiscUtils.GetMD5AsString(url));
                            File.Move(before, after);
                        }
                    }
                }
                File.Delete(idxFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SCBannerServer : ConvertOldType() caused an exception.");
                Console.WriteLine(ex);
            }

           
        }

        public void Interrupt()
        {
            Close();
        }


        private class SavingQueueElement
        {
            public string Url { get; }
            public Stream DataStream { get; }

            public SavingQueueElement(string url, Stream dataStream)
            {
                Url = url;
                DataStream = dataStream;
            }
        }
    }
}