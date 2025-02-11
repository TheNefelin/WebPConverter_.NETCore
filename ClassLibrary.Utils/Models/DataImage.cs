namespace ClassLibrary.Utils.Models
{
    public class DataImage
    {
        public int OriginalWidth { get; set; }
        public int OriginalHeight { get; set; }
        public float OriginalRatio { get; set; }
        public string OriginalAspectRatio { get; set; } = string.Empty;
        public int TargetWidth { get; set; }
        public int TargetHeight { get; set; }
        public float TargetRatio { get; set; }
        public string TargetAspectRatio { get; set; } = string.Empty;
    }
}
