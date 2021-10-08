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

using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs.Host;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Concurrent;
using Quest.SystemLayer;
using Quest.SystemLayer.Interfaces;

namespace Azure_Functions
{
    public class InvalidInputException : Exception
    {
        public InvalidInputException(string message) :
            base(message)
        {
        }
    }

    public abstract class HttpTriggerBase
    {
        public string TriggerName => GetType().Name;
        private static ConcurrentDictionary<Type, string[]> _AllowedRoles = new ConcurrentDictionary<Type, string[]>();
        private static ConcurrentDictionary<Type, string[]> _AllowedModuleSubscriptions = new ConcurrentDictionary<Type, string[]>();
        private static ConcurrentDictionary<Type, AccessAllowAllAttribute> _AllowedAll = new ConcurrentDictionary<Type, AccessAllowAllAttribute>();

        protected ODMTraceWriter Log { get; private set; }

        public static ODMTraceWriter GetODMTraceWriter(TraceWriter existingWriter, string operation_ParentId = null, string operation_Id = null)
        {
            return ODMTraceWriter.Create(existingWriter, null, operation_ParentId, operation_Id);
        }

        public HttpResponseMessage Handle(HttpRequestMessage req, ODMTraceWriter log)
        {
            Log = log;
            Log.Operation_Name = TriggerName;
            Log.StartRequest(TriggerName);
            HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
            try
            {
                var rv = ExecuteWithAccessCheck(req);
                statusCode = rv.StatusCode;
                return rv;
            }
            finally
            {
                Log.StopRequest(((int)statusCode).ToString(), statusCode < HttpStatusCode.InternalServerError);
            }
        }

        private HttpResponseMessage ExecuteWithAccessCheck(HttpRequestMessage req)
        {
            var allowedRoles = _AllowedRoles.GetOrAdd(GetType(),
                x => x.GetCustomAttributes<AccessAllowRoleAttribute>().Select(r => r.Role).ToArray());
            if (allowedRoles.Length > 0)
            {
                var validator = AuthenticationFactory.CreateAzureValidator(req);
                validator.Log = Log;

                if (!validator.CheckAllowed(allowedRoles))
                {
                    return validator.GenerateFailureResponse();
                }
            }

            var allowedModules = _AllowedModuleSubscriptions.GetOrAdd(GetType(),
                x => x.GetCustomAttributes<AccessAllowModuleAttribute>().Select(r => r.Module).ToArray());
            if (allowedModules.Length > 0)
            {
                var validator = AuthenticationFactory.CreateXcloudValidator(req);
                if (!validator.CheckAllowed(allowedModules))
                {
                    return validator.GenerateFailureResponse();
                }
            }

            if (allowedRoles.Length == 0 &&
                allowedModules.Length == 0 &&
                _AllowedAll.GetOrAdd(GetType(), x => x.GetCustomAttributes<AccessAllowAllAttribute>().FirstOrDefault()) == null)
            {
                return new HttpResponseMessage(HttpStatusCode.Forbidden);
            }
            return InternalHandle();
        }
        private HttpResponseMessage InternalHandle()
        {
            try
            {
                return SafeInternalHandle();
            }
            catch (AggregateException e)
            {
                var message = string.Join(". ", e.InnerExceptions.Select(x => x.Message));
                Log.Error($"Error in API call. {message}", e, GetType().FullName);
                return Http.CreateErrorResponse(message);
            }
            catch (InvalidInputException e)
            {
                var message = e.Message;
                Log.Error($"Error in API call parameters. {message}", e, GetType().FullName);
                return Http.CreateErrorResponse(message, HttpStatusCode.BadRequest);
            }
            catch (ExpectedException e)
            {
                var message = e.Message;
                Log.Warning(message, GetType().FullName);
                return Http.CreateErrorResponse(message, HttpStatusCode.BadRequest);
            }
            catch (HttpExpectedException e)
            {
                var message = e.Message;
                Log.Warning(message, GetType().FullName);
                return Http.CreateErrorResponse(message, e.StatusCode, e.ErrorCode);
            }
            catch (Exception e)
            {
                var message = e.Message;
                Log.Error($"Error in API call. {message}", e, GetType().FullName);
                return Http.CreateErrorResponse(message);
            }
        }
        protected virtual HttpResponseMessage SafeInternalHandle()
        {
            return Handler();
        }

        protected virtual HttpResponseMessage Handler()
        {
            return Http.CreateResponse<object>(HttpStatusCode.OK);
        }

        protected void TrackCustomer(string customer)
        {
            Log.TrackExecutionInfo(customer);
        }
    }
}
