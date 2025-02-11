using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Diagnostics;
using System.Windows.Markup;

namespace MS.Internal;

internal static class TraceData
{
	private static AvTrace _avTrace;

	private static AvTraceDetails _CannotCreateDefaultValueConverter;

	private static AvTraceDetails _NoMentor;

	private static AvTraceDetails _NoDataContext;

	private static AvTraceDetails _NoSource;

	private static AvTraceDetails _BadValueAtTransfer;

	private static AvTraceDetails _BadConverterForTransfer;

	private static AvTraceDetails _BadConverterForUpdate;

	private static AvTraceDetails _WorkerUpdateFailed;

	private static AvTraceDetails _RequiresExplicitCulture;

	private static AvTraceDetails _NoValueToTransfer;

	private static AvTraceDetails _FallbackConversionFailed;

	private static AvTraceDetails _TargetNullValueConversionFailed;

	private static AvTraceDetails _BindingGroupNameMatchFailed;

	private static AvTraceDetails _BindingGroupWrongProperty;

	private static AvTraceDetails _BindingGroupMultipleInheritance;

	private static AvTraceDetails _SharesProposedValuesRequriesImplicitBindingGroup;

	private static AvTraceDetails _CannotGetClrRawValue;

	private static AvTraceDetails _CannotSetClrRawValue;

	private static AvTraceDetails _MissingDataItem;

	private static AvTraceDetails _MissingInfo;

	private static AvTraceDetails _NullDataItem;

	private static AvTraceDetails _DefaultValueConverterFailed;

	private static AvTraceDetails _DefaultValueConverterFailedForCulture;

	private static AvTraceDetails _StyleAndStyleSelectorDefined;

	private static AvTraceDetails _TemplateAndTemplateSelectorDefined;

	private static AvTraceDetails _ItemTemplateForDirectItem;

	private static AvTraceDetails _BadMultiConverterForUpdate;

	private static AvTraceDetails _MultiValueConverterMissingForTransfer;

	private static AvTraceDetails _MultiValueConverterMissingForUpdate;

	private static AvTraceDetails _MultiValueConverterMismatch;

	private static AvTraceDetails _MultiBindingHasNoConverter;

	private static AvTraceDetails _UnsetValueInMultiBindingExpressionUpdate;

	private static AvTraceDetails _ObjectDataProviderHasNoSource;

	private static AvTraceDetails _ObjDPCreateFailed;

	private static AvTraceDetails _ObjDPInvokeFailed;

	private static AvTraceDetails _RefPreviousNotInContext;

	private static AvTraceDetails _RefNoWrapperInChildren;

	private static AvTraceDetails _RefAncestorTypeNotSpecified;

	private static AvTraceDetails _RefAncestorLevelInvalid;

	private static AvTraceDetails _ClrReplaceItem;

	private static AvTraceDetails _NullItem;

	private static AvTraceDetails _PlaceholderItem;

	private static AvTraceDetails _DataErrorInfoFailed;

	private static AvTraceDetails _DisallowTwoWay;

	private static AvTraceDetails _XmlBindingToNonXml;

	private static AvTraceDetails _XmlBindingToNonXmlCollection;

	private static AvTraceDetails _CannotGetXmlNodeCollection;

	private static AvTraceDetails _BadXPath;

	private static AvTraceDetails _XmlDPInlineDocError;

	private static AvTraceDetails _XmlNamespaceNotSet;

	private static AvTraceDetails _XmlDPAsyncDocError;

	private static AvTraceDetails _XmlDPSelectNodesFailed;

	private static AvTraceDetails _CollectionViewIsUnsupported;

	private static AvTraceDetails _CollectionChangedWithoutNotification;

	private static AvTraceDetails _CannotSort;

	private static AvTraceDetails _CreatedExpression;

	private static AvTraceDetails _CreatedExpressionInParent;

	private static AvTraceDetails _BindingPath;

	private static AvTraceDetails _BindingXPathAndPath;

	private static AvTraceDetails _ResolveDefaultMode;

	private static AvTraceDetails _ResolveDefaultUpdate;

	private static AvTraceDetails _AttachExpression;

	private static AvTraceDetails _DetachExpression;

	private static AvTraceDetails _UseMentor;

	private static AvTraceDetails _DeferAttachToContext;

	private static AvTraceDetails _SourceRequiresTreeContext;

	private static AvTraceDetails _AttachToContext;

	private static AvTraceDetails _PathRequiresTreeContext;

	private static AvTraceDetails _NoMentorExtended;

	private static AvTraceDetails _ContextElement;

	private static AvTraceDetails _NullDataContext;

	private static AvTraceDetails _RelativeSource;

	private static AvTraceDetails _AncestorLookup;

	private static AvTraceDetails _ElementNameQuery;

	private static AvTraceDetails _ElementNameQueryTemplate;

	private static AvTraceDetails _UseCVS;

	private static AvTraceDetails _UseDataProvider;

	private static AvTraceDetails _ActivateItem;

	private static AvTraceDetails _Deactivate;

	private static AvTraceDetails _GetRawValue;

	private static AvTraceDetails _ConvertDBNull;

	private static AvTraceDetails _UserConverter;

	private static AvTraceDetails _NullConverter;

	private static AvTraceDetails _DefaultConverter;

	private static AvTraceDetails _FormattedValue;

	private static AvTraceDetails _FormattingFailed;

	private static AvTraceDetails _BadValueAtTransferExtended;

	private static AvTraceDetails _UseFallback;

	private static AvTraceDetails _TransferValue;

	private static AvTraceDetails _UpdateRawValue;

	private static AvTraceDetails _ValidationRuleFailed;

	private static AvTraceDetails _UserConvertBack;

	private static AvTraceDetails _DefaultConvertBack;

	private static AvTraceDetails _Update;

	private static AvTraceDetails _GotEvent;

	private static AvTraceDetails _GotPropertyChanged;

	private static AvTraceDetails _PriorityTransfer;

	private static AvTraceDetails _ChildNotAttached;

	private static AvTraceDetails _GetRawValueMulti;

	private static AvTraceDetails _UserConvertBackMulti;

	private static AvTraceDetails _GetValue;

	private static AvTraceDetails _SetValue;

	private static AvTraceDetails _ReplaceItemShort;

	private static AvTraceDetails _ReplaceItemLong;

	private static AvTraceDetails _GetInfo_Reuse;

	private static AvTraceDetails _GetInfo_Null;

	private static AvTraceDetails _GetInfo_Cache;

	private static AvTraceDetails _GetInfo_Property;

	private static AvTraceDetails _GetInfo_Indexer;

	private static AvTraceDetails _XmlContextNode;

	private static AvTraceDetails _XmlNewCollection;

	private static AvTraceDetails _XmlSynchronizeCollection;

	private static AvTraceDetails _SelectNodes;

	private static AvTraceDetails _BeginQuery;

	private static AvTraceDetails _QueryFinished;

	private static AvTraceDetails _QueryResult;

	private static AvTraceDetails _XmlLoadSource;

	private static AvTraceDetails _XmlLoadDoc;

	private static AvTraceDetails _XmlLoadInline;

	private static AvTraceDetails _XmlBuildCollection;

	public static AvTraceDetails NoMentor
	{
		get
		{
			if (_NoMentor == null)
			{
				_NoMentor = new AvTraceDetails(2, new string[1] { "Cannot find governing FrameworkElement or FrameworkContentElement for target element." });
			}
			return _NoMentor;
		}
	}

