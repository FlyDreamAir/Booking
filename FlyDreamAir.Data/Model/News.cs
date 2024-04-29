namespace FlyDreamAir.Data.Model;

public class News {
	public required string Id {get; set;}
	public required string ImageSrc {get; set;}
	public required string Title {get; set;}
	public required string Description {get; set;}
	public required DateTime PublishDate {get; set;}
	public required string url {get; set;}
}
