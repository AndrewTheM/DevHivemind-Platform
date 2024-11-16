namespace Aggregator.DTO;

public class PostDto
{
    public Guid Id { get; set; }

    public string Title { get; set; }

    public string TitleIdentifier { get; set; }

    public string Author { get; set; }

    public DateTime PublishedOn { get; set; }

    public string RelativePublishTime { get; set; }

    public string ThumbnailPath { get; set; }
}
