using System.Windows;
using System.Windows.Forms;

namespace Voto.Consolidate
{
    /// <summary>
    ///     Interaction logic for WindowPopUp.xaml
    /// </summary>
    public partial class WindowPopUp : Window
    {
        public string DirectoryPath { get; set; }
        public bool SubDirectorySearch { get; set; }
        public bool Valid { get; set; }
        public WindowPopUp()
        {
            InitializeComponent();
            DirectoryPath = "";
            SubDirectorySearch = false;
            Valid = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DirectoryPath = fbd.SelectedPath;
                if (SubDirectorySearch)
                {
                    DirectoryPath += "*";
                }

                textBlock.Text = DirectoryPath;
            }
        }

        private void CheckBoxSubDirectory_Click(object sender, RoutedEventArgs e)
        {
            SubDirectorySearch = checkBoxSubDirectory.IsChecked ?? false;

            if (SubDirectorySearch)
            {
                DirectoryPath += "*";
            }
            else
            {
                if (DirectoryPath[^1] == '*')
                {
                    DirectoryPath = DirectoryPath.Remove(DirectoryPath.Length - 1);
                }
            }

            textBlock.Text = DirectoryPath;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            Valid = true;
            Close();
        }
    }
}