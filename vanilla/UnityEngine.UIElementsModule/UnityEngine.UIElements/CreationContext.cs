using System;
using System.Collections.Generic;

namespace UnityEngine.UIElements;

public struct CreationContext : IEquatable<CreationContext>
{
	public static readonly CreationContext Default = default(CreationContext);

	public VisualElement target { get; private set; }

	public VisualTreeAsset visualTreeAsset { get; private set; }

	public Dictionary<string, VisualElement> slotInsertionPoints { get; private set; }

	internal List<TemplateAsset.AttributeOverride> attributeOverrides { get; private set; }

	internal CreationContext(Dictionary<string, VisualElement> slotInsertionPoints, VisualTreeAsset vta, VisualElement target)
		: this(slotInsertionPoints, null, vta, target)
	{
	}

	internal CreationContext(Dictionary<string, VisualElement> slotInsertionPoints, List<TemplateAsset.AttributeOverride> attributeOverrides, VisualTreeAsset vta, VisualElement target)
	{
		this.target = target;
		this.slotInsertionPoints = slotInsertionPoints;
		this.attributeOverrides = attributeOverrides;
		visualTreeAsset = vta;
	}

	public override bool Equals(object obj)
	{
		return obj is CreationContext && Equals((CreationContext)obj);
	}

	public bool Equals(CreationContext other)
	{
		return EqualityComparer<VisualElement>.Default.Equals(target, other.target) && EqualityComparer<VisualTreeAsset>.Default.Equals(visualTreeAsset, other.visualTreeAsset) && EqualityComparer<Dictionary<string, VisualElement>>.Default.Equals(slotInsertionPoints, other.slotInsertionPoints);
	}

	public override int GetHashCode()
	{
		int num = -2123482148;
		num = num * -1521134295 + EqualityComparer<VisualElement>.Default.GetHashCode(target);
		num = num * -1521134295 + EqualityComparer<VisualTreeAsset>.Default.GetHashCode(visualTreeAsset);
		return num * -1521134295 + EqualityComparer<Dictionary<string, VisualElement>>.Default.GetHashCode(slotInsertionPoints);
	}

	public static bool operator ==(CreationContext context1, CreationContext context2)
	{
		return context1.Equals(context2);
	}

	public static bool operator !=(CreationContext context1, CreationContext context2)
	{
		return !(context1 == context2);
	}
}
