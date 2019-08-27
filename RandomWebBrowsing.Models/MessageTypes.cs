using System;

namespace RandomWebBrowsing.Models
{
	[Flags]
	public enum MessageTypes : byte
	{
		None = 0,
		Comment = 1,
		Link = 2,
		RandomSubreddit = 4,
		Subreddit = 8,
		Thread = 16,
	}
}
