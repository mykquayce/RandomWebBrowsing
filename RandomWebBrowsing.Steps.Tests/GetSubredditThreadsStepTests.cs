using System.Threading.Tasks;
using WorkflowCore.Models;
using Xunit;

namespace RandomWebBrowsing.Steps.Tests
{
	public class GetSubredditThreadsStepTests : IClassFixture<Fixtures.RedditServiceFixture>
	{
		private readonly GetSubredditThreadsStep _sut;

		public GetSubredditThreadsStepTests(Fixtures.RedditServiceFixture fixture)
		{
			_sut = new GetSubredditThreadsStep(fixture.RedditService);
		}

		[Theory]
		[InlineData("https://old.reddit.com/r/apexlegends/.rss")]
		[InlineData("https://old.reddit.com/r/Midsommar/.rss")]
		public async Task Run(string subredditUriString)
		{
			// Arrange
			_sut.SubredditUriString = subredditUriString;

			// Act
			await _sut.RunAsync(new StepExecutionContext());

			// Assert
			Assert.NotNull(_sut.ThreadsUris);
			Assert.NotEmpty(_sut.ThreadsUris);
			Assert.All(_sut.ThreadsUris, s => Assert.Matches(@"^https:\/\/old\.reddit\.com\/r\/[_0-9A-Za-z]+\/comments\/[0-9a-z]+\/[_%0-9A-Za-z]+\/.rss$", s));
		}
	}
}
