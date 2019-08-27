using System;
using System.Collections.Generic;

namespace RandomWebBrowsing.Services
{
	public interface IMessageService
	{
		IEnumerable<Uri> GetLinksFromComment(string comment);
		Models.MessageTypes GetMessageTypes(string message);
	}
}
