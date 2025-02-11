using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.Jobs.LowLevel.Unsafe;

namespace Unity.Burst;

public sealed class BurstCompilerOptions
{
	private const string DisableCompilationArg = "--burst-disable-compilation";

	private const string ForceSynchronousCompilationArg = "--burst-force-sync-compilation";

	internal const string DefaultLibraryName = "lib_burst_generated";

	internal const string BurstInitializeName = "burst.initialize";

	internal const string OptionDoNotEagerCompile = "do-not-eager-compile";

	internal const string DoNotEagerCompile = "--do-not-eager-compile";

	internal const string OptionGroup = "group";

	internal const string OptionPlatform = "platform=";

	internal const string OptionBackend = "backend=";

	internal const string OptionSafetyChecks = "safety-checks";

	internal const string OptionDisableSafetyChecks = "disable-safety-checks";

	internal const string OptionDisableOpt = "disable-opt";

	internal const string OptionFastMath = "fastmath";

	internal const string OptionTarget = "target=";

	internal const string OptionIROpt = "ir-opt";

	internal const string OptionCpuOpt = "cpu-opt=";

	internal const string OptionFloatPrecision = "float-precision=";

	internal const string OptionFloatMode = "float-mode=";

	internal const string OptionDisableWarnings = "disable-warnings=";

	internal const string OptionCompilationDefines = "compilation-defines=";

	internal const string OptionDump = "dump=";

	internal const string OptionFormat = "format=";

	internal const string OptionDebugTrap = "debugtrap";

	internal const string OptionDisableVectors = "disable-vectors";

	internal const string OptionDebug = "debug=";

	internal const string OptionDebugMode = "debugMode";

	internal const string OptionStaticLinkage = "generate-static-linkage-methods";

	internal const string OptionJobMarshalling = "generate-job-marshalling-methods";

	internal const string OptionTempDirectory = "temp-folder=";

	internal const string OptionEnableDirectExternalLinking = "enable-direct-external-linking";

	internal const string OptionLinkerOptions = "linker-options=";

	internal const string OptionCacheDirectory = "cache-directory=";

	internal const string OptionJitDisableFunctionCaching = "disable-function-caching";

	internal const string OptionJitDisableAssemblyCaching = "disable-assembly-caching";

	internal const string OptionJitEnableAssemblyCachingLogs = "enable-assembly-caching-logs";

	internal const string OptionJitEnableSynchronousCompilation = "enable-synchronous-compilation";

	internal const string OptionJitCompilationPriority = "compilation-priority=";

	internal const string OptionJitLogTimings = "log-timings";

	internal const string OptionJitIsForFunctionPointer = "is-for-function-pointer";

	internal const string OptionJitManagedFunctionPointer = "managed-function-pointer=";

	internal const string OptionJitProvider = "jit-provider=";

	internal const string OptionJitSkipCheckDiskCache = "skip-check-disk-cache";

	internal const string OptionJitSkipBurstInitialize = "skip-burst-initialize";

	internal const string OptionAotAssemblyFolder = "assembly-folder=";

	internal const string OptionRootAssembly = "root-assembly=";

	internal const string OptionIncludeRootAssemblyReferences = "include-root-assembly-references=";

	internal const string OptionAotMethod = "method=";

	internal const string OptionAotType = "type=";

	internal const string OptionAotAssembly = "assembly=";

	internal const string OptionAotOutputPath = "output=";

	internal const string OptionAotKeepIntermediateFiles = "keep-intermediate-files";

	internal const string OptionAotNoLink = "nolink";

	internal const string OptionAotPatchedAssembliesOutputFolder = "patch-assemblies-into=";

	internal const string OptionAotPinvokeNameToPatch = "pinvoke-name=";

	internal const string OptionAotExecuteMethodNameToFind = "execute-method-name=";

	internal const string OptionAotUsePlatformSDKLinkers = "use-platform-sdk-linkers";

