using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RandomWebBrowsing.Clients
{
	public interface IWebClient : IDisposable
	{
		Task<IReadOnlyDictionary<string, StringValues>> GetHeadersAsync(
			Uri uri,
			[CallerMemberName] string? callerMemberName = default,
			[CallerFilePath] string? callerFilePath = default);

		Task<Helpers.Reddit.Models.feed> GetFeedAsync(
			Uri uri,
			[CallerMemberName] string? callerMemberName = default,
			[CallerFilePath] string? callerFilePath = default);
		Task VisitLinkAsync(Uri uri);
	}
}
