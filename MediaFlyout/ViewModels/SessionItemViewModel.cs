using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace MediaFlyout.ViewModels
{
    class SessionItemViewModel : INotifyPropertyChanged
    {

        #region General

        private string _Title;
        public string Title
        {
            get { return _Title; }
            set { if (!value.Equals(_Title)) { _Title = value; OnPropertyChanged(); } }
        }

        private string _Artist;
        public string Artist
        {
            get { return _Artist; }
            set { if (!value.Equals(_Artist)) { _Artist = value; OnPropertyChanged(); } }
        }

        private ImageSource _Thumbnail;
        public ImageSource Thumbnail
        {
            get { return _Thumbnail; }
            set {
                if (value != _Thumbnail)
                {
                    if (value != null)
                    {
                        ThumbnailSize = DEFAULT_THUMBNAIL_SIZE;
                        _Thumbnail = value;
                        OnPropertyChanged();
                    } else
                    {
                        SetAppIconAsThumbnail();
                    }
                }
            }
        }

        private void SetAppIconAsThumbnail()
        {
            ThumbnailSize = 32;
            _Thumbnail = AppIcon;
            OnPropertyChanged("Thumbnail");
        }

        private const int DEFAULT_THUMBNAIL_SIZE = 48;
        private int _ThumbnailSize;
        public int ThumbnailSize
        {
            get { return _ThumbnailSize; }
            set { if (value != _ThumbnailSize) { _ThumbnailSize = value; OnPropertyChanged(); } }
        }

        #endregion

        #region Source App

        private string _AppName;
        public string AppName
        {
            get { return _AppName; }
            set { if (!value.Equals(_AppName)) { _AppName = value; OnPropertyChanged(); } }
        }

        private ImageSource _AppIcon;
        public ImageSource AppIcon
        {
            get { return _AppIcon; }
            set { 
                if (value != _AppIcon) {
                    _AppIcon = value;
                    OnPropertyChanged(); 
                    if (_Thumbnail == null)
                    {
                        SetAppIconAsThumbnail();
                    }
                } 
            }
        }

        #endregion

        #region Control Buttons

        private bool _IsPreviousEnabled;
        public bool IsPreviousEnabled
        {
            get { return _IsPreviousEnabled; }
            set { if (!value.Equals(_IsPreviousEnabled)) { _IsPreviousEnabled = value; OnPropertyChanged(); } }
        }

        private bool _IsPlayPauseEnabled;
        public bool IsPlayPauseEnabled
        {
            get { return _IsPlayPauseEnabled; }
            set { if (!value.Equals(_IsPlayPauseEnabled)) { _IsPlayPauseEnabled = value; OnPropertyChanged(); } }
        }

        private bool _IsNextEnabled;
        public bool IsNextEnabled
        {
            get { return _IsNextEnabled; }
            set { if (!value.Equals(_IsNextEnabled)) { _IsNextEnabled = value; OnPropertyChanged(); } }
        }

        private string _ToggleButton;
        public string ToggleButton
        {
            get { return _ToggleButton; }
            set { if (!value.Equals(_ToggleButton)) { _ToggleButton = value; OnPropertyChanged(); } }
        }

        #endregion

        #region Additional Control Buttons

        private bool _IsRewindEnabled;
        public bool IsRewindEnabled
        {
            get { return _IsRewindEnabled; }
            set { if (!value.Equals(_IsRewindEnabled)) { _IsRewindEnabled = value; OnPropertyChanged(); } }
        }

        private bool _IsShuffleEnabled;
        public bool IsShuffleEnabled
        {
            get { return _IsShuffleEnabled; }
            set { if (!value.Equals(_IsShuffleEnabled)) { _IsShuffleEnabled = value; if (!value) { IsShuffleActive = null; } OnPropertyChanged(); } }
        }
        private bool? _IsShuffleActive;
        public bool? IsShuffleActive
        {
            get { return _IsShuffleActive; }
            set { if (!IsShuffleEnabled) { value = null; } if (!value.Equals(_IsShuffleActive)) { _IsShuffleActive = value; OnPropertyChanged(); } }
        }

        private bool _IsRepeatEnabled;
        public bool IsRepeatEnabled
        {
            get { return _IsRepeatEnabled; }
            set { if (!value.Equals(_IsRepeatEnabled)) { _IsRepeatEnabled = value; if (!value) { IsRepeatActive = null; } OnPropertyChanged(); } }
        }
        private bool? _IsRepeatActive;
        public bool? IsRepeatActive
        {
            get { return _IsRepeatActive; }
            set { if (!IsRepeatEnabled) { value = null; } if (!value.Equals(_IsRepeatActive)) { _IsRepeatActive = value; OnPropertyChanged(); } }
        }
        private string _RepeatButton;
        public string RepeatButton
        {
            get { return _RepeatButton; }
            set { if (!value.Equals(_RepeatButton)) { _RepeatButton = value; OnPropertyChanged(); } }
        }

        private bool _IsStopEnabled;
        public bool IsStopEnabled
        {
            get { return _IsStopEnabled; }
            set { if (!value.Equals(_IsStopEnabled)) { _IsStopEnabled = value; OnPropertyChanged(); } }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
