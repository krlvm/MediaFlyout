using MediaFlyout.AppInformation;
using MediaFlyout.Utilities;
using MediaFlyout.ViewModels;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Windows.Media;
using Windows.Media.Control;
using static System.WindowsRuntimeSystemExtensions;

namespace MediaFlyout.Views
{
    /// <summary>
    /// Interaction logic for SessionItem.xaml
    /// </summary>
    public partial class SessionItem : UserControl
    {
        private MediaFlyoutWindow window;
        private GlobalSystemMediaTransportControlsSession session;

        private SourceAppInfo sourceInfo;
        private MediaPlaybackAutoRepeatMode? currentRepeatMode;

        private SessionItemViewModel model = new SessionItemViewModel();
        private const bool ENABLE_DETAILS_PANE = false; // WIP

        public SessionItem(MediaFlyoutWindow window, GlobalSystemMediaTransportControlsSession session)
        {
            InitializeComponent();
            DataContext = model;

            this.window = window;

            this.session = session;
            session.MediaPropertiesChanged += Session_MediaPropertiesChanged;
            session.PlaybackInfoChanged += Session_PlaybackInfoChanged;

            SourceAppInfo_Initialize();
            UpdateInfo();

            if(!ENABLE_DETAILS_PANE)
            {
                DetailsArrow.Height = 0;
            }
        }

        public void Invalidate()
        {
            session.MediaPropertiesChanged -= Session_MediaPropertiesChanged;
            session.PlaybackInfoChanged -= Session_PlaybackInfoChanged;
            sourceInfo?.Dispose();

            sourceInfo = null;
            session = null;
            window = null;
        }

        #region UI Management

        private void UpdateInfo()
        {
            if (session == null) return;
            UpdateControls();
            UpdateDetails();
        }

        private void UpdateControls()
        {
            if (session == null) return;
            var playback = session.GetPlaybackInfo();
            model.ToggleButton = IsPlaying(playback) ? "\uE769" : "\uE768";

            if (playback == null) return;
            model.IsPreviousEnabled = playback.Controls.IsPreviousEnabled;
            model.IsPlayPauseEnabled = playback.Controls.IsPauseEnabled || playback.Controls.IsPlayEnabled;
            model.IsNextEnabled = playback.Controls.IsNextEnabled;

            model.IsRewindEnabled = playback.Controls.IsRewindEnabled && false; // WIP

            model.RepeatButton = (currentRepeatMode = playback.AutoRepeatMode) == MediaPlaybackAutoRepeatMode.Track ? "\uE8ED" : "\uE8EE";
            model.IsRepeatEnabled = playback.Controls.IsRepeatEnabled;
            model.IsRepeatActive = playback.AutoRepeatMode != null && playback.AutoRepeatMode != MediaPlaybackAutoRepeatMode.None;

            model.IsShuffleEnabled = playback.Controls.IsShuffleEnabled;
            model.IsShuffleActive = playback.IsShuffleActive ?? false;

            model.IsStopEnabled = playback.Controls.IsStopEnabled;
        }

        private async void UpdateDetails()
        {
            if (session == null) return;
            GlobalSystemMediaTransportControlsSessionMediaProperties properties;
            try
            {
                properties = await session.TryGetMediaPropertiesAsync();
            } catch (FileNotFoundException)
            {
                // Rare exception
                // Occurs when editing video in Windows Photos app
                return;
            }
            if (properties == null) return;
            model.Title = properties.Title;
            model.Artist = properties.Artist;

            UpdateThumbnail(properties);
        }

        private async void UpdateThumbnail(GlobalSystemMediaTransportControlsSessionMediaProperties properties)
        {
            if (properties.Thumbnail != null)
            {
                using (var stream = await properties.Thumbnail.OpenReadAsync())
                {
                    if (stream != null)
                    {
                        using (var nstream = stream.AsStream())
                        {
                            if (ImageUtility.CreateBitmapImageFromStream(nstream, out var bitmap))
                            {
                                model.Thumbnail = bitmap;
                            }
                        }
                    }
                }
            }
            else
            {
                model.Thumbnail = null;
            }
        }

        private bool IsPlaying(GlobalSystemMediaTransportControlsSessionPlaybackInfo playback)
        {
            return playback != null && 
                playback.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
        }

