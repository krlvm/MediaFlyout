using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace MediaFlyout.ViewModels
{
    internal class SessionItemViewModel : ViewModelBase
    {

        #region General

        private string _title;
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private string _artist;
        public string Artist
        {
            get => _artist;
            set => SetProperty(ref _artist, value);
        }

        private ImageSource _thumbnail;
        public ImageSource Thumbnail
        {
            get => _thumbnail;
            set
            {
                if (value != _thumbnail)
                {
                    if (value != null)
                    {
                        ThumbnailSize = 48;
                        _thumbnail = value;
                        OnPropertyChanged();
                    }
                    else
                    {
                        SetAppIconAsThumbnail();
                    }
                }
            }
        }

        private int _thumbnailSize;
        public int ThumbnailSize
        {
            get => _thumbnailSize;
            set => SetProperty(ref _thumbnailSize, value);
        }

        #endregion

        #region Source App

        private string _appName;
        public string AppName
        {
            get => _appName;
            set => SetProperty(ref _appName, value);
        }

        private ImageSource _appIcon;
        public ImageSource AppIcon
        {
            get => _appIcon;
            set
            {
                if (value != _appIcon)
                {
                    _appIcon = value;
                    OnPropertyChanged();
                    if (Thumbnail == null)
                    {
                        SetAppIconAsThumbnail();
                    }
                }
            }
        }

        #endregion

        #region Control Buttons

        private bool _isPreviousEnabled;
        public bool IsPreviousEnabled
        {
            get => _isPreviousEnabled;
            set => SetProperty(ref _isPreviousEnabled, value);
        }

        private bool _isPlayPauseEnabled;
        public bool IsPlayPauseEnabled
        {
            get => _isPlayPauseEnabled;
            set => SetProperty(ref _isPlayPauseEnabled, value);
        }

        private bool _isNextEnabled;
        public bool IsNextEnabled
        {
            get => _isNextEnabled;
            set => SetProperty(ref _isNextEnabled, value);
        }

        private string _toggleButton;
        public string ToggleButton
        {
            get => _toggleButton;
            set => SetProperty(ref _toggleButton, value);
        }

        #endregion

        private void SetAppIconAsThumbnail()
        {
            Thumbnail = AppIcon;
            ThumbnailSize = 32;
        }
    }
}
