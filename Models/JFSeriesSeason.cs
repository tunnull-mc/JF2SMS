
public class SeasonSearchResults
{
    public SeasonItem[] Items { get; set; }
    public int TotalRecordCount { get; set; }
    public int StartIndex { get; set; }
}

public class SeasonItem
{
    public string Name { get; set; }
    public string ServerId { get; set; }
    public string Id { get; set; }
    public DateTime PremiereDate { get; set; }
    public object ChannelId { get; set; }
    public int ProductionYear { get; set; }
    public int IndexNumber { get; set; }
    public bool IsFolder { get; set; }
    public string Type { get; set; }
    public string ParentLogoItemId { get; set; }
    public string ParentBackdropItemId { get; set; }
    public string[] ParentBackdropImageTags { get; set; }
    public string SeriesName { get; set; }
    public string SeriesId { get; set; }
    public string SeriesPrimaryImageTag { get; set; }
    public Imagetags ImageTags { get; set; }
    public object[] BackdropImageTags { get; set; }
    public string ParentLogoImageTag { get; set; }
    public string ParentThumbItemId { get; set; }
    public string ParentThumbImageTag { get; set; }
    public string LocationType { get; set; }
    public string MediaType { get; set; }
}