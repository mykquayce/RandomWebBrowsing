using RandomWebBrowsing.Models;
using System.Threading.Tasks;
using Xunit;

namespace RandomWebBrowsing.Steps.Tests
{
	public class EvaluateMessageStepTests : IClassFixture<Fixtures.MessageServiceFixture>
	{
		private readonly Steps.EvaluateMessageStep _sut;

		public EvaluateMessageStepTests(Fixtures.MessageServiceFixture fixture)
		{
			_sut = new Steps.EvaluateMessageStep(fixture.MessageService);
		}

		[Theory]
		[InlineData("https://old.reddit.com/r/random/.rss", MessageTypes.RandomSubreddit)]
		[InlineData("https://old.reddit.com/r/BuyItForLife/.rss", MessageTypes.Subreddit)]
		[InlineData("https://old.reddit.com/r/BuyItForLife/comments/gkwf8y/my_1960s_philips_type_m_3060_4_speed_hand_whisk/.rss", MessageTypes.Thread)]
		[InlineData("https://www.youtube.com/watch?v=WW58zWrqQdc", MessageTypes.Link)]
		[InlineData("A quick brown fox jumps over the lazy dog", MessageTypes.Comment)]
		public async Task Run(string message, MessageTypes expected)
		{
			// Arrange
			_sut.Message = message;

			// Act
			await _sut.RunAsync(new WorkflowCore.Models.StepExecutionContext());

			// Assert
			Assert.Equal(expected, _sut.MessageTypes);
		}
	}
}
