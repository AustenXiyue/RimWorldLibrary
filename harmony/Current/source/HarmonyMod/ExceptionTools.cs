using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using HarmonyLib;

namespace HarmonyMod;

internal static class ExceptionTools
{
	private delegate void GetFullNameForStackTraceDelegate(StackTrace instance, StringBuilder sb, MethodBase mi);

	private delegate uint GetMethodIndexDelegate(StackFrame instance);

	private delegate string GetSecureFileNameDelegate(StackFrame instance);

	private delegate string GetAotIdDelegate();

	private static readonly AccessTools.FieldRef<StackTrace, StackTrace[]> captured_traces = AccessTools.FieldRefAccess<StackTrace, StackTrace[]>("captured_traces");

	private static readonly AccessTools.FieldRef<StackFrame, string> internalMethodName = AccessTools.FieldRefAccess<StackFrame, string>("internalMethodName");

	private static readonly AccessTools.FieldRef<StackFrame, long> methodAddress = AccessTools.FieldRefAccess<StackFrame, long>("methodAddress");

	private static readonly MethodInfo m_GetFullNameForStackTrace = AccessTools.Method(typeof(StackTrace), "GetFullNameForStackTrace");

	private static readonly GetFullNameForStackTraceDelegate GetFullNameForStackTrace = AccessTools.MethodDelegate<GetFullNameForStackTraceDelegate>(m_GetFullNameForStackTrace);

	private static readonly MethodInfo m_GetMethodIndex = AccessTools.Method(typeof(StackFrame), "GetMethodIndex");

	private static readonly GetMethodIndexDelegate GetMethodIndex = AccessTools.MethodDelegate<GetMethodIndexDelegate>(m_GetMethodIndex);

	private static readonly MethodInfo m_GetSecureFileName = AccessTools.Method(typeof(StackFrame), "GetSecureFileName");

	private static readonly GetSecureFileNameDelegate GetSecureFileName = AccessTools.MethodDelegate<GetSecureFileNameDelegate>(m_GetSecureFileName);

	private static readonly MethodInfo m_GetAotId = AccessTools.Method(typeof(StackTrace), "GetAotId");

	private static readonly GetAotIdDelegate GetAotId = AccessTools.MethodDelegate<GetAotIdDelegate>(m_GetAotId);

	internal static readonly ConcurrentDictionary<int, int> seenStacktraces = new ConcurrentDictionary<int, int>();

	internal static string ExtractHarmonyEnhancedStackTrace(StackTrace trace, bool forceRefresh, out int hash)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StackTrace[] array = captured_traces(trace);
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (stringBuilder.AddHarmonyFrames(array[i]))
				{
					stringBuilder.Append("\n--- End of stack trace from previous location where exception was thrown ---\n");
				}
			}
		}
		stringBuilder.AddHarmonyFrames(trace);
		string text = stringBuilder.ToString();
		hash = text.GetHashCode();
		if (HarmonyMain.noStacktraceCaching)
		{
			return text;
		}
		string text2 = $"[Ref {hash:X}]";
		if (forceRefresh)
		{
			return text2 + "\n" + text;
		}
		if (seenStacktraces.AddOrUpdate(hash, 1, (int k, int v) => v + 1) > 1)
		{
			return text2 + " Duplicate stacktrace, see ref for original";
		}
		return text2 + "\n" + text;
	}

	private static bool AddHarmonyFrames(this StringBuilder sb, StackTrace trace)
	{
		if (trace.FrameCount == 0)
		{
			return false;
		}
		for (int i = 0; i < trace.FrameCount; i++)
		{
			StackFrame frame = trace.GetFrame(i);
			if (i > 0)
			{
				sb.Append('\n');
			}
			sb.Append(" at ");
			MethodBase originalMethodFromStackframe = Harmony.GetOriginalMethodFromStackframe(frame);
			if (originalMethodFromStackframe == null)
			{
				string text = internalMethodName(frame);
				if (text != null)
				{
					sb.Append(text);
				}
				else
				{
					sb.AppendFormat("<0x{0:x5} + 0x{1:x5}> <unknown method>", methodAddress(frame), frame.GetNativeOffset());
				}
				continue;
			}
			GetFullNameForStackTrace(trace, sb, originalMethodFromStackframe);
			if (frame.GetILOffset() == -1)
			{
				sb.AppendFormat(" <0x{0:x5} + 0x{1:x5}>", methodAddress(frame), frame.GetNativeOffset());
				if (GetMethodIndex(frame) != 16777215)
				{
					sb.AppendFormat(" {0}", GetMethodIndex(frame));
				}
			}
			else
			{
				sb.AppendFormat(" [0x{0:x5}]", frame.GetILOffset());
			}
			string text2 = GetSecureFileName(frame);
			if (text2[0] == '<')
			{
				string arg = originalMethodFromStackframe.Module.ModuleVersionId.ToString("N");
				string text3 = GetAotId();
				text2 = ((frame.GetILOffset() == -1 && text3 != null) ? $"<{arg}#{text3}>" : $"<{arg}>");
			}
			sb.AppendFormat(" in {0}:{1} ", text2, frame.GetFileLineNumber());
			Patches patchInfo = Harmony.GetPatchInfo(originalMethodFromStackframe);
			if (patchInfo != null)
			{
				sb.AppendPatch(originalMethodFromStackframe, patchInfo.Transpilers, "TRANSPILER");
				sb.AppendPatch(originalMethodFromStackframe, patchInfo.Prefixes, "PREFIX");
				sb.AppendPatch(originalMethodFromStackframe, patchInfo.Postfixes, "POSTFIX");
				sb.AppendPatch(originalMethodFromStackframe, patchInfo.Finalizers, "FINALIZER");
			}
		}
		return true;
	}

	private static void AppendPatch(this StringBuilder sb, MethodBase method, IEnumerable<Patch> fixes, string name)
	{
		foreach (MethodInfo patch in PatchProcessor.GetSortedPatchMethods(method, fixes.ToArray()))
		{
			string owner = fixes.First((Patch p) => p.PatchMethod == patch).owner;
			string text = patch.GetParameters().Join((ParameterInfo p) => p.ParameterType.Name + " " + p.Name);
			sb.AppendFormat("\n     - {0} {1}: {2} {3}:{4}({5})", name, owner, patch.ReturnType.Name, patch.DeclaringType.FullName, patch.Name, text);
		}
	}
}
