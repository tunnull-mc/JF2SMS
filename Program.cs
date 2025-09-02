using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static JellyfinAPI.Get.Series;

var SettingsFile = File.ReadAllText("settings.json");
var Settings = JObject.Parse(SettingsFile);
var JellyfinUri = new Uri(Settings.SelectToken("Jellyfin.Hostname").ToString());
var Username = Settings.SelectToken("Jellyfin.Username").ToString();
var Password = Settings.SelectToken("Jellyfin.Password").ToString();
var SMSDirectory = Settings.SelectToken("JF2SMS.DownloadDir").ToString();
var FFMpegExecutable = Settings.SelectToken("JF2SMS.FFMpegExecutable").ToString();

Console.OutputEncoding = Encoding.Unicode;

if(!Directory.Exists($"{SMSDirectory}\\Movies"))
{
    Directory.CreateDirectory($"{SMSDirectory}\\Movies");
}
if (!Directory.Exists($"{SMSDirectory}\\Shows"))
{
    Directory.CreateDirectory($"{SMSDirectory}\\Shows");
}


Regex IllegalCharRegex = new Regex(@"[\\/:*?""<>|]");

//Auth

var LoginPayload = JellyfinAPI.User.Login(JellyfinUri, Username, Password);
var ApiKey = LoginPayload.SelectToken("AccessToken").ToString();
var UserId = LoginPayload.SelectToken("User.Id").ToString();

var PrevCurrentPosition = 0;
var CurrentPosition = 0;
var SearchTerm = "";
var JFSeries = new SeasonSearchResults();
var SearchType = "Movie";
JFSeries = JellyfinAPI.Get.Movie.AllMovies(JellyfinUri, UserId, ApiKey).Result;
var MenuType = "SeriesSelection";
Dictionary<string, string> SeriesNameIds = new Dictionary<string, string>();
Dictionary<string, string> SeriesSeasons = new Dictionary<string, string>();
foreach (var Series in JFSeries.Items)
{
    if (!SeriesNameIds.ContainsKey(Series.Name))
    {
        SeriesNameIds.Add(Series.Name, Series.Id);

    }
}
string[][] Chunks = Chunks = SeriesNameIds.Keys.Chunk(15).ToArray();
var ChunkPosition = 0;
UpdateMenu(SeriesNameIds);
while (true)
{
    switch (MenuType)
    {
        case "SeriesSelection":
            Chunks = SeriesNameIds.Keys.Chunk(20).ToArray();
            UpdateMenu(SeriesNameIds);
            break;
        case "SeasonSelection":
            Chunks = SeriesSeasons.Keys.Chunk(20).ToArray();
            UpdateMenu(SeriesSeasons);
            break;
    }
    ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
    Console.Clear();
    if (consoleKeyInfo.Key == ConsoleKey.DownArrow)
    {
        if (CurrentPosition <= Chunks[ChunkPosition].Length - 2)
        {
            CurrentPosition++;
        }
        else if (CurrentPosition >= Chunks[ChunkPosition].Length - 2)
        {
            CurrentPosition = 0;
        }
        else
        {
            Console.WriteLine("EOL");

        }
    }
    if (consoleKeyInfo.Key == ConsoleKey.UpArrow)
    {
        if (CurrentPosition == 0)
        {
            CurrentPosition = Chunks[ChunkPosition].Length;
        }
        if (CurrentPosition <= Chunks[ChunkPosition].Length)
        {
            CurrentPosition--;
        }
        else if (CurrentPosition >= Chunks[ChunkPosition].Length)
        {
            CurrentPosition = Chunks[ChunkPosition].Length - 2;
        }
        else
        {
            Console.WriteLine("EOL");

        }
    }
    if (consoleKeyInfo.Key == ConsoleKey.RightArrow)
    {
        if (ChunkPosition <= Chunks.Length - 2)
        {
            ChunkPosition++;
        }
    }
    if (consoleKeyInfo.Key == ConsoleKey.LeftArrow)
    {
        if (ChunkPosition - 1 != -1)
        {
            if (ChunkPosition <= Chunks.Length - 1)
            {
                ChunkPosition--;
            }
        }
    }
    if(consoleKeyInfo.Key == ConsoleKey.Tab)
    {
        switch(SearchType)
        {
            case "Movie":
                SearchType = "Show";
                JFSeries = JellyfinAPI.Get.Series.AllSeries(JellyfinUri, UserId, ApiKey).Result;
                break;
            case "Show":
                SearchType = "Movie";
                JFSeries = JellyfinAPI.Get.Movie.AllMovies(JellyfinUri, UserId, ApiKey).Result;
                break;
        }
        ChunkPosition = 0;
        CurrentPosition = 0;
        SeriesNameIds = new Dictionary<string, string>();
        foreach (var Series in JFSeries.Items)
        {
            if (!SeriesNameIds.ContainsKey(Series.Name))
            {
                SeriesNameIds.Add(Series.Name, Series.Id);

            }
        }
        UpdateMenu(SeriesNameIds);
    }
    if (consoleKeyInfo.Key == ConsoleKey.Enter)
    {
        Console.Clear();
        if (MenuType == "SeasonSelection")
        {
            var SeasonIndex = SeriesSeasons[Chunks[ChunkPosition][CurrentPosition]];
            var EpisodeIndex = 0;
            var EpisodeList = JellyfinAPI.Get.Series.Episodes(JellyfinUri, ApiKey, SearchTerm, SeasonIndex).Result;
            if (!Directory.Exists($"{SMSDirectory}\\{EpisodeList.Items.First().SeriesName}\\{EpisodeList.Items.First().SeasonName}"))
            {
                Directory.CreateDirectory($"{SMSDirectory}\\{EpisodeList.Items.First().SeriesName}\\{EpisodeList.Items.First().SeasonName}");
            }
            foreach (var Episode in EpisodeList.Items)
            {
                var SeriesDestination = $"{Episode.SeriesName}\\{Episode.SeasonName}\\E{Episode.IndexNumber:D2} - {IllegalCharRegex.Replace(Episode.Name, "-")}";
                var EpisodeStream = JellyfinAPI.Get.Series.Episode.Stream(JellyfinUri, ApiKey, Episode.Id);
                Console.WriteLine($"Converting {Episode.SeriesName}/{Episode.SeasonName}/{Episode.Name}");
                FFMpeg.Transcode(FFMpegExecutable, EpisodeStream.ToString(), $"{SMSDirectory}\\{SeriesDestination}");
            }
            break;
        }
        if(SearchType == "Show")
        {
            SMSDirectory = $"{SMSDirectory}\\Shows";

            MenuType = "SeasonSelection";
            PrevCurrentPosition = CurrentPosition;
            SearchTerm = SeriesNameIds[Chunks[ChunkPosition][PrevCurrentPosition]];
            CurrentPosition = 0;
            ChunkPosition = 0;
            var SeasonSearch = JellyfinAPI.Get.Series.Seasons(JellyfinUri, ApiKey, SearchTerm).Result;
            foreach (var Season in SeasonSearch.Items)
            {
                SeriesSeasons.Add(Season.Name, Season.IndexNumber.ToString());
            }
        }
        if(SearchType == "Movie")
        {
            SMSDirectory = $"{SMSDirectory}\\Movies";

            SearchTerm = SeriesNameIds[Chunks[ChunkPosition][CurrentPosition]];
            var MovieName = Chunks[ChunkPosition][CurrentPosition];
            var MovieData = JellyfinAPI.Get.Movie.Metadata(JellyfinUri, ApiKey, MovieName).Result.SearchHints.First();
            var MovieDestination = $"{IllegalCharRegex.Replace(MovieData.Name, "-")} ({MovieData.ProductionYear})";
            var MovieStream = JellyfinAPI.Get.Movie.Stream(JellyfinUri, ApiKey, MovieData.Id);
            Console.WriteLine($"Converting {MovieData.Name} ({MovieData.ProductionYear})");
            FFMpeg.Transcode(FFMpegExecutable, MovieStream.ToString(), $"{SMSDirectory}\\{MovieDestination}");
        }
    }
    if (consoleKeyInfo.Key == ConsoleKey.Escape)
    if (consoleKeyInfo.Key == ConsoleKey.Backspace)
    {
        Console.Clear();
        ChunkPosition = PrevCurrentPosition;
        SeriesSeasons = new Dictionary<string, string>();
        MenuType = "SeriesSelection";
    }

}

