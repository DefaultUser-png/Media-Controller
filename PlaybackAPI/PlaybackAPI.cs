using System;
using System.Threading.Tasks;
using Windows.Media.Control;
using Meta = Windows.Media.Control.GlobalSystemMediaTransportControlsSessionMediaProperties;
using PlaybackInfo = Windows.Media.Control.GlobalSystemMediaTransportControlsSessionPlaybackInfo;
using Timeline = Windows.Media.Control.GlobalSystemMediaTransportControlsSessionTimelineProperties;
using Session = Windows.Media.Control.GlobalSystemMediaTransportControlsSession;
using SessionManager = Windows.Media.Control.GlobalSystemMediaTransportControlsSessionManager;
using Windows.Foundation;

namespace Media_Controller {

	public class PlaybackAPI {
		public SessionManager manager;

		public delegate void PlaybackHandler(PlaybackInfo playback);
		public delegate void TimelineHandler(Timeline timeline);
		public delegate Task AsyncMetaHandler(Meta meta);

		public event PlaybackHandler? PlaybackUpdated;
		public event TimelineHandler? TimelineUpdated;
		public event AsyncMetaHandler? MetaUpdated;


		public PlaybackAPI() {
			manager = SessionManager.RequestAsync().GetAwaiter().GetResult();
			manager.CurrentSessionChanged += OnSessionChanged;
			OnSessionChanged(manager, null);
		}

		private void OnSessionChanged(SessionManager manager, CurrentSessionChangedEventArgs? args) {
			Session s = manager.GetCurrentSession();
			if (s is not null) {
				s.PlaybackInfoChanged += OnPlaybackChanged;
				s.TimelinePropertiesChanged += OnTimelineChanged;
				s.MediaPropertiesChanged += OnMetaChanged;
				OnPlaybackChanged(s, null);
				OnTimelineChanged(s, null);
				OnMetaChanged(s, null);
			}
		}

		private void OnPlaybackChanged(Session s, PlaybackInfoChangedEventArgs? args) {
			PlaybackUpdated?.Invoke(s.GetPlaybackInfo());
		}

		private void OnTimelineChanged(Session s, TimelinePropertiesChangedEventArgs? args) {
			TimelineUpdated?.Invoke(s.GetTimelineProperties());
		}

		private void OnMetaChanged(Session s, MediaPropertiesChangedEventArgs? args) {
			MetaTask(s);
		}


		async private Task MetaTask(Session s) {
			if (MetaUpdated is not null) {
				IAsyncOperation<Meta> op = s.TryGetMediaPropertiesAsync();
				await op;
				Meta m = op.GetResults();
				await MetaUpdated.Invoke(m);
			}
		}

	}
}
