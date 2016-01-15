using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PodcatcherDotNet.Models {
    public static class FeedSerializer {
        private const string SaveHeader = "PODCATCHERDOTNET";
        private const string DownloadHeader = "PCDNDOWNLOADS";
        private const int SaveVersion = 1;
        private const int DownloadVersion = 1;
        private const string FeedsFileName = "Feeds.dat";
        private const string DownloadsFileName = "Downloads.dat";

        private static string GetAppDataFolder() {
            var appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PodcatcherDotNet");

            if (!Directory.Exists(appData)) {
                Directory.CreateDirectory(appData);
            }

            return appData;
        }

        private static string GetFeedsPath() {
            var appData = GetAppDataFolder();

            return Path.Combine(appData, FeedsFileName);
        }

        public static void SaveFeeds(IEnumerable<Feed> feeds) {
            // Save to temp file first in case of error.
            var tempFileName = Path.GetTempFileName();

            FileStream stream = null;
            BinaryWriter writer = null;

            try {
                stream = File.Open(tempFileName, FileMode.Create, FileAccess.Write, FileShare.None);
                writer = new BinaryWriter(stream);

                writer.Write(SaveHeader);
                writer.Write(SaveVersion);

                writer.Write(feeds.Count());
                foreach (var feed in feeds) {
                    writer.Write(feed.Title);
                    writer.Write(feed.Url);
                    writer.Write(feed.SiteUrl);
                    writer.Write(feed.Description ?? String.Empty);
                    writer.Write(feed.Updated.ToBinary());
                    writer.Write(feed.IsPreserved);
                    writer.Write(feed.Category ?? String.Empty);

                    writer.Write(feed.Items.Count);
                    foreach (var item in feed.Items) {
                        writer.Write(item.Title);
                        writer.Write(item.Url);
                        writer.Write(item.PodcastUrl ?? String.Empty);
                        writer.Write(item.Published.ToBinary());
                        writer.Write(item.Summary);
                        writer.Write(item.IsRead);
                    }
                }
            }
            finally {
                if (writer != null) {
                    writer.Dispose();
                }

                if (stream != null) {
                    stream.Dispose();
                }

                // Copy temp file to actual file.
                File.Copy(tempFileName, GetFeedsPath(), overwrite: true);
                File.Delete(tempFileName);
            }
        }

        public static IEnumerable<Feed> LoadFeeds() {
            var filename = GetFeedsPath();

            if (!File.Exists(filename)) {
                yield break;
            }

            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new BinaryReader(stream)) {
                if (reader.ReadString() != SaveHeader) {
                    throw new InvalidOperationException("header");
                }

                if (reader.ReadInt32() != SaveVersion) {
                    throw new InvalidOperationException("version");
                }

                int feedCount = reader.ReadInt32();
                for (int i = 0; i < feedCount; i++) {
                    var feed = new Feed {
                        Title = reader.ReadString(),
                        Url = reader.ReadString(),
                        SiteUrl = reader.ReadString(),
                        Description = reader.ReadString(),
                        Updated = DateTime.FromBinary(reader.ReadInt64()),
                        IsPreserved = reader.ReadBoolean(),
                        Category = reader.ReadString()
                    };

                    int itemCount = reader.ReadInt32();
                    feed.Items = new List<Item>(itemCount);
                    for (int j = 0; j < itemCount; j++) {
                        feed.Items.Add(new Item {
                            Title = reader.ReadString(),
                            Url = reader.ReadString(),
                            PodcastUrl = reader.ReadString(),
                            Published = DateTime.FromBinary(reader.ReadInt64()),
                            Summary = reader.ReadString(),
                            IsRead = reader.ReadBoolean()
                        });
                    }

                    yield return feed;
                }
            }
        }

        private static string GetDownloadsPath() {
            var appData = GetAppDataFolder();

            return Path.Combine(appData, DownloadsFileName);
        }

        public static void SaveDownloads(IEnumerable<Download> downloads) {
            var tempFileName = Path.GetTempFileName();

            FileStream stream = null;
            BinaryWriter writer = null;

            try {
                stream = File.Open(tempFileName, FileMode.Create, FileAccess.Write, FileShare.None);
                writer = new BinaryWriter(stream);

                writer.Write(DownloadHeader);
                writer.Write(DownloadVersion);

                writer.Write(downloads.Count());
                foreach (var download in downloads) {
                    writer.Write(download.Title);
                    writer.Write(download.FeedTitle);
                    writer.Write(download.Url);
                    writer.Write(download.FileName ?? String.Empty);
                    writer.Write(download.DownloadedDate.ToBinary());
                }
            }
            finally {
                if (writer != null) {
                    writer.Dispose();
                }

                if (stream != null) {
                    stream.Dispose();
                }

                File.Copy(tempFileName, GetDownloadsPath(), overwrite: true);
                File.Delete(tempFileName);
            }
        }

        public static IEnumerable<Download> LoadDownloads() {
            var filename = GetDownloadsPath();

            if (!File.Exists(filename)) {
                yield break;
            }

            using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new BinaryReader(stream)) {
                if (reader.ReadString() != DownloadHeader) {
                    throw new InvalidOperationException("header");
                }

                if (reader.ReadInt32() != DownloadVersion) {
                    throw new InvalidOperationException("version");
                }

                int count = reader.ReadInt32();
                var downloads = new List<Download>(count);
                for (int i = 0; i < count; i++) {
                    yield return new Download {
                        Title = reader.ReadString(),
                        FeedTitle = reader.ReadString(),
                        Url = reader.ReadString(),
                        FileName = reader.ReadString(),
                        DownloadedDate = DateTime.FromBinary(reader.ReadInt64())
                    };
                }
            }
        }

        public static void SaveBackup(string backupFileName) {
            using (var zip = new ZipFile()) {

                var feedsPath = GetFeedsPath();
                if (File.Exists(feedsPath)) {
                    zip.AddFile(feedsPath, String.Empty);
                }

                var downloadsPath = GetDownloadsPath();
                if (File.Exists(downloadsPath)) {
                    zip.AddFile(downloadsPath, String.Empty);
                }

                zip.Save(backupFileName);
            }
        }

        public static void LoadBackup(string backupFileName) {
            var targetPath = GetAppDataFolder();
            using (var zip = new ZipFile(backupFileName)) {
                zip.ExtractAll(targetPath, ExtractExistingFileAction.OverwriteSilently);
            }
        }
    }
}
