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

using (var webClient = new WebClient())
{
    using (var httpClient = new HttpClient())
    {
        while (true)
        {
            using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://api.gotinder.com/v2/fast-match/teasers?locale=en-GB"))
            {
                Console.WriteLine("Finding Likes");

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

                    var fileName = userId + ".jpeg";
                    var filePath = "./likes/" + fileName;

                    if (!File.Exists(filePath))
                    {
                        Console.WriteLine("Downloading " + fileName);
                        await webClient.DownloadFileTaskAsync(new Uri(photoUrl), filePath);
                    }
                }
            }
            Console.WriteLine("Waiting 60 Seconds Before Repeating");
            Thread.Sleep(60000);
        }
    }
}