	internal const string OptionAotOnlyStaticMethods = "only-static-methods";

	internal const string OptionMethodPrefix = "method-prefix=";

	internal const string OptionAotNoNativeToolchain = "no-native-toolchain";

	internal const string OptionAotKeyFolder = "key-folder=";

	internal const string OptionAotDecodeFolder = "decode-folder=";

	internal const string OptionVerbose = "verbose";

	internal const string OptionValidateExternalToolChain = "validate-external-tool-chain";

	internal const string OptionCompilerThreads = "threads=";

	internal const string OptionChunkSize = "chunk-size=";

	internal const string OptionPrintLogOnMissingPInvokeCallbackAttribute = "print-monopinvokecallbackmissing-message";

	internal const string OptionOutputMode = "output-mode=";

	internal const string OptionAlwaysCreateOutput = "always-create-output=";

	internal const string OptionAotPdbSearchPaths = "pdb-search-paths=";

	internal const string CompilerCommandShutdown = "$shutdown";

	internal const string CompilerCommandCancel = "$cancel";

	internal const string CompilerCommandEnableCompiler = "$enable_compiler";

	internal const string CompilerCommandDisableCompiler = "$disable_compiler";

	internal const string CompilerCommandTriggerRecompilation = "$trigger_recompilation";

	internal const string CompilerCommandEagerCompileMethods = "$eager_compile_methods";

	internal const string CompilerCommandWaitUntilCompilationFinished = "$wait_until_compilation_finished";

	internal const string CompilerCommandClearEagerCompilationQueues = "$clear_eager_compilation_queues";

	internal const string CompilerCommandCancelEagerCompilation = "$cancel_eager_compilation";

	internal const string CompilerCommandReset = "$reset";

	internal const string CompilerCommandDomainReload = "$domain_reload";

	internal const string CompilerCommandUpdateAssemblyFolders = "$update_assembly_folders";

	internal const string CompilerCommandVersionNotification = "$version";

	internal const string CompilerCommandSetProgressCallback = "$set_progress_callback";

	internal const string CompilerCommandRequestClearJitCache = "$request_clear_jit_cache";

	internal static readonly bool ForceDisableBurstCompilation;

	private static readonly bool ForceBurstCompilationSynchronously;

	private bool _enableBurstCompilation;

	private bool _enableBurstCompileSynchronously;

	private bool _enableBurstSafetyChecks;

	private bool _enableBurstTimings;

	private bool _enableBurstDebug;

	private bool _forceEnableBurstSafetyChecks;

	private bool IsGlobal { get; }

	public bool IsEnabled
	{
		get
		{
			if (EnableBurstCompilation)
			{
				return !ForceDisableBurstCompilation;
			}
			return false;
		}
	}

	public bool EnableBurstCompilation
	{
		get
		{
			return _enableBurstCompilation;
		}
		set
		{
			if (IsGlobal && ForceDisableBurstCompilation)
			{
				value = false;
			}
			bool num = _enableBurstCompilation != value;
			if (num && value)
			{
				MaybePreventChangingOption();
			}
			_enableBurstCompilation = value;
			if (IsGlobal)
			{
				JobsUtility.JobCompilerEnabled = value;
				BurstCompiler._IsEnabled = value;
			}
			if (num)
			{
				OnOptionsChanged();
			}
		}
	}

	public bool EnableBurstCompileSynchronously
	{
		get
		{
			return _enableBurstCompileSynchronously;
		}
		set
		{
			bool num = _enableBurstCompileSynchronously != value;
			_enableBurstCompileSynchronously = value;
			if (num)
			{
				OnOptionsChanged();
			}
		}
	}

	public bool EnableBurstSafetyChecks
	{
		get
		{
			return _enableBurstSafetyChecks;
		}
		set
		{
			bool num = _enableBurstSafetyChecks != value;
			if (num)
			{
				MaybePreventChangingOption();
			}
			_enableBurstSafetyChecks = value;
			if (num)
			{
				OnOptionsChanged();
				MaybeTriggerRecompilation();
			}
		}
	}

