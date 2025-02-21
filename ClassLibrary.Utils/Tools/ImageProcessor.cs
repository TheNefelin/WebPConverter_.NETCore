using ClassLibrary.Utils.Models;
using SkiaSharp;

namespace ClassLibrary.Utils.Tools
{
    public class ImageProcessor
    {
        private const int _quality = 65;
        private const int _targetWidth = 800;
        private const int _targetHeight = 600;

        public byte[] ConvertToBytes_FromPath(string inputPath, int quality = _quality)
        {
            using var imageStream = File.OpenRead(inputPath);                       // Crear un flujo de memoria a partir de la ruta
            using var skBitmap = SKBitmap.Decode(imageStream);                      // Decodificar la imagen desde el flujo
            using var skImage = SKImage.FromBitmap(skBitmap);                       // Crear SKImage desde SKBitmap
            using var skData = skImage.Encode(SKEncodedImageFormat.Webp, quality);  // Codificar a WebP

            return skData.ToArray();                                                // Retornar los bytes del formato WebP
        }

        public byte[] ConvertToBytes_FromStream(Stream imageStream, string outputPath, int quality)
        {
            using var skBitmap = SKBitmap.Decode(imageStream);                      // Decodificar la imagen desde el flujo
            using var skImage = SKImage.FromBitmap(skBitmap);                       // Crear SKImage desde SKBitmap
            using var skData = skImage.Encode(SKEncodedImageFormat.Webp, quality);  // Codificar a WebP

            return skData.ToArray();                                                // Retornar los bytes del formato WebP
        }

        public void SaveImage(byte[] imageBytes, string outputPath)
        {
            if (imageBytes == null || imageBytes.Length == 0)
            {
                throw new ArgumentException("imageBytes cannot be null or empty.", nameof(imageBytes));
            }

            File.WriteAllBytes(outputPath, imageBytes);
        }

        public DataImage ConvertToWebP_FromStream_AndSave(Stream imageStream, string outputPath, int quality)
        {
            using var skBitmap = SKBitmap.Decode(imageStream);                      // Decodificar la imagen desde el flujo

            if (quality <= 0 || quality > 100)
                quality = _quality;

            var dataImage = new DataImage()
            {
                OriginalWidth = skBitmap.Width,
                OriginalHeight = skBitmap.Height,
                OriginalRatio = (float)skBitmap.Width / skBitmap.Height,
                OriginalAspectRatio = SimplifyRatio(skBitmap.Width, skBitmap.Height),
                TargetWidth = skBitmap.Width,
                TargetHeight = skBitmap.Height,
                TargetRatio = (float)skBitmap.Width / skBitmap.Height,
                TargetAspectRatio = SimplifyRatio(skBitmap.Width, skBitmap.Height),
            };

            using var skImage = SKImage.FromBitmap(skBitmap);                       // Crear SKImage desde SKBitmap
            using var skData = skImage.Encode(SKEncodedImageFormat.Webp, quality);  // Codificar a WebP

            var webpBytes = skData.ToArray();                                       // Convierte a Arreglo de Bytes

            SaveImage(webpBytes, outputPath);                                       // Retornar los bytes del formato WebP

            return dataImage;
        }

        public DataImage ConvertToWebP_FromStream_ResizeAndSave(Stream imageStream, string outputPath, int quality, int targetWidth, int targetHeight)
        {
            using var skBitmap = SKBitmap.Decode(imageStream);                      // Decodificar la imagen desde el flujo

            if (quality <= 0 || quality > 100)
                quality = _quality;

            if (targetWidth <= 0 || targetHeight <= 0)
            {
                targetWidth = _targetWidth;
                targetHeight = _targetHeight;
            }

            var dataImage = new DataImage()
            {
                OriginalWidth = skBitmap.Width,
                OriginalHeight = skBitmap.Height,
                OriginalRatio = (float)skBitmap.Width / skBitmap.Height,
                OriginalAspectRatio = SimplifyRatio(skBitmap.Width, skBitmap.Height),
                TargetWidth = targetWidth,
                TargetHeight = targetHeight,
                TargetRatio = (float)targetWidth / targetHeight,
                TargetAspectRatio = SimplifyRatio(targetWidth, targetHeight),
            };

            using var resizedBitmap = CropImage(skBitmap, dataImage);               // Recorta la imagen

            using var skImage = SKImage.FromBitmap(resizedBitmap);                  // Crear SKImage desde SKBitmap
            using var skData = skImage.Encode(SKEncodedImageFormat.Webp, quality);  // Codificar a WebP

            var webpBytes = skData.ToArray();                                       // Convierte a Arreglo de Bytes

            SaveImage(webpBytes, outputPath);                                       // Retornar los bytes del formato WebP

            return dataImage;
        }

