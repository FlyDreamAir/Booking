using FlyDreamAir.Data.Model;

namespace FlyDreamAir.Services;

public class NewsService {
	private readonly List<News> _newsList = [
		new() {
			Id = "b1828ed0-c658-41b6-a0ae-f68a46cc01f2",
			ImageSrc = "/assets/news_images/syd.png",
			Title = "Sydney Skyline Transformed: Innovative Architecture Ushers in New Era",
			Description = "The iconic Sydney skyline sees a dramatic transformation as cutting-edge buildings redefine urban aesthetics, promising a future where sustainability and design go hand in hand",
			PublishDate = new DateTime(2024, 5, 20),
			url = "/Explore/sydney-skyline-transformedZ-innovative-architecture-ushers-in-new-era-b1828ed0-c658-41b6-a0ae-f68a46cc01f2"
		},
		new() {
			Id = "24cd9029-e86e-4586-ae36-663de87f0a39",
			ImageSrc = "/assets/news_images/mel.png",
			Title = "Melbourne Marvel: City Unveils Green Oasis Amidst Urban Jungle",
			Description = "Melbourne introduces a breathtaking green space, blending nature with urban living, as part of a visionary project to create a sustainable cityscape for future generations",
			PublishDate = new DateTime(2024, 5, 20),
			url = "/Explore/melbourne-marvel-city-unveils-green-oasis-amidst-urban-jungle-24cd9029-e86e-4586-ae36-663de87f0a39"
		},
		new() {
			Id = "d49081b8-b66c-4d3b-ac94-306ddf150315",
			ImageSrc = "/assets/news_images/han.png",
			Title = "Hanoi’s Historic Heartbeat: Ancient City Embraces the Digital Age",
			Description = "In a blend of tradition and technology, Hanoi’s historic streets are now home to digital art installations, bringing a new pulse to the timeless capital",
			PublishDate = new DateTime(2024, 5, 20),
			url = "/Explore/hanois-historic-heartbeat-ancient-city-embraces-the-digital-age-d49081b8-b66c-4d3b-ac94-306ddf150315"
		},
		new() {
			Id = "a3f4c818-638b-4846-b973-f79f3fdbf042",
			ImageSrc = "/assets/news_images/sgn.png",
			Title = "Ho Chi Minh City Soars: Skyline Reimagined with Eco-Friendly Skyscrapers",
			Description = "Ho Chi Minh City unveils a new generation of eco-friendly skyscrapers, soaring towards a sustainable future while honoring its rich cultural heritage",
			PublishDate = new DateTime(2024, 5, 20),
			url = "/Explore/ho-chi-minh-city-soars-skyline-reimagined-with-ecoMfriendly-skyscrapers-a3f4c818-638b-4846-b973-f79f3fdbf042"
		},
		new() {
			Id = "5259c25e-adf0-42f1-8eae-8aa5751150eb",
			ImageSrc = "/assets/news_images/hnd.png",
			Title = "Tokyo Tomorrow: City of the Future Unveiled in Dazzling Display",
			Description = "Tokyo sets a new global standard with its innovative ‘City of the Future’ project, featuring smart technology and eco-friendly initiatives that promise a brighter, more sustainable tomorrow",
			PublishDate = new DateTime(2024, 5, 20),
			url = "/Explore/tokyo-tomorrow-city-of-the-future-unveiled-in-dazzling-display-5259c25e-adf0-42f1-8eae-8aa5751150eb"
		}
	];
	
	public NewsService() {}
	
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async IAsyncEnumerable<News> GetNewsAsync()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        foreach (var news in _newsList)
        {
            yield return news;
        }
    }
}
