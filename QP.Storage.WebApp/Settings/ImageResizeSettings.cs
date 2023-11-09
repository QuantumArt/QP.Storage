namespace QP.Storage.WebApp.Settings;

public class ImageResizeSettings
{
    public string BasePath = "/_reduce_settings";
    public string QueryParameter { get; set; }
    public ReduceSize[] ReduceSizes { get; set; }
    public string[] ExtensionsAllowedToResize { get; set; }
    public string ResizedImageTemplate { get; set; }
}