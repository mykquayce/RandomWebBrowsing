using Dawn;
using Microsoft.Extensions.Options;
using RandomWebBrowsing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RandomWebBrowsing.Services.Concrete
{
	public class MessageService : IMessageService
	{
		private readonly IReadOnlyCollection<string> _blacklist;
		private const RegexOptions _regexOptions = RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant;
		private const string _subredditUriPattern = @"^https:\/\/old\.reddit\.com\/r\/[_\d\w]+\/\.rss\b";
		private const string _threadUriPattern = @"^https:\/\/old\.reddit\.com\/r\/[_\d\w]+\/comments\/[_\d\w]+\/[%_\d\w]+\/\.rss$$";
		private const string _htmlLinkPattern = @"(href|src)=""(?<Uri>http.+?)""";
		private const string _linkPattern = @"^https?:\/\/.+?[^\""^\s]+$";
		private static readonly Regex _subredditUriRegex = new Regex(_subredditUriPattern, _regexOptions);
		private static readonly Regex _threadUriRegex = new Regex(_threadUriPattern, _regexOptions);
		private static readonly Regex _htmlLinkRegex = new Regex(_htmlLinkPattern, _regexOptions);
		private static readonly Regex _linkRegex = new Regex(_linkPattern, _regexOptions);

		public MessageService(
			IOptions<List<string>> options)
		{
			Guard.Argument(() => options).NotNull();
			_blacklist = Guard.Argument(() => options.Value).NotNull().DoesNotContainNull().DoesNotContain(string.Empty).Value;
		}

		public IEnumerable<Uri> GetLinksFromComment(string comment)
		{
			Guard.Argument(() => comment).NotNull().NotEmpty().NotWhiteSpace();

			var matches = _htmlLinkRegex.Matches(comment);

			foreach (Match match in matches)
			{
				var uriString = match.Groups["Uri"].Value;

				var uri = new Uri(uriString, UriKind.Absolute);

				// if blacklisted...
				if (_blacklist.Any(s => uri.Host.EndsWith(s, StringComparison.InvariantCultureIgnoreCase)))
				{
					// ...continue
					yield break;
				}

				yield return uri;
			}
		}

		public MessageTypes GetMessageTypes(string message)
		{
			if (string.Equals(message, "https://old.reddit.com/r/random/.rss", StringComparison.InvariantCultureIgnoreCase))
			{
				return MessageTypes.RandomSubreddit;
			}

			if (_subredditUriRegex.IsMatch(message))
			{
				return MessageTypes.Subreddit;
			}

			if (_threadUriRegex.IsMatch(message))
			{
				return MessageTypes.Thread;
			}

			if (_linkRegex.IsMatch(message))
			{
				return MessageTypes.Link;
			}

			return MessageTypes.Comment;
		}
	}
}
