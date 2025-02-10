using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using API.Models.Auth;
using Impactly.Test.IntegrationTests.Models;
using Xunit;
using Authorization = API.Models.Auth.Authorization;

namespace Impactly.Test.IntegrationTests.Client
{
    public static class AuthExtension
    {
        public const string AdminEmail = "admin@innosocial.dk";
        public const string AdminPassword = "Password1";
        public const string AdminId = "admin";
        public const string AdminProjectId = "admin";
        public const string AdminProjectName = "Test";


        public static async Task SignIn(this HttpClient client, string email, string password, string projectId)
        {
            await SignInWithEmail(client, new SignInWithEmailRequest()
            {
                Email = email,
                Password = password,
            }).ConfigureAwait(false);
            await SignInWithProject(client, projectId).ConfigureAwait(false);
        }
        
        public static async Task SignInProject(this HttpClient client)
        {
            await SignInWithEmail(client, new SignInWithEmailRequest
            {
                Email = AdminEmail,
                Password = AdminPassword
            }).ConfigureAwait(false);
            await SignInWithProject(client, AdminProjectId).ConfigureAwait(false);
        }

        public static async Task SignInAdmin(this HttpClient client)
        {
            await SignInWithEmail(client, new SignInWithEmailRequest
            {
                Email = AdminEmail,
                Password = AdminPassword
            }).ConfigureAwait(false);
            await SignInWithAdmin(client).ConfigureAwait(false);
        }

        public static async Task<Authorization> SignInAsAdmin(this HttpClient client, SignInWithEmailRequest loginRequest)
        {
            var res = await SignInWithEmail(client,loginRequest).ConfigureAwait(false);
            Assert.NotNull(res);
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            var auth = await client.Fetch<Authorization>(HttpMethod.Post, $"/api/web/v1/auth/admins/{res.Value.User.Id}").ConfigureAwait(false);
            if (auth.IsSuccessStatusCode)
            {
                client.SetAuthorization(auth.Value);
            }

            return auth.Value;
        }

        public static async Task<Authorization> SignInAsProject(this HttpClient client, SignInWithEmailRequest loginRequest, string projectId)
        {

            var res = await SignInWithEmail(client, loginRequest).ConfigureAwait(false);
            Assert.NotNull(res);
            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
            var response =
                await client.Fetch<SignInResponse>(HttpMethod.Post, $"/api/web/v1/auth/projects/{projectId}").ConfigureAwait(false);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            client.SetAuthorization(response.Value.Authorization);
            return response.Value.Authorization;
        }
        
        
        public static void SignOut(this HttpClient client)
        {
            client.SetAuthorization(null);
        }

        public static async Task<RestResponse<SignInResponse>> SignInWithEmail(this HttpClient client,
            SignInWithEmailRequest request)
        {
            var response = await client.Fetch<SignInResponse>(HttpMethod.Post, "/api/web/v1/auth", request).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode) return response;

            client.SetAuthorization(response.Value.Authorization);

            return response;
        }

        public static async Task<RestResponse<Authorization>> SignInWithAdmin(this HttpClient client)
        {
            var response = await client.Fetch<Authorization>(HttpMethod.Post, $"/api/web/v1/auth/admins/{AdminId}");

            if (response.IsSuccessStatusCode)
            {
                client.SetAuthorization(response.Value);
            }

            return response;
        }

        public static async Task<RestResponse<SignInResponse>> SignInWithProject(this HttpClient client,
            string projectId)
        {
            var response =
                await client.Fetch<SignInResponse>(HttpMethod.Post, $"/api/web/v1/auth/projects/{projectId}");

            if (response.IsSuccessStatusCode)
            {
                client.SetAuthorization(response.Value.Authorization);
            }

            return response;
        }

        public static async Task<RestResponse<AuthUser>> Refresh(this HttpClient client,
            RefreshAuthorizationRequest request)
        {
            return await client.Fetch<AuthUser>(HttpMethod.Post, "/api/web/v1/auth/refresh", request);
        }

        public static async Task<RestResponse<object>> ForgotPassword(this HttpClient client,
            ForgotPasswordRequest request)
        {
            return await client.Fetch<object>(HttpMethod.Post, "/api/web/v1/auth/forgot-password", request);
        }

        public static async Task<RestResponse<AuthUser>> ResetPassword(this HttpClient client,
            ResetPasswordRequest request, string AccessToken)
        {
            return await client.Fetch<AuthUser>(HttpMethod.Post, "/api/web/v1/auth/reset-password", request,
                new Dictionary<string, string> {{"Authorization", $"Bearer {AccessToken}"}}).ConfigureAwait(false);
        }
    }
}