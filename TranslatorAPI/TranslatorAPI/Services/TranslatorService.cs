using System.Text.Json;
using System.Text;
using TranslatorAPI.Models;
using Microsoft.Extensions.Options;
using Microsoft.Data.SqlClient;
using System.Data;
using TranslatorAPI.Helpers;
using Serilog;

namespace TranslatorAPI.Services
{
    public class TranslatorService
    {
        private readonly HttpClient _httpClient;
        //private readonly string _translatorEndpoint = "https://api.cognitive.microsofttranslator.com/"; // e.g., https://api.cognitive.microsofttranslator.com/
        //private readonly string _subscriptionKey = "CG0nReNV1qtlnzXGibgJZjy8wUQ5fDCMiG7XCiZ9ib6tZPi8nbo6JQQJ99AKACYeBjFXJ3w3AAAbACOGX73h";
        //private readonly string _region = "eastus"; // Optional if the key isn't region-specific.
        private readonly TranslatorConfig _config;
        private readonly string _connectionString;
        public TranslatorService(HttpClient httpClient, IOptions<TranslatorConfig> config, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _config = config.Value;
            _connectionString = configuration.GetConnectionString("PACEDatabase");
        }

        public async Task<string> TranslateTextAsync(string text, string toLanguage = "en")
        {
            var endpoint = $"{_config.Endpoint}/translate?api-version=3.0&to={toLanguage}";
            var requestBody = new[] { new { Text = text } };

            var requestContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint);
            requestMessage.Headers.Add("Ocp-Apim-Subscription-Key", _config.SubscriptionKey);
            if (!string.IsNullOrWhiteSpace(_config.Region))
            {
                requestMessage.Headers.Add("Ocp-Apim-Subscription-Region", _config.Region);
            }
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
        public async Task<List<Message>> GetMessages()
        {
            var messages = new List<Message>();
            Log.Information("Translation service");
            try
            {
                using (var sqlCommand = new SqlCommand())
                {
                    sqlCommand.CommandText = @"SELECT Id, Sender, Receiver, MessageText, Timestamp
                                       FROM TranslatorMessages order by TimeStamp Asc";

                    using (var reader = EYSql.ExecuteReader(_connectionString, CommandType.Text, sqlCommand.CommandText))
                    {
                        while (await reader.ReadAsync())
                        {
                            var message = new Message
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Sender = reader.GetString(reader.GetOrdinal("Sender")),
                                Receiver = reader.IsDBNull(reader.GetOrdinal("Receiver"))
                                            ? null
                                            : reader.GetString(reader.GetOrdinal("Receiver")),
                                MessageText = reader.GetString(reader.GetOrdinal("MessageText")),
                                Timestamp = reader.GetDateTime(reader.GetOrdinal("Timestamp"))
                            };
                            messages.Add(message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.InnerException?.ToString());
            }

            return messages;
        }
        public async Task<List<Message>> GetMessagesByReceiver(string sender,string receiver)
        {
            var messages = new List<Message>();
            Log.Information("Translation service");

            try
            {
                using (var sqlCommand = new SqlCommand())
                {
                    // Use parameterized query to avoid issues with column and parameter names
                    string caseSQL = $@"SELECT Id, Sender, Receiver, MessageText, Timestamp
                                       FROM TranslatorMessages where Sender = '{sender}' And Receiver ='{receiver}' 
                                       OR  Sender = '{receiver}' And Receiver ='{sender}' 
                                       And Timestamp is not null 
                                       order by TimeStamp Asc";
                    using (var reader = EYSql.ExecuteReader(_connectionString, CommandType.Text, caseSQL))
                    {
                        while (await reader.ReadAsync())
                        {
                            var message = new Message
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Sender = reader.GetString(reader.GetOrdinal("Sender")),
                                Receiver = reader.IsDBNull(reader.GetOrdinal("Receiver"))
                                              ? null
                                              : reader.GetString(reader.GetOrdinal("Receiver")),
                                MessageText = reader.GetString(reader.GetOrdinal("MessageText")),
                                Timestamp = reader.GetDateTime(reader.GetOrdinal("Timestamp"))
                            };
                            messages.Add(message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
                Log.Error(ex.InnerException?.ToString());
            }

            return messages;
        }

    }
}