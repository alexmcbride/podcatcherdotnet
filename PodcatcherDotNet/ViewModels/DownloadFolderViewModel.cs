using Caliburn.Micro;
using PodcatcherDotNet.Helpers;
using PodcatcherDotNet.Messages;
using PodcatcherDotNet.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PodcatcherDotNet.ViewModels {
    public class DownloadFolderViewModel : Conductor<FileViewModel>.Collection.OneActive, IHandle<DownloadFinishedMessage>, IHandle<DownloadEditedMessage> {
        private const double DefaultColumnWidth = 100;
        private const double ColumnWidthPadding = 40;
        private static readonly string[] PodcastExtensions = new string[] { ".mp3", ".m4a" };

        private IEventAggregator _eventAggregator;
        private IEditableCollectionView _editableFilesView;
        private DownloadSectionViewModel _downloadSection;
        private double _titleColumnWidth;
        private double _albumColumnWidth;
        private double _artistColumnWidth;
        private bool _downloadFolderLoaded;
        private bool _canDelete;
        private bool _canEdit;
        private bool _canOpenFolder;
        private bool _canPlay;
        private bool _canRefreshFiles;
        private bool _canConvert;
        private double _totalFileSize;

        public ICollectionView FilesView { get; private set; }

        public double TitleColumnWidth {
            get { return _titleColumnWidth; }
            set {
                if (_titleColumnWidth != value) {
                    _titleColumnWidth = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public double AlbumColumnWidth {
            get { return _albumColumnWidth; }
            set {
                if (_albumColumnWidth != value) {
                    _albumColumnWidth = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public double ArtistColumnWidth {
            get { return _artistColumnWidth; }
            set {
                if (_artistColumnWidth != value) {
                    _artistColumnWidth = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        // Mb
        public double TotalFileSize {
            get { return _totalFileSize; }
            set {
                if (_totalFileSize != value) {
                    _totalFileSize = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool CanPlay {
            get { return _canPlay; }
            set {
                if (_canPlay != value) {
                    _canPlay = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool CanDelete {
            get { return _canDelete; }
            set {
                if (_canDelete != value) {
                    _canDelete = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool CanEdit {
            get { return _canEdit; }
            set {
                if (_canEdit != value) {
                    _canEdit = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool CanOpenFolder {
            get { return _canOpenFolder; }
            set {
                if (_canOpenFolder != value) {
                    _canOpenFolder = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool CanRefreshFiles {
            get { return _canRefreshFiles; }
            set {
                if (_canRefreshFiles != value) {
                    _canRefreshFiles = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public bool CanConvert {
            get { return _canConvert; }
            set {
                if (_canConvert != value) {
                    _canConvert = value;
                    NotifyOfPropertyChange();
                }
            }
        }

        public DownloadFolderViewModel(DownloadSectionViewModel downloadSection) {
            _downloadSection = downloadSection;

            _eventAggregator = IoC.Get<IEventAggregator>();
            _eventAggregator.Subscribe(this);

            FilesView = CollectionViewSource.GetDefaultView(Items);
            FilesView.SortDescriptions.Add(new SortDescription("Album", ListSortDirection.Ascending));
            FilesView.SortDescriptions.Add(new SortDescription("Title", ListSortDirection.Ascending));

            _editableFilesView = (IEditableCollectionView)FilesView; // Used for refreshing the sort order once a FileViewModel has been updated.

            TitleColumnWidth = AlbumColumnWidth = ArtistColumnWidth = DefaultColumnWidth;

            CanRefreshFiles = true;
        }

        public bool FileExists(string filename) {
            return Items.Any(i => i.FileName == filename);
        }

        protected override void ChangeActiveItem(FileViewModel newItem, bool closePrevious) {
            base.ChangeActiveItem(newItem, closePrevious);

            CanPlay = CanOpenFolder = CanEdit = CanDelete = (newItem != null);
            CanConvert = (newItem != null) && (newItem.PodcastType == PodcastType.M4a);
        }

        public void Handle(DownloadFinishedMessage message) {
            // Adds finished download to list, if it's loaded and file belongs in the download folder.
            var folder = Path.GetDirectoryName(message.FileName);
            if (_downloadFolderLoaded && folder == Settings.Default.DownloadFolder && !FileExists(message.FileName)) {
                AddFile(message.FileName);
                NotifyOfPropertyChange("TotalFileSize");
            }
        }

        // A download has been edited, so update its FileViewModel to match.
        public void Handle(DownloadEditedMessage message) {
            var file = Items.SingleOrDefault(f => f.FileName == message.FileName);

            if (file != null) {
                _editableFilesView.EditItem(file);

                file.Title = message.Title;
                file.Album = message.Album;
                file.Artist = message.Artist;

                _editableFilesView.CommitEdit();
            }
        }

        public void LoadDownloadFolder() {
            if (!_downloadFolderLoaded) {
                _downloadFolderLoaded = true;

                LoadInternalAsync();
            }
        }

        private async void LoadInternalAsync() {
            try {
                CanRefreshFiles = false;

                string folder = Settings.Default.DownloadFolder;
                if (Directory.Exists(folder)) {
                    var filenames = EnumeratePodcastFileNames(folder);

                    Items.Clear();
                    TotalFileSize = 0;
                    foreach (var filename in filenames) {
                        await AddFileAsync(filename);
                    }

                    NotifyOfPropertyChange("TotalFileSize");
                }
            }
#if !DEBUG
            catch (Exception ex) {
                DialogHelper.ShowError("Open Folder Error", ex.Message);
            }
#endif
            finally {
                CanRefreshFiles = true;
            }
        }

        private IEnumerable<string> EnumeratePodcastFileNames(string folder) {
            return Directory.EnumerateFiles(folder, "*", SearchOption.TopDirectoryOnly).Where(f => IsPodcastFile(f) && !_downloadSection.IsActiveDownload(f));
        }

        private static bool IsPodcastFile(string filename) {
            return PodcastExtensions.Contains(Path.GetExtension(filename));
        }

        public Task<FileViewModel> AddFileAsync(string filename) {
            return Task.Run<FileViewModel>(() => AddFile(filename));
        }

        public FileViewModel AddFile(string filename) {
#if !DEBUG
            try {
#endif
                var file = new FileViewModel(filename);

                UpdateColumnWidths(file);
                TotalFileSize += file.FileSize;

                Items.Add(file);

                return file;
#if !DEBUG
            }
            catch (Exception ex) {
                DialogHelper.ShowError(
                    "Load File Error",
                    "Error reading '{0}': {1}",
                    Path.GetFileName(filename),
                    ex.Message);
            }

            return null;
#endif
        }

        // When adding items from a background thread the ListView column widths are not 
        // updated automatically, so instead we figure them out ourselves.
        private void UpdateColumnWidths(FileViewModel file) {
            var titleWidth = TextHelper.GetTextWidth(file.Title) + ColumnWidthPadding;
            if (titleWidth > TitleColumnWidth) {
                TitleColumnWidth = titleWidth;
            }

            var albumWidth = TextHelper.GetTextWidth(file.Album) + ColumnWidthPadding;
            if (albumWidth > AlbumColumnWidth) {
                AlbumColumnWidth = albumWidth;
            }

            var artistWidth = TextHelper.GetTextWidth(file.Artist) + ColumnWidthPadding;
            if (artistWidth > ArtistColumnWidth) {
                ArtistColumnWidth = artistWidth;
            }
        }

        public void RefreshFiles() {
            Task.Run(() => LoadInternalAsync());
        }

        public void Play() {
            try {
                if (File.Exists(ActiveItem.FileName)) {
                    WindowsHelper.OpenMusicPlayer(ActiveItem.FileName);
                }
            }
            catch (Exception ex) {
                DialogHelper.ShowError("Play Download Error", ex.Message);
            }
        }

        public void Delete() {
            var result = DialogHelper.ShowYesNo(
                "Delete File",
                "Delete this file?\n\n{0}",
                ActiveItem.FileName);

            if (result) {
                try {
                    File.Delete(ActiveItem.FileName);

                    TotalFileSize -= ActiveItem.FileSize;
                    Items.Remove(ActiveItem);

                    NotifyOfPropertyChange("TotalFileSize");
                }
                catch (Exception ex) {
                    DialogHelper.ShowError("File Error", ex.Message);
                }
            }
        }

        public void Edit() {
            new EditDownloadViewModel(ActiveItem.FileName).Activate();
        }

        public void OpenFolder() {
            try {
                WindowsHelper.OpenFolder(ActiveItem.FileName, select: true);
            }
            catch (Exception ex) {
                DialogHelper.ShowError("Open Folder Error", ex.Message);
            }
        }
    }
}
