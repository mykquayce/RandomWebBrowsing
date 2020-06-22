using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xunit;

namespace RandomWebBrowsing.Clients.Tests
{
	public class WebClientTests : IDisposable
	{
		private readonly HttpMessageHandler _httpMessageHandler;
		private readonly HttpClient _httpClient;
		private readonly IWebClient _sut;

		public WebClientTests()
		{
			_httpMessageHandler = new HttpClientHandler { AllowAutoRedirect = false, };
			_httpClient = new HttpClient(_httpMessageHandler);
			var xmlSerializerFactory = new XmlSerializerFactory();
			_sut = new Concrete.WebClient(_httpClient, xmlSerializerFactory);
		}

		[Theory]
		[InlineData(
			"https://old.reddit.com/r/random/.rss",
			@"^https:\/\/old\.reddit\.com\/r\/[_0-9A-Za-z]+\/\.rss\?utm_campaign=redirect&utm_medium=desktop&utm_source=reddit&utm_name=random_subreddit$")]
		public async Task GetHeaders(string uriString, string locationPattern)
		{
			// Arrange
			var uri = new Uri(uriString, UriKind.Absolute);

			// Act
			var headers = await _sut.GetHeadersAsync(uri);

			// Assert
			Assert.NotNull(headers);
			Assert.NotEmpty(headers);
			Assert.Contains("location", headers.Keys);
			Assert.Matches(locationPattern, headers["location"]);
		}

		[Theory]
		[InlineData("https://old.reddit.com/r/BuyItForLife/.rss")]
		[InlineData("https://old.reddit.com/r/euphoria/comments/cm3ryv/euphoria_s1_e8_and_salt_the_earth_behind_you/.rss")]
		public async Task GetFeed(string uriString)
		{
			// Arrange
			var uri = new Uri(uriString, UriKind.Absolute);

			// Act
			var feed = await _sut.GetFeedAsync(uri);

			// Assert
			Assert.NotNull(feed);
			Assert.NotNull(feed.id);
			Assert.NotEmpty(feed.id);
		}

		[Theory]
		[InlineData("https://i.imgur.com/JB6bl5l.jpg")]
		[InlineData("https://i.imgur.com/hzKEh2E.jpg")]
		public Task VisitLink(string uriString)
		{
			// Arrange
			var uri = new Uri(uriString, UriKind.Absolute);

			// Act
			return _sut.VisitLinkAsync(uri);
		}

		public void Dispose()
		{
			_sut?.Dispose();
			_httpClient?.Dispose();
			_httpMessageHandler?.Dispose();
		}
	}
}
