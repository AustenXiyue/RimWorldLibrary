using System;
using System.IO.Packaging;
using System.Windows;
using MS.Internal.Interop;

namespace MS.Internal.IO.Packaging;

internal class CorePropertyEnumerator
{
	private PackageProperties _coreProperties;

	private ManagedFullPropSpec[] _attributes;

	private int _currentIndex;

	internal Guid CurrentGuid
	{
		get
		{
			ValidateCurrent();
			return _attributes[_currentIndex].Guid;
		}
	}

	internal uint CurrentPropId
	{
		get
		{
			ValidateCurrent();
			return _attributes[_currentIndex].Property.PropId;
		}
	}

	internal object CurrentValue
	{
		get
		{
			ValidateCurrent();
			return GetValue(CurrentGuid, CurrentPropId);
		}
	}

	internal CorePropertyEnumerator(PackageProperties coreProperties, IFILTER_INIT grfFlags, ManagedFullPropSpec[] attributes)
	{
		if (attributes != null && attributes.Length != 0)
		{
			_attributes = attributes;
		}
		else if ((grfFlags & IFILTER_INIT.IFILTER_INIT_APPLY_INDEX_ATTRIBUTES) == IFILTER_INIT.IFILTER_INIT_APPLY_INDEX_ATTRIBUTES)
		{
			_attributes = new ManagedFullPropSpec[16]
			{
				new ManagedFullPropSpec(FormatId.SummaryInformation, 2u),
				new ManagedFullPropSpec(FormatId.SummaryInformation, 3u),
				new ManagedFullPropSpec(FormatId.SummaryInformation, 4u),
				new ManagedFullPropSpec(FormatId.SummaryInformation, 5u),
				new ManagedFullPropSpec(FormatId.SummaryInformation, 6u),
				new ManagedFullPropSpec(FormatId.SummaryInformation, 8u),
				new ManagedFullPropSpec(FormatId.SummaryInformation, 9u),
				new ManagedFullPropSpec(FormatId.SummaryInformation, 11u),
				new ManagedFullPropSpec(FormatId.SummaryInformation, 12u),
				new ManagedFullPropSpec(FormatId.SummaryInformation, 13u),
				new ManagedFullPropSpec(FormatId.DocumentSummaryInformation, 2u),
				new ManagedFullPropSpec(FormatId.DocumentSummaryInformation, 18u),
				new ManagedFullPropSpec(FormatId.DocumentSummaryInformation, 26u),
				new ManagedFullPropSpec(FormatId.DocumentSummaryInformation, 27u),
				new ManagedFullPropSpec(FormatId.DocumentSummaryInformation, 28u),
				new ManagedFullPropSpec(FormatId.DocumentSummaryInformation, 29u)
			};
		}
		_coreProperties = coreProperties;
		_currentIndex = -1;
	}

	internal bool MoveNext()
	{
		if (_attributes == null)
		{
			return false;
		}
		_currentIndex++;
		while (_currentIndex < _attributes.Length)
		{
			if (_attributes[_currentIndex].Property.PropType == PropSpecType.Id && CurrentValue != null)
			{
				return true;
			}
			_currentIndex++;
		}
		return false;
	}

	private void ValidateCurrent()
	{
		if (_currentIndex < 0 || _currentIndex >= _attributes.Length)
		{
			throw new InvalidOperationException(SR.CorePropertyEnumeratorPositionedOutOfBounds);
		}
	}

	private object GetValue(Guid guid, uint propId)
	{
		if (guid == FormatId.SummaryInformation)
		{
			switch (propId)
			{
			case 2u:
				return _coreProperties.Title;
			case 3u:
				return _coreProperties.Subject;
			case 4u:
				return _coreProperties.Creator;
			case 5u:
				return _coreProperties.Keywords;
			case 6u:
				return _coreProperties.Description;
			case 8u:
				return _coreProperties.LastModifiedBy;
			case 9u:
				return _coreProperties.Revision;
			case 11u:
				if (_coreProperties.LastPrinted.HasValue)
				{
					return _coreProperties.LastPrinted.Value;
				}
				return null;
			case 12u:
				if (_coreProperties.Created.HasValue)
				{
					return _coreProperties.Created.Value;
				}
				return null;
			case 13u:
				if (_coreProperties.Modified.HasValue)
				{
					return _coreProperties.Modified.Value;
				}
				return null;
			}
		}
		else if (guid == FormatId.DocumentSummaryInformation)
		{
			switch (propId)
			{
			case 2u:
				return _coreProperties.Category;
			case 18u:
				return _coreProperties.Identifier;
			case 26u:
				return _coreProperties.ContentType;
			case 27u:
				return _coreProperties.Language;
			case 28u:
				return _coreProperties.Version;
			case 29u:
				return _coreProperties.ContentStatus;
			}
		}
		return null;
	}
}
