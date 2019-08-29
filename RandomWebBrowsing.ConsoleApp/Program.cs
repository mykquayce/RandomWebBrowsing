using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace RandomWebBrowsing.ConsoleApp
{
	public static class Program
	{
		public static Task Main()
		{
			var hostBuilder = new HostBuilder();

			hostBuilder
				.ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
				{
					if (string.IsNullOrWhiteSpace(hostBuilderContext.HostingEnvironment.ApplicationName))
					{
						hostBuilderContext.HostingEnvironment.ApplicationName = System.Reflection.Assembly.GetAssembly(typeof(Program))!.GetName().Name;
					}

					var environmentName = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") ?? Environments.Production;

					hostBuilderContext.HostingEnvironment.EnvironmentName = environmentName;

					configurationBuilder
						.SetBasePath(Environment.CurrentDirectory)
						.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
						.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);
				});

			hostBuilder
				.ConfigureLogging((hostBuilderContext, loggingBuilder) =>
				{
					loggingBuilder
						.AddConfiguration(hostBuilderContext.Configuration.GetSection("Logging"))
						.AddConsole()
						.AddDebug()
						.AddEventSourceLogger();
				});

			hostBuilder
				.ConfigureServices((hostBuilderContext, services) =>
				{
					services
						.AddHttpClient(
							nameof(Clients.Concrete.HttpClient),
							(provider, client) =>
							{
								var uriSettings = hostBuilderContext.Configuration
									.GetSection(nameof(Config.Uris))
									.Get<Config.Uris>();

								client.Timeout = TimeSpan.FromSeconds(10);
								client.BaseAddress = new Uri(uriSettings.RedditBaseUriString!, UriKind.Absolute);
								client.DefaultRequestHeaders.Accept.Clear();
								client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
							})
						.ConfigurePrimaryHttpMessageHandler(() =>
						{
							return new HttpClientHandler { AllowAutoRedirect = false, };
						});

					services
						.AddTransient<Clients.IHttpClient, Clients.Concrete.HttpClient>();

					services
						.Configure<List<string>>(hostBuilderContext.Configuration.GetSection("Blacklist"))
						.Configure<Helpers.RabbitMQ.Models.Settings>(hostBuilderContext.Configuration.GetSection("RabbitMQSettings"))
						.Configure<Helpers.Jaeger.Models.Settings>(hostBuilderContext.Configuration.GetSection("JaegerSettings"))
						.Configure<Config.Settings>(hostBuilderContext.Configuration.GetSection(nameof(Config.Settings)))
						.Configure<Config.Uris>(hostBuilderContext.Configuration.GetSection(nameof(Config.Uris)));

					services
						.AddJaegerTracing(hostBuilderContext.Configuration.GetSection("JaegerSettings"));

					services
						.AddTransient<Services.IMessageService, Services.Concrete.MessageService>()
						.AddSingleton<Services.IMessageQueueService, Services.Concrete.MessageQueueService>()
						.AddTransient<Services.IRedditService, Services.Concrete.RedditService>();

					services
						.AddTransient<Steps.AcknowledgeMessageStep>()
						.AddTransient<Steps.ConsumeMessageStep>()
						.AddTransient<Steps.EvaluateMessageStep>()
						.AddTransient<Steps.GetSubredditThreadsStep>()
						.AddTransient<Steps.ProcessThreadStep>()
						.AddTransient<Steps.GetUriRedirectStep>()
						.AddTransient<Steps.PublishMessageStep>()
						.AddTransient<Steps.VisitLinkStep>();

					services
						.AddHostedService<HostedService>()
						.AddWorkflow();
				});

			return hostBuilder
				.RunConsoleAsync();
		}
	}
}
