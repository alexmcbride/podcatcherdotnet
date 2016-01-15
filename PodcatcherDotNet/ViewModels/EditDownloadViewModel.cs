using Caliburn.Micro;
using PodcatcherDotNet.Helpers;
using PodcatcherDotNet.Messages;
using PodcatcherDotNet.Models;
using System;
using System.IO;

namespace PodcatcherDotNet.ViewModels {
    public class EditDownloadViewModel : Screen {
        private IEventAggregator _eventAggregator;
        private bool _canSave;
        private string _originalTitle;
        private string _originalComment;
        private string _originalArtist;
        private string _originalAlbum;
        private string _title;
        private string _comment;
        private string _artist;
        private string _album;
        private string _filename;

        public string FileName {
            get { return _filename; }
            set {
                if (_filename != value) {
                    _filename = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public string Title {
            get { return _title; }
            set {
                if (_title != value) {
                    _title = value;
                    NotifyOfPropertyChange();
                    CanSave = _title != _originalTitle;
                }
            }
        }

        public string Comment {
            get { return _comment; }
            set {
                if (_comment != value) {
                    _comment = value;
                    NotifyOfPropertyChange();
                    CanSave = _comment != _originalComment;
                }
            }
        }

        public string Artist {
            get { return _artist; }
            set {
                if (_artist != value) {
                    _artist = value;
                    NotifyOfPropertyChange();
                    CanSave = _artist != _originalArtist;
                }
            }
        }

        public string Album {
            get { return _album; }
            set {
                if (_album != value) {
                    _album = value;
                    NotifyOfPropertyChange();
                    CanSave = _album != _originalAlbum;
                }
            }
        }

        public bool CanSave {
            get { return _canSave; }
            set {
                if (_canSave != value) {
                    _canSave = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public EditDownloadViewModel(string filename) {
            _eventAggregator = IoC.Get<IEventAggregator>();

            FileName = filename;

            if (File.Exists(filename)) {
                LoadId3Tags();
            }

            CanSave = false;
        }

        private void LoadId3Tags() {
#if !DEBUG
            try {
#endif
                var tag = Id3Tag.Read(FileName);
                _originalTitle = Title = tag.Title;
                _originalComment = Comment = tag.Comment;
                _originalArtist = Artist = tag.Artist;
                _originalAlbum = Album = tag.Album;
#if !DEBUG
            }
            catch (Exception ex) {
                DialogHelper.ShowError("Load ID3 Tags Error", ex.Message);
            }
#endif
        }

        public void Save() {
            if (File.Exists(FileName)) {
#if !DEBUG
                try {
#endif
                    var tag = new Id3Tag {
                        Title = Title,
                        Album = Album,
                        Artist = Artist,
                        Comment = Comment,
                    };

                    tag.Write(FileName);
                    TryClose(true);
                    _eventAggregator.PublishOnUIThread(new DownloadEditedMessage(FileName, Title, Artist, Album));
#if !DEBUG
                }
                catch (Exception ex) {
                    DialogHelper.ShowError("Save ID3 Tags Error", ex.Message);
                }
#endif
            }
            else {
                DialogHelper.ShowError("Download File Missing", "The file '{0}' does not exist", FileName);
            }
        }

        public void Activate() {
            _eventAggregator.PublishOnUIThread(new ActivateScreenMessage(this));
        }
    }
}
