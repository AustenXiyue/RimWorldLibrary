using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Mono.Cecil;
using MonoMod.Utils;

namespace HarmonyLib;

internal class InlineSignature : ICallSiteGenerator
{
	public class ModifierType
	{
		public bool IsOptional;

		public Type Modifier;

		public object Type;

		public override string ToString()
		{
			return $"{((Type is Type type) ? type.FullDescription() : Type?.ToString())} mod{(IsOptional ? "opt" : "req")}({Modifier?.FullDescription()})";
		}

		internal TypeReference ToTypeReference(ModuleDefinition module)
		{
			if (IsOptional)
			{
				return new OptionalModifierType(module.ImportReference(Modifier), GetTypeReference(module, Type));
			}
			return new RequiredModifierType(module.ImportReference(Modifier), GetTypeReference(module, Type));
		}
	}

	public bool HasThis { get; set; }

	public bool ExplicitThis { get; set; }

	public CallingConvention CallingConvention { get; set; } = CallingConvention.Winapi;

	public List<object> Parameters { get; set; } = new List<object>();

	public object ReturnType { get; set; } = typeof(void);

	public override string ToString()
	{
		return ((ReturnType is Type type) ? type.FullDescription() : ReturnType?.ToString()) + " (" + Parameters.Join((object p) => (!(p is Type type2)) ? p?.ToString() : type2.FullDescription()) + ")";
	}

	internal static TypeReference GetTypeReference(ModuleDefinition module, object param)
	{
		if (!(param is Type type))
		{
			if (!(param is InlineSignature inlineSignature))
			{
				if (param is ModifierType modifierType)
				{
					return modifierType.ToTypeReference(module);
				}
				throw new NotSupportedException($"Unsupported inline signature parameter type: {param} ({param?.GetType().FullDescription()})");
			}
			return inlineSignature.ToFunctionPointer(module);
		}
		return module.ImportReference(type);
	}

	CallSite ICallSiteGenerator.ToCallSite(ModuleDefinition module)
	{
		CallSite callSite = new CallSite(GetTypeReference(module, ReturnType))
		{
			HasThis = HasThis,
			ExplicitThis = ExplicitThis,
			CallingConvention = (MethodCallingConvention)((byte)CallingConvention - 1)
		};
		foreach (object parameter in Parameters)
		{
			callSite.Parameters.Add(new ParameterDefinition(GetTypeReference(module, parameter)));
		}
		return callSite;
	}

	private FunctionPointerType ToFunctionPointer(ModuleDefinition module)
	{
		FunctionPointerType functionPointerType = new FunctionPointerType
		{
			ReturnType = GetTypeReference(module, ReturnType),
			HasThis = HasThis,
			ExplicitThis = ExplicitThis,
			CallingConvention = (MethodCallingConvention)((byte)CallingConvention - 1)
		};
		foreach (object parameter in Parameters)
		{
			functionPointerType.Parameters.Add(new ParameterDefinition(GetTypeReference(module, parameter)));
		}
		return functionPointerType;
	}
}
