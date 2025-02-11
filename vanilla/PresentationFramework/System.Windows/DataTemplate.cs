using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Markup;

namespace System.Windows;

/// <summary>Describes the visual structure of a data object.</summary>
[DictionaryKeyProperty("DataTemplateKey")]
public class DataTemplate : FrameworkTemplate
{
	private object _dataType;

	private TriggerCollection _triggers;

	/// <summary>Gets or sets the type for which this <see cref="T:System.Windows.DataTemplate" /> is intended. </summary>
	/// <returns>The default value is null.</returns>
	[DefaultValue(null)]
	[Ambient]
	public object DataType
	{
		get
		{
			return _dataType;
		}
		set
		{
			Exception ex = TemplateKey.ValidateDataType(value, "value");
			if (ex != null)
			{
				throw ex;
			}
			CheckSealed();
			_dataType = value;
		}
	}

	/// <summary>Gets a collection of triggers that apply property values or perform actions based on one or more conditions.</summary>
	/// <returns>A collection of trigger objects. The default value is null.</returns>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[DependsOn("VisualTree")]
	[DependsOn("Template")]
	public TriggerCollection Triggers
	{
		get
		{
			if (_triggers == null)
			{
				_triggers = new TriggerCollection();
				if (base.IsSealed)
				{
					_triggers.Seal();
				}
			}
			return _triggers;
		}
	}

	/// <summary>Gets the default key of the <see cref="T:System.Windows.DataTemplate" />. </summary>
	/// <returns>The default key of the <see cref="T:System.Windows.DataTemplate" />.</returns>
	public object DataTemplateKey
	{
		get
		{
			if (DataType == null)
			{
				return null;
			}
			return new DataTemplateKey(DataType);
		}
	}

	internal override Type TargetTypeInternal => DefaultTargetType;

	internal override object DataTypeInternal => DataType;

	internal override TriggerCollection TriggersInternal => Triggers;

	internal static Type DefaultTargetType => typeof(ContentPresenter);

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.DataTemplate" /> class.</summary>
	public DataTemplate()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.DataTemplate" /> class with the specified <see cref="P:System.Windows.DataTemplate.DataType" /> property.</summary>
	/// <param name="dataType">If the template is intended for object data, this is the Type name of the data object. </param>
	public DataTemplate(object dataType)
	{
		Exception ex = TemplateKey.ValidateDataType(dataType, "dataType");
		if (ex != null)
		{
			throw ex;
		}
		_dataType = dataType;
	}

	internal override void SetTargetTypeInternal(Type targetType)
	{
		throw new InvalidOperationException(SR.TemplateNotTargetType);
	}

	/// <summary>Checks the templated parent against a set of rules.</summary>
	/// <param name="templatedParent">The element this template is applied to.</param>
	protected override void ValidateTemplatedParent(FrameworkElement templatedParent)
	{
		if (templatedParent == null)
		{
			throw new ArgumentNullException("templatedParent");
		}
		if (!(templatedParent is ContentPresenter))
		{
			throw new ArgumentException(SR.Format(SR.TemplateTargetTypeMismatch, "ContentPresenter", templatedParent.GetType().Name));
		}
	}
}
