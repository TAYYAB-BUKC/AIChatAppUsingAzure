using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using MyFirstChatUI.Models;
using OpenAI.Assistants;
using OpenAI.Files;
using OpenAI.VectorStores;
using System.Reflection.Metadata;

namespace MyFirstChatUI.Agents
{
	#pragma warning disable OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
	public class CoffeeFileAgent
	{
		// Client for interacting with the OpenAI Assistant API.
		private readonly AssistantClient assistantClient;

		// Client for managing vector stores (document stores for embeddings).
		private readonly VectorStoreClient storeClient;

		// Client for uploading and managing files in OpenAI.
		private readonly OpenAIFileClient fileClient;

		// Service for accessing coffee data and file names.
		private readonly CoffeeData coffeeDataService;

		// The ID of the current vector store used for document search.
		private string vectorStoreId = null!;

		// The AI chat agent instance used for answering questions about coffee data.
		public ChatClientAgent Agent { get; private set; } = null!;

		// Private constructor to prevent direct instantiation
		private CoffeeFileAgent(AzureOpenAIClient azureOpenAIClient, CoffeeData coffeeDataService)
		{
			this.coffeeDataService = coffeeDataService;
			storeClient = azureOpenAIClient.GetVectorStoreClient();
			assistantClient = azureOpenAIClient.GetAssistantClient();
			fileClient = azureOpenAIClient.GetOpenAIFileClient();
		}

		// Static factory method for async initialization
		public static async Task<CoffeeFileAgent> CreateAsync(AzureOpenAIClient azureOpenAIClient, CoffeeData coffeeDataService)
		{
			var agent = new CoffeeFileAgent(azureOpenAIClient, coffeeDataService);
			await agent.InitializeAsync();
			return agent;
		}

		private async Task InitializeAsync()
		{
			vectorStoreId = storeClient.GetVectorStores()?.FirstOrDefault()?.Id ?? string.Empty;
			if (string.IsNullOrEmpty(vectorStoreId))
			{
				//  Create a new store
				vectorStoreId = await CreateNewStore();
			}

			// Create an Agent
			string prompt = @"
				The document store contains the text of coffee descriptions.
				Always analyze the document store to provide an answer to the user's question.
				Never rely on your knowledge not included in the document store.
				Always format response using markdown.
            ";

			Agent = await assistantClient.CreateAIAgentAsync(
					"gpt-4o-mini",
					name: "Coffee Agent",
					instructions: prompt,
					tools: [new HostedFileSearchTool()
					{
						Inputs = [new HostedVectorStoreContent(vectorStoreId)]
					}]);

			//var Agent = await assistantClient.CreateAssistantAsync(
			//		"gpt-4o-mini",
			//		new AssistantCreationOptions
			//		{
			//			Name = "Coffee Agent",
			//			Instructions = prompt,
			//			ToolResources = new ToolResources
			//			{
			//				FileSearch = new FileSearchToolResources
			//				{
			//					VectorStoreIds = { vectorStoreId }
			//				}
			//			}
			//		}
			//	);
		}

		private async Task<string> CreateNewStore()
		{
			var store = await storeClient.CreateVectorStoreAsync();
			if (store?.Value is not { Id: var storeId }) throw new Exception("Store was not created");
			foreach (string fileName in coffeeDataService.GetMarkdownFileNames())
			{
				var fullPath = Path.Combine(coffeeDataService.DataPath, fileName);
				OpenAIFile fileInfo = await fileClient.UploadFileAsync(fullPath, FileUploadPurpose.Assistants);
				await storeClient.AddFileToVectorStoreAsync(storeId, fileInfo.Id);
			}
			return storeId;
		}
	}
	#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
}