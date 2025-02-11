using System.ComponentModel;
using System.Windows.Markup;

namespace System.Windows.Data;

/// <summary>Implements a markup extension that describes the location of the binding source relative to the position of the binding target.</summary>
[MarkupExtensionReturnType(typeof(RelativeSource))]
public class RelativeSource : MarkupExtension, ISupportInitialize
{
	private RelativeSourceMode _mode;

	private Type _ancestorType;

	private int _ancestorLevel = -1;

	private static RelativeSource s_previousData;

	private static RelativeSource s_templatedParent;

	private static RelativeSource s_self;

	/// <summary>Gets a static value that is used to return a <see cref="T:System.Windows.Data.RelativeSource" /> constructed for the <see cref="F:System.Windows.Data.RelativeSourceMode.PreviousData" /> mode.</summary>
	/// <returns>A static <see cref="T:System.Windows.Data.RelativeSource" />.</returns>
	public static RelativeSource PreviousData
	{
		get
		{
			if (s_previousData == null)
			{
				s_previousData = new RelativeSource(RelativeSourceMode.PreviousData);
			}
			return s_previousData;
		}
	}

	/// <summary>Gets a static value that is used to return a <see cref="T:System.Windows.Data.RelativeSource" /> constructed for the <see cref="F:System.Windows.Data.RelativeSourceMode.TemplatedParent" /> mode.</summary>
	/// <returns>A static <see cref="T:System.Windows.Data.RelativeSource" />.</returns>
	public static RelativeSource TemplatedParent
	{
		get
		{
			if (s_templatedParent == null)
			{
				s_templatedParent = new RelativeSource(RelativeSourceMode.TemplatedParent);
			}
			return s_templatedParent;
		}
	}

	/// <summary>Gets a static value that is used to return a <see cref="T:System.Windows.Data.RelativeSource" /> constructed for the <see cref="F:System.Windows.Data.RelativeSourceMode.Self" /> mode.</summary>
	/// <returns>A static <see cref="T:System.Windows.Data.RelativeSource" />.</returns>
	public static RelativeSource Self
	{
		get
		{
			if (s_self == null)
			{
				s_self = new RelativeSource(RelativeSourceMode.Self);
			}
			return s_self;
		}
	}

	/// <summary>Gets or sets a <see cref="T:System.Windows.Data.RelativeSourceMode" /> value that describes the location of the binding source relative to the position of the binding target.</summary>
	/// <returns>One of the <see cref="T:System.Windows.Data.RelativeSourceMode" /> values. The default value is null.</returns>
	/// <exception cref="T:System.InvalidOperationException">This property is immutable after initialization. Instead of changing the <see cref="P:System.Windows.Data.RelativeSource.Mode" /> on this instance, create a new <see cref="T:System.Windows.Data.RelativeSource" /> or use a different static instance.</exception>
	[ConstructorArgument("mode")]
	public RelativeSourceMode Mode
	{
		get
		{
			return _mode;
		}
		set
		{
			if (IsUninitialized)
			{
				InitializeMode(value);
			}
			else if (value != _mode)
			{
				throw new InvalidOperationException(SR.RelativeSourceModeIsImmutable);
			}
		}
	}

	/// <summary>Gets or sets the type of ancestor to look for.</summary>
	/// <returns>The type of ancestor. The default value is null.</returns>
	/// <exception cref="T:System.InvalidOperationException">The <see cref="T:System.Windows.Data.RelativeSource" /> is not in the <see cref="F:System.Windows.Data.RelativeSourceMode.FindAncestor" /> mode.</exception>
	public Type AncestorType
	{
		get
		{
			return _ancestorType;
		}
		set
		{
			if (IsUninitialized)
			{
				AncestorLevel = 1;
			}
			if (_mode != RelativeSourceMode.FindAncestor)
			{
				if (value != null)
				{
					throw new InvalidOperationException(SR.RelativeSourceNotInFindAncestorMode);
				}
			}
			else
			{
				_ancestorType = value;
			}
		}
	}

