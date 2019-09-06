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
		private const int _bufferSize = 1_024, _maxDownloadSize = 10_485_760;
		private static readonly XmlSerializer _feedSerializer = new XmlSerializer(typeof(Models.Generated.feedType));

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

		public async Task VisitLinkAsync(Uri uri)
		{
			var (_, stream, _) = await SendAsync(HttpMethod.Get, uri);

			int offset = 0, count;
			var buffer = new byte[_bufferSize];

			using (stream)
			{
				do
				{
					count = await stream.ReadAsync(buffer, offset, _bufferSize);

					offset += count;
				}
				while (count > 0 && offset > _maxDownloadSize);
			}
		}
	}
}
