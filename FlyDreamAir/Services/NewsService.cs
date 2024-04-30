using FlyDreamAir.Data.Model;

namespace FlyDreamAir.Services;

public class NewsService
{
    private readonly List<Post> _newsList = [
        new()
        {
            Id = new Guid("b1828ed0-c658-41b6-a0ae-f68a46cc01f2"),
            ImageSrc = new Uri("/assets/news_images/syd.png", UriKind.Relative),
            Title = "Sydney Skyline Transformed: Innovative Architecture Ushers in New Era",
            Description = "The iconic Sydney skyline sees a dramatic transformation as cutting-edge buildings redefine urban aesthetics, promising a future where sustainability and design go hand in hand",
            PublishDate = new DateTime(2024, 5, 20),
            Url = new Uri("/Explore/sydney-skyline-transformed-innovative-architecture-ushers-in-new-era-b1828ed0-c658-41b6-a0ae-f68a46cc01f2", UriKind.Relative)
        },
        new()
        {
            Id = new Guid("24cd9029-e86e-4586-ae36-663de87f0a39"),
            ImageSrc = new Uri("/assets/news_images/mel.png", UriKind.Relative),
            Title = "Melbourne Marvel: City Unveils Green Oasis Amidst Urban Jungle",
            Description = "Melbourne introduces a breathtaking green space, blending nature with urban living, as part of a visionary project to create a sustainable cityscape for future generations",
            PublishDate = new DateTime(2024, 5, 20),
            Url = new Uri("/Explore/melbourne-marvel-city-unveils-green-oasis-amidst-urban-jungle-24cd9029-e86e-4586-ae36-663de87f0a39", UriKind.Relative)
        },
        new()
        {
            Id = new Guid("d49081b8-b66c-4d3b-ac94-306ddf150315"),
            ImageSrc = new Uri("/assets/news_images/han.png", UriKind.Relative),
            Title = "Hanoi’s Historic Heartbeat: Ancient City Embraces the Digital Age",
            Description = "In a blend of tradition and technology, Hanoi’s historic streets are now home to digital art installations, bringing a new pulse to the timeless capital",
            PublishDate = new DateTime(2024, 5, 20),
            Url = new Uri("/Explore/hanois-historic-heartbeat-ancient-city-embraces-the-digital-age-d49081b8-b66c-4d3b-ac94-306ddf150315", UriKind.Relative)
        },
        new()
        {
            Id = new Guid("a3f4c818-638b-4846-b973-f79f3fdbf042"),
            ImageSrc = new Uri("/assets/news_images/sgn.png", UriKind.Relative),
            Title = "Ho Chi Minh City Soars: Skyline Reimagined with Eco-Friendly Skyscrapers",
            Description = "Ho Chi Minh City unveils a new generation of eco-friendly skyscrapers, soaring towards a sustainable future while honoring its rich cultural heritage",
            PublishDate = new DateTime(2024, 5, 20),
            Url = new Uri("/Explore/ho-chi-minh-city-soars-skyline-reimagined-with-ecofriendly-skyscrapers-a3f4c818-638b-4846-b973-f79f3fdbf042", UriKind.Relative)
        },
        new()
        {
            Id = new Guid("5259c25e-adf0-42f1-8eae-8aa5751150eb"),
            ImageSrc = new Uri("/assets/news_images/hnd.png", UriKind.Relative),
            Title = "Tokyo Tomorrow: City of the Future Unveiled in Dazzling Display",
            Description = "Tokyo sets a new global standard with its innovative ‘City of the Future’ project, featuring smart technology and eco-friendly initiatives that promise a brighter, more sustainable tomorrow",
            PublishDate = new DateTime(2024, 5, 20),
            Url = new Uri("/Explore/tokyo-tomorrow-city-of-the-future-unveiled-in-dazzling-display-5259c25e-adf0-42f1-8eae-8aa5751150eb", UriKind.Relative)
        }
    ];

    public NewsService() { }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async IAsyncEnumerable<Post> GetPosts()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        foreach (var news in _newsList)
        {
            yield return news;
        }
    }
}
