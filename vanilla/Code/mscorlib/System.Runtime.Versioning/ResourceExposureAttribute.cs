using System.Diagnostics;

namespace System.Runtime.Versioning;

/// <summary>Specifies the resource exposure for a member of a class. This class cannot be inherited.</summary>
[Conditional("RESOURCE_ANNOTATION_WORK")]
[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, Inherited = false)]
public sealed class ResourceExposureAttribute : Attribute
{
	private ResourceScope _resourceExposureLevel;

	/// <summary>Gets the resource exposure scope.</summary>
	/// <returns>A <see cref="T:System.Runtime.Versioning.ResourceScope" /> object.</returns>
	public ResourceScope ResourceExposureLevel => _resourceExposureLevel;

	/// <summary>Initializes a new instance of the <see cref="T:System.Runtime.Versioning.ResourceExposureAttribute" /> class with the specified exposure level.</summary>
	/// <param name="exposureLevel">The scope of the resource.</param>
	public ResourceExposureAttribute(ResourceScope exposureLevel)
	{
		_resourceExposureLevel = exposureLevel;
	}
}
