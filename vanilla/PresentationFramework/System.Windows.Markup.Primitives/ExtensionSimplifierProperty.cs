using System.Collections.Generic;
using System.Text;

namespace System.Windows.Markup.Primitives;

internal class ExtensionSimplifierProperty : MarkupPropertyWrapper
{
	private IValueSerializerContext _context;

	private const int EXTENSIONLENGTH = 9;

	public override bool IsComposite
	{
		get
		{
			if (!base.IsComposite)
			{
				return false;
			}
			if (base.IsCollectionProperty)
			{
				return true;
			}
			bool flag = true;
			foreach (MarkupObject item in Items)
			{
				if (!flag || !typeof(MarkupExtension).IsAssignableFrom(item.ObjectType))
				{
					return true;
				}
				flag = false;
				item.AssignRootContext(_context);
				foreach (MarkupProperty property in item.Properties)
				{
					if (property.IsComposite)
					{
						return true;
					}
				}
			}
			return flag;
		}
	}

	public override IEnumerable<MarkupObject> Items
	{
		get
		{
			foreach (MarkupObject baseItem in GetBaseItems())
			{
				ExtensionSimplifierMarkupObject extensionSimplifierMarkupObject = new ExtensionSimplifierMarkupObject(baseItem, _context);
				extensionSimplifierMarkupObject.AssignRootContext(_context);
				yield return extensionSimplifierMarkupObject;
			}
		}
	}

	public override string StringValue
	{
		get
		{
			string text = null;
			if (!base.IsComposite)
			{
				text = MarkupExtensionParser.AddEscapeToLiteralString(base.StringValue);
			}
			else
			{
				using (IEnumerator<MarkupObject> enumerator = Items.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						MarkupObject current = enumerator.Current;
						text = ConvertMarkupItemToString(current);
					}
				}
				if (text == null)
				{
					text = "";
				}
			}
			return text;
		}
	}

	public ExtensionSimplifierProperty(MarkupProperty baseProperty, IValueSerializerContext context)
		: base(baseProperty)
	{
		_context = context;
	}

	private IEnumerable<MarkupObject> GetBaseItems()
	{
		return base.Items;
	}

	private string ConvertMarkupItemToString(MarkupObject item)
	{
		ValueSerializer valueSerializerFor = _context.GetValueSerializerFor(typeof(Type));
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('{');
		string text = valueSerializerFor.ConvertToString(item.ObjectType, _context);
		if (text.EndsWith("Extension", StringComparison.Ordinal))
		{
			stringBuilder.Append(text, 0, text.Length - 9);
		}
		else
		{
			stringBuilder.Append(text);
		}
		bool flag = true;
		foreach (MarkupProperty property in item.Properties)
		{
			stringBuilder.Append(flag ? " " : ", ");
			flag = false;
			if (!property.IsConstructorArgument)
			{
				stringBuilder.Append(property.Name);
				stringBuilder.Append('=');
			}
			ReadOnlySpan<char> value = property.StringValue;
			if (value.IsEmpty)
			{
				continue;
			}
			if (value[0] == '{')
			{
				if (value.Length <= 1 || value[1] != '}')
				{
					stringBuilder.Append(value);
					continue;
				}
				value = value.Slice(2);
			}
			for (int i = 0; i < value.Length; i++)
			{
				char c = value[i];
				switch (c)
				{
				case '{':
					stringBuilder.Append("\\{");
					break;
				case '}':
					stringBuilder.Append("\\}");
					break;
				case ',':
					stringBuilder.Append("\\,");
					break;
				default:
					stringBuilder.Append(c);
					break;
				}
			}
		}
		stringBuilder.Append('}');
		return stringBuilder.ToString();
	}

	internal override void VerifyOnlySerializableTypes()
	{
		base.VerifyOnlySerializableTypes();
		if (!base.IsComposite)
		{
			return;
		}
		foreach (MarkupObject item in Items)
		{
			MarkupWriter.VerifyTypeIsSerializable(item.ObjectType);
			foreach (MarkupProperty property in item.Properties)
			{
				property.VerifyOnlySerializableTypes();
			}
		}
	}
}
