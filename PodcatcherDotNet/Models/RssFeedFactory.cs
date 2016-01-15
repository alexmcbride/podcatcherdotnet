using Argotic.Extensions.Core;
using Argotic.Syndication;
using PodcatcherDotNet.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace PodcatcherDotNet.Models {
    public class RssFeedFactory : FeedFactory {
        protected override Feed CreateFeed(XmlReader reader, string url) {
            var rssFeed = new RssFeed();
            rssFeed.Load(reader);

            var items = rssFeed.Channel.Items.OrderBy(i => GetPublishedDate(i)).Select(CreateFeedItem);

            var feed = new Feed {
                Title = rssFeed.Channel.Title,
                Url = url,
                SiteUrl = rssFeed.Channel.Link.ToString(),
                Description = rssFeed.Channel.Description,
                Items = new List<Item>(items)
            };

            var latest = items.LastOrDefault();
            if (latest != null) {
                feed.Updated = latest.Published;
            }

            return feed;
        }

        protected override IList<Item> UpdateFeed(Feed feed, XmlReader reader) {
            var rssFeed = new RssFeed();
            rssFeed.Load(reader);

            var items = (from i in rssFeed.Channel.Items
                         let date = GetPublishedDate(i)
                         orderby date
                         where date > feed.Updated
                         select CreateFeedItem(i)).ToList();

            feed.Items.AddRange(items);

            var latest = items.LastOrDefault();
            if (latest != null) {
                feed.Updated = latest.Published;
            }

            // OPML does not set full description so check for one when updating.
            if (String.IsNullOrEmpty(feed.Description)) {
                feed.Description = rssFeed.Channel.Description;
            }

            return items;
        }

        private static Item CreateFeedItem(RssItem rssItem) {
            return new Item {
                Title = HtmlHelper.DecodeHtml(rssItem.Title),
                Url = rssItem.Link == null ? String.Empty : rssItem.Link.ToString(),
                PodcastUrl = GetPodcastUrl(rssItem),
                Summary = GetHtmlString(rssItem.Description, MaxSummaryLength),
                Published = GetPublishedDate(rssItem),
                IsRead = false
            };
        }

        private static DateTime GetPublishedDate(RssItem item) {
            if (item.PublicationDate == DateTime.MinValue && item.HasExtensions) {
                var ext = item.FindExtension(DublinCoreElementSetSyndicationExtension.MatchByType) as DublinCoreElementSetSyndicationExtension;
                if (ext != null) {
                    return ext.Context.Date;
                }
            }
            return item.PublicationDate;
        }

        private static string GetPodcastUrl(RssItem item) {
            var podcasts = GetPodcastsFromExtensions(item);
            var podcast = podcasts.FirstOrDefault();
            if (podcast != null) {
                return podcast;
            }

            podcasts = GetPodcastsFromEnclousures(item);
            podcast = podcasts.FirstOrDefault();
            if (podcast != null) {
                return podcast;
            }

            return null;
        }

        private static IEnumerable<string> GetPodcastsFromEnclousures(RssItem item) {
            if (item.Enclosures.Any()) {
                return from e in item.Enclosures
                       let url = e.Url.ToString()
                       where ContainsPodcastExtension(url)
                       select url;
            }
            return Enumerable.Empty<string>();
        }

        private static IEnumerable<string> GetPodcastsFromExtensions(RssItem item) {
            if (item.HasExtensions) {
                var extension = item.FindExtension(YahooMediaSyndicationExtension.MatchByType) as YahooMediaSyndicationExtension;
                if (extension != null) {
                    return from c in extension.Context.Contents
                           let url = GetPodcastUrl(c.Url)
                           where ContainsPodcastExtension(url)
                           select url;
                }
            }
            return Enumerable.Empty<string>();
        }

        private static string GetPodcastUrl(Uri uri) {
            return uri == null ? null : uri.ToString();
        }

        private static bool ContainsPodcastExtension(string url) {
            if (!String.IsNullOrEmpty(url)) {
                var filename = new Uri(url).Segments.LastOrDefault();
                if (!String.IsNullOrEmpty(filename)) {
                    var ext = Path.GetExtension(filename);
                    return PodcastExtensions.Contains(ext);
                }
            }
            return false;
        }
    }
}
