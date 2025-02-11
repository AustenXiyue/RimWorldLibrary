using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;

namespace MS.Internal.ComponentModel;

internal class AttachInfo
{
	private readonly DependencyProperty _dp;

	private MethodInfo _getMethod;

	private AttachedPropertyBrowsableAttribute[] _attributes;

	private AttachedPropertyBrowsableAttribute _paramTypeAttribute;

	private bool _attributesChecked;

	private bool _getMethodChecked;

	private bool _paramTypeAttributeChecked;

	private MethodInfo AttachMethod
	{
		get
		{
			if (!_getMethodChecked)
			{
				_getMethod = DependencyObjectPropertyDescriptor.GetAttachedPropertyMethod(_dp);
				_getMethodChecked = true;
			}
			return _getMethod;
		}
	}

	private AttachedPropertyBrowsableAttribute[] Attributes
	{
		get
		{
			if (!_attributesChecked)
			{
				MethodInfo attachMethod = AttachMethod;
				object[] array = null;
				if (attachMethod != null)
				{
					AttachedPropertyBrowsableAttribute[] array2 = null;
					array = attachMethod.GetCustomAttributes(DependencyObjectPropertyDescriptor.AttachedPropertyBrowsableAttributeType, inherit: false);
					bool flag = false;
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i] is AttachedPropertyBrowsableForTypeAttribute)
						{
							flag = true;
							break;
						}
					}
					if (!flag && array.Length != 0)
					{
						array2 = new AttachedPropertyBrowsableAttribute[array.Length + 1];
						for (int j = 0; j < array.Length; j++)
						{
							array2[j] = (AttachedPropertyBrowsableAttribute)array[j];
						}
						array2[array.Length] = ParameterTypeAttribute;
					}
					else
					{
						array2 = new AttachedPropertyBrowsableAttribute[array.Length];
						for (int k = 0; k < array.Length; k++)
						{
							array2[k] = (AttachedPropertyBrowsableAttribute)array[k];
						}
					}
					_attributes = array2;
				}
				_attributesChecked = true;
			}
			return _attributes;
		}
	}

	private AttachedPropertyBrowsableAttribute ParameterTypeAttribute
	{
		get
		{
			if (!_paramTypeAttributeChecked)
			{
				MethodInfo attachMethod = AttachMethod;
				if (attachMethod != null)
				{
					ParameterInfo[] parameters = attachMethod.GetParameters();
					TypeDescriptionProvider provider = TypeDescriptor.GetProvider(_dp.OwnerType);
					_paramTypeAttribute = new AttachedPropertyBrowsableForTypeAttribute(provider.GetRuntimeType(parameters[0].ParameterType));
				}
				_paramTypeAttributeChecked = true;
			}
			return _paramTypeAttribute;
		}
	}

	internal AttachInfo(DependencyProperty dp)
	{
		_dp = dp;
	}

	internal bool CanAttach(DependencyObject instance)
	{
		if (AttachMethod != null)
		{
			int num = 0;
			AttachedPropertyBrowsableAttribute[] attributes = Attributes;
			if (attributes != null)
			{
				num = attributes.Length;
				for (int i = 0; i < num; i++)
				{
					AttachedPropertyBrowsableAttribute attachedPropertyBrowsableAttribute = attributes[i];
					if (attachedPropertyBrowsableAttribute.IsBrowsable(instance, _dp))
					{
						continue;
					}
					bool flag = false;
					if (attachedPropertyBrowsableAttribute.UnionResults)
					{
						Type type = attachedPropertyBrowsableAttribute.GetType();
						for (int j = 0; j < num; j++)
						{
							AttachedPropertyBrowsableAttribute attachedPropertyBrowsableAttribute2 = attributes[j];
							if (type == attachedPropertyBrowsableAttribute2.GetType() && attachedPropertyBrowsableAttribute2.IsBrowsable(instance, _dp))
							{
								flag = true;
								break;
							}
						}
					}
					if (!flag)
					{
						return false;
					}
				}
			}
			return num > 0;
		}
		return false;
	}
}
