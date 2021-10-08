//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Text;
//using Microsoft.ApplicationInsights;
//using Microsoft.ApplicationInsights.Channel;
//using Microsoft.ApplicationInsights.DataContracts;
//using Microsoft.ApplicationInsights.Extensibility;
//using Microsoft.Azure.WebJobs.Host;

//namespace AzureFunc
//{
//    public class tempClass : TraceWriter
//    {

//        private static Lazy<TelemetryClient> AppInsightsLog;
//        private static TelemetryClient CreateTelemetry()
//        {
//            //var connectionString = ApplicationGlobals.Configuration.AppInsightsConnectionString;
//            var connectionString =
//                @"InstrumentationKey=146839c8-9f0d-491b-a306-e8d6050df4fe;IngestionEndpoint=https://eastus-2.in.applicationinsights.azure.com/";
//            if (string.IsNullOrEmpty(connectionString))
//            {
//                return null;
//            }
//            var config = new TelemetryConfiguration()
//            {
//                ConnectionString = connectionString
//            };
//            return new TelemetryClient(config);
//        }
//        static tempClass()
//        {
//            ResetStatics();
//        }
//        public static void ResetStatics()
//        {
//            AppInsightsLog = new Lazy<TelemetryClient>(CreateTelemetry);
//        }

//        public override void Trace(TraceEvent traceEvent)
//        {
//            AppInsightsLog.Value?.Track(InLoggerContext(traceTelemtry));
//        }
//    }
//}
