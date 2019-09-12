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

					var environmentName = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT") ?? Environments.Production;

					hostContext.HostingEnvironment.EnvironmentName = environmentName;

					configurationBuilder
						.SetBasePath(Environment.CurrentDirectory)
						.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
						.AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true);
				});

			hostBuilder
				.ConfigureServices((hostContext, services) =>
				{
					services
						.AddHttpClient(
							nameof(Clients.Concrete.HttpClient),
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
						.AddTransient<Clients.IHttpClient, Clients.Concrete.HttpClient>();

					services
						.Configure<List<string>>(hostContext.Configuration.GetSection("Blacklist"))
						.Configure<Helpers.RabbitMQ.Models.Settings>(hostContext.Configuration.GetSection("RabbitMQSettings"))
						.Configure<Helpers.Jaeger.Models.Settings>(hostContext.Configuration.GetSection("JaegerSettings"))
						.Configure<Config.Settings>(hostContext.Configuration.GetSection(nameof(Config.Settings)))
						.Configure<Config.Uris>(hostContext.Configuration.GetSection(nameof(Config.Uris)));

					services
						.AddJaegerTracing(hostContext.Configuration.GetSection("JaegerSettings"));

					services
						.AddTransient<Services.IMessageService, Services.Concrete.MessageService>()
						.AddSingleton<Services.IMessageQueueService, Services.Concrete.MessageQueueService>()
						.AddTransient<Services.IRedditService, Services.Concrete.RedditService>();

					services
						.AddTransient<Steps.AcknowledgeMessageStep>()
						.AddTransient<Steps.ConsumeMessageStep>()
						.AddTransient<Steps.EvaluateMessageStep>()
						.AddTransient<Steps.GetSubredditThreadsStep>()
						.AddTransient<Steps.GetUriRedirectStep>()
						.AddTransient<Steps.ProcessThreadStep>()
						.AddTransient<Steps.PublishMessageStep>()
						.AddTransient<Steps.StartParentTraceStep>()
						.AddTransient<Steps.StopParentTraceStep>()
						.AddTransient<Steps.VisitLinkStep>();

					services
						.AddTransient<OpenTracing.IScope>(_ => Models.Ioc.Scope!);

					services
						.AddHostedService<Worker>()
						.AddWorkflow();
				});

			return hostBuilder.RunConsoleAsync();
		}
	}
}
