using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
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

		// The ID of the current vector store used for document search.
		private string vectorStoreId = null!;

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

		private async Task InitializeAsync()
		{
			vectorStoreId = storeClient.GetVectorStores()?.FirstOrDefault()?.Id ?? string.Empty;
			if (string.IsNullOrEmpty(vectorStoreId))
			{
				//  Create a new store
				vectorStoreId = await CreateNewStore();
			}
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