	public bool ForceEnableBurstSafetyChecks
	{
		get
		{
			return _forceEnableBurstSafetyChecks;
		}
		set
		{
			bool num = _forceEnableBurstSafetyChecks != value;
			if (num)
			{
				MaybePreventChangingOption();
			}
			_forceEnableBurstSafetyChecks = value;
			if (num)
			{
				OnOptionsChanged();
				MaybeTriggerRecompilation();
			}
		}
	}

	public bool EnableBurstDebug
	{
		get
		{
			return _enableBurstDebug;
		}
		set
		{
			bool num = _enableBurstDebug != value;
			if (num)
			{
				MaybePreventChangingOption();
			}
			_enableBurstDebug = value;
			if (num)
			{
				OnOptionsChanged();
				MaybeTriggerRecompilation();
			}
		}
	}

	[Obsolete("This property is no longer used and will be removed in a future major release")]
	public bool DisableOptimizations
	{
		get
		{
			return false;
		}
		set
		{
		}
	}

	[Obsolete("This property is no longer used and will be removed in a future major release. Use the [BurstCompile(FloatMode = FloatMode.Fast)] on the method directly to enable this feature")]
	public bool EnableFastMath
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	internal bool EnableBurstTimings
	{
		get
		{
			return _enableBurstTimings;
		}
		set
		{
			bool num = _enableBurstTimings != value;
			_enableBurstTimings = value;
			if (num)
			{
				OnOptionsChanged();
			}
		}
	}

	internal bool RequiresSynchronousCompilation
	{
		get
		{
			if (!EnableBurstCompileSynchronously)
			{
				return ForceBurstCompilationSynchronously;
			}
			return true;
		}
	}

	internal Action OptionsChanged { get; set; }

	private BurstCompilerOptions()
		: this(isGlobal: false)
	{
	}

	internal BurstCompilerOptions(bool isGlobal)
	{
		IsGlobal = isGlobal;
		EnableBurstCompilation = true;
		EnableBurstSafetyChecks = true;
	}

	internal BurstCompilerOptions Clone()
	{
		return new BurstCompilerOptions
		{
			EnableBurstCompilation = EnableBurstCompilation,
			EnableBurstCompileSynchronously = EnableBurstCompileSynchronously,
			EnableBurstSafetyChecks = EnableBurstSafetyChecks,
			EnableBurstTimings = EnableBurstTimings,
			EnableBurstDebug = EnableBurstDebug,
			ForceEnableBurstSafetyChecks = ForceEnableBurstSafetyChecks
		};
	}

	private static bool TryGetAttribute(MemberInfo member, out BurstCompileAttribute attribute, bool isForEagerCompilation = false)
	{
		attribute = null;
		if (member == null)
		{
			return false;
		}
		attribute = GetBurstCompileAttribute(member);
		if (attribute == null)
		{
			return false;
		}
		if (isForEagerCompilation)
		{
			string[] options = attribute.Options;
			if (options != null && options.Contains("--do-not-eager-compile"))
			{
				return false;
			}
		}
		return true;
	}

	private static BurstCompileAttribute GetBurstCompileAttribute(MemberInfo memberInfo)
	{
		BurstCompileAttribute customAttribute = memberInfo.GetCustomAttribute<BurstCompileAttribute>();
		if (customAttribute != null)
		{
			return customAttribute;
		}
		foreach (Attribute customAttribute2 in memberInfo.GetCustomAttributes())
		{
			Type type = customAttribute2.GetType();
			if (type.FullName == "Burst.Compiler.IL.Tests.TestCompilerAttribute")
			{
				List<string> list = new List<string>();
				PropertyInfo property = type.GetProperty("ExpectCompilerException");
				if (property != null && (bool)property.GetValue(customAttribute2))
				{
					list.Add("--do-not-eager-compile");
				}
				return new BurstCompileAttribute(FloatPrecision.Standard, FloatMode.Default)
				{
					CompileSynchronously = true,
					Options = list.ToArray()
				};
			}
		}
		return null;
	}

