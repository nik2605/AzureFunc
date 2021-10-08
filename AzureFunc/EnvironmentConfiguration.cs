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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using Functions.ResourceManagerHelper;
//using Quest.MMD.FunctionModels.Common;

namespace Azure_Functions
{
    public class DeploymentConfig
    {
        public string QmmpRest;
        public string QmmpAzureAdResource;
        public string OdmAzureAdResource;
        public string OdmeUrl;
        public string OdmeAzureResourceId;
        public string XcloudApimSubscription;
        public string XcloudApiUrl;
        public string XcloudUiUrl;
        public string XcloudAuthUrl;
        public string KeyVaultName;

        public string ArsVnetResourceGroup;
        public string ArsVnetName;
        public string ArsNsgName;

        public string StorageAccount;
        public string Region;
        //public TenantEnvironmentEnum AzureConfiguration;

        public Dictionary<string, string> AzureRoleToGroupMap;
    }

    public class CopyPermissionConfiguration
    {

        /// <summary>
        /// Maximum messages that can be added at a time to the queue
        /// </summary>
        public int MaximumMessagesToBeAddedToQueue;

        /// <summary>
        /// Maximum messages that can be processed at a time in the queue
        /// </summary>
        public int MaximumMessagesCount;

        public int MaximumBatchRetry;

        public int SitesPerTask;

        /// <summary>
        /// Based on this flag; Process with auto scale up/down depends on the throttling.
        /// </summary>
        public bool OverrideConfiguration;

        /// <summary>
        /// Maximum wait time before adding a new set of messages to the Queue.
        /// </summary>
        public int TimeToAddMessagesToQueue;

        public Condition DynamicCondition1 { get; set; }

        public Condition DynamicCondition2 { get; set; }

        public Condition DynamicCondition3 { get; set; }

        public Condition DynamicCondition4 { get; set; }

        public ElseCondition Remaining { get; set; }
    }

    public class Condition
    {
        public int MessageCountLessthan { get; set; }

        public int MaximumMessagesToBeAddedToQueue { get; set; }

        public int TimeToAddMessagesToQueue { get; set; }
    }

    public class ElseCondition
    {
        public int MaximumMessagesToBeAddedToQueue { get; set; }

        public int TimeToAddMessagesToQueue { get; set; }
    }

    public class AzureResourceTagsConfig
    {
        public Dictionary<string, string> DomainCoexistenceResourceGroup;
        public Dictionary<string, string> DomainCoexistenceVm;
    }

    public interface IEnvironmentConfiguration
    {
        string BaseUrl { get; }
        DeploymentConfig DeploymentConfig { get; }
        AzureResourceTagsConfig AzureResourceTagsConfig { get; }
        ServicePrincipal AzureServicePrincipal { get; }
        CopyPermissionConfiguration CopyPermissionConfig { get; }
        string LogAnalyticsId { get; }
        string LogAnalyticsKey { get; }
        string XCloudAzureAdClientId { get; }
        string XCloudAzureAdClientSecret { get; }
        string XCloudAzureAdResource { get; }
        string XCloudAzureAdTenant { get; }
        string RelayConnectionString { get; }
        string OneDriveDataStorage { get; }
        string OneDriveContentStorage { get; }
        string AppInsightsConnectionString { get; }


    }

    public interface IEnvironmentConfigurationProvider
    {
        IDictionary<string, string> ConnectionStrings
        {
            get;
        }
        IDictionary<string, string> AppSettings
        {
            get;
        }
    }

    public class EnvironmentConfigurationProvider : IEnvironmentConfigurationProvider
    {
        private IDictionary<string, string> _ConnectionStrings = null;
        public IDictionary<string, string> ConnectionStrings
        {
            get
            {
                return _ConnectionStrings = _ConnectionStrings ?? LoadConfiguration(ConnectionStringsDefault);
            }
        }

        private IDictionary<string, string> _AppSettings = null;
        public IDictionary<string, string> AppSettings
        {
            get
            {
                return _AppSettings = _AppSettings ?? LoadConfiguration(AppSettingsDefault);
            }
        }

        protected virtual IDictionary<string, string> ConnectionStringsDefault()
        {
            return ConfigurationManager.ConnectionStrings
                        .Cast<ConnectionStringSettings>()
                        .ToDictionary(x => x.Name, x => x.ConnectionString);
        }
        protected virtual IDictionary<string, string> AppSettingsDefault()
        {
            return ConfigurationManager.AppSettings.AllKeys
                .ToDictionary(x => x, x => ConfigurationManager.AppSettings[x]);
        }
        protected static IDictionary<string, string> LoadConfiguration(Func<IDictionary<string, string>> func)
        {
            for (var i = 0; i < Config.ConfigurationMaxTriesToLoad; i++)
            {
                var rv = func();
                if (rv.Count == 0)
                {
                    Thread.Sleep(Config.ConfigurationSleepBetweenTriesToLoadMs);
                    continue;
                }
                return rv;
            }
            throw new Exception(Config.ConfigurationMaxTriesToLoadMessage);
        }
    }

