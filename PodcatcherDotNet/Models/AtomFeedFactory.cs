using Argotic.Syndication;
using PodcatcherDotNet.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace PodcatcherDotNet.Models {
    public class AtomFeedFactory : FeedFactory {
        protected override Feed CreateFeed(XmlReader reader, string url) {
            var atomFeed = new AtomFeed();
            atomFeed.Load(reader);

            var items = atomFeed.Entries.OrderBy(e => GetPublishedDate(e)).Select(CreateFeedItem);

            var feed = new Feed {
                Title = atomFeed.Title.Content,
                Url = url,
                SiteUrl = atomFeed.Links.Any() ? atomFeed.Links[0].ToString() : String.Empty,
                Description = atomFeed.Subtitle == null ? String.Empty : atomFeed.Subtitle.Content,
                Items = new List<Item>(items)
            };

            var latest = items.LastOrDefault();
            if (latest != null) {
                feed.Updated = latest.Published;
            }

            return feed;
        }

        protected override IList<Item> UpdateFeed(Feed feed, XmlReader reader) {
            var atomFeed = new AtomFeed();
            atomFeed.Load(reader);

            var items = (from e in atomFeed.Entries
                         let date = GetPublishedDate(e)
                         orderby date
                         where date > feed.Updated
                         select CreateFeedItem(e)).ToList();

            feed.Items.AddRange(items);

            var latest = items.LastOrDefault();
            if (latest != null) {
                feed.Updated = latest.Published;
            }

            // OPML does not set full description so check for one when updating.
            if (String.IsNullOrEmpty(feed.Description) && atomFeed.Subtitle != null) {
                feed.Description = atomFeed.Subtitle.Content;
            }

            return items;
        }

        private static Item CreateFeedItem(AtomEntry atomEntry) {
            var item = new Item();
            item.Title = HtmlHelper.DecodeHtml(atomEntry.Title.Content);
            item.Summary = GetItemSummary(atomEntry);
            item.Url = GetAtomLink(atomEntry.Links);
            item.Published = GetPublishedDate(atomEntry);
            item.IsRead = false;
            return item;
        }

        public static DateTime GetPublishedDate(AtomEntry entry) {
            if (entry.PublishedOn == DateTime.MinValue) {
                return entry.UpdatedOn;
            }

            return entry.PublishedOn;
        }

        private static string GetItemSummary(AtomEntry entry) {
            if (entry.Summary != null) {
                return GetHtmlString(entry.Summary.Content, MaxSummaryLength);
            }

            if (entry.Content != null) {
                return GetHtmlString(entry.Content.Content, MaxSummaryLength);
            }

            return String.Empty;
        }

        private static string GetAtomLink(IEnumerable<AtomLink> links) {
            AtomLink link = links.FirstOrDefault();
            if (link != null) {
                return link.Uri.ToString();
            }
            return null;
        }
    }
}
