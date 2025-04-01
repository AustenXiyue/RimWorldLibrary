namespace System.Threading.Tasks;

/// <summary>Stores options that configure the operation of methods on the <see cref="T:System.Threading.Tasks.Parallel" /> class.</summary>
public class ParallelOptions
{
	private TaskScheduler m_scheduler;

	private int m_maxDegreeOfParallelism;

	private CancellationToken m_cancellationToken;

	/// <summary>Gets or sets the <see cref="T:System.Threading.Tasks.TaskScheduler" /> associated with this <see cref="T:System.Threading.Tasks.ParallelOptions" /> instance. Setting this property to null indicates that the current scheduler should be used.</summary>
	/// <returns>The task scheduler that is associated with this instance.</returns>
	public TaskScheduler TaskScheduler
	{
		get
		{
			return m_scheduler;
		}
		set
		{
			m_scheduler = value;
		}
	}

	internal TaskScheduler EffectiveTaskScheduler
	{
		get
		{
			if (m_scheduler == null)
			{
				return TaskScheduler.Current;
			}
			return m_scheduler;
		}
	}

	/// <summary>Gets or sets the maximum degree of parallelism enabled by this ParallelOptions instance.</summary>
	/// <returns>An integer that represents the maximum degree of parallelism.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The exception that is thrown when this <see cref="P:System.Threading.Tasks.ParallelOptions.MaxDegreeOfParallelism" /> is set to 0 or some value less than -1.</exception>
	public int MaxDegreeOfParallelism
	{
		get
		{
			return m_maxDegreeOfParallelism;
		}
		set
		{
			if (value == 0 || value < -1)
			{
				throw new ArgumentOutOfRangeException("MaxDegreeOfParallelism");
			}
			m_maxDegreeOfParallelism = value;
		}
	}

	/// <summary>Gets or sets the <see cref="T:System.Threading.CancellationToken" /> associated with this <see cref="T:System.Threading.Tasks.ParallelOptions" /> instance.</summary>
	/// <returns>The token that is associated with this instance.</returns>
	public CancellationToken CancellationToken
	{
		get
		{
			return m_cancellationToken;
		}
		set
		{
			m_cancellationToken = value;
		}
	}

	internal int EffectiveMaxConcurrencyLevel
	{
		get
		{
			int num = MaxDegreeOfParallelism;
			int maximumConcurrencyLevel = EffectiveTaskScheduler.MaximumConcurrencyLevel;
			if (maximumConcurrencyLevel > 0 && maximumConcurrencyLevel != int.MaxValue)
			{
				num = ((num == -1) ? maximumConcurrencyLevel : Math.Min(maximumConcurrencyLevel, num));
			}
			return num;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Threading.Tasks.ParallelOptions" /> class.</summary>
	public ParallelOptions()
	{
		m_scheduler = TaskScheduler.Default;
		m_maxDegreeOfParallelism = -1;
		m_cancellationToken = CancellationToken.None;
	}
}
