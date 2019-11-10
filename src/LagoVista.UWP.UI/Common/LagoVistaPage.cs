using LagoVista.Core.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.IOC;
using System.Diagnostics;

namespace LagoVista.UWP.UI
{
    public class LagoVistaPage : Windows.UI.Xaml.Controls.Page
    {
        Grid _mask;
        TextBlock _loadingMessage;
        ProgressRing _progressRing;

        Grid _popupWindowMask;
        TextBlock _popupMessage;
        Button _dismissButton;

        public bool IsConnected { get; set; }
        private bool _isBusy = false;
        private bool _initialized = false;
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
                _mask.Visibility = value ? Windows.UI.Xaml.Visibility.Visible : Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        public String BusyMessage { get; set; }

        public async Task ShowMessage(string message)
        {
            await SLWIOC.Get<IPopupServices>().ShowAsync(message);
        }

        public LagoVistaPage()
        {
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                this.Loaded += LagoVistaPage_Loaded;
            }
        }

        public IStorageService Storage
        {
            get { return SLWIOC.Get<IStorageService>(); }
        }

        private void AddLoadingMask()
        {
            if (!_initialized)
            {
                lock (this)
                {
                    _initialized = true;

                    var content = Content as Panel;
                    if (content == null)
                    {
                        throw new NullReferenceException("Could not cast content to panel.");
                    }

                    Content = null;
                    var contentContainer = new Grid();

                    _mask = new Grid() { Background = new SolidColorBrush(Colors.Black), Opacity = 0.5 };
                    _loadingMessage = new TextBlock() { HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center, VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center, Foreground = new SolidColorBrush(Colors.White), FontSize = 32, Text = "Loading...", Margin = new Windows.UI.Xaml.Thickness(0, 80, 0, 0) };
                    _progressRing = new ProgressRing() { HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center, VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center, Foreground = new SolidColorBrush(Colors.White), Width = 100, Height = 100, Margin = new Windows.UI.Xaml.Thickness(0, 240, 0, 0) };
                    _mask.Children.Add(_loadingMessage);
                    _mask.Children.Add(_progressRing);
                    _mask.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                    _popupWindowMask = new Grid();
                    _popupWindowMask.Visibility = Visibility.Collapsed;
                    _popupWindowMask.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                    _popupWindowMask.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Auto) });
                    _popupWindowMask.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

