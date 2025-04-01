using System.Runtime.CompilerServices;
using System.Security.Permissions;

namespace System.Diagnostics;

/// <summary>Represents a performance object, which defines a category of performance counters.</summary>
/// <filterpriority>2</filterpriority>
[PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
public sealed class PerformanceCounterCategory
{
	private string categoryName;

	private string machineName;

	private PerformanceCounterCategoryType type = PerformanceCounterCategoryType.Unknown;

	/// <summary>Gets the category's help text.</summary>
	/// <returns>A description of the performance object that this category measures.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Diagnostics.PerformanceCounterCategory.CategoryName" /> property is null. The category name must be set before getting the category help. </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">A call to an underlying system API failed. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public string CategoryHelp
	{
		get
		{
			string text = CategoryHelpInternal(categoryName, machineName);
			if (text != null)
			{
				return text;
			}
			throw new InvalidOperationException();
		}
	}

	/// <summary>Gets or sets the name of the performance object that defines this category.</summary>
	/// <returns>The name of the performance counter category, or performance object, with which to associate this <see cref="T:System.Diagnostics.PerformanceCounterCategory" /> instance.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Diagnostics.PerformanceCounterCategory.CategoryName" /> is an empty string (""). </exception>
	/// <exception cref="T:System.ArgumentNullException">The <see cref="P:System.Diagnostics.PerformanceCounterCategory.CategoryName" /> is null. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public string CategoryName
	{
		get
		{
			return categoryName;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value == "")
			{
				throw new ArgumentException("value");
			}
			categoryName = value;
		}
	}

	/// <summary>Gets or sets the name of the computer on which this category exists.</summary>
	/// <returns>The name of the computer on which the performance counter category and its associated counters exist.</returns>
	/// <exception cref="T:System.ArgumentException">The <see cref="P:System.Diagnostics.PerformanceCounterCategory.MachineName" /> syntax is invalid. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public string MachineName
	{
		get
		{
			return machineName;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (value == "")
			{
				throw new ArgumentException("value");
			}
			machineName = value;
		}
	}

	/// <summary>Gets the performance counter category type.</summary>
	/// <returns>One of the <see cref="T:System.Diagnostics.PerformanceCounterCategoryType" /> values. </returns>
	/// <filterpriority>1</filterpriority>
	public PerformanceCounterCategoryType CategoryType => type;

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool CategoryDelete(string name);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern string CategoryHelpInternal(string category, string machine);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool CounterCategoryExists(string counter, string category, string machine);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern bool Create(string categoryName, string categoryHelp, PerformanceCounterCategoryType categoryType, CounterCreationData[] items);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern int InstanceExistsInternal(string instance, string category, string machine);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern string[] GetCategoryNames(string machine);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern string[] GetCounterNames(string category, string machine);

	[MethodImpl(MethodImplOptions.InternalCall)]
	private static extern string[] GetInstanceNames(string category, string machine);

	private static void CheckCategory(string categoryName)
	{
		if (categoryName == null)
		{
			throw new ArgumentNullException("categoryName");
		}
		if (categoryName == "")
		{
			throw new ArgumentException("categoryName");
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.PerformanceCounterCategory" /> class, leaves the <see cref="P:System.Diagnostics.PerformanceCounterCategory.CategoryName" /> property empty, and sets the <see cref="P:System.Diagnostics.PerformanceCounterCategory.MachineName" /> property to the local computer.</summary>
	public PerformanceCounterCategory()
		: this("", ".")
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.PerformanceCounterCategory" /> class, sets the <see cref="P:System.Diagnostics.PerformanceCounterCategory.CategoryName" /> property to the specified value, and sets the <see cref="P:System.Diagnostics.PerformanceCounterCategory.MachineName" /> property to the local computer.</summary>
	/// <param name="categoryName">The name of the performance counter category, or performance object, with which to associate this <see cref="T:System.Diagnostics.PerformanceCounterCategory" /> instance. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="categoryName" /> is an empty string (""). </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="categoryName" /> is null. </exception>
	public PerformanceCounterCategory(string categoryName)
		: this(categoryName, ".")
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Diagnostics.PerformanceCounterCategory" /> class and sets the <see cref="P:System.Diagnostics.PerformanceCounterCategory.CategoryName" /> and <see cref="P:System.Diagnostics.PerformanceCounterCategory.MachineName" /> properties to the specified values.</summary>
	/// <param name="categoryName">The name of the performance counter category, or performance object, with which to associate this <see cref="T:System.Diagnostics.PerformanceCounterCategory" /> instance. </param>
	/// <param name="machineName">The computer on which the performance counter category and its associated counters exist. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="categoryName" /> is an empty string ("").-or- The <paramref name="machineName" /> syntax is invalid. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="categoryName" /> is null. </exception>
	public PerformanceCounterCategory(string categoryName, string machineName)
	{
		CheckCategory(categoryName);
		if (machineName == null)
		{
			throw new ArgumentNullException("machineName");
		}
		this.categoryName = categoryName;
		this.machineName = machineName;
	}

	/// <summary>Determines whether the specified counter is registered to this category, which is indicated by the <see cref="P:System.Diagnostics.PerformanceCounterCategory.CategoryName" /> and <see cref="P:System.Diagnostics.PerformanceCounterCategory.MachineName" /> properties.</summary>
	/// <returns>true if the counter is registered to the category that is specified by the <see cref="P:System.Diagnostics.PerformanceCounterCategory.CategoryName" /> and <see cref="P:System.Diagnostics.PerformanceCounterCategory.MachineName" /> properties; otherwise, false.</returns>
	/// <param name="counterName">The name of the performance counter to look for. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="counterName" /> is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Diagnostics.PerformanceCounterCategory.CategoryName" /> property has not been set. </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">A call to an underlying system API failed. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">Code that is executing without administrative privileges attempted to read a performance counter.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public bool CounterExists(string counterName)
	{
		return CounterExists(counterName, categoryName, machineName);
	}

	/// <summary>Determines whether the specified counter is registered to the specified category on the local computer.</summary>
	/// <returns>true, if the counter is registered to the specified category on the local computer; otherwise, false.</returns>
	/// <param name="counterName">The name of the performance counter to look for. </param>
	/// <param name="categoryName">The name of the performance counter category, or performance object, with which the specified performance counter is associated. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="categoryName" /> is null.-or- The <paramref name="counterName" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="categoryName" /> is an empty string (""). </exception>
	/// <exception cref="T:System.InvalidOperationException">The category name does not exist. </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">A call to an underlying system API failed. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">Code that is executing without administrative privileges attempted to read a performance counter.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static bool CounterExists(string counterName, string categoryName)
	{
		return CounterExists(counterName, categoryName, ".");
	}

	/// <summary>Determines whether the specified counter is registered to the specified category on a remote computer.</summary>
	/// <returns>true, if the counter is registered to the specified category on the specified computer; otherwise, false.</returns>
	/// <param name="counterName">The name of the performance counter to look for. </param>
	/// <param name="categoryName">The name of the performance counter category, or performance object, with which the specified performance counter is associated. </param>
	/// <param name="machineName">The name of the computer on which the performance counter category and its associated counters exist. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="categoryName" /> is null.-or- The <paramref name="counterName" /> is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="categoryName" /> is an empty string ("").-or- The <paramref name="machineName" /> is invalid. </exception>
	/// <exception cref="T:System.InvalidOperationException">The category name does not exist. </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">A call to an underlying system API failed. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">Code that is executing without administrative privileges attempted to read a performance counter.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static bool CounterExists(string counterName, string categoryName, string machineName)
	{
		if (counterName == null)
		{
			throw new ArgumentNullException("counterName");
		}
		CheckCategory(categoryName);
		if (machineName == null)
		{
			throw new ArgumentNullException("machineName");
		}
		return CounterCategoryExists(counterName, categoryName, machineName);
	}

	/// <summary>Registers the custom performance counter category containing the specified counters on the local computer.</summary>
	/// <returns>A <see cref="T:System.Diagnostics.PerformanceCounterCategory" /> that is associated with the new custom category, or performance object.</returns>
	/// <param name="categoryName">The name of the custom performance counter category to create and register with the system. </param>
	/// <param name="categoryHelp">A description of the custom category. </param>
	/// <param name="counterData">A <see cref="T:System.Diagnostics.CounterCreationDataCollection" /> that specifies the counters to create as part of the new category. </param>
	/// <exception cref="T:System.ArgumentException">A counter name that is specified within the <paramref name="counterData" /> collection is null or an empty string ("").-or- A counter that is specified within the <paramref name="counterData" /> collection already exists.-or- The <paramref name="counterName" /> parameter has invalid syntax. It might contain backslash characters ("\") or have length greater than 80 characters. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="categoryName" /> parameter is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The category already exists on the local computer.-or- The layout of the <paramref name="counterData" /> collection is incorrect for base counters. A counter of type AverageCount64, AverageTimer32, CounterMultiTimer, CounterMultiTimerInverse, CounterMultiTimer100Ns, CounterMultiTimer100NsInverse, RawFraction, SampleFraction or SampleCounter has to be immediately followed by one of the base counter types (AverageBase, MultiBase, RawBase, or SampleBase). </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">A call to an underlying system API failed. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">Code that is executing without administrative privileges attempted to read a performance counter.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[Obsolete("Use another overload that uses PerformanceCounterCategoryType instead")]
	public static PerformanceCounterCategory Create(string categoryName, string categoryHelp, CounterCreationDataCollection counterData)
	{
		return Create(categoryName, categoryHelp, PerformanceCounterCategoryType.Unknown, counterData);
	}

	/// <summary>Registers a custom performance counter category containing a single counter of type NumberOfItems32 on the local computer.</summary>
	/// <returns>A <see cref="T:System.Diagnostics.PerformanceCounterCategory" /> that is associated with the new system category, or performance object.</returns>
	/// <param name="categoryName">The name of the custom performance counter category to create and register with the system. </param>
	/// <param name="categoryHelp">A description of the custom category. </param>
	/// <param name="counterName">The name of a new counter, of type NumberOfItems32, to create as part of the new category. </param>
	/// <param name="counterHelp">A description of the counter that is associated with the new custom category. </param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="counterName" /> is null or is an empty string ("").-or- The counter that is specified by <paramref name="counterName" /> already exists.-or- <paramref name="counterName" /> has invalid syntax. It might contain backslash characters ("\") or have length greater than 80 characters. </exception>
	/// <exception cref="T:System.InvalidOperationException">The category already exists on the local computer. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="categoryName" /> is null. -or-<paramref name="counterHelp" /> is null.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">A call to an underlying system API failed. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">Code that is executing without administrative privileges attempted to read a performance counter.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[Obsolete("Use another overload that uses PerformanceCounterCategoryType instead")]
	public static PerformanceCounterCategory Create(string categoryName, string categoryHelp, string counterName, string counterHelp)
	{
		return Create(categoryName, categoryHelp, PerformanceCounterCategoryType.Unknown, counterName, counterHelp);
	}

	/// <summary>Registers the custom performance counter category containing the specified counters on the local computer.</summary>
	/// <returns>A <see cref="T:System.Diagnostics.PerformanceCounterCategory" /> that is associated with the new custom category, or performance object.</returns>
	/// <param name="categoryName">The name of the custom performance counter category to create and register with the system.</param>
	/// <param name="categoryHelp">A description of the custom category.</param>
	/// <param name="categoryType">One of the <see cref="T:System.Diagnostics.PerformanceCounterCategoryType" />  values. </param>
	/// <param name="counterData">A <see cref="T:System.Diagnostics.CounterCreationDataCollection" /> that specifies the counters to create as part of the new category.</param>
	/// <exception cref="T:System.ArgumentException">A counter name that is specified within the <paramref name="counterData" /> collection is null or an empty string ("").-or- A counter that is specified within the <paramref name="counterData" /> collection already exists.-or- <paramref name="counterName" /> has invalid syntax. It might contain backslash characters ("\") or have length greater than 80 characters. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="categoryName" /> is null. -or-<paramref name="counterData" /> is null.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   <paramref name="categoryType" /> value is outside of the range of the following values: MultiInstance, SingleInstance, or Unknown.</exception>
	/// <exception cref="T:System.InvalidOperationException">The category already exists on the local computer.-or- The layout of the <paramref name="counterData" /> collection is incorrect for base counters. A counter of type AverageCount64, AverageTimer32, CounterMultiTimer, CounterMultiTimerInverse, CounterMultiTimer100Ns, CounterMultiTimer100NsInverse, RawFraction, SampleFraction, or SampleCounter must be immediately followed by one of the base counter types (AverageBase, MultiBase, RawBase, or SampleBase). </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">A call to an underlying system API failed. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">Code that is executing without administrative privileges attempted to read a performance counter.</exception>
	/// <filterpriority>1</filterpriority>
	public static PerformanceCounterCategory Create(string categoryName, string categoryHelp, PerformanceCounterCategoryType categoryType, CounterCreationDataCollection counterData)
	{
		CheckCategory(categoryName);
		if (counterData == null)
		{
			throw new ArgumentNullException("counterData");
		}
		if (counterData.Count == 0)
		{
			throw new ArgumentException("counterData");
		}
		CounterCreationData[] array = new CounterCreationData[counterData.Count];
		counterData.CopyTo(array, 0);
		if (!Create(categoryName, categoryHelp, categoryType, array))
		{
			throw new InvalidOperationException();
		}
		return new PerformanceCounterCategory(categoryName, categoryHelp);
	}

	/// <summary>Registers the custom performance counter category containing a single counter of type <see cref="F:System.Diagnostics.PerformanceCounterType.NumberOfItems32" /> on the local computer.</summary>
	/// <returns>A <see cref="T:System.Diagnostics.PerformanceCounterCategory" /> that is associated with the new system category, or performance object.</returns>
	/// <param name="categoryName">The name of the custom performance counter category to create and register with the system.</param>
	/// <param name="categoryHelp">A description of the custom category.</param>
	/// <param name="categoryType">One of the <see cref="T:System.Diagnostics.PerformanceCounterCategoryType" />  values specifying whether the category is <see cref="F:System.Diagnostics.PerformanceCounterCategoryType.MultiInstance" />, <see cref="F:System.Diagnostics.PerformanceCounterCategoryType.SingleInstance" />, or <see cref="F:System.Diagnostics.PerformanceCounterCategoryType.Unknown" />.</param>
	/// <param name="counterName">The name of a new counter to create as part of the new category.</param>
	/// <param name="counterHelp">A description of the counter that is associated with the new custom category.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="counterName" /> is null or is an empty string ("").-or- The counter that is specified by <paramref name="counterName" /> already exists.-or- <paramref name="counterName" /> has invalid syntax. It might contain backslash characters ("\") or have length greater than 80 characters. </exception>
	/// <exception cref="T:System.InvalidOperationException">The category already exists on the local computer. </exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="categoryName" /> is null. -or-<paramref name="counterHelp" /> is null.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">A call to an underlying system API failed. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">Code that is executing without administrative privileges attempted to read a performance counter.</exception>
	/// <filterpriority>1</filterpriority>
	public static PerformanceCounterCategory Create(string categoryName, string categoryHelp, PerformanceCounterCategoryType categoryType, string counterName, string counterHelp)
	{
		CheckCategory(categoryName);
		if (!Create(categoryName, categoryHelp, categoryType, new CounterCreationData[1]
		{
			new CounterCreationData(counterName, counterHelp, PerformanceCounterType.NumberOfItems32)
		}))
		{
			throw new InvalidOperationException();
		}
		return new PerformanceCounterCategory(categoryName, categoryHelp);
	}

	/// <summary>Removes the category and its associated counters from the local computer.</summary>
	/// <param name="categoryName">The name of the custom performance counter category to delete. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="categoryName" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="categoryName" /> parameter has invalid syntax. It might contain backslash characters ("\") or have length greater than 80 characters. </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">A call to an underlying system API failed. </exception>
	/// <exception cref="T:System.InvalidOperationException">The category cannot be deleted because it is not a custom category. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">Code that is executing without administrative privileges attempted to read a performance counter.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static void Delete(string categoryName)
	{
		CheckCategory(categoryName);
		if (!CategoryDelete(categoryName))
		{
			throw new InvalidOperationException();
		}
	}

	/// <summary>Determines whether the category is registered on the local computer.</summary>
	/// <returns>true if the category is registered; otherwise, false.</returns>
	/// <param name="categoryName">The name of the performance counter category to look for. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="categoryName" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="categoryName" /> parameter is an empty string (""). </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">A call to an underlying system API failed. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">Code that is executing without administrative privileges attempted to read a performance counter.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static bool Exists(string categoryName)
	{
		return Exists(categoryName, ".");
	}

	/// <summary>Determines whether the category is registered on the specified computer.</summary>
	/// <returns>true if the category is registered; otherwise, false.</returns>
	/// <param name="categoryName">The name of the performance counter category to look for. </param>
	/// <param name="machineName">The name of the computer to examine for the category. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="categoryName" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="categoryName" /> parameter is an empty string ("").-or- The <paramref name="machineName" /> parameter is invalid. </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">A call to an underlying system API failed. </exception>
	/// <exception cref="T:System.IO.IOException">The network path cannot be found.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.-or-Code that is executing without administrative privileges attempted to read a performance counter.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static bool Exists(string categoryName, string machineName)
	{
		CheckCategory(categoryName);
		return CounterCategoryExists(null, categoryName, machineName);
	}

	/// <summary>Retrieves a list of the performance counter categories that are registered on the local computer.</summary>
	/// <returns>An array of <see cref="T:System.Diagnostics.PerformanceCounterCategory" /> objects indicating the categories that are registered on the local computer.</returns>
	/// <exception cref="T:System.ComponentModel.Win32Exception">A call to an underlying system API failed. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">Code that is executing without administrative privileges attempted to read a performance counter.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static PerformanceCounterCategory[] GetCategories()
	{
		return GetCategories(".");
	}

	/// <summary>Retrieves a list of the performance counter categories that are registered on the specified computer.</summary>
	/// <returns>An array of <see cref="T:System.Diagnostics.PerformanceCounterCategory" /> objects indicating the categories that are registered on the specified computer.</returns>
	/// <param name="machineName">The computer to look on. </param>
	/// <exception cref="T:System.ArgumentException">The <paramref name="machineName" /> parameter is invalid. </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">A call to an underlying system API failed. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">Code that is executing without administrative privileges attempted to read a performance counter.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static PerformanceCounterCategory[] GetCategories(string machineName)
	{
		if (machineName == null)
		{
			throw new ArgumentNullException("machineName");
		}
		string[] categoryNames = GetCategoryNames(machineName);
		PerformanceCounterCategory[] array = new PerformanceCounterCategory[categoryNames.Length];
		for (int i = 0; i < categoryNames.Length; i++)
		{
			array[i] = new PerformanceCounterCategory(categoryNames[i], machineName);
		}
		return array;
	}

	/// <summary>Retrieves a list of the counters in a performance counter category that contains exactly one instance.</summary>
	/// <returns>An array of <see cref="T:System.Diagnostics.PerformanceCounter" /> objects indicating the counters that are associated with this single-instance performance counter category.</returns>
	/// <exception cref="T:System.ArgumentException">The category is not a single instance. </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">A call to an underlying system API failed. </exception>
	/// <exception cref="T:System.InvalidOperationException">The category does not have an associated instance.</exception>
	/// <exception cref="T:System.UnauthorizedAccessException">Code that is executing without administrative privileges attempted to read a performance counter.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public PerformanceCounter[] GetCounters()
	{
		return GetCounters("");
	}

	/// <summary>Retrieves a list of the counters in a performance counter category that contains one or more instances.</summary>
	/// <returns>An array of <see cref="T:System.Diagnostics.PerformanceCounter" /> objects indicating the counters that are associated with the specified object instance of this performance counter category.</returns>
	/// <param name="instanceName">The performance object instance for which to retrieve the list of associated counters. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="instanceName" /> parameter is null. </exception>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Diagnostics.PerformanceCounterCategory.CategoryName" /> property for this <see cref="T:System.Diagnostics.PerformanceCounterCategory" /> instance has not been set.-or- The category does not contain the instance that is specified by the <paramref name="instanceName" /> parameter. </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">A call to an underlying system API failed. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">Code that is executing without administrative privileges attempted to read a performance counter.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	public PerformanceCounter[] GetCounters(string instanceName)
	{
		string[] counterNames = GetCounterNames(categoryName, machineName);
		PerformanceCounter[] array = new PerformanceCounter[counterNames.Length];
		for (int i = 0; i < counterNames.Length; i++)
		{
			array[i] = new PerformanceCounter(categoryName, counterNames[i], instanceName, machineName);
		}
		return array;
	}

	/// <summary>Retrieves the list of performance object instances that are associated with this category.</summary>
	/// <returns>An array of strings representing the performance object instance names that are associated with this category or, if the category contains only one performance object instance, a single-entry array that contains an empty string ("").</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Diagnostics.PerformanceCounterCategory.CategoryName" /> property is null. The property might not have been set. -or-The category does not have an associated instance.</exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">A call to an underlying system API failed. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">Code that is executing without administrative privileges attempted to read a performance counter.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public string[] GetInstanceNames()
	{
		return GetInstanceNames(categoryName, machineName);
	}

	/// <summary>Determines whether the specified performance object instance exists in the category that is identified by this <see cref="T:System.Diagnostics.PerformanceCounterCategory" /> object's <see cref="P:System.Diagnostics.PerformanceCounterCategory.CategoryName" /> property.</summary>
	/// <returns>true if the category contains the specified performance object instance; otherwise, false.</returns>
	/// <param name="instanceName">The performance object instance in this performance counter category to search for. </param>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Diagnostics.PerformanceCounterCategory.CategoryName" /> property is null. The property might not have been set. </exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="instanceName" /> parameter is null. </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">A call to an underlying system API failed. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">Code that is executing without administrative privileges attempted to read a performance counter.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public bool InstanceExists(string instanceName)
	{
		return InstanceExists(instanceName, categoryName, machineName);
	}

	/// <summary>Determines whether a specified category on the local computer contains the specified performance object instance.</summary>
	/// <returns>true if the category contains the specified performance object instance; otherwise, false.</returns>
	/// <param name="instanceName">The performance object instance to search for. </param>
	/// <param name="categoryName">The performance counter category to search. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="instanceName" /> parameter is null.-or- The <paramref name="categoryName" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="categoryName" /> parameter is an empty string (""). </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">A call to an underlying system API failed. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">Code that is executing without administrative privileges attempted to read a performance counter.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static bool InstanceExists(string instanceName, string categoryName)
	{
		return InstanceExists(instanceName, categoryName, ".");
	}

	/// <summary>Determines whether a specified category on a specified computer contains the specified performance object instance.</summary>
	/// <returns>true if the category contains the specified performance object instance; otherwise, false.</returns>
	/// <param name="instanceName">The performance object instance to search for. </param>
	/// <param name="categoryName">The performance counter category to search. </param>
	/// <param name="machineName">The name of the computer on which to look for the category instance pair. </param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="instanceName" /> parameter is null.-or- The <paramref name="categoryName" /> parameter is null. </exception>
	/// <exception cref="T:System.ArgumentException">The <paramref name="categoryName" /> parameter is an empty string ("").-or- The <paramref name="machineName" /> parameter is invalid. </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">A call to an underlying system API failed. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">Code that is executing without administrative privileges attempted to read a performance counter.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public static bool InstanceExists(string instanceName, string categoryName, string machineName)
	{
		if (instanceName == null)
		{
			throw new ArgumentNullException("instanceName");
		}
		CheckCategory(categoryName);
		if (machineName == null)
		{
			throw new ArgumentNullException("machineName");
		}
		return InstanceExistsInternal(instanceName, categoryName, machineName) switch
		{
			0 => false, 
			1 => true, 
			_ => throw new InvalidOperationException(), 
		};
	}

	/// <summary>Reads all the counter and performance object instance data that is associated with this performance counter category.</summary>
	/// <returns>An <see cref="T:System.Diagnostics.InstanceDataCollectionCollection" /> that contains the counter and performance object instance data for the category.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Diagnostics.PerformanceCounterCategory.CategoryName" /> property is null. The property might not have been set. </exception>
	/// <exception cref="T:System.ComponentModel.Win32Exception">A call to an underlying system API failed. </exception>
	/// <exception cref="T:System.UnauthorizedAccessException">Code that is executing without administrative privileges attempted to read a performance counter.</exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode" />
	/// </PermissionSet>
	[System.MonoTODO]
	public InstanceDataCollectionCollection ReadCategory()
	{
		throw new NotImplementedException();
	}
}
