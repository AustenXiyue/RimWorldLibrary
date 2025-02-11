using System.ComponentModel;
using MS.Internal.WindowsBase;

namespace System.Windows;

/// <summary>This type supports the Windows Presentation Foundation (WPF) infrastructure and is not intended to be used directly from your code. </summary>
[TypeConverter(typeof(ExpressionConverter))]
public class Expression
{
	[Flags]
	private enum InternalFlags
	{
		None = 0,
		NonShareable = 1,
		ForwardsInvalidations = 2,
		SupportsUnboundSources = 4,
		Attached = 8,
		Detached = 0x10
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal static readonly object NoValue = new object();

	private InternalFlags _flags;

	internal bool Attachable
	{
		get
		{
			if (!Shareable)
			{
				return !HasBeenAttached;
			}
			return true;
		}
	}

	internal bool Shareable => (_flags & InternalFlags.NonShareable) == 0;

	internal bool ForwardsInvalidations => (_flags & InternalFlags.ForwardsInvalidations) != 0;

	internal bool SupportsUnboundSources => (_flags & InternalFlags.SupportsUnboundSources) != 0;

	internal bool HasBeenAttached => (_flags & InternalFlags.Attached) != 0;

	internal bool HasBeenDetached => (_flags & InternalFlags.Detached) != 0;

	internal Expression()
		: this(ExpressionMode.None)
	{
	}

	internal Expression(ExpressionMode mode)
	{
		_flags = InternalFlags.None;
		switch (mode)
		{
		case ExpressionMode.NonSharable:
			_flags |= InternalFlags.NonShareable;
			break;
		case ExpressionMode.ForwardsInvalidations:
			_flags |= InternalFlags.ForwardsInvalidations;
			_flags |= InternalFlags.NonShareable;
			break;
		case ExpressionMode.SupportsUnboundSources:
			_flags |= InternalFlags.ForwardsInvalidations;
			_flags |= InternalFlags.NonShareable;
			_flags |= InternalFlags.SupportsUnboundSources;
			break;
		default:
			throw new ArgumentException(SR.UnknownExpressionMode);
		case ExpressionMode.None:
			break;
		}
	}

	[MS.Internal.WindowsBase.FriendAccessAllowed]
	internal virtual Expression Copy(DependencyObject targetObject, DependencyProperty targetDP)
	{
		return this;
	}

	internal virtual DependencySource[] GetSources()
	{
		return null;
	}

	internal virtual object GetValue(DependencyObject d, DependencyProperty dp)
	{
		return DependencyProperty.UnsetValue;
	}

	internal virtual bool SetValue(DependencyObject d, DependencyProperty dp, object value)
	{
		return false;
	}

	internal virtual void OnAttach(DependencyObject d, DependencyProperty dp)
	{
	}

	internal virtual void OnDetach(DependencyObject d, DependencyProperty dp)
	{
	}

	internal virtual void OnPropertyInvalidation(DependencyObject d, DependencyPropertyChangedEventArgs args)
	{
	}

	internal void ChangeSources(DependencyObject d, DependencyProperty dp, DependencySource[] newSources)
	{
		if (d == null && !ForwardsInvalidations)
		{
			throw new ArgumentNullException("d");
		}
		if (dp == null && !ForwardsInvalidations)
		{
			throw new ArgumentNullException("dp");
		}
		if (Shareable)
		{
			throw new InvalidOperationException(SR.ShareableExpressionsCannotChangeSources);
		}
		DependencyObject.ValidateSources(d, newSources, this);
		if (ForwardsInvalidations)
		{
			DependencyObject.ChangeExpressionSources(this, null, null, newSources);
		}
		else
		{
			DependencyObject.ChangeExpressionSources(this, d, dp, newSources);
		}
	}

	internal void MarkAttached()
	{
		_flags |= InternalFlags.Attached;
	}

	internal void MarkDetached()
	{
		_flags |= InternalFlags.Detached;
	}
}
