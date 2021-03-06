﻿using simpleCAD;

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

		//=============================================================================
		private bool m_bIsSaveButtonVisible = true;
		public bool IsSaveButtonVisible
		{
			get { return m_bIsSaveButtonVisible; }
			set
			{
				if(m_bIsSaveButtonVisible != value)
				{
					m_bIsSaveButtonVisible = value;
					NotifyPropertyChanged(() => IsSaveButtonVisible);
				}
			}
		}
	}
}
