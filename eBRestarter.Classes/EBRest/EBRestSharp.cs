using RestSharp;
using RestSharp.Authenticators;
using Serilog;
using eBRestarter.Classes.Enums;

namespace eBRestarter.Classes.EBRest
{
    public class EBRestSharp
    {

        private string? _url;
        private string? _username;
        private string? _password;

        public string? URL { get => _url; set => _url = value; }
        public string? UserName { get => _username; set => _username = value; }
        public string? Password { get => _password; set => _password = value; }

        public string? GetJSONResponse { get; set; }

        private readonly CancellationToken cancellationToken = new();

        private RestClient? eBRestSharp = new();
        private RestRequest? request = new();
        private RestResponse? response = new();

        public EBRestSharp()
        {
            URL = _url;
            UserName = _username;
            Password = _password;
        }


        private RestClient CheckAuthenticatorCredentials()
        {
            if ((_username is null && _password is null) ^ (_username is not null && _password is null) ^ (_username is null && _password is not null))
            {
                eBRestSharp = new RestClient();
            }
            else
            {
                var options = new RestClientOptions(_url!)
                {
                    Authenticator = new HttpBasicAuthenticator(_username!, _password!)
                };

                eBRestSharp = new RestClient(options);
            }

            return eBRestSharp;
        }

        public ResponseCode GetRequestStatus()
        {
            request = new RestRequest(_url) { Timeout = new TimeSpan(0,0,60) }; 

            try
            {
                response = CheckAuthenticatorCredentials().Get(request);

                if (response.IsSuccessful)
                {
                    string? countOfXRatelimit = (string?)response.Headers!.Where(x => x.Name == "X-Ratelimit-Remaining").Select(x => x.Value).FirstOrDefault();

                    if (int.Parse(countOfXRatelimit!) <= 10)
                    {
                        //MessageBox.Show("Dein Anfragelimit ist gleich überschritten" + " du hast nur noch " + countOfXRatelimit + " Anfragen übrig", "Anfragelimit");

                        GetJSONResponse = response.Content;

                        return ResponseCode.RequestLimit;
                    }

                    GetJSONResponse = response.Content;

                    return ResponseCode.Success;
                }
                else
                {
                    return ResponseCode.NoConnectionToServer;
                }
            }

            catch (HttpRequestException e)
            {
                Log.Error(e, "Eine Ausnahme ist aufgetreten. Logging...");

                if (e.Message.Contains("Request failed with status code TooManyRequests"))
                {
                    return ResponseCode.HttpRE429; // Too many requests, limit is 400
                }
                else if (e.Message.Contains("Unauthorized"))
                {
                    return ResponseCode.HttpRE401; // Unauthorized;
                }
                else if (e.Message.Contains("InternalServerError"))
                {
                    return ResponseCode.InternalServerError; // Unauthorized;
                }
                else
                {
                    return ResponseCode.GeneralExceptionError;
                }
            }
            catch (TimeoutException e)
            {
                Log.Error("API Timeout: " + e.Message);

                return ResponseCode.HTTPTimeout; // The request is taking too long and has been canceled
            }

            catch (Exception e)
            {
                Log.Error("General: " + e.Message);

                return ResponseCode.GeneralExceptionError;
            }
            finally
            {
                if (eBRestSharp != null)
                {
                    ((IDisposable?)eBRestSharp!).Dispose();
                }
            }
        }


        public async Task<string?> GetRequestAsync()
        {
            request = new RestRequest(_url) { Timeout = new TimeSpan(0, 0, 60) };

            try
            {
                response = await CheckAuthenticatorCredentials().GetAsync(request, cancellationToken);

                if (response.IsSuccessful)
                {
                    return response.Content;
                }
                else
                {
                    return "Es konnte keine Verbindung zum Abfrageserver gestellt werden.";
                }
            }

            catch (HttpRequestException e)
            {
                if (e.Message.Contains("429"))
                {
                    return "429"; // To many requests

                }
                else if (e.Message.Contains("Unauthorized"))
                {
                    return "401";

                }
                else
                {
                    return e.Message.ToString();
                }
            }
            catch (TimeoutException)
            {
                return "408"; //The request is taking too long and has been canceled
            }
            catch (Exception e)
            {
                return e.Message.ToString();
            }
            finally
            {
                if (eBRestSharp != null)
                {
                    ((IDisposable?)eBRestSharp!).Dispose();
                }
            }
        }
    }
}
