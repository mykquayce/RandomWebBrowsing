using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RandomWebBrowsing.Services
{
	public interface IRedditService
	{
		Task<Uri> GetRandomSubredditAsync();
		IAsyncEnumerable<Uri> GetSubredditThreadsAsync(Uri subredditUri);
		IAsyncEnumerable<string> GetThreadCommentsAsync(Uri threadUri);
	}
}
