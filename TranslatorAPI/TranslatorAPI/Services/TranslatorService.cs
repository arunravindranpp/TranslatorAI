using System.Text.Json;
using System.Text;
using TranslatorAPI.Models;

namespace TranslatorAPI.Services
{
    public class TranslatorService
    {
        private readonly HttpClient _httpClient;
        private readonly string _translatorEndpoint = "https://api.cognitive.microsofttranslator.com/"; // e.g., https://api.cognitive.microsofttranslator.com/
        private readonly string _subscriptionKey = "CG0nReNV1qtlnzXGibgJZjy8wUQ5fDCMiG7XCiZ9ib6tZPi8nbo6JQQJ99AKACYeBjFXJ3w3AAAbACOGX73h";
        private readonly string _region = "eastus"; // Optional if the key isn't region-specific.

        public TranslatorService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> TranslateTextAsync(string text, string toLanguage = "en")
        {
            var endpoint = $"{_translatorEndpoint}/translate?api-version=3.0&to={toLanguage}";
            var requestBody = new[]
            {
            new { Text = text }
        };

            var requestContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint);
            requestMessage.Headers.Add("Ocp-Apim-Subscription-Key", _subscriptionKey);
            requestMessage.Headers.Add("Ocp-Apim-Subscription-Region", _region); // Add only if region is required.
            requestMessage.Content = requestContent;

            var response = await _httpClient.SendAsync(requestMessage);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var translationResults = JsonSerializer.Deserialize<List<TranslationResponse>>(responseContent);

                if (translationResults != null && translationResults.Count > 0)
                {
                    var detectedLanguage = translationResults[0].detectedLanguage?.language ?? "Unknown";
                    var translatedText = translationResults[0].translations[0].text;
                    return translatedText;
                }
            }

            throw new HttpRequestException("Translation failed");
        }
    }
}