using System.Security.Permissions;
using System.Threading;

namespace System.ComponentModel;

/// <summary>Executes an operation on a separate thread.</summary>
[DefaultEvent("DoWork")]
[SRDescription("Executes an operation on a separate thread.")]
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class BackgroundWorker : Component
{
	private delegate void WorkerThreadStartDelegate(object argument);

	private static readonly object doWorkKey = new object();

	private static readonly object runWorkerCompletedKey = new object();

	private static readonly object progressChangedKey = new object();

	private bool canCancelWorker;

	private bool workerReportsProgress;

	private bool cancellationPending;

	private bool isRunning;

	private AsyncOperation asyncOperation;

	private readonly WorkerThreadStartDelegate threadStart;

	private readonly SendOrPostCallback operationCompleted;

	private readonly SendOrPostCallback progressReporter;

	/// <summary>Gets a value indicating whether the application has requested cancellation of a background operation.</summary>
	/// <returns>true if the application has requested cancellation of a background operation; otherwise, false. The default is false.</returns>
	[Browsable(false)]
	[SRDescription("Has the user attempted to cancel the operation? To be accessed from DoWork event handler.")]
	public bool CancellationPending => cancellationPending;

	/// <summary>Gets a value indicating whether the <see cref="T:System.ComponentModel.BackgroundWorker" /> is running an asynchronous operation.</summary>
	/// <returns>true, if the <see cref="T:System.ComponentModel.BackgroundWorker" /> is running an asynchronous operation; otherwise, false.</returns>
	[Browsable(false)]
	[SRDescription("Is the worker still currently working on a background operation?")]
	public bool IsBusy => isRunning;

	/// <summary>Gets or sets a value indicating whether the <see cref="T:System.ComponentModel.BackgroundWorker" /> can report progress updates.</summary>
	/// <returns>true if the <see cref="T:System.ComponentModel.BackgroundWorker" /> supports progress updates; otherwise false. The default is false.</returns>
	[DefaultValue(false)]
	[SRDescription("Whether the worker will report progress.")]
	[SRCategory("Asynchronous")]
	public bool WorkerReportsProgress
	{
		get
		{
			return workerReportsProgress;
		}
		set
		{
			workerReportsProgress = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether the <see cref="T:System.ComponentModel.BackgroundWorker" /> supports asynchronous cancellation.</summary>
	/// <returns>true if the <see cref="T:System.ComponentModel.BackgroundWorker" /> supports cancellation; otherwise false. The default is false.</returns>
	[SRCategory("Asynchronous")]
	[SRDescription("Whether the worker supports cancellation.")]
	[DefaultValue(false)]
	public bool WorkerSupportsCancellation
	{
		get
		{
			return canCancelWorker;
		}
		set
		{
			canCancelWorker = value;
		}
	}

	/// <summary>Occurs when <see cref="M:System.ComponentModel.BackgroundWorker.RunWorkerAsync" /> is called.</summary>
	[SRDescription("Event handler to be run on a different thread when the operation begins.")]
	[SRCategory("Asynchronous")]
	public event DoWorkEventHandler DoWork
	{
		add
		{
			base.Events.AddHandler(doWorkKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(doWorkKey, value);
		}
	}

	/// <summary>Occurs when <see cref="M:System.ComponentModel.BackgroundWorker.ReportProgress(System.Int32)" /> is called.</summary>
	[SRCategory("Asynchronous")]
	[SRDescription("Raised when the worker thread indicates that some progress has been made.")]
	public event ProgressChangedEventHandler ProgressChanged
	{
		add
		{
			base.Events.AddHandler(progressChangedKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(progressChangedKey, value);
		}
	}

	/// <summary>Occurs when the background operation has completed, has been canceled, or has raised an exception.</summary>
	[SRDescription("Raised when the worker has completed (either through success, failure, or cancellation).")]
	[SRCategory("Asynchronous")]
	public event RunWorkerCompletedEventHandler RunWorkerCompleted
	{
		add
		{
			base.Events.AddHandler(runWorkerCompletedKey, value);
		}
		remove
		{
			base.Events.RemoveHandler(runWorkerCompletedKey, value);
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.BackgroundWorker" /> class.</summary>
	public BackgroundWorker()
	{
		threadStart = WorkerThreadStart;
		operationCompleted = AsyncOperationCompleted;
		progressReporter = ProgressReporter;
	}

	private void AsyncOperationCompleted(object arg)
	{
		isRunning = false;
		cancellationPending = false;
		OnRunWorkerCompleted((RunWorkerCompletedEventArgs)arg);
	}

	/// <summary>Requests cancellation of a pending background operation.</summary>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.ComponentModel.BackgroundWorker.WorkerSupportsCancellation" /> is false. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void CancelAsync()
	{
		if (!WorkerSupportsCancellation)
		{
			throw new InvalidOperationException(global::SR.GetString("This BackgroundWorker states that it doesn't support cancellation. Modify WorkerSupportsCancellation to state that it does support cancellation."));
		}
		cancellationPending = true;
	}

	/// <summary>Raises the <see cref="E:System.ComponentModel.BackgroundWorker.DoWork" /> event. </summary>
	/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
	protected virtual void OnDoWork(DoWorkEventArgs e)
	{
		((DoWorkEventHandler)base.Events[doWorkKey])?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.ComponentModel.BackgroundWorker.RunWorkerCompleted" /> event.</summary>
	/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
	protected virtual void OnRunWorkerCompleted(RunWorkerCompletedEventArgs e)
	{
		((RunWorkerCompletedEventHandler)base.Events[runWorkerCompletedKey])?.Invoke(this, e);
	}

	/// <summary>Raises the <see cref="E:System.ComponentModel.BackgroundWorker.ProgressChanged" /> event.</summary>
	/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data. </param>
	protected virtual void OnProgressChanged(ProgressChangedEventArgs e)
	{
		((ProgressChangedEventHandler)base.Events[progressChangedKey])?.Invoke(this, e);
	}

	private void ProgressReporter(object arg)
	{
		OnProgressChanged((ProgressChangedEventArgs)arg);
	}

	/// <summary>Raises the <see cref="E:System.ComponentModel.BackgroundWorker.ProgressChanged" /> event.</summary>
	/// <param name="percentProgress">The percentage, from 0 to 100, of the background operation that is complete. </param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.ComponentModel.BackgroundWorker.WorkerReportsProgress" /> property is set to false. </exception>
	public void ReportProgress(int percentProgress)
	{
		ReportProgress(percentProgress, null);
	}

	/// <summary>Raises the <see cref="E:System.ComponentModel.BackgroundWorker.ProgressChanged" /> event.</summary>
	/// <param name="percentProgress">The percentage, from 0 to 100, of the background operation that is complete.</param>
	/// <param name="userState">The state object passed to <see cref="M:System.ComponentModel.BackgroundWorker.RunWorkerAsync(System.Object)" />.</param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.ComponentModel.BackgroundWorker.WorkerReportsProgress" /> property is set to false. </exception>
	public void ReportProgress(int percentProgress, object userState)
	{
		if (!WorkerReportsProgress)
		{
			throw new InvalidOperationException(global::SR.GetString("This BackgroundWorker states that it doesn't report progress. Modify WorkerReportsProgress to state that it does report progress."));
		}
		ProgressChangedEventArgs progressChangedEventArgs = new ProgressChangedEventArgs(percentProgress, userState);
		if (asyncOperation != null)
		{
			asyncOperation.Post(progressReporter, progressChangedEventArgs);
		}
		else
		{
			progressReporter(progressChangedEventArgs);
		}
	}

	/// <summary>Starts execution of a background operation.</summary>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.ComponentModel.BackgroundWorker.IsBusy" /> is true.</exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
	/// </PermissionSet>
	public void RunWorkerAsync()
	{
		RunWorkerAsync(null);
	}

	/// <summary>Starts execution of a background operation.</summary>
	/// <param name="argument">A parameter for use by the background operation to be executed in the <see cref="E:System.ComponentModel.BackgroundWorker.DoWork" /> event handler. </param>
	/// <exception cref="T:System.InvalidOperationException">
	///   <see cref="P:System.ComponentModel.BackgroundWorker.IsBusy" /> is true. </exception>
	public void RunWorkerAsync(object argument)
	{
		if (isRunning)
		{
			throw new InvalidOperationException(global::SR.GetString("This BackgroundWorker is currently busy and cannot run multiple tasks concurrently."));
		}
		isRunning = true;
		cancellationPending = false;
		asyncOperation = AsyncOperationManager.CreateOperation(null);
		threadStart.BeginInvoke(argument, null, null);
	}

	private void WorkerThreadStart(object argument)
	{
		object result = null;
		Exception error = null;
		bool cancelled = false;
		try
		{
			DoWorkEventArgs doWorkEventArgs = new DoWorkEventArgs(argument);
			OnDoWork(doWorkEventArgs);
			if (doWorkEventArgs.Cancel)
			{
				cancelled = true;
			}
			else
			{
				result = doWorkEventArgs.Result;
			}
		}
		catch (Exception ex)
		{
			error = ex;
		}
		RunWorkerCompletedEventArgs arg = new RunWorkerCompletedEventArgs(result, error, cancelled);
		asyncOperation.PostOperationCompleted(operationCompleted, arg);
	}
}
