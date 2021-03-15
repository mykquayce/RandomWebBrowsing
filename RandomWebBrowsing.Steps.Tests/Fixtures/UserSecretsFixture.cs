using Microsoft.Extensions.Configuration;
using Xunit;

namespace RandomWebBrowsing.Steps.Tests.Fixtures
{
	public class UserSecretsFixture
	{
		public UserSecretsFixture()
		{
			var config = new ConfigurationBuilder()
				.AddUserSecrets(this.GetType().Assembly)
				.Build();

			RabbitMQSettings = config
				.GetSection("RabbitMQ")
				.Get<Helpers.RabbitMQ.Models.RabbitMQSettings>();
		}

		public Helpers.RabbitMQ.Models.RabbitMQSettings RabbitMQSettings { get; }

		[Fact]
		public void Values()
		{
			Assert.NotNull(RabbitMQSettings);
			Assert.NotNull(RabbitMQSettings.HostName);
			Assert.InRange(RabbitMQSettings.Port, 1, ushort.MaxValue);
			Assert.NotNull(RabbitMQSettings.UserName);
			Assert.NotEmpty(RabbitMQSettings.UserName);
			Assert.NotNull(RabbitMQSettings.Password);
			Assert.NotEmpty(RabbitMQSettings.Password);
			Assert.NotNull(RabbitMQSettings.VirtualHost);
			Assert.NotEmpty(RabbitMQSettings.VirtualHost);
		}
	}
}
