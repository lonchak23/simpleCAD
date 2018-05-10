using simpleCAD;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace simpleCAD_Example
{
	public class MainWindow_ViewModel : BaseViewModel
	{
		public MainWindow_ViewModel()
		{
			m_DocManager.OnDocumentChanged += M_DocManager_OnDocumentChanged;
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
