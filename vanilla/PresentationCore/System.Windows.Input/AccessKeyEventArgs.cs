using MS.Internal;

namespace System.Windows.Input;

/// <summary>Provides information for access keys events. </summary>
public class AccessKeyEventArgs : EventArgs
{
	private string _key;

	private bool _isMultiple;

	private MS.Internal.SecurityCriticalDataForSet<bool> _userInitiated;

	/// <summary>Gets the access keys that was pressed. </summary>
	/// <returns>The access key.</returns>
	public string Key => _key;

	/// <summary>Gets a value that indicates whether other elements are invoked by the key. </summary>
	/// <returns>true if other elements are invoked; otherwise, false.</returns>
	public bool IsMultiple => _isMultiple;

	internal bool UserInitiated => _userInitiated.Value;

	internal AccessKeyEventArgs(string key, bool isMultiple, bool userInitiated)
	{
		_key = key;
		_isMultiple = isMultiple;
		_userInitiated = new MS.Internal.SecurityCriticalDataForSet<bool>(userInitiated);
	}

	internal void ClearUserInitiated()
	{
		_userInitiated.Value = false;
	}
}
