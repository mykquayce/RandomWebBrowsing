using Microsoft.Extensions.Logging;
using OpenTracing;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RandomWebBrowsing.Clients.Concrete
{
	public class HttpClient : Helpers.HttpClient.HttpClientBase, IHttpClient
	{
		private static readonly XmlSerializer _feedSerializer;

		static HttpClient()
		{
			_feedSerializer = new XmlSerializer(typeof(Models.Generated.feedType));
		}

		public HttpClient(
			IHttpClientFactory httpClientFactory,
			ILogger? logger = default,
			ITracer? tracer = default)
			: base(httpClientFactory, logger, tracer)
		{ }

		public async Task<IDictionary<string, IEnumerable<string>>> GetHeadersAsync(
			Uri uri,
			[CallerMemberName] string? methodName = default)
		{
			var (_, _, headers) = await base.SendAsync(HttpMethod.Head, uri, body: default, methodName);

			return headers;
		}

		public async Task<Models.Generated.feedType> GetFeedAsync(
			Uri uri,
			[CallerMemberName] string? methodName = default)
		{
			var (_, stream, _) = await base.SendAsync(HttpMethod.Get, uri, body: default, methodName);

			using (stream)
			{
				return (Models.Generated.feedType)_feedSerializer.Deserialize(stream);
			}
		}

		public Task VisitLinkAsync(Uri uri)
			=> SendAsync(HttpMethod.Get, uri);
	}
}
