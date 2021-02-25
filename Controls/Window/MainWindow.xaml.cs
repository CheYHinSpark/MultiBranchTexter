using MultiBranchTexter.ViewModel;

namespace MultiBranchTexter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            ViewModelFactory.SetViewModel(typeof(MainViewModel), DataContext as MainViewModel);
        }
    }
}
