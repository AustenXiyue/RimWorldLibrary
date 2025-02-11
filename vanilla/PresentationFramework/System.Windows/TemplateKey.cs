using System.ComponentModel;
using System.Reflection;
using System.Windows.Markup;

namespace System.Windows;

/// <summary>When used as a resource key for a data template, allows the data template to participate in the lookup process.</summary>
[TypeConverter(typeof(TemplateKeyConverter))]
public abstract class TemplateKey : ResourceKey, ISupportInitialize
{
	/// <summary>Describes the different types of templates that use <see cref="T:System.Windows.TemplateKey" />.</summary>
	protected enum TemplateType
	{
		/// <summary>A type that is a <see cref="T:System.Windows.DataTemplate" />.</summary>
		DataTemplate,
		/// <summary>A type that is a TableTemplate. This is obsolete.</summary>
		TableTemplate
	}

	private object _dataType;

	private TemplateType _templateType;

	private bool _initializing;

	/// <summary>Gets or sets the type for which the template is designed. </summary>
	/// <returns>The <see cref="T:System.Type" /> that specifies the type of object that the template is used to display, or a string that specifies the XML tag name for the XML data that the template is used to display.</returns>
	public object DataType
	{
		get
		{
			return _dataType;
		}
		set
		{
			if (!_initializing)
			{
				throw new InvalidOperationException(SR.Format(SR.PropertyIsInitializeOnly, "DataType", GetType().Name));
			}
			if (_dataType != null && value != _dataType)
			{
				throw new InvalidOperationException(SR.Format(SR.PropertyIsImmutable, "DataType", GetType().Name));
			}
			Exception ex = ValidateDataType(value, "value");
			if (ex != null)
			{
				throw ex;
			}
			_dataType = value;
		}
	}

	/// <summary>Gets or sets the assembly that contains the template definition.</summary>
	/// <returns>The assembly in which the template is defined.</returns>
	public override Assembly Assembly
	{
		get
		{
			Type type = _dataType as Type;
			if (type != null)
			{
				return type.Assembly;
			}
			return null;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.TemplateKey" /> class with the specified template type. This constructor is protected.</summary>
	/// <param name="templateType">A <see cref="T:System.Windows.TemplateKey.TemplateType" /> value that specifies the type of this template.</param>
	protected TemplateKey(TemplateType templateType)
	{
		_dataType = null;
		_templateType = templateType;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.TemplateKey" /> class with the specified parameters. This constructor is protected.</summary>
	/// <param name="templateType">A <see cref="T:System.Windows.TemplateKey.TemplateType" /> value that specifies the type of this template.</param>
	/// <param name="dataType">The type for which this template is designed.</param>
	protected TemplateKey(TemplateType templateType, object dataType)
	{
		Exception ex = ValidateDataType(dataType, "dataType");
		if (ex != null)
		{
			throw ex;
		}
		_dataType = dataType;
		_templateType = templateType;
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void ISupportInitialize.BeginInit()
	{
		_initializing = true;
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void ISupportInitialize.EndInit()
	{
		if (_dataType == null)
		{
			throw new InvalidOperationException(SR.Format(SR.PropertyMustHaveValue, "DataType", GetType().Name));
		}
		_initializing = false;
	}

	/// <summary>Returns the hash code for this instance of <see cref="T:System.Windows.TemplateKey" />.</summary>
	/// <returns>The hash code for this instance of <see cref="T:System.Windows.TemplateKey" />.</returns>
	public override int GetHashCode()
	{
		int num = (int)_templateType;
		if (_dataType != null)
		{
			num += _dataType.GetHashCode();
		}
		return num;
	}

	/// <summary>Returns a value that indicates whether the given instance is identical to this instance of <see cref="T:System.Windows.TemplateKey" />.</summary>
	/// <returns>true if the two instances are identical; otherwise, false.</returns>
	/// <param name="o">The object to compare for equality.</param>
	public override bool Equals(object o)
	{
		if (o is TemplateKey templateKey)
		{
			if (_templateType == templateKey._templateType)
			{
				return object.Equals(_dataType, templateKey._dataType);
			}
			return false;
		}
		return false;
	}

	/// <summary>Returns a string representation of this <see cref="T:System.Windows.TemplateKey" />.</summary>
	/// <returns>A string representation of this <see cref="T:System.Windows.TemplateKey" />.</returns>
	public override string ToString()
	{
		_ = DataType;
		if (DataType == null)
		{
			return string.Format(TypeConverterHelper.InvariantEnglishUS, "{0}(null)", GetType().Name);
		}
		return string.Format(TypeConverterHelper.InvariantEnglishUS, "{0}({1})", GetType().Name, DataType);
	}

	internal static Exception ValidateDataType(object dataType, string argName)
	{
		Exception result = null;
		if (dataType == null)
		{
			result = new ArgumentNullException(argName);
		}
		else if (!(dataType is Type) && !(dataType is string))
		{
			result = new ArgumentException(SR.Format(SR.MustBeTypeOrString, dataType.GetType().Name), argName);
		}
		else if (typeof(object).Equals(dataType))
		{
			result = new ArgumentException(SR.DataTypeCannotBeObject, argName);
		}
		return result;
	}
}
