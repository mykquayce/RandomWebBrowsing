using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Xunit;

namespace RandomWebBrowsing.Services.Tests
{
	public sealed class RedditServiceTests : IDisposable
	{
		private readonly Stack<IDisposable> _disposables = new();
		private readonly IRedditService _sut;
		private readonly XmlSerializerFactory _xmlSerializerFactory;

		public RedditServiceTests()
		{
			var handler = new HttpClientHandler { AllowAutoRedirect = false, };
			var netHttpClient = new HttpClient(handler)
			{
				BaseAddress = new Uri("https://old.reddit.com", UriKind.Absolute),
			};
			_xmlSerializerFactory = new XmlSerializerFactory();
			var httpClient = new Clients.Concrete.WebClient(netHttpClient, _xmlSerializerFactory);
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
		[InlineData("https://old.reddit.com/r/programming/comments/gle9a0/collection_of_100s_of_conference_tech_talks_in/.rss")]
		public async Task GetThreadComments(string uriString)
		{
			// Arrange
			var count = 0;
			var uri = new Uri(uriString, UriKind.Absolute);

			// Act
			await foreach (var comment in _sut.GetThreadCommentsAsync(uri))
			{
				count++;
				// Assert
				Assert.NotNull(comment);
				Assert.NotEmpty(comment);
			}

			Assert.InRange(count, 1, int.MaxValue);
		}

		[Theory]
		[InlineData(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<feed xmlns=""http://www.w3.org/2005/Atom"" xmlns:media=""http://search.yahoo.com/mrss/"">
    <category term=""programming"" label=""r/programming""/>
    <updated>2020-09-11T13:47:28+00:00</updated>
    <icon>https://www.redditstatic.com/icon.png/</icon>
    <id>/r/programming/comments/gle9a0/collection_of_100s_of_conference_tech_talks_in/.rss</id>
    <link rel=""self"" href=""https://old.reddit.com/r/programming/comments/gle9a0/collection_of_100s_of_conference_tech_talks_in/.rss"" type=""application/atom+xml"" />
    <link rel=""alternate"" href=""https://old.reddit.com/r/programming/comments/gle9a0/collection_of_100s_of_conference_tech_talks_in/"" type=""text/html"" />
    <logo>https://b.thumbs.redditmedia.com/2rTE46grzsr-Ll3Q.png</logo>
    <subtitle>Computer Programming</subtitle>
    <title>Collection of 100's of conference tech talks in various categories like Frontend, Backend, Mobile, Security, DevOps and even UX : programming</title>
    <entry>
        <author>
            <name>/u/apvarun</name>
            <uri>https://old.reddit.com/user/apvarun</uri>
        </author>
        <category term=""programming"" label=""r/programming""/>
        <content type=""html"">&amp;#32; submitted by &amp;#32; &lt;a href=&quot;https://old.reddit.com/user/apvarun&quot;&gt; /u/apvarun &lt;/a&gt; &lt;br/&gt; &lt;span&gt;&lt;a href=&quot;https://confs.space&quot;&gt;[link]&lt;/a&gt;&lt;/span&gt; &amp;#32; &lt;span&gt;&lt;a href=&quot;https://old.reddit.com/r/programming/comments/gle9a0/collection_of_100s_of_conference_tech_talks_in/&quot;&gt;[comments]&lt;/a&gt;&lt;/span&gt;</content>
        <id>t3_gle9a0</id>
        <link href=""https://old.reddit.com/r/programming/comments/gle9a0/collection_of_100s_of_conference_tech_talks_in/"" />
        <updated>2020-05-17T12:03:09+00:00</updated>
        <title>Collection of 100's of conference tech talks in various categories like Frontend, Backend, Mobile, Security, DevOps and even UX</title>
    </entry>
    <entry>
        <category term=""programming"" label=""r/programming"" />
        <content type=""html"">&lt;!-- SC_OFF --&gt;&lt;div class=&quot;md&quot;&gt;&lt;p&gt;[deleted]&lt;/p&gt; &lt;/div&gt;&lt;!-- SC_ON --&gt;</content>
        <id>t1_fqx27jj</id>
        <link href=""https://old.reddit.com/r/programming/comments/gle9a0/collection_of_100s_of_conference_tech_talks_in/fqx27jj/""/>
        <updated>2020-05-17T14:01:03+00:00</updated>
        <title>/u/[deleted] on Collection of 100's of conference tech talks in various categories like Frontend, Backend, Mobile, Security, DevOps and even UX</title>
    </entry>
    <entry>
        <author>
            <name>/u/apvarun</name>
            <uri>https://old.reddit.com/user/apvarun</uri>
        </author>
        <category term=""programming"" label=""r/programming"" />
        <content type=""html"">&lt;!-- SC_OFF --&gt;&lt;div class=&quot;md&quot;&gt;&lt;p&gt;Appreciate your feedback. Will add more in coming weeks 👍&lt;/p&gt; &lt;/div&gt;&lt;!-- SC_ON --&gt;</content>
        <id>t1_fqxrfkb</id>
        <link href=""https://old.reddit.com/r/programming/comments/gle9a0/collection_of_100s_of_conference_tech_talks_in/fqxrfkb/""/>
        <updated>2020-05-17T17:31:42+00:00</updated>
        <title>/u/apvarun on Collection of 100's of conference tech talks in various categories like Frontend, Backend, Mobile, Security, DevOps and even UX</title>
    </entry>
</feed>")]
		public void ProcessNearlyEmptyCommentThread_DoesntShitTheBed(string json)
		{
			var bytes = System.Text.Encoding.UTF8.GetBytes(json);
			using var stream = new MemoryStream(bytes);
			var serializer = _xmlSerializerFactory.CreateSerializer(typeof(Helpers.Reddit.Models.feed));
			var feed = serializer.Deserialize(stream) as Helpers.Reddit.Models.feed;

			Assert.NotNull(feed);
			Assert.Equal(3, feed!.entry.Length);
			Assert.All(feed.entry, Assert.NotNull);
			Assert.All(feed.entry, e => Assert.NotNull(e.content));
			Assert.All(feed.entry, e => Assert.NotNull(e.content.Value));
			Assert.All(feed.entry, e => Assert.NotEmpty(e.content.Value));
		}
	}
}