	internal static bool HasBurstCompileAttribute(MemberInfo member)
	{
		if (member == null)
		{
			throw new ArgumentNullException("member");
		}
		BurstCompileAttribute attribute;
		return TryGetAttribute(member, out attribute);
	}

	internal bool TryGetOptions(MemberInfo member, bool isJit, out string flagsOut, bool isForEagerCompilation = false)
	{
		flagsOut = null;
		if (!TryGetAttribute(member, out var attribute, isForEagerCompilation))
		{
			return false;
		}
		flagsOut = GetOptions(isJit, attribute, isForEagerCompilation);
		return true;
	}

	internal string GetOptions(bool isJit, BurstCompileAttribute attr = null, bool isForEagerCompilation = false)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (isJit && !isForEagerCompilation && ((attr != null && attr.CompileSynchronously) || RequiresSynchronousCompilation))
		{
			AddOption(stringBuilder, GetOption("enable-synchronous-compilation"));
		}
		bool flag = EnableBurstSafetyChecks;
		if (isJit && isForEagerCompilation)
		{
			CompilationPriority compilationPriority = ((attr == null || !attr.CompileSynchronously) ? CompilationPriority.LowPriority : CompilationPriority.HighPriority);
			AddOption(stringBuilder, GetOption("compilation-priority=", compilationPriority));
			AddOption(stringBuilder, GetOption("skip-burst-initialize"));
		}
		if (attr != null)
		{
			if (attr.FloatMode != 0)
			{
				AddOption(stringBuilder, GetOption("float-mode=", attr.FloatMode));
			}
			if (attr.FloatPrecision != 0)
			{
				AddOption(stringBuilder, GetOption("float-precision=", attr.FloatPrecision));
			}
			if (attr.DisableSafetyChecks)
			{
				flag = false;
			}
			if (attr.Options != null)
			{
				string[] options = attr.Options;
				foreach (string text in options)
				{
					if (!string.IsNullOrEmpty(text))
					{
						AddOption(stringBuilder, text);
					}
				}
			}
		}
		if (ForceEnableBurstSafetyChecks)
		{
			flag = true;
		}
		if (flag)
		{
			AddOption(stringBuilder, GetOption("safety-checks"));
		}
		else
		{
			AddOption(stringBuilder, GetOption("disable-safety-checks"));
		}
		if (isJit && EnableBurstTimings)
		{
			AddOption(stringBuilder, GetOption("log-timings"));
		}
		if (EnableBurstDebug || (attr != null && attr.Debug))
		{
			AddOption(stringBuilder, GetOption("debugMode"));
		}
		return stringBuilder.ToString();
	}

	private static void AddOption(StringBuilder builder, string option)
	{
		if (builder.Length != 0)
		{
			builder.Append('\n');
		}
		builder.Append(option);
	}

	internal static string GetOption(string optionName, object value = null)
	{
		if (optionName == null)
		{
			throw new ArgumentNullException("optionName");
		}
		return "--" + optionName + (value ?? string.Empty);
	}

	private void OnOptionsChanged()
	{
		OptionsChanged?.Invoke();
	}

	private void MaybeTriggerRecompilation()
	{
	}

	private void MaybePreventChangingOption()
	{
	}

	static BurstCompilerOptions()
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		foreach (string text in commandLineArgs)
		{
			if (!(text == "--burst-disable-compilation"))
			{
				if (text == "--burst-force-sync-compilation")
				{
					ForceBurstCompilationSynchronously = false;
				}
			}
			else
			{
				ForceDisableBurstCompilation = true;
			}
		}
	}
}
