﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OneLogin.Descriptors;
using OneLogin.Requests;
using OneLogin.Responses;

namespace OneLogin
{
    /// <summary>
    /// A client class to access the onelogin API /1
    /// </summary>
    [SourceDocumentation("https://developers.onelogin.com/api-docs/1/getting-started/dev-overview")]
    public partial class OneLoginClient
    {
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _region;
        private static HttpClient _client;
        private static readonly List<string> ValidRegions = new List<string> { "us", "eu" };

        /// <summary>
        /// Initializes a new instance of the <see cref="OneLoginClient"/> class.
        /// </summary>
        /// <param name="clientId">The client id to connect with.</param>
        /// <param name="clientSecret">The client secret to connect with.</param>
        /// <param name="region"></param>
        /// <example>
        /// <code>
        /// var client = new OneLoginClient("client id", "client secret")
        /// </code>
        /// </example>
        /// <exception cref="System.ArgumentNullException">clientId</exception>
        /// <exception cref="System.ArgumentNullException">clientSecret</exception>
        public OneLoginClient(string clientId, string clientSecret, string region = "us")
        {
            if (string.IsNullOrWhiteSpace(clientId)) throw new ArgumentNullException(nameof(clientSecret));
            if (string.IsNullOrWhiteSpace(clientSecret)) throw new ArgumentNullException(nameof(clientSecret));
            if (!ValidRegions.Contains(region)) throw new ArgumentException("Invalid region code", nameof(region));
            _clientId = clientId;
            _clientSecret = clientSecret;
            _region = region;
        }

        public async Task<HttpClient> GetClient()
        {
            if (_client != null)
            {
                return _client;
            }

            var token = await GenerateTokens();
            token.EnsureSuccess();
            var client = new HttpClient { BaseAddress = new Uri(Endpoints.BaseApi.Replace("<us_or_eu>", _region)) };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Data[0].AccessToken);
            return _client = client;
        }

        /// <summary>
        /// Generate an access token and refresh token that you can use to call our resource APIs.
        /// For an overview of the authorization flow, see Authorizing Resource API Calls.
        /// Once generated, an access token is valid for 10 hours.
        /// <a href="https://developers.onelogin.com/api-docs/1/oauth20-tokens/generate-tokens-2">Documentation</a>.
        /// </summary>
        /// <returns>Returns the serialized <see cref="GenerateTokensResponse"/> as an asynchronous operation.</returns>
        public async Task<GenerateTokensResponse> GenerateTokens()
        {
            var client = new HttpClient();

            var credentials = $"{_clientId}:{_clientSecret}";

            var base64EncodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedCredentials);

            var content = new StringContent(JsonConvert.SerializeObject(new { grant_type = "client_credentials" }));
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(Endpoints.Token.Replace("<us_or_eu>", _region)),
                Content = content
            };

            // We add the Content-Type Header like this because otherwise dotnet
            // adds the utf-8 charset extension to it which is not compatible with OneLogin
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var response = client.SendAsync(request);
            return await GetResponse<GenerateTokensResponse>(response);
        }


        /// <summary>
        /// Use to get a list of groups that are available in your account. The call returns up to 50 groups per page.
        /// </summary>
        /// <returns></returns>
        public async Task<GetGroupsResponse> GetGroups()
        {
            return await GetResource<GetGroupsResponse>(Endpoints.ONELOGIN_GROUPS);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">Set to the group’s ID with .xml appended. For example, 123456.xml. If you don’t know the group’s id, use the Get all groups API call to return all groups and their id values.</param>
        /// <returns></returns>
        //todo: onelogin documentation is wrong.
        public async Task<GetGroupsResponse> GetGroup(int id)
        {
            return await GetResource<GetGroupsResponse>($"{Endpoints.ONELOGIN_GROUPS}/{id}");
        }







        /// <summary>
        /// Send an invite link to a user that you have already created in your OneLogin account.
        /// </summary>
        /// <param name="email">Set to the email address of the user that you want to generate an invite link for.</param>
        /// <param name="personalEmail">If you want to send the invite email to an email other than the one provided in  email, provide it here. The invite link will be sent to this address instead.</param>
        /// <returns>The user can click the link to set his password and access your OneLogin portal.</returns>
        public async Task<EmptyResponse> SendInviteLink(string email, string personalEmail = null)
        {
            return await PostResource<EmptyResponse>($"{Endpoints.ONELOGIN_INVITES}/send_invite_link", new SendInviteLinkRequest { Email = email, personal_email = personalEmail});
        }


        public async Task<List<T>> GetNextPages<T>(T source, int? pages = null) where T : IPageable
        {
            var results = new List<T>();
            var isTrue = Uri.IsWellFormedUriString(source.Pagination.NextLink, UriKind.Absolute);
            var pageCount = 1;
            var nextLink = source.Pagination.NextLink;
            while (isTrue && pageCount <= pages)
            {
                var result = await GetResource<T>(nextLink);
                results.Add(result);
                nextLink = result.Pagination.NextLink;
                isTrue = Uri.IsWellFormedUriString(nextLink, UriKind.Absolute);
                pageCount++;
            }

            return results;
        }

        public async Task<List<T>> GetPreviousPages<T>(T source, int? pages = null) where T : IPageable
        {
            var results = new List<T>();
            var isTrue = Uri.IsWellFormedUriString(source.Pagination.PreviousLink, UriKind.Absolute);
            var pageCount = 1;
            while (isTrue && pageCount <= pages)
            {
                var result = await GetResource<T>(source.Pagination.PreviousLink);
                results.Add(result);
                isTrue = Uri.IsWellFormedUriString(result.Pagination.PreviousLink, UriKind.Absolute);
                pageCount++;
            }

            return results;
        }

        private async Task<T> GetResource<T>(string url)
        {
            var client = await GetClient();
            return await GetResponse<T>(client.GetAsync(url));
        }

        private async Task<T> PostResource<T>(string url, object request)
        {
            var content = new StringContent(JsonConvert.SerializeObject(request));
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Content = content
            };

            // We add the Content-Type Header like this because otherwise dotnet
            // adds the utf-8 charset extension to it which is not compatible with OneLogin
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var client = await GetClient();
            var response = client.SendAsync(httpRequest);
            return await GetResponse<T>(response);
        }

        private async Task<T> GetResponse<T>(Task<HttpResponseMessage> taskResponse)
        {
            var response = await taskResponse;
            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseBody);
        }
    }
}