	public static AvTraceDetails NoDataContext
	{
		get
		{
			if (_NoDataContext == null)
			{
				_NoDataContext = new AvTraceDetails(3, new string[1] { "UpdateCannot find element that provides DataContext." });
			}
			return _NoDataContext;
		}
	}

	public static AvTraceDetails BadValueAtTransfer
	{
		get
		{
			if (_BadValueAtTransfer == null)
			{
				_BadValueAtTransfer = new AvTraceDetails(5, new string[1] { "Value produced by BindingExpression is not valid for target property." });
			}
			return _BadValueAtTransfer;
		}
	}

	public static AvTraceDetails WorkerUpdateFailed
	{
		get
		{
			if (_WorkerUpdateFailed == null)
			{
				_WorkerUpdateFailed = new AvTraceDetails(8, new string[1] { "Cannot save value from target back to source." });
			}
			return _WorkerUpdateFailed;
		}
	}

	public static AvTraceDetails RequiresExplicitCulture
	{
		get
		{
			if (_RequiresExplicitCulture == null)
			{
				_RequiresExplicitCulture = new AvTraceDetails(9, new string[1] { "Binding for property cannot use the target element's Language for conversion; if a culture is required, ConverterCulture must be explicitly specified on the Binding." });
			}
			return _RequiresExplicitCulture;
		}
	}

	public static AvTraceDetails NoValueToTransfer
	{
		get
		{
			if (_NoValueToTransfer == null)
			{
				_NoValueToTransfer = new AvTraceDetails(10, new string[1] { "Cannot retrieve value using the binding and no valid fallback value exists; using default instead." });
			}
			return _NoValueToTransfer;
		}
	}

	public static AvTraceDetails BindingGroupMultipleInheritance
	{
		get
		{
			if (_BindingGroupMultipleInheritance == null)
			{
				_BindingGroupMultipleInheritance = new AvTraceDetails(15, new string[1] { "BindingGroup used as a value of multiple properties.  This disables its normal behavior." });
			}
			return _BindingGroupMultipleInheritance;
		}
	}

	public static AvTraceDetails MissingDataItem
	{
		get
		{
			if (_MissingDataItem == null)
			{
				_MissingDataItem = new AvTraceDetails(19, new string[1] { "BindingExpression has no source data item. This could happen when currency is moved to a null data item or moved off the list." });
			}
			return _MissingDataItem;
		}
	}

	public static AvTraceDetails MissingInfo
	{
		get
		{
			if (_MissingInfo == null)
			{
				_MissingInfo = new AvTraceDetails(20, new string[1] { "BindingExpression cannot retrieve value due to missing information." });
			}
			return _MissingInfo;
		}
	}

	public static AvTraceDetails NullDataItem
	{
		get
		{
			if (_NullDataItem == null)
			{
				_NullDataItem = new AvTraceDetails(21, new string[1] { "BindingExpression cannot retrieve value from null data item. This could happen when binding is detached or when binding to a Nullable type that has no value." });
			}
			return _NullDataItem;
		}
	}

	public static AvTraceDetails ItemTemplateForDirectItem
	{
		get
		{
			if (_ItemTemplateForDirectItem == null)
			{
				_ItemTemplateForDirectItem = new AvTraceDetails(26, new string[1] { "ItemTemplate and ItemTemplateSelector are ignored for items already of the ItemsControl's container type" });
			}
			return _ItemTemplateForDirectItem;
		}
	}

	public static AvTraceDetails MultiValueConverterMissingForTransfer
	{
		get
		{
			if (_MultiValueConverterMissingForTransfer == null)
			{
				_MultiValueConverterMissingForTransfer = new AvTraceDetails(28, new string[1] { "MultiBinding failed because it has no valid Converter." });
			}
			return _MultiValueConverterMissingForTransfer;
		}
	}

	public static AvTraceDetails MultiValueConverterMissingForUpdate
	{
		get
		{
			if (_MultiValueConverterMissingForUpdate == null)
			{
				_MultiValueConverterMissingForUpdate = new AvTraceDetails(29, new string[1] { "MultiBinding cannot update value on source item because there is no valid Converter." });
			}
			return _MultiValueConverterMissingForUpdate;
		}
	}

	public static AvTraceDetails MultiValueConverterMismatch
	{
		get
		{
			if (_MultiValueConverterMismatch == null)
			{
				_MultiValueConverterMismatch = new AvTraceDetails(30, new string[1] { "MultiValueConverter did not return the same number of values as the count of inner bindings." });
			}
			return _MultiValueConverterMismatch;
		}
	}

	public static AvTraceDetails MultiBindingHasNoConverter
	{
		get
		{
			if (_MultiBindingHasNoConverter == null)
			{
				_MultiBindingHasNoConverter = new AvTraceDetails(31, new string[1] { "Cannot set MultiBinding because MultiValueConverter must be specified." });
			}
			return _MultiBindingHasNoConverter;
		}
	}

	public static AvTraceDetails ObjectDataProviderHasNoSource
	{
		get
		{
			if (_ObjectDataProviderHasNoSource == null)
			{
				_ObjectDataProviderHasNoSource = new AvTraceDetails(33, new string[1] { "ObjectDataProvider needs either an ObjectType or ObjectInstance." });
			}
			return _ObjectDataProviderHasNoSource;
		}
	}

	public static AvTraceDetails ObjDPCreateFailed
	{
		get
		{
			if (_ObjDPCreateFailed == null)
			{
				_ObjDPCreateFailed = new AvTraceDetails(34, new string[1] { "ObjectDataProvider cannot create object" });
			}
			return _ObjDPCreateFailed;
		}
	}

	public static AvTraceDetails ObjDPInvokeFailed
	{
		get
		{
			if (_ObjDPInvokeFailed == null)
			{
				_ObjDPInvokeFailed = new AvTraceDetails(35, new string[1] { "ObjectDataProvider: Failure trying to invoke method on type" });
			}
			return _ObjDPInvokeFailed;
		}
	}

	public static AvTraceDetails RefPreviousNotInContext
	{
		get
		{
			if (_RefPreviousNotInContext == null)
			{
				_RefPreviousNotInContext = new AvTraceDetails(36, new string[1] { "Cannot find previous element for use as RelativeSource because there is no parent in generated context." });
			}
			return _RefPreviousNotInContext;
		}
	}

	public static AvTraceDetails RefNoWrapperInChildren
	{
		get
		{
			if (_RefNoWrapperInChildren == null)
			{
				_RefNoWrapperInChildren = new AvTraceDetails(37, new string[1] { "Cannot find previous element for use as RelativeSource because children cannot be found for parent element." });
			}
			return _RefNoWrapperInChildren;
		}
	}

	public static AvTraceDetails RefAncestorTypeNotSpecified
	{
		get
		{
			if (_RefAncestorTypeNotSpecified == null)
			{
				_RefAncestorTypeNotSpecified = new AvTraceDetails(38, new string[1] { "Reference error: cannot find ancestor element; no AncestorType was specified on RelativeSource." });
			}
			return _RefAncestorTypeNotSpecified;
		}
	}

	public static AvTraceDetails RefAncestorLevelInvalid
	{
		get
		{
			if (_RefAncestorLevelInvalid == null)
			{
				_RefAncestorLevelInvalid = new AvTraceDetails(39, new string[1] { "Reference error: cannot find ancestor element; AncestorLevel on RelativeSource must be greater than 0." });
			}
			return _RefAncestorLevelInvalid;
		}
	}

