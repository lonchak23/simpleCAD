using simpleCAD;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace simpleCAD_Example
{
	public class MainWindow_ViewModel : BaseViewModel
	{
		// Link to main window. For SimpleCAD control sizes in create new document command.
		MainWindow m_MainWindow = null;
		public MainWindow_ViewModel(MainWindow mw)
		{
			m_MainWindow = mw;
			m_DocManager.OnDocumentChanged += M_DocManager_OnDocumentChanged;
		}

		//=============================================================================
		public double SimpleCAD_ActualHeight
		{
			get
			{
				if (m_MainWindow != null)
					return m_MainWindow.SimpleCAD_ActualHeight;

				return 0.0;
			}
		}

		//=============================================================================
		public double SimpleCAD_ActualWidth
		{
			get
			{
				if (m_MainWindow != null)
					return m_MainWindow.SimpleCAD_ActualWidth;

				return 0.0;
			}
		}

		//=============================================================================
		private void M_DocManager_OnDocumentChanged(object sender, System.EventArgs e)
		{
			foreach(ICommand cmd in m_commands)
			{
				Command myCommand = cmd as Command;
				if (myCommand != null)
					myCommand.RaiseCanExecuteChanged();
			}
		}

		//=============================================================================
		private DocumentManager m_DocManager = new DocumentManager();
		public DocumentManager DocManager
		{
			get
			{
				return m_DocManager;
			}
		}

		//=============================================================================
		private ObservableCollection<ICommand> m_commands = new ObservableCollection<ICommand>()
		{
			new Command_New(),
			new Command_Open(),
			new Command_Save(),
			new Command_Undo(),
			new Command_Redo()
		};
		public ObservableCollection<ICommand> Commands { get { return m_commands; } }
	}
}
