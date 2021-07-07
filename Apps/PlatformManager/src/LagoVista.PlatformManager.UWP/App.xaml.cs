using LagoVista.Client.Core.Net;
using LagoVista.Core.IOC;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.UWP.Services;
using LagoVista.XPlat.UWP.Network;
using System;
using System.Diagnostics;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace LagoVista.PlatformManager.UWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /* Note this is a dev key, if you really wanna copy, feel free and have fun! :) */
        public const string MOBILE_CENTER_KEY = "33298eb8-4b6e-43fe-9130-b25d3e6fb2ac";

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            UnhandledException += (sender, e) =>
            {
                Debug.WriteLine("EXCPETION");
                Debug.WriteLine(e.Exception);
                Debug.WriteLine(e.Message);
                Debug.WriteLine(e.Exception.StackTrace);
                if (global::System.Diagnostics.Debugger.IsAttached) global::System.Diagnostics.Debugger.Break();
            };
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
                    LagoVista.PlatformManager.App.Instance.HandleURIActivation(protocolActivatedEventArgs.Uri);
                }
            }

            base.OnActivated(args);
        }

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Frame;

            if (rootFrame == null)
            {
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                Xamarin.Forms.Forms.Init(e);

                SLWIOC.Register<IWebSocket, WebSocket>();

                LagoVista.Core.UWP.Startup.Init(this, rootFrame.Dispatcher, MOBILE_CENTER_KEY);               

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated) { }

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


        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
