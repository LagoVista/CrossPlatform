using LagoVista.AppLoader.Common;
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

        private Dictionary<Type, Type> _registeredViews = new Dictionary<Type, Type>();

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
            var view = _registeredViews[args.ViewModelType];
            var page = Activator.CreateInstance(view) as LagoVistaPage;
            _navWindow.Navigate(page);
            var vm = SLWIOC.CreateForType(args.ViewModelType) as ViewModelBase;
            vm.LaunchArgs =args;

            page.DataContext = vm;
            return Task.CompletedTask;
        }

        public Task NavigateAsync(ViewModelBase parentViewModel, Type viewModelType, params KeyValuePair<string, object>[] args)
        {
            throw new NotImplementedException();
        }

        public Task NavigateAsync<TViewModel>(ViewModelBase parentViewModel, params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            throw new NotImplementedException();
        }

        public Task SetAsNewRootAsync<TViewModel>(params KeyValuePair<string, object>[] args) where TViewModel : ViewModelBase
        {
            var view = _registeredViews[typeof(TViewModel)];
            var page = Activator.CreateInstance(view) as LagoVistaPage;
            _navWindow.Navigate(page);
            page.ClearNavStack = true;

            var vm = SLWIOC.CreateForType(typeof(TViewModel)) as ViewModelBase;
            vm.LaunchArgs = new ViewModelLaunchArgs()
            {
                IsNewRoot = true,
                LaunchType = LaunchTypes.Other,
            };

            foreach (var arg in args)
            {
                vm.LaunchArgs.Parameters.Add(arg.Key, arg.Value);
            }

            page.DataContext = vm;

            return Task.CompletedTask;
        }

        public Task SetAsNewRootAsync(Type viewModelType, params KeyValuePair<string, object>[] args)
        {
            var view = _registeredViews[viewModelType];
            var page = Activator.CreateInstance(view) as LagoVistaPage;
            _navWindow.Navigate(page);
            page.ClearNavStack = true;

            var vm = SLWIOC.CreateForType(viewModelType) as ViewModelBase;
            vm.LaunchArgs = new ViewModelLaunchArgs()
            {
                IsNewRoot = true,
                LaunchType = LaunchTypes.Other,
            };

            foreach (var arg in args)
            {
                vm.LaunchArgs.Parameters.Add(arg.Key, arg.Value);
            }

            page.DataContext = vm;

            return Task.CompletedTask;
        }

        public void RegisterPage<TViewModel, TView>()
        {
            _registeredViews.Add(typeof(TViewModel), typeof(TView));
        }
    }
}
