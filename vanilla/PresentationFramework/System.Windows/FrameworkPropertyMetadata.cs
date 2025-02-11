using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace System.Windows;

/// <summary>Reports or applies metadata for a dependency property, specifically adding framework-specific property system characteristics.</summary>
public class FrameworkPropertyMetadata : UIPropertyMetadata
{
	/// <summary> Gets or sets a value that indicates whether a dependency property potentially affects the measure pass during layout engine operations. </summary>
	/// <returns>true if the dependency property on which this metadata exists potentially affects the measure pass; otherwise, false. The default is false.</returns>
	/// <exception cref="T:System.InvalidOperationException">The metadata has already been applied to a dependency property operation, so that metadata is sealed and properties of the metadata cannot be set.</exception>
	public bool AffectsMeasure
	{
		get
		{
			return ReadFlag(MetadataFlags.FW_AffectsMeasureID);
		}
		set
		{
			if (base.IsSealed)
			{
				throw new InvalidOperationException(SR.TypeMetadataCannotChangeAfterUse);
			}
			WriteFlag(MetadataFlags.FW_AffectsMeasureID, value);
		}
	}

	/// <summary> Gets or sets a value that indicates whether a dependency property potentially affects the arrange pass during layout engine operations. </summary>
	/// <returns>true if the dependency property on which this metadata exists potentially affects the arrange pass; otherwise, false. The default is false.</returns>
	/// <exception cref="T:System.InvalidOperationException">The metadata has already been applied to a dependency property operation, so that metadata is sealed and properties of the metadata cannot be set.</exception>
	public bool AffectsArrange
	{
		get
		{
			return ReadFlag(MetadataFlags.FW_AffectsArrangeID);
		}
		set
		{
			if (base.IsSealed)
			{
				throw new InvalidOperationException(SR.TypeMetadataCannotChangeAfterUse);
			}
			WriteFlag(MetadataFlags.FW_AffectsArrangeID, value);
		}
	}

	/// <summary> Gets or sets a value that indicates whether a dependency property potentially affects the measure pass of its parent element's layout during layout engine operations. </summary>
	/// <returns>true if the dependency property on which this metadata exists potentially affects the measure pass specifically on its parent element; otherwise, false.The default is false. </returns>
	/// <exception cref="T:System.InvalidOperationException">The metadata has already been applied to a dependency property operation, so that metadata is sealed and properties of the metadata cannot be set.</exception>
	public bool AffectsParentMeasure
	{
		get
		{
			return ReadFlag(MetadataFlags.FW_AffectsParentMeasureID);
		}
		set
		{
			if (base.IsSealed)
			{
				throw new InvalidOperationException(SR.TypeMetadataCannotChangeAfterUse);
			}
			WriteFlag(MetadataFlags.FW_AffectsParentMeasureID, value);
		}
	}

	/// <summary> Gets or sets a value that indicates whether a dependency property potentially affects the arrange pass of its parent element's layout during layout engine operations. </summary>
	/// <returns>true if the dependency property on which this metadata exists potentially affects the arrange pass specifically on its parent element; otherwise, false. The default is false.</returns>
	/// <exception cref="T:System.InvalidOperationException">The metadata has already been applied to a dependency property operation, so that metadata is sealed and properties of the metadata cannot be set.</exception>
	public bool AffectsParentArrange
	{
		get
		{
			return ReadFlag(MetadataFlags.FW_AffectsParentArrangeID);
		}
		set
		{
			if (base.IsSealed)
			{
				throw new InvalidOperationException(SR.TypeMetadataCannotChangeAfterUse);
			}
			WriteFlag(MetadataFlags.FW_AffectsParentArrangeID, value);
		}
	}

