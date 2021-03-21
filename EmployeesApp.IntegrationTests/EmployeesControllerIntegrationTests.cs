using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;

namespace EmployeesApp.IntegrationTests
{
    public class EmployeesControllerIntegrationTests : IClassFixture<TestingWebAppFactory<Startup>>
    {
        private readonly HttpClient _client;
        public EmployeesControllerIntegrationTests(TestingWebAppFactory<Startup> factory)
        {
           _client = factory.CreateClient();
        }

        [Fact]
        public async Task Index_WhenCalled_ReturnsApplicationForm()
        {
            var response = await _client.GetAsync("/Employees");
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("Mark", responseString);
            Assert.Contains("Evelin", responseString);
        }

        [Fact]
        public async Task Create_WhenCalled_ReturnsCreateForm()
        {
            var response = await _client.GetAsync("/Employees/Create");

            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("Please provide a new employee data", responseString);
        }

        [Fact]
        public async Task Create_SentWrongModel_ReturnsViewWithErrorMessages2()
        {
            var initResponse = await _client.GetAsync("/Employees/Create");
            var antiForgeryValues = await AntiForgeryTokenExtractor.ExtractAntiForgeryValues(initResponse);
            var postRequest = new HttpRequestMessage(HttpMethod.Post, "/Employees/Create");
            postRequest.Headers.Add("Cookie", new CookieHeaderValue(AntiForgeryTokenExtractor.AntiForgeryCookieName, antiForgeryValues.cookieValue).ToString());
            var formModel = new Dictionary<string, string>
    {
        { AntiForgeryTokenExtractor.AntiForgeryFieldName, antiForgeryValues.fieldValue },
        { "Name", "New Employee" },
        { "Age", "25" }
    };
            postRequest.Content = new FormUrlEncodedContent(formModel);
            var response = await _client.SendAsync(postRequest);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("Account number is required", responseString);
        }

        [Fact]
        public async Task Create_SentWrongModel_ReturnsViewWithErrorMessages()
        {
            var postRequest = new HttpRequestMessage(HttpMethod.Post, "/Employees/Create");
            var formModel = new Dictionary<string, string>
                {
                    { "Name", "New Employee" },
                    { "Age", "25" }
                };
            postRequest.Content = new FormUrlEncodedContent(formModel);
            var response = await _client.SendAsync(postRequest);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("Account number is required", responseString);
        }

        [Fact]
        public async Task Create_WhenPOSTExecuted_ReturnsToIndexViewWithCreatedEmployee()
        {
            var postRequest = new HttpRequestMessage(HttpMethod.Post, "/Employees/Create");
            var formModel = new Dictionary<string, string>
            {
                { "Name", "New Employee" },
                { "Age", "25" },
                { "AccountNumber", "214-5874986532-21" }
            };
            postRequest.Content = new FormUrlEncodedContent(formModel);
            var response = await _client.SendAsync(postRequest);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("New Employee", responseString);
            Assert.Contains("214-5874986532-21", responseString);
        }

        [Fact]
        public async Task Create_WhenPOSTExecuted_ReturnsToIndexViewWithCreatedEmployee2()
        {
            var initResponse = await _client.GetAsync("/Employees/Create");
            var antiForgeryValues = await AntiForgeryTokenExtractor.ExtractAntiForgeryValues(initResponse);
            var postRequest = new HttpRequestMessage(HttpMethod.Post, "/Employees/Create");
            postRequest.Headers.Add("Cookie", new CookieHeaderValue(AntiForgeryTokenExtractor.AntiForgeryCookieName, antiForgeryValues.cookieValue).ToString());
            var formModel = new Dictionary<string, string>
            {
                { AntiForgeryTokenExtractor.AntiForgeryFieldName, antiForgeryValues.fieldValue },
                { "Name", "New Employee" },
                { "Age", "25" },
                { "AccountNumber", "214-5874986532-21" }
            };
            postRequest.Content = new FormUrlEncodedContent(formModel);
            var response = await _client.SendAsync(postRequest);
            response.EnsureSuccessStatusCode();
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("New Employee", responseString);
            Assert.Contains("214-5874986532-21", responseString);
        }
    }




    public static class AntiForgeryTokenExtractor
    {
        public static string AntiForgeryFieldName { get; } = "AntiForgeryTokenField";
        public static string AntiForgeryCookieName { get; } = "AntiForgeryTokenCookie";

        public static async Task<(string fieldValue, string cookieValue)> ExtractAntiForgeryValues(HttpResponseMessage response)
        {
            var cookie = ExtractAntiForgeryCookieValueFrom(response);
            var token = ExtractAntiForgeryToken(await response.Content.ReadAsStringAsync());
            return (token, cookie);
        }
        private static string ExtractAntiForgeryCookieValueFrom(HttpResponseMessage response)
        {
            //questo non è in grado di farlo il codice fraudolento!!
            string antiForgeryCookie = response.Headers.GetValues("Set-Cookie")
                .FirstOrDefault(x => x.Contains(AntiForgeryCookieName));
            if (antiForgeryCookie is null)
            {
                throw new ArgumentException($"Cookie '{AntiForgeryCookieName}' not found in HTTP response", nameof(response));
            }
            string antiForgeryCookieValue = SetCookieHeaderValue.Parse(antiForgeryCookie).Value.ToString();
            return antiForgeryCookieValue;
        }
        private static string ExtractAntiForgeryToken(string htmlBody)
        {
            var requestVerificationTokenMatch = Regex.Match(htmlBody, $@"\<input name=""{AntiForgeryFieldName}"" type=""hidden"" value=""([^""]+)"" \/\>");
            if (requestVerificationTokenMatch.Success)
            {
                return requestVerificationTokenMatch.Groups[1].Captures[0].Value;
            }
            throw new ArgumentException($"Anti forgery token '{AntiForgeryFieldName}' not found in HTML", nameof(htmlBody));
        }
    }
}
