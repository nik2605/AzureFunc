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
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Quest.MMD.FunctionModels.ResourceProcessing;

namespace Azure_Functions
{
    public abstract class HttpTriggerJobStartBase<T>: HttpCustomerTriggerBase<T> where T : ICustomerRequest
    {
        protected IJobStatistic Statistics { get; private set; }
        protected Guid JobId { get; private set; }

        public HttpResponseMessage StartJob(HttpRequestMessage req, IJobRequestDescriptor jobDescr, CloudTable statisticTable, CloudBlobContainer blobContainer, CloudQueue progressQueue, ODMTraceWriter log)
        {
            JobId = jobDescr != null && jobDescr.JobId != Guid.Empty ? jobDescr.JobId : Guid.NewGuid();
            Statistics = new StorageJobStatistics(JobId, CloudTableProvider<StatisticsEntity>.Create(statisticTable),
                CloudBlobContainerProvider.Create(blobContainer), progressQueue)
            {
                Log = log
            };

            return Handle(req, log);
        }

        protected sealed override HttpResponseMessage Handler()
        {
            TrackJob(JobId.ToString());
            StartJobHandler();

            return Http.CreateResponse(HttpStatusCode.OK, new { JobId = JobId.ToString() });
        }

        protected abstract void StartJobHandler();
    }
}
