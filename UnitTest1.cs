using System;
using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using ExamPrep2.Models;



namespace ExamPrep2

{
    [TestFixture]
    public class Tests
    {
        private RestClient client;
        private static string lastCreatedFoodId;

        private const string BaseUrl = "http://144.91.123.158:81/";
        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiJkNmU5MjM2MC1hMjQ3LTQ3NmUtYjM1Mi03YWQyMTE0NjZmZTYiLCJpYXQiOiIwNC8xNi8yMDI2IDE2OjE2OjI2IiwiVXNlcklkIjoiNGJlMDA1ZjYtNmQxNy00NDQ2LTc0YmEtMDhkZTc2OGU4Zjk3IiwiRW1haWwiOiJzb3VwYUBleGFtcGxlLmNvbSIsIlVzZXJOYW1lIjoic291cGExMjMiLCJleHAiOjE3NzYzNzc3ODYsImlzcyI6IkZvb2R5X0FwcF9Tb2Z0VW5pIiwiYXVkIjoiRm9vZHlfV2ViQVBJX1NvZnRVbmkifQ._Rpg66ZQY9lZaTuVYHMkn6sokIuOo37rVs8LrbOtnqw";

        private const string userName = "soupa123";
        private const string password = "soup123";

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;

            if (!string.IsNullOrWhiteSpace(StaticToken))
            {
                jwtToken = StaticToken;
            }
            else
            {
                jwtToken = GetJwtToken(userName, password);
            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string userName, string password)
        {
            var tempClient = new RestClient(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { userName, password });    
            var response = tempClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Status code: {response.StatusCode}, Response: {response.Content}");
            }
        }

        [Order(1)]
        [Test]

        public void CreateNewFood_WithRequiredFields_ReturnsSuccess()
        {
            var request = new RestRequest("api/Food/Create", Method.Post);
            var newFood = new FoodDTO
            {
                Name = "Test Food",
                Description = "This is a test food item.",
                Url = "http://example.com/test-food.jpg"
            };
            request.AddJsonBody(newFood);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            var apiResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(apiResponse.FoodId, Is.Not.Null.And.Not.Empty);

            lastCreatedFoodId = apiResponse.FoodId;

        }

        [Order(2)]
        [Test]

        public void EditFood_ShouldReturnSuccess()
        {
            var request = new RestRequest($"api/Food/Edit/{lastCreatedFoodId}", Method.Patch);

            var editedFood = new[]
            {
                new
                {
                path = "/name",
                op = "replace",
                value = "Edited Food"
                 }
            };
            
            
            request.AddJsonBody(editedFood);

            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var apiResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(apiResponse.Msg, Is.EqualTo("Successfully edited"));
        }

        [Order(3)]
        [Test]

        public void GetAllFoods_ShouldReturnSuccess()
        {
            var request = new RestRequest("api/Food/All", Method.Get);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var apiResponse = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);
            Assert.That(apiResponse, Is.Not.Empty);
        }

        [Order(4)]
        [Test]

        public void DeleteFood_ShouldReturnSuccess()
        {
            var request = new RestRequest($"api/Food/Delete/{lastCreatedFoodId}", Method.Delete);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var apiResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Assert.That(apiResponse.Msg, Is.EqualTo("Deleted successfully!"));
        }

        [Order(5)]
        [Test]

        public void TryCreateFoodWithoutName_ShouldReturnBadRequest()
        {
            var request = new RestRequest("api/Food/Create", Method.Post);
            var newFood = new FoodDTO
            {
                Name = "",
                Description = "This food has no name.",
                Url = ""
            };
            request.AddJsonBody(newFood);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(6)]
        [Test]

        public void TrytoEditNonExistingFood_ShouldReturnNotFound()
        {
            var request = new RestRequest($"api/Food/Edit/{Guid.NewGuid()}", Method.Patch);
            var editedFood = new[]
            {
                new
                {
                path = "/name",
                op = "replace",
                value = "Edited Food"
                 }
            };
            request.AddJsonBody(editedFood);
            var response = this.client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Order(7)]
        [Test]

        public void TrytoDeleteNonExistingFood_ShouldReturnBadRequest()
        {
            var nonExistingId = Guid.NewGuid().ToString();

            var request = new RestRequest($"api/Food/Delete/{nonExistingId}", Method.Delete);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));


            var apiResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(apiResponse.Msg, Is.EqualTo("Unable to delete this food revue!"));

        }


        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}