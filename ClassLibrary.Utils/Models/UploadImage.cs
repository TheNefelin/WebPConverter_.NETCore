namespace ClassLibrary.Utils.Models
{
    public class UploadImage
    {
        public string ImageName { get; set; }
        public string ImageLocalPath { get; set; } = string.Empty;
        public int OriginalWidth { get; set; }
        public int OriginalHeight { get; set; }
        public string OriginalAspectRatio { get; set; } = string.Empty;
        public int TargetWidth { get; set; }
        public int TargetHeight { get; set; }
        public string TargetAspectRatio { get; set; } = string.Empty;
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