	public static AvTraceDetails XmlBindingToNonXml
	{
		get
		{
			if (_XmlBindingToNonXml == null)
			{
				_XmlBindingToNonXml = new AvTraceDetails(45, new string[1] { "BindingExpression with XPath cannot bind to non-XML object." });
			}
			return _XmlBindingToNonXml;
		}
	}

	public static AvTraceDetails XmlBindingToNonXmlCollection
	{
		get
		{
			if (_XmlBindingToNonXmlCollection == null)
			{
				_XmlBindingToNonXmlCollection = new AvTraceDetails(46, new string[1] { "BindingExpression with XPath cannot bind to a collection with non-XML objects." });
			}
			return _XmlBindingToNonXmlCollection;
		}
	}

	public static AvTraceDetails CannotGetXmlNodeCollection
	{
		get
		{
			if (_CannotGetXmlNodeCollection == null)
			{
				_CannotGetXmlNodeCollection = new AvTraceDetails(47, new string[1] { "XML binding failed. Cannot obtain result node collection because of bad source node or bad Path." });
			}
			return _CannotGetXmlNodeCollection;
		}
	}

	public static AvTraceDetails XmlDPInlineDocError
	{
		get
		{
			if (_XmlDPInlineDocError == null)
			{
				_XmlDPInlineDocError = new AvTraceDetails(49, new string[1] { "XmlDataProvider cannot load inline document because of load or parse error in XML." });
			}
			return _XmlDPInlineDocError;
		}
	}

	public static AvTraceDetails XmlNamespaceNotSet
	{
		get
		{
			if (_XmlNamespaceNotSet == null)
			{
				_XmlNamespaceNotSet = new AvTraceDetails(50, new string[1] { "XmlDataProvider has inline XML that does not explicitly set its XmlNamespace (xmlns='')." });
			}
			return _XmlNamespaceNotSet;
		}
	}

	public static AvTraceDetails XmlDPAsyncDocError
	{
		get
		{
			if (_XmlDPAsyncDocError == null)
			{
				_XmlDPAsyncDocError = new AvTraceDetails(51, new string[1] { "XmlDataProvider cannot load asynchronous document from Source because of load or parse error in XML stream." });
			}
			return _XmlDPAsyncDocError;
		}
	}

	public static AvTraceDetails XmlDPSelectNodesFailed
	{
		get
		{
			if (_XmlDPSelectNodesFailed == null)
			{
				_XmlDPSelectNodesFailed = new AvTraceDetails(52, new string[1] { "Cannot select nodes because XPath for Binding is not valid" });
			}
			return _XmlDPSelectNodesFailed;
		}
	}

	public static AvTraceDetails CollectionViewIsUnsupported
	{
		get
		{
			if (_CollectionViewIsUnsupported == null)
			{
				_CollectionViewIsUnsupported = new AvTraceDetails(53, new string[1] { "Using CollectionView directly is not fully supported. The basic features work, although with some inefficiencies, but advanced features may encounter known bugs. Consider using a derived class to avoid these problems." });
			}
			return _CollectionViewIsUnsupported;
		}
	}

	public static bool IsEnabled
	{
		get
		{
			if (_avTrace != null)
			{
				return _avTrace.IsEnabled;
			}
			return false;
		}
	}

	public static bool IsEnabledOverride => _avTrace.IsEnabledOverride;

	public static AvTraceDetails CannotCreateDefaultValueConverter(params object[] args)
	{
		if (_CannotCreateDefaultValueConverter == null)
		{
			_CannotCreateDefaultValueConverter = new AvTraceDetails(1, new string[1] { "Cannot create default converter to perform '{2}' conversions between types '{0}' and '{1}'. Consider using Converter property of Binding." });
		}
		return new AvTraceFormat(_CannotCreateDefaultValueConverter, args);
	}

	public static AvTraceDetails NoSource(params object[] args)
	{
		if (_NoSource == null)
		{
			_NoSource = new AvTraceDetails(4, new string[1] { "Cannot find source for binding with reference '{0}'." });
		}
		return new AvTraceFormat(_NoSource, args);
	}

	public static AvTraceDetails BadConverterForTransfer(params object[] args)
	{
		if (_BadConverterForTransfer == null)
		{
			_BadConverterForTransfer = new AvTraceDetails(6, new string[1] { "'{0}' converter failed to convert value '{1}' (type '{2}'); fallback value will be used, if available." });
		}
		return new AvTraceFormat(_BadConverterForTransfer, args);
	}

	public static AvTraceDetails BadConverterForUpdate(params object[] args)
	{
		if (_BadConverterForUpdate == null)
		{
			_BadConverterForUpdate = new AvTraceDetails(7, new string[1] { "ConvertBack cannot convert value '{0}' (type '{1}')." });
		}
		return new AvTraceFormat(_BadConverterForUpdate, args);
	}

	public static AvTraceDetails FallbackConversionFailed(params object[] args)
	{
		if (_FallbackConversionFailed == null)
		{
			_FallbackConversionFailed = new AvTraceDetails(11, new string[1] { "Fallback value '{0}' (type '{1}') cannot be converted for use in '{2}' (type '{3}')." });
		}
		return new AvTraceFormat(_FallbackConversionFailed, args);
	}

	public static AvTraceDetails TargetNullValueConversionFailed(params object[] args)
	{
		if (_TargetNullValueConversionFailed == null)
		{
			_TargetNullValueConversionFailed = new AvTraceDetails(12, new string[1] { "TargetNullValue '{0}' (type '{1}') cannot be converted for use in '{2}' (type '{3}')." });
		}
		return new AvTraceFormat(_TargetNullValueConversionFailed, args);
	}

	public static AvTraceDetails BindingGroupNameMatchFailed(params object[] args)
	{
		if (_BindingGroupNameMatchFailed == null)
		{
			_BindingGroupNameMatchFailed = new AvTraceDetails(13, new string[1] { "No BindingGroup found with name matching '{0}'." });
		}
		return new AvTraceFormat(_BindingGroupNameMatchFailed, args);
	}

	public static AvTraceDetails BindingGroupWrongProperty(params object[] args)
	{
		if (_BindingGroupWrongProperty == null)
		{
			_BindingGroupWrongProperty = new AvTraceDetails(14, new string[1] { "BindingGroup used as a value of property '{0}' on object of type '{1}'.  This may disable its normal behavior." });
		}
		return new AvTraceFormat(_BindingGroupWrongProperty, args);
	}

	public static AvTraceDetails SharesProposedValuesRequriesImplicitBindingGroup(params object[] args)
	{
		if (_SharesProposedValuesRequriesImplicitBindingGroup == null)
		{
			_SharesProposedValuesRequriesImplicitBindingGroup = new AvTraceDetails(16, new string[1] { "Binding expression '{0}' with BindingGroupName '{1}' has joined BindingGroup '{2}' with SharesProposedValues='true'.  The SharesProposedValues feature only works for binding expressions that implicitly join a binding group." });
		}
		return new AvTraceFormat(_SharesProposedValuesRequriesImplicitBindingGroup, args);
	}

	public static AvTraceDetails CannotGetClrRawValue(params object[] args)
	{
		if (_CannotGetClrRawValue == null)
		{
			_CannotGetClrRawValue = new AvTraceDetails(17, new string[1] { "Cannot get '{0}' value (type '{1}') from '{2}' (type '{3}')." });
		}
		return new AvTraceFormat(_CannotGetClrRawValue, args);
	}

