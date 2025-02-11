using System;

namespace MS.Internal.Controls.StickyNote;

internal class LockHelper
{
	[Flags]
	public enum LockFlag
	{
		AnnotationChanged = 1,
		DataChanged = 2
	}

	public class AutoLocker : IDisposable
	{
		private LockHelper _helper;

		private LockFlag _flag;

		public AutoLocker(LockHelper helper, LockFlag flag)
		{
			if (helper == null)
			{
				throw new ArgumentNullException("helper");
			}
			_helper = helper;
			_flag = flag;
			_helper.Lock(_flag);
		}

		public void Dispose()
		{
			_helper.Unlock(_flag);
			GC.SuppressFinalize(this);
		}

		private AutoLocker()
		{
		}
	}

	private LockFlag _backingStore;

	public bool IsLocked(LockFlag flag)
	{
		return (_backingStore & flag) != 0;
	}

	private void Lock(LockFlag flag)
	{
		_backingStore |= flag;
	}

	private void Unlock(LockFlag flag)
	{
		_backingStore &= ~flag;
	}
}
