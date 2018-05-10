using simpleCAD;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;

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

				string strDisplayName = Path;
				try
				{
					// regex and cut only filename
					string strPattern = @"[^\\]+\.scad$";
					Regex regex = new Regex(strPattern);
					Match match = regex.Match(Path);

					strDisplayName = match.Value;
				}
				catch { }

				return strDisplayName;
			}
		}

		//=============================================================================
		public int ChangesCount
		{
			get
			{
				// For new document add 1 to current state index.
				if (IsItNewDocument)
					return m_CurrentStateIndex + 1;

				return m_CurrentStateIndex;
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
				NotifyPropertyChanged(() => CanUndo);
				NotifyPropertyChanged(() => CanRedo);

				if (m_DocManager != null)
					m_DocManager.RaiseDocumentChanged();
			}
		}

		//=============================================================================
		public bool CanUndo
		{
			get
			{
				if (m_CurrentStateIndex > 0)
					return true;

				return false;
			}
		}

		//=============================================================================
		public bool CanRedo
		{
			get
			{
				if (m_CurrentStateIndex >= 0 && m_CurrentStateIndex < m_states.Count - 1)
					return true;

				return false;
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

			//
			if (strPath != Path)
				Path = strPath;

			FileStream fs = new FileStream(Path, FileMode.OpenOrCreate);
			if (fs == null)
				return false;

			SimpleCAD_State state = m_states[m_states.Count - 1];

			BinaryFormatter bf = new BinaryFormatter();
			bf.Serialize(fs, state);

			//
			if (m_states.Count > 1)
			{
				m_states.RemoveRange(0, m_states.Count - 1);
				m_CurrentStateIndex = 0;
			}

			NotifyPropertyChanged(() => DisplayName);
			NotifyPropertyChanged(() => ChangesCount);

			return true;
		}

		//=============================================================================
		public void Undo()
		{
			if (!CanUndo)
				return;

			--m_CurrentStateIndex;

			NotifyPropertyChanged(() => ChangesCount);
			NotifyPropertyChanged(() => CanUndo);
			NotifyPropertyChanged(() => CanRedo);
			NotifyPropertyChanged(() => CurrentState);

			if (m_DocManager != null)
				m_DocManager.RaiseDocumentChanged();
		}

		//=============================================================================
		public void Redo()
		{
			if (!CanRedo)
				return;

			++m_CurrentStateIndex;

			NotifyPropertyChanged(() => ChangesCount);
			NotifyPropertyChanged(() => CanUndo);
			NotifyPropertyChanged(() => CanRedo);
			NotifyPropertyChanged(() => CurrentState);

			if (m_DocManager != null)
				m_DocManager.RaiseDocumentChanged();
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
		public event EventHandler OnDocumentChanged;
		public void RaiseDocumentChanged()
		{
			if (OnDocumentChanged != null)
				OnDocumentChanged(this, null);
		}

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
				RaiseDocumentChanged();
			}
		}

		//=============================================================================
		public Document Add(string strPath, SimpleCAD_State state)
		{
			return Add(new Document(this, strPath, state));
		}
		public Document Add(Document doc)
		{
			if (doc == null)
				return null;

			OpenDocuments.Add(doc);
			return doc;
		}
	}
}
