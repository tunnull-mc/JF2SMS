
public class JFSearchResult
{
    public Searchhint[] SearchHints { get; set; }
    public int TotalRecordCount { get; set; }
}

public class Searchhint
{
    public string ItemId { get; set; }
    public string Id { get; set; }
    public string Name { get; set; }
    public int ProductionYear { get; set; }
    public string PrimaryImageTag { get; set; }
    public string ThumbImageTag { get; set; }
    public string ThumbImageItemId { get; set; }
    public string BackdropImageTag { get; set; }
    public string BackdropImageItemId { get; set; }
    public string Type { get; set; }
    public bool IsFolder { get; set; }
    public long RunTimeTicks { get; set; }
    public string MediaType { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; }
    public object[] Artists { get; set; }
    public object ChannelId { get; set; }
    public float PrimaryImageAspectRatio { get; set; }
}



public class EpisodeList
{
    public SeriesItem[] Items { get; set; }
    public int TotalRecordCount { get; set; }
    public int StartIndex { get; set; }
}

public class SeriesItem
{
    public string Name { get; set; }
    public string ServerId { get; set; }
    public string Id { get; set; }
    public bool HasSubtitles { get; set; }
    public string Container { get; set; }
    public DateTime PremiereDate { get; set; }
    public string OfficialRating { get; set; }
    public object ChannelId { get; set; }
    public float CommunityRating { get; set; }
    public long RunTimeTicks { get; set; }
    public int ProductionYear { get; set; }
    public int IndexNumber { get; set; }
    public int ParentIndexNumber { get; set; }
    public bool IsFolder { get; set; }
    public string Type { get; set; }
    public string ParentLogoItemId { get; set; }
    public string ParentBackdropItemId { get; set; }
    public string[] ParentBackdropImageTags { get; set; }
    public string SeriesName { get; set; }
    public string SeriesId { get; set; }
    public string SeasonId { get; set; }
    public string SeriesPrimaryImageTag { get; set; }
    public string SeasonName { get; set; }
    public string VideoType { get; set; }
    public Imagetags ImageTags { get; set; }
    public object[] BackdropImageTags { get; set; }
    public string ParentLogoImageTag { get; set; }
    public string ParentThumbItemId { get; set; }
    public string ParentThumbImageTag { get; set; }
    public string LocationType { get; set; }
    public string MediaType { get; set; }
}