	/// <summary>Gets or sets a value that indicates whether a dependency property potentially affects the general layout in some way that does not specifically influence arrangement or measurement, but would require a redraw. </summary>
	/// <returns>true if the dependency property on which this metadata exists affects rendering; otherwise, false. The default is false.</returns>
	/// <exception cref="T:System.InvalidOperationException">The metadata has already been applied to a dependency property operation, so that metadata is sealed and properties of the metadata cannot be set.</exception>
	public bool AffectsRender
	{
		get
		{
			return ReadFlag(MetadataFlags.FW_AffectsRenderID);
		}
		set
		{
			if (base.IsSealed)
			{
				throw new InvalidOperationException(SR.TypeMetadataCannotChangeAfterUse);
			}
			WriteFlag(MetadataFlags.FW_AffectsRenderID, value);
		}
	}

	/// <summary> Gets or sets a value that indicates whether the value of the dependency property is inheritable. </summary>
	/// <returns>true if the property value is inheritable; otherwise, false. The default is false.</returns>
	/// <exception cref="T:System.InvalidOperationException">The metadata has already been applied to a dependency property operation, so that metadata is sealed and properties of the metadata cannot be set.</exception>
	public bool Inherits
	{
		get
		{
			return base.IsInherited;
		}
		set
		{
			if (base.IsSealed)
			{
				throw new InvalidOperationException(SR.TypeMetadataCannotChangeAfterUse);
			}
			base.IsInherited = value;
			SetModified(MetadataFlags.FW_InheritsModifiedID);
		}
	}

	/// <summary>Gets or sets a value that indicates whether the property value inheritance evaluation should span across certain content boundaries in the logical tree of elements. </summary>
	/// <returns>true if the property value inheritance should span across certain content boundaries; otherwise, false. The default is false.</returns>
	/// <exception cref="T:System.InvalidOperationException">The metadata has already been applied to a dependency property operation, so that metadata is sealed and properties of the metadata cannot be set.</exception>
	public bool OverridesInheritanceBehavior
	{
		get
		{
			return ReadFlag(MetadataFlags.FW_OverridesInheritanceBehaviorID);
		}
		set
		{
			if (base.IsSealed)
			{
				throw new InvalidOperationException(SR.TypeMetadataCannotChangeAfterUse);
			}
			WriteFlag(MetadataFlags.FW_OverridesInheritanceBehaviorID, value);
			SetModified(MetadataFlags.FW_OverridesInheritanceBehaviorModifiedID);
		}
	}

	/// <summary> Gets or sets a value that indicates whether the dependency property supports data binding. </summary>
	/// <returns>true if the property does not support data binding; otherwise, false. The default is false.</returns>
	/// <exception cref="T:System.InvalidOperationException">The metadata has already been applied to a dependency property operation, so that metadata is sealed and properties of the metadata cannot be set.</exception>
	public bool IsNotDataBindable
	{
		get
		{
			return ReadFlag(MetadataFlags.FW_IsNotDataBindableID);
		}
		set
		{
			if (base.IsSealed)
			{
				throw new InvalidOperationException(SR.TypeMetadataCannotChangeAfterUse);
			}
			WriteFlag(MetadataFlags.FW_IsNotDataBindableID, value);
		}
	}

	/// <summary> Gets or sets a value that indicates whether the property binds two-way by default. </summary>
	/// <returns>true if the dependency property on which this metadata exists binds two-way by default; otherwise, false. The default is false.</returns>
	/// <exception cref="T:System.InvalidOperationException">The metadata has already been applied to a dependency property operation, so that metadata is sealed and properties of the metadata cannot be set.</exception>
	public bool BindsTwoWayByDefault
	{
		get
		{
			return ReadFlag(MetadataFlags.FW_BindsTwoWayByDefaultID);
		}
		set
		{
			if (base.IsSealed)
			{
				throw new InvalidOperationException(SR.TypeMetadataCannotChangeAfterUse);
			}
			WriteFlag(MetadataFlags.FW_BindsTwoWayByDefaultID, value);
		}
	}

