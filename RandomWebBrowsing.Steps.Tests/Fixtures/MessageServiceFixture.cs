using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace RandomWebBrowsing.Steps.Tests.Fixtures
{
	public class MessageServiceFixture
	{
		public MessageServiceFixture()
		{
			var blacklist = new List<string>(4)
			{
				"redd.it",
				"reddit.com",
				"redditmedia.com",
				"redditstatic.com",
			};

			var options = Options.Create(blacklist);

			MessageService = new Services.Concrete.MessageService(options);
		}

		public Services.IMessageService MessageService { get; }
	}
}
