using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;

namespace System.Threading;

/// <summary>Creates and controls a thread, sets its priority, and gets its status. </summary>
/// <filterpriority>1</filterpriority>
[StructLayout(LayoutKind.Sequential)]
[ComDefaultInterface(typeof(_Thread))]
[ClassInterface(ClassInterfaceType.None)]
[ComVisible(true)]
public sealed class Thread : CriticalFinalizerObject, _Thread
{
	private static LocalDataStoreMgr s_LocalDataStoreMgr;

	[ThreadStatic]
	private static LocalDataStoreHolder s_LocalDataStore;

	[ThreadStatic]
	internal static CultureInfo m_CurrentCulture;

	[ThreadStatic]
	internal static CultureInfo m_CurrentUICulture;

	private static AsyncLocal<CultureInfo> s_asyncLocalCurrentCulture;

	private static AsyncLocal<CultureInfo> s_asyncLocalCurrentUICulture;

	private InternalThread internal_thread;

	private object m_ThreadStartArg;

	private object pending_exception;

	private IPrincipal principal;

	private int principal_version;

	[ThreadStatic]
	private static Thread current_thread;

	private MulticastDelegate m_Delegate;

	private ExecutionContext m_ExecutionContext;

	private bool m_ExecutionContextBelongsToOuterScope;

	internal bool ExecutionContextBelongsToCurrentScope
	{
		get
		{
			return !m_ExecutionContextBelongsToOuterScope;
		}
		set
		{
			m_ExecutionContextBelongsToOuterScope = !value;
		}
	}

	/// <summary>Gets an <see cref="T:System.Threading.ExecutionContext" /> object that contains information about the various contexts of the current thread. </summary>
	/// <returns>An <see cref="T:System.Threading.ExecutionContext" /> object that consolidates context information for the current thread.</returns>
	/// <filterpriority>2</filterpriority>
	public ExecutionContext ExecutionContext
	{
		[SecuritySafeCritical]
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		get
		{
			if (this == CurrentThread)
			{
				return GetMutableExecutionContext();
			}
			return m_ExecutionContext;
		}
	}

	/// <summary>Gets or sets a value indicating the scheduling priority of a thread.</summary>
	/// <returns>One of the <see cref="T:System.Threading.ThreadPriority" /> values. The default value is Normal.</returns>
	/// <exception cref="T:System.Threading.ThreadStateException">The thread has reached a final state, such as <see cref="F:System.Threading.ThreadState.Aborted" />. </exception>
	/// <exception cref="T:System.ArgumentException">The value specified for a set operation is not a valid ThreadPriority value. </exception>
	/// <filterpriority>1</filterpriority>
	public ThreadPriority Priority
	{
		[SecuritySafeCritical]
		get
		{
			return (ThreadPriority)GetPriorityNative();
		}
		[SecuritySafeCritical]
		[HostProtection(SecurityAction.LinkDemand, SelfAffectingThreading = true)]
		set
		{
			SetPriorityNative((int)value);
		}
	}

