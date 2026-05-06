using Media_Controller.Widget;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Meta = Windows.Media.Control.GlobalSystemMediaTransportControlsSessionMediaProperties;
using PlaybackInfo = Windows.Media.Control.GlobalSystemMediaTransportControlsSessionPlaybackInfo;
using PlaybackStatus = Windows.Media.Control.GlobalSystemMediaTransportControlsSessionPlaybackStatus;
using Timeline = Windows.Media.Control.GlobalSystemMediaTransportControlsSessionTimelineProperties;

namespace Media_Controller {
	public sealed partial class PlayerWindow : Window {
		private PlaybackAPI api;
		private DispatcherTimer timer;
		private int seconds_current;
		private int seconds_max;
		private bool slider_dragged;

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
			timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(1);

			api = new PlaybackAPI();
			timer.Tick += OnTimerTick;
			api.PlaybackUpdated += OnPlaybackUpdated;
			api.TimelineUpdated += OnTimelineUpdated;
			api.MetaUpdated += OnMetaUpdated;
			api.ForceUpdate();
		}




		private String timespan_to_str(int h, int m, int s) {
			String output = "";
			if (h > 0) output = $"{h:0}:";
			output += $"{m:0}:{s:00}";
			return output;
		}



		private void OnTimerTick(object? sender, object? e) {
			if (seconds_current >= seconds_max) {
				timer.Stop();
				return;
			}

			seconds_current += 1;
			int m = seconds_current / 60;
			int h = m / 60;
			LabelTimeNow.Text = timespan_to_str(h, m, seconds_current % 60);
			if (!slider_dragged) SliderTime.Value = seconds_current;
		}

		async private Task OnMetaUpdated(Meta meta) {
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

		private void OnTimelineUpdated(Timeline timeline) {
			DispatcherQueue.TryEnqueue(() => {
				TimeSpan time_cur = timeline.Position;
				TimeSpan time_total = timeline.EndTime;
				LabelTimeTotal.Text = timespan_to_str(time_total.Hours, time_total.Minutes, time_total.Seconds);
				SliderTime.Maximum = timeline.EndTime.TotalSeconds;

				seconds_current = time_cur.Hours * 3600 + time_cur.Minutes * 60 + time_cur.Seconds;
				seconds_max = time_total.Hours * 3600 + time_total.Minutes * 60 + time_total.Seconds;

				LabelTimeNow.Text = timespan_to_str(time_cur.Hours, time_cur.Minutes, seconds_current % 60);

				if (!slider_dragged) SliderTime.Value = seconds_current;
			});
		}

		private void OnPlaybackUpdated(PlaybackInfo playback) {
			if (playback == null) return;
			DispatcherQueue.TryEnqueue(() => {
				if (playback.PlaybackStatus == PlaybackStatus.Playing) {
					if (!timer.IsEnabled) {
						PlaySym.Glyph = "\uE769";
						timer.Start();
					}
				} else {
					timer.Stop();
					PlaySym.Glyph = "\uE768";
				}

				PrevBtn.IsEnabled = playback.Controls.IsPreviousEnabled;
				PlayBtn.IsEnabled = playback.Controls.IsPlayPauseToggleEnabled;
				NextBtn.IsEnabled = playback.Controls.IsNextEnabled;
			});
		}


		private void Slider_Loaded(object sender, RoutedEventArgs e) {
			SliderTime.AddHandler(
				UIElement.PointerPressedEvent,
				new PointerEventHandler(OnSliderDrag),
				true
			);
			SliderTime.PointerCaptureLost += OnSliderReleased;
		}

		private void OnSliderDrag(object sender, object e) {
			slider_dragged = true;
		}

		private void OnSliderReleased(object sender, object e) {
			slider_dragged = false;
			seconds_current = (int)SliderTime.Value;
			int m = seconds_current / 60;
			int h = m / 60;
			LabelTimeNow.Text = timespan_to_str(h, m, seconds_current % 60);
			if (api.cur_session != null)
				api.cur_session.TryChangePlaybackPositionAsync(10000000 * (long)SliderTime.Value).AsTask().ContinueWith(
					t => Console.WriteLine(t.Exception),
					TaskContinuationOptions.OnlyOnFaulted
				);
		}


		private void BtnSettings_Click(object sender, RoutedEventArgs e) {

		}

		private void BtnMinimize_Click(object sender, RoutedEventArgs e) {
			if (AppWindow.Presenter is OverlappedPresenter presenter) {
				presenter.Minimize();
			}
		}

		private void BtnClose_Click(object sender, RoutedEventArgs e) {
			Close();
		}


		private void BtnPrev_Click(object sender, RoutedEventArgs e) {
			if (api.cur_session != null)
				api.cur_session.TrySkipPreviousAsync().AsTask().ContinueWith(
					t => Console.WriteLine(t.Exception),
					TaskContinuationOptions.OnlyOnFaulted
				);
		}

		private void BtnPlay_Click(object sender, RoutedEventArgs e) {
			if (api.cur_session != null)
				api.cur_session.TryTogglePlayPauseAsync().AsTask().ContinueWith(
					t => Console.WriteLine(t.Exception),
					TaskContinuationOptions.OnlyOnFaulted
				);
		}

		private void BtnNext_Click(object sender, RoutedEventArgs e) {
			if (api.cur_session != null)
				api.cur_session.TrySkipNextAsync().AsTask().ContinueWith(
					t => Console.WriteLine(t.Exception),
					TaskContinuationOptions.OnlyOnFaulted
				);
		}
	}
}
