using System;
using System.Threading.Tasks;
using WorkflowCore.Models;
using Xunit;

namespace RandomWebBrowsing.Steps.Tests
{
	public class ProcessThreadStepTests : IClassFixture<Fixtures.RedditServiceFixture>, IClassFixture<Fixtures.MessageServiceFixture>
	{
		private readonly ProcessThreadStep _sut;

		public ProcessThreadStepTests(
			Fixtures.RedditServiceFixture redditServiceFixture,
			Fixtures.MessageServiceFixture messageServiceFixture)
		{
			_sut = new ProcessThreadStep(
				redditServiceFixture.RedditService,
				messageServiceFixture.MessageService);
		}

		[Theory]
		[InlineData("https://old.reddit.com/r/Multicopter/comments/glex6t/first_build_completed_obviously_it_has_already/.rss")]
		[InlineData("https://old.reddit.com/r/programming/comments/gle9a0/collection_of_100s_of_conference_tech_talks_in/.rss")]
		public async Task Run(string threadUriString)
		{
			// Arrange
			var count = 0;
			_sut.ThreadUriString = threadUriString;

			// Act
			await _sut.RunAsync(new StepExecutionContext());

			// Assert
			foreach (var link in _sut.Links)
			{
				count++;
				Assert.NotNull(link);
				Assert.NotEmpty(link);
				Assert.StartsWith("http", link, StringComparison.InvariantCultureIgnoreCase);
			}

			Assert.InRange(count, 1, int.MaxValue);
		}
	}
}
