using Dawn;
using Helpers.Common;
using Helpers.Tracing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RandomWebBrowsing.Services.Concrete
{
	public class RedditService : IRedditService
	{
		private readonly Clients.IWebClient _client;
		private readonly OpenTracing.ITracer? _tracer;

		public RedditService(
			Clients.IWebClient client,
			OpenTracing.ITracer? tracer = default)
		{
			_client = Guard.Argument(() => client).NotNull().Value;
			_tracer = tracer;
		}

		public async Task<Uri> GetRandomSubredditAsync()
		{
			using var scope = _tracer?.StartSpan();

			var uri = new Uri("/r/random/.rss", UriKind.Relative);

			var headers = await _client.GetHeadersAsync(uri);

			var uriString = headers["Location"].Single();

			var subredditUri = new Uri(uriString, UriKind.Absolute)
				.StripQuery();

			scope?.Span.SetTag(nameof(subredditUri), subredditUri.OriginalString);

			return subredditUri;
		}

		public async IAsyncEnumerable<Uri> GetSubredditThreadsAsync(Uri subredditUri)
		{
			using var scope = _tracer?.StartSpan();

			scope?.Span.SetTag(nameof(subredditUri), subredditUri.OriginalString);

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
			using var scope = _tracer?.StartSpan();

			scope?.Span.SetTag(nameof(threadUri), threadUri.OriginalString);

			Guard.Argument(() => threadUri).NotNull();
			Guard.Argument(() => threadUri.OriginalString)
				.NotNull()
				.NotEmpty()
				.NotWhiteSpace()
				.Matches(@"^https:\/\/old\.reddit\.com\/r\/[_\d\w]+\/comments\/[_\d\w]+\/[%_\d\w]+\/.rss$");

			var feed = await _client.GetFeedAsync(threadUri);

			foreach (var entry in feed.entry)
			{
				if (string.IsNullOrWhiteSpace(entry?.content?.Value))
				{
					continue;
				}

				yield return entry.content.Value;
			}
		}

		public void Dispose()
		{
			_client?.Dispose();
		}
	}
}
