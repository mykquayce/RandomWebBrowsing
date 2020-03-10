using Microsoft.Extensions.Options;
using Moq;
using RandomWebBrowsing.Models;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace RandomWebBrowsing.Services.Tests
{
	public class MessageServiceTests
	{
		private readonly IMessageService _service;

		public MessageServiceTests()
		{
			var blacklist = new List<string>();
			var options = Mock.Of<IOptions<List<string>>>(o => o.Value == blacklist);

			_service = new Concrete.MessageService(default, options);
		}

		[Theory]
		[InlineData("https://old.reddit.com/r/random/.rss", MessageTypes.RandomSubreddit)]
		[InlineData("https://old.reddit.com/r/totalwar/.rss?utm_campaign=redirect&utm_medium=desktop&utm_source=reddit&utm_name=random_subreddit", MessageTypes.Subreddit)]
		[InlineData("https://old.reddit.com/r/euphoria/comments/cm3ryv/euphoria_s1_e8_and_salt_the_earth_behind_you/.rss", MessageTypes.Thread)]
		[InlineData("https://old.reddit.com/r/ImaginaryLandscapes/comments/9dnj8h/reminder_a_special_flair_is_available_for/.rss", MessageTypes.Thread)]
		[InlineData("https://old.reddit.com/r/Colorization/comments/ctgnfn/king_peter_i_karadjordjevi%C4%87_18441921_aka_the_king/.rss", MessageTypes.Thread)]
		[InlineData("https://play.google.com/store/apps/details?id=com.AlchemistCreative.Polygolf", MessageTypes.Link)]
		public void MessageServiceTests_GetMessageTypes(string message, MessageTypes expected)
		{
			var actual = _service.GetMessageTypes(message);

			Assert.Equal(expected, actual);
		}

		[Theory]
		[InlineData(@"<!-- SC_OFF --><div class=""md""><p>Ok so labyrinth bud you can drop the album now</p> </div><!-- SC_ON -->", 0)]
		[InlineData(
			@"<!-- SC_OFF --><div class=""md""><p>I really want that as my ringtone lol whats the name of the song?</p> <p>edit: nvm found it <a href=""https://www.youtube.com/watch?v=WW58zWrqQdc"">https://www.youtube.com/watch?v=WW58zWrqQdc</a></p> </div><!-- SC_ON -->",
			1)]
		public void MessageServiceTests_GetLinksFromComment(string comment, int expectedCount)
		{
			// Act
			var uris = _service.GetLinksFromComment(comment).ToList();

			// Assert
			Assert.Equal(expectedCount, uris.Count);
			Assert.All(uris, Assert.NotNull);
			Assert.All(uris, uri => Assert.True(uri.IsAbsoluteUri));
		}
	}
}
