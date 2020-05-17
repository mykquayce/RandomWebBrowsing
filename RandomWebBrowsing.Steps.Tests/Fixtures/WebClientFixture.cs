using System;
using System.Net.Http;
using System.Xml.Serialization;

namespace RandomWebBrowsing.Steps.Tests.Fixtures
{
	public class WebClientFixture : IDisposable
	{
		private readonly HttpMessageHandler _httpMessageHandler;
		private readonly HttpClient _httpClient;

		public WebClientFixture()
		{
			_httpMessageHandler = new HttpClientHandler { AllowAutoRedirect = false, };
			_httpClient = new HttpClient(_httpMessageHandler)
			{
				BaseAddress = new Uri("https://old.reddit.com", UriKind.Absolute),
			};
			var xmlSerializaterFactory = new XmlSerializerFactory();

			WebClient = new Clients.Concrete.WebClient(_httpClient, xmlSerializaterFactory);
		}

		public Clients.IWebClient WebClient { get; }

		public void Dispose()
		{
			WebClient?.Dispose();
			_httpClient?.Dispose();
			_httpMessageHandler?.Dispose();
		}
	}
}
