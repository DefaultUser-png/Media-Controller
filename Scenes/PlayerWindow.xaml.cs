using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System.Threading.Tasks;

namespace Media_Controller {
	public sealed partial class PlayerWindow: Window {
		public PlayerWindow() {
			InitializeComponent();

			AppWindow.Resize(new Windows.Graphics.SizeInt32(450, 160));
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(TitleBar);
            //SizeChanged += SampleWindow3_SizeChanged;
            if (AppWindow.Presenter is OverlappedPresenter presenter) {
                presenter.IsResizable = false;
                presenter.IsMaximizable = false;
                presenter.IsMinimizable = false;
                presenter.SetBorderAndTitleBar(false, false);
            }

        }

		/*
		private void SampleWindow3_SizeChanged(object sender, WindowSizeChangedEventArgs e)
		{
			OverlappedPresenter presenter = (OverlappedPresenter)AppWindow.Presenter;
			MaximizeRestoreBtn.Content = presenter.State == OverlappedPresenterState.Maximized ? "Restore" : "Maximize";
		}
		*/

		private void MinimizeBtn_Click(object sender, RoutedEventArgs e) {
			if (AppWindow.Presenter is OverlappedPresenter presenter) {
				presenter.Minimize();
			}
		}

		private void CloseBtn_Click(object sender, RoutedEventArgs e) {
			this.Close();
		}
	}


}
