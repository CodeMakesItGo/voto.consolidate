using file.consolidate;
using File.Consolidate;
using google.consolidate;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace Voto.Consolidate
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileBase _fileOperation;
        private Thread _fileOperationthread;
        private bool IsCanceling { get; set; }
        private PageConsolidate PageConsolidate { get; }
        private PageSettings PageSettings { get; }
        private PageMedia PageMedia { get; }
       

        public MainWindow()
        {
            InitializeComponent();

            PageConsolidate = new PageConsolidate();
            PageSettings = new PageSettings();
            PageMedia = new PageMedia();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Frame.Content = PageConsolidate;
            Title += $" {Assembly.GetExecutingAssembly().GetName().Version}";
        }

        private void ButtonConsolidate_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.Content is PageConsolidate)
            {
                return;
            }

            Frame.Content = PageConsolidate;
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.Content is PageSettings)
            {
                return;
            }

            Frame.Content = PageSettings;
        }

        private void ButtonMediaTypes_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.Content is PageMedia)
            {
                return;
            }

            Frame.Content = PageMedia;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Settings.Serialize();
        }

        private void ButtonRun_Click(object sender, RoutedEventArgs e)
        {
            if (_fileOperationthread == null || _fileOperationthread.IsAlive == false)
            {
                if (string.IsNullOrEmpty(Settings.Instance.DestinationRootSetting))
                {
                    MessageBox.Show(this, "Please goto settings and configure a destination directory.",
                        "No Destination Setting", MessageBoxButton.OK, MessageBoxImage.Stop);
                    return;
                }

                _fileOperationthread = Settings.Instance.CopySetting
                    ? new Thread(CopyFiles)
                    : new Thread(MoveFiles);

                ProgressReport.Instance.ClearReport();
                
                _fileOperationthread.Start();

                IsCanceling = false;

                //switch to status page
                Frame.Content = PageConsolidate;
            }
            else
            {
                FileBase.CancelAll();

                IsCanceling = true;

                var count = 0;
                while (_fileOperationthread.IsAlive && count < 20)
                {
                    Thread.Sleep(250);
                    count++;

                    buttonRun.Content = "Stopping...";
                }
            }

            buttonRun.Content = _fileOperationthread.IsAlive ? "Stop" : "Run";
        }

        private void FileOperationComplete()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new FileOperationCompleteDelegate(FileOperationComplete));
                return;
            }

            buttonRun.Content = "Run";
            PageConsolidate.Complete();
        }

        private void MoveFiles()
        {
            var extensions = BuildFileExtensionList();

            if (Settings.Instance.SourceDirectorySetting == null)
            {
                return;
            }

            foreach (var v in Settings.Instance.SourceDirectorySetting)
            {
                var source = v;
                var subdirectories = false;

                if (v[^1] == '*')
                {
                    source = v.Remove(v.Length - 1);
                    subdirectories = true;
                }

                _fileOperation = new FileMove
                {
                    DestinationDirectory = Settings.Instance.DestinationRootSetting,
                    SourceDirectory = source,
                    SubDirectoriesFlag = subdirectories,
                    StoreByPictureDate = Settings.Instance.SubfolderLastWriteDateSetting,
                    DaysOld = Settings.Instance.ConsolidationSelectionAllSetting
                        ? 0
                        : Settings.Instance.DaysOldSetting,
                    Extensions = extensions,
                    DeleteDuplicateFiles = Settings.Instance.DeleteDuplicateFiles,
                    DeleteEmptyDirectories = Settings.Instance.DeleteEmptyDirectories
                };

                ((FileMove)_fileOperation).SyncronousMove();
            }

            FileOperationComplete();
        }

        private void CopyFiles()
        {
            if (Settings.Instance.SourceDirectorySetting == null)
            {
                return;
            }

            foreach (var v in Settings.Instance.SourceDirectorySetting)
            {
                var source = v;
                var subdirectories = false;

                if (string.IsNullOrEmpty(source))
                {
                    continue;
                }

                if (v[^1] == '*')
                {
                    source = v.Remove(v.Length - 1);
                    subdirectories = true;
                }

                _fileOperation = new FileCopy
                {
                    DestinationDirectory = Settings.Instance.DestinationRootSetting,
                    SourceDirectory = source,
                    SubDirectoriesFlag = subdirectories,
                    StoreByPictureDate = Settings.Instance.SubfolderLastWriteDateSetting,
                    OverwriteFlag = Settings.Instance.OverwriteSetting,
                    DaysOld = Settings.Instance.ConsolidationSelectionAllSetting
                        ? 0
                        : Settings.Instance.DaysOldSetting,
                    Extensions = BuildFileExtensionList()
            };

                ((FileCopy)_fileOperation).SyncronousCopy();
            }

            CopyGoolgeFiles();

            
        }

        private async void CopyGoolgeFiles()
        { 
            bool gotallphotos = false;
            
            List<string> photos = new ();

            foreach (var selectedAlbum in Settings.Instance.GoogleAlbumsSetting)
            {
                if (selectedAlbum.Equals("1234")) //id for all photos
                {
                    ProgressReport.Instance.Report = "Getting list of all photos...";
                    photos.AddRange(await GooglePhotos.GetAllGooglePhotos());
                    ProgressReport.Instance.Total += photos.Count;
                    gotallphotos = true;
                }
            }

            if (gotallphotos == false)
            {
                var albums = await GooglePhotos.GetPhotoAlbums();

                if (albums.Count > 0)
                {
                    foreach (var album in albums)
                    {
                        foreach (var selectedAlbum in Settings.Instance.GoogleAlbumsSetting)
                        {
                            if (album.Id.Equals(selectedAlbum))
                            {
                                album.IsSelected = true;
                                ProgressReport.Instance.Total += album.MediaCount;
                            }
                        }

                        if (album.IsSelected == false)
                        {
                            continue;
                        }

                        photos.AddRange( await GooglePhotos.GetGooglePhotoIds(album.Id));
                    }
                }
            }

            foreach (var id in photos)
            {
                if (IsCanceling)
                {
                    break;
                }

                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }

                _fileOperation = new FileCopy()
                {
                    DestinationDirectory = Settings.Instance.DestinationRootSetting,
                    SubDirectoriesFlag = false,
                    StoreByPictureDate = Settings.Instance.SubfolderLastWriteDateSetting,
                    OverwriteFlag = Settings.Instance.OverwriteSetting,
                    DaysOld = Settings.Instance.ConsolidationSelectionAllSetting
                                ? 0
                                : Settings.Instance.DaysOldSetting,
                    Extensions = BuildFileExtensionList()
                };

                var photoInfo = await GooglePhotos.GetGooglePhotoInfo(id);

                if (((FileCopy)_fileOperation).SkipCopyFile(photoInfo.Title, photoInfo.TimeStamp) == false)
                {
                    var photo = await GooglePhotos.GetGooglePhoto(id);
                    ((FileCopy)_fileOperation).CopyFile(photo, photoInfo.Title, photoInfo.TimeStamp);
                }
            }

            FileOperationComplete();
        }

        private static List<string> BuildFileExtensionList()
        {
            var extensions = new List<string>();

            if (Settings.Instance.PicBmpSetting)
            {
                extensions.Add(".bmp");
            }

            if (Settings.Instance.PicGifSetting)
            {
                extensions.Add(".gif");
            }

            if (Settings.Instance.PicJpegSetting)
            {
                extensions.Add(".jpeg");
            }

            if (Settings.Instance.PicJpgSetting)
            {
                extensions.Add(".jpg");
            }

            if (Settings.Instance.PicPngSetting)
            {
                extensions.Add(".png");
            }

            if (Settings.Instance.PicPsdSetting)
            {
                extensions.Add(".psd");
            }

            if (Settings.Instance.PicRawSetting)
            {
                extensions.Add(".raw");
            }

            if (Settings.Instance.PicTiffSetting)
            {
                extensions.Add(".tiff");
            }

            if (Settings.Instance.VidAviSetting)
            {
                extensions.Add(".avi");
            }

            if (Settings.Instance.VidFlvSetting)
            {
                extensions.Add(".flv");
            }

            if (Settings.Instance.VidMovSetting)
            {
                extensions.Add(".mov");
            }

            if (Settings.Instance.VidMp4Setting)
            {
                extensions.Add(".mp4");
            }

            if (Settings.Instance.VidMpgSetting)
            {
                extensions.Add(".mpg");
            }

            if (Settings.Instance.VidWmvSetting)
            {
                extensions.Add(".wmv");
            }

            if (Settings.Instance.VidMtsSetting)
            {
                extensions.Add(".mts");
            }

            return extensions;
        }

        private delegate void FileOperationCompleteDelegate();
    }
}