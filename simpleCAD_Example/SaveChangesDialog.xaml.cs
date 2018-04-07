using System.Windows.Controls;

namespace simpleCAD_Example
{
	/// <summary>
	/// Interaction logic for SaveChangesDialog.xaml
	/// </summary>
	public partial class SaveChangesDialog : UserControl
	{
		SaveChangesDialog_ViewModel m_vm = null;

		public SaveChangesDialog(SaveChangesDialog_ViewModel vm)
		{
			InitializeComponent();

			m_vm = vm;
			DataContext = m_vm;
		}
	}
}
