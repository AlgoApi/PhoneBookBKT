using System.Net.Http.Json;

namespace MonkeyFinder.Services;

public class MonkeyService
{
    HttpClient httpClient;
    public MonkeyService()
    {
        this.httpClient = new HttpClient();
    }

    List<Model.Monkey> monkeyList;
    public async Task<List<Model.Monkey>> GetMonkeys(string user_id)
    {
        if (monkeyList?.Count > 0)
            return monkeyList;

        // Online
        var response = await httpClient.GetAsync($"http://176.109.104.102/api/PhoneBook/{user_id}");
        if (response.IsSuccessStatusCode)
        {
            var result_debug = await response.Content.ReadAsStringAsync();
            monkeyList = await response.Content.ReadFromJsonAsync(MonkeyContext.Default.ListMonkey);
        }

        // Offline
        /*using var stream = await FileSystem.OpenAppPackageFileAsync("monkeydata.json");
        using var reader = new StreamReader(stream);
        var contents = await reader.ReadToEndAsync();
        monkeyList = JsonSerializer.Deserialize(contents, MonkeyContext.Default.ListMonkey);*/
        
        return monkeyList;
    }
}
