
public class MusicAlbumSearchResults
{
    public MusicAlbumItem[] Items { get; set; }
    public int TotalRecordCount { get; set; }
    public int StartIndex { get; set; }
}

public class MusicAlbumItem
{
    public string Name { get; set; }
    public string ServerId { get; set; }
    public string Id { get; set; }
    public DateTime PremiereDate { get; set; }
    public object ChannelId { get; set; }
    public long RunTimeTicks { get; set; }
    public int ProductionYear { get; set; }
    public bool IsFolder { get; set; }
    public string Type { get; set; }
    public string ParentBackdropItemId { get; set; }
    public string[] ParentBackdropImageTags { get; set; }
    public string[] Artists { get; set; }
    public string Album { get; set; }
    public string AlbumArtist { get; set; }
    public Albumartist[] AlbumArtists { get; set; }
    public Imagetags ImageTags { get; set; }
    public object[] BackdropImageTags { get; set; }
    public string LocationType { get; set; }
    public string MediaType { get; set; }
    public float NormalizationGain { get; set; }
    public string IndexNumber { get; set; }
}

public class Imagetags
{
    public string Primary { get; set; }
}
public class Albumartist
{
    public string Name { get; set; }
    public string Id { get; set; }
}
