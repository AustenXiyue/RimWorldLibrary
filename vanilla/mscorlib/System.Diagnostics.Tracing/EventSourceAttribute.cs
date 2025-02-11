namespace System.Diagnostics.Tracing;

/// <summary>Allows the event tracing for Windows (ETW) name to be defined independently of the name of the event source class.   </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class EventSourceAttribute : Attribute
{
	/// <summary>Gets or sets the name of the event source.</summary>
	/// <returns>The name of the event source.</returns>
	public string Name { get; set; }

	/// <summary>Gets or sets the event source identifier.</summary>
	/// <returns>The event source identifier.</returns>
	public string Guid { get; set; }

	/// <summary>Gets or sets the name of the localization resource file.</summary>
	/// <returns>The name of the localization resource file, or null if the localization resource file does not exist.</returns>
	public string LocalizationResources { get; set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.Tracing.EventSourceAttribute" /> class.</summary>
	public EventSourceAttribute()
	{
	}
}
