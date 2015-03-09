using System;
using System.Net.Http;
using ManlyManFoodClient.Models;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace ManlyManFoodClient
{
	internal class RecipeDetailsPage : ContentPage
	{
		private readonly HttpClient _client;

		public RecipeDetailsPage(int recipeId, HttpClient client)
		{
			_client = client;
			GetRecipeAsync(recipeId);

			var recipeLabel = new Label();
			recipeLabel.SetBinding(Label.TextProperty, "Name");
			var listView = new ListView {ItemTemplate = new DataTemplate(typeof (TextCell))};
			listView.ItemTemplate.SetBinding(TextCell.TextProperty, "Name");
			listView.SetBinding(ListView.ItemsSourceProperty, "Ingredients");


			Content = new StackLayout()
			{
				Children =
				{
					new StackLayout()
					{
						Orientation = StackOrientation.Horizontal,
						Children = {new Label() {Text = "Recipe:"}, recipeLabel}
					},
					listView
				}
			};
		}

		private async void GetRecipeAsync(int recipeId)
		{
			var response = await _client.GetAsync(string.Format("recipes/{0}", recipeId));
			var recipeString = await response.Content.ReadAsStringAsync();
			var recipe = JsonConvert.DeserializeObject<Recipe>(recipeString);
			this.BindingContext = recipe;
		}
	}
}