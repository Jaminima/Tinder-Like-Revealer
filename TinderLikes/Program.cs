// See https://aka.ms/new-console-template for more information
using System.Net;
using System.Text.Json;

var tokenFile = "token.txt";

if (!File.Exists(tokenFile))
{
    File.Create(tokenFile);
    Console.WriteLine("Please Paste Your Token Into " + tokenFile);
    Console.ReadLine();
    return;
}

string token = File.ReadAllText(tokenFile);

if (token.Length == 0)
{
    Console.WriteLine("Please Correct Your Token In " + tokenFile);
    Console.ReadLine();
    return;
}

if (!Directory.Exists("likes")) Directory.CreateDirectory("likes");

List<string> consoleBody = new List<string>();

using (var webClient = new WebClient())
{
    using (var httpClient = new HttpClient())
    {
        while (true)
        {
            using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://api.gotinder.com/v2/fast-match/teasers?locale=en-GB"))
            {
                request.Headers.TryAddWithoutValidation("platform", "android");
                request.Headers.TryAddWithoutValidation("x-auth-token", token);

                var response = await httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Your Token Is Likely Invalid, Please Update It In " + tokenFile);
                    Console.ReadLine();
                    return;
                }

                var stream = response.Content.ReadAsStringAsync();
                stream.Wait();

                var body = stream.Result;

                var json = JsonSerializer.Deserialize<JsonElement>(body);

                var results = json.GetProperty("data").GetProperty("results").EnumerateArray();

                foreach (var u in results)
                {
                    var user = u.GetProperty("user");

                    var userId = user.GetProperty("_id").GetString();

                    var photos = user.GetProperty("photos").EnumerateArray();

                    var photoUrl = photos.First().GetProperty("url").GetString();

                    var filePath = "./likes/" + userId + ".jpeg";

                    if (!File.Exists(filePath))
                    {
                        consoleBody.Add("Found Like With Id " + userId);
                        await webClient.DownloadFileTaskAsync(new Uri(photoUrl), filePath);
                    }
                }
            }

            string consoleOut = String.Join('\n', consoleBody) + (consoleBody.Count > 0 ? "\n" : "") + "Likes Found: " + consoleBody.Count;
            string dotsString = "";

            const int dotsLim = 4;

            for (int i = 0; i < 30; i++)
            {
                dotsString = dotsString.Length > dotsLim ? "." : dotsString + ".";

                Console.Clear();
                Console.WriteLine(consoleOut + dotsString);
                Thread.Sleep(2000);
            }
        }
    }
}