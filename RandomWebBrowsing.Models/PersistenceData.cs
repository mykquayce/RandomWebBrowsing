using System.Collections.Generic;

namespace RandomWebBrowsing.Models
{
	public class PersistenceData
	{
		public string? Message { get; set; }
		public IEnumerable<string>? Messages { get; set; }
		public ulong? DeliveryTag { get; set; }
		public MessageTypes? MessageTypes { get; set; }
	}
}
