using LagoVista.Client.Core.Net;
using LagoVista.Core.IOC;
using LagoVista.Core.PlatformSupport;
using LagoVista.XPlat.Core.Resources;
using LagoVista.XPlat.UWP.Network;
using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace LagoVista.XPlat.Sample.UWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private const string MOBILE_CENTER_KEY = "f4def9b7-eb9f-465f-a474-1956ab779936";

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

     
            this.UnhandledException += App_UnhandledException;
        }

        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine("============================================");
            Debug.WriteLine("Unhandled Exception");
            Debug.WriteLine(e.Message);
            Debug.WriteLine(e.Exception.Message);
            Debug.WriteLine(e.Exception.StackTrace);
            Debug.WriteLine("============================================");

            throw e.Exception;
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.Protocol)
            {
                var logger = SLWIOC.Get<ILogger>();

                var protocolActivatedEventArgs = (args as ProtocolActivatedEventArgs);
                if (protocolActivatedEventArgs == null)
                {
                    logger.AddCustomEvent(LogLevel.Error, "App_OnActivated", "EventArgs Not ProtocolActivatedEventArgs", new System.Collections.Generic.KeyValuePair<string, string>("type", args.GetType().Name));
                }
                else
                {
                    logger.AddCustomEvent(LogLevel.Message, "App_OnActivated", "URI App Activation", new System.Collections.Generic.KeyValuePair<string, string>("uri", protocolActivatedEventArgs.Uri.ToString()));
                    LagoVista.XPlat.Sample.App.Instance.HandleURIActivation(protocolActivatedEventArgs.Uri);
                }
            }

            base.OnActivated(args);
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;
                Xamarin.Forms.Forms.SetFlags("Shapes_Experimental");
                Xamarin.Forms.Forms.Init(e);

                SLWIOC.Register<IWebSocket, WebSocket>();
                LagoVista.Core.UWP.Startup.Init(this, rootFrame.Dispatcher, MOBILE_CENTER_KEY);

                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }

                Window.Current.Activate();
            }
        }

        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
