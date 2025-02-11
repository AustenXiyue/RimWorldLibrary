using System.Collections.Generic;

namespace System.Windows.Markup.Primitives;

internal class ExtensionSimplifierMarkupObject : MarkupObjectWrapper
{
	private IValueSerializerContext _context;

	public ExtensionSimplifierMarkupObject(MarkupObject baseObject, IValueSerializerContext context)
		: base(baseObject)
	{
		_context = context;
	}

	private IEnumerable<MarkupProperty> GetBaseProperties(bool mapToConstructorArgs)
	{
		return base.GetProperties(mapToConstructorArgs);
	}

	internal override IEnumerable<MarkupProperty> GetProperties(bool mapToConstructorArgs)
	{
		foreach (MarkupProperty baseProperty in GetBaseProperties(mapToConstructorArgs))
		{
			yield return new ExtensionSimplifierProperty(baseProperty, _context);
		}
	}

	public override void AssignRootContext(IValueSerializerContext context)
	{
		_context = context;
		base.AssignRootContext(context);
	}
}
