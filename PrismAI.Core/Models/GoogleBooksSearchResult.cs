using System.ComponentModel;

namespace PrismAI.Core.Models;

/// <summary>
/// Represents a book search result from the Google Books API.
/// </summary>
public class GoogleBooksSearchResult : WebSearchResult
{
    /// <summary>
    /// Gets or sets the unique Google Books identifier for the book.
    /// </summary>
    [Description("The unique Google Books identifier for the book.")]
    public string GoogleId { get; set; }

    /// <summary>
    /// Gets or sets the URL of the book's thumbnail image.
    /// </summary>
    [Description("The URL of the book's thumbnail image.")]
    public string BookThumbnailUrl { get; set; }

    /// <summary>
    /// Gets or sets the authors of the book, as a comma-separated string.
    /// </summary>
    [Description("The authors of the book, as a comma-separated string.")]
    public string Authors { get; set; }

    /// <summary>
    /// Gets or sets the publisher of the book.
    /// </summary>
    [Description("The publisher of the book.")]
    public string? Publisher { get; set; }

    /// <summary>
    /// Gets or sets the published date of the book.
    /// </summary>
    [Description("The published date of the book.")]
    public string? PublishedDate { get; set; }

    /// <summary>
    /// Gets or sets the number of pages in the book.
    /// </summary>
    [Description("The number of pages in the book.")]
    public int? PageCount { get; set; }

    /// <summary>
    /// Gets or sets the average rating of the book.
    /// </summary>
    [Description("The average rating of the book.")]
    public double? AverageRating { get; set; }

    /// <summary>
    /// Gets or sets the number of ratings the book has received.
    /// </summary>
    [Description("The number of ratings the book has received.")]
    public int? RatingsCount { get; set; }

    /// <summary>
    /// Gets or sets the categories or genres of the book, as a comma-separated string.
    /// </summary>
    [Description("The categories or genres of the book, as a comma-separated string.")]
    public string? Categories { get; set; }

    
}