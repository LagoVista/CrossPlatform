using LagoVista.Core.Networking.Interfaces;
using LagoVista.Core.PlatformSupport;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LagoVista.Core.Interfaces;
using LagoVista.Client.Core.Models;
using System.Net.Http.Headers;
using System.Text;
using LagoVista.Core.Validation;
using LagoVista.Core.Authentication.Interfaces;
using LagoVista.Core.Authentication.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using LagoVista.Client.Core.Resources;

namespace LagoVista.Client.Core.Net
{
    /*
     * 100% of all authenticated calls to the server with go through this singleton, idea is that if
     * a refresh token is upodated and two calls happen at the same time with the same refresh token
     * only one will succeed and the user will be locked out since the refresh tokens are single use.
     */
    public class RawRestClient : IRestClient
    {
        HttpClient _httpClient;
        IAuthManager _authManager;
        ILogger _logger;
        IAuthClient _authClient;
        IDeviceInfo _deviceInfo;
        IAppConfig _appConfig;
        SemaphoreSlim _callSemaphore;

        public RawRestClient(HttpClient httpClient, IDeviceInfo deviceInfo, IAppConfig appConfig, IAuthClient authClient, IAuthManager authManager, ILogger logger)
        {
            _httpClient = httpClient;
            _authClient = authClient;
            _deviceInfo = deviceInfo;
            _authManager = authManager;
            _logger = logger;
            _appConfig = appConfig;
            _callSemaphore = new SemaphoreSlim(1);
        }

        private async Task<InvokeResult> RenewRefreshToken()
        {
            var authRequest = new AuthRequest();
            authRequest.AppId = _appConfig.AppId;
            authRequest.ClientType = "mobileapp";
            authRequest.DeviceId = _deviceInfo.DeviceUniqueId;
            authRequest.AppInstanceId = _authManager.AppInstanceId;
            authRequest.GrantType = "refreshtoken";
            authRequest.UserName = _authManager.User.Email;
            authRequest.Email = _authManager.User.Email;
            authRequest.RefreshToken = _authManager.RefreshToken;

            var response = await _authClient.LoginAsync(authRequest);
            if (response.Successful)
            {
                _authManager.AccessToken = response.Result.AccessToken;
                _authManager.AccessTokenExpirationUTC = response.Result.AccessTokenExpiresUTC;
                _authManager.RefreshToken = response.Result.RefreshToken;
                _authManager.AppInstanceId = response.Result.AppInstanceId;
                _authManager.RefreshTokenExpirationUTC = response.Result.RefreshTokenExpiresUTC;
                _logger.AddCustomEvent(LogLevel.Message, "RawRestClient_RenewRefreshTokenAsync", "Access Token Renewed with Refresh Token");
                await _authManager.PersistAsync();
                return InvokeResult.Success;
            }
            else
            {
                _logger.AddCustomEvent(LogLevel.Error, "RawRestClient_RenewRefreshTokenAsync", "Could Not Renew Access Token", response.ErrorsToKVPArray());
                var result = new InvokeResult();
                result.Concat(response);
                throw new Exceptions.CouldNotRenewTokenException();
            }
        }

        public async Task<RawResponse> PerformCall(Func<Task<HttpResponseMessage>> call, CancellationTokenSource cancellationTokenSource)
        {
            await _callSemaphore.WaitAsync();
            var retry = true;

            var rawResponse = RawResponse.FromNotCompleted();

            while (retry)
            {
                _httpClient.DefaultRequestHeaders.Clear();
                if (_authManager.IsAuthenticated)
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authManager.AccessToken);
                }