	public static AvTraceDetails CannotSetClrRawValue(params object[] args)
	{
		if (_CannotSetClrRawValue == null)
		{
			_CannotSetClrRawValue = new AvTraceDetails(18, new string[1] { "'{3}' is not a valid value for '{0}' of '{2}'." });
		}
		return new AvTraceFormat(_CannotSetClrRawValue, args);
	}

	public static AvTraceDetails DefaultValueConverterFailed(params object[] args)
	{
		if (_DefaultValueConverterFailed == null)
		{
			_DefaultValueConverterFailed = new AvTraceDetails(22, new string[1] { "Cannot convert '{0}' from type '{1}' to type '{2}' with default conversions; consider using Converter property of Binding." });
		}
		return new AvTraceFormat(_DefaultValueConverterFailed, args);
	}

	public static AvTraceDetails DefaultValueConverterFailedForCulture(params object[] args)
	{
		if (_DefaultValueConverterFailedForCulture == null)
		{
			_DefaultValueConverterFailedForCulture = new AvTraceDetails(23, new string[1] { "Cannot convert '{0}' from type '{1}' to type '{2}' for '{3}' culture with default conversions; consider using Converter property of Binding." });
		}
		return new AvTraceFormat(_DefaultValueConverterFailedForCulture, args);
	}

	public static AvTraceDetails StyleAndStyleSelectorDefined(params object[] args)
	{
		if (_StyleAndStyleSelectorDefined == null)
		{
			_StyleAndStyleSelectorDefined = new AvTraceDetails(24, new string[1] { "Both '{0}Style' and '{0}StyleSelector' are set;  '{0}StyleSelector' will be ignored." });
		}
		return new AvTraceFormat(_StyleAndStyleSelectorDefined, args);
	}

	public static AvTraceDetails TemplateAndTemplateSelectorDefined(params object[] args)
	{
		if (_TemplateAndTemplateSelectorDefined == null)
		{
			_TemplateAndTemplateSelectorDefined = new AvTraceDetails(25, new string[1] { "Both '{0}Template' and '{0}TemplateSelector' are set;  '{0}TemplateSelector' will be ignored." });
		}
		return new AvTraceFormat(_TemplateAndTemplateSelectorDefined, args);
	}

	public static AvTraceDetails BadMultiConverterForUpdate(params object[] args)
	{
		if (_BadMultiConverterForUpdate == null)
		{
			_BadMultiConverterForUpdate = new AvTraceDetails(27, new string[1] { "'{0}' MultiValueConverter failed to convert back value '{1}' (type '{2}'). Check the converter's ConvertBack method." });
		}
		return new AvTraceFormat(_BadMultiConverterForUpdate, args);
	}

	public static AvTraceDetails UnsetValueInMultiBindingExpressionUpdate(params object[] args)
	{
		if (_UnsetValueInMultiBindingExpressionUpdate == null)
		{
			_UnsetValueInMultiBindingExpressionUpdate = new AvTraceDetails(32, new string[1] { "'{0}' MultiValueConverter returned UnsetValue after converting '{1}' for source binding '{2}' (type '{3}')." });
		}
		return new AvTraceFormat(_UnsetValueInMultiBindingExpressionUpdate, args);
	}

	public static AvTraceDetails ClrReplaceItem(params object[] args)
	{
		if (_ClrReplaceItem == null)
		{
			_ClrReplaceItem = new AvTraceDetails(40, new string[1] { "BindingExpression path error: '{0}' property not found on '{2}' '{1}'." });
		}
		return new AvTraceFormat(_ClrReplaceItem, args);
	}

	public static AvTraceDetails NullItem(params object[] args)
	{
		if (_NullItem == null)
		{
			_NullItem = new AvTraceDetails(41, new string[1] { "BindingExpression path error: '{0}' property not found for '{1}' because data item is null. This could happen because the data provider has not produced any data yet." });
		}
		return new AvTraceFormat(_NullItem, args);
	}

	public static AvTraceDetails PlaceholderItem(params object[] args)
	{
		if (_PlaceholderItem == null)
		{
			_PlaceholderItem = new AvTraceDetails(42, new string[1] { "BindingExpression path error: '{0}' property not found for '{1}' because data item is the NewItemPlaceholder." });
		}
		return new AvTraceFormat(_PlaceholderItem, args);
	}

	public static AvTraceDetails DataErrorInfoFailed(params object[] args)
	{
		if (_DataErrorInfoFailed == null)
		{
			_DataErrorInfoFailed = new AvTraceDetails(43, new string[1] { "Cannot obtain IDataErrorInfo.Error[{0}] from source of type {1} - {2} '{3}'" });
		}
		return new AvTraceFormat(_DataErrorInfoFailed, args);
	}

	public static AvTraceDetails DisallowTwoWay(params object[] args)
	{
		if (_DisallowTwoWay == null)
		{
			_DisallowTwoWay = new AvTraceDetails(44, new string[1] { "Binding mode has been changed to OneWay because source property '{0}.{1}' has a non-public setter." });
		}
		return new AvTraceFormat(_DisallowTwoWay, args);
	}

	public static AvTraceDetails BadXPath(params object[] args)
	{
		if (_BadXPath == null)
		{
			_BadXPath = new AvTraceDetails(48, new string[1] { "XPath '{0}' returned no results on XmlNode '{1}'" });
		}
		return new AvTraceFormat(_BadXPath, args);
	}

	public static AvTraceDetails CollectionChangedWithoutNotification(params object[] args)
	{
		if (_CollectionChangedWithoutNotification == null)
		{
			_CollectionChangedWithoutNotification = new AvTraceDetails(54, new string[1] { "Collection of type '{0}' has been changed without raising a CollectionChanged event. Support for this is incomplete and inconsistent, and will be removed completely in a future version of WPF. Consider either (a) implementing INotifyCollectionChanged, or (b) avoiding changes to this type of collection." });
		}
		return new AvTraceFormat(_CollectionChangedWithoutNotification, args);
	}

	public static AvTraceDetails CannotSort(params object[] args)
	{
		if (_CannotSort == null)
		{
			_CannotSort = new AvTraceDetails(55, new string[1] { "Cannot sort by '{0}'" });
		}
		return new AvTraceFormat(_CannotSort, args);
	}

	public static AvTraceDetails CreatedExpression(params object[] args)
	{
		if (_CreatedExpression == null)
		{
			_CreatedExpression = new AvTraceDetails(56, new string[1] { "Created {0} for {1}" });
		}
		return new AvTraceFormat(_CreatedExpression, args);
	}

	public static AvTraceDetails CreatedExpressionInParent(params object[] args)
	{
		if (_CreatedExpressionInParent == null)
		{
			_CreatedExpressionInParent = new AvTraceDetails(57, new string[1] { "Created {0} for {1} within {2}" });
		}
		return new AvTraceFormat(_CreatedExpressionInParent, args);
	}

	public static AvTraceDetails BindingPath(params object[] args)
	{
		if (_BindingPath == null)
		{
			_BindingPath = new AvTraceDetails(58, new string[1] { " Path: {0}" });
		}
		return new AvTraceFormat(_BindingPath, args);
	}

	public static AvTraceDetails BindingXPathAndPath(params object[] args)
	{
		if (_BindingXPathAndPath == null)
		{
			_BindingXPathAndPath = new AvTraceDetails(59, new string[1] { " XPath: {0} Path: {1}" });
		}
		return new AvTraceFormat(_BindingXPathAndPath, args);
	}