	/// <summary>Gets or sets the default for <see cref="T:System.Windows.Data.UpdateSourceTrigger" /> to use when bindings for the property with this metadata are applied, which have their <see cref="T:System.Windows.Data.UpdateSourceTrigger" /> set to <see cref="F:System.Windows.Data.UpdateSourceTrigger.Default" />.</summary>
	/// <returns>A value of the enumeration, other than <see cref="F:System.Windows.Data.UpdateSourceTrigger.Default" />.</returns>
	/// <exception cref="T:System.ArgumentException">This property is set to <see cref="F:System.Windows.Data.UpdateSourceTrigger.Default" />; the value you set is supposed to become the default when requested by bindings.</exception>
	/// <exception cref="T:System.InvalidOperationException">The metadata has already been applied to a dependency property operation, so that metadata is sealed and properties of the metadata cannot be set.</exception>
	public UpdateSourceTrigger DefaultUpdateSourceTrigger
	{
		get
		{
			return (UpdateSourceTrigger)(((uint)_flags >> 30) & 3);
		}
		set
		{
			if (base.IsSealed)
			{
				throw new InvalidOperationException(SR.TypeMetadataCannotChangeAfterUse);
			}
			if (!BindingOperations.IsValidUpdateSourceTrigger(value))
			{
				throw new InvalidEnumArgumentException("value", (int)value, typeof(UpdateSourceTrigger));
			}
			if (value == UpdateSourceTrigger.Default)
			{
				throw new ArgumentException(SR.NoDefaultUpdateSourceTrigger, "value");
			}
			_flags = (MetadataFlags)((uint)(_flags & (MetadataFlags)1073741823u) | (uint)((int)value << 30));
			SetModified(MetadataFlags.FW_DefaultUpdateSourceTriggerModifiedID);
		}
	}

	/// <summary> Gets or sets a value that indicates whether this property contains journaling information that applications can or should store as part of a journaling implementation. </summary>
	/// <returns>true if journaling should be performed on the dependency property that this metadata is applied to; otherwise, false. The default is false.</returns>
	/// <exception cref="T:System.InvalidOperationException">The metadata has already been applied to a dependency property operation, so that metadata is sealed and properties of the metadata cannot be set.</exception>
	public bool Journal
	{
		get
		{
			return ReadFlag(MetadataFlags.FW_ShouldBeJournaledID);
		}
		set
		{
			if (base.IsSealed)
			{
				throw new InvalidOperationException(SR.TypeMetadataCannotChangeAfterUse);
			}
			WriteFlag(MetadataFlags.FW_ShouldBeJournaledID, value);
			SetModified(MetadataFlags.FW_ShouldBeJournaledModifiedID);
		}
	}

	/// <summary>Gets or sets a value that indicates whether sub-properties of the dependency property do not affect the rendering of the containing object. </summary>
	/// <returns>true if changes to sub-property values do not affect rendering if changed; otherwise, false. The default is false.</returns>
	/// <exception cref="T:System.InvalidOperationException">The metadata has already been applied to a dependency property operation, so that metadata is sealed and properties of the metadata cannot be set.</exception>
	public bool SubPropertiesDoNotAffectRender
	{
		get
		{
			return ReadFlag(MetadataFlags.FW_SubPropertiesDoNotAffectRenderID);
		}
		set
		{
			if (base.IsSealed)
			{
				throw new InvalidOperationException(SR.TypeMetadataCannotChangeAfterUse);
			}
			WriteFlag(MetadataFlags.FW_SubPropertiesDoNotAffectRenderID, value);
			SetModified(MetadataFlags.FW_SubPropertiesDoNotAffectRenderModifiedID);
		}
	}

	private bool ReadOnly
	{
		get
		{
			return ReadFlag(MetadataFlags.FW_ReadOnlyID);
		}
		set
		{
			if (base.IsSealed)
			{
				throw new InvalidOperationException(SR.TypeMetadataCannotChangeAfterUse);
			}
			WriteFlag(MetadataFlags.FW_ReadOnlyID, value);
		}
	}

