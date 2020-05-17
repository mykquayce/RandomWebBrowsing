using RandomWebBrowsing.Models;
using System.Linq;
using Xunit;

namespace RandomWebBrowsing.Services.Tests
{
	public class MessageServiceTests : IClassFixture<Fixtures.MessageServiceFixture>
	{
		private readonly IMessageService _sut;

		public MessageServiceTests(Fixtures.MessageServiceFixture messageServiceFixture)
		{
			_sut = messageServiceFixture.MessageService;
		}

		[Theory]
		[InlineData(default, MessageTypes.None)]
		[InlineData("", MessageTypes.None)]
		[InlineData(" ", MessageTypes.None)]
		[InlineData("https://old.reddit.com/r/random/.rss", MessageTypes.RandomSubreddit)]
		[InlineData("https://old.reddit.com/r/BuyItForLife/.rss", MessageTypes.Subreddit)]
		[InlineData("https://old.reddit.com/r/BuyItForLife/comments/gkwf8y/my_1960s_philips_type_m_3060_4_speed_hand_whisk/.rss", MessageTypes.Thread)]
		[InlineData("https://www.youtube.com/watch?v=WW58zWrqQdc", MessageTypes.Link)]
		[InlineData("A quick brown fox jumps over the lazy dog", MessageTypes.Comment)]
		public void GetMessageType(string message, MessageTypes expected)
		{
			var actual = _sut.GetMessageTypes(message);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData(
			"<!-- SC_OFF --><div class=\"md\"><p>I really want that as my ringtone lol whats the name of the song?</p> <p>edit: nvm found it <a href=\"https://www.youtube.com/watch?v=WW58zWrqQdc\">https://www.youtube.com/watch?v=WW58zWrqQdc</a></p> </div><!-- SC_ON -->",
			"https://www.youtube.com/watch?v=WW58zWrqQdc")]
		public void GetLinksFromComment(string comment, params string[] expecteds)
		{
			var actual = _sut.GetLinksFromComment(comment).ToList();

			Assert.Equal(expecteds.Length, actual.Count);

			for (var a = 0; a < expecteds.Length; a++)
			{
				Assert.Equal(expecteds[a], actual[a].OriginalString);
			}
		}
	}
}
