using TwitchLeecher.Core.Models;
using TwitchLeecher.Core.Enums;

namespace TwitchLeecher.Gui.Interfaces
{
    public interface ISearchService
    {
        SearchParameters LastSearchParams { get; }

        void PerformSearch(SearchParameters searchParams);
    }
}