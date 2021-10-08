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
using System.Security.Cryptography;

public class Config
{
    public static readonly TimeSpan LeaseExpirationDefault = TimeSpan.FromHours(0.5);
    public static readonly TimeSpan MaxFunctionRunDefault = TimeSpan.FromMinutes(4.5);
    public const string StatisticTableNameFormat = "{0}Statistics{1}";
    public const string StatisticBlobContainerNameFormat = "{0}-attachments-{1}";
    public static TimeSpan ProgressAggregatorTimeout = TimeSpan.FromSeconds(10);
    public static TimeSpan ProgressAggregatorMaxFunctionRun = TimeSpan.FromMinutes(4.5);
    public static TimeSpan MaxThrottlingDelay = TimeSpan.FromHours(4.55d);
    public static string MaxThrottlingDelayMessage = "The maximum number of requests to Microsoft Azure servers has been exceeded. Please try again later. Original error is '{0}'.";
    public static double ThrottlingDelayVariationMin = 0.9d; // delay = delay * rnd.next(min, max)
    public static double ThrottlingDelayVariationMax = 1.1d;

    public static int CustomerSettingsDefaultMaxMailboxesPerCustomer = 5;
    public static TimeSpan CustomerSettingsLeaseTimeout = TimeSpan.FromSeconds(60);
    public static int CustomerSettingsNumberOfTriesToUpdateSettings = 100;
    public static TimeSpan CustomerSettingsSleepBetweenTriesToUpdateSettings = TimeSpan.FromSeconds(1);
    public static TimeSpan LeaseExpiration = LeaseExpirationDefault;
    public static TimeSpan MaxFunctionRun = MaxFunctionRunDefault;
    public static int ConfigurationMaxTriesToLoad = 15;
    public static int ConfigurationSleepBetweenTriesToLoadMs = 1000;
    public static string ConfigurationMaxTriesToLoadMessage = "Cannot get application settings from Azure App Service. Please try again later.";
    
    public static TimeSpan OnPremPullTimeout = TimeSpan.FromSeconds(10);
    public static int OnPremMinProcessedObjects = 15;
    public static int OnPremMaxProcessedObjects = 15 * 10;
    public static int OnPremMaxMessageCount = 100;
    public static TimeSpan OnPremSyncWaitPullTimeout = TimeSpan.FromSeconds(60);
    public static TimeSpan OnPremSyncWaitPullTimeoutMaxDeltaRange = TimeSpan.FromSeconds(30); // used for queue delay randomization
    public static TimeSpan OnPremSyncWaitTimeout = TimeSpan.FromHours(2); // max time we wait for objects to be synced with AAD
    public static int OnPremMaxConnectionTries = 180; // TotalWaitTime = OnPremPullTimeout * OnPremMaxConnectionTries, and now half an hour
    public static Version OnPremMinorMigratorSupportedVersion = new Version("1.0.0.0");

    //public static OdmeLicenseOptions OdmeTrialLicenseOptions = new OdmeLicenseOptions
    //{
    //    LimitsEnabled = true,
    //    AppointmentLimit = 10,
    //    ContactLimit = 10,
    //    MessageLimit = 10,
    //    RecoverableItemLimit = 10,
    //    StickyNoteLimit = 10,
    //    TaskLimit = 10,
    //    MaxMegaBytesPerMailbox = 10,
    //    MaxSuccessfulMigrations = 10
    //};

    //public static OdmeLicenseOptions OdmePaidLicenseOptions = new OdmeLicenseOptions
    //{
    //    LimitsEnabled = false,
    //    MaxMegaBytesPerMailbox = Int64.MaxValue / 1024 / 1024,
    //    MaxSuccessfulMigrations = Int64.MaxValue
    //};

    public class OneDriveTrialLicenseOptions
    {
        public const long MaxByteCount = 100 * 1024 * 1024;
        public const int MaxFileCount = 5;
    }


    public static JsonSerializerSettings JsonCompact = new JsonSerializerSettings
    {
        Formatting = Formatting.None,
        NullValueHandling = NullValueHandling.Ignore
    };

    public static JsonSerializerSettings JsonReadable = new JsonSerializerSettings
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore
    };

    [ThreadStatic]
    private static Random _rnd;
    public static Random Random
    {
        get { return _rnd = _rnd ?? new Random(); }
        set { _rnd = value; }
    }
    public static Func<TimeSpan, TimeSpan> NextThrottlingDelay = (ts) => NextThrottlingDelayDefault(ts);

    public static TimeSpan NextThrottlingDelayDefault (TimeSpan lastThrottlingDelay)
    {
        if (lastThrottlingDelay == TimeSpan.Zero)
        {
            return TimeSpan.FromSeconds(32 * (Random.NextDouble() * (ThrottlingDelayVariationMax - ThrottlingDelayVariationMin) +
                                      ThrottlingDelayVariationMin));
        }
        return TimeSpan.FromSeconds(lastThrottlingDelay.TotalSeconds * 2);
    }
}
