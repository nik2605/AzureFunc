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
using System.Diagnostics;
using Azure_Functions;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.WebJobs.Host;

public class ODMTraceWriter : TraceWriter
{
    private readonly TraceWriter ConsoleLog;
    private static Lazy<TelemetryClient> AppInsightsLog;
    public static Func<ITelemetryChannel> GetDefaultChannel;
    public static string CloudRoleName = Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME");
    public static string CloudRoleInstance = Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID");
    private static TelemetryClient CreateTelemetry()
    {
        //var connectionString = ApplicationGlobals.Configuration.AppInsightsConnectionString;
        var connectionString =
            @"InstrumentationKey=2f74edf5-376d-44be-8f2a-a2fe8e7accba;IngestionEndpoint=https://canadacentral-0.in.applicationinsights.azure.com/";
        if (string.IsNullOrEmpty(connectionString))
        {
            return null;
        }
        var config = new TelemetryConfiguration()
        {
            ConnectionString = connectionString,
            TelemetryChannel = GetDefaultChannel()
        };
        return new TelemetryClient(config);
    }
    static ODMTraceWriter()
    {
        ResetStatics();
    }
    public static void ResetStatics()
    {
        AppInsightsLog = new Lazy<TelemetryClient>(CreateTelemetry);
        GetDefaultChannel = () => new InMemoryChannel();
    }
    public string Operation_Name { get; set; }
    public string Operation_Id { get; }
    public string Operation_ParentId { get; }
    private RequestTelemetry RequestTelemetry;

    private ODMTraceWriter(TraceWriter consoleLog, string operation_Name, string operation_ParentId, string operation_Id) : base(TraceLevel.Verbose)
    {
        ConsoleLog = consoleLog;
        Operation_Name = operation_Name;
        Operation_Id = operation_Id ?? Guid.NewGuid().ToString();
        Operation_ParentId = operation_ParentId ?? Operation_Id;
    }

    /// <summary>
    /// Create hybrid writer, which reports to Console and AppInsights TraceWriters
    /// </summary>
    /// <param name="consoleLog">existing TraceWriter passed to SomeFunction.Run(), to report to console also</param>
    /// <param name="operation_Name">ordinary trigger name</param>
    /// <param name="operation_ParentId">parent operation Id, like task.id</param>
    /// <param name="operation_Id">current operation id to correlate output events with trigger instance</param>
    /// <returns></returns>
    public static ODMTraceWriter Create(TraceWriter consoleLog, string operation_Name, string operation_ParentId, string operation_Id)
    {
        return new ODMTraceWriter(consoleLog, operation_Name, operation_ParentId, operation_Id);
    }

    private ITelemetry InLoggerContext(ITelemetry telemetry)
    {
        telemetry.Context.Operation.Name = Operation_Name;
        telemetry.Context.Operation.Id = Operation_Id;
        telemetry.Context.Operation.ParentId = Operation_ParentId;
        telemetry.Context.Cloud.RoleName = CloudRoleName;
        telemetry.Context.Cloud.RoleInstance = CloudRoleInstance;
        return telemetry;
    }
    private void UpdateProperties(ISupportProperties telemetry, TraceEvent traceEvent)
    {
        foreach (var kv in traceEvent.Properties)
        {
            if (kv.Value != null && !kv.Key.StartsWith("MS_")) // skip empty and MS properties
            {
                telemetry.Properties["prop__" + kv.Key] = kv.Value.ToString();
            }
        }
    }

    public override void Trace(TraceEvent traceEvent)
    {
        ConsoleLog.Trace(traceEvent);
        if (traceEvent.Level == TraceLevel.Error && traceEvent.Exception != null)
        {
            var exceptionTelemetry = new ExceptionTelemetry(traceEvent.Exception)
            {
                SeverityLevel = SeverityLevel.Error,
                Timestamp = traceEvent.Timestamp,
            };
            exceptionTelemetry.Properties["FormattedMessage"] = traceEvent.Message;
            UpdateProperties(exceptionTelemetry, traceEvent);
            AppInsightsLog.Value?.Track(InLoggerContext(exceptionTelemetry));
            Flush(); // send immediately
        }
        else
        {
            var traceTelemtry = new TraceTelemetry()
            {
                SeverityLevel = ToSeverityLevel(traceEvent.Level),
                Message = traceEvent.Message,
                Timestamp = traceEvent.Timestamp,
            };
            UpdateProperties(traceTelemtry, traceEvent);
            AppInsightsLog.Value?.Track(InLoggerContext(traceTelemtry));
        }
    }

    public void StartRequest(string requestName)
    {
        RequestTelemetry = new RequestTelemetry() { Name = requestName };
        RequestTelemetry.Start();
    }

    public void StopRequest(string responseCode, bool success)
    {
        RequestTelemetry.ResponseCode = responseCode;
        RequestTelemetry.Success = success;
        RequestTelemetry.Stop();
        AppInsightsLog.Value?.Track(InLoggerContext(RequestTelemetry));
    }

    protected SeverityLevel ToSeverityLevel(TraceLevel logEventLevel)
    {
        switch (logEventLevel)
        {
            case TraceLevel.Info:
                return SeverityLevel.Information;
            case TraceLevel.Warning:
                return SeverityLevel.Warning;
            case TraceLevel.Error:
                return SeverityLevel.Error;
            default:
                return SeverityLevel.Verbose;
        }
    }

    public override void Flush()
    {
        ConsoleLog.Flush();
        AppInsightsLog.Value?.Flush();
    }
}