void UpdateMenu(Dictionary<string, string> MenuItems)
{
    Console.Clear();
    switch (MenuType)
    {
        case "SeriesSelection":
            Console.WriteLine($"Page {ChunkPosition}/{Chunks.Length - 1}\n");
            break;
        case "SeasonSelection":
            Console.WriteLine("Select season for download.\n");
            break;
    }
    //Console.WriteLine($"CHUNKPOS: {ChunkPosition}; CURSPOS: {CurrentPostion}");
    foreach (var Item in Chunks[ChunkPosition])
    {
        if (CurrentPosition == Array.IndexOf(Chunks[ChunkPosition], Item))
        {
            Console.BackgroundColor = ConsoleColor.Gray;
            Console.ForegroundColor = ConsoleColor.Black;
            Console.WriteLine($"> {Item} <");
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;

        }
        else
        {
            Console.WriteLine(Item);
        }
    }
    if (ChunkPosition == Chunks.Length - 1)
    {
        Console.WriteLine("<- Last Listing");

    }
    else if (ChunkPosition != 0 | ChunkPosition == Chunks.Length - 3)
    {
        Console.WriteLine("<- Last Listing | -> Next Listing");
    }
    else if (ChunkPosition == 0)
    {
        Console.WriteLine("-> Next Listing");
    }
    Console.CursorTop = Console.WindowTop + Console.WindowHeight - 1;
    Console.Write($"↑/↓ Move Cursor | ←/→ Switch Page | ↵ Select | ⌫  Back");
}

