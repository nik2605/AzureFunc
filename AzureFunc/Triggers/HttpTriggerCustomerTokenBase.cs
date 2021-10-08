// QUEST SOFTWARE PROPRIETARY INFORMATION
//
// This software is confidential. Quest Software Inc., or one of its
// subsidiaries, has supplied this software to you under terms of a
// license agreement, nondisclosure agreement or both.
//
// You may not copy, disclose, or use this software except in
// accordance with those terms.
//
// COPYRIGHT 2021 Quest Software Inc.
// ALL RIGHTS RESERVED.
//
// QUEST SOFTWARE MAKES NO REPRESENTATIONS OR
// WARRANTIES ABOUT THE SUITABILITY OF THE SOFTWARE,
// EITHER EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
// TO THE IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE, OR NON-INFRINGEMENT.
// QUEST SOFTWARE SHALL NOT BE LIABLE FOR ANY DAMAGES
// SUFFERED BY LICENSEE AS A RESULT OF USING, MODIFYING
// OR DISTRIBUTING THIS SOFTWARE OR ITS DERIVATIVES.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Host;

namespace Azure_Functions
{
    public class HttpTriggerCustomerTokenBase : HttpTriggerBase
    {
        private HttpRequestMessage Request { get; set; }
        protected string CustomerId { get; set; }
        protected string[] EffectiveSubscriptions;

        protected XCloudClient OnDemandClient { get; set; }
        protected HttpResponseMessage CustomerTokenHandle(HttpRequestMessage req, ODMTraceWriter log)
        {
            Request = req;
            return Handle(req, log);
        }

        protected sealed override HttpResponseMessage SafeInternalHandle()
        {
            try
            {
                var authToken = Request.Headers.Authorization.ToString();
                var tokenHandler = new JwtSecurityTokenHandler();
                var claims = tokenHandler.ReadJwtToken(authToken).Claims.ToList();

                var org  = claims.First(claim => claim.Type == XcloudClaims.Organization).Value.AsObject<XCloudClient.Organization>();
                CustomerId = org.organizationId;
                TrackCustomer(CustomerId);

                OnDemandClient = XCloudClient.Factory.Create(new TokenAuthHelper(authToken), CustomerId, Log);

                Log.Info($"Check module access. Customer: {org.deploymentRegion} {org.organizationId} {org.organizationDisplayName}");

                EffectiveSubscriptions = claims
                    .Where(claim => claim.Type == XcloudClaims.EffectiveSubscriptions)
                    .Select(claim => claim.Value)
                    .Where(s => s.StartsWith($"{XcloudModules.Migration}.")).ToArray();

                Log.Info($"EffectiveSubscriptions count: {EffectiveSubscriptions.Length}");

                if (EffectiveSubscriptions.All(s => XCloudClient.IsSubscriptionOfType(s, XCloudClient.SubscriptionTypes.Preview)))
                {
                    Log.Info("Access to module is denied");
                    return CreateAccessDeniedError();
                }

                return Handler();
            }
            catch (Exception e)
            {
                Log.Error($"Forbidden due to exception: {e.Message}", e, GetType().FullName);
                return CreateAccessDeniedError();
            }
        }

        private static HttpResponseMessage CreateAccessDeniedError()
        {
            var response = Http.CreateResponse(HttpStatusCode.Unauthorized, string.Empty);
            response.Headers.Add("Access-Control-Expose-Headers", "Location");
            response.Headers.Location = new Uri(ApplicationGlobals.Configuration.DeploymentConfig.XcloudUiUrl);
            return response;
        }
    }
}
