using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Markup;
using MS.Internal;
using MS.Internal.AppModel;
using MS.Internal.Interop;
using MS.Win32;

namespace System.Windows.Shell;

/// <summary>Represents a list of items and tasks displayed as a menu on a Windows 7 taskbar button.</summary>
[ContentProperty("JumpItems")]
public sealed class JumpList : ISupportInitialize
{
	private class _RejectedJumpItemPair
	{
		public JumpItem JumpItem { get; set; }

		public JumpItemRejectionReason Reason { get; set; }
	}

	private class _ShellObjectPair
	{
		public JumpItem JumpItem { get; set; }

		public object ShellObject { get; set; }

		public static void ReleaseShellObjects(List<_ShellObjectPair> list)
		{
			if (list == null)
			{
				return;
			}
			foreach (_ShellObjectPair item in list)
			{
				object comObject = item.ShellObject;
				item.ShellObject = null;
				Utilities.SafeRelease(ref comObject);
			}
		}
	}

	private static readonly object s_lock;

	private static readonly Dictionary<Application, JumpList> s_applicationMap;

	private Application _application;

	private bool? _initializing;

	private List<JumpItem> _jumpItems;

	private static readonly string _FullName;

	/// <summary>Gets or sets a value that indicates whether frequently used items are displayed in the Jump List.</summary>
	/// <returns>true if frequently used items are displayed in the Jump List; otherwise, false. The default is false.</returns>
	public bool ShowFrequentCategory { get; set; }

	/// <summary>Gets or sets a value that indicates whether recently used items are displayed in the Jump List.</summary>
	/// <returns>true if recently used items are displayed in the Jump List; otherwise, false. The default is false.</returns>
	public bool ShowRecentCategory { get; set; }

	/// <summary>Gets the collection of <see cref="T:System.Windows.Shell.JumpItem" /> objects that are displayed in the Jump List.</summary>
	/// <returns>The collection of <see cref="T:System.Windows.Shell.JumpItem" /> objects displayed in the Jump List. The default is an empty collection.</returns>
	public List<JumpItem> JumpItems => _jumpItems;

	private bool IsUnmodified
	{
		get
		{
			if (!_initializing.HasValue && JumpItems.Count == 0 && !ShowRecentCategory)
			{
				return !ShowFrequentCategory;
			}
			return false;
		}
	}

	private static string _RuntimeId
	{
		get
		{
			string AppID;
			MS.Internal.Interop.HRESULT hRESULT = NativeMethods2.GetCurrentProcessExplicitAppUserModelID(out AppID);
			if (hRESULT == MS.Internal.Interop.HRESULT.E_FAIL)
			{
				hRESULT = MS.Internal.Interop.HRESULT.S_OK;
				AppID = null;
			}
			hRESULT.ThrowIfFailed();
			return AppID;
		}
	}

	/// <summary>Occurs when jump items are not successfully added to the Jump List by the Windows shell.</summary>
	public event EventHandler<JumpItemsRejectedEventArgs> JumpItemsRejected;

	/// <summary>Occurs when jump items previously in the Jump List are removed from the list by the user.</summary>
	public event EventHandler<JumpItemsRemovedEventArgs> JumpItemsRemovedByUser;

	static JumpList()
	{
		s_lock = new object();
		s_applicationMap = new Dictionary<Application, JumpList>();
		_FullName = MS.Win32.UnsafeNativeMethods.GetModuleFileName(default(HandleRef));
	}

	/// <summary>Adds the specified item path to the Recent category of the Jump List.</summary>
	/// <param name="itemPath">The path to add to the Jump List.</param>
	public static void AddToRecentCategory(string itemPath)
	{
		Verify.FileExists(itemPath, "itemPath");
		itemPath = Path.GetFullPath(itemPath);
		NativeMethods2.SHAddToRecentDocs(itemPath);
	}

	/// <summary>Adds the specified jump path to the Recent category of the Jump List.</summary>
	/// <param name="jumpPath">The <see cref="T:System.Windows.Shell.JumpPath" /> to add to the Jump List.</param>
	public static void AddToRecentCategory(JumpPath jumpPath)
	{
		Verify.IsNotNull(jumpPath, "jumpPath");
		AddToRecentCategory(jumpPath.Path);
	}

