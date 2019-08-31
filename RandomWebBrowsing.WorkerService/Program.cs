using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RandomWebBrowsing.WorkerService
{
	public static class Program
	{
		public static Task Main(string[] args)
		{
			var hostBuilder = Host.CreateDefaultBuilder(args);

			hostBuilder
				.ConfigureServices((hostContext, services) =>
				{
					services.AddHostedService<Worker>();
				});

			return hostBuilder.RunConsoleAsync();
		}
	}
}