                    _popupWindowMask.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                    _popupWindowMask.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
                    _popupWindowMask.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });

                    var messageBackground = new RectangleGeometry();
                    var rect = new Windows.UI.Xaml.Shapes.Rectangle();
                    rect.Fill = new SolidColorBrush(Colors.White);
                    rect.Stroke = new SolidColorBrush(Colors.Black);
                    rect.Width = 600;
                    rect.Height = 400;
                    rect.StrokeThickness = 4;
                    rect.SetValue(Grid.RowProperty, 1);
                    rect.SetValue(Grid.ColumnProperty, 1);

                    var backgroundRect = new Windows.UI.Xaml.Shapes.Rectangle() { Fill = new SolidColorBrush(Colors.Black), Opacity = 0.5 };
                    backgroundRect.SetValue(Grid.ColumnSpanProperty, 3);
                    backgroundRect.SetValue(Grid.RowSpanProperty, 3);

                    _popupMessage = new TextBlock() { Width = 580, VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left, Foreground = new SolidColorBrush(Colors.Black), FontSize = 32 };
                    _popupMessage.Margin = new Thickness(10);
                    _popupMessage.SetValue(Grid.RowProperty, 1);
                    _popupMessage.SetValue(Grid.ColumnProperty, 1);
                    _popupMessage.TextWrapping = TextWrapping.Wrap;
                    _dismissButton = new Button() { Content = "OK", VerticalAlignment = VerticalAlignment.Bottom, HorizontalAlignment = HorizontalAlignment.Right, Width = 120, Height = 60, Margin = new Thickness(10) };
                    _dismissButton.SetValue(Grid.RowProperty, 1);
                    _dismissButton.SetValue(Grid.ColumnProperty, 1);
                    _dismissButton.Click += (e, a) =>
                    {
                        var containerViewModel = DataContext as ViewModelBase;
                        if (containerViewModel != null)
                            containerViewModel.MessageText = String.Empty;
                        _popupWindowMask.Visibility = Visibility.Collapsed;
                    };


                    _popupWindowMask.Children.Add(backgroundRect);
                    _popupWindowMask.Children.Add(rect);
                    _popupWindowMask.Children.Add(_popupMessage);
                    _popupWindowMask.Children.Add(_dismissButton);

                    contentContainer.Children.Add(content);
                    contentContainer.Children.Add(_mask);
                    contentContainer.Children.Add(_popupWindowMask);
                    Content = contentContainer;

                    var vm = DataContext as ViewModelBase;
                    SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = _navigation.CanGoBack() ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
                }
            }
        }

        private IViewModelNavigation _navigation => SLWIOC.Get<IViewModelNavigation>();


        private async void LagoVistaPage_BackRequested(object sender, BackRequestedEventArgs e)
        {
            var localViewModel = DataContext as ViewModelBase;
            if (localViewModel != null)
            {
                if (_navigation.CanGoBack())
                    await _navigation.GoBackAsync();
            }
            else
                await _navigation.GoBackAsync();
        }

        public ViewModelBase ViewModel
        {
            get { return DataContext as ViewModelBase; }
        }

        private void LagoVistaPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            AddLoadingMask();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            AddLoadingMask();

            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                var vm = DataContext as ViewModelBase;
                if (vm != null)
                {
                    vm.PropertyChanged += Vm_PropertyChanged;
                    await PerformNetworkOperation(async () =>
                    {
                        vm.LaunchArgs = e.Parameter as ViewModelLaunchArgs;
                        await vm.InitAsync();
                        SetViewModel(vm);
                    });
                }
                else
                {
                    if (e.Parameter is ViewModelLaunchArgs)
                    {
                        var args = e.Parameter as ViewModelLaunchArgs;
                        vm = SLWIOC.CreateForType(args.ViewModelType) as ViewModelBase;
                        vm.PropertyChanged += Vm_PropertyChanged;
                        await PerformNetworkOperation(async () =>
                        {   
                            vm.LaunchArgs = e.Parameter as ViewModelLaunchArgs;
                            await vm.InitAsync();
                            
                            SetViewModel(vm);
                        });                        
                    }
                }
            }
        }

        protected virtual void SetViewModel(ViewModelBase vm)
        {
            DataContext = vm;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                var vm = DataContext as ViewModelBase;
                if (vm != null)
                {
                    vm.IsClosingAsync();
                }
            }
        }

        private void ShowPanel(String name, bool zoomIn)
        {
            var stryBoard = new Storyboard();
            var xAxisScale = new DoubleAnimation()
            {
                From = zoomIn ? 0.9 : 1.1,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(0.1),
            };

            var yAxisScale = new DoubleAnimation()
            {
                From = zoomIn ? 0.9 : 1.1,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(0.1),
            };

            Storyboard.SetTargetProperty(xAxisScale, "(UIElement.RenderTransform).(CompositeTransform.ScaleX)");
            Storyboard.SetTargetProperty(yAxisScale, "(UIElement.RenderTransform).(CompositeTransform.ScaleY)");

            var ctl = FindName(name) as UIElement;

            Storyboard.SetTarget(xAxisScale, ctl);
            Storyboard.SetTarget(yAxisScale, ctl);

            stryBoard.Children.Add(xAxisScale);
            stryBoard.Children.Add(yAxisScale);
            stryBoard.Completed += Storyboard_Completed;

            stryBoard.Begin();
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var vm = DataContext as ViewModelBase;

            switch (e.PropertyName)
            {
                case "MessageText":
                    if (!String.IsNullOrEmpty(vm.MessageText))
                    {
                        _popupMessage.Text = vm.MessageText;
                        _popupWindowMask.Visibility = Visibility.Visible;
                    }
                    break;
                case "IsBusy":
                    IsBusy = vm.IsBusy;
                    break;
                case "VSGTransition":

                    //TODO: Need to think this through
                    /*
                    var transition = vm.VSGTransition;

                    if (!String.IsNullOrEmpty(transition.Animation) && Resources.ContainsKey(transition.Animation))
                    {
                        var animation = Resources[transition.Animation] as Windows.UI.Xaml.Media.Animation.Storyboard;
                        animation.Begin();
                    }
                    else if (!String.IsNullOrEmpty(transition.ControlName))
                        ShowPanel(transition.ControlName, transition.ZoomIn);
                    else
                    {
                        var groups = VisualStateManager.GetVisualStateGroups(this);
                        foreach (var grp in groups)
                        {
                            Debug.WriteLine(grp.Name);
                        }
                        var group = VisualStateManager.GetVisualStateGroups(this).Where(vsg => vsg.Name == transition.GroupName).FirstOrDefault();
                        var result = VisualStateManager.GoToState(this, transition.StateName, true);
                    }*/
                    break;
            }

        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                var vm = DataContext as ViewModelBase;
                if (vm != null)
                {
                    vm.PropertyChanged -= Vm_PropertyChanged;
                }
            }
        }


        protected void Storyboard_Completed(object sender, object e)
        {
            var vm = DataContext as ViewModelBase;
            if (vm != null)
                vm.TransitionCompleted();
        }

        protected async Task<bool> PerformNetworkOperation(Func<Task> action, String waitMessage = null)
        {
            IsConnected = true;

            var success = false;
            if (!IsConnected)
                await ShowMessage("Not Connected");
            else
            {
                try
                {
                    BusyMessage = (String.IsNullOrEmpty(waitMessage) ? "Loading" : waitMessage);
                    IsBusy = true;
                    await action();
                    success = true;
                }
                catch (Exception ex)
                {
                    _popupMessage.Text = ex.Message;
                    _popupWindowMask.Visibility = Visibility.Visible;
                }
                finally
                {
                    IsBusy = false;
                }
            }

            return success;
        }

    }
}
