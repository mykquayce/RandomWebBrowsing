using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RandomWebBrowsing.WorkerService
{
	public class Worker : BackgroundService
	{
		private readonly ILogger? _logger;

		public Worker(
			ILogger<Worker>? logger = default)
		{
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				_logger?.LogInformation("Worker running at: {time:u}", DateTimeOffset.Now);
				await Task.Delay(1_000, stoppingToken);
			}
		}
	}
}
