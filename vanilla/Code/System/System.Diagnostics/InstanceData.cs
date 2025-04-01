namespace System.Diagnostics;

/// <summary>Holds instance data associated with a performance counter sample.</summary>
/// <filterpriority>2</filterpriority>
public class InstanceData
{
	private string instanceName;

	private CounterSample sample;

	/// <summary>Gets the instance name associated with this instance data.</summary>
	/// <returns>The name of an instance associated with the performance counter.</returns>
	/// <filterpriority>2</filterpriority>
	public string InstanceName => instanceName;

	/// <summary>Gets the raw data value associated with the performance counter sample.</summary>
	/// <returns>The raw value read by the performance counter sample associated with the <see cref="P:System.Diagnostics.InstanceData.Sample" /> property.</returns>
	/// <filterpriority>2</filterpriority>
	public long RawValue => sample.RawValue;

	/// <summary>Gets the performance counter sample that generated this data.</summary>
	/// <returns>A <see cref="T:System.Diagnostics.CounterSample" /> taken from the instance specified by the <see cref="P:System.Diagnostics.InstanceData.InstanceName" /> property.</returns>
	/// <filterpriority>2</filterpriority>
	public CounterSample Sample => sample;

	/// <summary>Initializes a new instance of the InstanceData class, using the specified sample and performance counter instance.</summary>
	/// <param name="instanceName">The name of an instance associated with the performance counter. </param>
	/// <param name="sample">A <see cref="T:System.Diagnostics.CounterSample" /> taken from the instance specified by the <paramref name="instanceName" /> parameter. </param>
	public InstanceData(string instanceName, CounterSample sample)
	{
		this.instanceName = instanceName;
		this.sample = sample;
	}
}
