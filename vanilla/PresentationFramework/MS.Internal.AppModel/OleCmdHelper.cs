using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using MS.Internal.Documents;
using MS.Internal.KnownBoxes;

namespace MS.Internal.AppModel;

internal sealed class OleCmdHelper : MarshalByRefObject
{
	internal const int OLECMDERR_E_NOTSUPPORTED = -2147221248;

	internal const int OLECMDERR_E_DISABLED = -2147221247;

	internal const int OLECMDERR_E_UNKNOWNGROUP = -2147221244;

	internal const uint CommandUnsupported = 0u;

	internal const uint CommandEnabled = 3u;

	internal const uint CommandDisabled = 1u;

	internal static readonly Guid CGID_ApplicationCommands = new Guid(3955001955u, 34137, 18578, 151, 168, 49, 233, 176, 233, 133, 145);

	internal static readonly Guid CGID_EditingCommands = new Guid(209178181, 3356, 20266, 178, 147, 237, 213, 226, 126, 186, 71);

	private MS.Internal.SecurityCriticalDataForSet<SortedList> _oleCmdMappingTable;

	private MS.Internal.SecurityCriticalDataForSet<Hashtable> _applicationCommandsMappingTable;

	private MS.Internal.SecurityCriticalDataForSet<SortedList> _editingCommandsMappingTable;

	internal OleCmdHelper()
	{
	}