                retry = false;
                try
                {

                    var start = DateTime.Now;
                    var response = await call();
                    var delta = DateTime.Now - start;

                    
                    if (response.IsSuccessStatusCode)
                    {
                        rawResponse = RawResponse.FromSuccess(await response.Content.ReadAsStringAsync());                        
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        _logger.AddCustomEvent(LogLevel.Error, "RawRestClient_PerformCall", "401 From Server");
                        retry = ((await RenewRefreshToken()).Successful);
                        if(!retry)
                        {
                            rawResponse = RawResponse.FromNotAuthorized();
                        }
                    }
                    else
                    {
                        _logger.AddCustomEvent(LogLevel.Message, "RawRestClient_PerformCall", $"Http Error {(int)response.StatusCode}");
                        /* Check for 401 (I think, if so then attempt to get a new access token,  */
                        rawResponse = RawResponse.FromHttpFault((int)response.StatusCode, $"{ClientResources.Err_GeneralErrorCallingServer} : HTTP{(int)response.StatusCode} - {response.ReasonPhrase}");
                    }

                }
                catch(Exceptions.CouldNotRenewTokenException)
                {
                    _callSemaphore.Release();
                    throw;
                }
                catch (TaskCanceledException tce)
                {
                    _logger.AddException("RawRestClient_PerformCall_TaskCancelled", tce);
                    rawResponse = RawResponse.FromException(tce, tce.CancellationToken.IsCancellationRequested);
                }
                catch (Exception ex)
                {
                    _logger.AddException("RawRestClient_PerformCall", ex);
                    rawResponse = RawResponse.FromException(ex);
                }
            }

            _callSemaphore.Release();

            return rawResponse;
        }

        public Task<RawResponse> GetAsync(string path, CancellationTokenSource cancellationTokenSource)
        {
            return PerformCall(async() =>
            {
                var timedEvent = _logger.StartTimedEvent("RawRestClient_Get", path);
                var result = await  _httpClient.GetAsync(path, cancellationTokenSource.Token);
                _logger.EndTimedEvent(timedEvent);
                return result;
            }, cancellationTokenSource);
        }

        public Task<RawResponse> PostAsync(string path, string payload, CancellationTokenSource cancellationTokenSource)
        {
            return PerformCall(async() =>
            {
                var timedEvent = _logger.StartTimedEvent("RawRestClient_Post", path);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var result  = await _httpClient.PostAsync(path, content, cancellationTokenSource.Token);
                _logger.EndTimedEvent(timedEvent);
                return result;
            }, cancellationTokenSource);
        }

        public Task<RawResponse> PutAsync(string path, string payload, CancellationTokenSource cancellationTokenSource)
        {
            return PerformCall(async () =>
            {
                var timedEvent = _logger.StartTimedEvent("RawRestClient_Put", path);
                var content = new StringContent(payload, Encoding.UTF8, "application/json");
                var result = await _httpClient.PutAsync(path, content, cancellationTokenSource.Token);
                _logger.EndTimedEvent(timedEvent);
                return result;
            }, cancellationTokenSource);
        }

        public Task<RawResponse> DeleteAsync(string path, CancellationTokenSource cancellationTokenSource = null)
        {
            return PerformCall(async () =>
            {
                if (cancellationTokenSource == null) cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

                var timedEvent = _logger.StartTimedEvent("RawRestClient_Delete", path);
                var result =  await _httpClient.DeleteAsync(path, cancellationTokenSource.Token);
                _logger.EndTimedEvent(timedEvent);
                return result;
            }, cancellationTokenSource);
        }

        public async Task<InvokeResult> PostAsync<TModel>(string path, TModel model, CancellationTokenSource cancellationTokenSource = null) where TModel : class
        {
            if (cancellationTokenSource == null) cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            var json = JsonConvert.SerializeObject(model, new Newtonsoft.Json.JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver(), });
            var response = await PostAsync(path, json, cancellationTokenSource);
            return response.ToInvokeResult();
        }

        public async Task<InvokeResult<TResponseModel>> PostAsync<TModel, TResponseModel>(string path, TModel model, CancellationTokenSource cancellationTokenSource = null) where TModel : class where TResponseModel : class
        {
            if (cancellationTokenSource == null) cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            var json = JsonConvert.SerializeObject(model, new Newtonsoft.Json.JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver(), });
            var response = await PostAsync(path, json, cancellationTokenSource);
            if (response.Success)
            {
                return JsonConvert.DeserializeObject<InvokeResult<TResponseModel>>(response.Content);
            }
            else
            {
                return response.ToInvokeResult<TResponseModel>();
            }
        }

        public async Task<InvokeResult<TResponseModel>> GetAsync<TResponseModel>(string path, CancellationTokenSource cancellationTokenSource = null) where TResponseModel : class
        {
            if (cancellationTokenSource == null) cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            var response = await GetAsync(path, cancellationTokenSource);
            return response.ToInvokeResult<TResponseModel>();
        }
    }
}
