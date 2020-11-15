using System;

namespace RandomWebBrowsing.Steps.Tests.Fixtures
{
	public sealed class RedditServiceFixture : IDisposable
	{
		public RedditServiceFixture()
		{
			var fixture = new Fixtures.WebClientFixture();

			RedditService = new Services.Concrete.RedditService(fixture.WebClient);
		}

		public Services.IRedditService RedditService { get; }

		public void Dispose()
		{
			RedditService?.Dispose();
		}
	}
}
