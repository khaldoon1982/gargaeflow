namespace GarageFlow.Application.Interfaces;

public interface IGlobalSearchService
{
    Task<GlobalSearchResultDto> SearchAsync(string query);
}

public class GlobalSearchResultDto
{
    public List<SearchResultItem> Results { get; set; } = new();
}

public class SearchResultItem
{
    public string Type { get; set; } = string.Empty;
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string? Extra { get; set; }
}
