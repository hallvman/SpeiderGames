namespace Common.ViewModels
{
    public class GameViewModel
    {
        public DateTime Date { get; set; }
        public int? Performance { get; set; }
        public int? Accessibility { get; set; }
        public int? BestPractices { get; set; }
        public int? Seo { get; set; }
        public int? Security { get; set; }
        public List<LinkViewModel>? Links { get; set; }
    }
}