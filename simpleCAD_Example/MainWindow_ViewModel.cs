using simpleCAD;

namespace simpleCAD_Example
{
	public class MainWindow_ViewModel : BaseViewModel
	{
		public MainWindow_ViewModel() { }

		//=============================================================================
		private DocumentManager m_DocManager = new DocumentManager();
		public DocumentManager DocManager
		{
			get
			{
				return m_DocManager;
			}
		}
	}
}
