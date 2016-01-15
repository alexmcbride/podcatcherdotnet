using Caliburn.Micro;
using PodcatcherDotNet.Helpers;
using PodcatcherDotNet.Models;
using System;
using System.ComponentModel;
using System.IO;

namespace PodcatcherDotNet.ViewModels {
    public class FileViewModel : PropertyChangedBase, IEditableObject {
        private IEventAggregator _eventAggregator;
        private string _title;
        private string _album;
        private string _artist;
        private double _fileSize;

        public string FileName { get; private set; }

        public string Title {
            get { return _title; }
            set {
                if (_title != value) {
                    _title = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public string Album {
            get { return _album; }
            set {
                if (_album != value) {
                    _album = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public string Artist {
            get { return _artist; }
            set {
                if (_artist != value) {
                    _artist = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public double FileSize {
            get { return _fileSize; }
            set {
                if (_fileSize != value) {
                    _fileSize = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public PodcastType PodcastType {
            get { return DownloadHelper.GetPodcastType(FileName); }
        }

        public FileViewModel(string filename) {
            FileName = filename;
            FileSize = new FileInfo(filename).Length / 1048576.0;

            _eventAggregator = IoC.Get<IEventAggregator>();
            _eventAggregator.Subscribe(this);

            LoadId3TagData();
        }

        public void LoadId3TagData() {
            var tag = Id3Tag.Read(FileName);
            Title = tag.Title ?? String.Empty;
            Album = tag.Album ?? String.Empty;
            Artist = tag.Artist ?? String.Empty;

            if (String.IsNullOrEmpty(Title)) {
                Title = Path.GetFileNameWithoutExtension(tag.FileName);
            }
        }

        #region IEditableObject
        public void BeginEdit() {

        }

        public void CancelEdit() {

        }

        public void EndEdit() {

        }
        #endregion
    }
}
