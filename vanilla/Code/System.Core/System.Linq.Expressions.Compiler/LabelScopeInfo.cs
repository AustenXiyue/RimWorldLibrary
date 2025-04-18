using System.Collections.Generic;

namespace System.Linq.Expressions.Compiler;

internal sealed class LabelScopeInfo
{
	private Dictionary<LabelTarget, LabelInfo> _labels;

	internal readonly LabelScopeKind Kind;

	internal readonly LabelScopeInfo Parent;

	internal bool CanJumpInto
	{
		get
		{
			LabelScopeKind kind = Kind;
			if ((uint)kind <= 3u)
			{
				return true;
			}
			return false;
		}
	}

	internal LabelScopeInfo(LabelScopeInfo parent, LabelScopeKind kind)
	{
		Parent = parent;
		Kind = kind;
	}

	internal bool ContainsTarget(LabelTarget target)
	{
		if (_labels == null)
		{
			return false;
		}
		return _labels.ContainsKey(target);
	}

	internal bool TryGetLabelInfo(LabelTarget target, out LabelInfo info)
	{
		if (_labels == null)
		{
			info = null;
			return false;
		}
		return _labels.TryGetValue(target, out info);
	}

	internal void AddLabelInfo(LabelTarget target, LabelInfo info)
	{
		if (_labels == null)
		{
			_labels = new Dictionary<LabelTarget, LabelInfo>();
		}
		_labels.Add(target, info);
	}
}