	/// <summary>Gets or sets the level of ancestor to look for, in <see cref="F:System.Windows.Data.RelativeSourceMode.FindAncestor" /> mode. Use 1 to indicate the one nearest to the binding target element.</summary>
	/// <returns>The ancestor level. Use 1 to indicate the one nearest to the binding target element.</returns>
	public int AncestorLevel
	{
		get
		{
			return _ancestorLevel;
		}
		set
		{
			if (_mode != RelativeSourceMode.FindAncestor)
			{
				if (value != 0)
				{
					throw new InvalidOperationException(SR.RelativeSourceNotInFindAncestorMode);
				}
				return;
			}
			if (value < 1)
			{
				throw new ArgumentOutOfRangeException(SR.RelativeSourceInvalidAncestorLevel);
			}
			_ancestorLevel = value;
		}
	}

	private bool IsUninitialized => _ancestorLevel == -1;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.RelativeSource" /> class.</summary>
	public RelativeSource()
	{
		_mode = RelativeSourceMode.FindAncestor;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.RelativeSource" /> class with an initial mode.</summary>
	/// <param name="mode">One of the <see cref="T:System.Windows.Data.RelativeSourceMode" /> values.</param>
	public RelativeSource(RelativeSourceMode mode)
	{
		InitializeMode(mode);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Data.RelativeSource" /> class with an initial mode and additional tree-walking qualifiers for finding the desired relative source.</summary>
	/// <param name="mode">One of the <see cref="T:System.Windows.Data.RelativeSourceMode" /> values. For this signature to be relevant, this should be <see cref="F:System.Windows.Data.RelativeSourceMode.FindAncestor" />.</param>
	/// <param name="ancestorType">The <see cref="T:System.Type" /> of ancestor to look for.</param>
	/// <param name="ancestorLevel">The ordinal position of the desired ancestor among all ancestors of the given type. </param>
	public RelativeSource(RelativeSourceMode mode, Type ancestorType, int ancestorLevel)
	{
		InitializeMode(mode);
		AncestorType = ancestorType;
		AncestorLevel = ancestorLevel;
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void ISupportInitialize.BeginInit()
	{
	}

	/// <summary>This member supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code.</summary>
	void ISupportInitialize.EndInit()
	{
		if (IsUninitialized)
		{
			throw new InvalidOperationException(SR.RelativeSourceNeedsMode);
		}
		if (_mode == RelativeSourceMode.FindAncestor && AncestorType == null)
		{
			throw new InvalidOperationException(SR.RelativeSourceNeedsAncestorType);
		}
	}

	/// <summary>Indicates whether the <see cref="P:System.Windows.Data.RelativeSource.AncestorType" /> property should be persisted.</summary>
	/// <returns>true if the property value has changed from its default; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeAncestorType()
	{
		return _mode == RelativeSourceMode.FindAncestor;
	}

	/// <summary>Indicates whether the <see cref="P:System.Windows.Data.RelativeSource.AncestorLevel" /> property should be persisted.</summary>
	/// <returns>true if the property value has changed from its default; otherwise, false.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public bool ShouldSerializeAncestorLevel()
	{
		return _mode == RelativeSourceMode.FindAncestor;
	}

	/// <summary>Returns an object that should be set as the value on the target object's property for this markup extension. For <see cref="T:System.Windows.Data.RelativeSource" />, this is another <see cref="T:System.Windows.Data.RelativeSource" />, using the appropriate source for the specified mode. </summary>
	/// <returns>Another <see cref="T:System.Windows.Data.RelativeSource" />.</returns>
	/// <param name="serviceProvider">An object that can provide services for the markup extension. In this implementation, this parameter can be null.</param>
	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		if (_mode == RelativeSourceMode.PreviousData)
		{
			return PreviousData;
		}
		if (_mode == RelativeSourceMode.Self)
		{
			return Self;
		}
		if (_mode == RelativeSourceMode.TemplatedParent)
		{
			return TemplatedParent;
		}
		return this;
	}

	private void InitializeMode(RelativeSourceMode mode)
	{
		switch (mode)
		{
		case RelativeSourceMode.FindAncestor:
			_ancestorLevel = 1;
			_mode = mode;
			break;
		case RelativeSourceMode.PreviousData:
		case RelativeSourceMode.TemplatedParent:
		case RelativeSourceMode.Self:
			_ancestorLevel = 0;
			_mode = mode;
			break;
		default:
			throw new ArgumentException(SR.RelativeSourceModeInvalid, "mode");
		}
	}
}
