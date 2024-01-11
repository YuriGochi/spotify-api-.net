using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;

namespace spotify_consumer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SpotifyController : ControllerBase
    {

        private readonly ILogger<SpotifyController> _logger;
        private readonly IConfiguration _configuration;

        public SpotifyController(ILogger<SpotifyController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet("track/{trackId}")]
        public async Task<ActionResult<MusicInfo>> GetTrackInfo(string trackId)
        {
            var clientId = _configuration["SpotifyApi:ClientId"];
            var clientSecret = _configuration["SpotifyApi:ClientSecret"];

            var tokenSpotify = new TokenSpotify(clientId, clientSecret);
            var accessToken = await tokenSpotify.GetAccessToken();

            if (accessToken == null)
            {
                return Unauthorized("Failed to obtain access token.");
            }

            var spotify = new SpotifyClient(accessToken);

            try
            {
                var track = await spotify.Tracks.Get(trackId);

                var musicInfo = new MusicInfo
                {
                    Name = track.Name,
                    Artists = track.Artists.Select(a => new ArtistInfo { Name = a.Name }).ToList(),
                    Album = new AlbumInfo
                    {
                        Name = track.Album.Name,
                        ReleaseDate = track.Album.ReleaseDate,
                        Images = track.Album.Images.Select(i => i.Url).ToList()
                    }
                };

                return Ok(musicInfo);
            }
            catch (APIException ex)
            {
                return BadRequest($"Error from Spotify API: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}

public class MusicInfo
{
    public string Name { get; set; }
    public List<ArtistInfo> Artists { get; set; }
    public AlbumInfo Album { get; set; }
}

public class ArtistInfo
{
    public string Name { get; set; }
}

public class AlbumInfo
{
    public string Name { get; set; }
    public string ReleaseDate { get; set; }
    public List<string> Images { get; set; }
}
