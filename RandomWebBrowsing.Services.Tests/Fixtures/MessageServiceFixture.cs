using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace RandomWebBrowsing.Services.Tests.Fixtures
{
	public class MessageServiceFixture
	{
		private static readonly List<string> _blacklist = new List<string>(4)
		{
			"redd.it",
			"reddit.com",
			"redditmedia.com",
			"redditstatic.com",
		};

		public MessageServiceFixture()
		{
			var options = Options.Create(_blacklist);

			MessageService = new Services.Concrete.MessageService(options);
		}

		public IMessageService MessageService { get; }
	}
}
