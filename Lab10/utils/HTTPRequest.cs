using System.Net.Http;
using System.Threading.Tasks;

namespace Lab10.utils
{
    public class HTTPRequest
    {
        public static async Task<string> Request(string url)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        throw new HttpRequestException($"HTTP Error: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
                catch (HttpRequestException e)
                {
                    throw new HttpRequestException($"Request error: {e.Message}");
                }
            }
        }
    }
}
