namespace Dragonfly.SiteAuditor.Models;

    using System;

   public class StandardViewInfo
    {
        /// <summary>
        /// Current version of Package
        /// </summary>
        public Version? CurrentToolVersion { get; set; }

        public int ThumbnailWidth { get; set; }
        public int ThumbnailHeight { get; set; }
        public string SerilogDirectory { get; set; }
    }

