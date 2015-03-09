using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ManlyManFoodClient.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace ManlyManFoodClient
{
	public class MainPage : ContentPage
	{
		private ListView _recipesListView;
		private readonly HttpClient _client;

		public MainPage()
		{
			_client = new HttpClient() { BaseAddress = new Uri("http://manlymanfood.azurewebsites.net") };
			_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

			var stackLayout = CreateLayout();
			Content = stackLayout;
		}

		private StackLayout CreateLayout()
		{
			_recipesListView = new ListView { ItemTemplate = new DataTemplate(typeof(TextCell)) };
			_recipesListView.ItemTemplate.SetBinding(TextCell.TextProperty, "Name");
			_recipesListView.ItemTapped += GoToIngredientDetail;
			var btnSimple = new Button()
			{
				Text = "Get recipes"
			};
			btnSimple.Clicked += GetRecipes;
			var btnOAuth = new Button()
			{
				Text = "Get recipes with OAUTH"
			};
			btnOAuth.Clicked += GetRecipesOAuth;

			var stackLayout = new StackLayout()
			{
				VerticalOptions = LayoutOptions.FillAndExpand,
				Children =
				{
					new Label() {Text = "Recipes", XAlign = TextAlignment.Center},
					_recipesListView,
					btnSimple,
					btnOAuth
				}
			};
			return stackLayout;
		}

		private async void GetRecipes(object sender, EventArgs e)
		{
			var request = new HttpRequestMessage(HttpMethod.Get, "recipes");

			var response = await _client.SendAsync(request);
			if (response.IsSuccessStatusCode)
			{
				var responseString = await response.Content.ReadAsStringAsync();
				var recipes = JsonConvert.DeserializeObject<IEnumerable<Recipe>>(responseString);
				_recipesListView.ItemsSource = recipes;
			}
			else
			{
				await DisplayAlert("Oops!", string.Format("{0} : {1}", response.StatusCode, response.ReasonPhrase), "Got it!");
			}

		}

		private async void GetRecipesOAuth(object sender, EventArgs e)
		{
			await GetToken();
			GetRecipes(sender, e);
		}

		private async Task GetToken()
		{
			var request = new HttpRequestMessage(HttpMethod.Post, "token");
			const string username = "";
			const string password = "";
			const string grantType = "password";
			request.Content = new StringContent(
				string.Format("username={0}&password={1}&grant_type={2}", username, password, grantType),
				Encoding.UTF8, "application/x-www-form-urlencoded");
			var tokenResponse = await _client.SendAsync(request);
			var tokenString = (await tokenResponse.Content.ReadAsStringAsync());
			var jsonToken = JObject.Parse(tokenString);
			_client.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", jsonToken["access_token"]));
		}

		private void GoToIngredientDetail(object sender, ItemTappedEventArgs e)
		{
			var recipe = (Recipe)e.Item;
			this.Navigation.PushAsync(new RecipeDetailsPage(recipe.Id, _client));
		}
	}
}