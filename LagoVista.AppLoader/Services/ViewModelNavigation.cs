using LagoVista.Client.Core;
using LagoVista.Core.IOC;
using LagoVista.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace LagoVista.AppLoader.Services
{
    public class ViewModelNavigation : IViewModelNavigation
    {
        private readonly Application _app;
        private readonly NavigationWindow _navWindow;

        private Dictionary<Type, String> _registeredViews = new Dictionary<Type, String>();

        public ViewModelNavigation(NavigationWindow navWindow)
        {
            _navWindow = navWindow ?? throw new ArgumentNullException(nameof(navWindow));
            _navWindow.Navigated += _navWindow_Navigated;
        }

        private void _navWindow_Navigated(object sender, NavigationEventArgs e)
        {
             
        }

        public bool CanGoBack()
        {
            throw new NotImplementedException();
        }

        public Task GoBackAsync()
        {
            throw new NotImplementedException();
        }

        public Task NavigateAndCreateAsync<TViewModel>(ViewModelBase parentViewModel, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateAndCreateAsync<TViewModel>(ViewModelBase parentViewModel, object parentModel, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateAndEditAsync<TViewModel>(ViewModelBase parentViewModel, object parentModel, object child, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {

            throw new NotImplementedException();
        }

        public Task NavigateAndEditAsync<TViewModel>(ViewModelBase parentViewModel, object parentModel, string id, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateAndEditAsync<TViewModel>(ViewModelBase parentViewModel, string id, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateAndPickAsync<TViewModel>(ViewModelBase parentViewModel, Action<object> selectedAction, Action cancelledAction = null, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateAndViewAsync<TViewModel>(ViewModelBase parentViewModel, object parentModel, object child, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateAndViewAsync<TViewModel>(ViewModelBase parentViewModel, object parentModel, string id, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task NavigateAsync(ViewModelLaunchArgs args)
        {
            throw new NotImplementedException();
        }

        public Task NavigateAsync(ViewModelBase parentViewModel, Type viewModelType, params KeyValuePair<string, object>[] args)
        {
            throw new NotImplementedException();
        }

        public Task NavigateAsync<TViewModel>(ViewModelBase parentViewModel, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public void ClearHistory()
        {
            if (!this._navWindow.NavigationService.CanGoBack && !this._navWindow.NavigationService.CanGoForward)
            {
                return;
            }

            var entry = this._navWindow.NavigationService.RemoveBackEntry();
            while (entry != null)
            {
                entry = this._navWindow.NavigationService.RemoveBackEntry();
            }

//            this.Frame.Navigate(new PageFunction<string>() { RemoveFromJournal = true });
        }

        public async Task SetAsNewRootAsync<TViewModel>(params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            var view = _registeredViews[typeof(TViewModel)];
//            _navWindow.Source = ;
            _navWindow.NavigationService.Navigate(new Uri(view, UriKind.Relative));

            var page = _navWindow.Content as Page;

            var vm = SLWIOC.CreateForType(typeof(TViewModel)) as ViewModelBase;
            page.DataContext = vm;
            await vm.InitAsync();

            this.ClearHistory();
        }

        public async Task SetAsNewRootAsync(Type viewModelType, params KeyValuePair<string, object>[] args)
        {
            this.ClearHistory();

            var view = _registeredViews[viewModelType];
            _navWindow.Source = new Uri(view);
            var vm = SLWIOC.CreateForType(viewModelType) as ViewModelBase;
            await vm.InitAsync();
        }

        public void RegisterPage<TViewModel>(String page)
        {
            _registeredViews.Add(typeof(TViewModel), page);
        }
    }
}