	/// <summary>Adds the specified jump task to the Recent category of the Jump List.</summary>
	/// <param name="jumpTask">The <see cref="T:System.Windows.Shell.JumpTask" /> to add to the Jump List.</param>
	public static void AddToRecentCategory(JumpTask jumpTask)
	{
		Verify.IsNotNull(jumpTask, "jumpTask");
		if (!Utilities.IsOSWindows7OrNewer)
		{
			return;
		}
		IShellLinkW comObject = CreateLinkFromJumpTask(jumpTask, allowSeparators: false);
		try
		{
			if (comObject != null)
			{
				NativeMethods2.SHAddToRecentDocs(comObject);
			}
		}
		finally
		{
			Utilities.SafeRelease(ref comObject);
		}
	}

	/// <summary>Sets the <see cref="T:System.Windows.Shell.JumpList" /> object associated with an application.</summary>
	/// <param name="application">The application associated with the <see cref="T:System.Windows.Shell.JumpList" />.</param>
	/// <param name="value">The <see cref="T:System.Windows.Shell.JumpList" /> to associate with the application.</param>
	public static void SetJumpList(Application application, JumpList value)
	{
		Verify.IsNotNull(application, "application");
		lock (s_lock)
		{
			if (s_applicationMap.TryGetValue(application, out var value2) && value2 != null)
			{
				value2._application = null;
			}
			s_applicationMap[application] = value;
			if (value != null)
			{
				value._application = application;
			}
		}
		value?.ApplyFromApplication();
	}

