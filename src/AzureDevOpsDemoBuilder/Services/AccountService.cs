using AzureDevOpsDemoBuilder.Models;
using AzureDevOpsDemoBuilder.ServiceInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;

namespace AzureDevOpsDemoBuilder.Services
{
    public class AccountService : IAccountService
    {
        public IConfiguration AppKeyConfiguration { get; }

        private ILogger<AccountService> logger;

        public AccountService(IConfiguration configuration, ILogger<AccountService> _logger)
        {
            AppKeyConfiguration = configuration;
            logger = _logger;

        }
        /// <summary>
        /// Formatting the request for OAuth
        /// </summary>
        /// <param name="appSecret"></param>
        /// <param name="authCode"></param>
        /// <param name="callbackUrl"></param>
        /// <returns></returns>
        public string GenerateRequestPostData(string appSecret, string authCode, string callbackUrl)
        {
            try
            {
                return String.Format("client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion={0}&grant_type=urn:ietf:params:oauth:grant-type:jwt-bearer&assertion={1}&redirect_uri={2}",
                            HttpUtility.UrlEncode(appSecret),
                            HttpUtility.UrlEncode(authCode),
                            callbackUrl
                     );
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                //ViewBag.ErrorMessage = ex.Message;
            }
            return string.Empty;
        }

        /// <summary>
        /// Generate Access Token
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public AccessDetails GetAccessToken(string body)
        {
            try
            {
                string baseAddress = "https://app.vssps.visualstudio.com";
                var client = new HttpClient
                {
                    BaseAddress = new Uri(baseAddress)
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "/oauth2/token");

                var requestContent = body;
                request.Content = new StringContent(requestContent, Encoding.UTF8, "application/x-www-form-urlencoded");

                var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    AccessDetails details = Newtonsoft.Json.JsonConvert.DeserializeObject<AccessDetails>(result);
                    return details;
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                //ViewBag.ErrorMessage = ex.Message;
            }
            return new AccessDetails();
        }

        /// <summary>
        /// Get Profile details
        /// </summary>
        /// <param name="accessDetails"></param>
        /// <returns></returns>
        public ProfileDetails GetProfile(AccessDetails accessDetails)
        {
            ProfileDetails profile = new ProfileDetails();
            using (var client = new HttpClient())
            {
                try
                {
                    string baseAddress = "https://app.vssps.visualstudio.com";

                    client.BaseAddress = new Uri(baseAddress);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessDetails.access_token);
                    HttpResponseMessage response = client.GetAsync("_apis/profile/profiles/me?api-version=4.1").Result;
                    if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        profile = JsonConvert.DeserializeObject<ProfileDetails>(result);
                        return profile;
                    }
                    else
                    {
                        var errorMessage = response.Content.ReadAsStringAsync();
                        logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t Get Profile :" + errorMessage + "\n");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                }
                return profile;
            }
        }


        /// <summary>
        /// Refresh access token
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        public AccessDetails Refresh_AccessToken(string refreshToken)
        {
            using (var client = new HttpClient())
            {
                string redirectUri = "https://gpactazuredevopsonboardingsimulator.azurewebsites.net/Environment/Create";
                string cientSecret = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6Im9PdmN6NU1fN3AtSGpJS2xGWHo5M3VfVjBabyJ9.eyJjaWQiOiJhODhlMTBhMS05NGZiLTQ5NTAtOGRhNS0zOGM5OWU4NGRhZWQiLCJjc2kiOiJiZjQyYzBkYS00YTFjLTQ5YzYtYWM1YS0xZGU1MjFiMTdlNTEiLCJuYW1laWQiOiJiNGU2YTA5MC0zN2E4LTZmMjktYTVmYi1hODFhYTFhNzE2NGYiLCJpc3MiOiJhcHAudnN0b2tlbi52aXN1YWxzdHVkaW8uY29tIiwiYXVkIjoiYXBwLnZzdG9rZW4udmlzdWFsc3R1ZGlvLmNvbSIsIm5iZiI6MTYyNzM3NjI1MSwiZXhwIjoxNzg1MTQyNjUxfQ.3WLbkE6WrHagrhT-mTLEv9e1N6RUn5D0Wk6GMUd5TlmaAb-4tC7eRY3i175AdNTwCy9am4Sziw3JU0CZBi2s_kfFsCn8SLOq0ecYhz29G8TUX6KQYiOPCteBp9v1VWw1vGN5I5Rm7uDk1ULOS91k_nxNgvNazsWka2oGZ9pAQzrVwwdfsKV-KEJeewWhWGqInJrMQzSXKTU4UZ3j5rLV6FBg8lsLQMuvGc0N39HfcUj8hjXsOPp6CaOLbynBsPC5AIqqTHJtZmkef57EWJ0HAyQWm2QJwV-g8Rsmi1C4nm36qYm2jXSihjj-B0gxC--JXPIsMMxzxRD07keXpSQGSQ";
                string baseAddress = "https://app.vssps.visualstudio.com";

                var request = new HttpRequestMessage(HttpMethod.Post, baseAddress + "/oauth2/token");
                var requestContent = string.Format(
                    "client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer&client_assertion={0}&grant_type=refresh_token&assertion={1}&redirect_uri={2}",
                    HttpUtility.UrlEncode(cientSecret),
                    HttpUtility.UrlEncode(refreshToken), redirectUri
                    );

                request.Content = new StringContent(requestContent, Encoding.UTF8, "application/x-www-form-urlencoded");
                try
                {
                    var response = client.SendAsync(request).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string result = response.Content.ReadAsStringAsync().Result;
                        AccessDetails accesDetails = JsonConvert.DeserializeObject<AccessDetails>(result);
                        return accesDetails;
                    }
                    else
                    {
                        return new AccessDetails();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
                    return new AccessDetails();
                }
            }
        }

        /// <summary>
        /// Get list of accounts
        /// </summary>
        /// <param name="memberID"></param>
        /// <param name="details"></param>
        /// <returns></returns>
        public AccountsResponse.AccountList GetAccounts(string memberID, AccessDetails details)
        {
            AccountsResponse.AccountList accounts = new AccountsResponse.AccountList();
            var client = new HttpClient();
            string baseAddress = "https://app.vssps.visualstudio.com";

            string requestContent = baseAddress + "/_apis/Accounts?memberId=" + memberID + "&api-version=4.1";
            var request = new HttpRequestMessage(HttpMethod.Get, requestContent);
            request.Headers.Add("Authorization", "Bearer " + details.access_token);
            try
            {
                var response = client.SendAsync(request).Result;
                if (response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.OK)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    accounts = JsonConvert.DeserializeObject<Models.AccountsResponse.AccountList>(result);
                    return accounts;
                }
                else
                {
                    var errorMessage = response.Content.ReadAsStringAsync();
                    logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t Get Accounts :" + errorMessage + "\t" + "\n");
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + "\t" + ex.Message + "\t" + "\n" + ex.StackTrace + "\n");
            }
            return accounts;
        }

    }
}