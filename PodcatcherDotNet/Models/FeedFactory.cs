using PodcatcherDotNet.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Linq;

namespace PodcatcherDotNet.Models {
    // Factory class for creating and updating RSS and Atom feeds.
    public abstract class FeedFactory {
        protected const int MaxSummaryLength = 400;
        protected static readonly string[] PodcastExtensions = new string[] { ".mp3", ".m4a" };

        public static Feed DownloadFeed(string url, TimeSpan timeout) {
            using (var download = new DownloadHelper(timeout))
            using (var reader = download.DownloadXml(url)) {
                var factory = GetFeedFactory(reader);

                if (factory != null) {
                    var feed = factory.CreateFeed(reader, url);

                    return feed;
                }

                return null;
            }
        }

        public static async Task<IList<Item>> UpdateFeedAsync(Feed feed, TimeSpan timeout) {
            using (var downloader = new DownloadHelper(timeout))
            using (var reader = await downloader.DownloadXmlAsync(feed.Url)) {
                var factory = GetFeedFactory(reader);

                if (factory != null) {
                    return factory.UpdateFeed(feed, reader);
                }

                return new Item[0];
            }
        }

        private static FeedFactory GetFeedFactory(XmlReader reader) {
            if (reader.IsStartElement("rss", String.Empty) ||
                reader.IsStartElement("rdf:RDF")) {
                return new RssFeedFactory();
            }

            if (reader.IsStartElement("atom", "http://www.w3.org/2005/Atom") ||
                reader.IsStartElement("feed", "http://www.w3.org/2005/Atom")) {
                return new AtomFeedFactory();
            }

            return null;
        }

        // Removes HTML tags, decodes HTML special characters, and limits the length of strings.
        protected static string GetHtmlString(string source, int maxLength) {
            source = HtmlHelper.DecodeHtml(source);//.Trim();

            return HtmlHelper.RemoveHtmlTags(source, maxLength);
        }

        protected abstract Feed CreateFeed(XmlReader reader, string url);
        protected abstract IList<Item> UpdateFeed(Feed feed, XmlReader reader);
    }
}
