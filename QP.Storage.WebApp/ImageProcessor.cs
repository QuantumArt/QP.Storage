using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace QP.Storage.WebApp;

public class ImageProcessor
{
    public async Task ResizeImage(string path, decimal size, string resizedImage)
    {
        using var image = await Image.LoadAsync(path);
        var width = image.Width;
        var height = image.Height;
        image.Mutate(x => x.Resize((int)(width / size), (int)(height / size)));
        await image.SaveAsync(resizedImage);
    }
}