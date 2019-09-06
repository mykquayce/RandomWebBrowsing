using System;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace RandomWebBrowsing.Workflows
{
	public class Workflow : IWorkflow<Models.PersistenceData>
	{
		public string Id => nameof(Workflow);

		public int Version => 1;

		public void Build(IWorkflowBuilder<Models.PersistenceData> builder)
		{
			builder
				.StartWith<Steps.ConsumeMessageStep>()
					.Output(data => data.Message, step => step.Message)
					.Output(data => data.DeliveryTag, step => step.DeliveryTag)
				.If(data => data.Message == default)
					.Do(then => then
						.StartWith<Steps.PublishMessageStep>()
							.Input(step => step.Message, _ => "https://old.reddit.com/r/random/.rss")
						.EndWorkflow()
					)
				.Then<Steps.EvaluateMessageStep>()
					.Input(step => step.Message, data => data.Message)
					.Output(data => data.MessageTypes, step => step.MessageTypes)
				.If(data => (data.MessageTypes & Models.MessageTypes.RandomSubreddit) != 0)
					.Do(then => then
						.StartWith<Steps.GetUriRedirectStep>()
							.Input(step => step.Uri, data => new Uri(data.Message!, UriKind.Absolute))
							.Output(data => data.Message, step => step.RedirectUri!.OriginalString)
						.Then<Steps.PublishMessageStep>()
							.Input(step => step.Message, data => data.Message)
					)
				.If(data => (data.MessageTypes & Models.MessageTypes.Subreddit) != 0)
					.Do(then => then
						.StartWith<Steps.GetSubredditThreadsStep>()
							.Input(step => step.SubredditUriString, data => data.Message)
							.Output(data => data.Messages, step => step.ThreadsUris)
						.ForEach(data => data.Messages)
							.Do(each => each
								.StartWith<Steps.PublishMessageStep>()
									.Input(step => step.Message, (_, context) => context.Item as string)
							)
					)
				.If(data => (data.MessageTypes & Models.MessageTypes.Thread) != 0)
					.Do(then => then
						.StartWith<Steps.ProcessThreadStep>()
							.Input(step => step.ThreadUriString, data => data.Message)
							.Output(data => data.Messages, step => step.Links)
						.ForEach(data => data.Messages)
							.Do(each => each
								.StartWith<Steps.PublishMessageStep>()
									.Input(step => step.Message, (_, context) => context.Item as string)
							)
					)
				.If(data => (data.MessageTypes & Models.MessageTypes.Link) != 0)
					.Do(then => then
						.StartWith<Steps.VisitLinkStep>()
							.Input(step => step.UriString, data => data.Message)
					)
				.Then<Steps.AcknowledgeMessageStep>()
					.Input(step => step.DeliveryTag, data => data.DeliveryTag)
				.EndWorkflow();
		}
	}
}