        private MediaPlaybackAutoRepeatMode NextRepeatMode()
        {
            switch (currentRepeatMode)
            {
                case MediaPlaybackAutoRepeatMode.List: return MediaPlaybackAutoRepeatMode.Track;
                case MediaPlaybackAutoRepeatMode.Track: return MediaPlaybackAutoRepeatMode.None;
                default: case MediaPlaybackAutoRepeatMode.None: return MediaPlaybackAutoRepeatMode.List;
            }
        }

        #endregion

        #region Buttons

        private async void Previous_Click(object sender, RoutedEventArgs e)
        {
            await session.TrySkipPreviousAsync();
        }

        private async void PlayPause_Click(object sender, RoutedEventArgs e)
        {
            var playback = session.GetPlaybackInfo();
            if (playback.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
            {
                await session.TryPauseAsync();
            }
            else if (playback.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused)
            {
                await session.TryPlayAsync();
            }
        }

        private async void Next_Click(object sender, RoutedEventArgs e)
        {
            await session.TrySkipNextAsync();
        }

        private async void Shuffle_Click(object sender, RoutedEventArgs e)
        {
            await session.TryChangeShuffleActiveAsync(!(bool)model.IsShuffleActive);
        }

        private async void Repeat_Click(object sender, RoutedEventArgs e)
        {
            await session.TryChangeAutoRepeatModeAsync(NextRepeatMode());
        }

        private async void Stop_Click(object sender, RoutedEventArgs e)
        {
            await session.TryStopAsync();
        }

        private void DoubleClick(object sender, RoutedEventArgs e)
        {
            sourceInfo?.Activate();
        }

        private void AppContainer_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                sourceInfo?.Activate();
            }
        }

        #endregion

        #region Details Panel Management

        private bool? isDetailsVisible = false;
        private const double detailsAnimationTime = 0.25;

        public void Details_Expand()
        {
            isDetailsVisible = null;
            var animation = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(detailsAnimationTime),
                EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseIn },
                From = 0,
                To = 200,
            };
            animation.Completed += (s, e) => isDetailsVisible = true;
            DetailsContainer.BeginAnimation(Grid.HeightProperty, animation);
        }

        public void Details_Collapse()
        {
            isDetailsVisible = null;
            var animation = new DoubleAnimation()
            {
                Duration = TimeSpan.FromSeconds(detailsAnimationTime),
                EasingFunction = new ExponentialEase() { EasingMode = EasingMode.EaseOut },
                From = DetailsContainer.ActualHeight,
                To = 0
            };
            animation.Completed += (s, e) => isDetailsVisible = false;
            DetailsContainer.BeginAnimation(Grid.HeightProperty, animation);
        }

        public void Details_Toggle()
        {
            if (isDetailsVisible == null) return;
            if ((bool)isDetailsVisible)
            {
                Details_Collapse();
            } else
            {
                Details_Expand();
            }
        }

        private void DetailsArrow_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Details_Toggle();
        }

        private void DetailsArrow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                Details_Toggle();
            }
        }

        #endregion

        #region Session Management

        private void Session_PlaybackInfoChanged(GlobalSystemMediaTransportControlsSession session, PlaybackInfoChangedEventArgs args)
        {
            UpdateControls();
            window?.UpdateStatus();
        }

        private void Session_MediaPropertiesChanged(GlobalSystemMediaTransportControlsSession session, MediaPropertiesChangedEventArgs args)
        {
            UpdateInfo();
        }

        #endregion

        #region Source App Info Management

        private void SourceAppInfo_Initialize()
        {
            sourceInfo = SourceAppInfo.FromAppUserModelId(session.SourceAppUserModelId);
            if (sourceInfo != null)
            {
                sourceInfo.InfoFetched += SourceAppInfo_InfoFetched;
                sourceInfo.FetchInfosAsync();

                InfoContainer.MouseDown += DoubleClick;
            }
        }

        private void SourceAppInfo_InfoFetched(object sender, EventArgs e)
        {
            if (sourceInfo == null) return;
            sourceInfo.InfoFetched -= SourceAppInfo_InfoFetched;

            model.AppName = sourceInfo.DisplayName;
            if (ImageUtility.CreateBitmapImageFromStream(sourceInfo.LogoStream, out var bitmap))
            {
                model.AppIcon = bitmap;
            }
        }

        #endregion
    }
}
