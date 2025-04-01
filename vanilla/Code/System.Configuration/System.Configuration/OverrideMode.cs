namespace System.Configuration;

/// <summary>Specifies the override behavior of a configuration element for configuration elements in child directories.</summary>
public enum OverrideMode
{
	/// <summary>The configuration setting of the element or group can be overridden by configuration settings that are in child directories.</summary>
	Allow = 1,
	/// <summary>The configuration setting of the element or group cannot be overridden by configuration settings that are in child directories.</summary>
	Deny = 2,
	/// <summary>The configuration setting of the element or group will be overridden by configuration settings that are in child directories if explicitly allowed by a parent element of the current configuration element or group. Permission to override is specified by using the OverrideMode attribute.</summary>
	Inherit = 0
}
