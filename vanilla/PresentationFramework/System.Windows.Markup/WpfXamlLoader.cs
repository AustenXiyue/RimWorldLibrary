using System.Collections;
using System.Windows.Baml2006;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Diagnostics;
using System.Windows.Navigation;
using System.Xaml;
using System.Xaml.Permissions;
using MS.Internal;
using MS.Internal.Xaml.Context;
using MS.Utility;

namespace System.Windows.Markup;

internal class WpfXamlLoader
{
	private static Lazy<XamlMember> XmlSpace = new Lazy<XamlMember>(() => new WpfXamlMember(XmlAttributeProperties.XmlSpaceProperty, isAttachable: true));

	public static object Load(System.Xaml.XamlReader xamlReader, bool skipJournaledProperties, Uri baseUri)
	{
		XamlObjectWriterSettings settings = XamlReader.CreateObjectWriterSettings();
		object obj = Load(xamlReader, null, skipJournaledProperties, null, settings, baseUri);
		EnsureXmlNamespaceMaps(obj, xamlReader.SchemaContext);
		return obj;
	}

	public static object LoadDeferredContent(System.Xaml.XamlReader xamlReader, IXamlObjectWriterFactory writerFactory, bool skipJournaledProperties, object rootObject, XamlObjectWriterSettings parentSettings, Uri baseUri)
	{
		XamlObjectWriterSettings settings = XamlReader.CreateObjectWriterSettings(parentSettings);
		return Load(xamlReader, writerFactory, skipJournaledProperties, rootObject, settings, baseUri);
	}

	public static object LoadBaml(System.Xaml.XamlReader xamlReader, bool skipJournaledProperties, object rootObject, XamlAccessLevel accessLevel, Uri baseUri)
	{
		XamlObjectWriterSettings xamlObjectWriterSettings = XamlReader.CreateObjectWriterSettingsForBaml();
		xamlObjectWriterSettings.RootObjectInstance = rootObject;
		xamlObjectWriterSettings.AccessLevel = accessLevel;
		object obj = Load(xamlReader, null, skipJournaledProperties, rootObject, xamlObjectWriterSettings, baseUri);
		EnsureXmlNamespaceMaps(obj, xamlReader.SchemaContext);
		return obj;
	}

	internal static void EnsureXmlNamespaceMaps(object rootObject, XamlSchemaContext schemaContext)
	{
		if (rootObject is DependencyObject dependencyObject)
		{
			dependencyObject.SetValue(value: (schemaContext is XamlTypeMapper.XamlTypeMapperSchemaContext xamlTypeMapperSchemaContext) ? xamlTypeMapperSchemaContext.GetNamespaceMapHashList() : new Hashtable(), dp: XmlAttributeProperties.XmlNamespaceMapsProperty);
		}
	}

	private static object Load(System.Xaml.XamlReader xamlReader, IXamlObjectWriterFactory writerFactory, bool skipJournaledProperties, object rootObject, XamlObjectWriterSettings settings, Uri baseUri)
	{
		XamlObjectWriter xamlObjectWriter = null;
		MS.Internal.Xaml.Context.XamlContextStack<WpfXamlFrame> stack = new MS.Internal.Xaml.Context.XamlContextStack<WpfXamlFrame>(() => new WpfXamlFrame());
		int persistId = 1;
		settings.AfterBeginInitHandler = delegate(object sender, XamlObjectEventArgs args)
		{
			if (EventTrace.IsEnabled(EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordXamlBaml, EventTrace.Level.Verbose))
			{
				IXamlLineInfo xamlLineInfo = xamlReader as IXamlLineInfo;
				int num = -1;
				int num2 = -1;
				if (xamlLineInfo != null && xamlLineInfo.HasLineInfo)
				{
					num = xamlLineInfo.LineNumber;
					num2 = xamlLineInfo.LinePosition;
				}
				EventTrace.EventProvider.TraceEvent(EventTrace.Event.WClientParseXamlBamlInfo, EventTrace.Keyword.KeywordPerf | EventTrace.Keyword.KeywordXamlBaml, EventTrace.Level.Verbose, (args.Instance == null) ? 0 : PerfService.GetPerfElementID(args.Instance), num, num2);
			}
			if (args.Instance is UIElement uIElement)
			{
				uIElement.SetPersistId(persistId++);
			}
			XamlSourceInfoHelper.SetXamlSourceInfo(args.Instance, args, baseUri);
			if (args.Instance is DependencyObject dependencyObject && stack.CurrentFrame.XmlnsDictionary != null)
			{
				XmlnsDictionary xmlnsDictionary = stack.CurrentFrame.XmlnsDictionary;
				xmlnsDictionary.Seal();
				XmlAttributeProperties.SetXmlnsDictionary(dependencyObject, xmlnsDictionary);
			}
			stack.CurrentFrame.Instance = args.Instance;
			if (xamlReader is RestrictiveXamlXmlReader && args != null)
			{
				if (args.Instance is ResourceDictionary resourceDictionary)
				{
					resourceDictionary.IsUnsafe = true;
				}
				else if (args.Instance is Frame frame)
				{
					frame.NavigationService.IsUnsafe = true;
				}
				else if (args.Instance is NavigationWindow navigationWindow)
				{
					navigationWindow.NavigationService.IsUnsafe = true;
				}
			}
		};
		xamlObjectWriter = ((writerFactory == null) ? new XamlObjectWriter(xamlReader.SchemaContext, settings) : writerFactory.GetXamlObjectWriter(settings));
		IXamlLineInfo xamlLineInfo2 = null;
		try
		{
			xamlLineInfo2 = xamlReader as IXamlLineInfo;
			IXamlLineInfoConsumer xamlLineInfoConsumer = xamlObjectWriter;
			bool shouldPassLineNumberInfo = false;
			if (xamlLineInfo2 != null && xamlLineInfo2.HasLineInfo && xamlLineInfoConsumer != null && xamlLineInfoConsumer.ShouldProvideLineInfo)
			{
				shouldPassLineNumberInfo = true;
			}
			IStyleConnector styleConnector = rootObject as IStyleConnector;
			TransformNodes(xamlReader, xamlObjectWriter, onlyLoadOneNode: false, skipJournaledProperties, shouldPassLineNumberInfo, xamlLineInfo2, xamlLineInfoConsumer, stack, styleConnector);
			xamlObjectWriter.Close();
			return xamlObjectWriter.Result;
		}
		catch (Exception ex)
		{
			if (CriticalExceptions.IsCriticalException(ex) || !XamlReader.ShouldReWrapException(ex, baseUri))
			{
				throw;
			}
			XamlReader.RewrapException(ex, xamlLineInfo2, baseUri);
			return null;
		}
	}

