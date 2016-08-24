using System;
using Microsoft.Extensions.Configuration;

namespace WebVRPano.Services
{
    public class ConfigurationService : IConfigurationService
    {
		private readonly IConfigurationRoot configuration;
		
		public ConfigurationService(IConfigurationRoot configuration) 
		{
			this.configuration = configuration;
		}
		
		public string Get(string key) 
		{
            string envValue = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrEmpty(envValue))
            {
                return envValue;
            }
			return configuration[key];
		}
	}
}