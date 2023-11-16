using System.Collections.Generic;

namespace QP.Storage.WebApp.Settings;

public class ImageResizeSettings
{
    public string BasePath = "/_reduce_settings";
    public string SearchParameter { get; set; }
    public string WidthParameter { get; set; }
    public List<ReduceSize> ReduceSizes { get; set; }
    public string[] ExtensionsAllowedToResize { get; set; }
    public string ResizedImageTemplate { get; set; }
}