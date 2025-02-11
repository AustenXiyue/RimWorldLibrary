using System.ComponentModel;

namespace MS.Internal.Data;

internal class ViewRecord
{
	private ICollectionView _view;

	private int _version;

	private bool _isInitialized;

	internal ICollectionView View => _view;

	internal int Version
	{
		get
		{
			return _version;
		}
		set
		{
			_version = value;
		}
	}

	internal bool IsInitialized => _isInitialized;

	internal ViewRecord(ICollectionView view)
	{
		_view = view;
		_version = -1;
	}

	internal void InitializeView()
	{
		_view.MoveCurrentToFirst();
		_isInitialized = true;
	}
}
