using System;
using System.Collections.Generic;

namespace QuotesApi.Models
{
    public partial class Quote
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public string? Source { get; set; }
        public string? SubSource { get; set; }
        public DateTime? WhenAdded { get; set; }
    }
}
