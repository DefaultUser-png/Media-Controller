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
		public Session? cur_session;

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



		public void ForceUpdate() {
			Session s = manager.GetCurrentSession();
			OnPlaybackChanged(s, null);
			OnTimelineChanged(s, null);
			OnMetaChanged(s, null);
		}

		private void OnSessionChanged(SessionManager manager, CurrentSessionChangedEventArgs? args) {
			if (cur_session != null) {
				cur_session.PlaybackInfoChanged -= OnPlaybackChanged;
				cur_session.TimelinePropertiesChanged -= OnTimelineChanged;
				cur_session.MediaPropertiesChanged -= OnMetaChanged;
				OnPlaybackChanged(cur_session, null);
				OnTimelineChanged(cur_session, null);
				OnMetaChanged(cur_session, null);
			}
			cur_session = manager.GetCurrentSession();
			if (cur_session != null) {
				cur_session.PlaybackInfoChanged += OnPlaybackChanged;
				cur_session.TimelinePropertiesChanged += OnTimelineChanged;
				cur_session.MediaPropertiesChanged += OnMetaChanged;
				OnPlaybackChanged(cur_session, null);
				OnTimelineChanged(cur_session, null);
				OnMetaChanged(cur_session, null);
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
