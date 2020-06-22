using Dawn;
using Helpers.Web;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RandomWebBrowsing.Clients.Concrete
{
	public class WebClient : WebClientBase, IWebClient
	{
		private const int _bufferSize = 1_024, _maxDownloadSize = 10_485_760;
		private readonly XmlSerializerFactory _xmlSerializerFactory;

		public WebClient(
			HttpClient httpClient,
			XmlSerializerFactory xmlSerializerFactory,
			ILogger? logger = default)
			: base(httpClient, logger)
		{
			_xmlSerializerFactory = Guard.Argument(() => xmlSerializerFactory).NotNull().Value;
		}

		public async Task<IReadOnlyDictionary<string, StringValues>> GetHeadersAsync(
			Uri uri,
			[CallerMemberName] string? callerMemberName = default,
			[CallerFilePath] string? callerFilePath = default)
		{
			var response = await base.SendAsync(HttpMethod.Head, uri, callerMemberName: callerMemberName, callerFilePath: callerFilePath);

			return response.Headers!;
		}

		public async Task<Models.Generated.feedType> GetFeedAsync(
			Uri uri,
			[CallerMemberName] string? callerMemberName = default,
			[CallerFilePath] string? callerFilePath = default)
		{
			var response = await base.SendAsync(HttpMethod.Get, uri, callerMemberName: callerMemberName, callerFilePath: callerFilePath);

			using var stream = await response!.TaskStream!;

			var serializer = _xmlSerializerFactory.CreateSerializer(typeof(Models.Generated.feedType));

			return (Models.Generated.feedType)serializer.Deserialize(stream);
		}

		public async Task VisitLinkAsync(Uri uri)
		{
			int count, total = 0;
			var buffer = new Memory<char>(new char[_bufferSize]);

			var response = await SendAsync(HttpMethod.Get, uri);

			using var stream = await response!.TaskStream!;
			using var reader = new StreamReader(stream);

			do
			{
				count = await reader.ReadBlockAsync(buffer);
				total += count;
			}
			while (count > 0 && total < _maxDownloadSize);
		}
	}
}
