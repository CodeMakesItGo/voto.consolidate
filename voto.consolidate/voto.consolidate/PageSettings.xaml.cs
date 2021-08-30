using file.consolidate;
using google.consolidate;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace Voto.Consolidate
{
    /// <summary>
    ///     Interaction logic for PageSettings.xaml
    /// </summary>
    public partial class PageSettings : Page
    {
        public Settings Settings { get; set; }

        public PageSettings()
        {
            InitializeComponent();
            DataContext = this;
            Settings = Settings.Instance;

            if (Settings.Instance.SourceDirectorySetting != null)
            {
                foreach (var v in Settings.Instance.SourceDirectorySetting)
                {
                    listBox.Items.Add(v);
                }
            }
            else
            {
                Settings.Instance.SourceDirectorySetting = new StringCollection();
            }

            if (Settings.Instance.GoogleAlbumsSetting != null &&
                Settings.Instance.GoogleAlbumsSetting.Count > 0)
            {
                LoadGoogleAlbums();
            }
        }

        private void TextBoxDestinationRoot_Loaded(object sender, RoutedEventArgs e)
        {
            checkBoxOverwrite.IsEnabled = Settings.Instance.OverwriteSetting;
            checkBoxDeleteEmptyDir.IsEnabled = Settings.Instance.DeleteEmptyDirectories;
            checkBoxDeleteDuplicates.IsEnabled = Settings.Instance.DeleteDuplicateFiles;
            SliderDaysOlder.IsEnabled = Settings.Instance.ConsolidationSelectionOldSetting;
        }

        private void ButtonAddSourceDirectory_Click(object sender, RoutedEventArgs e)
        {
            var fbd = new WindowPopUp();
            fbd.ShowDialog();

            if (fbd.Valid)
            {
                listBox.Items.Add(fbd.DirectoryPath);
                Settings.Instance.SourceDirectorySetting.Add(fbd.DirectoryPath);
            }
        }

        private void ButtonRemoveSourceDirectory_Click(object sender, RoutedEventArgs e)
        {
            while (listBox.SelectedItems.Count > 0)
            {
                Settings.Instance.SourceDirectorySetting.Remove(listBox.SelectedItems[0].ToString());
                listBox.Items.Remove(listBox.SelectedItems[0]);
            }
        }

        private void ButtonDestinationRoot_Click(object sender, RoutedEventArgs e)
        {
            var fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                textBoxDestinationRoot.Text = fbd.SelectedPath;
                Settings.Instance.DestinationRootSetting = fbd.SelectedPath;
            }
        }

        private void RadioButtonCopy_Click(object sender, RoutedEventArgs e)
        {
            checkBoxOverwrite.IsEnabled = true;
            checkBoxDeleteEmptyDir.IsEnabled = false;
            checkBoxDeleteDuplicates.IsEnabled = false;
        }

        private void RadioButtonMove_Click(object sender, RoutedEventArgs e)
        {
            checkBoxOverwrite.IsEnabled = false;
            checkBoxDeleteEmptyDir.IsEnabled = true;
            checkBoxDeleteDuplicates.IsEnabled = true;
        }

        private void RadioButtonConsolidationSelectionAll_Click(object sender, RoutedEventArgs e)
        {
            SliderDaysOlder.IsEnabled = false;
        }

        private void RadioButtonConsolidationSelectionOld_Click(object sender, RoutedEventArgs e)
        {
            SliderDaysOlder.IsEnabled = true;
        }

        private void ButtonGetGoogleAlbums_Click(object sender, RoutedEventArgs e)
        {
            LoadGoogleAlbums();
        }

        private async void LoadGoogleAlbums()
        {
            ProgressReport.Instance.Report = "Loading Google Photo Albums";

            listBoxGoogleAlbums.Items.Clear();

            listBoxGoogleAlbums.Items.Add(new GooglePhotos.PhotoAlbum() { Url = "", Title = "All Photos", Id = "1234", MediaCount = 0 });

            if (Settings.Instance.GoogleAlbumsSetting == null)
            {
                Settings.Instance.GoogleAlbumsSetting = new StringCollection();
            }

            var albums = await GooglePhotos.GetPhotoAlbums();
            
            foreach (var album in albums)
            {
                listBoxGoogleAlbums.Items.Add(album);
            }

            for (int i = 0; i < albums.Count; ++i)
            {
                var album = (GooglePhotos.PhotoAlbum)listBoxGoogleAlbums.Items[i];

                foreach (var selectedAlbum in Settings.Instance.GoogleAlbumsSetting)
                {
                    if (album.Id.Equals(selectedAlbum))
                    {
                        album.IsSelected = true;
                    }
                }
            }

            ProgressReport.Instance.Report = "Ready!";
        }

        private void CheckBoxZone_Checked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox chkZone = (System.Windows.Controls.CheckBox)sender;

            if (Settings.Instance.GoogleAlbumsSetting.IndexOf((string)chkZone.Tag) == -1)
            {
                Settings.Instance.GoogleAlbumsSetting.Add((string)chkZone.Tag);
            }
        }

        private void CheckBoxZone_Unchecked(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.CheckBox chkZone = (System.Windows.Controls.CheckBox)sender;
            Settings.Instance.GoogleAlbumsSetting.Remove((string)chkZone.Tag);
        }

        private void CheckBoxDeleteEmptyDir_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBoxDeleteDuplicates_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CheckBoxOverwrite_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void RadioButtonSubfolderConsolidationDate_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RadioButtonSubfolderLastWriteDate_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}