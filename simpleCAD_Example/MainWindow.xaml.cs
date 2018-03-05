using System.Windows;

namespace simpleCAD_Example
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			this.KeyDown += MainWindow_KeyDown;
		}

		//=============================================================================
		private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			sCAD.OnKeyDown(sender, e);
		}
	}
}
