using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;

namespace System.Windows.Baml2006;

internal class WpfXamlType : XamlType
{
	[Flags]
	private enum BoolTypeBits
	{
		BamlScenerio = 1,
		V3Rules = 2
	}

	private const int ConcurrencyLevel = 1;

	private const int Capacity = 11;

	private ConcurrentDictionary<string, XamlMember> _attachableMembers;

	private ConcurrentDictionary<string, XamlMember> _members;

	protected byte _bitField;

	private bool IsBamlScenario
	{
		get
		{
			return GetFlag(ref _bitField, 1);
		}
		set
		{
			SetFlag(ref _bitField, 1, value);
		}
	}

	private bool UseV3Rules
	{
		get
		{
			return GetFlag(ref _bitField, 2);
		}
		set
		{
			SetFlag(ref _bitField, 2, value);
		}
	}

	protected ConcurrentDictionary<string, XamlMember> Members
	{
		get
		{
			if (_members == null)
			{
				_members = new ConcurrentDictionary<string, XamlMember>(1, 11);
			}
			return _members;
		}
	}

	protected ConcurrentDictionary<string, XamlMember> AttachableMembers
	{
		get
		{
			if (_attachableMembers == null)
			{
				_attachableMembers = new ConcurrentDictionary<string, XamlMember>(1, 11);
			}
			return _attachableMembers;
		}
	}

	private bool IsXmlNamespaceMappingCollection
	{
		[MethodImpl(MethodImplOptions.NoInlining)]
		get
		{
			return typeof(XmlNamespaceMappingCollection).IsAssignableFrom(base.UnderlyingType);
		}
	}

	public WpfXamlType(Type type, XamlSchemaContext schema, bool isBamlScenario, bool useV3Rules)
		: base(type, schema)
	{
		IsBamlScenario = isBamlScenario;
		UseV3Rules = useV3Rules;
	}

	protected override XamlMember LookupContentProperty()
	{
		XamlMember xamlMember = base.LookupContentProperty();
		WpfXamlMember wpfXamlMember = xamlMember as WpfXamlMember;
		if (wpfXamlMember != null)
		{
			xamlMember = wpfXamlMember.AsContentProperty;
		}
		return xamlMember;
	}

	protected override bool LookupIsNameScope()
	{
		if (base.UnderlyingType == typeof(ResourceDictionary))
		{
			return false;
		}
		if (typeof(ResourceDictionary).IsAssignableFrom(base.UnderlyingType))
		{
			MethodInfo[] targetMethods = base.UnderlyingType.GetInterfaceMap(typeof(INameScope)).TargetMethods;
			foreach (MethodInfo methodInfo in targetMethods)
			{
				if (methodInfo.Name.Contains("RegisterName"))
				{
					return methodInfo.DeclaringType != typeof(ResourceDictionary);
				}
			}
			return false;
		}
		return base.LookupIsNameScope();
	}

	private XamlMember FindMember(string name, bool isAttached, bool skipReadOnlyCheck)
	{
		XamlMember xamlMember = FindKnownMember(name, isAttached);
		if (xamlMember != null)
		{
			return xamlMember;
		}
		xamlMember = FindDependencyPropertyBackedProperty(name, isAttached, skipReadOnlyCheck);
		if (xamlMember != null)
		{
			return xamlMember;
		}
		xamlMember = FindRoutedEventBackedProperty(name, isAttached, skipReadOnlyCheck);
		if (xamlMember != null)
		{
			return xamlMember;
		}
		xamlMember = ((!isAttached) ? base.LookupMember(name, skipReadOnlyCheck) : base.LookupAttachableMember(name));
		WpfKnownType wpfXamlType;
		if (xamlMember != null && (wpfXamlType = xamlMember.DeclaringType as WpfKnownType) != null)
		{
			XamlMember xamlMember2 = FindKnownMember(wpfXamlType, name, isAttached);
			if (xamlMember2 != null)
			{
				return xamlMember2;
			}
		}
		return xamlMember;
	}

	protected override XamlMember LookupMember(string name, bool skipReadOnlyCheck)
	{
		return FindMember(name, isAttached: false, skipReadOnlyCheck);
	}

	protected override XamlMember LookupAttachableMember(string name)
	{
		return FindMember(name, isAttached: true, skipReadOnlyCheck: false);
	}

	protected override IEnumerable<XamlMember> LookupAllMembers()
	{
		List<XamlMember> list = new List<XamlMember>();
		foreach (XamlMember item in base.LookupAllMembers())
		{
			XamlMember xamlMember = item;
			if (!(xamlMember is WpfXamlMember))
			{
				xamlMember = GetMember(xamlMember.Name);
			}
			list.Add(xamlMember);
		}
		return list;
	}