	public static AvTraceDetails ResolveDefaultMode(params object[] args)
	{
		if (_ResolveDefaultMode == null)
		{
			_ResolveDefaultMode = new AvTraceDetails(60, new string[1] { "{0}: Default mode resolved to {1}" });
		}
		return new AvTraceFormat(_ResolveDefaultMode, args);
	}

	public static AvTraceDetails ResolveDefaultUpdate(params object[] args)
	{
		if (_ResolveDefaultUpdate == null)
		{
			_ResolveDefaultUpdate = new AvTraceDetails(61, new string[1] { "{0}: Default update trigger resolved to {1}" });
		}
		return new AvTraceFormat(_ResolveDefaultUpdate, args);
	}

	public static AvTraceDetails AttachExpression(params object[] args)
	{
		if (_AttachExpression == null)
		{
			_AttachExpression = new AvTraceDetails(62, new string[1] { "{0}: Attach to {1}.{2} (hash={3})" });
		}
		return new AvTraceFormat(_AttachExpression, args);
	}

	public static AvTraceDetails DetachExpression(params object[] args)
	{
		if (_DetachExpression == null)
		{
			_DetachExpression = new AvTraceDetails(63, new string[1] { "{0}: Detach" });
		}
		return new AvTraceFormat(_DetachExpression, args);
	}

	public static AvTraceDetails UseMentor(params object[] args)
	{
		if (_UseMentor == null)
		{
			_UseMentor = new AvTraceDetails(64, new string[1] { "{0}: Use Framework mentor {1}" });
		}
		return new AvTraceFormat(_UseMentor, args);
	}

	public static AvTraceDetails DeferAttachToContext(params object[] args)
	{
		if (_DeferAttachToContext == null)
		{
			_DeferAttachToContext = new AvTraceDetails(65, new string[1] { "{0}: Resolve source deferred" });
		}
		return new AvTraceFormat(_DeferAttachToContext, args);
	}

	public static AvTraceDetails SourceRequiresTreeContext(params object[] args)
	{
		if (_SourceRequiresTreeContext == null)
		{
			_SourceRequiresTreeContext = new AvTraceDetails(66, new string[1] { "{0}: {1} requires tree context" });
		}
		return new AvTraceFormat(_SourceRequiresTreeContext, args);
	}

	public static AvTraceDetails AttachToContext(params object[] args)
	{
		if (_AttachToContext == null)
		{
			_AttachToContext = new AvTraceDetails(67, new string[1] { "{0}: Resolving source {1}" });
		}
		return new AvTraceFormat(_AttachToContext, args);
	}

	public static AvTraceDetails PathRequiresTreeContext(params object[] args)
	{
		if (_PathRequiresTreeContext == null)
		{
			_PathRequiresTreeContext = new AvTraceDetails(68, new string[1] { "{0}: Path '{1}' requires namespace information" });
		}
		return new AvTraceFormat(_PathRequiresTreeContext, args);
	}

	public static AvTraceDetails NoMentorExtended(params object[] args)
	{
		if (_NoMentorExtended == null)
		{
			_NoMentorExtended = new AvTraceDetails(69, new string[1] { "{0}: Framework mentor not found" });
		}
		return new AvTraceFormat(_NoMentorExtended, args);
	}

	public static AvTraceDetails ContextElement(params object[] args)
	{
		if (_ContextElement == null)
		{
			_ContextElement = new AvTraceDetails(70, new string[1] { "{0}: Found data context element: {1} ({2})" });
		}
		return new AvTraceFormat(_ContextElement, args);
	}

	public static AvTraceDetails NullDataContext(params object[] args)
	{
		if (_NullDataContext == null)
		{
			_NullDataContext = new AvTraceDetails(71, new string[1] { "{0}: DataContext is null" });
		}
		return new AvTraceFormat(_NullDataContext, args);
	}

	public static AvTraceDetails RelativeSource(params object[] args)
	{
		if (_RelativeSource == null)
		{
			_RelativeSource = new AvTraceDetails(72, new string[1] { " RelativeSource.{0} found {1}" });
		}
		return new AvTraceFormat(_RelativeSource, args);
	}

	public static AvTraceDetails AncestorLookup(params object[] args)
	{
		if (_AncestorLookup == null)
		{
			_AncestorLookup = new AvTraceDetails(73, new string[1] { "  Lookup ancestor of type {0}: queried {1}" });
		}
		return new AvTraceFormat(_AncestorLookup, args);
	}

	public static AvTraceDetails ElementNameQuery(params object[] args)
	{
		if (_ElementNameQuery == null)
		{
			_ElementNameQuery = new AvTraceDetails(74, new string[1] { "  Lookup name {0}: queried {1}" });
		}
		return new AvTraceFormat(_ElementNameQuery, args);
	}

	public static AvTraceDetails ElementNameQueryTemplate(params object[] args)
	{
		if (_ElementNameQueryTemplate == null)
		{
			_ElementNameQueryTemplate = new AvTraceDetails(75, new string[1] { "  Lookup name {0}: queried template of {1}" });
		}
		return new AvTraceFormat(_ElementNameQueryTemplate, args);
	}

	public static AvTraceDetails UseCVS(params object[] args)
	{
		if (_UseCVS == null)
		{
			_UseCVS = new AvTraceDetails(76, new string[1] { "{0}: Use View from {1}" });
		}
		return new AvTraceFormat(_UseCVS, args);
	}

	public static AvTraceDetails UseDataProvider(params object[] args)
	{
		if (_UseDataProvider == null)
		{
			_UseDataProvider = new AvTraceDetails(77, new string[1] { "{0}: Use Data from {1}" });
		}
		return new AvTraceFormat(_UseDataProvider, args);
	}

	public static AvTraceDetails ActivateItem(params object[] args)
	{
		if (_ActivateItem == null)
		{
			_ActivateItem = new AvTraceDetails(78, new string[1] { "{0}: Activate with root item {1}" });
		}
		return new AvTraceFormat(_ActivateItem, args);
	}

	public static AvTraceDetails Deactivate(params object[] args)
	{
		if (_Deactivate == null)
		{
			_Deactivate = new AvTraceDetails(79, new string[1] { "{0}: Deactivate" });
		}
		return new AvTraceFormat(_Deactivate, args);
	}

	public static AvTraceDetails GetRawValue(params object[] args)
	{
		if (_GetRawValue == null)
		{
			_GetRawValue = new AvTraceDetails(80, new string[1] { "{0}: TransferValue - got raw value {1}" });
		}
		return new AvTraceFormat(_GetRawValue, args);
	}

	public static AvTraceDetails ConvertDBNull(params object[] args)
	{
		if (_ConvertDBNull == null)
		{
			_ConvertDBNull = new AvTraceDetails(81, new string[1] { "{0}: TransferValue - converted DBNull to {1}" });
		}
		return new AvTraceFormat(_ConvertDBNull, args);
	}

	public static AvTraceDetails UserConverter(params object[] args)
	{
		if (_UserConverter == null)
		{
			_UserConverter = new AvTraceDetails(82, new string[1] { "{0}: TransferValue - user's converter produced {1}" });
		}
		return new AvTraceFormat(_UserConverter, args);
	}

	public static AvTraceDetails NullConverter(params object[] args)
	{
		if (_NullConverter == null)
		{
			_NullConverter = new AvTraceDetails(83, new string[1] { "{0}: TransferValue - null-value conversion produced {1}" });
		}
		return new AvTraceFormat(_NullConverter, args);
	}

