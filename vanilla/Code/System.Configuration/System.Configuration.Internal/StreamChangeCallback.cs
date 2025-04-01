namespace System.Configuration.Internal;

/// <summary>Represents a method for hosts to call when a monitored stream has changed.</summary>
/// <param name="streamName">The name of the <see cref="T:System.IO.Stream" /> object performing I/O tasks on the configuration file.</param>
public delegate void StreamChangeCallback(string streamName);
