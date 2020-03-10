using Moq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace RandomWebBrowsing.Services.Tests
{
	public sealed class RedditServiceTests : IDisposable
	{
		private readonly HttpClient _httpClient;
		private readonly Services.IRedditService _redditService;

		public RedditServiceTests()
		{
			var handler = new HttpClientHandler { AllowAutoRedirect = false, };

			_httpClient = new HttpClient(handler, disposeHandler: true)
			{
				BaseAddress = new Uri("https://old.reddit.com/", UriKind.Absolute),
			};

			var httpClientFactoryMock = new Mock<IHttpClientFactory>();

			httpClientFactoryMock
				.Setup(f => f.CreateClient(It.Is<string>(s => s == nameof(Clients.Concrete.HttpClient))))
				.Returns(_httpClient);

			var httpClient = new Clients.Concrete.HttpClient(httpClientFactoryMock.Object);

			_redditService = new Services.Concrete.RedditService(tracer: default, httpClient);
		}

		public void Dispose()
		{
			_httpClient?.Dispose();
		}

		[Fact]
		public async Task RedditServiceTests_GetRandomSubredditAsync()
		{
			var actual = await _redditService.GetRandomSubredditAsync();

			Assert.Matches(
				@"^https:\/\/old\.reddit\.com\/r\/[_\d\w]+\/.rss$",
				actual.OriginalString);
		}

		[Theory]
		[InlineData("https://old.reddit.com/r/nostalgia/.rss")]
		public async Task RedditServiceTests_GetSubredditThreadsAsync(string uriString)
		{
			var count = 0;
			var subredditUri = new Uri(uriString, UriKind.Absolute);

			await foreach (var uri in _redditService.GetSubredditThreadsAsync(subredditUri))
			{
				count++;
				Assert.NotNull(uri);
				Assert.NotNull(uri.OriginalString);
				Assert.NotEmpty(uri.OriginalString);
				Assert.True(uri.IsAbsoluteUri);
			}

			Assert.InRange(count, 1, int.MaxValue);
		}

		[Theory]
		[InlineData("https://old.reddit.com/r/euphoria/comments/cm3ryv/euphoria_s1_e8_and_salt_the_earth_behind_you/.rss")]
		public async Task RedditServiceTests_GetThreadCommentsAsync(string threadUriString)
		{
			var count = 0;
			var threadUri = new Uri(threadUriString, UriKind.Absolute);

			await foreach (var comment in _redditService.GetThreadCommentsAsync(threadUri))
			{
				count++;
				Assert.NotNull(comment);
				Assert.NotEmpty(comment);
			}

			Assert.InRange(count, 1, int.MaxValue);
		}
	}
}
