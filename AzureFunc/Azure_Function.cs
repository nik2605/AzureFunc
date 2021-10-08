using System;

namespace Azure_Functions
{
    public static class ApplicationGlobals
    {
        private static Lazy<IEnvironmentConfiguration> _Configuration = new Lazy<IEnvironmentConfiguration>(EnvironmentConfiguration.Create);
        public static IEnvironmentConfiguration Configuration => _Configuration.Value;
    }
}