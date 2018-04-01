using simpleCAD;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace simpleCAD_Example
{
	public class Document : BaseViewModel
	{
		public Document(DocumentManager docManager, string strPath, SimpleCAD_State state)
		{
			m_DocManager = docManager;
			Path = strPath;
			if (state != null)
			{
				m_states.Add(state);
				m_CurrentStateIndex = 0;
			}
		}

		//=============================================================================
		private DocumentManager m_DocManager = null;

		//=============================================================================
		public bool IsItNewDocument
		{
			get
			{
				return string.IsNullOrEmpty(Path);
			}
		}

		//=============================================================================
		private bool m_IsSelected = false;
		public bool IsSelected
		{
			get
			{
				return m_IsSelected;
			}
			set
			{
				if(m_IsSelected != value)
				{
					m_IsSelected = value;
					NotifyPropertyChanged(() => IsSelected);
				}
			}
		}

		//=============================================================================
		public string Path { get; private set; }

		//=============================================================================
		private string m_strNewDocName = string.Empty;
		public string DisplayName
		{
			get
			{
				if(IsItNewDocument)
				{
					if (string.IsNullOrEmpty(m_strNewDocName))
					{
						m_strNewDocName = "NewDocument";
						if(m_DocManager != null)
							m_strNewDocName += m_DocManager.OpenDocuments.Count;
					}

					return m_strNewDocName;
				}

				// regex and cut only filename
				return Path;
			}
		}

		//=============================================================================
		public int ChangesCount
		{
			get
			{
				if (IsItNewDocument)
					return m_states.Count;

				return m_states.Count - 1;
			}
		}

		//=============================================================================
		private List<SimpleCAD_State> m_states = new List<SimpleCAD_State>();

		//=============================================================================
		private int m_CurrentStateIndex = -1;
		public SimpleCAD_State CurrentState
		{
			get
			{
				if (m_CurrentStateIndex >= 0 && m_CurrentStateIndex < m_states.Count)
					return m_states[m_CurrentStateIndex];

				return null;
			}
			set
			{
				//add new state after current
				if(m_CurrentStateIndex < 0)
				{
					m_states.Add(value);
					m_CurrentStateIndex = 1;
				}
				else if (m_CurrentStateIndex >= 0 && m_CurrentStateIndex < m_states.Count)
				{
					if (m_CurrentStateIndex < m_states.Count - 1)
						m_states.RemoveRange(m_CurrentStateIndex + 1, m_states.Count - 1 - m_CurrentStateIndex);

					m_states.Add(value);
					++m_CurrentStateIndex;
				}

				NotifyPropertyChanged(() => ChangesCount);
			}
		}

		//=============================================================================
		public bool Save()
		{
			return this.Save(string.Empty);
		}
		public bool Save(string strNewPath)
		{
			string strPath = strNewPath;
			if (string.IsNullOrEmpty(strPath))
				strPath = Path;

			if (string.IsNullOrEmpty(strPath))
				return false;

			if (m_states.Count == 0)
				return false;

			FileStream fs = new FileStream(strPath, FileMode.OpenOrCreate);
			if (fs == null)
				return false;

			SimpleCAD_State state = m_states[m_states.Count - 1];

			BinaryFormatter bf = new BinaryFormatter();
			bf.Serialize(fs, state);

			if (!string.IsNullOrEmpty(strNewPath) && strNewPath != Path)
				Path = strNewPath;

			return true;
		}
	}

	public class NewDocument : Document
	{
		public NewDocument(DocumentManager docManager, double rControlWidth, double rControlHeight)
			: base(docManager, string.Empty, new DefaultState(rControlWidth, rControlHeight)) { }
	}

	public class DocumentManager : BaseViewModel
	{
		//=============================================================================
		private ObservableCollection<Document> m_OpenDocuments = new ObservableCollection<Document>();
		public ObservableCollection<Document> OpenDocuments
		{
			get
			{
				return m_OpenDocuments;
			}
		}

		//=============================================================================
		private Document m_CurrentDocument = null;
		public Document CurrentDocument
		{
			get
			{
				return m_CurrentDocument;
			}
			set
			{
				m_CurrentDocument = value;
				NotifyPropertyChanged(() => CurrentDocument);
			}
		}

		//=============================================================================
		public bool Add(Document doc)
		{
			if (doc == null)
				return false;

			OpenDocuments.Add(doc);
			return true;
		}
	}
}