	public static AvTraceDetails DefaultConverter(params object[] args)
	{
		if (_DefaultConverter == null)
		{
			_DefaultConverter = new AvTraceDetails(84, new string[1] { "{0}: TransferValue - implicit converter produced {1}" });
		}
		return new AvTraceFormat(_DefaultConverter, args);
	}

	public static AvTraceDetails FormattedValue(params object[] args)
	{
		if (_FormattedValue == null)
		{
			_FormattedValue = new AvTraceDetails(85, new string[1] { "{0}: TransferValue - string formatting produced {1}" });
		}
		return new AvTraceFormat(_FormattedValue, args);
	}

	public static AvTraceDetails FormattingFailed(params object[] args)
	{
		if (_FormattingFailed == null)
		{
			_FormattingFailed = new AvTraceDetails(86, new string[1] { "{0}: TransferValue - string formatting failed, using format '{1}'" });
		}
		return new AvTraceFormat(_FormattingFailed, args);
	}

	public static AvTraceDetails BadValueAtTransferExtended(params object[] args)
	{
		if (_BadValueAtTransferExtended == null)
		{
			_BadValueAtTransferExtended = new AvTraceDetails(87, new string[1] { "{0}: TransferValue - value {1} is not valid for target" });
		}
		return new AvTraceFormat(_BadValueAtTransferExtended, args);
	}

	public static AvTraceDetails UseFallback(params object[] args)
	{
		if (_UseFallback == null)
		{
			_UseFallback = new AvTraceDetails(88, new string[1] { "{0}: TransferValue - using fallback/default value {1}" });
		}
		return new AvTraceFormat(_UseFallback, args);
	}

	public static AvTraceDetails TransferValue(params object[] args)
	{
		if (_TransferValue == null)
		{
			_TransferValue = new AvTraceDetails(89, new string[1] { "{0}: TransferValue - using final value {1}" });
		}
		return new AvTraceFormat(_TransferValue, args);
	}

	public static AvTraceDetails UpdateRawValue(params object[] args)
	{
		if (_UpdateRawValue == null)
		{
			_UpdateRawValue = new AvTraceDetails(90, new string[1] { "{0}: Update - got raw value {1}" });
		}
		return new AvTraceFormat(_UpdateRawValue, args);
	}

	public static AvTraceDetails ValidationRuleFailed(params object[] args)
	{
		if (_ValidationRuleFailed == null)
		{
			_ValidationRuleFailed = new AvTraceDetails(91, new string[1] { "{0}: Update - {1} failed" });
		}
		return new AvTraceFormat(_ValidationRuleFailed, args);
	}

	public static AvTraceDetails UserConvertBack(params object[] args)
	{
		if (_UserConvertBack == null)
		{
			_UserConvertBack = new AvTraceDetails(92, new string[1] { "{0}: Update - user's converter produced {1}" });
		}
		return new AvTraceFormat(_UserConvertBack, args);
	}

	public static AvTraceDetails DefaultConvertBack(params object[] args)
	{
		if (_DefaultConvertBack == null)
		{
			_DefaultConvertBack = new AvTraceDetails(93, new string[1] { "{0}: Update - implicit converter produced {1}" });
		}
		return new AvTraceFormat(_DefaultConvertBack, args);
	}

	public static AvTraceDetails Update(params object[] args)
	{
		if (_Update == null)
		{
			_Update = new AvTraceDetails(94, new string[1] { "{0}: Update - using final value {1}" });
		}
		return new AvTraceFormat(_Update, args);
	}

	public static AvTraceDetails GotEvent(params object[] args)
	{
		if (_GotEvent == null)
		{
			_GotEvent = new AvTraceDetails(95, new string[1] { "{0}: Got {1} event from {2}" });
		}
		return new AvTraceFormat(_GotEvent, args);
	}

	public static AvTraceDetails GotPropertyChanged(params object[] args)
	{
		if (_GotPropertyChanged == null)
		{
			_GotPropertyChanged = new AvTraceDetails(96, new string[1] { "{0}: Got PropertyChanged event from {1} for {2}" });
		}
		return new AvTraceFormat(_GotPropertyChanged, args);
	}

	public static AvTraceDetails PriorityTransfer(params object[] args)
	{
		if (_PriorityTransfer == null)
		{
			_PriorityTransfer = new AvTraceDetails(97, new string[1] { "{0}: TransferValue '{1}' from child {2} - {3}" });
		}
		return new AvTraceFormat(_PriorityTransfer, args);
	}

	public static AvTraceDetails ChildNotAttached(params object[] args)
	{
		if (_ChildNotAttached == null)
		{
			_ChildNotAttached = new AvTraceDetails(98, new string[1] { "{0}: One or more children have not resolved sources" });
		}
		return new AvTraceFormat(_ChildNotAttached, args);
	}

	public static AvTraceDetails GetRawValueMulti(params object[] args)
	{
		if (_GetRawValueMulti == null)
		{
			_GetRawValueMulti = new AvTraceDetails(99, new string[1] { "{0}: TransferValue - got raw value {1}: {2}" });
		}
		return new AvTraceFormat(_GetRawValueMulti, args);
	}

	public static AvTraceDetails UserConvertBackMulti(params object[] args)
	{
		if (_UserConvertBackMulti == null)
		{
			_UserConvertBackMulti = new AvTraceDetails(100, new string[1] { "{0}: Update - multiconverter produced value {1}: {2}" });
		}
		return new AvTraceFormat(_UserConvertBackMulti, args);
	}

	public static AvTraceDetails GetValue(params object[] args)
	{
		if (_GetValue == null)
		{
			_GetValue = new AvTraceDetails(101, new string[1] { "{0}: GetValue at level {1} from {2} using {3}: {4}" });
		}
		return new AvTraceFormat(_GetValue, args);
	}

	public static AvTraceDetails SetValue(params object[] args)
	{
		if (_SetValue == null)
		{
			_SetValue = new AvTraceDetails(102, new string[1] { "{0}: SetValue at level {1} to {2} using {3}: {4}" });
		}
		return new AvTraceFormat(_SetValue, args);
	}

	public static AvTraceDetails ReplaceItemShort(params object[] args)
	{
		if (_ReplaceItemShort == null)
		{
			_ReplaceItemShort = new AvTraceDetails(103, new string[1] { "{0}: Replace item at level {1} with {2}" });
		}
		return new AvTraceFormat(_ReplaceItemShort, args);
	}

	public static AvTraceDetails ReplaceItemLong(params object[] args)
	{
		if (_ReplaceItemLong == null)
		{
			_ReplaceItemLong = new AvTraceDetails(104, new string[1] { "{0}: Replace item at level {1} with {2}, using accessor {3}" });
		}
		return new AvTraceFormat(_ReplaceItemLong, args);
	}

	public static AvTraceDetails GetInfo_Reuse(params object[] args)
	{
		if (_GetInfo_Reuse == null)
		{
			_GetInfo_Reuse = new AvTraceDetails(105, new string[1] { "{0}:   Item at level {1} has same type - reuse accessor {2}" });
		}
		return new AvTraceFormat(_GetInfo_Reuse, args);
	}

	public static AvTraceDetails GetInfo_Null(params object[] args)
	{
		if (_GetInfo_Null == null)
		{
			_GetInfo_Null = new AvTraceDetails(106, new string[1] { "{0}:   Item at level {1} is null - no accessor" });
		}
		return new AvTraceFormat(_GetInfo_Null, args);
	}

