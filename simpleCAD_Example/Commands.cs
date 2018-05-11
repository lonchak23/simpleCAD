using MaterialDesignThemes.Wpf;
using simpleCAD;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace simpleCAD_Example
{
	public class Command : ICommand
	{
		public event EventHandler CanExecuteChanged;

		public Command(PackIconKind iconKind, string strDescription)
		{
			IconKind = iconKind;
			Description = strDescription;
		}

		//=============================================================================
		public PackIconKind IconKind { get; set; }

		//=============================================================================
		public string Description { get; set; }

		//=============================================================================
		public bool CanExecute(object parameter)
		{
			return _CanExecute(parameter);
		}
		protected virtual bool _CanExecute(object parameter)
		{
			return true;
		}

		//=============================================================================
		public void RaiseCanExecuteChanged()
		{
			if (CanExecuteChanged != null)
				CanExecuteChanged(this, null);
		}

		//=============================================================================
		public void Execute(object parameter)
		{
			_Execute(parameter);
		}
		protected virtual void _Execute(object parameter) { }
	}

	public class Command_New : Command
	{
		public Command_New()
			: base(PackIconKind.File, "New") { }

		//=============================================================================
		protected override void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if(vm != null)
			{
				Document newDoc = vm.DocManager.Add(new NewDocument(vm.DocManager, vm.SimpleCAD_ActualWidth, vm.SimpleCAD_ActualHeight));
				if (newDoc != null)
					newDoc.IsSelected = true;
			}
		}
	}

	public class Command_Open : Command
	{
		public Command_Open()
			: base(PackIconKind.Folder, "Open") { }

		//=============================================================================
		protected override void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm != null)
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

						Document newDoc = vm.DocManager.Add(dlg.FileName, state);
						if (newDoc != null)
							newDoc.IsSelected = true;
					}
				}
			}
		}
	}

	public class Command_Save : Command
	{
		public Command_Save()
			: base(PackIconKind.ContentSave, "Save") { }

		//=============================================================================
		protected override void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm != null)
			{
				Document curDoc = vm.DocManager.CurrentDocument;
				if (curDoc == null)
					return;

				string strFilePath = string.Empty;
				if (curDoc.IsItNewDocument)
				{
					strFilePath = MainWindow._GetPath();
					if (string.IsNullOrEmpty(strFilePath))
						return;
				}

				curDoc.Save(strFilePath);
			}
		}
	}

	public class Command_Undo : Command
	{
		public Command_Undo()
			: base(PackIconKind.UndoVariant, "Undo") { }

		//=============================================================================
		protected override bool _CanExecute(object parameter)
		{
			bool bResult = false;

			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm != null && vm.DocManager != null)
			{
				Document curDoc = vm.DocManager.CurrentDocument;
				if (curDoc != null && curDoc.CanUndo)
					bResult = true;
			}

			return bResult;
		}

		//=============================================================================
		protected override void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm != null && vm.DocManager != null)
			{
				Document curDoc = vm.DocManager.CurrentDocument;
				if (curDoc != null)
					curDoc.Undo();
			}
		}
	}

	public class Command_Redo : Command
	{
		public Command_Redo()
			: base(PackIconKind.RedoVariant, "Redo") { }

		//=============================================================================
		protected override bool _CanExecute(object parameter)
		{
			bool bResult = false;

			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm != null && vm.DocManager != null)
			{
				Document curDoc = vm.DocManager.CurrentDocument;
				if (curDoc != null && curDoc.CanRedo)
					bResult = true;
			}

			return bResult;
		}

		//=============================================================================
		protected override void _Execute(object parameter)
		{
			MainWindow_ViewModel vm = parameter as MainWindow_ViewModel;
			if (vm != null && vm.DocManager != null)
			{
				Document curDoc = vm.DocManager.CurrentDocument;
				if (curDoc != null)
					curDoc.Redo();
			}
		}
	}
}
