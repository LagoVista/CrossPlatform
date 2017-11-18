using LagoVista.Core.Networking.Interfaces;
using LagoVista.Core.Networking.Models;
using LagoVista.Core.PlatformSupport;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Core.UWP.Services
{
    public class RestMessageHandler
    {
        public RestMessageHandler()
        {
            Handlers = new List<IApiHandler>();
        }

        public ILogger Logger { get; set; }

        public List<IApiHandler> Handlers { get; private set; }

        public async Task<bool> HandleMessage(HttpRequestMessage msg)
        {
            foreach (var handler in Handlers)
            {
                var methods = handler.GetType().GetMethods();
                foreach (var method in methods)
                {
                    var route = msg.Path.ToLower();
                    String methodRouteName = "/" + method.Name.ToLower();

                    Debug.WriteLine(methodRouteName + " " + msg.Path);

                    List<Object> _argValues = new List<Object>();
                    _argValues.Add(msg);


                    var methodAttr = method.GetCustomAttribute(typeof(MethodHandlerAttribute)) as MethodHandlerAttribute;
                    if (methodAttr != null)
                    {
                        if (msg.Method.ToUpper() == methodAttr.MethodType.ToString().ToUpper())
                        {
                            //If we supplied a path for the method, use that rather than the name of the method (default)
                            if (!String.IsNullOrEmpty(methodAttr.FullPath))
                            {
                                var argIndex = 1; /* Offset by one, first parameter is the http message */
                                var routePartIndex = 0;
                                var pathParts = methodAttr.FullPath.TrimStart('/').Split('/');
                                var routeParts = route.TrimStart('/').Split('/');
                                var methodParameters = method.GetParameters();

                                route = String.Empty;
                                methodRouteName = String.Empty;

                                foreach (var part in pathParts)
                                {
                                    if (routePartIndex < routeParts.Length)
                                    {
                                        if (argIndex < methodParameters.Length)
                                        {
                                            if (part.StartsWith("{"))
                                            {
                                                var param = methodParameters[argIndex++];

                                                try
                                                {
                                                    switch (param.ParameterType.Name)
                                                    {
                                                        case "String": _argValues.Add(routeParts[routePartIndex++].ToString()); break;
                                                        case "Int32": _argValues.Add(Convert.ToInt32(routeParts[routePartIndex++])); break;
                                                        case "Guid": _argValues.Add(new Guid(routeParts[routePartIndex++])); break;
                                                        default: _argValues.Add(routeParts[routePartIndex++].ToString()); break;
                                                    }
                                                }
                                                catch (Exception)
                                                {
                                                    route = String.Empty;
                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                methodRouteName += String.Format("/{0}", part.ToLower());
                                                route += String.Format("/{0}", routeParts[routePartIndex++].ToLower());
                                            }
                                        }
                                        else
                                        {
                                            methodRouteName += String.Format("/{0}", part.ToLower());
                                            route += String.Format("/{0}", routeParts[routePartIndex++].ToLower());
                                        }
                                    }
                                }

                                /*If we are doing a message with a body, and the values from the query path are one less than the 
                                  method parameter list, assume the last parameter is object and attempt to deserialize. */
                                if (methodAttr.MethodType == MethodHandlerAttribute.MethodTypes.POST &&
                                    _argValues.Count == methodParameters.Count() - 1)
                                {
                                    var contentArgType = methodParameters.Last();
                                    try
                                    {
                                        _argValues.Add(JsonConvert.DeserializeObject(msg.Body, contentArgType.ParameterType));
                                    }
                                    catch(Exception ex)
                                    {
                                        Debug.WriteLine(ex.Message);
                                        route = String.Empty;
                                        continue;
                                    }
                                }

                                if (_argValues.Count != methodParameters.Count())
                                    continue;
                            }

                            if (!String.IsNullOrEmpty(route) && methodRouteName == route)
                            {
                                try
                                {
                                    var responseMessage = (HttpResponseMessage)method.Invoke(handler, _argValues.ToArray());
                                    if (responseMessage != null)
                                    {
                                        await responseMessage.Send();
                                        return true;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    if (ex.InnerException != null)
                                        ex = ex.InnerException;

                                    var errorMessage = msg.GetErrorMessage(ex);
                                    await errorMessage.Send();

                                }
                            }
                        }
                    }
                }
            }
            /* not happy about not understanding the code above...we need to await on the send method */
            await Task.Delay(1);
            return false;
        }
    }
}
