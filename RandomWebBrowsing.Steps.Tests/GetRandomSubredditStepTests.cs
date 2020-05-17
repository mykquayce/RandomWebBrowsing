using System.Threading.Tasks;
using WorkflowCore.Models;
using Xunit;

namespace RandomWebBrowsing.Steps.Tests
{
	public class GetRandomSubredditStepTests : IClassFixture<Fixtures.RedditServiceFixture>
	{
		private readonly Steps.GetRandomSubredditStep _sut;

		public GetRandomSubredditStepTests(Fixtures.RedditServiceFixture fixture)
		{
			_sut = new Steps.GetRandomSubredditStep(fixture.RedditService);
		}

		[Fact]
		public async Task Run()
		{
			// Act
			await _sut.RunAsync(new StepExecutionContext());

			// Assert
			Assert.Matches(
				@"^https:\/\/old\.reddit\.com\/r\/[_0-9A-Za-z]+\/\.rss$",
				_sut.RandomSubredditUri?.OriginalString);
		}
	}
}
