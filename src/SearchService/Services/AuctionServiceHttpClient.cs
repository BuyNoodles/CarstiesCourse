using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services;

public class AuctionServiceHttpClient(HttpClient httpClient, IConfiguration config)
{
    public async Task<List<Item>> GetItemsForSearchDb()
    {
        string? lastUpdated = await DB.Find<Item, string>()
            .Sort(sort => sort.Descending(item => item.UpdatedAt))
            .Project(item => item.UpdatedAt.ToString())
            .ExecuteFirstAsync();

        return await httpClient.GetFromJsonAsync<List<Item>>(config["AuctionServiceUrl"]
            + "/api/auctions?date=" + lastUpdated) ?? [];
    }

}