	/// <summary>Gets or sets the current culture used by the Resource Manager to look up culture-specific resources at run time.</summary>
	/// <returns>An object that represents the current culture.</returns>
	/// <exception cref="T:System.ArgumentNullException">The property is set to null. </exception>
	/// <exception cref="T:System.ArgumentException">The property is set to a culture name that cannot be used to locate a resource file. Resource filenames must include only letters, numbers, hyphens or underscores.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public CultureInfo CurrentUICulture
	{
		get
		{
			return GetCurrentUICultureNoAppX();
		}
		[SecuritySafeCritical]
		[HostProtection(SecurityAction.LinkDemand, ExternalThreading = true)]
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			CultureInfo.VerifyCultureName(value, throwException: true);
			if (m_CurrentUICulture == null && m_CurrentCulture == null)
			{
				nativeInitCultureAccessors();
			}
			if (!AppContextSwitches.NoAsyncCurrentCulture)
			{
				if (s_asyncLocalCurrentUICulture == null)
				{
					Interlocked.CompareExchange(ref s_asyncLocalCurrentUICulture, new AsyncLocal<CultureInfo>(AsyncLocalSetCurrentUICulture), null);
				}
				s_asyncLocalCurrentUICulture.Value = value;
			}
			else
			{
				m_CurrentUICulture = value;
			}
		}
	}

	/// <summary>Gets or sets the culture for the current thread.</summary>
	/// <returns>An object that represents the culture for the current thread.</returns>
	/// <exception cref="T:System.ArgumentNullException">The property is set to null.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlThread" />
	/// </PermissionSet>
	public CultureInfo CurrentCulture
	{
		get
		{
			return GetCurrentCultureNoAppX();
		}
		[SecuritySafeCritical]
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (m_CurrentCulture == null && m_CurrentUICulture == null)
			{
				nativeInitCultureAccessors();
			}
			if (!AppContextSwitches.NoAsyncCurrentCulture)
			{
				if (s_asyncLocalCurrentCulture == null)
				{
					Interlocked.CompareExchange(ref s_asyncLocalCurrentCulture, new AsyncLocal<CultureInfo>(AsyncLocalSetCurrentCulture), null);
				}
				s_asyncLocalCurrentCulture.Value = value;
			}
			else
			{
				m_CurrentCulture = value;
			}
		}
	}

	private static LocalDataStoreMgr LocalDataStoreManager
	{
		get
		{
			if (s_LocalDataStoreMgr == null)
			{
				Interlocked.CompareExchange(ref s_LocalDataStoreMgr, new LocalDataStoreMgr(), null);
			}
			return s_LocalDataStoreMgr;
		}
	}

	private InternalThread Internal
	{
		get
		{
			if (internal_thread == null)
			{
				ConstructInternalThread();
			}
			return internal_thread;
		}
	}

	/// <summary>Gets the current context in which the thread is executing.</summary>
	/// <returns>A <see cref="T:System.Runtime.Remoting.Contexts.Context" /> representing the current thread context.</returns>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure" />
	/// </PermissionSet>
	public static Context CurrentContext
	{
		[SecurityPermission(SecurityAction.LinkDemand, Infrastructure = true)]
		get
		{
			return AppDomain.InternalGetContext();
		}
	}

	/// <summary>Gets or sets the thread's current principal (for role-based security).</summary>
	/// <returns>An <see cref="T:System.Security.Principal.IPrincipal" /> value representing the security context.</returns>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the permission required to set the principal. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlPrincipal" />
	/// </PermissionSet>
	public static IPrincipal CurrentPrincipal
	{
		get
		{
			Thread currentThread = CurrentThread;
			if (currentThread.principal_version != currentThread.Internal._serialized_principal_version)
			{
				currentThread.principal = null;
			}
			if (currentThread.principal != null)
			{
				return currentThread.principal;
			}
			if (currentThread.Internal._serialized_principal != null)
			{
				try
				{
					DeserializePrincipal(currentThread);
					return currentThread.principal;
				}
				catch
				{
				}
			}
			currentThread.principal = GetDomain().DefaultPrincipal;
			currentThread.principal_version = currentThread.Internal._serialized_principal_version;
			return currentThread.principal;
		}
		[SecurityPermission(SecurityAction.Demand, ControlPrincipal = true)]
		set
		{
			Thread currentThread = CurrentThread;
			if (value != GetDomain().DefaultPrincipal)
			{
				currentThread.Internal._serialized_principal_version++;
				try
				{
					SerializePrincipal(currentThread, value);
				}
				catch (Exception)
				{
					currentThread.Internal._serialized_principal = null;
				}
				currentThread.principal_version = currentThread.Internal._serialized_principal_version;
			}
			else
			{
				currentThread.Internal._serialized_principal = null;
			}
			currentThread.principal = value;
		}
	}

	/// <summary>Gets the currently running thread.</summary>
	/// <returns>A <see cref="T:System.Threading.Thread" /> that is the representation of the currently running thread.</returns>
	/// <filterpriority>1</filterpriority>
	public static Thread CurrentThread
	{
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
		get
		{
			Thread thread = current_thread;
			if (thread != null)
			{
				return thread;
			}
			return GetCurrentThread();
		}
	}

	internal static int CurrentThreadId => (int)CurrentThread.internal_thread.thread_id;

	/// <summary>Gets or sets the apartment state of this thread.</summary>
	/// <returns>One of the <see cref="T:System.Threading.ApartmentState" /> values. The initial value is Unknown.</returns>
	/// <exception cref="T:System.ArgumentException">An attempt is made to set this property to a state that is not a valid apartment state (a state other than single-threaded apartment (STA) or multithreaded apartment (MTA)). </exception>
	/// <filterpriority>2</filterpriority>
	[Obsolete("Deprecated in favor of GetApartmentState, SetApartmentState and TrySetApartmentState.")]
	public ApartmentState ApartmentState
	{
		get
		{
			ValidateThreadState();
			return (ApartmentState)Internal.apartment_state;
		}
		set
		{
			ValidateThreadState();
			TrySetApartmentState(value);
		}
	}

	/// <summary>Gets a value indicating whether or not a thread belongs to the managed thread pool.</summary>
	/// <returns>true if this thread belongs to the managed thread pool; otherwise, false.</returns>
	/// <filterpriority>2</filterpriority>
	public bool IsThreadPoolThread => IsThreadPoolThreadInternal;

	internal bool IsThreadPoolThreadInternal
	{
		get
		{
			return Internal.threadpool_thread;
		}
		set
		{
			Internal.threadpool_thread = value;
		}
	}

	/// <summary>Gets a value indicating the execution status of the current thread.</summary>
	/// <returns>true if this thread has been started and has not terminated normally or aborted; otherwise, false.</returns>
	/// <filterpriority>1</filterpriority>
	public bool IsAlive
	{
		get
		{
			ThreadState state = GetState(Internal);
			if ((state & ThreadState.Aborted) != 0 || (state & ThreadState.Stopped) != 0 || (state & ThreadState.Unstarted) != 0)
			{
				return false;
			}
			return true;
		}
	}

	/// <summary>Gets or sets a value indicating whether or not a thread is a background thread.</summary>
	/// <returns>true if this thread is or is to become a background thread; otherwise, false.</returns>
	/// <exception cref="T:System.Threading.ThreadStateException">The thread is dead. </exception>
	/// <filterpriority>1</filterpriority>
	public bool IsBackground
	{
		get
		{
			return (ValidateThreadState() & ThreadState.Background) != 0;
		}
		set
		{
			ValidateThreadState();
			if (value)
			{
				SetState(Internal, ThreadState.Background);
			}
			else
			{
				ClrState(Internal, ThreadState.Background);
			}
		}
	}

	/// <summary>Gets or sets the name of the thread.</summary>
	/// <returns>A string containing the name of the thread, or null if no name was set.</returns>
	/// <exception cref="T:System.InvalidOperationException">A set operation was requested, and the Name property has already been set. </exception>
	/// <filterpriority>1</filterpriority>
	public string Name
	{
		get
		{
			return GetName_internal(Internal);
		}
		set
		{
			SetName_internal(Internal, value);
		}
	}

	/// <summary>Gets a value containing the states of the current thread.</summary>
	/// <returns>One of the <see cref="T:System.Threading.ThreadState" /> values indicating the state of the current thread. The initial value is Unstarted.</returns>
	/// <filterpriority>2</filterpriority>
	public ThreadState ThreadState => GetState(Internal);

	internal object AbortReason => GetAbortExceptionState();

	/// <summary>Gets a unique identifier for the current managed thread.</summary>
	/// <returns>An integer that represents a unique identifier for this managed thread.</returns>
	/// <filterpriority>1</filterpriority>
	public int ManagedThreadId
	{
		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
		get
		{
			return Internal.managed_id;
		}
	}

	private static void AsyncLocalSetCurrentCulture(AsyncLocalValueChangedArgs<CultureInfo> args)
	{
		m_CurrentCulture = args.CurrentValue;
	}

	private static void AsyncLocalSetCurrentUICulture(AsyncLocalValueChangedArgs<CultureInfo> args)
	{
		m_CurrentUICulture = args.CurrentValue;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.Thread" /> class.</summary>
	/// <param name="start">A <see cref="T:System.Threading.ThreadStart" /> delegate that represents the methods to be invoked when this thread begins executing. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="start" /> parameter is null. </exception>
	[SecuritySafeCritical]
	public Thread(ThreadStart start)
	{
		if (start == null)
		{
			throw new ArgumentNullException("start");
		}
		SetStartHelper(start, 0);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.Thread" /> class, specifying the maximum stack size for the thread.</summary>
	/// <param name="start">A <see cref="T:System.Threading.ThreadStart" /> delegate that represents the methods to be invoked when this thread begins executing.</param>
	/// <param name="maxStackSize">The maximum stack size, in bytes, to be used by the thread, or 0 to use the default maximum stack size specified in the header for the executable.Important   For partially trusted code, <paramref name="maxStackSize" /> is ignored if it is greater than the default stack size. No exception is thrown. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="start" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="maxStackSize" /> is less than zero.</exception>
	[SecuritySafeCritical]
	public Thread(ThreadStart start, int maxStackSize)
	{
		if (start == null)
		{
			throw new ArgumentNullException("start");
		}
		if (0 > maxStackSize)
		{
			throw new ArgumentOutOfRangeException("maxStackSize", Environment.GetResourceString("Non-negative number required."));
		}
		SetStartHelper(start, maxStackSize);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.Thread" /> class, specifying a delegate that allows an object to be passed to the thread when the thread is started.</summary>
	/// <param name="start">A <see cref="T:System.Threading.ParameterizedThreadStart" /> delegate that represents the methods to be invoked when this thread begins executing.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="start" /> is null. </exception>
	[SecuritySafeCritical]
	public Thread(ParameterizedThreadStart start)
	{
		if (start == null)
		{
			throw new ArgumentNullException("start");
		}
		SetStartHelper(start, 0);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.Thread" /> class, specifying a delegate that allows an object to be passed to the thread when the thread is started and specifying the maximum stack size for the thread.</summary>
	/// <param name="start">A <see cref="T:System.Threading.ParameterizedThreadStart" /> delegate that represents the methods to be invoked when this thread begins executing.</param>
	/// <param name="maxStackSize">The maximum stack size, in bytes, to be used by the thread, or 0 to use the default maximum stack size specified in the header for the executable.Important   For partially trusted code, <paramref name="maxStackSize" /> is ignored if it is greater than the default stack size. No exception is thrown.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="start" /> is null. </exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="maxStackSize" /> is less than zero.</exception>
	[SecuritySafeCritical]
	public Thread(ParameterizedThreadStart start, int maxStackSize)
	{
		if (start == null)
		{
			throw new ArgumentNullException("start");
		}
		if (0 > maxStackSize)
		{
			throw new ArgumentOutOfRangeException("maxStackSize", Environment.GetResourceString("Non-negative number required."));
		}
		SetStartHelper(start, maxStackSize);
	}

	/// <summary>Causes the operating system to change the state of the current instance to <see cref="F:System.Threading.ThreadState.Running" />.</summary>
	/// <exception cref="T:System.Threading.ThreadStateException">The thread has already been started. </exception>
	/// <exception cref="T:System.OutOfMemoryException">There is not enough memory available to start this thread. </exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.NoInlining)]
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	public void Start()
	{
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		Start(ref stackMark);
	}

	/// <summary>Causes the operating system to change the state of the current instance to <see cref="F:System.Threading.ThreadState.Running" />, and optionally supplies an object containing data to be used by the method the thread executes.</summary>
	/// <param name="parameter">An object that contains data to be used by the method the thread executes.</param>
	/// <exception cref="T:System.Threading.ThreadStateException">The thread has already been started. </exception>
	/// <exception cref="T:System.OutOfMemoryException">There is not enough memory available to start this thread. </exception>
	/// <exception cref="T:System.InvalidOperationException">This thread was created using a <see cref="T:System.Threading.ThreadStart" /> delegate instead of a <see cref="T:System.Threading.ParameterizedThreadStart" /> delegate.</exception>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.NoInlining)]
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	public void Start(object parameter)
	{
		if (m_Delegate is ThreadStart)
		{
			throw new InvalidOperationException(Environment.GetResourceString("The thread was created with a ThreadStart delegate that does not accept a parameter."));
		}
		m_ThreadStartArg = parameter;
		StackCrawlMark stackMark = StackCrawlMark.LookForMyCaller;
		Start(ref stackMark);
	}

	[SecuritySafeCritical]
	private void Start(ref StackCrawlMark stackMark)
	{
		if ((object)m_Delegate != null)
		{
			ThreadHelper obj = (ThreadHelper)m_Delegate.Target;
			ExecutionContext executionContextHelper = ExecutionContext.Capture(ref stackMark, ExecutionContext.CaptureOptions.IgnoreSyncCtx);
			obj.SetExecutionContextHelper(executionContextHelper);
		}
		IPrincipal principal = null;
		StartInternal(principal, ref stackMark);
	}

	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	internal ExecutionContext.Reader GetExecutionContextReader()
	{
		return new ExecutionContext.Reader(m_ExecutionContext);
	}

	[SecurityCritical]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	internal ExecutionContext GetMutableExecutionContext()
	{
		if (m_ExecutionContext == null)
		{
			m_ExecutionContext = new ExecutionContext();
		}
		else if (!ExecutionContextBelongsToCurrentScope)
		{
			ExecutionContext executionContext = m_ExecutionContext.CreateMutableCopy();
			m_ExecutionContext = executionContext;
		}
		ExecutionContextBelongsToCurrentScope = true;
		return m_ExecutionContext;
	}

	[SecurityCritical]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	internal void SetExecutionContext(ExecutionContext value, bool belongsToCurrentScope)
	{
		m_ExecutionContext = value;
		ExecutionContextBelongsToCurrentScope = belongsToCurrentScope;
	}

	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[SecurityCritical]
	internal void SetExecutionContext(ExecutionContext.Reader value, bool belongsToCurrentScope)
	{
		m_ExecutionContext = value.DangerousGetRawExecutionContext();
		ExecutionContextBelongsToCurrentScope = belongsToCurrentScope;
	}

	/// <summary>Applies a captured <see cref="T:System.Threading.CompressedStack" /> to the current thread. </summary>
	/// <param name="stack">The <see cref="T:System.Threading.CompressedStack" /> object to be applied to the current thread.</param>
	/// <exception cref="T:System.InvalidOperationException">In all cases. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Security.Permissions.StrongNameIdentityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PublicKeyBlob="00000000000000000400000000000000" />
	/// </PermissionSet>
	[Obsolete("Thread.SetCompressedStack is no longer supported. Please use the System.Threading.CompressedStack class")]
	[SecurityCritical]
	public void SetCompressedStack(CompressedStack stack)
	{
		throw new InvalidOperationException(Environment.GetResourceString("Use CompressedStack.(Capture/Run) or ExecutionContext.(Capture/Run) APIs instead."));
	}

	/// <summary>Returns a <see cref="T:System.Threading.CompressedStack" /> object that can be used to capture the stack for the current thread. </summary>
	/// <returns>None. </returns>
	/// <exception cref="T:System.InvalidOperationException">In all cases.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Security.Permissions.StrongNameIdentityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PublicKeyBlob="00000000000000000400000000000000" />
	/// </PermissionSet>
	[SecurityCritical]
	[Obsolete("Thread.GetCompressedStack is no longer supported. Please use the System.Threading.CompressedStack class")]
	public CompressedStack GetCompressedStack()
	{
		throw new InvalidOperationException(Environment.GetResourceString("Use CompressedStack.(Capture/Run) or ExecutionContext.(Capture/Run) APIs instead."));
	}

	/// <summary>Cancels an <see cref="M:System.Threading.Thread.Abort(System.Object)" /> requested for the current thread.</summary>
	/// <exception cref="T:System.Threading.ThreadStateException">Abort was not invoked on the current thread. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required security permission for the current thread. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlThread" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	[SecurityPermission(SecurityAction.Demand, ControlThread = true)]
	public static void ResetAbort()
	{
		Thread currentThread = CurrentThread;
		if ((currentThread.ThreadState & ThreadState.AbortRequested) == 0)
		{
			throw new ThreadStateException(Environment.GetResourceString("Unable to reset abort because no abort was requested."));
		}
		currentThread.ResetAbortNative();
		currentThread.ClearAbortReason();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private extern void ResetAbortNative();

	/// <summary>Either suspends the thread, or if the thread is already suspended, has no effect.</summary>
	/// <exception cref="T:System.Threading.ThreadStateException">The thread has not been started or is dead. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the appropriate <see cref="T:System.Security.Permissions.SecurityPermission" />. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlThread" />
	/// </PermissionSet>
	[Obsolete("Thread.Suspend has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202", false)]
	[SecuritySafeCritical]
	[SecurityPermission(SecurityAction.Demand, ControlThread = true)]
	[SecurityPermission(SecurityAction.Demand, ControlThread = true)]
	public void Suspend()
	{
		SuspendInternal();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private extern void SuspendInternal();

	/// <summary>Resumes a thread that has been suspended.</summary>
	/// <exception cref="T:System.Threading.ThreadStateException">The thread has not been started, is dead, or is not in the suspended state. </exception>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the appropriate <see cref="T:System.Security.Permissions.SecurityPermission" />. </exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlThread" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	[Obsolete("Thread.Resume has been deprecated.  Please use other classes in System.Threading, such as Monitor, Mutex, Event, and Semaphore, to synchronize Threads or protect resources.  http://go.microsoft.com/fwlink/?linkid=14202", false)]
	[SecurityPermission(SecurityAction.Demand, ControlThread = true)]
	public void Resume()
	{
		ResumeInternal();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private extern void ResumeInternal();

	/// <summary>Interrupts a thread that is in the WaitSleepJoin thread state.</summary>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the appropriate <see cref="T:System.Security.Permissions.SecurityPermission" />. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlThread" />
	/// </PermissionSet>
	[SecuritySafeCritical]
	[SecurityPermission(SecurityAction.Demand, ControlThread = true)]
	public void Interrupt()
	{
		InterruptInternal();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private extern void InterruptInternal();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private extern int GetPriorityNative();

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private extern void SetPriorityNative(int priority);

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private extern bool JoinInternal(int millisecondsTimeout);

	/// <summary>Blocks the calling thread until a thread terminates, while continuing to perform standard COM and SendMessage pumping.</summary>
	/// <exception cref="T:System.Threading.ThreadStateException">The caller attempted to join a thread that is in the <see cref="F:System.Threading.ThreadState.Unstarted" /> state. </exception>
	/// <exception cref="T:System.Threading.ThreadInterruptedException">The thread is interrupted while waiting. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	public void Join()
	{
		JoinInternal(-1);
	}

	/// <summary>Blocks the calling thread until a thread terminates or the specified time elapses, while continuing to perform standard COM and SendMessage pumping.</summary>
	/// <returns>true if the thread has terminated; false if the thread has not terminated after the amount of time specified by the <paramref name="millisecondsTimeout" /> parameter has elapsed.</returns>
	/// <param name="millisecondsTimeout">The number of milliseconds to wait for the thread to terminate. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of <paramref name="millisecondsTimeout" /> is negative and is not equal to <see cref="F:System.Threading.Timeout.Infinite" /> in milliseconds. </exception>
	/// <exception cref="T:System.Threading.ThreadStateException">The thread has not been started. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	public bool Join(int millisecondsTimeout)
	{
		if (millisecondsTimeout < -1)
		{
			throw new ArgumentOutOfRangeException("millisecondsTimeout", Environment.GetResourceString("Number must be either non-negative and less than or equal to Int32.MaxValue or -1."));
		}
		return JoinInternal(millisecondsTimeout);
	}

	/// <summary>Blocks the calling thread until a thread terminates or the specified time elapses, while continuing to perform standard COM and SendMessage pumping.</summary>
	/// <returns>true if the thread terminated; false if the thread has not terminated after the amount of time specified by the <paramref name="timeout" /> parameter has elapsed.</returns>
	/// <param name="timeout">A <see cref="T:System.TimeSpan" /> set to the amount of time to wait for the thread to terminate. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of <paramref name="timeout" /> is negative and is not equal to <see cref="F:System.Threading.Timeout.Infinite" /> in milliseconds, or is greater than <see cref="F:System.Int32.MaxValue" /> milliseconds. </exception>
	/// <exception cref="T:System.Threading.ThreadStateException">The caller attempted to join a thread that is in the <see cref="F:System.Threading.ThreadState.Unstarted" /> state. </exception>
	/// <filterpriority>1</filterpriority>
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	public bool Join(TimeSpan timeout)
	{
		long num = (long)timeout.TotalMilliseconds;
		if (num < -1 || num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("Number must be either non-negative and less than or equal to Int32.MaxValue or -1."));
		}
		return Join((int)num);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	private static extern void SleepInternal(int millisecondsTimeout);

	/// <summary>Suspends the current thread for a specified time.</summary>
	/// <param name="millisecondsTimeout">The number of milliseconds for which the thread is blocked. Specify zero (0) to indicate that this thread should be suspended to allow other waiting threads to execute. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to block the thread indefinitely. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The time-out value is negative and is not equal to <see cref="F:System.Threading.Timeout.Infinite" />. </exception>
	/// <filterpriority>1</filterpriority>
	[SecuritySafeCritical]
	public static void Sleep(int millisecondsTimeout)
	{
		if (millisecondsTimeout < -1)
		{
			throw new ArgumentOutOfRangeException("millisecondsTimeout", Environment.GetResourceString("Number must be either non-negative and less than or equal to Int32.MaxValue or -1."));
		}
		SleepInternal(millisecondsTimeout);
	}

	/// <summary>Blocks the current thread for a specified time.</summary>
	/// <param name="timeout">A <see cref="T:System.TimeSpan" /> set to the amount of time for which the thread is blocked. Specify zero to indicate that this thread should be suspended to allow other waiting threads to execute. Specify <see cref="F:System.Threading.Timeout.Infinite" /> to block the thread indefinitely. </param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The value of <paramref name="timeout" /> is negative and is not equal to <see cref="F:System.Threading.Timeout.Infinite" /> in milliseconds, or is greater than <see cref="F:System.Int32.MaxValue" /> milliseconds. </exception>
	/// <filterpriority>1</filterpriority>
	public static void Sleep(TimeSpan timeout)
	{
		long num = (long)timeout.TotalMilliseconds;
		if (num < -1 || num > int.MaxValue)
		{
			throw new ArgumentOutOfRangeException("timeout", Environment.GetResourceString("Number must be either non-negative and less than or equal to Int32.MaxValue or -1."));
		}
		Sleep((int)num);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecurityCritical]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[SuppressUnmanagedCodeSecurity]
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	private static extern bool YieldInternal();

	/// <summary>Causes the calling thread to yield execution to another thread that is ready to run on the current processor. The operating system selects the thread to yield to.</summary>
	/// <returns>true if the operating system switched execution to another thread; otherwise, false.</returns>
	[SecuritySafeCritical]
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	[HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	public static bool Yield()
	{
		return YieldInternal();
	}

	[SecurityCritical]
	private void SetStartHelper(Delegate start, int maxStackSize)
	{
		maxStackSize = GetProcessDefaultStackSize(maxStackSize);
		ThreadHelper @object = new ThreadHelper(start);
		if (start is ThreadStart)
		{
			SetStart(new ThreadStart(@object.ThreadStart), maxStackSize);
		}
		else
		{
			SetStart(new ParameterizedThreadStart(@object.ThreadStart), maxStackSize);
		}
	}

	/// <summary>Allocates an unnamed data slot on all the threads. For better performance, use fields that are marked with the <see cref="T:System.ThreadStaticAttribute" /> attribute instead.</summary>
	/// <returns>The allocated named data slot on all threads.</returns>
	/// <filterpriority>2</filterpriority>
	[HostProtection(SecurityAction.LinkDemand, SharedState = true, ExternalThreading = true)]
	public static LocalDataStoreSlot AllocateDataSlot()
	{
		return LocalDataStoreManager.AllocateDataSlot();
	}

	/// <summary>Allocates a named data slot on all threads. For better performance, use fields that are marked with the <see cref="T:System.ThreadStaticAttribute" /> attribute instead.</summary>
	/// <returns>The allocated named data slot on all threads.</returns>
	/// <param name="name">The name of the data slot to be allocated. </param>
	/// <exception cref="T:System.ArgumentException">A named data slot with the specified name already exists.</exception>
	/// <filterpriority>2</filterpriority>
	[HostProtection(SecurityAction.LinkDemand, SharedState = true, ExternalThreading = true)]
	public static LocalDataStoreSlot AllocateNamedDataSlot(string name)
	{
		return LocalDataStoreManager.AllocateNamedDataSlot(name);
	}

	/// <summary>Looks up a named data slot. For better performance, use fields that are marked with the <see cref="T:System.ThreadStaticAttribute" /> attribute instead.</summary>
	/// <returns>A <see cref="T:System.LocalDataStoreSlot" /> allocated for this thread.</returns>
	/// <param name="name">The name of the local data slot. </param>
	/// <filterpriority>2</filterpriority>
	[HostProtection(SecurityAction.LinkDemand, SharedState = true, ExternalThreading = true)]
	public static LocalDataStoreSlot GetNamedDataSlot(string name)
	{
		return LocalDataStoreManager.GetNamedDataSlot(name);
	}

	/// <summary>Eliminates the association between a name and a slot, for all threads in the process. For better performance, use fields that are marked with the <see cref="T:System.ThreadStaticAttribute" /> attribute instead.</summary>
	/// <param name="name">The name of the data slot to be freed. </param>
	/// <filterpriority>2</filterpriority>
	[HostProtection(SecurityAction.LinkDemand, SharedState = true, ExternalThreading = true)]
	public static void FreeNamedDataSlot(string name)
	{
		LocalDataStoreManager.FreeNamedDataSlot(name);
	}

	/// <summary>Retrieves the value from the specified slot on the current thread, within the current thread's current domain. For better performance, use fields that are marked with the <see cref="T:System.ThreadStaticAttribute" /> attribute instead.</summary>
	/// <returns>The retrieved value.</returns>
	/// <param name="slot">The <see cref="T:System.LocalDataStoreSlot" /> from which to get the value. </param>
	/// <filterpriority>2</filterpriority>
	[HostProtection(SecurityAction.LinkDemand, SharedState = true, ExternalThreading = true)]
	public static object GetData(LocalDataStoreSlot slot)
	{
		LocalDataStoreHolder localDataStoreHolder = s_LocalDataStore;
		if (localDataStoreHolder == null)
		{
			LocalDataStoreManager.ValidateSlot(slot);
			return null;
		}
		return localDataStoreHolder.Store.GetData(slot);
	}

	/// <summary>Sets the data in the specified slot on the currently running thread, for that thread's current domain. For better performance, use fields marked with the <see cref="T:System.ThreadStaticAttribute" /> attribute instead.</summary>
	/// <param name="slot">The <see cref="T:System.LocalDataStoreSlot" /> in which to set the value. </param>
	/// <param name="data">The value to be set. </param>
	/// <filterpriority>1</filterpriority>
	[HostProtection(SecurityAction.LinkDemand, SharedState = true, ExternalThreading = true)]
	public static void SetData(LocalDataStoreSlot slot, object data)
	{
		LocalDataStoreHolder localDataStoreHolder = s_LocalDataStore;
		if (localDataStoreHolder == null)
		{
			localDataStoreHolder = (s_LocalDataStore = LocalDataStoreManager.CreateLocalDataStore());
		}
		localDataStoreHolder.Store.SetData(slot, data);
	}

	internal CultureInfo GetCurrentUICultureNoAppX()
	{
		if (m_CurrentUICulture == null)
		{
			CultureInfo defaultThreadCurrentUICulture = CultureInfo.DefaultThreadCurrentUICulture;
			if (defaultThreadCurrentUICulture == null)
			{
				return CultureInfo.UserDefaultUICulture;
			}
			return defaultThreadCurrentUICulture;
		}
		return m_CurrentUICulture;
	}

	private CultureInfo GetCurrentCultureNoAppX()
	{
		if (m_CurrentCulture == null)
		{
			CultureInfo defaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentCulture;
			if (defaultThreadCurrentCulture == null)
			{
				return CultureInfo.UserDefaultCulture;
			}
			return defaultThreadCurrentCulture;
		}
		return m_CurrentCulture;
	}

	private static void nativeInitCultureAccessors()
	{
		m_CurrentCulture = CultureInfo.ConstructCurrentCulture();
		m_CurrentUICulture = CultureInfo.ConstructCurrentUICulture();
	}

	/// <summary>Synchronizes memory access as follows: The processor executing the current thread cannot reorder instructions in such a way that memory accesses prior to the call to <see cref="M:System.Threading.Thread.MemoryBarrier" /> execute after memory accesses that follow the call to <see cref="M:System.Threading.Thread.MemoryBarrier" />.</summary>
	/// <filterpriority>2</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[SecuritySafeCritical]
	public static extern void MemoryBarrier();

	/// <summary>Retrieves the number of type information interfaces that an object provides (either 0 or 1).</summary>
	/// <param name="pcTInfo">Points to a location that receives the number of type information interfaces provided by the object.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _Thread.GetTypeInfoCount(out uint pcTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Retrieves the type information for an object, which can then be used to get the type information for an interface.</summary>
	/// <param name="iTInfo">The type information to return.</param>
	/// <param name="lcid">The locale identifier for the type information.</param>
	/// <param name="ppTInfo">Receives a pointer to the requested type information object.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _Thread.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
	{
		throw new NotImplementedException();
	}

	/// <summary>Maps a set of names to a corresponding set of dispatch identifiers.</summary>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="rgszNames">Passed-in array of names to be mapped.</param>
	/// <param name="cNames">Count of the names to be mapped.</param>
	/// <param name="lcid">The locale context in which to interpret the names.</param>
	/// <param name="rgDispId">Caller-allocated array which receives the IDs corresponding to the names.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _Thread.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
	{
		throw new NotImplementedException();
	}

	/// <summary>Provides access to properties and methods exposed by an object.</summary>
	/// <param name="dispIdMember">Identifies the member.</param>
	/// <param name="riid">Reserved for future use. Must be IID_NULL.</param>
	/// <param name="lcid">The locale context in which to interpret arguments.</param>
	/// <param name="wFlags">Flags describing the context of the call.</param>
	/// <param name="pDispParams">Pointer to a structure containing an array of arguments, an array of argument DISPIDs for named arguments, and counts for the number of elements in the arrays.</param>
	/// <param name="pVarResult">Pointer to the location where the result is to be stored.</param>
	/// <param name="pExcepInfo">Pointer to a structure that contains exception information.</param>
	/// <param name="puArgErr">The index of the first argument that has an error.</param>
	/// <exception cref="T:System.NotImplementedException">Late-bound access using the COM IDispatch interface is not supported.</exception>
	void _Thread.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
	{
		throw new NotImplementedException();
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern void ConstructInternalThread();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern byte[] ByteArrayToRootDomain(byte[] arr);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern byte[] ByteArrayToCurrentDomain(byte[] arr);

	private static void DeserializePrincipal(Thread th)
	{
		MemoryStream memoryStream = new MemoryStream(ByteArrayToCurrentDomain(th.Internal._serialized_principal));
		int num = memoryStream.ReadByte();
		switch (num)
		{
		case 0:
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			th.principal = (IPrincipal)binaryFormatter.Deserialize(memoryStream);
			th.principal_version = th.Internal._serialized_principal_version;
			break;
		}
		case 1:
		{
			BinaryReader binaryReader = new BinaryReader(memoryStream);
			string name = binaryReader.ReadString();
			string type = binaryReader.ReadString();
			int num2 = binaryReader.ReadInt32();
			string[] array = null;
			if (num2 >= 0)
			{
				array = new string[num2];
				for (int i = 0; i < num2; i++)
				{
					array[i] = binaryReader.ReadString();
				}
			}
			th.principal = new GenericPrincipal(new GenericIdentity(name, type), array);
			break;
		}
		case 2:
		case 3:
		{
			string[] roles = ((num == 2) ? null : new string[0]);
			th.principal = new GenericPrincipal(new GenericIdentity("", ""), roles);
			break;
		}
		}
	}

	private static void SerializePrincipal(Thread th, IPrincipal value)
	{
		MemoryStream memoryStream = new MemoryStream();
		bool flag = false;
		if (value.GetType() == typeof(GenericPrincipal))
		{
			GenericPrincipal genericPrincipal = (GenericPrincipal)value;
			if (genericPrincipal.Identity != null && genericPrincipal.Identity.GetType() == typeof(GenericIdentity))
			{
				GenericIdentity genericIdentity = (GenericIdentity)genericPrincipal.Identity;
				if (genericIdentity.Name == "" && genericIdentity.AuthenticationType == "")
				{
					if (genericPrincipal.Roles == null)
					{
						memoryStream.WriteByte(2);
						flag = true;
					}
					else if (genericPrincipal.Roles.Length == 0)
					{
						memoryStream.WriteByte(3);
						flag = true;
					}
				}
				else
				{
					memoryStream.WriteByte(1);
					BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
					binaryWriter.Write(genericPrincipal.Identity.Name);
					binaryWriter.Write(genericPrincipal.Identity.AuthenticationType);
					string[] roles = genericPrincipal.Roles;
					if (roles == null)
					{
						binaryWriter.Write(-1);
					}
					else
					{
						binaryWriter.Write(roles.Length);
						string[] array = roles;
						foreach (string value2 in array)
						{
							binaryWriter.Write(value2);
						}
					}
					binaryWriter.Flush();
					flag = true;
				}
			}
		}
		if (!flag)
		{
			memoryStream.WriteByte(0);
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			try
			{
				binaryFormatter.Serialize(memoryStream, value);
			}
			catch
			{
			}
		}
		th.Internal._serialized_principal = ByteArrayToRootDomain(memoryStream.ToArray());
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern Thread GetCurrentThread();

	/// <summary>Returns the current domain in which the current thread is running.</summary>
	/// <returns>An <see cref="T:System.AppDomain" /> representing the current application domain of the running thread.</returns>
	/// <filterpriority>2</filterpriority>
	public static AppDomain GetDomain()
	{
		return AppDomain.CurrentDomain;
	}

	/// <summary>Returns a unique application domain identifier.</summary>
	/// <returns>A 32-bit signed integer uniquely identifying the application domain.</returns>
	/// <filterpriority>2</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern int GetDomainID();

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern IntPtr Thread_internal(MulticastDelegate start);

	private Thread(InternalThread it)
	{
		internal_thread = it;
	}

	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	~Thread()
	{
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern string GetName_internal(InternalThread thread);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void SetName_internal(InternalThread thread, string name);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void Abort_internal(InternalThread thread, object stateInfo);

	/// <summary>Raises a <see cref="T:System.Threading.ThreadAbortException" /> in the thread on which it is invoked, to begin the process of terminating the thread. Calling this method usually terminates the thread.</summary>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.Threading.ThreadStateException">The thread that is being aborted is currently suspended.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlThread" />
	/// </PermissionSet>
	[SecurityPermission(SecurityAction.Demand, ControlThread = true)]
	public void Abort()
	{
		Abort_internal(Internal, null);
	}

	/// <summary>Raises a <see cref="T:System.Threading.ThreadAbortException" /> in the thread on which it is invoked, to begin the process of terminating the thread while also providing exception information about the thread termination. Calling this method usually terminates the thread.</summary>
	/// <param name="stateInfo">An object that contains application-specific information, such as state, which can be used by the thread being aborted. </param>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <exception cref="T:System.Threading.ThreadStateException">The thread that is being aborted is currently suspended.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlThread" />
	/// </PermissionSet>
	[SecurityPermission(SecurityAction.Demand, ControlThread = true)]
	public void Abort(object stateInfo)
	{
		Abort_internal(Internal, stateInfo);
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private extern object GetAbortExceptionState();

	private void ClearAbortReason()
	{
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void SpinWait_nop();

	/// <summary>Causes a thread to wait the number of times defined by the <paramref name="iterations" /> parameter.</summary>
	/// <param name="iterations">A 32-bit signed integer that defines how long a thread is to wait. </param>
	/// <filterpriority>1</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static void SpinWait(int iterations)
	{
		if (iterations >= 0)
		{
			while (iterations-- > 0)
			{
				SpinWait_nop();
			}
		}
	}

	private void StartInternal(IPrincipal principal, ref StackCrawlMark stackMark)
	{
		Internal._serialized_principal = CurrentThread.Internal._serialized_principal;
		if (Thread_internal(m_Delegate) == IntPtr.Zero)
		{
			throw new SystemException("Thread creation failed.");
		}
		m_ThreadStartArg = null;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void SetState(InternalThread thread, ThreadState set);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern void ClrState(InternalThread thread, ThreadState clr);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern ThreadState GetState(InternalThread thread);

	/// <summary>Reads the value of a field. The value is the latest written by any processor in a computer, regardless of the number of processors or the state of processor cache.</summary>
	/// <returns>The latest value written to the field by any processor.</returns>
	/// <param name="address">The field to be read. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern byte VolatileRead(ref byte address);

	/// <summary>Reads the value of a field. The value is the latest written by any processor in a computer, regardless of the number of processors or the state of processor cache.</summary>
	/// <returns>The latest value written to the field by any processor.</returns>
	/// <param name="address">The field to be read. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern double VolatileRead(ref double address);

	/// <summary>Reads the value of a field. The value is the latest written by any processor in a computer, regardless of the number of processors or the state of processor cache.</summary>
	/// <returns>The latest value written to the field by any processor.</returns>
	/// <param name="address">The field to be read. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern short VolatileRead(ref short address);

	/// <summary>Reads the value of a field. The value is the latest written by any processor in a computer, regardless of the number of processors or the state of processor cache.</summary>
	/// <returns>The latest value written to the field by any processor.</returns>
	/// <param name="address">The field to be read. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern int VolatileRead(ref int address);

	/// <summary>Reads the value of a field. The value is the latest written by any processor in a computer, regardless of the number of processors or the state of processor cache.</summary>
	/// <returns>The latest value written to the field by any processor.</returns>
	/// <param name="address">The field to be read. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern long VolatileRead(ref long address);

	/// <summary>Reads the value of a field. The value is the latest written by any processor in a computer, regardless of the number of processors or the state of processor cache.</summary>
	/// <returns>The latest value written to the field by any processor.</returns>
	/// <param name="address">The field to be read. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern IntPtr VolatileRead(ref IntPtr address);

	/// <summary>Reads the value of a field. The value is the latest written by any processor in a computer, regardless of the number of processors or the state of processor cache.</summary>
	/// <returns>The latest value written to the field by any processor.</returns>
	/// <param name="address">The field to be read. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern object VolatileRead(ref object address);

	/// <summary>Reads the value of a field. The value is the latest written by any processor in a computer, regardless of the number of processors or the state of processor cache.</summary>
	/// <returns>The latest value written to the field by any processor.</returns>
	/// <param name="address">The field to be read. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[CLSCompliant(false)]
	public static extern sbyte VolatileRead(ref sbyte address);

	/// <summary>Reads the value of a field. The value is the latest written by any processor in a computer, regardless of the number of processors or the state of processor cache.</summary>
	/// <returns>The latest value written to the field by any processor.</returns>
	/// <param name="address">The field to be read. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern float VolatileRead(ref float address);

	/// <summary>Reads the value of a field. The value is the latest written by any processor in a computer, regardless of the number of processors or the state of processor cache.</summary>
	/// <returns>The latest value written to the field by any processor.</returns>
	/// <param name="address">The field to be read. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[CLSCompliant(false)]
	public static extern ushort VolatileRead(ref ushort address);

	/// <summary>Reads the value of a field. The value is the latest written by any processor in a computer, regardless of the number of processors or the state of processor cache.</summary>
	/// <returns>The latest value written to the field by any processor.</returns>
	/// <param name="address">The field to be read. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[CLSCompliant(false)]
	public static extern uint VolatileRead(ref uint address);

	/// <summary>Reads the value of a field. The value is the latest written by any processor in a computer, regardless of the number of processors or the state of processor cache.</summary>
	/// <returns>The latest value written to the field by any processor.</returns>
	/// <param name="address">The field to be read. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[CLSCompliant(false)]
	public static extern ulong VolatileRead(ref ulong address);

	/// <summary>Reads the value of a field. The value is the latest written by any processor in a computer, regardless of the number of processors or the state of processor cache.</summary>
	/// <returns>The latest value written to the field by any processor.</returns>
	/// <param name="address">The field to be read. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[CLSCompliant(false)]
	public static extern UIntPtr VolatileRead(ref UIntPtr address);

	/// <summary>Writes a value to a field immediately, so that the value is visible to all processors in the computer.</summary>
	/// <param name="address">The field to which the value is to be written. </param>
	/// <param name="value">The value to be written. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void VolatileWrite(ref byte address, byte value);

	/// <summary>Writes a value to a field immediately, so that the value is visible to all processors in the computer.</summary>
	/// <param name="address">The field to which the value is to be written. </param>
	/// <param name="value">The value to be written. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void VolatileWrite(ref double address, double value);

	/// <summary>Writes a value to a field immediately, so that the value is visible to all processors in the computer.</summary>
	/// <param name="address">The field to which the value is to be written. </param>
	/// <param name="value">The value to be written. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void VolatileWrite(ref short address, short value);

	/// <summary>Writes a value to a field immediately, so that the value is visible to all processors in the computer.</summary>
	/// <param name="address">The field to which the value is to be written. </param>
	/// <param name="value">The value to be written. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void VolatileWrite(ref int address, int value);

	/// <summary>Writes a value to a field immediately, so that the value is visible to all processors in the computer.</summary>
	/// <param name="address">The field to which the value is to be written. </param>
	/// <param name="value">The value to be written. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void VolatileWrite(ref long address, long value);

	/// <summary>Writes a value to a field immediately, so that the value is visible to all processors in the computer.</summary>
	/// <param name="address">The field to which the value is to be written. </param>
	/// <param name="value">The value to be written. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void VolatileWrite(ref IntPtr address, IntPtr value);

	/// <summary>Writes a value to a field immediately, so that the value is visible to all processors in the computer.</summary>
	/// <param name="address">The field to which the value is to be written. </param>
	/// <param name="value">The value to be written. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void VolatileWrite(ref object address, object value);

	/// <summary>Writes a value to a field immediately, so that the value is visible to all processors in the computer.</summary>
	/// <param name="address">The field to which the value is to be written. </param>
	/// <param name="value">The value to be written. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[CLSCompliant(false)]
	public static extern void VolatileWrite(ref sbyte address, sbyte value);

	/// <summary>Writes a value to a field immediately, so that the value is visible to all processors in the computer.</summary>
	/// <param name="address">The field to which the value is to be written. </param>
	/// <param name="value">The value to be written. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	public static extern void VolatileWrite(ref float address, float value);

	/// <summary>Writes a value to a field immediately, so that the value is visible to all processors in the computer.</summary>
	/// <param name="address">The field to which the value is to be written. </param>
	/// <param name="value">The value to be written. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[CLSCompliant(false)]
	public static extern void VolatileWrite(ref ushort address, ushort value);

	/// <summary>Writes a value to a field immediately, so that the value is visible to all processors in the computer.</summary>
	/// <param name="address">The field to which the value is to be written. </param>
	/// <param name="value">The value to be written. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[CLSCompliant(false)]
	public static extern void VolatileWrite(ref uint address, uint value);

	/// <summary>Writes a value to a field immediately, so that the value is visible to all processors in the computer.</summary>
	/// <param name="address">The field to which the value is to be written. </param>
	/// <param name="value">The value to be written. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[CLSCompliant(false)]
	public static extern void VolatileWrite(ref ulong address, ulong value);

	/// <summary>Writes a value to a field immediately, so that the value is visible to all processors in the computer.</summary>
	/// <param name="address">The field to which the value is to be written. </param>
	/// <param name="value">The value to be written. </param>
	/// <filterpriority>1</filterpriority>
	[MethodImpl(MethodImplOptions.InternalCall)]
	[CLSCompliant(false)]
	public static extern void VolatileWrite(ref UIntPtr address, UIntPtr value);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern int SystemMaxStackStize();

	private static int GetProcessDefaultStackSize(int maxStackSize)
	{
		if (maxStackSize == 0)
		{
			return 0;
		}
		if (maxStackSize < 131072)
		{
			return 131072;
		}
		int pageSize = Environment.GetPageSize();
		if (maxStackSize % pageSize != 0)
		{
			maxStackSize = maxStackSize / (pageSize - 1) * pageSize;
		}
		return Math.Min(maxStackSize, SystemMaxStackStize());
	}

	private void SetStart(MulticastDelegate start, int maxStackSize)
	{
		m_Delegate = start;
		Internal.stack_size = maxStackSize;
	}

	/// <summary>Notifies a host that execution is about to enter a region of code in which the effects of a thread abort or unhandled exception might jeopardize other tasks in the application domain.</summary>
	/// <filterpriority>2</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public static void BeginCriticalRegion()
	{
		CurrentThread.Internal.critical_region_level++;
	}

	/// <summary>Notifies a host that execution is about to enter a region of code in which the effects of a thread abort or unhandled exception are limited to the current task.</summary>
	/// <filterpriority>2</filterpriority>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
	public static void EndCriticalRegion()
	{
		CurrentThread.Internal.critical_region_level--;
	}

	/// <summary>Notifies a host that managed code is about to execute instructions that depend on the identity of the current physical operating system thread.</summary>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlThread" />
	/// </PermissionSet>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public static void BeginThreadAffinity()
	{
	}

	/// <summary>Notifies a host that managed code has finished executing instructions that depend on the identity of the current physical operating system thread.</summary>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlThread" />
	/// </PermissionSet>
	[ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
	public static void EndThreadAffinity()
	{
	}

	/// <summary>Returns an <see cref="T:System.Threading.ApartmentState" /> value indicating the apartment state.</summary>
	/// <returns>One of the <see cref="T:System.Threading.ApartmentState" /> values indicating the apartment state of the managed thread. The default is <see cref="F:System.Threading.ApartmentState.Unknown" />.</returns>
	/// <filterpriority>1</filterpriority>
	public ApartmentState GetApartmentState()
	{
		ValidateThreadState();
		return (ApartmentState)Internal.apartment_state;
	}

	/// <summary>Sets the apartment state of a thread before it is started.</summary>
	/// <param name="state">The new apartment state.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="state" /> is not a valid apartment state.</exception>
	/// <exception cref="T:System.Threading.ThreadStateException">The thread has already been started.</exception>
	/// <exception cref="T:System.InvalidOperationException">The apartment state has already been initialized.</exception>
	/// <filterpriority>1</filterpriority>
	public void SetApartmentState(ApartmentState state)
	{
		if (!TrySetApartmentState(state))
		{
			throw new InvalidOperationException("Failed to set the specified COM apartment state.");
		}
	}

	/// <summary>Sets the apartment state of a thread before it is started.</summary>
	/// <returns>true if the apartment state is set; otherwise, false.</returns>
	/// <param name="state">The new apartment state.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="state" /> is not a valid apartment state.</exception>
	/// <exception cref="T:System.Threading.ThreadStateException">The thread has already been started.</exception>
	/// <filterpriority>1</filterpriority>
	public bool TrySetApartmentState(ApartmentState state)
	{
		if ((ThreadState & ThreadState.Unstarted) == 0)
		{
			throw new ThreadStateException("Thread was in an invalid state for the operation being executed.");
		}
		if (Internal.apartment_state != 2 && (ApartmentState)Internal.apartment_state != state)
		{
			return false;
		}
		Internal.apartment_state = (byte)state;
		return true;
	}

	/// <summary>Returns a hash code for the current thread.</summary>
	/// <returns>An integer hash code value.</returns>
	/// <filterpriority>2</filterpriority>
	[ComVisible(false)]
	public override int GetHashCode()
	{
		return ManagedThreadId;
	}

	[MethodImpl(MethodImplOptions.InternalCall)]
	internal static extern void GetStackTraces(out Thread[] threads, out object[] stack_frames);

	internal static Dictionary<Thread, StackTrace> Mono_GetStackTraces()
	{
		GetStackTraces(out var threads, out var stack_frames);
		Dictionary<Thread, StackTrace> dictionary = new Dictionary<Thread, StackTrace>();
		for (int i = 0; i < threads.Length; i++)
		{
			dictionary[threads[i]] = new StackTrace((StackFrame[])stack_frames[i]);
		}
		return dictionary;
	}

	/// <summary>Turns off automatic cleanup of runtime callable wrappers (RCW) for the current thread. </summary>
	public void DisableComObjectEagerCleanup()
	{
		throw new PlatformNotSupportedException();
	}

	private ThreadState ValidateThreadState()
	{
		ThreadState state = GetState(Internal);
		if ((state & ThreadState.Stopped) != 0)
		{
			throw new ThreadStateException("Thread is dead; state can not be accessed.");
		}
		return state;
	}
}
