using System.CodeDom.Compiler;
using System.Collections;
using System.Runtime.CompilerServices;

namespace System.Text.RegularExpressions.Generated;

[GeneratedCode("System.Text.RegularExpressions.Generator", "8.0.10.36612")]
[SkipLocalsInit]
internal sealed class _003CRegexGenerator_g_003EF74B1AE921BCEFE4BA601AA541C7A23B1CA9711EA81E8FE504B5B6446748E035A__UnknownNodeObjectNullRegex_9 : Regex
{
	private sealed class RunnerFactory : RegexRunnerFactory
	{
		private sealed class Runner : RegexRunner
		{
			protected override void Scan(ReadOnlySpan<char> inputSpan)
			{
				while (TryFindNextPossibleStartingPosition(inputSpan) && !TryMatchAtCurrentPosition(inputSpan) && runtextpos != inputSpan.Length)
				{
					runtextpos++;
					if (_003CRegexGenerator_g_003EF74B1AE921BCEFE4BA601AA541C7A23B1CA9711EA81E8FE504B5B6446748E035A__Utilities.s_hasTimeout)
					{
						CheckTimeout();
					}
				}
			}

			private bool TryFindNextPossibleStartingPosition(ReadOnlySpan<char> inputSpan)
			{
				int num = runtextpos;
				if (num <= inputSpan.Length - 29)
				{
					int num2 = inputSpan.Slice(num).IndexOf("UnknownNode((object)");
					if (num2 >= 0)
					{
						runtextpos = num + num2;
						return true;
					}
				}
				runtextpos = inputSpan.Length;
				return false;
			}

			private bool TryMatchAtCurrentPosition(ReadOnlySpan<char> inputSpan)
			{
				int num = runtextpos;
				int start = num;
				int num2 = 0;
				ReadOnlySpan<char> span = inputSpan.Slice(num);
				if (!span.StartsWith("UnknownNode((object)"))
				{
					UncaptureUntil(0);
					return false;
				}
				num += 20;
				span = inputSpan.Slice(num);
				num2 = num;
				int num3 = span.IndexOf(',');
				if (num3 < 0)
				{
					num3 = span.Length;
				}
				if (num3 == 0)
				{
					UncaptureUntil(0);
					return false;
				}
				span = span.Slice(num3);
				num += num3;
				Capture(1, num2, num);
				if (!span.StartsWith(", null);"))
				{
					UncaptureUntil(0);
					return false;
				}
				Capture(0, start, runtextpos = num + 8);
				return true;
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				void UncaptureUntil(int capturePosition)
				{
					while (Crawlpos() > capturePosition)
					{
						Uncapture();
					}
				}
			}
		}

		protected override RegexRunner CreateInstance()
		{
			return new Runner();
		}
	}

	internal static readonly _003CRegexGenerator_g_003EF74B1AE921BCEFE4BA601AA541C7A23B1CA9711EA81E8FE504B5B6446748E035A__UnknownNodeObjectNullRegex_9 Instance = new _003CRegexGenerator_g_003EF74B1AE921BCEFE4BA601AA541C7A23B1CA9711EA81E8FE504B5B6446748E035A__UnknownNodeObjectNullRegex_9();

	private _003CRegexGenerator_g_003EF74B1AE921BCEFE4BA601AA541C7A23B1CA9711EA81E8FE504B5B6446748E035A__UnknownNodeObjectNullRegex_9()
	{
		pattern = "UnknownNode[(][(]object[)](?<o>[^,]+), null[)];";
		roptions = RegexOptions.None;
		Regex.ValidateMatchTimeout(_003CRegexGenerator_g_003EF74B1AE921BCEFE4BA601AA541C7A23B1CA9711EA81E8FE504B5B6446748E035A__Utilities.s_defaultTimeout);
		internalMatchTimeout = _003CRegexGenerator_g_003EF74B1AE921BCEFE4BA601AA541C7A23B1CA9711EA81E8FE504B5B6446748E035A__Utilities.s_defaultTimeout;
		factory = new RunnerFactory();
		base.CapNames = new Hashtable
		{
			{ "0", 0 },
			{ "o", 1 }
		};
		capslist = new string[2] { "0", "o" };
		capsize = 2;
	}
}