        // Redimensiona y recorta la imagen manteniendo la relación de aspecto
        private SKBitmap CropImage(SKBitmap bitmap, DataImage dataImage)
        {
            int cropX, cropY, cropWidth, cropHeight;

            if (dataImage.OriginalRatio > dataImage.TargetRatio)
            {
                cropWidth = (int)(dataImage.OriginalHeight * dataImage.TargetRatio);
                cropHeight = dataImage.OriginalHeight;
                cropX = (dataImage.OriginalWidth - cropWidth) / 2;
                cropY = 0;
            }
            else if (dataImage.OriginalRatio < dataImage.TargetRatio)
            {
                cropWidth = dataImage.OriginalWidth;
                cropHeight = (int)(dataImage.OriginalWidth / dataImage.TargetRatio);
                cropX = 0;
                cropY = (dataImage.OriginalHeight - cropHeight) / 2;
            }
            else
            {
                cropWidth = dataImage.OriginalWidth;
                cropHeight = dataImage.OriginalHeight; ;
                cropX = 0;
                cropY = 0;
            }

            using var croppedBitmap = new SKBitmap(cropWidth, cropHeight);
            using (var canvas = new SKCanvas(croppedBitmap))
            {
                var srcRect = new SKRect(cropX, cropY, cropX + cropWidth, cropY + cropHeight);
                var destRect = new SKRect(0, 0, cropWidth, cropHeight);
                canvas.DrawBitmap(bitmap, srcRect, destRect);
            }

            // Redimensionar el croppedBitmap a las dimensiones objetivo
            var resizedBitmap = new SKBitmap(dataImage.TargetWidth, dataImage.TargetHeight);
            using (var canvas = new SKCanvas(resizedBitmap))
            {
                var srcRect = SKRect.Create(croppedBitmap.Width, croppedBitmap.Height);
                var destRect = SKRect.Create(dataImage.TargetWidth, dataImage.TargetHeight);

                canvas.DrawBitmap(croppedBitmap, srcRect, destRect);
            }

            return resizedBitmap;
        }

        // Método para simplificar el ratio como cadena
        private string SimplifyRatio(int width, int height)
        {
            int gcd = GreatestCommonDivisor(width, height);
            return $"{width / gcd}/{height / gcd}";
        }

        // Calcula el máximo común divisor (MCD)
        private int GreatestCommonDivisor(int w, int h)
        {
            while (h != 0)
            {
                int temp = h;
                h = w % h;
                w = temp;
            }

            return w;
        }

        public bool ValidateImageFormat(Stream imageStream)
        {
            byte[] header = new byte[8];
            imageStream.Read(header, 0, header.Length);
            imageStream.Seek(0, SeekOrigin.Begin); // Volver al inicio del flujo para decodificar

            return IsJpeg(header) || IsPng(header);
        }

        private bool IsJpeg(byte[] imageBytes)
        {
            // JPEG header bytes: FF D8
            return imageBytes.Length >= 2 &&
                   imageBytes[0] == 0xFF &&
                   imageBytes[1] == 0xD8;
        }

        private bool IsPng(byte[] imageBytes)
        {
            // PNG header bytes: 89 50 4E 47 0D 0A 1A 0A
            return imageBytes.Length >= 8 &&
                   imageBytes[0] == 0x89 &&
                   imageBytes[1] == 0x50 &&
                   imageBytes[2] == 0x4E &&
                   imageBytes[3] == 0x47 &&
                   imageBytes[4] == 0x0D &&
                   imageBytes[5] == 0x0A &&
                   imageBytes[6] == 0x1A &&
                   imageBytes[7] == 0x0A;
        }
    }
}
