namespace TestAssesment.DataModel
{
    public sealed class Book
    {
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string Genre { get; set; } = "";
        public int Year { get; set; } = 0;
        public string Publisher { get; set; } = "";
    }

    public sealed class InvalidBook
    {
        public string Title { get; set; } = "";
        public string Reason { get; set; } = "";
    }

    public sealed class BooksResponse
    {
        public List<Book> ValidBooks { get; } = new();
        public List<InvalidBook> InvalidBooks { get; } = new();
    }

}
