using System;

namespace PodcatcherDotNet.Models {
   public class Download {
       public string Title { get; set; }
       public string FeedTitle { get; set; }
       public DateTime DownloadedDate { get; set; }
       public string Url { get; set; }
       public string FileName { get; set; }
    }
}
