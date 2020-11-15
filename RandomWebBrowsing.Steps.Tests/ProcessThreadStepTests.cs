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
		[InlineData("https://old.reddit.com/r/programming/comments/iq1hi1/computer_productivity_why_it_is_important_that/.rss")]
		[InlineData("https://old.reddit.com/r/raspberrypi/comments/xus58/trying_to_find_a_mini_usb_keyboard_only_finding/.rss")]
		public async Task Run(string threadUriString)
		{
			// Arrange
			_sut.ThreadUriString = threadUriString;

			// Act
			await _sut.RunAsync(new StepExecutionContext());

			// Assert
			Assert.NotNull(_sut.Links);
			Assert.NotEmpty(_sut.Links);
			Assert.All(_sut.Links, Assert.NotNull);
			Assert.All(_sut.Links, s => Assert.StartsWith("http", s, StringComparison.InvariantCultureIgnoreCase));
		}
	}
}
