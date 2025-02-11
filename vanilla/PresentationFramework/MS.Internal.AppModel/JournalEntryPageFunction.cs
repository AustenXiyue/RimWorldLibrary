using System;
using System.Runtime.Serialization;
using System.Windows.Navigation;

namespace MS.Internal.AppModel;

[Serializable]
internal abstract class JournalEntryPageFunction : JournalEntry, ISerializable
{
	private Guid _pageFunctionId;

	private Guid _parentPageFunctionId;

	internal const int _NoParentPage = -1;

	internal Guid PageFunctionId
	{
		get
		{
			return _pageFunctionId;
		}
		set
		{
			_pageFunctionId = value;
		}
	}

	internal Guid ParentPageFunctionId
	{
		get
		{
			return _parentPageFunctionId;
		}
		set
		{
			_parentPageFunctionId = value;
		}
	}

	internal JournalEntryPageFunction(JournalEntryGroupState jeGroupState, PageFunctionBase pageFunction)
		: base(jeGroupState, null)
	{
		PageFunctionId = pageFunction.PageFunctionId;
		ParentPageFunctionId = pageFunction.ParentPageFunctionId;
	}

	protected JournalEntryPageFunction(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		_pageFunctionId = (Guid)info.GetValue("_pageFunctionId", typeof(Guid));
		_parentPageFunctionId = (Guid)info.GetValue("_parentPageFunctionId", typeof(Guid));
	}

	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("_pageFunctionId", _pageFunctionId);
		info.AddValue("_parentPageFunctionId", _parentPageFunctionId);
	}

	internal override bool IsPageFunction()
	{
		return true;
	}

	internal override bool IsAlive()
	{
		return false;
	}

	internal abstract PageFunctionBase ResumePageFunction();

	internal static int GetParentPageJournalIndex(NavigationService NavigationService, Journal journal, PageFunctionBase endingPF)
	{
		for (int num = journal.CurrentIndex - 1; num >= 0; num--)
		{
			JournalEntry journalEntry = journal[num];
			if (!(journalEntry.NavigationServiceId != NavigationService.GuidId))
			{
				JournalEntryPageFunction journalEntryPageFunction = journalEntry as JournalEntryPageFunction;
				if (endingPF.ParentPageFunctionId == Guid.Empty)
				{
					if (journalEntryPageFunction == null)
					{
						return num;
					}
				}
				else if (journalEntryPageFunction != null && journalEntryPageFunction.PageFunctionId == endingPF.ParentPageFunctionId)
				{
					return num;
				}
			}
		}
		return -1;
	}
}
