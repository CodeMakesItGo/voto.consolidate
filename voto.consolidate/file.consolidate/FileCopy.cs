using file.consolidate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace File.Consolidate
{
    public class FileCopy : FileBase
    {
        public bool OverwriteFlag { get; set; }


        public void SyncronousCopy()
        {
            CopyDirectory(SourceDirectory, DestinationDirectory, SubDirectoriesFlag, OverwriteFlag);
        }

        public async void AsyncronousCopy()
        {
            await Task.Run(() => CopyDirectory(SourceDirectory, DestinationDirectory, SubDirectoriesFlag,
                OverwriteFlag));
        }

        private void CopyDirectory(string source, string destination, bool subdirectories = true, bool overwrite = false)
        {
            if (Directory.Exists(destination) == false)
            {
                ProgressReport.Instance.Report = "Destination does not exist.";
                return;
            }

            var files = GetFileList(source, subdirectories);

            if (files == null)
            {
                ProgressReport.Instance.Report = "No files exist with selected extensions.";
                return;
            }
            CopyFiles(files, destination, overwrite);
        }


        private void CopyFiles(List<string> fileList, string destination, bool overwrite = false)
        {
            Cancel = false;

            if (fileList == null) return;

            try
            {
                double TotalBps = 0;

                ProgressReport.Instance.Total += fileList.Count;
                
                foreach (var file in fileList)
                {
                    if (Cancel) break;

                    if (System.IO.File.Exists(file) == false) continue;

                    ProgressReport.Instance.FileName = Path.GetFileName(file);
                    var size = new FileInfo(file).Length;

                    //Based on creation time, not LastWrite time
                    if (new TimeSpan(DateTime.Now.Ticks - new FileInfo(file).CreationTime.Ticks).TotalDays < DaysOld)
                    {
                        ProgressReport.Instance.Skipped++;
                        continue;
                    }


                    var fullPath = CreateDestinationPath(destination, file);

                    try
                    {
                        if (overwrite || System.IO.File.Exists(fullPath) == false)
                        {
                            var start = DateTime.Now;

                            System.IO.File.Copy(file, fullPath, overwrite);

                            TotalBps += size / new TimeSpan(DateTime.Now.Ticks - start.Ticks).TotalSeconds;

                            ProgressReport.Instance.Successful++;
                            ProgressReport.Instance.TotalBytes += size;
                            ProgressReport.Instance.Bps = TotalBps / ProgressReport.Instance.Successful;
                            ProgressReport.Instance.Report = "Successful";
                        }
                        else
                        {
                            ProgressReport.Instance.Skipped++;
                            ProgressReport.Instance.Report = "Skipped " + ProgressReport.Instance.FileName;
                        }
                    }

                    catch (Exception e)
                    {
                        ProgressReport.Instance.Failed++;
                        ProgressReport.Instance.Report = $"Failed: {e.Message}";
                    }
                }
            }
            catch (Exception e)
            {
                ProgressReport.Instance.Report = e.Message;
            }
        }

        public bool SkipCopyFile(string fileName, DateTime dateTime)
        {
            bool rtn = false;
            ProgressReport.Instance.FileName = fileName;

            //Based on creation time, not LastWrite time
            if (new TimeSpan(DateTime.Now.Ticks - dateTime.Ticks).TotalDays < DaysOld)
            {
                ProgressReport.Instance.Skipped++;
                ProgressReport.Instance.Report = "Skipped " + ProgressReport.Instance.FileName;
                rtn = true;
            }

            var FullPath = CreateDestinationPath(DestinationDirectory, fileName, dateTime);

            if (System.IO.File.Exists(FullPath) && OverwriteFlag == false)
            {
                ProgressReport.Instance.Skipped++;
                ProgressReport.Instance.Report = "Skipped " + ProgressReport.Instance.FileName;
                rtn = true;
            }

            return rtn;
        }

        public void CopyFile(byte[] fileBytes, string fileName, DateTime dateTime)
        {
            try
            {
                double TotalBps = 0;

                ProgressReport.Instance.Total += 1;

                if (Cancel) return;

                if (fileBytes.Length == 0) return;

                ProgressReport.Instance.FileName = fileName;
                var size = fileBytes.Length;

                //Based on creation time, not LastWrite time
                if (new TimeSpan(DateTime.Now.Ticks - dateTime.Ticks).TotalDays < DaysOld)
                {
                    ProgressReport.Instance.Skipped++;
                }

                var FullPath = CreateDestinationPath(DestinationDirectory, fileName, dateTime);

                try
                {
                    if (OverwriteFlag || System.IO.File.Exists(FullPath) == false)
                    {
                        var start = DateTime.Now;

                        System.IO.File.WriteAllBytes(FullPath, fileBytes);

                        TotalBps += size / new TimeSpan(DateTime.Now.Ticks - start.Ticks).TotalSeconds;

                        ProgressReport.Instance.Successful++;
                        ProgressReport.Instance.TotalBytes += size;
                        ProgressReport.Instance.Bps = TotalBps / ProgressReport.Instance.Successful;
                        ProgressReport.Instance.Report = "Successful";
                    }
                    else
                    {
                        ProgressReport.Instance.Skipped++;
                        ProgressReport.Instance.Report = "Skipped " + ProgressReport.Instance.FileName;
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