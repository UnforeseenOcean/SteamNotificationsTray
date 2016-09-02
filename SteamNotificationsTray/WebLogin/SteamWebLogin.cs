﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Security.Cryptography;
using Newtonsoft.Json;
using SteamNotificationsTray.WebLogin.Models;

namespace SteamNotificationsTray.WebLogin
{
    class SteamWebLogin
    {
        static readonly string BaseDomain = "https://steamcommunity.com/";
        static readonly string Endpoint = BaseDomain + "login/";
        static readonly string GetRsaKeyPath = Endpoint + "getrsakey/";
        static readonly string DoLoginPath = Endpoint + "dologin/";
        static readonly string RefreshCaptchaPath = Endpoint + "refreshcaptcha/";
        static readonly string RenderCaptchaPath = Endpoint + "rendercaptcha/";
        // Not handling: account recovery stuff

        HttpClient client = new HttpClient();

        public async Task<GetRsaKeyResponse> GetRsaKeyAsync(string username)
        {
            var p = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("username", username)
            });
            HttpResponseMessage resp = await client.PostAsync(GetRsaKeyPath, p);
            if (resp.IsSuccessStatusCode)
            {
                string respText = await resp.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<GetRsaKeyResponse>(respText);
            }
            else
            {
                return null;
            }
        }

        public async Task<DoLoginResponse> DoLoginAsync(DoLoginRequest req)
        {
            var p = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("password", req.Password ?? string.Empty),
                new KeyValuePair<string, string>("username", req.Username ?? string.Empty),
                new KeyValuePair<string, string>("twofactorcode", req.TwoFactorCode ?? string.Empty),
                new KeyValuePair<string, string>("emailauth", req.EmailAuth ?? string.Empty),
                new KeyValuePair<string, string>("loginfriendlyname", req.LoginFriendlyName ?? string.Empty),
                new KeyValuePair<string, string>("captchagid", req.CaptchaGid.ToString()),
                new KeyValuePair<string, string>("captcha_text", req.CaptchaText ?? string.Empty),
                new KeyValuePair<string, string>("emailsteamid", req.EmailSteamId.HasValue ? req.EmailSteamId.Value.ToString() : string.Empty),
                new KeyValuePair<string, string>("rsatimestamp", req.RsaTimeStamp.ToString()),
                new KeyValuePair<string, string>("remember_login", req.RememberLogin.ToString()),
            });
            HttpResponseMessage resp = await client.PostAsync(DoLoginPath, p);
            if (resp.IsSuccessStatusCode)
            {
                string respText = await resp.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<DoLoginResponse>(respText);
            }
            else
            {
                return null;
            }
        }

        public async Task<long> RefreshCaptchaAsync()
        {
            var p = new FormUrlEncodedContent(new KeyValuePair<string, string>[] { });
            HttpResponseMessage resp = await client.PostAsync(RefreshCaptchaPath, p);
            if (resp.IsSuccessStatusCode)
            {
                string respText = await resp.Content.ReadAsStringAsync();
                var obj = Newtonsoft.Json.Linq.JObject.Parse(respText);
                return obj.Value<long>("gid");
            }
            else
            {
                return -1;
            }
        }

        public async Task<byte[]> RenderCaptchaAsync(long gid)
        {
            string query;
            using (var p = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("gid", gid.ToString())
            }))
            {
                query = await p.ReadAsStringAsync();
            }
            UriBuilder builder = new UriBuilder(RenderCaptchaPath);
            builder.Query = query;

            HttpResponseMessage resp = await client.GetAsync(builder.Uri);
            if (resp.IsSuccessStatusCode)
            {
                return await resp.Content.ReadAsByteArrayAsync();
            }
            else
            {
                return null;
            }
        }
    }
}