	private XamlMember FindKnownMember(string name, bool isAttachable)
	{
		XamlType xamlType = this;
		if (this is WpfKnownType)
		{
			do
			{
				XamlMember xamlMember = FindKnownMember(xamlType as WpfXamlType, name, isAttachable);
				if (xamlMember != null)
				{
					return xamlMember;
				}
				xamlType = xamlType.BaseType;
			}
			while (xamlType != null);
		}
		return null;
	}

	private XamlMember FindRoutedEventBackedProperty(string name, bool isAttachable, bool skipReadOnlyCheck)
	{
		RoutedEvent routedEventFromName = EventManager.GetRoutedEventFromName(name, base.UnderlyingType);
		XamlMember result = null;
		if (routedEventFromName != null)
		{
			WpfXamlType wpfXamlType = null;
			wpfXamlType = ((!IsBamlScenario) ? (System.Windows.Markup.XamlReader.GetWpfSchemaContext().GetXamlType(routedEventFromName.OwnerType) as WpfXamlType) : (System.Windows.Markup.XamlReader.BamlSharedSchemaContext.GetXamlType(routedEventFromName.OwnerType) as WpfXamlType));
			if (wpfXamlType != null)
			{
				result = FindKnownMember(wpfXamlType, name, isAttachable);
			}
			if (IsBamlScenario)
			{
				result = new WpfXamlMember(routedEventFromName, isAttachable);
			}
			else if (isAttachable)
			{
				result = GetAttachedRoutedEvent(name, routedEventFromName);
				if (result == null)
				{
					result = GetRoutedEvent(name, routedEventFromName, skipReadOnlyCheck);
				}
				if (result == null)
				{
					result = new WpfXamlMember(routedEventFromName, isAttachable: true);
				}
			}
			else
			{
				result = GetRoutedEvent(name, routedEventFromName, skipReadOnlyCheck);
				if (result == null)
				{
					result = GetAttachedRoutedEvent(name, routedEventFromName);
				}
				if (result == null)
				{
					result = new WpfXamlMember(routedEventFromName, isAttachable: false);
				}
			}
			if (Members.TryAdd(name, result))
			{
				return result;
			}
			return Members[name];
		}
		return result;
	}

	private XamlMember FindDependencyPropertyBackedProperty(string name, bool isAttachable, bool skipReadOnlyCheck)
	{
		XamlMember xamlMember = null;
		DependencyProperty dependencyProperty;
		if ((dependencyProperty = DependencyProperty.FromName(name, base.UnderlyingType)) != null)
		{
			WpfXamlType wpfXamlType = null;
			wpfXamlType = ((!IsBamlScenario) ? (System.Windows.Markup.XamlReader.GetWpfSchemaContext().GetXamlType(dependencyProperty.OwnerType) as WpfXamlType) : (System.Windows.Markup.XamlReader.BamlSharedSchemaContext.GetXamlType(dependencyProperty.OwnerType) as WpfXamlType));
			if (wpfXamlType != null)
			{
				xamlMember = FindKnownMember(wpfXamlType, name, isAttachable);
			}
			if (xamlMember == null)
			{
				if (IsBamlScenario)
				{
					xamlMember = new WpfXamlMember(dependencyProperty, isAttachable);
				}
				else if (isAttachable)
				{
					xamlMember = GetAttachedDependencyProperty(name, dependencyProperty);
					if (xamlMember == null)
					{
						return null;
					}
				}
				else
				{
					xamlMember = GetRegularDependencyProperty(name, dependencyProperty, skipReadOnlyCheck);
					if (xamlMember == null)
					{
						xamlMember = GetAttachedDependencyProperty(name, dependencyProperty);
					}
					if (xamlMember == null)
					{
						xamlMember = new WpfXamlMember(dependencyProperty, isAttachable: false);
					}
				}
				return CacheAndReturnXamlMember(xamlMember);
			}
		}
		return xamlMember;
	}

	private XamlMember CacheAndReturnXamlMember(XamlMember xamlMember)
	{
		if (!xamlMember.IsAttachable || IsBamlScenario)
		{
			if (Members.TryAdd(xamlMember.Name, xamlMember))
			{
				return xamlMember;
			}
			return Members[xamlMember.Name];
		}
		if (AttachableMembers.TryAdd(xamlMember.Name, xamlMember))
		{
			return xamlMember;
		}
		return AttachableMembers[xamlMember.Name];
	}

