using simpleCAD;

namespace simpleCAD_Example
{
	public class SaveChangesDialog_ViewModel : BaseViewModel
	{
		public SaveChangesDialog_ViewModel() { }

		//=============================================================================
		private string m_strText = string.Empty;
		public string Text
		{
			get { return m_strText; }
			set
			{
				m_strText = value;
				NotifyPropertyChanged(() => Text);
			}
		}
	}
}
