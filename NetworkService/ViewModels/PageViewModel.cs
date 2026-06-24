using NetworkService.MVVM;

namespace NetworkService.ViewModels
{
    /// <summary>
    /// Base class for every "page" ViewModel hosted by the MainViewModel.
    /// Carries a Title that the main window shows in its header bar.
    /// </summary>
    public abstract class PageViewModel : ObservableObject
    {
        protected PageViewModel(IAppController controller)
        {
            Controller = controller;
        }

        protected IAppController Controller { get; }

        public abstract string Title { get; }
    }
}
