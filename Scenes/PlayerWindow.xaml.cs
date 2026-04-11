using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Meta = Windows.Media.Control.GlobalSystemMediaTransportControlsSessionMediaProperties;
using PlaybackInfo = Windows.Media.Control.GlobalSystemMediaTransportControlsSessionPlaybackInfo;
using Timeline = Windows.Media.Control.GlobalSystemMediaTransportControlsSessionTimelineProperties;

namespace Media_Controller {
	public sealed partial class PlayerWindow: Window {
		private PlaybackAPI api;

		public PlayerWindow() {
			InitializeComponent();
			AppWindow.Resize(new Windows.Graphics.SizeInt32(440, 150));
			ExtendsContentIntoTitleBar = true;
			SetTitleBar(TitleBar);
			if (AppWindow.Presenter is OverlappedPresenter presenter) {
				presenter.IsResizable = false;
				presenter.IsMaximizable = false;
				presenter.IsMinimizable = false;
				presenter.IsAlwaysOnTop = true;
				presenter.SetBorderAndTitleBar(false, false);
			}

			api = new PlaybackAPI();

			api.PlaybackUpdated += PlaybackUpdated;
			api.TimelineUpdated += TimelineUpdated;
			api.MetaUpdated += MetaUpdated;

			//api.manager.GetCurrentSession().TimelinePropertiesChanged += OnTimelineChanged;
			Play.Click += OnMetaChanged2;

			//LabelSong.Text = Task.Run(
			//	() => api.session.TryGetMediaPropertiesAsync().GetResults()
			//).GetAwaiter().GetResult().Title;
			//LabelSong.Text = api.session.GetTimelineProperties().Position.ToString();
		}




		// API Events

		async private Task MetaUpdated(Meta meta) {
			DispatcherQueue.TryEnqueue(async () => {
				using (IRandomAccessStream fileStream = await meta.Thumbnail.OpenReadAsync()) {
					BitmapImage bitmapImage = new BitmapImage();
					bitmapImage.DecodePixelHeight = (int)SongCover.Height;
					bitmapImage.DecodePixelWidth = (int)SongCover.Width;
					await bitmapImage.SetSourceAsync(fileStream);
					SongCover.Source = bitmapImage;
				}

				LabelArtist.Text = meta.Artist;
				LabelSong.Text = meta.Title;
			});
		}




		private void TimelineUpdated(Timeline timeline) {
			DispatcherQueue.TryEnqueue(() => {
				LabelTime.Text = timeline.Position.ToString();
			});
			//LabelTime.Text = timeline.Position.ToString();
		}

		private void PlaybackUpdated(PlaybackInfo playback) {
			//LabelTime.Text = api.manager.GetCurrentSession().GetTimelineProperties().Position.ToString();
		}

		private void OnMetaChanged2(object sender, RoutedEventArgs e) {
			LabelTime.Text = ":(";
		}



		// Buttons

		private void SettingsBtn_Click(object sender, RoutedEventArgs e) {
			
		}

		private void MinimizeBtn_Click(object sender, RoutedEventArgs e) {
			if (AppWindow.Presenter is OverlappedPresenter presenter) {
				presenter.Minimize();
			}
		}

		private void CloseBtn_Click(object sender, RoutedEventArgs e) {
			Close();
		}



		private void PrevBtn_Click(object sender, RoutedEventArgs e) {
			
		}

		private void PlayBtn_Click(object sender, RoutedEventArgs e) {
			
		}

		private void NextBtn_Click(object sender, RoutedEventArgs e) {
			
		}
	}
}
