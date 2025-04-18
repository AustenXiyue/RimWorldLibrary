using System;
using MonoMod.Utils;

namespace MonoMod.Core.Platforms;

internal readonly record struct Abi(ReadOnlyMemory<SpecialArgumentKind> ArgumentOrder, Classifier Classifier, bool ReturnsReturnBuffer)
{
	public TypeClassification Classify(Type type, bool isReturn)
	{
		Helpers.ThrowIfArgumentNull(type, "type");
		if (type == typeof(void))
		{
			return TypeClassification.InRegister;
		}
		if (!type.IsValueType)
		{
			return TypeClassification.InRegister;
		}
		if (type.IsPointer)
		{
			return TypeClassification.InRegister;
		}
		if (type.IsByRef)
		{
			return TypeClassification.InRegister;
		}
		return Classifier(type, isReturn);
	}
}
