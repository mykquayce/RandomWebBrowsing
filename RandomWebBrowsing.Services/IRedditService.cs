using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RandomWebBrowsing.Services
{
	public interface IRedditService : IDisposable
	{
		Task<Uri> GetRandomSubredditAsync();
		IAsyncEnumerable<Uri> GetSubredditThreadsAsync(Uri subredditUri);
		IAsyncEnumerable<string> GetThreadCommentsAsync(Uri threadUri);
	}
}
