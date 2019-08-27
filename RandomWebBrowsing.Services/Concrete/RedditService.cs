using Dawn;
using Helpers.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RandomWebBrowsing.Services.Concrete
{
	public class RedditService : IRedditService
	{
		private readonly Clients.IHttpClient _client;

		public RedditService(
			Clients.IHttpClient client)
		{
			_client = Guard.Argument(() => client).NotNull().Value;
		}

		public async Task<Uri> GetRandomSubredditAsync()
		{
			var uri = new Uri("/r/random/.rss", UriKind.Relative);

			var headers = await _client.GetHeadersAsync(uri);

			var uriString = headers["Location"].Single();

			return new Uri(uriString, UriKind.Absolute)
				.StripQuery();
		}

		public async IAsyncEnumerable<Uri> GetSubredditThreadsAsync(Uri subredditUri)
		{
			Guard.Argument(() => subredditUri).NotNull();
			Guard.Argument(() => subredditUri.OriginalString)
				.NotNull()
				.NotEmpty()
				.NotWhiteSpace()
				.Matches(@"^https:\/\/old\.reddit\.com\/r\/[_\d\w]+\/.rss$");

			var feed = await _client.GetFeedAsync(subredditUri);

			foreach (var entry in feed.entry)
			{
				var uriString = entry.link.href;

				if (uriString.EndsWith("/.rss", StringComparison.InvariantCultureIgnoreCase))
				{
					yield return new Uri(uriString, UriKind.Absolute);
				}
				else if (uriString.EndsWith("/", StringComparison.InvariantCultureIgnoreCase))
				{
					yield return new Uri(uriString + ".rss", UriKind.Absolute);
				}
				else
				{
					yield return new Uri(uriString + "/.rss", UriKind.Absolute);
				}
			}
		}

		public async IAsyncEnumerable<string> GetThreadCommentsAsync(Uri threadUri)
		{
			Guard.Argument(() => threadUri).NotNull();
			Guard.Argument(() => threadUri.OriginalString)
				.NotNull()
				.NotEmpty()
				.NotWhiteSpace()
				.Matches(@"^https:\/\/old\.reddit\.com\/r\/[_\d\w]+\/comments\/[_\d\w]+\/[%_\d\w]+\/.rss$");

			var feed = await _client.GetFeedAsync(threadUri);

			foreach (var entry in feed.entry)
			{
				yield return entry.content.Value;
			}
		}
	}
}
