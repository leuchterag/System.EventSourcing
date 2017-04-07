using IdentityModel.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.EventSourcing.Authorization;

namespace System.EventSourcing.AspNetCore.Hosting.Authorization
{
    public static class AppBuilderExtensions
    {


        public static IApplicationBuilder UseImpersonationBearer(this IApplicationBuilder subject, string authority, string clientid, string clientsecret)
        {
            //    // Discovery
            //    var disco = DiscoveryClient.GetAsync(authority).Result;
            //    // create token client
            //    var client = new TokenClient(disco.TokenEndpoint, clientid, clientsecret);

            subject.Use(async (x, n) =>
            {
                if (x.Request.Headers.ContainsKey(AuthrorizationTags.Subscriber))
                {
                    var subscriberId = x.Request.Headers[AuthrorizationTags.Subscriber];

                    var payload = new
                    {
                        sub = subscriberId.ToString()
                    };

                    var disco = await DiscoveryClient.GetAsync(authority);

                    // create token client
                    var client = new TokenClient(disco.TokenEndpoint, clientid, clientsecret);

                    // send custom grant to token endpoint, return response
                    var grant = await client.RequestCustomGrantAsync("delegation", "api", payload);

                    if (grant.IsError)
                    {
                        x.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return;
                    }

                    x.Request.Headers.Add("Authorization", $"Bearer {grant.AccessToken}");
                    await n();
                }
            });
            return subject;
        }
    }
}
