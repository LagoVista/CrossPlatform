﻿using LagoVista.Client.Core.Auth;
using LagoVista.Client.Core.Models;
using LagoVista.Client.Core.Net;
using LagoVista.Core.Authentication.Interfaces;
using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.Core.Networking.Interfaces;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Resources;

[assembly: NeutralResourcesLanguage("en-US")]

namespace LagoVista.Client.Core
{
    public static class Startup
    {
        public static void Init(ServerInfo serverInfo)
        {
            SLWIOC.RegisterSingleton<ServerInfo>(serverInfo);
            SLWIOC.RegisterSingleton<IAuthClient>(new AuthClient());
            SLWIOC.RegisterSingleton<ITokenManager, TokenManager>();
            SLWIOC.RegisterSingleton<IAuthManager, AuthManager>();

            var client = new HttpClient();
            client.BaseAddress = serverInfo.BaseAddress;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            SLWIOC.RegisterSingleton<HttpClient>(client);

            SLWIOC.RegisterSingleton<IRestClient, RawRestClient>();
        }
    }
}
