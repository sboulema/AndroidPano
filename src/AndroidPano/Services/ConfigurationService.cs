using Microsoft.Framework.Configuration;
using System;

namespace AndroidPano.Services
{
    public class ConfigurationService : IConfigurationService
    {
		private readonly IConfiguration configuration;
		
		public ConfigurationService(IConfiguration configuration) 
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