	private XamlMember GetAttachedRoutedEvent(string name, RoutedEvent re)
	{
		XamlMember xamlMember = base.LookupAttachableMember(name);
		if (xamlMember != null)
		{
			return new WpfXamlMember(re, (MethodInfo)xamlMember.UnderlyingMember, base.SchemaContext, UseV3Rules);
		}
		return null;
	}

	private XamlMember GetRoutedEvent(string name, RoutedEvent re, bool skipReadOnlyCheck)
	{
		XamlMember xamlMember = base.LookupMember(name, skipReadOnlyCheck);
		if (xamlMember != null)
		{
			return new WpfXamlMember(re, (EventInfo)xamlMember.UnderlyingMember, base.SchemaContext, UseV3Rules);
		}
		return null;
	}

	private XamlMember GetAttachedDependencyProperty(string name, DependencyProperty property)
	{
		XamlMember xamlMember = base.LookupAttachableMember(name);
		if (xamlMember != null)
		{
			return new WpfXamlMember(property, xamlMember.Invoker.UnderlyingGetter, xamlMember.Invoker.UnderlyingSetter, base.SchemaContext, UseV3Rules);
		}
		return null;
	}

	private XamlMember GetRegularDependencyProperty(string name, DependencyProperty property, bool skipReadOnlyCheck)
	{
		XamlMember xamlMember = base.LookupMember(name, skipReadOnlyCheck);
		if (xamlMember != null)
		{
			PropertyInfo propertyInfo = xamlMember.UnderlyingMember as PropertyInfo;
			if (propertyInfo != null)
			{
				return new WpfXamlMember(property, propertyInfo, base.SchemaContext, UseV3Rules);
			}
			throw new NotImplementedException();
		}
		return null;
	}

	private static XamlMember FindKnownMember(WpfXamlType wpfXamlType, string name, bool isAttachable)
	{
		XamlMember value = null;
		if (!isAttachable || wpfXamlType.IsBamlScenario)
		{
			if (wpfXamlType._members != null && wpfXamlType.Members.TryGetValue(name, out value))
			{
				return value;
			}
		}
		else if (wpfXamlType._attachableMembers != null && wpfXamlType.AttachableMembers.TryGetValue(name, out value))
		{
			return value;
		}
		WpfKnownType wpfKnownType = wpfXamlType as WpfKnownType;
		if (wpfKnownType != null)
		{
			if (!isAttachable || wpfXamlType.IsBamlScenario)
			{
				value = System.Windows.Markup.XamlReader.BamlSharedSchemaContext.CreateKnownMember(wpfXamlType.Name, name);
			}
			if (isAttachable || (value == null && wpfXamlType.IsBamlScenario))
			{
				value = System.Windows.Markup.XamlReader.BamlSharedSchemaContext.CreateKnownAttachableMember(wpfXamlType.Name, name);
			}
			if (value != null)
			{
				return wpfKnownType.CacheAndReturnXamlMember(value);
			}
		}
		return null;
	}

	protected override XamlCollectionKind LookupCollectionKind()
	{
		if (UseV3Rules)
		{
			if (base.UnderlyingType.IsArray)
			{
				return XamlCollectionKind.Array;
			}
			if (typeof(IDictionary).IsAssignableFrom(base.UnderlyingType))
			{
				return XamlCollectionKind.Dictionary;
			}
			if (typeof(IList).IsAssignableFrom(base.UnderlyingType))
			{
				return XamlCollectionKind.Collection;
			}
			if (typeof(DocumentReferenceCollection).IsAssignableFrom(base.UnderlyingType) || typeof(PageContentCollection).IsAssignableFrom(base.UnderlyingType))
			{
				return XamlCollectionKind.Collection;
			}
			if (typeof(ICollection<XmlNamespaceMapping>).IsAssignableFrom(base.UnderlyingType) && IsXmlNamespaceMappingCollection)
			{
				return XamlCollectionKind.Collection;
			}
			return XamlCollectionKind.None;
		}
		return base.LookupCollectionKind();
	}

	internal XamlMember FindBaseXamlMember(string name, bool isAttachable)
	{
		if (isAttachable)
		{
			return base.LookupAttachableMember(name);
		}
		return base.LookupMember(name, skipReadOnlyCheck: true);
	}

	internal static bool GetFlag(ref byte bitField, byte typeBit)
	{
		return (bitField & typeBit) != 0;
	}

	internal static void SetFlag(ref byte bitField, byte typeBit, bool value)
	{
		if (value)
		{
			bitField |= typeBit;
		}
		else
		{
			bitField = (byte)(bitField & ~typeBit);
		}
	}
}
