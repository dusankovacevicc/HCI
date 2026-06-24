using System;
using NetworkService.MVVM;
using System.Windows.Input;

namespace NetworkService.Services
{
    /// <summary>
    /// Drives an in-app confirmation overlay (used before destructive actions
    /// such as deleting an entity). MessageBox is intentionally avoided; the
    /// confirmation is rendered as a styled overlay inside the application.
    /// </summary>
    public class ConfirmationService : ObservableObject
    {
        private bool _isVisible;
        private string _title;
        private string _message;
        private Action _onConfirm;

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

        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        public ConfirmationService()
        {
            ConfirmCommand = new RelayCommand(() =>
            {
                Action callback = _onConfirm;
                IsVisible = false;
                _onConfirm = null;
                callback?.Invoke();
            });

            CancelCommand = new RelayCommand(() =>
            {
                IsVisible = false;
                _onConfirm = null;
            });
        }

        /// <summary>Requests confirmation; <paramref name="onConfirm"/> runs only if accepted.</summary>
        public void Request(string title, string message, Action onConfirm)
        {
            Title = title;
            Message = message;
            _onConfirm = onConfirm;
            IsVisible = true;
        }
    }
}
