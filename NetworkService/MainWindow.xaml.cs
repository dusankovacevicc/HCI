using System.Windows;
using NetworkService.ViewModels;

namespace NetworkService
{
    /// <summary>
    /// The single window of the application. It emulates a portrait phone screen
    /// (CG3). It owns the MainViewModel, starts the simulator server when loaded
    /// and stops it when closed.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            Loaded += (s, e) => _viewModel.Start();
            Closed += (s, e) => _viewModel.Shutdown();
        }
    }
}
