using Argotic.Syndication;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;

namespace PodcatcherDotNet.Models {
    public static class FeedImporter {
        public static IEnumerable<Feed> ImportOpml(string filename) {
            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                var opml = new OpmlDocument();
                opml.Load(stream);

                return from category in opml.Outlines
                       from feed in category.Outlines
                       select new Feed {
                           Title = feed.Attributes["title"],
                           Url = feed.Attributes["xmlUrl"],
                           SiteUrl = feed.Attributes["htmlUrl"],
                           Items = new List<Item>(),
                           Category = category.Attributes["title"],
                       };
            }
        }

        public static void ExportOpml(string filename, IEnumerable<Feed> feeds, string name) {
            var opml = new OpmlDocument();
            opml.Head.Title = String.Format("{0} Export", name); ;

            var categories = from f in feeds
                             group f by f.Category into c
                             select new {
                                 Name = c.Key,
                                 Feeds = c.ToList()
                             };

            foreach (var category in categories) {
                var outline = new OpmlOutline(category.Name);
                outline.Attributes.Add("title", category.Name);

                foreach (var feed in category.Feeds) {
                    var let = new OpmlOutline(feed.Title);
                    let.Attributes.Add("type", "rss");
                    let.Attributes.Add("title", feed.Title);
                    let.Attributes.Add("xmlUrl", feed.Url);
                    let.Attributes.Add("htmlUrl", feed.SiteUrl);
                    outline.Outlines.Add(let);
                }

                opml.AddOutline(outline);
            }

            using (var stream = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.None)) {
                opml.Save(stream);
            }
        }
    }
}
