﻿using UIKit;
using LagoVista.XPlat.iOS.Services;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core;
using LagoVista.Core.IOC;
using LagoVista.XPlat.Core.Services;
using LagoVista.Client.Core.Net;
using LagoVista.Client.Core.Auth;
using LagoVista.Client;
using LagoVista.Client.Core;
using LagoVista.Client.Core.Interfaces;
using LagoVista.Client.Core.Models;

namespace LagoVista.XPlat.iOS
{
    public static class Startup
    {
        public static void Init(UIApplication app, string key)
        {
            var dispatcherService = new DispatcherService(app);
            var popupService = new LagoVista.XPlat.Core.Services.PopupServices();

            SLWIOC.RegisterSingleton<ILogger>(new Loggers.AppCenterLogger(key));
            SLWIOC.Register<IStorageService>(new StorageService());
            SLWIOC.Register<INetworkService>(new NetworkService());
            SLWIOC.RegisterSingleton<ISecureStorage>(new SecureStorage());
            SLWIOC.Register<IPopupServices>(new LagoVista.XPlat.Core.Services.PopupServices());
            SLWIOC.Register<ITCPClient, Services.TCPClient>();

            SLWIOC.RegisterSingleton<IGATTConnection>(new GATTConnection(dispatcherService));
            SLWIOC.Get<IGATTConnection>().RegisterKnownServices(NuvIoTGATTProfile.GetNuvIoTGATT().Services);

            SLWIOC.Register<IUDPClient, Services.UDPClient>();
            SLWIOC.Register<IWebSocket, Services.WebSocket>();
            SLWIOC.Register<IClipBoard, Services.ClipBoard>();
            SLWIOC.Register<IDispatcherServices>(new DispatcherService(app));

            IconFonts.IconFontSupport.RegisterFonts();
        }
    }
}