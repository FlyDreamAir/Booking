namespace FlyDreamAir.Data.Model;

public class Post
{
    public required Guid Id { get; set; }
    public required Uri ImageSrc { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required DateTime PublishDate { get; set; }
    public required Uri Url { get; set; }
}
