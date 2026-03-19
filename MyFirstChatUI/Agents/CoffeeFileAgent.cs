using Azure.AI.OpenAI;
using MyFirstChatUI.Models;
using OpenAI.Assistants;
using OpenAI.Files;
using OpenAI.VectorStores;

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
			return new CoffeeFileAgent(azureOpenAIClient, coffeeDataService);
		}
	}
	#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
}