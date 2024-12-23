public class ScryfallCard
{
    public string name { get; set; }
    public string set { get; set; }
    public string collector_number { get; set; }
    public ImageUris image_uris { get; set; }
}

public class ImageUris
{
    public string Small { get; set; }
    public string Normal { get; set; }
    public string Large { get; set; }
    public string Png { get; set; }
    public string ArtCrop { get; set; }
    public string BorderCrop { get; set; }
}