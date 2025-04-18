using System.Reflection.Emit;

namespace System.Text.RegularExpressions;

internal sealed class CompiledRegexRunnerFactory : RegexRunnerFactory
{
	private DynamicMethod goMethod;

	private DynamicMethod findFirstCharMethod;

	private DynamicMethod initTrackCountMethod;

	internal CompiledRegexRunnerFactory(DynamicMethod go, DynamicMethod firstChar, DynamicMethod trackCount)
	{
		goMethod = go;
		findFirstCharMethod = firstChar;
		initTrackCountMethod = trackCount;
	}

	protected internal override RegexRunner CreateInstance()
	{
		CompiledRegexRunner compiledRegexRunner = new CompiledRegexRunner();
		compiledRegexRunner.SetDelegates((NoParamDelegate)goMethod.CreateDelegate(typeof(NoParamDelegate)), (FindFirstCharDelegate)findFirstCharMethod.CreateDelegate(typeof(FindFirstCharDelegate)), (NoParamDelegate)initTrackCountMethod.CreateDelegate(typeof(NoParamDelegate)));
		return compiledRegexRunner;
	}
}