	public static AvTraceDetails GetInfo_Cache(params object[] args)
	{
		if (_GetInfo_Cache == null)
		{
			_GetInfo_Cache = new AvTraceDetails(107, new string[1] { "{0}:   At level {1} using cached accessor for {2}.{3}: {4}" });
		}
		return new AvTraceFormat(_GetInfo_Cache, args);
	}

	public static AvTraceDetails GetInfo_Property(params object[] args)
	{
		if (_GetInfo_Property == null)
		{
			_GetInfo_Property = new AvTraceDetails(108, new string[1] { "{0}:   At level {1} - for {2}.{3} found accessor {4}" });
		}
		return new AvTraceFormat(_GetInfo_Property, args);
	}

	public static AvTraceDetails GetInfo_Indexer(params object[] args)
	{
		if (_GetInfo_Indexer == null)
		{
			_GetInfo_Indexer = new AvTraceDetails(109, new string[1] { "{0}:   At level {1} - for {2}[{3}] found accessor {4}" });
		}
		return new AvTraceFormat(_GetInfo_Indexer, args);
	}

	public static AvTraceDetails XmlContextNode(params object[] args)
	{
		if (_XmlContextNode == null)
		{
			_XmlContextNode = new AvTraceDetails(110, new string[1] { "{0}: Context for XML binding set to {1}" });
		}
		return new AvTraceFormat(_XmlContextNode, args);
	}

	public static AvTraceDetails XmlNewCollection(params object[] args)
	{
		if (_XmlNewCollection == null)
		{
			_XmlNewCollection = new AvTraceDetails(111, new string[1] { "{0}: Building collection from {1}" });
		}
		return new AvTraceFormat(_XmlNewCollection, args);
	}

	public static AvTraceDetails XmlSynchronizeCollection(params object[] args)
	{
		if (_XmlSynchronizeCollection == null)
		{
			_XmlSynchronizeCollection = new AvTraceDetails(112, new string[1] { "{0}: Synchronizing collection with {1}" });
		}
		return new AvTraceFormat(_XmlSynchronizeCollection, args);
	}

	public static AvTraceDetails SelectNodes(params object[] args)
	{
		if (_SelectNodes == null)
		{
			_SelectNodes = new AvTraceDetails(113, new string[1] { "{0}: SelectNodes at {1} using XPath {2}: {3}" });
		}
		return new AvTraceFormat(_SelectNodes, args);
	}

	public static AvTraceDetails BeginQuery(params object[] args)
	{
		if (_BeginQuery == null)
		{
			_BeginQuery = new AvTraceDetails(114, new string[1] { "{0}: Begin query ({1})" });
		}
		return new AvTraceFormat(_BeginQuery, args);
	}

	public static AvTraceDetails QueryFinished(params object[] args)
	{
		if (_QueryFinished == null)
		{
			_QueryFinished = new AvTraceDetails(115, new string[1] { "{0}: Query finished ({1}) with data {2} and error {3}" });
		}
		return new AvTraceFormat(_QueryFinished, args);
	}

	public static AvTraceDetails QueryResult(params object[] args)
	{
		if (_QueryResult == null)
		{
			_QueryResult = new AvTraceDetails(116, new string[1] { "{0}: Update result (on UI thread) with data {1}" });
		}
		return new AvTraceFormat(_QueryResult, args);
	}

	public static AvTraceDetails XmlLoadSource(params object[] args)
	{
		if (_XmlLoadSource == null)
		{
			_XmlLoadSource = new AvTraceDetails(117, new string[1] { "{0}: Request download ({1}) from {2}" });
		}
		return new AvTraceFormat(_XmlLoadSource, args);
	}

	public static AvTraceDetails XmlLoadDoc(params object[] args)
	{
		if (_XmlLoadDoc == null)
		{
			_XmlLoadDoc = new AvTraceDetails(118, new string[1] { "{0}: Load document from stream" });
		}
		return new AvTraceFormat(_XmlLoadDoc, args);
	}

	public static AvTraceDetails XmlLoadInline(params object[] args)
	{
		if (_XmlLoadInline == null)
		{
			_XmlLoadInline = new AvTraceDetails(119, new string[1] { "{0}: Load inline document" });
		}
		return new AvTraceFormat(_XmlLoadInline, args);
	}

	public static AvTraceDetails XmlBuildCollection(params object[] args)
	{
		if (_XmlBuildCollection == null)
		{
			_XmlBuildCollection = new AvTraceDetails(120, new string[1] { "{0}: Build XmlNode collection" });
		}
		return new AvTraceFormat(_XmlBuildCollection, args);
	}

	public static void Trace(TraceEventType type, AvTraceDetails traceDetails, params object[] parameters)
	{
		_avTrace.Trace(type, traceDetails.Id, traceDetails.Message, traceDetails.Labels, parameters);
	}

	public static void Trace(TraceEventType type, AvTraceDetails traceDetails)
	{
		_avTrace.Trace(type, traceDetails.Id, traceDetails.Message, traceDetails.Labels, Array.Empty<object>());
	}

	public static void Trace(TraceEventType type, AvTraceDetails traceDetails, object p1)
	{
		_avTrace.Trace(type, traceDetails.Id, traceDetails.Message, traceDetails.Labels, new object[1] { p1 });
	}

	public static void Trace(TraceEventType type, AvTraceDetails traceDetails, object p1, object p2)
	{
		_avTrace.Trace(type, traceDetails.Id, traceDetails.Message, traceDetails.Labels, new object[2] { p1, p2 });
	}

	public static void Trace(TraceEventType type, AvTraceDetails traceDetails, object p1, object p2, object p3)
	{
		_avTrace.Trace(type, traceDetails.Id, traceDetails.Message, traceDetails.Labels, new object[3] { p1, p2, p3 });
	}

	public static void TraceActivityItem(AvTraceDetails traceDetails, params object[] parameters)
	{
		_avTrace.TraceStartStop(traceDetails.Id, traceDetails.Message, traceDetails.Labels, parameters);
	}

	public static void TraceActivityItem(AvTraceDetails traceDetails)
	{
		_avTrace.TraceStartStop(traceDetails.Id, traceDetails.Message, traceDetails.Labels, Array.Empty<object>());
	}

	public static void TraceActivityItem(AvTraceDetails traceDetails, object p1)
	{
		_avTrace.TraceStartStop(traceDetails.Id, traceDetails.Message, traceDetails.Labels, new object[1] { p1 });
	}

	public static void TraceActivityItem(AvTraceDetails traceDetails, object p1, object p2)
	{
		_avTrace.TraceStartStop(traceDetails.Id, traceDetails.Message, traceDetails.Labels, new object[2] { p1, p2 });
	}

	public static void TraceActivityItem(AvTraceDetails traceDetails, object p1, object p2, object p3)
	{
		_avTrace.TraceStartStop(traceDetails.Id, traceDetails.Message, traceDetails.Labels, new object[3] { p1, p2, p3 });
	}

	public static void Refresh()
	{
		_avTrace.Refresh();
	}

	static TraceData()
	{
		_avTrace = new AvTrace(() => PresentationTraceSources.DataBindingSource, delegate
		{
			PresentationTraceSources._DataBindingSource = null;
		});
		_avTrace.TraceExtraMessages += OnTrace;
		_avTrace.EnabledByDebugger = true;
		_avTrace.SuppressGeneratedParameters = true;
	}

	public static bool IsExtendedTraceEnabled(object element, TraceDataLevel level)
	{
		if (IsEnabled)
		{
			return (int)PresentationTraceSources.GetTraceLevel(element) >= (int)level;
		}
		return false;
	}

