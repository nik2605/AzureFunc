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

using Functions.CorePusStart.Model;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Azure_Functions
{
    public class HttpTriggerPusBase<TReq> : HttpTriggerBase
    {
        protected StorageJobStatistics Statistics { get; private set; }
        protected Guid JobId { get; private set; }
        protected TReq Input { get; private set; }
        protected CorePusCustomerInfo CustomerInfo { get; private set; }
        protected string Thumbprint { get; private set; }
        private HttpRequestMessage Request { get; set; }
        private string RequestContent { get; set; }
        protected CustomerInfoTableProvider CustomerTableProvider { get; private set; }

        protected HttpResponseMessage CusHandle(HttpRequestMessage req, string thumbprint, CloudTable customerTable, CloudTable statisticTable, ODMTraceWriter log, string jobId = null)
        {
            try
            {
                JobId = jobId != null ? new Guid(jobId) : Guid.NewGuid();
                Statistics = new StorageJobStatistics(JobId, CloudTableProvider<StatisticsEntity>.Create(statisticTable), null) { Log = log };
                Thumbprint = thumbprint;
                Request = req;
                RequestContent = Request.Content == null ? string.Empty : Request.Content.ReadAsStringAsync().Result;
                Input = JsonConvert.DeserializeObject<TReq>(RequestContent);
                CustomerTableProvider = new CustomerInfoTableProvider(customerTable);
            }
            catch (Exception e)
            {
                log.Error($"Bad request due to exception: {e.Message}", e, GetType().FullName);
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }
            return Handle(req, log);
        }

        protected sealed override HttpResponseMessage SafeInternalHandle()
        {
            Log.Info($"Request with {Thumbprint} thumbprint.");
            CustomerInfo = CustomerTableProvider.Retrieve(Thumbprint.ToUpper());
            var errorReason = string.Empty;
            
            if (CustomerInfo == null)
            {
                errorReason = $"Certificate with thumbprint {Thumbprint} not found.";
            }
            else if (CustomerInfo.Deleted.GetValueOrDefault(false))
            {
                errorReason = $"Certificate with thumbprint {Thumbprint} was reissued.";
            }
            else if (!VerifySignature())
            {
                errorReason = "Invalid signature for valid certificate.";
            }

            if (!string.IsNullOrEmpty(errorReason))
            {
                Log.Warning($"Forbidden due: {errorReason}");
                return new HttpResponseMessage(HttpStatusCode.Forbidden);
            }

            Log.TrackExecutionInfo(CustomerInfo.CustomerId, JobId.ToString());
            return Handler();
        }

        private bool VerifySignature()
        {
            try
            {
                var date = Request.Headers.Date.GetValueOrDefault();

                if (date.UtcDateTime > DateTime.UtcNow - TimeSpan.FromMinutes(5)
                    && date.UtcDateTime < DateTime.UtcNow + TimeSpan.FromMinutes(5)
                    && Request.Headers.Authorization.Scheme.Equals("ODM"))
                {
                    var certificate = new X509Certificate2(Convert.FromBase64String(CustomerInfo.Certificate));
                    var csp = (RSACryptoServiceProvider)certificate.PublicKey.Key;

                    var requestUriString = string.Empty;
                    if (Request.Headers.TryGetValues("Function-App-Proxy", out var proxyUri))
                    {
                        requestUriString = proxyUri.Single();
                        Log.Info($"Function-App-Proxy: {requestUriString}");
                    }
                    else
                    {
                        requestUriString = Request.RequestUri.ToString();
                        Log.Info($"RequestUri: {requestUriString}");
                    }

                    var data = Encoding.Unicode.GetBytes($"{RequestContent.Trim()}:{requestUriString.ToLower().Trim()}:{date.ToUniversalTime():R}");

                    return csp.VerifyData(data, "SHA256", Convert.FromBase64String(Request.Headers.Authorization.Parameter));
                }
            }
            catch (Exception e)
            {
                Log.Error($"Forbidden due to exception: {e.Message}", e, GetType().FullName);
            }

            return false;
        }
    }
}
