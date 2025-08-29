using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ukagaka 
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;

    public class SCHeadlineSession
    {
        private readonly SCSession session;
        private readonly SCHLSensorPlugin sensor;
        private Thread thread;

        public SCHeadlineSession(SCSession session, SCHLSensorPlugin sensor)
        {
            this.session = session;
            this.sensor = sensor;
        }

        public void Start()
        {
            thread = new Thread(Run);
            thread.IsBackground = true;
            thread.Start();
        }

        private void Run()
        {
            if (session.IsInPassiveMode())
            {
                return;
            }
            string basePath = SCFoundation.GetParentDirOfBundle();
            string hashDir = Path.Combine(basePath, "home/_system/headline_hash");
            string tempDir = Path.Combine(basePath, "home/_system/headline_temp");

            Directory.CreateDirectory(hashDir);
            Directory.CreateDirectory(tempDir);

            var respDescription = sensor.Request(
                $"GET Description HEADLINE/1.0\r\nSender: {SCFoundation.STRING_FOR_SENDER}\r\n\r\n"
            );
            string siteName = respDescription.GetHeader("Name");
            string dataUrl = respDescription.GetHeader("Data-Location");
            string siteUrl = respDescription.GetHeader("Site-Location");

            session.DoShioriEvent("OnHeadlinesenseBegin", new[] { siteName, siteUrl });

            string tempFile = Path.Combine(tempDir, $"head_{Guid.NewGuid():N}.tmp");
            try
            {
                 SCFileDownloader.DownloadAsync(dataUrl, tempFile, 30 * 1000);
            }
            catch
            {
                session.DoShioriEvent("OnHeadlinesenseFailure", new[] { "can't download" });
                if (File.Exists(tempFile)) File.Delete(tempFile);
                return;
            }

            string calculatedHash = SCFileUtils.GetMD5AsString(tempFile);

            string hashFile = Path.Combine(hashDir, SCMiscUtils.GetMD5AsString(sensor.GetDirName()));
            if (File.Exists(hashFile))
            {
                try
                {
                    string savedHash = File.ReadAllText(hashFile).Trim();
                    if (savedHash == calculatedHash)
                    {
                        session.DoShioriEvent("OnHeadlinesenseComplete", new[] { "no update" });
                        File.Delete(tempFile);
                        return;
                    }
                }
                catch { }
            }

            try
            {
                File.WriteAllText(hashFile, calculatedHash);
            }
            catch { }

            try
            {
                string reqHeader =
                    $"GET Headline HEADLINE/1.0\r\n" +
                    $"Sender: {SCFoundation.STRING_FOR_SENDER}\r\n" +
                    $"Content-Length: {new FileInfo(tempFile).Length}\r\n\r\n";

                using var ms = new MemoryStream();
                byte[] headerBytes = Encoding.UTF8.GetBytes(reqHeader);
                ms.Write(headerBytes, 0, headerBytes.Length);

                byte[] buf = new byte[512];
                using (var fs = new FileStream(tempFile, FileMode.Open, FileAccess.Read))
                {
                    int count;
                    while ((count = fs.Read(buf, 0, buf.Length)) != 0)
                    {
                        ms.Write(buf, 0, count);
                    }
                }

                ms.Position = 0;
                var headlineResp = sensor.Request(new MemoryStream(ms.ToArray()));

                if (headlineResp.GetStatusMessage().EndsWith("200 OK"))
                {
                    var sb = new StringBuilder("\\b2");
                    using var sr = new StringReader(headlineResp.GetContent());
                    string? line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        sb.Append(line).Append("\\n");
                    }

                    session.DoShioriEvent("OnHeadlinesense.OnFind", new[]
                    {
                    siteName,
                    siteUrl,
                    "First and Last",
                    sb.ToString()
                });
                }
                else
                {
                    session.DoShioriEvent("OnHeadlinesenseFailure", new[] { "can't analyze" });
                }
            }
            catch
            {
                session.DoShioriEvent("OnHeadlinesenseFailure", new[] { "can't analyze" });
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        public void Join() => thread?.Join();
    }

}
