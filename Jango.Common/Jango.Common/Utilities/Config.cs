namespace Jango.Common.Utilities
{
    using System;
    using System.Configuration;
    public static class Config
    {
        public static T Get<T>(string key)
        {
            try
            {
                var val = ConfigurationManager.AppSettings[key];
                return (T)Convert.ChangeType(val, typeof(T));
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }
}
