using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic.Utils;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace System.Runtime.CompilerServices;

/// <summary>Class responsible for runtime binding of the dynamic operations on the dynamic call site.</summary>
public abstract class CallSiteBinder
{
	private sealed class LambdaSignature<T> where T : class
	{
		private static LambdaSignature<T> s_instance;

		internal readonly ReadOnlyCollection<ParameterExpression> Parameters;

		internal readonly LabelTarget ReturnLabel;

		internal static LambdaSignature<T> Instance
		{
			get
			{
				if (s_instance == null)
				{
					s_instance = new LambdaSignature<T>();
				}
				return s_instance;
			}
		}

		private LambdaSignature()
		{
			Type typeFromHandle = typeof(T);
			if (!typeFromHandle.IsSubclassOf(typeof(MulticastDelegate)))
			{
				throw Error.TypeParameterIsNotDelegate(typeFromHandle);
			}
			MethodInfo invokeMethod = typeFromHandle.GetInvokeMethod();
			ParameterInfo[] parametersCached = invokeMethod.GetParametersCached();
			if (parametersCached[0].ParameterType != typeof(CallSite))
			{
				throw Error.FirstArgumentMustBeCallSite();
			}
			ParameterExpression[] array = new ParameterExpression[parametersCached.Length - 1];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = Expression.Parameter(parametersCached[i + 1].ParameterType, "$arg" + i);
			}
			Parameters = new TrueReadOnlyCollection<ParameterExpression>(array);
			ReturnLabel = Expression.Label(invokeMethod.GetReturnType());
		}
	}

	internal Dictionary<Type, object> Cache;

	/// <summary>Gets a label that can be used to cause the binding to be updated. It indicates that the expression's binding is no longer valid. This is typically used when the "version" of a dynamic object has changed.</summary>
	/// <returns>The <see cref="T:System.Linq.Expressions.LabelTarget" /> object representing a label that can be used to trigger the binding update.</returns>
	public static LabelTarget UpdateLabel { get; } = Expression.Label("CallSiteBinder.UpdateLabel");

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.CompilerServices.CallSiteBinder" /> class.</summary>
	protected CallSiteBinder()
	{
	}

	/// <summary>Performs the runtime binding of the dynamic operation on a set of arguments.</summary>
	/// <returns>An Expression that performs tests on the dynamic operation arguments, and performs the dynamic operation if the tests are valid. If the tests fail on subsequent occurrences of the dynamic operation, Bind will be called again to produce a new <see cref="T:System.Linq.Expressions.Expression" /> for the new argument types.</returns>
	/// <param name="args">An array of arguments to the dynamic operation.</param>
	/// <param name="parameters">The array of <see cref="T:System.Linq.Expressions.ParameterExpression" /> instances that represent the parameters of the call site in the binding process.</param>
	/// <param name="returnLabel">A LabelTarget used to return the result of the dynamic binding.</param>
	public abstract Expression Bind(object[] args, ReadOnlyCollection<ParameterExpression> parameters, LabelTarget returnLabel);

	/// <summary>Provides low-level runtime binding support. Classes can override this and provide a direct delegate for the implementation of rule. This can enable saving rules to disk, having specialized rules available at runtime, or providing a different caching policy.</summary>
	/// <returns>A new delegate which replaces the CallSite Target.</returns>
	/// <param name="site">The CallSite the bind is being performed for.</param>
	/// <param name="args">The arguments for the binder.</param>
	/// <typeparam name="T">The target type of the CallSite.</typeparam>
	public virtual T BindDelegate<T>(CallSite<T> site, object[] args) where T : class
	{
		return null;
	}

	internal T BindCore<T>(CallSite<T> site, object[] args) where T : class
	{
		T val = BindDelegate(site, args);
		if (val != null)
		{
			return val;
		}
		LambdaSignature<T> instance = LambdaSignature<T>.Instance;
		T val2 = Stitch(Bind(args, instance.Parameters, instance.ReturnLabel) ?? throw Error.NoOrInvalidRuleProduced(), instance).Compile();
		CacheTarget(val2);
		return val2;
	}

	/// <summary>Adds a target to the cache of known targets. The cached targets will be scanned before calling BindDelegate to produce the new rule.</summary>
	/// <param name="target">The target delegate to be added to the cache.</param>
	/// <typeparam name="T">The type of target being added.</typeparam>
	protected void CacheTarget<T>(T target) where T : class
	{
		GetRuleCache<T>().AddRule(target);
	}

	private static Expression<T> Stitch<T>(Expression binding, LambdaSignature<T> signature) where T : class
	{
		Type typeFromHandle = typeof(CallSite<T>);
		ReadOnlyCollectionBuilder<Expression> obj = new ReadOnlyCollectionBuilder<Expression>(3) { binding };
		ParameterExpression parameterExpression = Expression.Parameter(typeof(CallSite), "$site");
		TrueReadOnlyCollection<ParameterExpression> trueReadOnlyCollection = signature.Parameters.AddFirst(parameterExpression);
		Expression item = Expression.Label(UpdateLabel);
		obj.Add(item);
		obj.Add(Expression.Label(signature.ReturnLabel, Expression.Condition(Expression.Call(CachedReflectionInfo.CallSiteOps_SetNotMatched, parameterExpression), Expression.Default(signature.ReturnLabel.Type), Expression.Invoke(Expression.Property(Expression.Convert(parameterExpression, typeFromHandle), typeof(CallSite<T>).GetProperty("Update")), trueReadOnlyCollection))));
		return Expression.Lambda<T>(Expression.Block(obj), "CallSite.Target", tailCall: true, trueReadOnlyCollection);
	}

	internal RuleCache<T> GetRuleCache<T>() where T : class
	{
		if (Cache == null)
		{
			Interlocked.CompareExchange(ref Cache, new Dictionary<Type, object>(), null);
		}
		Dictionary<Type, object> cache = Cache;
		object value;
		lock (cache)
		{
			if (!cache.TryGetValue(typeof(T), out value))
			{
				value = (cache[typeof(T)] = new RuleCache<T>());
			}
		}
		return value as RuleCache<T>;
	}
}
