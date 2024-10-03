using eBRestarter.Classes.InternetBrowser;

namespace eBRestarter.Classes.Services
{
    public class InternetService : IWebLinks
    {
        public async Task<bool> IsInternetAvailable()
        {
            try
            {
                using (HttpClient client = new())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    HttpResponseMessage response = await client.GetAsync(IWebLinks.GOOGLE_LINK);
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

    }
}
