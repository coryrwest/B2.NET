using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using B2Net.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace B2NET.Tests {
	internal class ConfigHelper {
		public static B2ConfigMap GetTestB2Config() {
			var configBuilder = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", true)
				.AddUserSecrets<ConfigHelper>(true);
			var configRoot = configBuilder.Build();
			var serviceCollection = new ServiceCollection();
			serviceCollection.Configure<B2ConfigMap>(configRoot.GetSection("B2Configs"));
			var serviceProvider = serviceCollection.BuildServiceProvider();

			var configOptions = serviceProvider.GetRequiredService<IOptions<B2ConfigMap>>();

			return configOptions.Value;
		}
	}
}
