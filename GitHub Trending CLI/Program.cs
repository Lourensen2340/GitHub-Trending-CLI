using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string duration = "week"; // Default duration is "week"
        int limit = 10; // Default limit is 10

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--duaration" && i + 1 < args.Length)
            {
                duration = args[i + 1];
                i++; // Skip the next argument since it's the value for duration
            }
            else if (args[i] == "--limit" && i + 1 < args.Length)
            {
                if (int.TryParse(args[i + 1], out int parsedLimit))
                {
                    limit = parsedLimit;
                }
                i++; // Skip the next argument since it's the value for limit

            }
        }
        if (duration != "day" && duration != "week" && duration != "month" && duration != "year")
        {
            Console.WriteLine("Invalid duration for --duaration. Please use 'day', 'week', 'month', or 'year'.");
            return;
        }

        DateTime formDate = duration switch
        {
            "day" => DateTime.Now.AddDays(-1),
            "week" => DateTime.Now.AddDays(-7),
            "month" => DateTime.Now.AddMonths(-1),
            "year" => DateTime.Now.AddYears(-1),
            _ => throw new ArgumentException("Invalid duration", nameof(duration))
        };

        string dateStr = formDate.ToString("yyyy-MM-dd");
        string url = $"https://api.github.com/search/repositories?q=created:>{dateStr}&sort=stars&order=desc&per_page={limit}";

        try
        {
            using HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GitHubTrendingCLI", "1.0"));

            Console.WriteLine($"Fetching trending repositories created since {dateStr}...\n");

            HttpResponseMessage responseMessage = await client.GetAsync(url);

            if (!responseMessage.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error fetching data: {responseMessage.ReasonPhrase}");
                return;
            }

            string jsonResponse = await responseMessage.Content.ReadAsStringAsync();

            using JsonDocument jsonDoc = JsonDocument.Parse(jsonResponse);
            JsonElement root = jsonDoc.RootElement;
            JsonElement items = root.GetProperty("items");

            int index = 1;
            foreach (JsonElement repo in items.EnumerateArray())
            {
                string name = repo.GetProperty("full_name").GetString() ?? "Unknown";
                string description = repo.GetProperty("description").ValueKind == JsonValueKind.Null ? "No description"
                    : repo.GetProperty("description").GetString() ?? "No description";
                int stars = repo.GetProperty("stargazers_count").GetInt32();
                string language = repo.GetProperty("language").ValueKind == JsonValueKind.Null ? "Unknown"
                    : repo.GetProperty("language").GetString() ?? "Unknown";

                Console.WriteLine($"{index}. {name}");
                Console.WriteLine($"Звезды: {stars} | Язык: {language}");
                Console.WriteLine($"Описание: {description}");
                Console.WriteLine(new string('-', 50));
                index++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
