using Azure.AI.OpenAI;
using MyFirstChatUI.Models;

namespace MyFirstChatUI.Agents
{
	public class CoffeeFileAgent
	{
		private CoffeeFileAgent(AzureOpenAIClient azureOpenAIClient, CoffeeData coffeeDataService)
		{

		}

		// Static factory method for async initialization
		public static async Task<CoffeeFileAgent> CreateAsync(AzureOpenAIClient azureOpenAIClient, CoffeeData coffeeDataService)
		{
			return new CoffeeFileAgent(azureOpenAIClient, coffeeDataService);
		}
	}
}