	internal void QueryStatus(Guid guidCmdGroup, uint cmdId, ref uint flags)
	{
		if (Application.Current == null || Application.IsShuttingDown)
		{
			Marshal.ThrowExceptionForHR(-2147467259);
		}
		IDictionary oleCmdMappingTable = GetOleCmdMappingTable(guidCmdGroup);
		if (oleCmdMappingTable == null)
		{
			Marshal.ThrowExceptionForHR(-2147221244);
		}
		if (!(oleCmdMappingTable[cmdId] is CommandWithArgument arg))
		{
			flags = 0u;
			return;
		}
		bool flag = (bool)Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, new DispatcherOperationCallback(QueryEnabled), arg);
		flags = ((!flag) ? 1u : 3u);
	}

	private object QueryEnabled(object command)
	{
		if (Application.Current.MainWindow == null)
		{
			return false;
		}
		IInputElement inputElement = FocusManager.GetFocusedElement(Application.Current.MainWindow);
		if (inputElement == null)
		{
			inputElement = Application.Current.MainWindow;
		}
		return BooleanBoxes.Box(((CommandWithArgument)command).QueryEnabled(inputElement, null));
	}

	internal void ExecCommand(Guid guidCmdGroup, uint commandId, object arg)
	{
		if (Application.Current == null || Application.IsShuttingDown)
		{
			Marshal.ThrowExceptionForHR(-2147467259);
		}
		int num = (int)Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, new DispatcherOperationCallback(ExecCommandCallback), new object[3] { guidCmdGroup, commandId, arg });
		if (num < 0)
		{
			Marshal.ThrowExceptionForHR(num);
		}
	}

	private object ExecCommandCallback(object arguments)
	{
		object[] obj = (object[])arguments;
		Invariant.Assert(obj.Length == 3);
		Guid guidCmdGroup = (Guid)obj[0];
		uint num = (uint)obj[1];
		object argument = obj[2];
		IDictionary oleCmdMappingTable = GetOleCmdMappingTable(guidCmdGroup);
		if (oleCmdMappingTable == null)
		{
			return -2147221244;
		}
		if (!(oleCmdMappingTable[num] is CommandWithArgument commandWithArgument))
		{
			return -2147221248;
		}
		if (Application.Current.MainWindow == null)
		{
			return -2147221247;
		}
		IInputElement inputElement = FocusManager.GetFocusedElement(Application.Current.MainWindow);
		if (inputElement == null)
		{
			inputElement = Application.Current.MainWindow;
		}
		return (!commandWithArgument.Execute(inputElement, argument)) ? (-2147221247) : 0;
	}

	private IDictionary GetOleCmdMappingTable(Guid guidCmdGroup)
	{
		IDictionary result = null;
		if (guidCmdGroup.Equals(CGID_ApplicationCommands))
		{
			EnsureApplicationCommandsTable();
			result = _applicationCommandsMappingTable.Value;
		}
		else if (guidCmdGroup.Equals(Guid.Empty))
		{
			EnsureOleCmdMappingTable();
			result = _oleCmdMappingTable.Value;
		}
		else if (guidCmdGroup.Equals(CGID_EditingCommands))
		{
			EnsureEditingCommandsTable();
			result = _editingCommandsMappingTable.Value;
		}
		return result;
	}

	private void EnsureOleCmdMappingTable()
	{
		if (_oleCmdMappingTable.Value == null)
		{
			_oleCmdMappingTable.Value = new SortedList(10);
			_oleCmdMappingTable.Value.Add(3u, new CommandWithArgument(ApplicationCommands.Save));
			_oleCmdMappingTable.Value.Add(4u, new CommandWithArgument(ApplicationCommands.SaveAs));
			_oleCmdMappingTable.Value.Add(6u, new CommandWithArgument(ApplicationCommands.Print));
			_oleCmdMappingTable.Value.Add(11u, new CommandWithArgument(ApplicationCommands.Cut));
			_oleCmdMappingTable.Value.Add(12u, new CommandWithArgument(ApplicationCommands.Copy));
			_oleCmdMappingTable.Value.Add(13u, new CommandWithArgument(ApplicationCommands.Paste));
			_oleCmdMappingTable.Value.Add(10u, new CommandWithArgument(ApplicationCommands.Properties));
			_oleCmdMappingTable.Value.Add(22u, new CommandWithArgument(NavigationCommands.Refresh));
			_oleCmdMappingTable.Value.Add(23u, new CommandWithArgument(NavigationCommands.BrowseStop));
		}
	}

	private void EnsureApplicationCommandsTable()
	{
		if (_applicationCommandsMappingTable.Value == null)
		{
			_applicationCommandsMappingTable.Value = new Hashtable(19);
			_applicationCommandsMappingTable.Value.Add(8001u, new CommandWithArgument(ApplicationCommands.Cut));
			_applicationCommandsMappingTable.Value.Add(8002u, new CommandWithArgument(ApplicationCommands.Copy));
			_applicationCommandsMappingTable.Value.Add(8003u, new CommandWithArgument(ApplicationCommands.Paste));
			_applicationCommandsMappingTable.Value.Add(8004u, new CommandWithArgument(ApplicationCommands.SelectAll));
			_applicationCommandsMappingTable.Value.Add(8005u, new CommandWithArgument(ApplicationCommands.Find));
			_applicationCommandsMappingTable.Value.Add(8016u, new CommandWithArgument(NavigationCommands.Refresh));
			_applicationCommandsMappingTable.Value.Add(8015u, new CommandWithArgument(NavigationCommands.BrowseStop));
			_applicationCommandsMappingTable.Value.Add(8007u, new CommandWithArgument(DocumentApplicationDocumentViewer.Sign));
			_applicationCommandsMappingTable.Value.Add(8008u, new CommandWithArgument(DocumentApplicationDocumentViewer.RequestSigners));
			_applicationCommandsMappingTable.Value.Add(8009u, new CommandWithArgument(DocumentApplicationDocumentViewer.ShowSignatureSummary));
			_applicationCommandsMappingTable.Value.Add(8011u, new CommandWithArgument(DocumentApplicationDocumentViewer.ShowRMPublishingUI));
			_applicationCommandsMappingTable.Value.Add(8012u, new CommandWithArgument(DocumentApplicationDocumentViewer.ShowRMPermissions));
			_applicationCommandsMappingTable.Value.Add(8013u, new CommandWithArgument(DocumentApplicationDocumentViewer.ShowRMCredentialManager));
			_applicationCommandsMappingTable.Value.Add(8019u, new CommandWithArgument(NavigationCommands.IncreaseZoom));
			_applicationCommandsMappingTable.Value.Add(8020u, new CommandWithArgument(NavigationCommands.DecreaseZoom));
			_applicationCommandsMappingTable.Value.Add(8021u, new CommandWithArgument(NavigationCommands.Zoom, 400));
			_applicationCommandsMappingTable.Value.Add(8022u, new CommandWithArgument(NavigationCommands.Zoom, 250));
			_applicationCommandsMappingTable.Value.Add(8023u, new CommandWithArgument(NavigationCommands.Zoom, 150));
			_applicationCommandsMappingTable.Value.Add(8024u, new CommandWithArgument(NavigationCommands.Zoom, 100));
			_applicationCommandsMappingTable.Value.Add(8025u, new CommandWithArgument(NavigationCommands.Zoom, 75));
			_applicationCommandsMappingTable.Value.Add(8026u, new CommandWithArgument(NavigationCommands.Zoom, 50));
			_applicationCommandsMappingTable.Value.Add(8027u, new CommandWithArgument(NavigationCommands.Zoom, 25));
			_applicationCommandsMappingTable.Value.Add(8028u, new CommandWithArgument(DocumentViewer.FitToWidthCommand));
			_applicationCommandsMappingTable.Value.Add(8029u, new CommandWithArgument(DocumentViewer.FitToHeightCommand));
			_applicationCommandsMappingTable.Value.Add(8030u, new CommandWithArgument(DocumentViewer.FitToMaxPagesAcrossCommand, 2));
			_applicationCommandsMappingTable.Value.Add(8031u, new CommandWithArgument(DocumentViewer.ViewThumbnailsCommand));
		}
	}

	private void EnsureEditingCommandsTable()
	{
		if (_editingCommandsMappingTable.Value == null)
		{
			_editingCommandsMappingTable.Value = new SortedList(2);
			_editingCommandsMappingTable.Value.Add(1u, new CommandWithArgument(EditingCommands.Backspace));
			_editingCommandsMappingTable.Value.Add(2u, new CommandWithArgument(EditingCommands.Delete));
		}
	}
}
