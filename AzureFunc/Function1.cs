using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Azure_Functions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
//using Functions.MailboxesSyncStart;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
//using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly: WebJobsStartup(typeof(AzureFunc.MyStartup))]
namespace AzureFunc
{
    public static class demo
    {
        public static ODMTraceWriter _OdmTraceWriter;
        public static string AckToken { get; set; }
    }


    internal class CustomTelemetryProcessor : ITelemetryProcessor
    {
        private ITelemetryProcessor _next;
        private IHttpContextAccessor _httpContextAccessor;

        public CustomTelemetryProcessor(ITelemetryProcessor next, IHttpContextAccessor httpContextAccessor)
        {
            _next = next;
            _httpContextAccessor = httpContextAccessor;
        }

        public void Process(ITelemetry item)
        {
            //for testing purpose, I just add custom property to trace telemetry, you can modify the code as per your need.
            if (item is TraceTelemetry traceTelemetry)
            {
                // use _httpContextAccessor here...        
                traceTelemetry.Properties.Add("Appid", "acktoken");
            }

            // Send the item to the next TelemetryProcessor
            _next.Process(item);
        }
    }

    public class MyStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {

            builder.Services.AddScoped<TelemetryConfiguration>(sp =>
            {
                var key = "2f74edf5-376d-44be-8f2a-a2fe8e7accba";
                if (!string.IsNullOrWhiteSpace(key))
                {
                    return new TelemetryConfiguration(key);
                }
                return new TelemetryConfiguration();
            });

            builder.Services.AddHttpContextAccessor();

            var configDescriptor = builder.Services.SingleOrDefault(tc => tc.ServiceType == typeof(TelemetryConfiguration));
            if (configDescriptor?.ImplementationFactory != null)
            {
                var implFactory = configDescriptor.ImplementationFactory;
                builder.Services.Remove(configDescriptor);
                builder.Services.AddSingleton(provider =>
                {
                    if (implFactory.Invoke(provider) is TelemetryConfiguration config)
                    {
                        var newConfig = TelemetryConfiguration.Active;
                        newConfig.ApplicationIdProvider = config.ApplicationIdProvider;
                        newConfig.InstrumentationKey = config.InstrumentationKey;
                        newConfig.TelemetryProcessorChainBuilder.Use(next => new CustomTelemetryProcessor(next, provider.GetRequiredService<IHttpContextAccessor>()));
                        foreach (var processor in config.TelemetryProcessors)
                        {
                            newConfig.TelemetryProcessorChainBuilder.Use(next => processor);
                        }
                        var quickPulseProcessor = config.TelemetryProcessors.OfType<QuickPulseTelemetryProcessor>().FirstOrDefault();
                        if (quickPulseProcessor != null)
                        {
                            var quickPulseModule = new QuickPulseTelemetryModule();
                            quickPulseModule.RegisterTelemetryProcessor(quickPulseProcessor);
                            newConfig.TelemetryProcessorChainBuilder.Use(next => quickPulseProcessor);
                        }
                        newConfig.TelemetryProcessorChainBuilder.Build();
                        newConfig.TelemetryProcessors.OfType<ITelemetryModule>().ToList().ForEach(module => module.Initialize(newConfig));
                        return newConfig;
                    }
                    return null;
                });
            }
        }
    }

    public static class Log4NetExtensions
    {
        public static void Error(this TraceWriter log, string details, string message, Exception exception)
        {
            var logger = ODMTraceWriter.Create(log, null, "app name", demo.AckToken);
            //Do something with parameters
            //log.LogError(exception, message);
            logger.Error("error message");
        }
    }


    public static class Function1
    {
        [FunctionName("Function1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
          TraceWriter log)
        //    ILogger log)
        {

            

            demo.AckToken = "ackid1";


          //  var logger = ODMTraceWriter.Create(log, null, "app name", demo.AckToken);


            // demo._OdmTraceWriter = ODMTraceWriter.Create(log, null, "mitesh", "mitesh1");

            //demo._OdmTraceWriter.Error("C# HTTP trigger function processed a request.");

            ////  log.Error("test test");

            log.Error("MyApplication", "logging error", new Exception("error"));

           // log.E

        //    log.LogInformation($"test");

            //log.LogError("test");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            //return new MailboxesSync { Request = req }
            //    .Handle(req, GetODMTraceWriter(log));
            //new tempClass();



            return new OkObjectResult(responseMessage);
        }
    }

    public class MyClass
    {
        private readonly ILogger<MyClass> _logger;

        public MyClass(ILogger<MyClass> logger)
        {
            _logger = logger;
        }
    }
}
