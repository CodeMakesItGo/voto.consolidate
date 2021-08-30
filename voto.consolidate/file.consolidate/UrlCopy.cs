using file.consolidate;
using System;
using System.IO;
using System.Net;

namespace File.Consolidate
{
    public class UrlCopy : FileBase
    {

        public UrlCopy(int totalFiles)
        {
            ProgressReport.Instance.Total += totalFiles;
        }

        public bool OverwriteFlag { get; set; }


        public void SyncronousDownload(string source, string cookies, string fileName, long fileSize, DateTime fileTime)
        {
            if (Directory.Exists(DestinationDirectory) == false)
            {
                ProgressReport.Instance.Report = "Destination does not exist.";
                return;
            }

            if (string.IsNullOrEmpty(source)) return;

            try
            {
                double TotalBps = 0;

                ProgressReport.Instance.FileName = fileName;
                var size = fileSize;

                //Based on creation time, not LastWrite time
                if (new TimeSpan(DateTime.Now.Ticks - fileTime.Ticks).TotalDays < DaysOld)
                {
                    ProgressReport.Instance.Skipped++;
                    return;
                }

                var FullPath = CreateDestinationPath(DestinationDirectory, fileName, fileTime);

                try
                {
                    if (OverwriteFlag || System.IO.File.Exists(FullPath) == false)
                    {
                        var start = DateTime.Now;

                        using (var wc = new WebClient())
                        {
                            wc.Headers.Add("Cookie: " + cookies);
                            wc.DownloadFile(new Uri(source), FullPath);
                        }

                        TotalBps += size / new TimeSpan(DateTime.Now.Ticks - start.Ticks).TotalSeconds;

                        ProgressReport.Instance.Successful++;
                        ProgressReport.Instance.TotalBytes += size;
                        ProgressReport.Instance.Bps = TotalBps / ProgressReport.Instance.Successful;
                        ProgressReport.Instance.Report = "Successful";
                    }
                    else
                    {
                        ProgressReport.Instance.Skipped++;
                        ProgressReport.Instance.Report = "Skipped";
                    }
                }

                catch (Exception e)
                {
                    ProgressReport.Instance.Failed++;
                    ProgressReport.Instance.Report = $"Failed: {e.Message}";
                }

            }
            catch (Exception e)
            {
                ProgressReport.Instance.Report = e.Message;
            }
        }
    }
}