	internal static void TransformNodes(System.Xaml.XamlReader xamlReader, XamlObjectWriter xamlWriter, bool onlyLoadOneNode, bool skipJournaledProperties, bool shouldPassLineNumberInfo, IXamlLineInfo xamlLineInfo, IXamlLineInfoConsumer xamlLineInfoConsumer, MS.Internal.Xaml.Context.XamlContextStack<WpfXamlFrame> stack, IStyleConnector styleConnector)
	{
		while (xamlReader.Read())
		{
			if (shouldPassLineNumberInfo && xamlLineInfo.LineNumber != 0)
			{
				xamlLineInfoConsumer.SetLineInfo(xamlLineInfo.LineNumber, xamlLineInfo.LinePosition);
			}
			switch (xamlReader.NodeType)
			{
			case System.Xaml.XamlNodeType.NamespaceDeclaration:
				xamlWriter.WriteNode(xamlReader);
				if (stack.Depth == 0 || stack.CurrentFrame.Type != null)
				{
					stack.PushScope();
					for (WpfXamlFrame wpfXamlFrame = stack.CurrentFrame; wpfXamlFrame != null; wpfXamlFrame = (WpfXamlFrame)wpfXamlFrame.Previous)
					{
						if (wpfXamlFrame.XmlnsDictionary != null)
						{
							stack.CurrentFrame.XmlnsDictionary = new XmlnsDictionary(wpfXamlFrame.XmlnsDictionary);
							break;
						}
					}
					if (stack.CurrentFrame.XmlnsDictionary == null)
					{
						stack.CurrentFrame.XmlnsDictionary = new XmlnsDictionary();
					}
				}
				stack.CurrentFrame.XmlnsDictionary.Add(xamlReader.Namespace.Prefix, xamlReader.Namespace.Namespace);
				break;
			case System.Xaml.XamlNodeType.StartObject:
				WriteStartObject(xamlReader, xamlWriter, stack);
				break;
			case System.Xaml.XamlNodeType.GetObject:
				xamlWriter.WriteNode(xamlReader);
				if (stack.CurrentFrame.Type != null)
				{
					stack.PushScope();
				}
				stack.CurrentFrame.Type = stack.PreviousFrame.Property.Type;
				break;
			case System.Xaml.XamlNodeType.EndObject:
				xamlWriter.WriteNode(xamlReader);
				if (stack.CurrentFrame.FreezeFreezable && xamlWriter.Result is Freezable { CanFreeze: not false } freezable)
				{
					freezable.Freeze();
				}
				if (xamlWriter.Result is DependencyObject dependencyObject && stack.CurrentFrame.XmlSpace.HasValue)
				{
					XmlAttributeProperties.SetXmlSpace(dependencyObject, stack.CurrentFrame.XmlSpace.Value ? "default" : "preserve");
				}
				stack.PopScope();
				break;
			case System.Xaml.XamlNodeType.StartMember:
			{
				if ((!xamlReader.Member.IsDirective || !(xamlReader.Member == XamlReaderHelper.Freeze)) && xamlReader.Member != XmlSpace.Value && xamlReader.Member != XamlLanguage.Space)
				{
					xamlWriter.WriteNode(xamlReader);
				}
				stack.CurrentFrame.Property = xamlReader.Member;
				if (!skipJournaledProperties || stack.CurrentFrame.Property.IsDirective)
				{
					break;
				}
				WpfXamlMember wpfXamlMember = stack.CurrentFrame.Property as WpfXamlMember;
				if (!(wpfXamlMember != null))
				{
					break;
				}
				DependencyProperty dependencyProperty = wpfXamlMember.DependencyProperty;
				if (dependencyProperty == null || !(dependencyProperty.GetMetadata(stack.CurrentFrame.Type.UnderlyingType) is FrameworkPropertyMetadata { Journal: not false }))
				{
					break;
				}
				int num = 1;
				while (xamlReader.Read())
				{
					switch (xamlReader.NodeType)
					{
					case System.Xaml.XamlNodeType.StartMember:
						num++;
						break;
					case System.Xaml.XamlNodeType.StartObject:
					{
						XamlType type = xamlReader.Type;
						XamlType xamlType = type.SchemaContext.GetXamlType(typeof(BindingBase));
						XamlType xamlType2 = type.SchemaContext.GetXamlType(typeof(DynamicResourceExtension));
						if (num == 1 && (type.CanAssignTo(xamlType) || type.CanAssignTo(xamlType2)))
						{
							num = 0;
							WriteStartObject(xamlReader, xamlWriter, stack);
						}
						break;
					}
					case System.Xaml.XamlNodeType.EndMember:
						num--;
						if (num == 0)
						{
							xamlWriter.WriteNode(xamlReader);
							stack.CurrentFrame.Property = null;
						}
						break;
					case System.Xaml.XamlNodeType.Value:
						if (xamlReader.Value is DynamicResourceExtension)
						{
							WriteValue(xamlReader, xamlWriter, stack, styleConnector);
						}
						break;
					}
					if (num == 0)
					{
						break;
					}
				}
				break;
			}
			case System.Xaml.XamlNodeType.EndMember:
			{
				WpfXamlFrame currentFrame = stack.CurrentFrame;
				XamlMember property = currentFrame.Property;
				if ((!property.IsDirective || !(property == XamlReaderHelper.Freeze)) && property != XmlSpace.Value && property != XamlLanguage.Space)
				{
					xamlWriter.WriteNode(xamlReader);
				}
				currentFrame.Property = null;
				break;
			}
			case System.Xaml.XamlNodeType.Value:
				WriteValue(xamlReader, xamlWriter, stack, styleConnector);
				break;
			default:
				xamlWriter.WriteNode(xamlReader);
				break;
			}
			if (onlyLoadOneNode)
			{
				break;
			}
		}
	}

