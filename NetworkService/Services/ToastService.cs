using System.Windows.Threading;
using NetworkService.MVVM;

namespace NetworkService.Services
{
    public enum ToastType
    {
        Success,
        Error,
        Info,
        Warning
    }

    /// <summary>
    /// Shows non-blocking in-app feedback messages (Toast notifications). Used
    /// consistently for every completed action (add / delete). MessageBox is not
    /// used anywhere in the application, as required by the specification.
    /// Each toast carries a type, a title and content text.
    /// </summary>
    public class ToastService : ObservableObject
    {
        private readonly DispatcherTimer _timer;

        private bool _isVisible;
        private string _title;
        private string _message;
        private ToastType _type;

        public ToastService()
        {
            _timer = new DispatcherTimer { Interval = System.TimeSpan.FromSeconds(3) };
            _timer.Tick += (s, e) =>
            {
                _timer.Stop();
                IsVisible = false;
            };
        }

        public bool IsVisible
        {
            get => _isVisible;
            private set => SetProperty(ref _isVisible, value);
        }

        public string Title
        {
            get => _title;
            private set => SetProperty(ref _title, value);
        }

        public string Message
        {
            get => _message;
            private set => SetProperty(ref _message, value);
        }

        public ToastType Type
        {
            get => _type;
            private set => SetProperty(ref _type, value);
        }

        public void Show(string title, string message, ToastType type)
        {
            Title = title;
            Message = message;
            Type = type;
            IsVisible = true;

            _timer.Stop();
            _timer.Start();
        }
    }
}
