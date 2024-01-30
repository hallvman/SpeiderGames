using System.Collections.Generic;

namespace Common.ViewModels
{
    public class PageViewModel
    {
        public PageViewModel() => HistoricGames= new List<GameViewModel>();

        public string? Url { get; set; }

        public GameViewModel? Current { get; set; }
        public List<GameViewModel> HistoricGames { get; set; }
    }
}