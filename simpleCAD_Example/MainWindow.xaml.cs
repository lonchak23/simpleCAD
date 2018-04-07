using MaterialDesignThemes.Wpf;
using simpleCAD;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Controls;

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
				strFilePath = _GetPath();
				if (string.IsNullOrEmpty(strFilePath))
					return;
			}

			curDoc.Save(strFilePath);
		}

		//=============================================================================
		private string _GetPath()
		{
			string strFilePath = null;

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

			return strFilePath;
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

		//=============================================================================
		private void NewButton_Click(object sender, RoutedEventArgs e)
		{
			m_VM.DocManager.Add(new NewDocument(m_VM.DocManager, sCAD.ActualWidth, sCAD.ActualHeight));
		}

		//=============================================================================
		private async void CloseDocumentButton_Click(object sender, RoutedEventArgs e)
		{
			Button btn = sender as Button;
			if (btn == null)
				return;

			ListBoxItem lbi = Utils.TryFindParent<ListBoxItem>(btn);
			if (lbi == null)
				return;

			Document docToClose = lbi.DataContext as Document;
			if (docToClose == null)
				return;

			if(docToClose.ChangesCount > 0)
			{
				StringBuilder sb = new StringBuilder();
				sb.Append("Document \"");
				sb.Append(docToClose.DisplayName);
				sb.Append("\" has changes. Do you want to save changes?");

				SaveChangesDialog_ViewModel vm = new SaveChangesDialog_ViewModel();
				vm.Text = sb.ToString();

				SaveChangesDialog saveChangesDialog = new SaveChangesDialog(vm);

				//show the dialog
				// true - save
				// false - cancel
				// null - continue
				var result = await DialogHost.Show(saveChangesDialog);

				if(result is bool)
				{
					bool bRes = (bool)result;

					// cancel - dont close document
					if (!bRes)
						return;

					// save doc
					if (docToClose.IsItNewDocument)
					{
						string strFilePath = _GetPath();
						if (string.IsNullOrEmpty(strFilePath))
							return;

						docToClose.Save(strFilePath);
					}
					else
						docToClose.Save();
				}
			}

			// select document
			int iOldIndex = m_VM.DocManager.OpenDocuments.IndexOf(docToClose);
			
			m_VM.DocManager.OpenDocuments.Remove(docToClose);

			if(m_VM.DocManager.OpenDocuments.Count > 0)
			{
				if (iOldIndex >= m_VM.DocManager.OpenDocuments.Count)
					iOldIndex = m_VM.DocManager.OpenDocuments.Count - 1;

				Document docToSelect = m_VM.DocManager.OpenDocuments[iOldIndex];
				if (docToSelect != null)
					docToSelect.IsSelected = true;
			}
		}

		//=============================================================================
		// Window closing event cant await for save changes dialog.
		// So mark event as canceled and call save changes dialog.
		// If user click "cancel" then it is nothing to do - event salready canceled.
		// If user click "continue" then set m_ShouldClose to "true" and try to close application again.
		// At the second try m_ShouldClose != null and we will not enter in save changes dialog section.
		//
		// m_ShouldClose is marker
		// - null - mark event canceled and call save changes dialog
		// - not null - do nothing and let window close
		private bool? m_ShouldClose = null;
		private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (m_ShouldClose == null)
			{
				// mark comment to m_ShouldClose
				e.Cancel = true;

				// is here unsaved documents?
				bool bUnsaved = false;
				foreach (Document doc in m_VM.DocManager.OpenDocuments)
				{
					if (doc.ChangesCount > 0)
					{
						bUnsaved = true;
						break;
					}
				}

				//
				if (bUnsaved)
				{
					SaveChangesDialog_ViewModel vm = new SaveChangesDialog_ViewModel();
					vm.Text = "There are unsaved documents. Some data will be lost possibly.";
					vm.IsSaveButtonVisible = false;

					SaveChangesDialog saveChangesDialog = new SaveChangesDialog(vm);

					//show the dialog
					// true - save
					// false - cancel
					// null - continue
					var result = await DialogHost.Show(saveChangesDialog);

					if (result is bool && !(bool)result)
						return;
				}

				m_ShouldClose = true;
				Application.Current.Shutdown();
			}
		}
	}
}
