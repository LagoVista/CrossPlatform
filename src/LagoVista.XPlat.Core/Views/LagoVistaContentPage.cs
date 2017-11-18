using LagoVista.Client.Core.ViewModels;
using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.Core.PlatformSupport;
using LagoVista.XPlat.Core.Controls.Common;
using LagoVista.XPlat.Core.Views;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace LagoVista.XPlat.Core
{


    public abstract class LagoVistaContentPage : ContentPage, ILagoVistaPage
    {
        Grid _toolBar;
        Grid _contentGrid;
        Grid _loadingMask;
        Grid _loadingContainer;
        SideMenu _menu;
        Grid _pageMenuMask;
        Label _title;
        ActivityIndicator _activityIndicator;
        View _mainContent;
        View _originalcontent;

        IconButton _leftMenuButton;
        IconButton _rightMenuButton;
        IconButton _helpButton;

        bool _hasAppeared = false;

        const int MENU_WIDTH = 300;
        const int TOOL_BAR_HEIGHT = 44;

        public LagoVistaContentPage() : base()
        {
            NavigationPage.SetHasNavigationBar(this, false);

            /*
             * The Page top level consists of a grid, to add additional faeture on top fo the grid such as loading window
             * and a slide out menu, we attach the actual content to the property MainContent, rather than just to the page.
             * Within the XAML it will look like:
             *     <pge:LagoVistaContentPage.MainContent>
             *                  .........
             *     </pge:LagoVistaContentPage.MainContent>
             * 
             * You can add any content to that node, just as you would to the primary content node of the page.
             * 
             */

            CreateActivityIndicator();
            CreateMenu();
            AddToolBar();
            AddBindings();
        }

        public void HandleURIActivation(Uri uri)
        {
            var logger = SLWIOC.Get<ILogger>();
            if (ViewModel != null)
            {
                var query = uri.Query.TrimStart('?');
                var segments = query.Split('&');
                var kvps = new Dictionary<string, string>();
                foreach (var segment in segments)
                {
                    var parts = segment.Split('=');
                    if (parts.Length != 2)
                    {
                        logger.AddCustomEvent(LagoVista.Core.PlatformSupport.LogLevel.Error, "VerifyUserViewModel_HandleURIActivation", "Invalid Query String", new KeyValuePair<string, string>("queryString", query));
                        return;
                    }

                    if (String.IsNullOrEmpty(parts[0]))
                    {
                        logger.AddCustomEvent(LagoVista.Core.PlatformSupport.LogLevel.Error, "VerifyUserViewModel_HandleURIActivation", "Invalid Key on Query String", new KeyValuePair<string, string>("queryString", query));
                        return;
                    }

                    if (String.IsNullOrEmpty(parts[1]))
                    {
                        logger.AddCustomEvent(LagoVista.Core.PlatformSupport.LogLevel.Error, "VerifyUserViewModel_HandleURIActivation", "Invalid Value on Query String", new KeyValuePair<string, string>("queryString", query));
                        return;
                    }

                    kvps.Add(parts[0].ToLower(), parts[1]);
                }

                ViewModel.HandleURIActivation(uri, kvps);
            }
            else
            {
                logger.AddCustomEvent(LogLevel.Error, "LagoVistaNavigationPage_HandleURIActivation", "Main Page Null");
            }
        }

        #region Initial Page Construction
        private void AddBindings()
        {
            this.SetBinding(LagoVistaContentPage.MenuVisibleProperty, nameof(ViewModel.MenuVisible));
            this.SetBinding(LagoVistaContentPage.SaveCommandProperty, nameof(ViewModel.SaveCommand));
            this.SetBinding(LagoVistaContentPage.HelpCommandProperty, nameof(ViewModel.HelpCommand));
            this.SetBinding(LagoVistaContentPage.EditCommandProperty, nameof(ViewModel.EditCommand));
            this.SetBinding(LagoVistaContentPage.MenuItemsProperty, nameof(ViewModel.MenuItems));
            this.SetBinding(LagoVistaContentPage.HasHelpProperty, nameof(ViewModel.HasHelp));
        }

        private void CreateActivityIndicator()
        {
            _activityIndicator = new ActivityIndicator() { IsRunning = false };
            _activityIndicator.Color = Xamarin.Forms.Color.White;
            _activityIndicator.VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false);

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    _activityIndicator.HorizontalOptions = new LayoutOptions(LayoutAlignment.Center, false);
                    break;
                case Device.Android:
                    _activityIndicator.HorizontalOptions = new LayoutOptions(LayoutAlignment.Center, false);
                    _activityIndicator.WidthRequest = 64;
                    _activityIndicator.HeightRequest = 64;
                    break;
            }

            _loadingContainer = new Grid() { IsVisible = false };

            _loadingMask = new Grid() { BackgroundColor = Xamarin.Forms.Color.Black, Opacity = 0.50 };
            _loadingContainer.Children.Add(_loadingMask);
            _loadingContainer.Children.Add(_activityIndicator);
            _loadingContainer.SetValue(Grid.RowProperty, 1);
        }

        private void CreateMenu()
        {
            _menu = new SideMenu(SLWIOC.Get<IAuthManager>());
            _menu.MenuItemTapped += _menu_MenuItemTapped;
            _menu.IsVisible = false;
            _menu.TranslationX = -MENU_WIDTH;
            _menu.BackgroundColor = AppStyle.MenuBarBackground.ToXamFormsColor();
            _menu.WidthRequest = MENU_WIDTH;
            _menu.HorizontalOptions = LayoutOptions.Start;
            _menu.SetValue(Grid.RowProperty, 1);

            _pageMenuMask = new Grid() { BackgroundColor = Xamarin.Forms.Color.Black, Opacity = 0.50 };
            _pageMenuMask.SetValue(Grid.RowProperty, 1);
            _pageMenuMask.IsVisible = false;
        }

        private void _menu_MenuItemTapped(object sender, Client.Core.ViewModels.MenuItem e)
        {
            MenuVisible = false;
        }

        private void AddToolBar()
        {
            _toolBar = new Grid
            {
                HeightRequest = TOOL_BAR_HEIGHT
            };
            _toolBar.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
            _toolBar.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            _toolBar.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
            _toolBar.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
            _toolBar.BackgroundColor = AppStyle.TitleBarBackground.ToXamFormsColor();

            _title = new Label();
            _title.SetValue(Grid.ColumnProperty, 1);
            _title.TextColor = AppStyle.TitleBarText.ToXamFormsColor();
            _title.FontSize = 22;
            _title.Margin = new Thickness(-5, -3, 0, 0);
            _title.VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false);

            _leftMenuButton = new IconButton
            {
                IsVisible = false,
                VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false),
                TextColor = AppStyle.TitleBarText.ToXamFormsColor(),
                WidthRequest = 48,
                HeightRequest = 48,
                FontSize = 22
            };
            _leftMenuButton.Clicked += _leftMenuButton_Clicked;

            _helpButton = new IconButton
            {
                IsVisible = false,
                IconKey = "fa-question",
                VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false),
                TextColor = AppStyle.TitleBarText.ToXamFormsColor(),
                WidthRequest = 48,
                HeightRequest = 48,
                FontSize = 22
            };
            _helpButton.Clicked += _helpButton_Clicked;
            _helpButton.SetValue(Grid.ColumnProperty, 3);

            _rightMenuButton = new IconButton
            {
                IsVisible = false,
                VerticalOptions = new LayoutOptions(LayoutAlignment.Center, false),
                TextColor = AppStyle.TitleBarText.ToXamFormsColor(),
                WidthRequest = 48,
                HeightRequest = 48,
                FontSize = 22
            };
            _rightMenuButton.SetValue(Grid.ColumnProperty, 2);
            _rightMenuButton.Clicked += _rightMenuButton_Clicked;

            _toolBar.Children.Add(_title);
            _toolBar.Children.Add(_leftMenuButton);
            _toolBar.Children.Add(_rightMenuButton);
            _toolBar.Children.Add(_helpButton);
        }

        private void _helpButton_Clicked(object sender, EventArgs e)
        {
            HelpCommand?.Execute(null);
        }

        private async void _leftMenuButton_Clicked(object sender, System.EventArgs e)
        {
            switch (LeftMenu)
            {
                case LeftMenuIcon.Back:
                    if (await ViewModel.CanCancelAsync())
                    {
                        await ViewModel.ViewModelNavigation.GoBackAsync();
                    }
                    break;
                case LeftMenuIcon.Cancel:
                    if (await ViewModel.CanCancelAsync())
                    {
                        if (CancelCommand != null)
                        {
                            CancelCommand.Execute(null);
                        }
                        else
                        {
                            await ViewModel.ViewModelNavigation.GoBackAsync();
                        }
                    }
                    break;
                case LeftMenuIcon.Menu:
                    MenuVisible = !MenuVisible;
                    break;
                case LeftMenuIcon.None:
                    break;
                case LeftMenuIcon.CustomText:
                case LeftMenuIcon.CustomIcon:
                    LeftMenuCommand?.Execute(null);
                    break;
            }
        }


        protected override bool OnBackButtonPressed()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var result = await ViewModel.CanCancelAsync();
                if (result)
                {
                    await ViewModel.ViewModelNavigation.GoBackAsync();
                }
            });

            return true;
        }

        private void _rightMenuButton_Clicked(object sender, System.EventArgs e)
        {
            switch (RightMenu)
            {
                case RightMenuIcon.Add:
                    AddCommand?.Execute(null);
                    break;
                case RightMenuIcon.CustomText:
                case RightMenuIcon.CustomIcon:
                    RightMenuCommand?.Execute(null);
                    break;
                case RightMenuIcon.Delete:
                    DeleteCommand?.Execute(null);
                    break;
                case RightMenuIcon.Edit:
                    EditCommand?.Execute(null);
                    break;
                case RightMenuIcon.Save:
                    SaveCommand?.Execute(null);
                    break;
                case RightMenuIcon.Settings:
                    SettingsCommand?.Execute(null);
                    break;
            }
        }
        #endregion

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            base.LayoutChildren(x, y, width, height);

            _title.Text = Title;
        }

        private IAppStyle AppStyle { get { return SLWIOC.Get<IAppStyle>(); } }


        public View MainContent
        {
            get { return _mainContent; }
            set
            {
                _contentGrid = new Grid();
                switch (Device.RuntimePlatform)
                {
                    case Device.iOS:
                        _contentGrid.Margin = new Thickness(0, 20, 0, 0);
                        break;
                }

                if (HasToolBar)
                {
                    _contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(TOOL_BAR_HEIGHT) });
                }
                else
                {
                    _contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0) });
                }

                _contentGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

                _contentGrid.BackgroundColor = AppStyle.TitleBarBackground.ToXamFormsColor();

                Content = _contentGrid;

                _mainContent = value;
                _mainContent.BackgroundColor = Color.White;

                if (value.BackgroundColor != null && value.BackgroundColor.R > -1)
                {
                    _mainContent.BackgroundColor = value.BackgroundColor;
                }
                else
                {
                    _mainContent.BackgroundColor = AppStyle.PageBackground.ToXamFormsColor();
                }
                _mainContent.SetValue(Grid.RowProperty, 1);
                _contentGrid.Children.Add(_mainContent);
                _contentGrid.Children.Add(_pageMenuMask);
                _contentGrid.Children.Add(_menu);
                _contentGrid.Children.Add(_loadingContainer);
                _contentGrid.Children.Add(_toolBar);
            }
        }

        public XPlatViewModel ViewModel
        {
            get { return BindingContext as XPlatViewModel; }
            set
            {
                BindingContext = value;
                if (value != null && value.MenuItems != null)
                {
                    _menu.MenuItems = value.MenuItems;
                }
                value.PropertyChanged += Value_PropertyChanged;
            }
        }

        private async void ToggleMenu(bool newMenuState)
        {
            if (newMenuState)
            {
                _pageMenuMask.IsVisible = true;
                _menu.IsVisible = true;
            }

            await _menu.TranslateTo(newMenuState ? 0 : -MENU_WIDTH, 0, 250, Easing.CubicInOut);

            await Task.Delay(300);
            if (!newMenuState)
            {
                _pageMenuMask.IsVisible = false;
                _menu.IsVisible = false;
            }
        }

        private void Value_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.IsBusy):
                    _activityIndicator.IsRunning = ViewModel.IsBusy;
                    _activityIndicator.IsVisible = ViewModel.IsBusy;
                    _loadingContainer.IsVisible = ViewModel.IsBusy;
                    break;
            }
        }

        #region Menu
        public static readonly BindableProperty MenuVisibleProperty = BindableProperty.Create(nameof(MenuVisible), typeof(bool), typeof(LagoVistaContentPage), false, BindingMode.TwoWay, null,
            (view, oldValue, newValue) => (view as LagoVistaContentPage).ToggleMenu((bool)newValue), null, null, (view) => { return false; });

        public bool MenuVisible
        {
            get { return (bool)GetValue(MenuVisibleProperty); }
            set
            {
                SetValue(MenuVisibleProperty, value);
                ToggleMenu(value);
            }
        }
        #endregion

        #region Bindable Properties
        public static readonly BindableProperty HasToolBarProperty = BindableProperty.Create(nameof(HasToolBar), typeof(bool), typeof(LagoVistaContentPage), true, BindingMode.TwoWay, null,
            (view, oldValue, newValue) => (view as LagoVistaContentPage).HasToolBar = (bool)newValue, null, null, (view) => { return true; });

        public bool HasToolBar
        {
            get { return (bool)GetValue(HasToolBarProperty); }
            set
            {
                SetValue(HasToolBarProperty, value);
                if (_contentGrid != null)
                {
                    if (value)
                    {
                        _contentGrid.RowDefinitions[0] = new RowDefinition() { Height = new GridLength(TOOL_BAR_HEIGHT) };
                    }
                    else
                    {
                        _contentGrid.RowDefinitions[0] = new RowDefinition() { Height = new GridLength(0) };
                    }
                }
            }
        }

        public static readonly BindableProperty SettingsCommandProperty = BindableProperty.Create(nameof(SettingsCommand), typeof(ICommand), typeof(LagoVistaContentPage), default(ICommand), BindingMode.Default, null,
           (view, oldValue, newValue) => (view as LagoVistaContentPage).SettingsCommand = (ICommand)newValue);

        public ICommand SettingsCommand
        {
            get { return (ICommand)GetValue(SettingsCommandProperty); }
            set { SetValue(SettingsCommandProperty, value); }
        }

        public static readonly BindableProperty SaveCommandProperty = BindableProperty.Create(nameof(SaveCommand), typeof(ICommand), typeof(LagoVistaContentPage), default(ICommand), BindingMode.Default, null,
            (view, oldValue, newValue) => (view as LagoVistaContentPage).SaveCommand = (ICommand)newValue);

        public ICommand SaveCommand
        {
            get { return (ICommand)GetValue(SaveCommandProperty); }
            set { SetValue(SaveCommandProperty, value); }
        }

        public static readonly BindableProperty CancelCommandProperty = BindableProperty.Create(nameof(CancelCommand), typeof(ICommand), typeof(LagoVistaContentPage), default(ICommand), BindingMode.Default, null,
            (view, oldValue, newValue) => (view as LagoVistaContentPage).CancelCommand = (ICommand)newValue);

        public ICommand CancelCommand
        {
            get { return (ICommand)GetValue(CancelCommandProperty); }
            set { SetValue(CancelCommandProperty, value); }
        }

        public static readonly BindableProperty AddCommandProperty = BindableProperty.Create(nameof(AddCommand), typeof(ICommand), typeof(LagoVistaContentPage), default(ICommand), BindingMode.Default, null,
            (view, oldValue, newValue) => (view as LagoVistaContentPage).AddCommand = (ICommand)newValue);

        public ICommand AddCommand
        {
            get { return (ICommand)GetValue(AddCommandProperty); }
            set { SetValue(AddCommandProperty, value); }
        }


        public static readonly BindableProperty HelpCommandProperty = BindableProperty.Create(nameof(AddCommand), typeof(ICommand), typeof(LagoVistaContentPage), default(ICommand), BindingMode.Default, null,
            (view, oldValue, newValue) => (view as LagoVistaContentPage).HelpCommand = (ICommand)newValue);

        public ICommand HelpCommand
        {
            get { return (ICommand)GetValue(HelpCommandProperty); }
            set { SetValue(HelpCommandProperty, value); }
        }



        public static readonly BindableProperty DeleteCommandProperty = BindableProperty.Create(nameof(DeleteCommand), typeof(ICommand), typeof(LagoVistaContentPage), default(ICommand), BindingMode.Default, null,
            (view, oldValue, newValue) => (view as LagoVistaContentPage).DeleteCommand = (ICommand)newValue);

        public ICommand DeleteCommand
        {
            get { return (ICommand)GetValue(DeleteCommandProperty); }
            set { SetValue(DeleteCommandProperty, value); }
        }


        public static readonly BindableProperty EditCommandProperty = BindableProperty.Create(nameof(EditCommand), typeof(ICommand), typeof(LagoVistaContentPage), default(ICommand), BindingMode.Default, null,
            (view, oldValue, newValue) => (view as LagoVistaContentPage).EditCommand = (ICommand)newValue);

        public ICommand EditCommand
        {
            get { return (ICommand)GetValue(EditCommandProperty); }
            set { SetValue(EditCommandProperty, value); }
        }

        public static readonly BindableProperty BackCommandProperty = BindableProperty.Create(nameof(BackCommand), typeof(ICommand), typeof(LagoVistaContentPage), default(ICommand), BindingMode.Default, null,
            (view, oldValue, newValue) => (view as LagoVistaContentPage).BackCommand = (ICommand)newValue);

        public ICommand BackCommand
        {
            get { return (ICommand)GetValue(BackCommandProperty); }
            set { SetValue(BackCommandProperty, value); }
        }

        public static readonly BindableProperty LeftMenuCommandProperty = BindableProperty.Create(nameof(LeftMenuCommand), typeof(ICommand), typeof(LagoVistaContentPage), default(ICommand), BindingMode.Default, null,
            (view, oldValue, newValue) => (view as LagoVistaContentPage).LeftMenuCommand = (ICommand)newValue);

        public ICommand LeftMenuCommand
        {
            get { return (ICommand)GetValue(LeftMenuCommandProperty); }
            set { SetValue(LeftMenuCommandProperty, value); }
        }

        public static readonly BindableProperty RightMenuCommandProperty = BindableProperty.Create(nameof(RightMenuCommand), typeof(ICommand), typeof(LagoVistaContentPage), default(ICommand), BindingMode.Default, null,
            (view, oldValue, newValue) => (view as LagoVistaContentPage).RightMenuCommand = (ICommand)newValue);

        public ICommand RightMenuCommand
        {
            get { return (ICommand)GetValue(RightMenuCommandProperty); }
            set { SetValue(RightMenuCommandProperty, value); }
        }

        public static readonly BindableProperty LeftMenuEnabledProperty = BindableProperty.Create(nameof(LeftMenuEnabled), typeof(bool), typeof(LagoVistaContentPage), false, BindingMode.TwoWay, null,
            (view, oldValue, newValue) => (view as LagoVistaContentPage).LeftMenuEnabled = (bool)newValue, null, null, (view) => { return false; });


        public bool LeftMenuEnabled
        {
            get { return (bool)GetValue(MenuVisibleProperty); }
            set
            {
                SetValue(MenuVisibleProperty, value);
                _leftMenuButton.IsEnabled = value;
            }
        }


        public new static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(LagoVistaContentPage), default(string), BindingMode.TwoWay, null,
            (view, oldValue, newValue) => (view as LagoVistaContentPage).Title = (string)newValue);

        public new string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set
            {
                SetValue(TitleProperty, value);
                _title.Text = value;
            }
        }

        public static readonly BindableProperty RightMenuEnabledProperty = BindableProperty.Create(nameof(RightMenuEnabled), typeof(bool), typeof(LagoVistaContentPage), false, BindingMode.TwoWay, null,
            (view, oldValue, newValue) => (view as LagoVistaContentPage).RightMenuEnabled = (bool)newValue, null, null, (view) => { return false; });

        public bool RightMenuEnabled
        {
            get { return (bool)GetValue(RightMenuEnabledProperty); }
            set
            {
                SetValue(RightMenuEnabledProperty, value);
                _rightMenuButton.IsEnabled = value;
            }
        }


        public static readonly BindableProperty LeftMenuCustomIconProperty = BindableProperty.Create(nameof(LeftMenuCustomIcon), typeof(string), typeof(LagoVistaContentPage), default(string), BindingMode.TwoWay, null,
            (view, oldValue, newValue) => (view as LagoVistaContentPage).LeftMenuCustomIcon = (string)newValue);

        public string LeftMenuCustomIcon
        {
            get { return (string)GetValue(LeftMenuCustomIconProperty); }
            set
            {
                SetValue(LeftMenuCustomIconProperty, value);
                SetLeftMenuIcon();
            }
        }


        public static readonly BindableProperty RightMenuCustomIconProperty = BindableProperty.Create(nameof(RightMenuCustomIcon), typeof(string), typeof(LagoVistaContentPage), default(string), BindingMode.TwoWay, null,
            (view, oldValue, newValue) => (view as LagoVistaContentPage).RightMenuCustomIcon = (string)newValue);

        public string RightMenuCustomIcon
        {
            get { return (string)GetValue(RightMenuCustomIconProperty); }
            set
            {
                SetValue(RightMenuCustomIconProperty, value);
                SetRightMenuIcon();
            }
        }

        public static readonly BindableProperty LeftMenuCustomTextProperty = BindableProperty.Create(nameof(LeftMenuCustomText), typeof(string), typeof(LagoVistaContentPage), default(string), BindingMode.TwoWay, null,
            (view, oldValue, newValue) => (view as LagoVistaContentPage).LeftMenuCustomText = (string)newValue);

        public string LeftMenuCustomText
        {
            get { return (string)GetValue(LeftMenuCustomTextProperty); }
            set
            {
                SetValue(LeftMenuCustomTextProperty, value);
                SetLeftMenuIcon();
            }
        }


        public static readonly BindableProperty RightMenuCustomTextProperty = BindableProperty.Create(nameof(RightMenuCustomText), typeof(string), typeof(LagoVistaContentPage), default(string), BindingMode.TwoWay, null,
            (view, oldValue, newValue) => (view as LagoVistaContentPage).RightMenuCustomText = (string)newValue);

        public string RightMenuCustomText
        {
            get { return (string)GetValue(RightMenuCustomTextProperty); }
            set
            {
                SetValue(RightMenuCustomTextProperty, value);
                SetRightMenuIcon();
            }
        }

        public static readonly BindableProperty HasHelpProperty = BindableProperty.Create(nameof(HasHelp), typeof(bool), typeof(LagoVistaContentPage), false, BindingMode.TwoWay, null,
            (view, oldValue, newValue) => (view as LagoVistaContentPage).HasHelp = (bool)newValue);

        public bool HasHelp
        {
            get { return (bool)GetValue(HasHelpProperty); }
            set
            {
                SetValue(HasHelpProperty, value);
                _helpButton.IsVisible = value;
            }
        }

        public static readonly BindableProperty LeftMenuProperty = BindableProperty.Create(nameof(LeftMenu), typeof(LeftMenuIcon), typeof(LagoVistaContentPage), LeftMenuIcon.None, BindingMode.TwoWay, null,
            (view, oldValue, newValue) => (view as LagoVistaContentPage).LeftMenu = (LeftMenuIcon)newValue);

        void SetLeftMenuIcon()
        {
            if (LeftMenu == LeftMenuIcon.None)
            {
                _leftMenuButton.IsVisible = false;
            }
            else
            {
                _leftMenuButton.IsVisible = true;
                switch (LeftMenu)
                {
                    case LeftMenuIcon.Menu: _leftMenuButton.IconKey = "fa-bars"; break;
                    case LeftMenuIcon.Cancel: _leftMenuButton.IconKey = "fa-chevron-left"; break;
                    case LeftMenuIcon.Back: _leftMenuButton.IconKey = "fa-chevron-left"; break;
                    case LeftMenuIcon.CustomIcon:
                        if (string.IsNullOrEmpty(LeftMenuCustomIcon))
                        {
                            _leftMenuButton.IconKey = LeftMenuCustomIcon;
                        }
                        break;
                    case LeftMenuIcon.CustomText: _leftMenuButton.Text = LeftMenuCustomText; break;
                }
            }
        }

        void SetRightMenuIcon()
        {
            if (RightMenu == RightMenuIcon.None)
            {
                _rightMenuButton.IsVisible = false;
            }
            else
            {
                _rightMenuButton.IsVisible = true;
                switch (RightMenu)
                {
                    case RightMenuIcon.Add: _rightMenuButton.IconKey = "fa-plus"; break;
                    case RightMenuIcon.Delete: _rightMenuButton.IconKey = "fa-trash"; break;
                    case RightMenuIcon.Save: _rightMenuButton.IconKey = "fa-floppy-o"; break;
                    case RightMenuIcon.Edit: _rightMenuButton.IconKey = "fa-pencil"; break;
                    case RightMenuIcon.Settings: _rightMenuButton.IconKey = "fa-cog"; break;
                    case RightMenuIcon.CustomIcon:
                        if (string.IsNullOrEmpty(RightMenuCustomIcon))
                        {
                            _rightMenuButton.IconKey = RightMenuCustomIcon;
                        }
                        break;
                    case RightMenuIcon.CustomText: _rightMenuButton.Text = RightMenuCustomText; break;
                }
            }
        }

        public LeftMenuIcon LeftMenu
        {
            get { return (LeftMenuIcon)GetValue(LeftMenuProperty); }
            set
            {
                SetValue(LeftMenuProperty, value);
                SetLeftMenuIcon();
            }
        }

        public static readonly BindableProperty RightMenuProperty = BindableProperty.Create(nameof(RightMenu), typeof(RightMenuIcon), typeof(LagoVistaContentPage), RightMenuIcon.None, BindingMode.TwoWay, null,
            (view, oldValue, newValue) => (view as LagoVistaContentPage).RightMenu = (RightMenuIcon)newValue, null, null, (view) => { return false; });

        public RightMenuIcon RightMenu
        {
            get { return (RightMenuIcon)GetValue(RightMenuProperty); }
            set
            {

                SetValue(RightMenuProperty, value);
                SetRightMenuIcon();
            }
        }

        public static readonly BindableProperty MenuItemsProperty = BindableProperty.Create(nameof(MenuItems), typeof(IEnumerable<LagoVista.Client.Core.ViewModels.MenuItem>), typeof(LagoVistaContentPage), default(IEnumerable<LagoVista.Client.Core.ViewModels.MenuItem>),
            BindingMode.TwoWay, null, (view, oldValue, newValue) => (view as LagoVistaContentPage).MenuItems = (IEnumerable<LagoVista.Client.Core.ViewModels.MenuItem>)newValue, null, null, (view) => { return false; });

        public IEnumerable<LagoVista.Client.Core.ViewModels.MenuItem> MenuItems
        {
            get { return (IEnumerable<LagoVista.Client.Core.ViewModels.MenuItem>)GetValue(MenuItemsProperty); }
            set { SetValue(MenuItemsProperty, value); }
        }
        #endregion

        protected async override void OnAppearing()
        {
            _originalcontent = this.Content;

            base.OnAppearing();

            if (!_hasAppeared)
            {
                if (ViewModel != null)
                {
                    ViewModel.Logger.AddCustomEvent(LagoVista.Core.PlatformSupport.LogLevel.Message, ViewModel.GetType().Name, "Appeared");
                    if (this.Content != null)
                    {
                        this.Content.BindingContext = ViewModel;
                    }

                    await ViewModel.InitAsync();
                }
            }
            else
            {
                if (ViewModel != null)
                {
                    ViewModel.Logger.AddCustomEvent(LagoVista.Core.PlatformSupport.LogLevel.Message, ViewModel.GetType().Name, "Re-appeared");
                    await ViewModel.ReloadedAsync();
                }
            }

            _hasAppeared = true;
        }


        protected async override void OnDisappearing()
        {
            _menu.IsVisible = false;
            _menu.TranslationX = -MENU_WIDTH;

            base.OnDisappearing();

            if (ViewModel != null)
            {
                ViewModel.Logger.AddCustomEvent(LagoVista.Core.PlatformSupport.LogLevel.Message, ViewModel.GetType().Name, "Disappeared");
                await ViewModel.IsClosingAsync();
            }
        }
    }
}