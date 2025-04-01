using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Threading;

namespace System;

/// <summary>Provides support for lazy initialization.</summary>
/// <typeparam name="T">The type of object that is being lazily initialized.</typeparam>
[Serializable]
[DebuggerDisplay("ThreadSafetyMode={Mode}, IsValueCreated={IsValueCreated}, IsValueFaulted={IsValueFaulted}, Value={ValueForDebugDisplay}")]
[DebuggerTypeProxy(typeof(System_LazyDebugView<>))]
[ComVisible(false)]
[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
public class Lazy<T>
{
	[Serializable]
	private class Boxed
	{
		internal T m_value;

		internal Boxed(T value)
		{
			m_value = value;
		}
	}

	private class LazyInternalExceptionHolder
	{
		internal ExceptionDispatchInfo m_edi;

		internal LazyInternalExceptionHolder(Exception ex)
		{
			m_edi = ExceptionDispatchInfo.Capture(ex);
		}
	}

	private static readonly Func<T> ALREADY_INVOKED_SENTINEL = () => default(T);

	private object m_boxed;

	[NonSerialized]
	private Func<T> m_valueFactory;

	[NonSerialized]
	private object m_threadSafeObj;

	internal T ValueForDebugDisplay
	{
		get
		{
			if (!IsValueCreated)
			{
				return default(T);
			}
			return ((Boxed)m_boxed).m_value;
		}
	}

	internal LazyThreadSafetyMode Mode
	{
		get
		{
			if (m_threadSafeObj == null)
			{
				return LazyThreadSafetyMode.None;
			}
			if (m_threadSafeObj == LazyHelpers.PUBLICATION_ONLY_SENTINEL)
			{
				return LazyThreadSafetyMode.PublicationOnly;
			}
			return LazyThreadSafetyMode.ExecutionAndPublication;
		}
	}

	internal bool IsValueFaulted => m_boxed is LazyInternalExceptionHolder;

	/// <summary>Gets a value that indicates whether a value has been created for this <see cref="T:System.Lazy`1" /> instance.</summary>
	/// <returns>true if a value has been created for this <see cref="T:System.Lazy`1" /> instance; otherwise, false.</returns>
	public bool IsValueCreated
	{
		get
		{
			if (m_boxed != null)
			{
				return m_boxed is Boxed;
			}
			return false;
		}
	}

	/// <summary>Gets the lazily initialized value of the current <see cref="T:System.Lazy`1" /> instance.</summary>
	/// <returns>The lazily initialized value of the current <see cref="T:System.Lazy`1" /> instance.</returns>
	/// <exception cref="T:System.MemberAccessException">The <see cref="T:System.Lazy`1" /> instance is initialized to use the default constructor of the type that is being lazily initialized, and permissions to access the constructor are missing. </exception>
	/// <exception cref="T:System.MissingMemberException">The <see cref="T:System.Lazy`1" /> instance is initialized to use the default constructor of the type that is being lazily initialized, and that type does not have a public, parameterless constructor. </exception>
	/// <exception cref="T:System.InvalidOperationException">The initialization function tries to access <see cref="P:System.Lazy`1.Value" /> on this instance. </exception>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public T Value
	{
		get
		{
			Boxed boxed = null;
			if (m_boxed != null)
			{
				if (m_boxed is Boxed boxed2)
				{
					return boxed2.m_value;
				}
				(m_boxed as LazyInternalExceptionHolder).m_edi.Throw();
			}
			Debugger.NotifyOfCrossThreadDependency();
			return LazyInitValue();
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Lazy`1" /> class. When lazy initialization occurs, the default constructor of the target type is used.</summary>
	public Lazy()
		: this(LazyThreadSafetyMode.ExecutionAndPublication)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Lazy`1" /> class. When lazy initialization occurs, the specified initialization function is used.</summary>
	/// <param name="valueFactory">The delegate that is invoked to produce the lazily initialized value when it is needed.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="valueFactory" /> is null. </exception>
	public Lazy(Func<T> valueFactory)
		: this(valueFactory, LazyThreadSafetyMode.ExecutionAndPublication)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Lazy`1" /> class. When lazy initialization occurs, the default constructor of the target type and the specified initialization mode are used.</summary>
	/// <param name="isThreadSafe">true to make this instance usable concurrently by multiple threads; false to make the instance usable by only one thread at a time. </param>
	public Lazy(bool isThreadSafe)
		: this(isThreadSafe ? LazyThreadSafetyMode.ExecutionAndPublication : LazyThreadSafetyMode.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Lazy`1" /> class that uses the default constructor of <paramref name="T" /> and the specified thread-safety mode.</summary>
	/// <param name="mode">One of the enumeration values that specifies the thread safety mode. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="mode" /> contains an invalid value. </exception>
	public Lazy(LazyThreadSafetyMode mode)
	{
		m_threadSafeObj = GetObjectFromMode(mode);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Lazy`1" /> class. When lazy initialization occurs, the specified initialization function and initialization mode are used.</summary>
	/// <param name="valueFactory">The delegate that is invoked to produce the lazily initialized value when it is needed.</param>
	/// <param name="isThreadSafe">true to make this instance usable concurrently by multiple threads; false to make this instance usable by only one thread at a time.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="valueFactory" /> is null. </exception>
	public Lazy(Func<T> valueFactory, bool isThreadSafe)
		: this(valueFactory, isThreadSafe ? LazyThreadSafetyMode.ExecutionAndPublication : LazyThreadSafetyMode.None)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Lazy`1" /> class that uses the specified initialization function and thread-safety mode.</summary>
	/// <param name="valueFactory">The delegate that is invoked to produce the lazily initialized value when it is needed.</param>
	/// <param name="mode">One of the enumeration values that specifies the thread safety mode. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="mode" /> contains an invalid value. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="valueFactory" /> is null. </exception>
	public Lazy(Func<T> valueFactory, LazyThreadSafetyMode mode)
	{
		if (valueFactory == null)
		{
			throw new ArgumentNullException("valueFactory");
		}
		m_threadSafeObj = GetObjectFromMode(mode);
		m_valueFactory = valueFactory;
	}

	private static object GetObjectFromMode(LazyThreadSafetyMode mode)
	{
		return mode switch
		{
			LazyThreadSafetyMode.ExecutionAndPublication => new object(), 
			LazyThreadSafetyMode.PublicationOnly => LazyHelpers.PUBLICATION_ONLY_SENTINEL, 
			LazyThreadSafetyMode.None => null, 
			_ => throw new ArgumentOutOfRangeException("mode", Environment.GetResourceString("The mode argument specifies an invalid value.")), 
		};
	}

	[OnSerializing]
	private void OnSerializing(StreamingContext context)
	{
		_ = Value;
	}

	/// <summary>Creates and returns a string representation of the <see cref="P:System.Lazy`1.Value" /> property for this instance.</summary>
	/// <returns>The result of calling the <see cref="M:System.Object.ToString" /> method on the <see cref="P:System.Lazy`1.Value" /> property for this instance, if the value has been created (that is, if the <see cref="P:System.Lazy`1.IsValueCreated" /> property returns true). Otherwise, a string indicating that the value has not been created. </returns>
	/// <exception cref="T:System.NullReferenceException">The <see cref="P:System.Lazy`1.Value" /> property is null.</exception>
	public override string ToString()
	{
		if (!IsValueCreated)
		{
			return Environment.GetResourceString("Value is not created.");
		}
		return Value.ToString();
	}

	private T LazyInitValue()
	{
		Boxed boxed = null;
		switch (Mode)
		{
		case LazyThreadSafetyMode.None:
			boxed = (Boxed)(m_boxed = CreateValue());
			break;
		case LazyThreadSafetyMode.PublicationOnly:
			boxed = CreateValue();
			if (boxed == null || Interlocked.CompareExchange(ref m_boxed, boxed, null) != null)
			{
				boxed = (Boxed)m_boxed;
			}
			else
			{
				m_valueFactory = ALREADY_INVOKED_SENTINEL;
			}
			break;
		default:
		{
			object obj = Volatile.Read(ref m_threadSafeObj);
			bool lockTaken = false;
			try
			{
				if (obj != ALREADY_INVOKED_SENTINEL)
				{
					Monitor.Enter(obj, ref lockTaken);
				}
				if (m_boxed == null)
				{
					boxed = (Boxed)(m_boxed = CreateValue());
					Volatile.Write(ref m_threadSafeObj, ALREADY_INVOKED_SENTINEL);
					break;
				}
				boxed = m_boxed as Boxed;
				if (boxed == null)
				{
					(m_boxed as LazyInternalExceptionHolder).m_edi.Throw();
				}
			}
			finally
			{
				if (lockTaken)
				{
					Monitor.Exit(obj);
				}
			}
			break;
		}
		}
		return boxed.m_value;
	}

	private Boxed CreateValue()
	{
		Boxed boxed = null;
		LazyThreadSafetyMode mode = Mode;
		if (m_valueFactory != null)
		{
			try
			{
				if (mode != LazyThreadSafetyMode.PublicationOnly && m_valueFactory == ALREADY_INVOKED_SENTINEL)
				{
					throw new InvalidOperationException(Environment.GetResourceString("ValueFactory attempted to access the Value property of this instance."));
				}
				Func<T> valueFactory = m_valueFactory;
				if (mode != LazyThreadSafetyMode.PublicationOnly)
				{
					m_valueFactory = ALREADY_INVOKED_SENTINEL;
				}
				else if (valueFactory == ALREADY_INVOKED_SENTINEL)
				{
					return null;
				}
				return new Boxed(valueFactory());
			}
			catch (Exception ex)
			{
				if (mode != LazyThreadSafetyMode.PublicationOnly)
				{
					m_boxed = new LazyInternalExceptionHolder(ex);
				}
				throw;
			}
		}
		try
		{
			return new Boxed((T)Activator.CreateInstance(typeof(T)));
		}
		catch (MissingMethodException)
		{
			Exception ex2 = new MissingMemberException(Environment.GetResourceString("The lazily-initialized type does not have a public, parameterless constructor."));
			if (mode != LazyThreadSafetyMode.PublicationOnly)
			{
				m_boxed = new LazyInternalExceptionHolder(ex2);
			}
			throw ex2;
		}
	}
}
