using System.Windows.Controls;

namespace Voto.Consolidate
{
    /// <summary>
    /// Interaction logic for PageMedia.xaml
    /// </summary>
    public partial class PageMedia : Page
    {
        public Settings Settings { get; set; } = null;
        public PageMedia()
        {
            InitializeComponent();
            DataContext = this;
            Settings = Settings.Instance;
        }
    }
}
