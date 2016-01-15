using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PodcatcherDotNet.Models {
    [DebuggerDisplay("Title = {Title}, Category = {Category}")]
    public class Feed {
        public string Title { get; set; }
        public string Url { get; set; }
        public string SiteUrl { get; set; }
        public string Description { get; set; }
        public DateTime Updated { get; set; }
        public bool IsPreserved { get; set; }
        public string Category { get; set; }
        public List<string> Tags { get; set; }
        public List<Item> Items { get; set; }

        public void RemoveItems(IEnumerable<Item> items) {
            foreach (var item in items) {
                Items.Remove(item);
            }
        }
    }
}
