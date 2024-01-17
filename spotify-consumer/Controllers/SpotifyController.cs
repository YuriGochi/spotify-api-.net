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
        private readonly string _clientId;
        private readonly string _clientSecret;

        public SpotifyController(ILogger<SpotifyController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _clientId = _configuration["SpotifyApi:ClientId"];
            _clientSecret = _configuration["SpotifyApi:ClientSecret"];
    }

        [HttpGet("track/{trackId}/{loginName}")]
        public async Task<ActionResult<MusicInfo>> GetTrackInfo(string trackId, string loginName)
        {
            var tokenSpotify = new TokenSpotify(_clientId, _clientSecret);
            var accessToken = await tokenSpotify.GetAccessToken();

            if (accessToken == null)
            {
                return Unauthorized("Failed to obtain access token.");
            }

            var spotify = new SpotifyClient(accessToken);

            try
            {
                var track = await spotify.Tracks.Get(trackId);
                _logger.LogInformation($"listando track por id, requisitado por {loginName}");
                var musicInfo = new MusicInfo
                {
                    Name = track.Name,
                    Artists = track.Artists.Select(a => new ArtistInfo { Name = a.Name }).ToList(),
                    Album = new AlbumInfo
                    {
                        name = track.Album.Name,
                        release_date = track.Album.ReleaseDate,
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

        [HttpGet("search/track/{trackName}/{loginName}")]
        public async Task<ActionResult<MusicInfo>> GetTrack(string trackName, string loginName)
        {
            var tokenSpotify = new TokenSpotify(_clientId, _clientSecret);
            var accessToken = await tokenSpotify.GetAccessToken();

            if (accessToken == null)
            {
                return Unauthorized("Failed to obtain access token.");
            }

            var spotify = new SpotifyClient(accessToken);
            _logger.LogInformation($"listando track por nome");

            try
            {
                var searchRequest = new SearchRequest(SearchRequest.Types.Track, trackName);

                var searchResponse = await spotify.Search.Item(searchRequest);
                _logger.LogInformation($"listando track por nome, requisitado por {loginName}");

                if (searchResponse != null && searchResponse.Tracks.Items.Any())
                {
                    var firstTrack = searchResponse.Tracks.Items.First();

                    var musicInfo = new MusicInfo
                    {
                        Name = firstTrack.Name,
                        Artists = firstTrack.Artists.Select(a => new ArtistInfo { Name = a.Name }).ToList(),
                        Album = new AlbumInfo
                        {
                            name = firstTrack.Album.Name,
                            release_date = firstTrack.Album.ReleaseDate,
                        }
                    };

                    return Ok(musicInfo);
                }
                else
                {
                    return NotFound("No track found with the given name.");
                }
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

        [HttpGet("search/album/{albumName}/{loginName}")]
        public async Task<ActionResult<MusicInfo>> GetAlbum(string albumName, string loginName)
        {
            var tokenSpotify = new TokenSpotify(_clientId, _clientSecret);
            var accessToken = await tokenSpotify.GetAccessToken();
            _logger.LogInformation($"listando track por nome, requisitado por {loginName}");

            if (accessToken == null)
            {
                return Unauthorized("Failed to obtain access token.");
            }

            var spotify = new SpotifyClient(accessToken);

            try
            {
                var searchRequest = new SearchRequest(SearchRequest.Types.Album, albumName);

                var searchResponse = await spotify.Search.Item(searchRequest);
                _logger.LogInformation($"listando track por nome");

                if (searchResponse != null && searchResponse.Albums.Items.Any())
                {
                    var firstAlbum = searchResponse.Albums.Items.First();

                    var albumInfo = new AlbumInfo
                    {
                        name = firstAlbum.Name,
                        artists = firstAlbum.Artists.Select(a => new ArtistInfo { Name = a.Name }).ToList(),
                        total_tracks = firstAlbum.TotalTracks,
                    };

                    return Ok(albumInfo);
                }
                else
                {
                    return NotFound("No track found with the given name.");
                }
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
    public string album_type { get; set; }
    public List<ArtistInfo> artists { get; set; }
    public string href { get; set; }
    public string id { get; set; }
    public string name { get; set; }
    public string release_date { get; set; }
    public string release_date_precision { get; set; }
    public int total_tracks { get; set; }
    public string type { get; set; }
    public string uri { get; set; }
}
