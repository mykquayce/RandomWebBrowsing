using System.Threading.Tasks;
using WorkflowCore.Models;
using Xunit;

namespace RandomWebBrowsing.Steps.Tests
{
	public class VisitLinkStepTests : IClassFixture<Fixtures.WebClientFixture>
	{
		private readonly VisitLinkStep _sut;

		public VisitLinkStepTests(Fixtures.WebClientFixture fixture)
		{
			_sut = new VisitLinkStep(fixture.WebClient);
		}

		[Theory]
		[InlineData("https://www.bing.com")]
		[InlineData("https://www.wikipedia.org/")]
		public Task Run(string uriString)
		{
			// Arrange
			_sut.UriString = uriString;

			// Act
			return _sut.RunAsync(new StepExecutionContext());
		}
	}
}
