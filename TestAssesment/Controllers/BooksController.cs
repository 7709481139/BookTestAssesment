using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Xml;
using TestAssesment.Configs;
using TestAssesment.DataModel;

[ApiController]
[Route("books")]
public class BooksController : ControllerBase
{
    private readonly string _xmlFilePath;

    public BooksController(IOptions<FileSettings> options)
    {
        _xmlFilePath = options.Value.BooksXmlPath;
    }

    [HttpGet("valid")]
    public async Task<IActionResult> GetValidBooksAsync()
    {
        if (string.IsNullOrWhiteSpace(_xmlFilePath))
            throw new FileNotFoundException("XML path not configured");

        if (!System.IO.File.Exists(_xmlFilePath))
            throw new FileNotFoundException("XML file not found");

        var xmlContent = await System.IO.File.ReadAllTextAsync(_xmlFilePath);

        if (string.IsNullOrWhiteSpace(xmlContent))
            return NotFound("XML file is empty");

        try
        {
            xmlContent = await System.IO.File.ReadAllTextAsync(_xmlFilePath);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error reading XML file: {ex.Message}");
        }

        if (string.IsNullOrWhiteSpace(xmlContent))
            return NotFound("XML file is empty.");

        var doc = new XmlDocument();
        try
        {
            doc.LoadXml(xmlContent);
        }
        catch (XmlException)
        {
            return BadRequest("Invalid XML format.");
        }

        var bookNodes = doc.SelectNodes("/library/book");
        if (bookNodes == null || bookNodes.Count == 0)
            return NotFound("No book records found.");

        var response = new BooksResponse();

        foreach (XmlNode node in bookNodes)
        {
            var book = new Book
            {
                Title = node["title"]?.InnerText?.Trim(),
                Author = node["author"]?.InnerText?.Trim(),
                Genre = node["genre"]?.InnerText?.Trim(),
                Publisher = node["publisher"]?.InnerText?.Trim()
            };

            if (string.IsNullOrWhiteSpace(book.Title))
            {
                response.InvalidBooks.Add(new InvalidBook
                {
                    Title = "(Unknown)",
                    Reason = "Missing title"
                });
                continue;
            }

            if (!int.TryParse(node["year"]?.InnerText, out int year) || year <= 0)
            {
                response.InvalidBooks.Add(new InvalidBook
                {
                    Title = book.Title,
                    Reason = "Invalid year"
                });
                continue;
            }
            book.Year = year;

            if (string.IsNullOrWhiteSpace(book.Publisher) ||
                !Uri.TryCreate(book.Publisher, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                response.InvalidBooks.Add(new InvalidBook
                {
                    Title = book.Title,
                    Reason = "Invalid publisher"
                });
                continue;
            }

            response.ValidBooks.Add(book);
        }

        if (response.ValidBooks.Count == 0 && response.InvalidBooks.Count == 0)
            return NotFound("No valid or invalid book records found.");

        return Ok(response);
    }
}
