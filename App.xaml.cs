using Microsoft.UI.Xaml;

namespace Media_Controller {
	public partial class App : Application {
		private Window? _window;
		public App() {
			InitializeComponent();
		}

		protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args) {
			_window = new PlayerWindow();
			_window.Activate();
		}
	}
}
