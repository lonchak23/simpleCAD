using System;
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

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			// Create SaveFileDialog
			Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

			// Set filter for file extension and default file extension
			dlg.DefaultExt = ".scad";
			dlg.Filter = "SimpleCAD drawings (.scad)|*.scad";

			// Display OpenFileDialog by calling ShowDialog method
			Nullable<bool> result = dlg.ShowDialog();

			// Get the selected file name and display in a TextBox
			if (result == true)
			{
				// save or create
				sCAD.Save(dlg.FileName);
			}
		}

		private void OpenButton_Click(object sender, RoutedEventArgs e)
		{
			// Create OpenFileDialog
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

			// Set filter for file extension and default file extension
			dlg.DefaultExt = ".scad";
			dlg.Filter = "SimpleCAD drawings (.scad)|*.scad";

			// Display OpenFileDialog by calling ShowDialog method
			Nullable<bool> result = dlg.ShowDialog();

			// Get the selected file name and display in a TextBox
			if (result == true)
			{
				// save or create
				sCAD.Open(dlg.FileName);
			}
		}
	}
}