	/// <summary>Returns the <see cref="T:System.Windows.Shell.JumpList" /> object associated with an application.</summary>
	/// <returns>The <see cref="T:System.Windows.Shell.JumpList" /> object associated with the specified application.</returns>
	/// <param name="application">The application associated with the <see cref="T:System.Windows.Shell.JumpList" />.</param>
	public static JumpList GetJumpList(Application application)
	{
		Verify.IsNotNull(application, "application");
		s_applicationMap.TryGetValue(application, out var value);
		return value;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Shell.JumpList" /> class.</summary>
	public JumpList()
		: this(null, showFrequent: false, showRecent: false)
	{
		_initializing = null;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Shell.JumpList" /> class with the specified parameters.</summary>
	/// <param name="items">The collection of <see cref="T:System.Windows.Shell.JumpItem" /> objects that are displayed in the Jump List.</param>
	/// <param name="showFrequent">A value that indicates whether frequently used items are displayed in the Jump List.</param>
	/// <param name="showRecent">A value that indicates whether recently used items are displayed in the Jump List.</param>
	public JumpList(IEnumerable<JumpItem> items, bool showFrequent, bool showRecent)
	{
		if (items != null)
		{
			_jumpItems = new List<JumpItem>(items);
		}
		else
		{
			_jumpItems = new List<JumpItem>();
		}
		ShowFrequentCategory = showFrequent;
		ShowRecentCategory = showRecent;
		_initializing = false;
	}

	/// <summary>Signals the start of the <see cref="T:System.Windows.Shell.JumpList" /> initialization.</summary>
	/// <exception cref="T:System.InvalidOperationException">This call to <see cref="M:System.Windows.Shell.JumpList.BeginInit" /> is nested in a previous call to <see cref="M:System.Windows.Shell.JumpList.BeginInit" />.</exception>
	public void BeginInit()
	{
		if (!IsUnmodified)
		{
			throw new InvalidOperationException(SR.JumpList_CantNestBeginInitCalls);
		}
		_initializing = true;
	}

	/// <summary>Signals the end of the <see cref="T:System.Windows.Shell.JumpList" /> initialization.</summary>
	/// <exception cref="T:System.NotSupportedException">This call to <see cref="M:System.Windows.Shell.JumpList.EndInit" /> is not paired with a call to <see cref="M:System.Windows.Shell.JumpList.BeginInit" />.</exception>
	public void EndInit()
	{
		if (_initializing != true)
		{
			throw new NotSupportedException(SR.JumpList_CantCallUnbalancedEndInit);
		}
		_initializing = false;
		ApplyFromApplication();
	}

	/// <summary>Sends the <see cref="T:System.Windows.Shell.JumpList" /> to the Windows shell in its current state.</summary>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Shell.JumpList" /> is not completely initialized.</exception>
	public void Apply()
	{
		if (_initializing == true)
		{
			throw new InvalidOperationException(SR.JumpList_CantApplyUntilEndInit);
		}
		_initializing = false;
		ApplyList();
	}

	private void ApplyFromApplication()
	{
		if (_initializing != true && !IsUnmodified)
		{
			_initializing = false;
		}
		if (_application == Application.Current && _initializing == false)
		{
			ApplyList();
		}
	}

	private void ApplyList()
	{
		Verify.IsApartmentState(ApartmentState.STA);
		if (!Utilities.IsOSWindows7OrNewer)
		{
			RejectEverything();
			return;
		}
		List<List<_ShellObjectPair>> list = null;
		List<_ShellObjectPair> list2 = null;
		ICustomDestinationList comObject = (ICustomDestinationList)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("77f10cf0-3db5-4966-b520-b7c54fd35ed6")));
		List<JumpItem> list3;
		List<_RejectedJumpItemPair> list4;
		try
		{
			string runtimeId = _RuntimeId;
			if (!string.IsNullOrEmpty(runtimeId))
			{
				comObject.SetAppID(runtimeId);
			}
			Guid riid = new Guid("92CA9DCD-5622-4bba-A805-5E9F541BD8C9");
			list2 = GenerateJumpItems((IObjectArray)comObject.BeginList(out var _, ref riid));
			list3 = new List<JumpItem>(JumpItems.Count);
			list4 = new List<_RejectedJumpItemPair>(JumpItems.Count);
			list = new List<List<_ShellObjectPair>>
			{
				new List<_ShellObjectPair>()
			};
			foreach (JumpItem jumpItem in JumpItems)
			{
				if (jumpItem == null)
				{
					list4.Add(new _RejectedJumpItemPair
					{
						JumpItem = jumpItem,
						Reason = JumpItemRejectionReason.InvalidItem
					});
					continue;
				}
				object comObject2 = null;
				try
				{
					comObject2 = GetShellObjectForJumpItem(jumpItem);
					if (comObject2 == null)
					{
						list4.Add(new _RejectedJumpItemPair
						{
							Reason = JumpItemRejectionReason.InvalidItem,
							JumpItem = jumpItem
						});
						continue;
					}
					if (ListContainsShellObject(list2, comObject2))
					{
						list4.Add(new _RejectedJumpItemPair
						{
							Reason = JumpItemRejectionReason.RemovedByUser,
							JumpItem = jumpItem
						});
						continue;
					}
					_ShellObjectPair item = new _ShellObjectPair
					{
						JumpItem = jumpItem,
						ShellObject = comObject2
					};
					if (string.IsNullOrEmpty(jumpItem.CustomCategory))
					{
						list[0].Add(item);
					}
					else
					{
						bool flag = false;
						foreach (List<_ShellObjectPair> item2 in list)
						{
							if (item2.Count > 0 && item2[0].JumpItem.CustomCategory == jumpItem.CustomCategory)
							{
								item2.Add(item);
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							list.Add(new List<_ShellObjectPair> { item });
						}
					}
					comObject2 = null;
				}
				finally
				{
					Utilities.SafeRelease(ref comObject2);
				}
			}
			list.Reverse();
			if (ShowFrequentCategory)
			{
				comObject.AppendKnownCategory(KDC.FREQUENT);
			}
			if (ShowRecentCategory)
			{
				comObject.AppendKnownCategory(KDC.RECENT);
			}
			foreach (List<_ShellObjectPair> item3 in list)
			{
				if (item3.Count > 0)
				{
					string customCategory = item3[0].JumpItem.CustomCategory;
					AddCategory(comObject, customCategory, item3, list3, list4);
				}
			}
			comObject.CommitList();
		}
		catch
		{
			if (TraceShell.IsEnabled)
			{
				TraceShell.Trace(TraceEventType.Error, TraceShell.RejectingJumpItemsBecauseCatastrophicFailure);
			}
			RejectEverything();
			return;
		}
		finally
		{
			Utilities.SafeRelease(ref comObject);
			if (list != null)
			{
				foreach (List<_ShellObjectPair> item4 in list)
				{
					_ShellObjectPair.ReleaseShellObjects(item4);
				}
			}
			_ShellObjectPair.ReleaseShellObjects(list2);
		}
		list3.Reverse();
		_jumpItems = list3;
		EventHandler<JumpItemsRejectedEventArgs> eventHandler = this.JumpItemsRejected;
		EventHandler<JumpItemsRemovedEventArgs> eventHandler2 = this.JumpItemsRemovedByUser;
		if (list4.Count > 0 && eventHandler != null)
		{
			List<JumpItem> list5 = new List<JumpItem>(list4.Count);
			List<JumpItemRejectionReason> list6 = new List<JumpItemRejectionReason>(list4.Count);
			foreach (_RejectedJumpItemPair item5 in list4)
			{
				list5.Add(item5.JumpItem);
				list6.Add(item5.Reason);
			}
			eventHandler(this, new JumpItemsRejectedEventArgs(list5, list6));
		}
		if (list2.Count <= 0 || eventHandler2 == null)
		{
			return;
		}
		List<JumpItem> list7 = new List<JumpItem>(list2.Count);
		foreach (_ShellObjectPair item6 in list2)
		{
			if (item6.JumpItem != null)
			{
				list7.Add(item6.JumpItem);
			}
		}
		if (list7.Count > 0)
		{
			eventHandler2(this, new JumpItemsRemovedEventArgs(list7));
		}
	}

	private static bool ListContainsShellObject(List<_ShellObjectPair> removedList, object shellObject)
	{
		if (removedList.Count == 0)
		{
			return false;
		}
		if (shellObject is IShellItem shellItem)
		{
			foreach (_ShellObjectPair removed in removedList)
			{
				if (removed.ShellObject is IShellItem psi && shellItem.Compare(psi, SICHINT.CANONICAL | SICHINT.TEST_FILESYSPATH_IF_NOT_EQUAL) == 0)
				{
					return true;
				}
			}
			return false;
		}
		if (shellObject is IShellLinkW shellLink)
		{
			foreach (_ShellObjectPair removed2 in removedList)
			{
				if (removed2.ShellObject is IShellLinkW shellLink2)
				{
					string text = ShellLinkToString(shellLink2);
					string text2 = ShellLinkToString(shellLink);
					if (text == text2)
					{
						return true;
					}
				}
			}
			return false;
		}
		return false;
	}

	private static object GetShellObjectForJumpItem(JumpItem jumpItem)
	{
		JumpPath jumpPath = jumpItem as JumpPath;
		JumpTask jumpTask = jumpItem as JumpTask;
		if (jumpPath != null)
		{
			return CreateItemFromJumpPath(jumpPath);
		}
		if (jumpTask != null)
		{
			return CreateLinkFromJumpTask(jumpTask, allowSeparators: true);
		}
		return null;
	}

	private static List<_ShellObjectPair> GenerateJumpItems(IObjectArray shellObjects)
	{
		List<_ShellObjectPair> list = new List<_ShellObjectPair>();
		Guid riid = new Guid("00000000-0000-0000-C000-000000000046");
		uint count = shellObjects.GetCount();
		for (uint num = 0u; num < count; num++)
		{
			object at = shellObjects.GetAt(num, ref riid);
			JumpItem jumpItem = null;
			try
			{
				jumpItem = GetJumpItemForShellObject(at);
			}
			catch (Exception ex)
			{
				if (ex is NullReferenceException || ex is SEHException)
				{
					throw;
				}
			}
			list.Add(new _ShellObjectPair
			{
				ShellObject = at,
				JumpItem = jumpItem
			});
		}
		return list;
	}

	private static void AddCategory(ICustomDestinationList cdl, string category, List<_ShellObjectPair> jumpItems, List<JumpItem> successList, List<_RejectedJumpItemPair> rejectionList)
	{
		AddCategory(cdl, category, jumpItems, successList, rejectionList, isHeterogenous: true);
	}

	private static void AddCategory(ICustomDestinationList cdl, string category, List<_ShellObjectPair> jumpItems, List<JumpItem> successList, List<_RejectedJumpItemPair> rejectionList, bool isHeterogenous)
	{
		IObjectCollection comObject = (IObjectCollection)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("2d3468c1-36a7-43b6-ac24-d3f02fd9607a")));
		foreach (_ShellObjectPair jumpItem in jumpItems)
		{
			comObject.AddObject(jumpItem.ShellObject);
		}
		MS.Internal.Interop.HRESULT hRESULT = ((!string.IsNullOrEmpty(category)) ? cdl.AppendCategory(category, comObject) : cdl.AddUserTasks(comObject));
		if (hRESULT.Succeeded)
		{
			int num = jumpItems.Count;
			while (--num >= 0)
			{
				successList.Add(jumpItems[num].JumpItem);
			}
			return;
		}
		if (isHeterogenous && hRESULT == MS.Internal.Interop.HRESULT.DESTS_E_NO_MATCHING_ASSOC_HANDLER)
		{
			if (TraceShell.IsEnabled)
			{
				TraceShell.Trace(TraceEventType.Error, TraceShell.RejectingJumpListCategoryBecauseNoRegisteredHandler(category));
			}
			Utilities.SafeRelease(ref comObject);
			List<_ShellObjectPair> list = new List<_ShellObjectPair>();
			foreach (_ShellObjectPair jumpItem2 in jumpItems)
			{
				if (jumpItem2.JumpItem is JumpPath)
				{
					rejectionList.Add(new _RejectedJumpItemPair
					{
						JumpItem = jumpItem2.JumpItem,
						Reason = JumpItemRejectionReason.NoRegisteredHandler
					});
				}
				else
				{
					list.Add(jumpItem2);
				}
			}
			if (list.Count > 0)
			{
				AddCategory(cdl, category, list, successList, rejectionList, isHeterogenous: false);
			}
			return;
		}
		foreach (_ShellObjectPair jumpItem3 in jumpItems)
		{
			rejectionList.Add(new _RejectedJumpItemPair
			{
				JumpItem = jumpItem3.JumpItem,
				Reason = JumpItemRejectionReason.InvalidItem
			});
		}
	}

	private static IShellLinkW CreateLinkFromJumpTask(JumpTask jumpTask, bool allowSeparators)
	{
		if (string.IsNullOrEmpty(jumpTask.Title) && (!allowSeparators || !string.IsNullOrEmpty(jumpTask.CustomCategory)))
		{
			return null;
		}
		IShellLinkW comObject = (IShellLinkW)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("00021401-0000-0000-C000-000000000046")));
		try
		{
			string path = _FullName;
			if (!string.IsNullOrEmpty(jumpTask.ApplicationPath))
			{
				path = jumpTask.ApplicationPath;
			}
			comObject.SetPath(path);
			if (!string.IsNullOrEmpty(jumpTask.WorkingDirectory))
			{
				comObject.SetWorkingDirectory(jumpTask.WorkingDirectory);
			}
			if (!string.IsNullOrEmpty(jumpTask.Arguments))
			{
				comObject.SetArguments(jumpTask.Arguments);
			}
			if (jumpTask.IconResourceIndex != -1)
			{
				string pszIconPath = _FullName;
				if (!string.IsNullOrEmpty(jumpTask.IconResourcePath))
				{
					if (jumpTask.IconResourcePath.Length >= 260)
					{
						return null;
					}
					pszIconPath = jumpTask.IconResourcePath;
				}
				comObject.SetIconLocation(pszIconPath, jumpTask.IconResourceIndex);
			}
			if (!string.IsNullOrEmpty(jumpTask.Description))
			{
				comObject.SetDescription(jumpTask.Description);
			}
			IPropertyStore propertyStore = (IPropertyStore)comObject;
			PROPVARIANT disposable = new PROPVARIANT();
			try
			{
				PKEY pKEY = default(PKEY);
				if (!string.IsNullOrEmpty(jumpTask.Title))
				{
					disposable.SetValue(jumpTask.Title);
					pKEY = PKEY.Title;
				}
				else
				{
					disposable.SetValue(f: true);
					pKEY = PKEY.AppUserModel_IsDestListSeparator;
				}
				propertyStore.SetValue(ref pKEY, disposable);
			}
			finally
			{
				Utilities.SafeDispose(ref disposable);
			}
			propertyStore.Commit();
			IShellLinkW result = comObject;
			comObject = null;
			return result;
		}
		catch (Exception)
		{
			return null;
		}
		finally
		{
			Utilities.SafeRelease(ref comObject);
		}
	}

	private static IShellItem2 CreateItemFromJumpPath(JumpPath jumpPath)
	{
		try
		{
			return ShellUtil.GetShellItemForPath(Path.GetFullPath(jumpPath.Path));
		}
		catch (Exception)
		{
		}
		return null;
	}

	private static JumpItem GetJumpItemForShellObject(object shellObject)
	{
		IShellItem2 shellItem = shellObject as IShellItem2;
		IShellLinkW shellLinkW = shellObject as IShellLinkW;
		if (shellItem != null)
		{
			return new JumpPath
			{
				Path = shellItem.GetDisplayName(SIGDN.DESKTOPABSOLUTEPARSING)
			};
		}
		if (shellLinkW != null)
		{
			StringBuilder stringBuilder = new StringBuilder(260);
			shellLinkW.GetPath(stringBuilder, stringBuilder.Capacity, null, SLGP.RAWPATH);
			StringBuilder stringBuilder2 = new StringBuilder(1024);
			shellLinkW.GetArguments(stringBuilder2, stringBuilder2.Capacity);
			StringBuilder stringBuilder3 = new StringBuilder(1024);
			shellLinkW.GetDescription(stringBuilder3, stringBuilder3.Capacity);
			StringBuilder stringBuilder4 = new StringBuilder(260);
			shellLinkW.GetIconLocation(stringBuilder4, stringBuilder4.Capacity, out var piIcon);
			StringBuilder stringBuilder5 = new StringBuilder(260);
			shellLinkW.GetWorkingDirectory(stringBuilder5, stringBuilder5.Capacity);
			JumpTask jumpTask = new JumpTask
			{
				ApplicationPath = stringBuilder.ToString(),
				Arguments = stringBuilder2.ToString(),
				Description = stringBuilder3.ToString(),
				IconResourceIndex = piIcon,
				IconResourcePath = stringBuilder4.ToString(),
				WorkingDirectory = stringBuilder5.ToString()
			};
			using PROPVARIANT pROPVARIANT = new PROPVARIANT();
			IPropertyStore obj = (IPropertyStore)shellLinkW;
			PKEY pkey = PKEY.Title;
			obj.GetValue(ref pkey, pROPVARIANT);
			jumpTask.Title = pROPVARIANT.GetValue() ?? "";
			return jumpTask;
		}
		return null;
	}

	private static string ShellLinkToString(IShellLinkW shellLink)
	{
		StringBuilder stringBuilder = new StringBuilder(260);
		shellLink.GetPath(stringBuilder, stringBuilder.Capacity, null, SLGP.RAWPATH);
		string text = null;
		using (PROPVARIANT pROPVARIANT = new PROPVARIANT())
		{
			IPropertyStore obj = (IPropertyStore)shellLink;
			PKEY pkey = PKEY.Title;
			obj.GetValue(ref pkey, pROPVARIANT);
			text = pROPVARIANT.GetValue() ?? "";
		}
		StringBuilder stringBuilder2 = new StringBuilder(1024);
		shellLink.GetArguments(stringBuilder2, stringBuilder2.Capacity);
		return stringBuilder.ToString().ToUpperInvariant() + text.ToUpperInvariant() + stringBuilder2.ToString();
	}

	private void RejectEverything()
	{
		EventHandler<JumpItemsRejectedEventArgs> eventHandler = this.JumpItemsRejected;
		if (eventHandler == null)
		{
			_jumpItems.Clear();
		}
		else if (_jumpItems.Count > 0)
		{
			List<JumpItemRejectionReason> list = new List<JumpItemRejectionReason>(_jumpItems.Count);
			for (int i = 0; i < _jumpItems.Count; i++)
			{
				list.Add(JumpItemRejectionReason.InvalidItem);
			}
			JumpItemsRejectedEventArgs e = new JumpItemsRejectedEventArgs(_jumpItems, list);
			_jumpItems.Clear();
			eventHandler(this, e);
		}
	}
}
