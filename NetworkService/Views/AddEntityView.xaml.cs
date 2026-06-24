using System.Windows.Controls;
using NetworkService.ViewModels;

namespace NetworkService.Views
{
    /// <summary>
    /// Form for adding an entity. The GotFocus handlers tell the ViewModel which
    /// field the virtual keyboard should type into.
    /// </summary>
    public partial class AddEntityView : UserControl
    {
        public AddEntityView()
        {
            InitializeComponent();
        }

        private void IdBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is AddEntityViewModel vm)
            {
                vm.ActiveField = InputField.Id;
            }
        }

        private void NameBox_GotFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is AddEntityViewModel vm)
            {
                vm.ActiveField = InputField.Name;
            }
        }
    }
}
