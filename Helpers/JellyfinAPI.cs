using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Encodings.Web;

public static class JellyfinAPI
{
    public static HttpRequestMessage CreateApiRequest(Uri JellyfinUri, string ApiKey)
    {
        HttpClient httpClient = new HttpClient();
        var Request = new HttpRequestMessage(HttpMethod.Get, JellyfinUri);
        //Request.Headers.Add("Accept", "application/json");
        Request.Headers.Add("Authorization", $"MediaBrowser Token=\"{ApiKey}\"");
        return Request;
    }
    public static class User
    {
        public static JObject Login(Uri JellyfinUri, string Username, string Password)
        {
            HttpClient httpClient = new HttpClient();
            var RequestBody = new
            {
                Username = Username,
                Pw = Password
            };
            var Request = new HttpRequestMessage(HttpMethod.Post, $"{JellyfinUri}Users/authenticatebyname");
            Request.Content = new StringContent(JsonConvert.SerializeObject(RequestBody), Encoding.UTF8, "application/json");
            Request.Headers.Add("Authorization", "MediaBrowser Client=\"Jellyfin Web\", Device=\"JF2SMS\", DeviceId=\"JF2SMS\", Version=\"10.10.7\"");
            var Authentication = httpClient.Send(Request);
            return JObject.Parse(Authentication.Content.ReadAsStringAsync().Result);
        }
    }
    public static class Get
    {
        public static class Movie
        {
            public static Uri Stream(Uri JellyfinUri, string ApiKey, string MovieId)
            {
                return new Uri($"{JellyfinUri}Videos/{MovieId}/Stream.mp4?videoBitrate=3500&audioCodec=mp3&audioBitRate=128&audioChannels=2");
            }
            public async static Task<JFSearchResult> Metadata(Uri JellyfinUri, string ApiKey, string MovieQuery)
            {
                var JellyfinRequest = CreateApiRequest(JellyfinUri, ApiKey);
                JellyfinRequest.RequestUri = new Uri($"{JellyfinUri}Search/Hints?searchTerm={MovieQuery}&includeItemTypes=Movie&excludeItemTypes=Series,Episode");
                using (HttpClient httpClient = new HttpClient())
                {
                    var SeriesSearchResults = await httpClient.SendAsync(JellyfinRequest);
                    return JsonConvert.DeserializeObject<JFSearchResult>(SeriesSearchResults.Content.ReadAsStringAsync().Result);
                }
            }
            public async static Task<SeasonSearchResults> AllMovies(Uri JellyfinUri,string UserId, string ApiKey)
            {
                var JellyfinRequest = CreateApiRequest(JellyfinUri, ApiKey);
                JellyfinRequest.RequestUri = new Uri($"{JellyfinUri}Users/{UserId}/Items?SortBy=SortName&SortOrder=Ascending&IncludeItemTypes=Movie&Recursive=true&StartIndex=0&ParentId=f137a2dd21bbc1b99aa5c0f6bf02a805");
                using (HttpClient httpClient = new HttpClient())
                {
                    var SeriesSearchResults = await httpClient.SendAsync(JellyfinRequest);
                    return JsonConvert.DeserializeObject<SeasonSearchResults>(SeriesSearchResults.Content.ReadAsStringAsync().Result);
                }
            }
        }
        public static class Series
        {
            public async static Task<SeasonSearchResults> AllSeries(Uri JellyfinUri, string UserId, string ApiKey)
            {
                var JellyfinRequest = CreateApiRequest(JellyfinUri, ApiKey);
                JellyfinRequest.RequestUri = new Uri($"{JellyfinUri}Users/{UserId}/Items?SortBy=SortName&SortOrder=Ascending&IncludeItemTypes=Series&Recursive=true&StartIndex=0&ParentId=a656b907eb3a73532e40e44b968d0225");
                using (HttpClient httpClient = new HttpClient())
                {
                    var SeriesSearchResults = await httpClient.SendAsync(JellyfinRequest);
                    return JsonConvert.DeserializeObject<SeasonSearchResults>(SeriesSearchResults.Content.ReadAsStringAsync().Result);
                }
            }
            public async static Task<JFSearchResult> Metadata(Uri JellyfinUri, string ApiKey, string SeriesQuery)
            {
                var JellyfinRequest = CreateApiRequest(JellyfinUri, ApiKey);
                JellyfinRequest.RequestUri = new Uri($"{JellyfinUri}Search/Hints?searchTerm={SeriesQuery}&includeItemTypes=Series&excludeItemTypes=Episode");
                using (HttpClient httpClient = new HttpClient())
                {
                    var SeriesSearchResults = await httpClient.SendAsync(JellyfinRequest);
                    return JsonConvert.DeserializeObject<JFSearchResult>(SeriesSearchResults.Content.ReadAsStringAsync().Result);
                }
            }
            public async static Task<SeasonSearchResults> Seasons(Uri JellyfinUri, string ApiKey, string SeriesId)
            {
                var JellyfinRequest = CreateApiRequest(JellyfinUri, ApiKey);
                JellyfinRequest.RequestUri = new Uri($"{JellyfinUri}Shows/{SeriesId}/Seasons");
                using (HttpClient httpClient = new HttpClient())
                {
                    var SeriesSeasonResults = await httpClient.SendAsync(JellyfinRequest);
                    return JsonConvert.DeserializeObject<SeasonSearchResults>(SeriesSeasonResults.Content.ReadAsStringAsync().Result);
                }
            }
            public async static Task<EpisodeList> Episodes(Uri JellyfinUri, string ApiKey, string SeriesId, string SeasonIndex)
            {
                var JellyfinRequest = CreateApiRequest(JellyfinUri, ApiKey);
                JellyfinRequest.RequestUri = new Uri($"{JellyfinUri}Shows/{SeriesId}/Episodes?season={SeasonIndex}");
                using (HttpClient httpClient = new HttpClient())
                {
                    return JsonConvert.DeserializeObject<EpisodeList>(httpClient.SendAsync(JellyfinRequest).Result.Content.ReadAsStringAsync().Result);
                }
            }
            public static class Episode
            {
                public static Uri Stream(Uri JellyfinUri, string ApiKey, string EpisodeId)
                {
                    return new Uri($"{JellyfinUri}Videos/{EpisodeId}/Stream?videoBitrate=200000&audioCodec=mp3&audioBitRate=128&audioChannels=2");
                }
            }
            public static class Season
            {
                public static List<Uri> EpisodeStreamList(Uri JellyfinUri, string ApiKey, string SeriesId, string SeasonIndex)
                {
                    var SeasonEpisodeList = JellyfinAPI.Get.Series.Episodes(JellyfinUri, ApiKey, SeriesId, SeasonIndex).Result;
                    List<Uri> EpisodeUriList = new List<Uri>();
                    foreach (var Episode in SeasonEpisodeList.Items)
                    {
                        EpisodeUriList.Add(new Uri($"{JellyfinUri}Videos/{Episode.Id}/Stream?maxWidth=480&maxHeight=640&width=480&height=640&videoBitrate=3500&audioCodec=mp3&audioBitRate=128&audioChannels=2"));
                    }
                    return EpisodeUriList;
                }
            }
        }
    }
}