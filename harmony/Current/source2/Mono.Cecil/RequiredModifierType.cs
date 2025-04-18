using System;
using Mono.Cecil.Metadata;

namespace Mono.Cecil;

internal sealed class RequiredModifierType : TypeSpecification, IModifierType
{
	private TypeReference modifier_type;

	public TypeReference ModifierType
	{
		get
		{
			return modifier_type;
		}
		set
		{
			modifier_type = value;
		}
	}

	public override string Name => base.Name + Suffix;

	public override string FullName => base.FullName + Suffix;

	private string Suffix => " modreq(" + modifier_type?.ToString() + ")";

	public override bool IsValueType
	{
		get
		{
			return false;
		}
		set
		{
			throw new InvalidOperationException();
		}
	}

	public override bool IsRequiredModifier => true;

	public override bool ContainsGenericParameter
	{
		get
		{
			if (!modifier_type.ContainsGenericParameter)
			{
				return base.ContainsGenericParameter;
			}
			return true;
		}
	}

	public RequiredModifierType(TypeReference modifierType, TypeReference type)
		: base(type)
	{
		if (modifierType == null)
		{
			throw new ArgumentNullException(Mixin.Argument.modifierType.ToString());
		}
		Mixin.CheckType(type);
		modifier_type = modifierType;
		etype = Mono.Cecil.Metadata.ElementType.CModReqD;
	}
}
