using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace RandomWebBrowsing.WorkerService
{
	public static class Program
	{
		public static Task Main(string[] args)
		{
			var hostBuilder = Host.CreateDefaultBuilder(args);

			hostBuilder
				.ConfigureAppConfiguration((hostContext, configurationBuilder) =>
				{
					if (string.IsNullOrWhiteSpace(hostContext.HostingEnvironment.ApplicationName))
					{
						hostContext.HostingEnvironment.ApplicationName = System.Reflection.Assembly.GetAssembly(typeof(Program))!.GetName().Name;
					}

					var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? Environments.Production;

					hostContext.HostingEnvironment.EnvironmentName = environmentName;

					configurationBuilder
						.SetBasePath(Environment.CurrentDirectory)
						.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
						.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
						.AddDockerSecret("rabbitmq_pass", "RabbitMQSettings:Password")
						.AddDockerSecret("rabbitmq_user", "RabbitMQSettings:UserName");
				});

			hostBuilder
				.ConfigureServices((hostContext, services) =>
				{
					services
						.AddHttpClient<Clients.IWebClient, Clients.Concrete.WebClient>(
							(provider, client) =>
							{
								var uriSettings = hostContext.Configuration
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
						.AddSingleton(new System.Xml.Serialization.XmlSerializerFactory());

					services
						.Configure<List<string>>(hostContext.Configuration.GetSection("Blacklist"))
						.Configure<Helpers.RabbitMQ.Models.RabbitMQSettings>(hostContext.Configuration.GetSection(nameof(Helpers.RabbitMQ.Models.RabbitMQSettings)))
						.Configure<Config.Settings>(hostContext.Configuration.GetSection(nameof(Config.Settings)))
						.Configure<Config.Uris>(hostContext.Configuration.GetSection(nameof(Config.Uris)));

					services
						.AddTransient<Services.IMessageService, Services.Concrete.MessageService>()
						.AddSingleton<Services.IMessageQueueService, Services.Concrete.MessageQueueService>()
						.AddTransient<Services.IRedditService, Services.Concrete.RedditService>();

					services
						.AddTransient<Steps.AcknowledgeMessageStep>()
						.AddTransient<Steps.ConsumeMessageStep>()
						.AddTransient<Steps.EvaluateMessageStep>()
						.AddTransient<Steps.GetRandomSubredditStep>()
						.AddTransient<Steps.GetSubredditThreadsStep>()
						.AddTransient<Steps.ProcessThreadStep>()
						.AddTransient<Steps.PublishMessageStep>()
						.AddTransient<Steps.VisitLinkStep>();

					services
						.AddHostedService<Worker>()
						.AddWorkflow();
				});

			return hostBuilder.RunConsoleAsync();
		}
	}
}