	public static void OnTrace(AvTraceBuilder traceBuilder, object[] parameters, int start)
	{
		for (int i = start; i < parameters.Length; i++)
		{
			object obj = parameters[i];
			string text = obj as string;
			traceBuilder.Append(" ");
			if (text != null)
			{
				traceBuilder.Append(text);
			}
			else if (obj != null)
			{
				traceBuilder.Append(obj.GetType().Name);
				traceBuilder.Append(":");
				Describe(traceBuilder, obj);
			}
			else
			{
				traceBuilder.Append("null");
			}
		}
	}

	public static void Describe(AvTraceBuilder traceBuilder, object o)
	{
		if (o == null)
		{
			traceBuilder.Append("null");
		}
		else if (o is BindingExpression)
		{
			BindingExpression bindingExpression = o as BindingExpression;
			Describe(traceBuilder, bindingExpression.ParentBinding);
			traceBuilder.Append("; DataItem=");
			DescribeSourceObject(traceBuilder, bindingExpression.DataItem);
			traceBuilder.Append("; ");
			DescribeTarget(traceBuilder, bindingExpression.TargetElement, bindingExpression.TargetProperty);
		}
		else if (o is Binding)
		{
			Binding binding = o as Binding;
			if (binding.Path != null)
			{
				traceBuilder.AppendFormat("Path={0}", binding.Path.Path);
			}
			else if (binding.XPath != null)
			{
				traceBuilder.AppendFormat("XPath={0}", binding.XPath);
			}
			else
			{
				traceBuilder.Append("(no path)");
			}
		}
		else if (o is BindingExpressionBase)
		{
			BindingExpressionBase bindingExpressionBase = o as BindingExpressionBase;
			DescribeTarget(traceBuilder, bindingExpressionBase.TargetElement, bindingExpressionBase.TargetProperty);
		}
		else if (o is DependencyObject)
		{
			DescribeSourceObject(traceBuilder, o);
		}
		else
		{
			traceBuilder.AppendFormat("'{0}'", AvTrace.ToStringHelper(o));
		}
	}

	public static void DescribeSourceObject(AvTraceBuilder traceBuilder, object o)
	{
		if (o == null)
		{
			traceBuilder.Append("null");
		}
		else if (o is FrameworkElement frameworkElement)
		{
			traceBuilder.AppendFormat("'{0}' (Name='{1}')", frameworkElement.GetType().Name, frameworkElement.Name);
		}
		else
		{
			traceBuilder.AppendFormat("'{0}' (HashCode={1})", o.GetType().Name, o.GetHashCode());
		}
	}

	public static string DescribeSourceObject(object o)
	{
		AvTraceBuilder avTraceBuilder = new AvTraceBuilder(null);
		DescribeSourceObject(avTraceBuilder, o);
		return avTraceBuilder.ToString();
	}

	public static void DescribeTarget(AvTraceBuilder traceBuilder, DependencyObject targetElement, DependencyProperty targetProperty)
	{
		if (targetElement != null)
		{
			traceBuilder.Append("target element is ");
			DescribeSourceObject(traceBuilder, targetElement);
			if (targetProperty != null)
			{
				traceBuilder.Append("; ");
			}
		}
		if (targetProperty != null)
		{
			traceBuilder.AppendFormat("target property is '{0}' (type '{1}')", targetProperty.Name, targetProperty.PropertyType.Name);
		}
	}

	public static string DescribeTarget(DependencyObject targetElement, DependencyProperty targetProperty)
	{
		AvTraceBuilder avTraceBuilder = new AvTraceBuilder(null);
		DescribeTarget(avTraceBuilder, targetElement, targetProperty);
		return avTraceBuilder.ToString();
	}

	public static string Identify(object o)
	{
		if (o == null)
		{
			return "<null>";
		}
		Type type = o.GetType();
		if (type.IsPrimitive || type.IsEnum)
		{
			return Format("'{0}'", o);
		}
		if (o is string s)
		{
			return Format("'{0}'", AvTrace.AntiFormat(s));
		}
		if (o is NamedObject namedObject)
		{
			return AvTrace.AntiFormat(namedObject.ToString());
		}
		if (o is ICollection collection)
		{
			return Format("{0} (hash={1} Count={2})", type.Name, AvTrace.GetHashCodeHelper(o), collection.Count);
		}
		return Format("{0} (hash={1})", type.Name, AvTrace.GetHashCodeHelper(o));
	}

	public static string IdentifyWeakEvent(Type type)
	{
		string text = type.Name;
		if (text.EndsWith("EventManager", StringComparison.Ordinal))
		{
			text = text.Substring(0, text.Length - "EventManager".Length);
		}
		return text;
	}

	public static string IdentifyAccessor(object accessor)
	{
		if (accessor is DependencyProperty dependencyProperty)
		{
			return Format("{0}({1})", dependencyProperty.GetType().Name, dependencyProperty.Name);
		}
		PropertyInfo propertyInfo = accessor as PropertyInfo;
		if (propertyInfo != null)
		{
			return Format("{0}({1})", propertyInfo.GetType().Name, propertyInfo.Name);
		}
		if (accessor is PropertyDescriptor propertyDescriptor)
		{
			return Format("{0}({1})", propertyDescriptor.GetType().Name, propertyDescriptor.Name);
		}
		return Identify(accessor);
	}

	public static string IdentifyException(Exception ex)
	{
		if (ex == null)
		{
			return "<no error>";
		}
		return Format("{0} ({1})", ex.GetType().Name, AvTrace.AntiFormat(ex.Message));
	}

	private static string Format(string format, params object[] args)
	{
		return string.Format(TypeConverterHelper.InvariantEnglishUS, format, args);
	}

	public static void TraceAndNotify(TraceEventType eventType, AvTraceDetails traceDetails, BindingExpressionBase binding, Exception exception = null)
	{
		object[] parameters = ((exception == null) ? new object[1] { binding } : new object[2] { binding, exception });
		string text = _avTrace.Trace(eventType, traceDetails.Id, traceDetails.Message, traceDetails.Labels, parameters);
		if (text != null && BindingDiagnostics.IsEnabled)
		{
			object[] parameters2 = ((exception == null) ? null : new object[1] { exception });
			BindingDiagnostics.NotifyBindingFailed(new BindingFailedEventArgs(eventType, traceDetails.Id, text, binding, parameters2));
		}
	}

	public static void TraceAndNotify(TraceEventType eventType, AvTraceDetails traceDetails, Exception exception = null)
	{
		object[] array = ((exception == null) ? null : new object[1] { exception });
		TraceAndNotify(eventType, traceDetails, null, array, array);
	}

	public static void TraceAndNotify(TraceEventType eventType, AvTraceDetails traceDetails, BindingExpressionBase binding, object[] traceParameters, object[] eventParameters = null)
	{
		string text = _avTrace.Trace(eventType, traceDetails.Id, traceDetails.Message, traceDetails.Labels, traceParameters);
		if (text != null && BindingDiagnostics.IsEnabled)
		{
			BindingDiagnostics.NotifyBindingFailed(new BindingFailedEventArgs(eventType, traceDetails.Id, text, binding, eventParameters));
		}
	}

	public static void TraceAndNotifyWithNoParameters(TraceEventType eventType, AvTraceDetails traceDetails, BindingExpressionBase binding)
	{
		TraceAndNotify(eventType, traceDetails, binding, null, null);
	}
}
