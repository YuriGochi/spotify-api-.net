using SpotifyAPI.Web.Http;
using SpotifyAPI.Web;
using System.Net.Http.Headers;
using System.Text.Json;

namespace spotify_consumer
{
    public class TokenSpotify
    {
        private readonly string clientId;
        private readonly string clientSecret;

        public TokenSpotify(string clientId, string clientSecret)
        {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
        }

        public async Task<string> GetAccessToken()
        {
            var base64Auth = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Auth);

                var requestBody = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                });

                var response = await client.PostAsync("https://accounts.spotify.com/api/token", requestBody);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var tokenResponse = JsonSerializer.Deserialize<AccessTokenResponse>(responseBody);
                    return tokenResponse.access_token;
                }
                else
                {
                    // Handle error, throw exception, or log the error
                    Console.WriteLine($"Error: {response.StatusCode}");
                    return null;
                }
            }
        }
    }

    public class AccessTokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }
}
