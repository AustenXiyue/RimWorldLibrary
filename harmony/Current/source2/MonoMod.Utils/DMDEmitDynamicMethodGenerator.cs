using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Mono.Cecil;
using MonoMod.Logs;

namespace MonoMod.Utils;

internal sealed class DMDEmitDynamicMethodGenerator : DMDGenerator<DMDEmitDynamicMethodGenerator>
{
	private static readonly FieldInfo _DynamicMethod_returnType = typeof(DynamicMethod).GetField("returnType", BindingFlags.Instance | BindingFlags.NonPublic) ?? typeof(DynamicMethod).GetField("_returnType", BindingFlags.Instance | BindingFlags.NonPublic) ?? typeof(DynamicMethod).GetField("m_returnType", BindingFlags.Instance | BindingFlags.NonPublic) ?? throw new InvalidOperationException("Cannot find returnType field on DynamicMethod");

	protected override MethodInfo GenerateCore(DynamicMethodDefinition dmd, object? context)
	{
		MethodBase originalMethod = dmd.OriginalMethod;
		MethodDefinition methodDefinition = dmd.Definition ?? throw new InvalidOperationException();
		Type[] array;
		if (originalMethod != null)
		{
			ParameterInfo[] parameters = originalMethod.GetParameters();
			int num = 0;
			if (!originalMethod.IsStatic)
			{
				num++;
				array = new Type[parameters.Length + 1];
				array[0] = originalMethod.GetThisParamType();
			}
			else
			{
				array = new Type[parameters.Length];
			}
			for (int i = 0; i < parameters.Length; i++)
			{
				array[i + num] = parameters[i].ParameterType;
			}
		}
		else
		{
			int num2 = 0;
			if (methodDefinition.HasThis)
			{
				num2++;
				array = new Type[methodDefinition.Parameters.Count + 1];
				Type type2 = methodDefinition.DeclaringType.ResolveReflection();
				if (type2.IsValueType)
				{
					type2 = type2.MakeByRefType();
				}
				array[0] = type2;
			}
			else
			{
				array = new Type[methodDefinition.Parameters.Count];
			}
			for (int j = 0; j < methodDefinition.Parameters.Count; j++)
			{
				array[j + num2] = methodDefinition.Parameters[j].ParameterType.ResolveReflection();
			}
		}
		string text = dmd.Name;
		if (text == null)
		{
			FormatInterpolatedStringHandler handler = new FormatInterpolatedStringHandler(5, 1);
			handler.AppendLiteral("DMD<");
			handler.AppendFormatted(((object)originalMethod) ?? ((object)methodDefinition.GetID(null, null, withType: true, simple: true)));
			handler.AppendLiteral(">");
			text = DebugFormatter.Format(ref handler);
		}
		string text2 = text;
		Type value = (originalMethod as MethodInfo)?.ReturnType ?? methodDefinition.ReturnType.ResolveReflection();
		bool isEnabled;
		MMDbgLog.DebugLogTraceStringHandler message = new MMDbgLog.DebugLogTraceStringHandler(22, 3, out isEnabled);
		if (isEnabled)
		{
			message.AppendLiteral("new DynamicMethod: ");
			message.AppendFormatted(value);
			message.AppendLiteral(" ");
			message.AppendFormatted(text2);
			message.AppendLiteral("(");
			message.AppendFormatted(string.Join(",", array.Select((Type type) => type?.ToString()).ToArray()));
			message.AppendLiteral(")");
		}
		MMDbgLog.Trace(ref message);
		if (originalMethod != null)
		{
			message = new MMDbgLog.DebugLogTraceStringHandler(6, 1, out isEnabled);
			if (isEnabled)
			{
				message.AppendLiteral("orig: ");
				message.AppendFormatted(originalMethod);
			}
			MMDbgLog.Trace(ref message);
		}
		message = new MMDbgLog.DebugLogTraceStringHandler(9, 3, out isEnabled);
		if (isEnabled)
		{
			message.AppendLiteral("mdef: ");
			message.AppendFormatted(methodDefinition.ReturnType?.ToString() ?? "NULL");
			message.AppendLiteral(" ");
			message.AppendFormatted(text2);
			message.AppendLiteral("(");
			message.AppendFormatted(string.Join(",", methodDefinition.Parameters.Select((ParameterDefinition arg) => arg?.ParameterType?.ToString() ?? "NULL").ToArray()));
			message.AppendLiteral(")");
		}
		MMDbgLog.Trace(ref message);
		DynamicMethod dynamicMethod = new DynamicMethod(text2, typeof(void), array, originalMethod?.DeclaringType ?? typeof(DynamicMethodDefinition), skipVisibility: true);
		_DynamicMethod_returnType.SetValue(dynamicMethod, value);
		ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
		_DMDEmit.Generate(dmd, dynamicMethod, iLGenerator);
		return dynamicMethod;
	}
}
