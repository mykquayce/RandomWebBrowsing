using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xunit;

namespace RandomWebBrowsing.Services.Tests
{
	public class RedditServiceTests : IDisposable
	{
		private readonly Stack<IDisposable> _disposables = new Stack<IDisposable>();
		private readonly IRedditService _sut;

		public RedditServiceTests()
		{
			var handler = new HttpClientHandler { AllowAutoRedirect = false, };
			var netHttpClient = new HttpClient(handler)
			{
				BaseAddress = new Uri("https://old.reddit.com", UriKind.Absolute),
			};
			var xmlSerializerFactory = new XmlSerializerFactory();
			var httpClient = new Clients.Concrete.WebClient(netHttpClient, xmlSerializerFactory);
			_sut = new Concrete.RedditService(httpClient);

			_disposables.Push(httpClient);
			_disposables.Push(netHttpClient);
			_disposables.Push(handler);
		}

		public void Dispose()
		{
			while (_disposables.TryPop(out var disposable))
			{
				disposable?.Dispose();
			}
		}

		[Fact]
		public async Task GetRandomSubreddit()
		{
			var actual = await _sut.GetRandomSubredditAsync();

			Assert.Matches(
				@"^https:\/\/old\.reddit\.com\/r\/[_\d\w]+\/.rss$",
				actual.OriginalString);
		}

		[Theory]
		[InlineData(10)]
		public async Task GetManyRandomSubreddits(int attempts)
		{
			while (--attempts > 0)
			{
				// Act
				var actual = await _sut.GetRandomSubredditAsync();

				// Assert
				Assert.NotNull(actual);
				Assert.NotNull(actual.OriginalString);
				Assert.Matches(
					@"^https:\/\/old\.reddit\.com\/r\/[_0-9A-Za-z]+\/\.rss$",
					actual.OriginalString);
			}
		}

		[Theory]
		[InlineData("https://old.reddit.com/r/BuyItForLife/.rss")]
		[InlineData("https://old.reddit.com/r/thinkorswim/.rss")]
		public async Task GetSubredditThreads(string subredditThreadUriString)
		{
			// Arrange
			var count = 0;
			var subredditThreadUri = new Uri(subredditThreadUriString, UriKind.Absolute);

			// Act
			var uris = _sut.GetSubredditThreadsAsync(subredditThreadUri);

			await foreach (var uri in uris)
			{
				count++;
				Assert.Matches(
					@"^https:\/\/old\.reddit\.com\/r\/[_0-9A-Za-z]+\/comments\/[0-9a-z]+\/[_0-9A-Za-z]+\/.rss$",
					uri.OriginalString);
			}

			Assert.InRange(count, 1, int.MaxValue);
		}

		[Theory]
		[InlineData("https://old.reddit.com/r/euphoria/comments/cm3ryv/euphoria_s1_e8_and_salt_the_earth_behind_you/.rss")]
		public async Task GetThreadComments(string uriString)
		{
			// Arrange
			var count = 0;
			var uri = new Uri(uriString, UriKind.Absolute);

			// Act
			await foreach(var comment in _sut.GetThreadCommentsAsync(uri))
			{
				count++;
				// Assert
				Assert.NotNull(comment);
				Assert.NotEmpty(comment);
			}

			Assert.InRange(count, 1, int.MaxValue);
		}
	}
}
