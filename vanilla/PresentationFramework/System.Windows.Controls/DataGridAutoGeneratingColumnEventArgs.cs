using System.ComponentModel;

namespace System.Windows.Controls;

/// <summary>Provides data for the <see cref="E:System.Windows.Controls.DataGrid.AutoGeneratingColumn" /> event.</summary>
public class DataGridAutoGeneratingColumnEventArgs : EventArgs
{
	private DataGridColumn _column;

	private string _propertyName;

	private Type _propertyType;

	private object _propertyDescriptor;

	private bool _cancel;

	/// <summary>Gets or sets the generated column.</summary>
	/// <returns>The generated column.</returns>
	public DataGridColumn Column
	{
		get
		{
			return _column;
		}
		set
		{
			_column = value;
		}
	}

	/// <summary>Gets the name of the property bound to the generated column.</summary>
	/// <returns>The name of the property bound to the generated column.</returns>
	public string PropertyName => _propertyName;

	/// <summary>Gets the type of the property bound to the generated column.</summary>
	/// <returns>The type of the property bound to the generated column.</returns>
	public Type PropertyType => _propertyType;

	/// <summary>Gets the descriptor of the property bound to the generated column.</summary>
	/// <returns>An object that contains metadata for the property.</returns>
	public object PropertyDescriptor
	{
		get
		{
			return _propertyDescriptor;
		}
		private set
		{
			if (value == null)
			{
				_propertyDescriptor = null;
			}
			else
			{
				_propertyDescriptor = value;
			}
		}
	}

	/// <summary>Gets or sets a value that indicates whether the event should be canceled.</summary>
	/// <returns>true if the event should be canceled; otherwise, false. The default is false.</returns>
	public bool Cancel
	{
		get
		{
			return _cancel;
		}
		set
		{
			_cancel = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Controls.DataGridAutoGeneratingColumnEventArgs" /> class. </summary>
	/// <param name="propertyName">The name of the property bound to the generated column.</param>
	/// <param name="propertyType">The type of the property bound to the generated column.</param>
	/// <param name="column">The generated column.</param>
	public DataGridAutoGeneratingColumnEventArgs(string propertyName, Type propertyType, DataGridColumn column)
		: this(column, propertyName, propertyType, null)
	{
	}

	internal DataGridAutoGeneratingColumnEventArgs(DataGridColumn column, ItemPropertyInfo itemPropertyInfo)
		: this(column, itemPropertyInfo.Name, itemPropertyInfo.PropertyType, itemPropertyInfo.Descriptor)
	{
	}

	internal DataGridAutoGeneratingColumnEventArgs(DataGridColumn column, string propertyName, Type propertyType, object propertyDescriptor)
	{
		_column = column;
		_propertyName = propertyName;
		_propertyType = propertyType;
		PropertyDescriptor = propertyDescriptor;
	}
}
