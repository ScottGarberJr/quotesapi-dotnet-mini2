

namespace QuotesApi.Models
{
    public class QuoteDTO
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string? Source { get; set; }
        public string? SubSource { get; set; }

        public QuoteDTO() { }
        public QuoteDTO(Quote QuoteItem) =>
        (Id, Content, Source, SubSource) = (QuoteItem.Id, QuoteItem.Content, QuoteItem.Source, QuoteItem.SubSource);
    }

}