/*
switch(args[0])
{
    case "queue":
        switch(args[1])
        {
            case "create":
                File.Create("queue.dls");
                break;
            case "add":
                if(!File.Exists("queue.dls"))
                {
                    Console.WriteLine("ERROR: Queue has not been created.");
                    break;
                }
                StringBuilder SB = new StringBuilder();
                SB.Append(File.ReadAllText("queue.dls"));
                if (!string.IsNullOrEmpty(SB.ToString()))
                {
                    SB.Append("\r\n");
                }
                switch (args[2])
                {
                    case "movie":
                        SB.Append(
                            args[2] +
                            "+" +
                            $"\"{args[3]}\"" +
                            $";"
                            );
                        File.WriteAllText("queue.dls", SB.ToString());
                        break;
                    case "show":
                        var SeasonEpisodeSelector = args[4].Split(':');
                        var SeasonIndex = "";
                        int EpisodeIndex = 0;
                        SeasonIndex = SeasonEpisodeSelector[0];
                        if (SeasonEpisodeSelector.Length > 1)
                        {
                            EpisodeIndex = Int32.Parse(SeasonEpisodeSelector[1]);
                            SB.Append(
                                args[2] +
                                "+" +
                                $"\"{args[3]}\"" +
                                "+" +
                                SeasonIndex +
                                "/" +
                                EpisodeIndex +
                                $";"
                                );
                        }
                        else
                        {
                            SB.Append(
                                args[2] +
                                "+" +
                                $"\"{args[3]}\"" +
                                "+" +
                                SeasonIndex +
                                "/" +
                                "a" +
                                $";"
                                );
                        }
                        File.WriteAllText("queue.dls", SB.ToString());
                        break;
                }
                break;
            case "clear":
                File.Delete("queue.dls");
                break;
        }
        break;
    case "download":
        switch(args[1])
        {
            case "movie":
                SMSDirectory = $"{SMSDirectory}\\Movies";
                var MovieName = args[2];
                var MovieData = JellyfinAPI.Get.Movie.Metadata(JellyfinUri, ApiKey, MovieName).Result.SearchHints.First();
                var MovieDestination = $"{IllegalCharRegex.Replace(MovieData.Name, "-")} ({MovieData.ProductionYear})";
                var MovieStream = JellyfinAPI.Get.Movie.Stream(JellyfinUri, ApiKey, MovieData.Id);
                FFMpeg.Transcode(MovieStream.ToString(), $"{SMSDirectory}\\{MovieDestination}");
                break;
            case "show":
                SMSDirectory = $"{SMSDirectory}\\Shows";
                var SeriesName = args[2];
                var SeasonEpisodeSelector = args[3].Split(':');
                var SeasonIndex = "";
                int EpisodeIndex = 0;
                SeasonIndex = SeasonEpisodeSelector[0];
                if (SeasonEpisodeSelector.Length > 1)
                {
                    EpisodeIndex = Int32.Parse(SeasonEpisodeSelector[1]) - 1;
                }
                else
                {
                    EpisodeIndex = 0;
                }
                var SeriesData = JellyfinAPI.Get.Series.Metadata(JellyfinUri, ApiKey, SeriesName).Result;
                var EpisodeList = JellyfinAPI.Get.Series.Episodes(JellyfinUri, ApiKey, SeriesData.SearchHints.First().Id, SeasonIndex).Result;
                if (!Directory.Exists($"{SMSDirectory}\\{EpisodeList.Items.First().SeriesName}\\{EpisodeList.Items.First().SeasonName}"))
                {
                    Directory.CreateDirectory($"{SMSDirectory}\\{EpisodeList.Items.First().SeriesName}\\{EpisodeList.Items.First().SeasonName}");
                }
                if(EpisodeIndex == 0)
                {
                    foreach (var Episode in EpisodeList.Items)
                    {
                        var SeriesDestination = $"{Episode.SeriesName}\\{Episode.SeasonName}\\E{Episode.IndexNumber:D2} - {IllegalCharRegex.Replace(Episode.Name, "-")}";
                        var EpisodeStream = JellyfinAPI.Get.Series.Episode.Stream(JellyfinUri, ApiKey, Episode.Id);
                        FFMpeg.Transcode(EpisodeStream.ToString(), $"{SMSDirectory}\\{SeriesDestination}");
                    }
                }
                else
                {
                    var SeriesDestination = $"{EpisodeList.Items[EpisodeIndex].SeriesName}\\{EpisodeList.Items[EpisodeIndex].SeasonName}\\E{EpisodeList.Items[EpisodeIndex].IndexNumber:D2} - {IllegalCharRegex.Replace(EpisodeList.Items[EpisodeIndex].Name, "-")}";
                    var EpisodeStream = JellyfinAPI.Get.Series.Episode.Stream(JellyfinUri, ApiKey, EpisodeList.Items[EpisodeIndex].Id);
                    FFMpeg.Transcode(EpisodeStream.ToString(), $"{SMSDirectory}\\{SeriesDestination}");

                }

                break;
        }
        break;
}
*/