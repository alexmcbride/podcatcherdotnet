using System;
using System.Threading.Tasks;

namespace PodcatcherDotNet.Models {
    public class Id3Tag {
        public string FileName { get; private set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Comment { get; set; }

        public void Write(string filename) {
            using (var file = TagLib.File.Create(filename)) {
                FileName = filename;

                file.Tag.Title = Title;
                file.Tag.Album = Album;
                file.Tag.AlbumArtists = new string[] { Artist };
                file.Tag.Comment = Comment;
                file.Save();
            }
        }

        public Task WriteAsync(string filename) {
            return Task.Run(() => Write(filename));
        }

        public static Id3Tag Read(string filename) {
            using (var file = TagLib.File.Create(filename)) {
                return new Id3Tag {
                    FileName = filename,
                    Title = file.Tag.Title,
                    Album = file.Tag.Album,
                    Artist = file.Tag.FirstAlbumArtist,
                    Comment = file.Tag.Comment,
                };
            }
        }
    }
}