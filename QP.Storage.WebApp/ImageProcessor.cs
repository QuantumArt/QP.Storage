using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace QP.Storage.WebApp;

public class ImageProcessor
{
    public void ResizeImage(string path, decimal size, string resizedImage)
    {
        using (Image image = Image.Load(path))
        {
            image.Mutate(x => x.Resize((int)(image.Width / size), (int)(image.Height / size)));
            image.Save(resizedImage);
        }
    }
}