	/// <summary> Gets a value that indicates whether data binding is supported for the dependency property. </summary>
	/// <returns>true if data binding is supported on the dependency property to which this metadata applies; otherwise, false. The default is true.</returns>
	public bool IsDataBindingAllowed
	{
		get
		{
			if (!ReadFlag(MetadataFlags.FW_IsNotDataBindableID))
			{
				return !ReadOnly;
			}
			return false;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.FrameworkPropertyMetadata" /> class. </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public FrameworkPropertyMetadata()
	{
		Initialize();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.FrameworkPropertyMetadata" /> class with the specified default value. </summary>
	/// <param name="defaultValue">The default value of the dependency property, usually provided as a value of a specific type.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="defaultValue" /> is set to <see cref="F:System.Windows.DependencyProperty.UnsetValue" />; see Remarks.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public FrameworkPropertyMetadata(object defaultValue)
		: base(defaultValue)
	{
		Initialize();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.FrameworkPropertyMetadata" /> class with the specified <see cref="T:System.Windows.PropertyChangedCallback" /> callback.</summary>
	/// <param name="propertyChangedCallback">A reference to a handler implementation that the property system will call whenever the effective value of the property changes.</param>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public FrameworkPropertyMetadata(PropertyChangedCallback propertyChangedCallback)
		: base(propertyChangedCallback)
	{
		Initialize();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.FrameworkPropertyMetadata" /> class with the specified callbacks. </summary>
	/// <param name="propertyChangedCallback">A reference to a handler implementation that the property system will call whenever the effective value of the property changes.</param>
	/// <param name="coerceValueCallback">A reference to a handler implementation will be called whenever the property system calls <see cref="M:System.Windows.DependencyObject.CoerceValue(System.Windows.DependencyProperty)" /> for this dependency property.</param>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public FrameworkPropertyMetadata(PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback)
		: base(propertyChangedCallback)
	{
		Initialize();
		base.CoerceValueCallback = coerceValueCallback;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.FrameworkPropertyMetadata" /> class with the provided default value and specified <see cref="T:System.Windows.PropertyChangedCallback" /> callback. </summary>
	/// <param name="defaultValue">The default value of the dependency property, usually provided as a value of a specific type.</param>
	/// <param name="propertyChangedCallback">A reference to a handler implementation that the property system will call whenever the effective value of the property changes.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="defaultValue" /> is set to <see cref="F:System.Windows.DependencyProperty.UnsetValue" />; see Remarks.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public FrameworkPropertyMetadata(object defaultValue, PropertyChangedCallback propertyChangedCallback)
		: base(defaultValue, propertyChangedCallback)
	{
		Initialize();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.FrameworkPropertyMetadata" /> class with the provided default value and specified callbacks.</summary>
	/// <param name="defaultValue">The default value of the dependency property, usually provided as a specific type.</param>
	/// <param name="propertyChangedCallback">A reference to a handler implementation that the property system will call whenever the effective value of the property changes.</param>
	/// <param name="coerceValueCallback">A reference to a handler implementation that will be called whenever the property system calls <see cref="M:System.Windows.DependencyObject.CoerceValue(System.Windows.DependencyProperty)" /> for this dependency property.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="defaultValue" /> is set to <see cref="F:System.Windows.DependencyProperty.UnsetValue" />; see Remarks.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public FrameworkPropertyMetadata(object defaultValue, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback)
		: base(defaultValue, propertyChangedCallback, coerceValueCallback)
	{
		Initialize();
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.FrameworkPropertyMetadata" /> class with the provided default value and framework-level metadata options. </summary>
	/// <param name="defaultValue">The default value of the dependency property, usually provided as a value of a specific type.</param>
	/// <param name="flags">The metadata option flags (a combination of <see cref="T:System.Windows.FrameworkPropertyMetadataOptions" /> values). These options specify characteristics of the dependency property that interact with systems such as layout or data binding.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="defaultValue" /> is set to <see cref="F:System.Windows.DependencyProperty.UnsetValue" />; see Remarks.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public FrameworkPropertyMetadata(object defaultValue, FrameworkPropertyMetadataOptions flags)
		: base(defaultValue)
	{
		TranslateFlags(flags);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.FrameworkPropertyMetadata" /> class with the provided default value and framework metadata options, and specified <see cref="T:System.Windows.PropertyChangedCallback" /> callback. </summary>
	/// <param name="defaultValue">The default value of the dependency property, usually provided as a value of a specific type.</param>
	/// <param name="flags">The metadata option flags (a combination of <see cref="T:System.Windows.FrameworkPropertyMetadataOptions" /> values). These options specify characteristics of the dependency property that interact with systems such as layout or data binding.</param>
	/// <param name="propertyChangedCallback">A reference to a handler implementation that the property system will call whenever the effective value of the property changes.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="defaultValue" /> is set to <see cref="F:System.Windows.DependencyProperty.UnsetValue" />; see Remarks.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public FrameworkPropertyMetadata(object defaultValue, FrameworkPropertyMetadataOptions flags, PropertyChangedCallback propertyChangedCallback)
		: base(defaultValue, propertyChangedCallback)
	{
		TranslateFlags(flags);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.FrameworkPropertyMetadata" /> class with the provided default value and framework metadata options, and specified callbacks. </summary>
	/// <param name="defaultValue">The default value of the dependency property, usually provided as a specific type.</param>
	/// <param name="flags">The metadata option flags (a combination of <see cref="T:System.Windows.FrameworkPropertyMetadataOptions" /> values). These options specify characteristics of the dependency property that interact with systems such as layout or data binding.</param>
	/// <param name="propertyChangedCallback">A reference to a handler implementation that the property system will call whenever the effective value of the property changes.</param>
	/// <param name="coerceValueCallback">A reference to a handler implementation that will be called whenever the property system calls <see cref="M:System.Windows.DependencyObject.CoerceValue(System.Windows.DependencyProperty)" /> against this property.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="defaultValue" /> is set to <see cref="F:System.Windows.DependencyProperty.UnsetValue" />; see Remarks.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public FrameworkPropertyMetadata(object defaultValue, FrameworkPropertyMetadataOptions flags, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback)
		: base(defaultValue, propertyChangedCallback, coerceValueCallback)
	{
		TranslateFlags(flags);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.FrameworkPropertyMetadata" /> class with the provided default value and framework metadata options, specified callbacks, and a Boolean that can be used to prevent animation of the property.</summary>
	/// <param name="defaultValue">The default value of the dependency property, usually provided as a specific type.</param>
	/// <param name="flags">The metadata option flags (a combination of <see cref="T:System.Windows.FrameworkPropertyMetadataOptions" /> values). These options specify characteristics of the dependency property that interact with systems such as layout or data binding.</param>
	/// <param name="propertyChangedCallback">A reference to a handler implementation that the property system will call whenever the effective value of the property changes.</param>
	/// <param name="coerceValueCallback">A reference to a handler implementation that will be called whenever the property system calls <see cref="M:System.Windows.DependencyObject.CoerceValue(System.Windows.DependencyProperty)" /> on this dependency property.</param>
	/// <param name="isAnimationProhibited">true to prevent the property system from animating the property that this metadata is applied to. Such properties will raise a run-time exception originating from the property system if animations of them are attempted. false to permit animating the property. The default is false.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="defaultValue" /> is set to <see cref="F:System.Windows.DependencyProperty.UnsetValue" />; see Remarks.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public FrameworkPropertyMetadata(object defaultValue, FrameworkPropertyMetadataOptions flags, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback, bool isAnimationProhibited)
		: base(defaultValue, propertyChangedCallback, coerceValueCallback, isAnimationProhibited)
	{
		TranslateFlags(flags);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.FrameworkPropertyMetadata" /> class with the provided default value and framework metadata options, specified callbacks, a Boolean that can be used to prevent animation of the property, and a data-binding update trigger default.</summary>
	/// <param name="defaultValue">The default value of the dependency property, usually provided as a specific type.</param>
	/// <param name="flags">The metadata option flags (a combination of <see cref="T:System.Windows.FrameworkPropertyMetadataOptions" /> values). These options specify characteristics of the dependency property that interact with systems such as layout or data binding.</param>
	/// <param name="propertyChangedCallback">A reference to a handler implementation that the property system will call whenever the effective value of the property changes.</param>
	/// <param name="coerceValueCallback">A reference to a handler implementation that will be called whenever the property system calls <see cref="M:System.Windows.DependencyObject.CoerceValue(System.Windows.DependencyProperty)" /> against this property.</param>
	/// <param name="isAnimationProhibited">true to prevent the property system from animating the property that this metadata is applied to. Such properties will raise a run-time exception originating from the property system if animations of them are attempted. The default is false.</param>
	/// <param name="defaultUpdateSourceTrigger">The <see cref="T:System.Windows.Data.UpdateSourceTrigger" /> to use when bindings for this property are applied that have their <see cref="T:System.Windows.Data.UpdateSourceTrigger" /> set to <see cref="F:System.Windows.Data.UpdateSourceTrigger.Default" />.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="defaultValue" /> is set to <see cref="F:System.Windows.DependencyProperty.UnsetValue" />; see Remarks.</exception>
	[MethodImpl(MethodImplOptions.NoInlining)]
	public FrameworkPropertyMetadata(object defaultValue, FrameworkPropertyMetadataOptions flags, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback, bool isAnimationProhibited, UpdateSourceTrigger defaultUpdateSourceTrigger)
		: base(defaultValue, propertyChangedCallback, coerceValueCallback, isAnimationProhibited)
	{
		if (!BindingOperations.IsValidUpdateSourceTrigger(defaultUpdateSourceTrigger))
		{
			throw new InvalidEnumArgumentException("defaultUpdateSourceTrigger", (int)defaultUpdateSourceTrigger, typeof(UpdateSourceTrigger));
		}
		if (defaultUpdateSourceTrigger == UpdateSourceTrigger.Default)
		{
			throw new ArgumentException(SR.NoDefaultUpdateSourceTrigger, "defaultUpdateSourceTrigger");
		}
		TranslateFlags(flags);
		DefaultUpdateSourceTrigger = defaultUpdateSourceTrigger;
	}

	private void Initialize()
	{
		_flags = (_flags & (MetadataFlags)1073741823u) | MetadataFlags.FW_DefaultUpdateSourceTriggerEnumBit1;
	}

	private static bool IsFlagSet(FrameworkPropertyMetadataOptions flag, FrameworkPropertyMetadataOptions flags)
	{
		return (flags & flag) != 0;
	}

	private void TranslateFlags(FrameworkPropertyMetadataOptions flags)
	{
		Initialize();
		if (IsFlagSet(FrameworkPropertyMetadataOptions.AffectsMeasure, flags))
		{
			AffectsMeasure = true;
		}
		if (IsFlagSet(FrameworkPropertyMetadataOptions.AffectsArrange, flags))
		{
			AffectsArrange = true;
		}
		if (IsFlagSet(FrameworkPropertyMetadataOptions.AffectsParentMeasure, flags))
		{
			AffectsParentMeasure = true;
		}
		if (IsFlagSet(FrameworkPropertyMetadataOptions.AffectsParentArrange, flags))
		{
			AffectsParentArrange = true;
		}
		if (IsFlagSet(FrameworkPropertyMetadataOptions.AffectsRender, flags))
		{
			AffectsRender = true;
		}
		if (IsFlagSet(FrameworkPropertyMetadataOptions.Inherits, flags))
		{
			base.IsInherited = true;
		}
		if (IsFlagSet(FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior, flags))
		{
			OverridesInheritanceBehavior = true;
		}
		if (IsFlagSet(FrameworkPropertyMetadataOptions.NotDataBindable, flags))
		{
			IsNotDataBindable = true;
		}
		if (IsFlagSet(FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, flags))
		{
			BindsTwoWayByDefault = true;
		}
		if (IsFlagSet(FrameworkPropertyMetadataOptions.Journal, flags))
		{
			Journal = true;
		}
		if (IsFlagSet(FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender, flags))
		{
			SubPropertiesDoNotAffectRender = true;
		}
	}

	internal override PropertyMetadata CreateInstance()
	{
		return new FrameworkPropertyMetadata();
	}

	/// <summary>Enables a merge of the source metadata with base metadata. </summary>
	/// <param name="baseMetadata">The base metadata to merge.</param>
	/// <param name="dp">The dependency property this metadata is being applied to.</param>
	protected override void Merge(PropertyMetadata baseMetadata, DependencyProperty dp)
	{
		base.Merge(baseMetadata, dp);
		if (baseMetadata is FrameworkPropertyMetadata frameworkPropertyMetadata)
		{
			WriteFlag(MetadataFlags.FW_AffectsMeasureID, ReadFlag(MetadataFlags.FW_AffectsMeasureID) | frameworkPropertyMetadata.AffectsMeasure);
			WriteFlag(MetadataFlags.FW_AffectsArrangeID, ReadFlag(MetadataFlags.FW_AffectsArrangeID) | frameworkPropertyMetadata.AffectsArrange);
			WriteFlag(MetadataFlags.FW_AffectsParentMeasureID, ReadFlag(MetadataFlags.FW_AffectsParentMeasureID) | frameworkPropertyMetadata.AffectsParentMeasure);
			WriteFlag(MetadataFlags.FW_AffectsParentArrangeID, ReadFlag(MetadataFlags.FW_AffectsParentArrangeID) | frameworkPropertyMetadata.AffectsParentArrange);
			WriteFlag(MetadataFlags.FW_AffectsRenderID, ReadFlag(MetadataFlags.FW_AffectsRenderID) | frameworkPropertyMetadata.AffectsRender);
			WriteFlag(MetadataFlags.FW_BindsTwoWayByDefaultID, ReadFlag(MetadataFlags.FW_BindsTwoWayByDefaultID) | frameworkPropertyMetadata.BindsTwoWayByDefault);
			WriteFlag(MetadataFlags.FW_IsNotDataBindableID, ReadFlag(MetadataFlags.FW_IsNotDataBindableID) | frameworkPropertyMetadata.IsNotDataBindable);
			if (!IsModified(MetadataFlags.FW_SubPropertiesDoNotAffectRenderModifiedID))
			{
				WriteFlag(MetadataFlags.FW_SubPropertiesDoNotAffectRenderID, frameworkPropertyMetadata.SubPropertiesDoNotAffectRender);
			}
			if (!IsModified(MetadataFlags.FW_InheritsModifiedID))
			{
				base.IsInherited = frameworkPropertyMetadata.Inherits;
			}
			if (!IsModified(MetadataFlags.FW_OverridesInheritanceBehaviorModifiedID))
			{
				WriteFlag(MetadataFlags.FW_OverridesInheritanceBehaviorID, frameworkPropertyMetadata.OverridesInheritanceBehavior);
			}
			if (!IsModified(MetadataFlags.FW_ShouldBeJournaledModifiedID))
			{
				WriteFlag(MetadataFlags.FW_ShouldBeJournaledID, frameworkPropertyMetadata.Journal);
			}
			if (!IsModified(MetadataFlags.FW_DefaultUpdateSourceTriggerModifiedID))
			{
				_flags = (MetadataFlags)((uint)(_flags & (MetadataFlags)1073741823u) | (uint)((int)frameworkPropertyMetadata.DefaultUpdateSourceTrigger << 30));
			}
		}
	}

	/// <summary>Called when this metadata has been applied to a property, which indicates that the metadata is being sealed. </summary>
	/// <param name="dp">The dependency property to which the metadata has been applied.</param>
	/// <param name="targetType">The type associated with this metadata if this is type-specific metadata. If this is default metadata, this value can be null.</param>
	protected override void OnApply(DependencyProperty dp, Type targetType)
	{
		ReadOnly = dp.ReadOnly;
		base.OnApply(dp, targetType);
	}

	internal void SetModified(MetadataFlags id)
	{
		WriteFlag(id, value: true);
	}

	internal bool IsModified(MetadataFlags id)
	{
		return ReadFlag(id);
	}
}
