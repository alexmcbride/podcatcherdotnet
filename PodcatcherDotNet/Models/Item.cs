using System;
using System.Diagnostics;
using System.IO;

namespace PodcatcherDotNet.Models {
    [DebuggerDisplay("Title = {Title}")]
    public class Item {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Summary { get; set; }
        public DateTime Published { get; set; }
        public bool IsRead { get; set; }
        public string PodcastUrl { get; set; }
    }
}
