using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;

namespace Media_Controller {
	public sealed partial class PlayerWindow: Window {
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

		}



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
