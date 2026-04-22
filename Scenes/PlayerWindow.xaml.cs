using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Meta = Windows.Media.Control.GlobalSystemMediaTransportControlsSessionMediaProperties;
using PlaybackInfo = Windows.Media.Control.GlobalSystemMediaTransportControlsSessionPlaybackInfo;
using Timeline = Windows.Media.Control.GlobalSystemMediaTransportControlsSessionTimelineProperties;
using PlaybackStatus = Windows.Media.Control.GlobalSystemMediaTransportControlsSessionPlaybackStatus;

namespace Media_Controller {
	public sealed partial class PlayerWindow: Window {
		private PlaybackAPI api;
		private DispatcherTimer timer;
		private int position_sec;

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
			timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(1);
			timer.Tick += OnTimedEvent;
			SetupApi(api);
		}


		async private Task SetupApi(PlaybackAPI _api) {
			_api.PlaybackUpdated += PlaybackUpdated;
			_api.TimelineUpdated += TimelineUpdated;
			_api.MetaUpdated += MetaUpdated;

			await Task.Delay(500);
			_api.ForceUpdate();
		}



		private String timespan_to_str(int h, int m, int s) {
			String output = "";
			if (h > 0) output = $"{h:00}:";
			output += $"{m:00}:{s:00}";
			return output;
		}

		

		private void OnTimedEvent(object? sender, object? e) {
			if (sender != null) position_sec += 1;
			int m = position_sec / 60;
			int h = m / 60;
			LabelTimeNow.Text = timespan_to_str(h, m, position_sec % 60);
			SliderTime.Value = position_sec;
		}

		async private Task MetaUpdated(Meta meta) {
			DispatcherQueue.TryEnqueue(async () => {
				if (meta.Thumbnail != null) {
					using (IRandomAccessStream fileStream = await meta.Thumbnail.OpenReadAsync()) {
						BitmapImage bitmapImage = new BitmapImage();
						bitmapImage.DecodePixelHeight = (int)CoverContainer.Height;
						await bitmapImage.SetSourceAsync(fileStream);
						CoverBrush.ImageSource = bitmapImage;
					}
				}
				

				String title = meta.Title;
				String artist = meta.Artist;

				bool is_topic = false;
				if (artist.EndsWith(" - Topic")) {
					artist = artist.Substring(0, artist.Length - 8);
					is_topic = true;
				}
				if (artist.EndsWith("VEVO")) artist = artist.Substring(0, artist.Length - 4);
				if (artist.Length > 28) artist = artist.Substring(0, 28) + "...";

				if (!is_topic) {
					if (title.ToUpper().StartsWith(artist.ToUpper())) title = title.Substring(artist.Length, title.Length - artist.Length).TrimStart();
					if (title.StartsWith("-") || title.StartsWith(":") || title.StartsWith("–")) title = title.Substring(1, title.Length - 1).TrimStart();
				}
				if (title.Length > 28) title = title.Substring(0, 28) + "...";

				LabelSong.Text = title;
				LabelArtist.Text = artist;
			});
		}

		private void TimelineUpdated(Timeline timeline) {
			DispatcherQueue.TryEnqueue(() => {
				TimeSpan time_cur = timeline.Position;
				TimeSpan time_total = timeline.EndTime;
				LabelTimeTotal.Text = timespan_to_str(time_total.Hours, time_total.Minutes, time_total.Seconds);
				SliderTime.Maximum = timeline.EndTime.TotalSeconds;

				position_sec = time_cur.Hours * 3600 + time_cur.Minutes * 60 + time_cur.Seconds;
				OnTimedEvent(null, null);
			});
		}

		private void PlaybackUpdated(PlaybackInfo playback) {
			DispatcherQueue.TryEnqueue(async () => {
				if (playback.PlaybackStatus == PlaybackStatus.Playing) {
					if (!timer.IsEnabled) {
						PlaySym.Glyph = "\uE769";
						timer.Start();
					}
				} else {
					timer.Stop();
					PlaySym.Glyph = "\uE768";
				}
			});
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


		// This is not a good practice
		private void PrevBtn_Click(object sender, RoutedEventArgs e) {
			if (api.cur_session != null)
				api.cur_session.TrySkipPreviousAsync();
		}

		private void PlayBtn_Click(object sender, RoutedEventArgs e) {
			if (api.cur_session != null)
				api.cur_session.TryTogglePlayPauseAsync();
		}

		private void NextBtn_Click(object sender, RoutedEventArgs e) {
			if (api.cur_session != null)
				api.cur_session.TrySkipNextAsync();
		}
	}
}
