using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SixLabors.Shapes;
using Path = System.IO.Path;

namespace AvatarWithRounded
{
    public static class Program
    {
        private const string FILE_PATH = @"./image.jpg";

        private const string OUTPUT_DIRECTORY_NAME = "output";
        private const string OUTPUT_FILE_EXTENSION = ".jpg";
        private const string OUTPUT_FILE_NAME_TEMPLATE = "{0}-{1}{2}";

        private const int IMAGE_WIDTH = 200;
        private const int IMAGE_HEIGHT = 200;

        private static readonly int[] CORNERS =
        {
            20,
            100,
            150
        };

        private static void Main()
        {
            if (Directory.Exists(OUTPUT_DIRECTORY_NAME))
            {
                Directory.Delete(OUTPUT_DIRECTORY_NAME, recursive: true);
            }

            Directory.CreateDirectory(OUTPUT_DIRECTORY_NAME);

            using var img = Image.Load(FILE_PATH);

            foreach (var corner in CORNERS)
            {
                var fileName = Path.GetFileNameWithoutExtension(FILE_PATH);
                var destFileName = string.Format(OUTPUT_FILE_NAME_TEMPLATE, fileName, corner, OUTPUT_FILE_EXTENSION);

                using var destImg = img.Clone(x => x.ConvertToAvatar(new Size(IMAGE_WIDTH, IMAGE_HEIGHT), corner));

                destImg.Save(Path.Combine(OUTPUT_DIRECTORY_NAME, destFileName));
            }
        }

        private static IImageProcessingContext ConvertToAvatar(this IImageProcessingContext processingContext, Size size, float cornerRadius)
        {
            return processingContext.Resize(new ResizeOptions
            {
                Size = size,
                Mode = ResizeMode.Crop
            }).ApplyRoundedCorners(cornerRadius);
        }

        private static IImageProcessingContext ApplyRoundedCorners(this IImageProcessingContext ctx, float cornerRadius)
        {
            var (width, height) = ctx.GetCurrentSize();
            var corners = BuildCorners(width, height, cornerRadius);

            var graphicOptions = new GraphicsOptions(enableAntialiasing: true)
            {
                AlphaCompositionMode = PixelAlphaCompositionMode.DestOut
            };

            return ctx.Fill(graphicOptions, Rgba32.Red, corners);
        }

        private static IPathCollection BuildCorners(int imageWidth, int imageHeight, float cornerRadius)
        {
            var rect = new RectangularPolygon(-0.5f, -0.5f, cornerRadius, cornerRadius);
            var cornerTopLeft = rect.Clip(new EllipsePolygon(cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius));

            var rightPos = imageWidth - cornerTopLeft.Bounds.Width + 1;
            var bottomPos = imageHeight - cornerTopLeft.Bounds.Height + 1;

            var cornerTopRight = cornerTopLeft.RotateDegree(90).Translate(rightPos, 0);
            var cornerBottomLeft = cornerTopLeft.RotateDegree(-90).Translate(0, bottomPos);
            var cornerBottomRight = cornerTopLeft.RotateDegree(180).Translate(rightPos, bottomPos);

            return new PathCollection(cornerTopLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight);
        }
    }
}