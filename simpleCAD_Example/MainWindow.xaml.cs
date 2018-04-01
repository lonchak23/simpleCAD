using simpleCAD;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;

namespace simpleCAD_Example
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private MainWindow_ViewModel m_VM = new MainWindow_ViewModel();

		public MainWindow()
		{
			InitializeComponent();

			DataContext = m_VM;

			this.KeyDown += MainWindow_KeyDown;

			// add new document on Loaded
			// Why loaded? - need SimpleCAD control ActualWidth and ActualHeight for correct offset
			this.Loaded += MainWindow_Loaded;
		}

		//=============================================================================
		private void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			m_VM.DocManager.Add(new NewDocument(m_VM.DocManager, sCAD.ActualWidth, sCAD.ActualHeight));
			m_VM.DocManager.Add(new NewDocument(m_VM.DocManager, sCAD.ActualWidth, sCAD.ActualHeight));
		}

		//=============================================================================
		private void MainWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			sCAD.OnKeyDown(sender, e);
		}

		//=============================================================================
		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			Document curDoc = m_VM.DocManager.CurrentDocument;
			if (curDoc == null)
				return;

			string strFilePath = string.Empty;
			if (curDoc.IsItNewDocument)
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
					strFilePath = dlg.FileName;
				else
					return;
			}

			curDoc.Save(strFilePath);
		}

		//=============================================================================
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
				FileStream fs = new FileStream(dlg.FileName, FileMode.Open);
				if (fs != null)
				{

					BinaryFormatter bf = new BinaryFormatter();
					SimpleCAD_State state = (SimpleCAD_State)bf.Deserialize(fs);

					m_VM.DocManager.Add(dlg.FileName, state);
				}
			}
		}
	}
}
