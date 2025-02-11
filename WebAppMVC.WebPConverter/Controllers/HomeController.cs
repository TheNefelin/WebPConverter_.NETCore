using System.Diagnostics;
using ClassLibrary.Utils.Models;
using ClassLibrary.Utils.Tools;
using Microsoft.AspNetCore.Mvc;
using WebAppMVC.WebPConverter.Models;

namespace WebAppMVC.WebPConverter.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly ImageProcessor _imageProcessor;

    public HomeController(ILogger<HomeController> logger, IWebHostEnvironment environment, ImageProcessor imageProcessor)
    {
        _logger = logger;
        _environment = environment;
        _imageProcessor = imageProcessor;
    }

    public IActionResult Index()
    {
        ViewBag.Quality = 65;
        ViewBag.Width = 800;
        ViewBag.Height = 600;
        ViewBag.Ratio = "4/3";

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    [HttpPost]
    [DisableRequestSizeLimit]
    public IActionResult Index(List<IFormFile> newImages, string conversionOption, int quality, int width, int height)
    {
        Persistence(quality, width, height);

        List<UploadImage> imageList = new List<UploadImage>();

        if (quality <= 0 || width <= 0 || height <= 0)
        {
            var error = new UploadImage()
            {
                IsError = true,
                ErrorMessage = "Define Quality, Width and Height",
                ImageLocalPath = "/webP/00-not-found-800x600.webp"
            };

            imageList.Add(error);
        }

        var uploadPath = Path.Combine(_environment.WebRootPath, "WebP");
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        foreach (var newImage in newImages)
        {
            string imageWebP = Path.ChangeExtension(newImage.FileName, ".webp");
            string outputPath = Path.Combine(uploadPath, imageWebP);
            DataImage dataImage = new();
            UploadImage uploadImages = new();

            try
            {
                using (var imageStream = newImage.OpenReadStream())
                {
                    if (conversionOption == "convertOnly")
                    {
                        dataImage = _imageProcessor.ConvertToWebP_FromStream_AndSave(imageStream, outputPath, quality);
                    }
                    else if (conversionOption == "convertAndResize")
                    {
                        dataImage = _imageProcessor.ConvertToWebP_FromStream_ResizeAndSave(imageStream, outputPath, quality, width, height);
                    }

                    uploadImages.IsError = false;
                    uploadImages.ImageName = imageWebP;
                    uploadImages.ImageLocalPath = "/WebP/" + imageWebP;
                    uploadImages.OriginalWidth = dataImage.OriginalWidth;
                    uploadImages.OriginalHeight = dataImage.OriginalHeight;
                    uploadImages.OriginalAspectRatio = dataImage.OriginalAspectRatio;
                    uploadImages.TargetWidth = dataImage.TargetWidth;
                    uploadImages.TargetHeight = dataImage.TargetHeight;
                    uploadImages.TargetAspectRatio = dataImage.TargetAspectRatio;
                }
            }
            catch (Exception ex)
            {
                uploadImages.IsError = true;
                uploadImages.ErrorMessage = ex.Message;
                uploadImages.ImageLocalPath = "/webP/00-not-found-800x600.webp";
            }

            imageList.Add(uploadImages);
        }

        return View(imageList);
    }

    private void Persistence(int quality, int width, int height)
    {
        int gcd = GCD(width, height);
        string ratio = $"{width / gcd}/{height / gcd}";

        // Persistir valores
        ViewBag.Quality = quality;
        ViewBag.Width = width;
        ViewBag.Height = height;
        ViewBag.Ratio = ratio;
    }

    private int GCD(int a, int b)
    {
        return b == 0 ? a : GCD(b, a % b);
    }
}
