using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NetworkService.MVVM
{
    /// <summary>
    /// Base class for all objects that need to notify the UI about property
    /// changes (used by ViewModels and bindable Models). Implements
    /// INotifyPropertyChanged which is the core of WPF DataBinding.
    /// </summary>
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event. The property name is filled in
        /// automatically by the compiler via [CallerMemberName].
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Helper that assigns a new value to a backing field and raises
        /// PropertyChanged only when the value actually changed.
        /// </summary>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