    public class EnvironmentConfiguration : IEnvironmentConfiguration
    {
        protected readonly IEnvironmentConfigurationProvider ConfigurationProvider;

        public EnvironmentConfiguration(IEnvironmentConfigurationProvider configurationProvider)
        {
            ConfigurationProvider = configurationProvider;
        }

        public string BaseUrl => ConfigurationProvider.AppSettings["FUNCTION_URL"];
        public DeploymentConfig DeploymentConfig => JsonConvert.DeserializeObject<DeploymentConfig>(ConfigurationProvider.AppSettings["DEPLOYMENT_CONFIGURATION"]);
        public AzureResourceTagsConfig AzureResourceTagsConfig => JsonConvert.DeserializeObject<AzureResourceTagsConfig>(ConfigurationProvider.AppSettings["AZURE_RESOURCE_TAGS_CONFIGURATION"]);
        public ServicePrincipal AzureServicePrincipal => JsonConvert.DeserializeObject<ServicePrincipal>(ConfigurationProvider.ConnectionStrings["AZURE_SERVICE_PRINCIPAL"]);
        public CopyPermissionConfiguration CopyPermissionConfig => ConfigurationProvider.AppSettings.TryGetValue("COPYPERMISSION_CONFIGURATION", out var copyPermissionConfig) ?
            JsonConvert.DeserializeObject<CopyPermissionConfiguration>(copyPermissionConfig) :
            new CopyPermissionConfiguration()
            {
                MaximumMessagesCount = 1500,
                MaximumMessagesToBeAddedToQueue = 2500,
                OverrideConfiguration = true,
                TimeToAddMessagesToQueue = 5,
                MaximumBatchRetry = 15,
                SitesPerTask = 10,
                DynamicCondition1 = new Condition
                {
                    MaximumMessagesToBeAddedToQueue = 2000,
                    MessageCountLessthan = 10,
                    TimeToAddMessagesToQueue = 4
                },
                DynamicCondition2 = new Condition
                {
                    MaximumMessagesToBeAddedToQueue = 1500,
                    MessageCountLessthan = 25,
                    TimeToAddMessagesToQueue = 5
                },
                DynamicCondition3 = new Condition
                {
                    MaximumMessagesToBeAddedToQueue = 1000,
                    MessageCountLessthan = 50,
                    TimeToAddMessagesToQueue = 6
                },
                DynamicCondition4 = new Condition
                {
                    MaximumMessagesToBeAddedToQueue = 500,
                    MessageCountLessthan = 100,
                    TimeToAddMessagesToQueue = 7
                },
                Remaining = new ElseCondition
                {
                    MaximumMessagesToBeAddedToQueue = 50,
                    TimeToAddMessagesToQueue = 8
                }
            };
        public string LogAnalyticsId => ConfigurationProvider.ConnectionStrings["LOG_ANALYTICS_ID"];
        public string LogAnalyticsKey => ConfigurationProvider.ConnectionStrings["LOG_ANALYTICS_KEY"];
        public string XCloudAzureAdClientId => ConfigurationProvider.ConnectionStrings["XCLOUD_AZUREAD_CLIENT_ID"];
        public string XCloudAzureAdClientSecret => ConfigurationProvider.ConnectionStrings["XCLOUD_AZUREAD_CLIENT_SECRET"];
        public string XCloudAzureAdResource => ConfigurationProvider.ConnectionStrings["XCLOUD_AZUREAD_RESOURCE"];
        public string XCloudAzureAdTenant => ConfigurationProvider.ConnectionStrings["XCLOUD_AZUREAD_TENANT"];
        public string RelayConnectionString => ConfigurationProvider.ConnectionStrings["RELAY"];
        public string OneDriveDataStorage => ConfigurationProvider.AppSettings["OneDriveDataStorage"];
        public string OneDriveContentStorage => ConfigurationProvider.AppSettings["OneDriveContentStorage"];
        public string AppInsightsConnectionString => ConfigurationProvider.AppSettings.TryGetValue("APPINSIGHTS_CONNECTIONSTRING", out var value) ? value : null;

        public static Func<IEnvironmentConfiguration> Create = () => new EnvironmentConfiguration(new EnvironmentConfigurationProvider());
    }
}