	private static void WriteValue(System.Xaml.XamlReader xamlReader, XamlObjectWriter xamlWriter, MS.Internal.Xaml.Context.XamlContextStack<WpfXamlFrame> stack, IStyleConnector styleConnector)
	{
		if (stack.CurrentFrame.Property.IsDirective && stack.CurrentFrame.Property == XamlLanguage.Shared && bool.TryParse(xamlReader.Value as string, out var result) && !result && !(xamlReader is Baml2006Reader))
		{
			throw new XamlParseException(SR.SharedAttributeInLooseXaml);
		}
		if (stack.CurrentFrame.Property.IsDirective && stack.CurrentFrame.Property == XamlReaderHelper.Freeze)
		{
			bool flag = Convert.ToBoolean(xamlReader.Value, TypeConverterHelper.InvariantEnglishUS);
			stack.CurrentFrame.FreezeFreezable = flag;
			if (xamlReader is Baml2006Reader baml2006Reader)
			{
				baml2006Reader.FreezeFreezables = flag;
			}
		}
		else if (stack.CurrentFrame.Property == XmlSpace.Value || stack.CurrentFrame.Property == XamlLanguage.Space)
		{
			if (typeof(DependencyObject).IsAssignableFrom(stack.CurrentFrame.Type.UnderlyingType))
			{
				stack.CurrentFrame.XmlSpace = (string)xamlReader.Value == "default";
			}
		}
		else
		{
			if (styleConnector != null && stack.CurrentFrame.Instance != null && stack.CurrentFrame.Property == XamlLanguage.ConnectionId && typeof(Style).IsAssignableFrom(stack.CurrentFrame.Type.UnderlyingType))
			{
				styleConnector.Connect((int)xamlReader.Value, stack.CurrentFrame.Instance);
			}
			xamlWriter.WriteNode(xamlReader);
		}
	}

	private static void WriteStartObject(System.Xaml.XamlReader xamlReader, XamlObjectWriter xamlWriter, MS.Internal.Xaml.Context.XamlContextStack<WpfXamlFrame> stack)
	{
		xamlWriter.WriteNode(xamlReader);
		if (stack.Depth != 0 && stack.CurrentFrame.Type == null)
		{
			stack.CurrentFrame.Type = xamlReader.Type;
			return;
		}
		stack.PushScope();
		stack.CurrentFrame.Type = xamlReader.Type;
		if (stack.PreviousFrame.FreezeFreezable)
		{
			stack.CurrentFrame.FreezeFreezable = true;
		}
	}
}
