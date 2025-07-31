using UglyToad.PdfPig;

namespace ReturnlyWebApi.Services;

public interface IPdfProcessingService
{
    /// <summary>
    /// Extracts text from a PDF document based on predefined regions.
    /// The regions are defined in the regionBoundaries dictionary, which maps region names to their
    /// boundaries (page number, xMin, xMax, yMin, yMax).
    /// The method processes each page of the PDF, extracting words that fall within the specified boundaries
    /// and grouping them by lines.
    /// The extracted text is returned as a dictionary where the keys are region names and the values
    /// are arrays of strings representing the lines of text in each region.
    /// </summary>
    /// <param name="fileStream">The PDF file stream</param>
    /// <param name="password">Optional password for encrypted PDFs</param>
    /// <returns>Dictionary of extracted text by region</returns>
    Dictionary<string, string[]> ExtractTextFromPdf(Stream fileStream, string? password = null);
    
    /// <summary>
    /// Extracts text from a PDF document using custom region boundaries.
    /// </summary>
    /// <param name="fileStream">The PDF file stream</param>
    /// <param name="password">Optional password for encrypted PDFs</param>
    /// <param name="customRegionBoundaries">Custom region boundaries to use instead of default</param>
    /// <returns>Dictionary of extracted text by region</returns>
    Dictionary<string, string[]> ExtractTextFromPdf(Stream fileStream, string? password, Dictionary<string, PdfRegion> customRegionBoundaries);
}

public abstract class PdfProcessingService(ILogger<PdfProcessingService> logger) : IPdfProcessingService
{
    private readonly ILogger<PdfProcessingService> _logger = logger;

    public Dictionary<string, string[]> ExtractTextFromPdf(Stream fileStream, string? password = null)
    {
        return ExtractTextFromPdf(fileStream, password, GetRegionBoundaries());
    }

    public Dictionary<string, string[]> ExtractTextFromPdf(Stream fileStream, string? password, Dictionary<string, PdfRegion> customRegionBoundaries)
    {
        using var document = PdfDocument.Open(fileStream, new ParsingOptions { Password = password ?? string.Empty });
        var regionText = new Dictionary<string, string[]>();
        
        foreach (var page in document.GetPages())
        {
            foreach (var kvp in customRegionBoundaries)
            {
                var name = kvp.Key;
                PdfRegion region = kvp.Value;
                var (regionPage, xMin, xMax, yMin, yMax) = (region.Page, region.XMin, region.XMax, region.YMin, region.YMax);
                
                if (regionPage != page.Number)
                {
                    continue;
                }
                
                var textInRegion = page.GetWords()
                    .Where(w =>
                        w.BoundingBox.Left >= xMin &&
                        w.BoundingBox.Right <= xMax &&
                        w.BoundingBox.Bottom >= yMin &&
                        w.BoundingBox.Top <= yMax)
                    .OrderByDescending(w => w.BoundingBox.Bottom)  // Top to bottom
                    .ThenBy(w => w.BoundingBox.Left)     // Left to right
                    .GroupBy(w => Math.Round(w.BoundingBox.Bottom, 1))  // 1 decimal place to group words on same line
                    .OrderByDescending(g => g.Key)  // Top to bottom
                    .Select(g => string.Join(" ", g.OrderBy(w => w.BoundingBox.Left)
                    .Select(w => w.Text)));

                regionText[name] = [.. textInRegion];
            }
            //foreach (var word in page.GetWords())
            //{
            //    _logger.LogInformation("Extracted word: {Word}. Top: {Top}, Bottom: {Bottom}, Left: {Left}, Right: {Right}", word.Text, word.BoundingBox.Top, word.BoundingBox.Bottom, word.BoundingBox.Left, word.BoundingBox.Right);
            //}
        }

        return regionText;
    }

    protected abstract Dictionary<string, PdfRegion> GetRegionBoundaries();
}

public record PdfRegion(string Name, int Page, double XMin, double XMax, double YMin, double YMax);