using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RandomWebBrowsing.Clients
{
	public interface IHttpClient : IDisposable
	{
		Task<IDictionary<string, IEnumerable<string>>> GetHeadersAsync(
			Uri uri,
			[CallerMemberName] string? methodName = default);

		Task<Models.Generated.feedType> GetFeedAsync(
			Uri uri,
			[CallerMemberName] string? methodName = default);
		Task VisitLinkAsync(Uri uri);
	}
}
