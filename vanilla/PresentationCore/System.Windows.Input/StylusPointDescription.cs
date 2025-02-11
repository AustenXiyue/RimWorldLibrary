using System.Collections.Generic;
using System.Collections.ObjectModel;
using MS.Internal.PresentationCore;

namespace System.Windows.Input;

/// <summary>Specifies the properties that are in a <see cref="T:System.Windows.Input.StylusPoint" />.</summary>
public class StylusPointDescription
{
	internal const int RequiredCountOfProperties = 3;

	internal const int RequiredXIndex = 0;

	internal const int RequiredYIndex = 1;

	internal const int RequiredPressureIndex = 2;

	internal const int MaximumButtonCount = 31;

	private int _buttonCount;

	private int _originalPressureIndex = 2;

	private StylusPointPropertyInfo[] _stylusPointPropertyInfos;

	/// <summary>Gets the number of properties in the <see cref="T:System.Windows.Input.StylusPointDescription" />.</summary>
	/// <returns>The number of properties in the <see cref="T:System.Windows.Input.StylusPointDescription" />.</returns>
	public int PropertyCount => _stylusPointPropertyInfos.Length;

	internal int ButtonCount => _buttonCount;

	internal bool ContainsTruePressure => _originalPressureIndex != -1;

	internal int OriginalPressureIndex => _originalPressureIndex;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.StylusPointDescription" /> class. </summary>
	public StylusPointDescription()
	{
		_stylusPointPropertyInfos = new StylusPointPropertyInfo[3]
		{
			StylusPointPropertyInfoDefaults.X,
			StylusPointPropertyInfoDefaults.Y,
			StylusPointPropertyInfoDefaults.NormalPressure
		};
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Input.StylusPointDescription" /> class with the specified <see cref="T:System.Windows.Input.StylusPointPropertyInfo" /> objects. </summary>
	/// <param name="stylusPointPropertyInfos">A generic IEnumerable of type <see cref="T:System.Windows.Input.StylusPointPropertyInfo" /> that specifies the properties in the <see cref="T:System.Windows.Input.StylusPointDescription" />.</param>
	public StylusPointDescription(IEnumerable<StylusPointPropertyInfo> stylusPointPropertyInfos)
	{
		if (stylusPointPropertyInfos == null)
		{
			throw new ArgumentNullException("stylusPointPropertyInfos");
		}
		List<StylusPointPropertyInfo> list = new List<StylusPointPropertyInfo>(stylusPointPropertyInfos);
		if (list.Count < 3 || list[0].Id != StylusPointPropertyIds.X || list[1].Id != StylusPointPropertyIds.Y || list[2].Id != StylusPointPropertyIds.NormalPressure)
		{
			throw new ArgumentException(SR.InvalidStylusPointDescription, "stylusPointPropertyInfos");
		}
		List<Guid> list2 = new List<Guid>
		{
			StylusPointPropertyIds.X,
			StylusPointPropertyIds.Y,
			StylusPointPropertyIds.NormalPressure
		};
		int num = 0;
		for (int i = 3; i < list.Count; i++)
		{
			if (list2.Contains(list[i].Id))
			{
				throw new ArgumentException(SR.InvalidStylusPointDescriptionDuplicatesFound, "stylusPointPropertyInfos");
			}
			if (list[i].IsButton)
			{
				num++;
			}
			else if (num > 0)
			{
				throw new ArgumentException(SR.InvalidStylusPointDescriptionButtonsMustBeLast, "stylusPointPropertyInfos");
			}
			list2.Add(list[i].Id);
		}
		if (num > 31)
		{
			throw new ArgumentException(SR.InvalidStylusPointDescriptionTooManyButtons, "stylusPointPropertyInfos");
		}
		_buttonCount = num;
		_stylusPointPropertyInfos = list.ToArray();
	}

	internal StylusPointDescription(IEnumerable<StylusPointPropertyInfo> stylusPointPropertyInfos, int originalPressureIndex)
		: this(stylusPointPropertyInfos)
	{
		_originalPressureIndex = originalPressureIndex;
	}

	/// <summary>Returns a value that indicates whether the current <see cref="T:System.Windows.Input.StylusPointDescription" /> has the specified property.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Input.StylusPointDescription" /> has the specified <see cref="T:System.Windows.Input.StylusPointProperty" />; otherwise, false.</returns>
	/// <param name="stylusPointProperty">The <see cref="T:System.Windows.Input.StylusPointProperty" /> to check for in the <see cref="T:System.Windows.Input.StylusPointDescription" />.</param>
	public bool HasProperty(StylusPointProperty stylusPointProperty)
	{
		if (stylusPointProperty == null)
		{
			throw new ArgumentNullException("stylusPointProperty");
		}
		int num = IndexOf(stylusPointProperty.Id);
		if (-1 == num)
		{
			return false;
		}
		return true;
	}

	/// <summary>Gets the <see cref="T:System.Windows.Input.StylusPointPropertyInfo" /> for the specified property.</summary>
	/// <returns>The <see cref="T:System.Windows.Input.StylusPointPropertyInfo" /> for the specified <see cref="T:System.Windows.Input.StylusPointProperty" />.</returns>
	/// <param name="stylusPointProperty">The <see cref="T:System.Windows.Input.StylusPointProperty" /> that specifies the property of the desired <see cref="T:System.Windows.Input.StylusPointPropertyInfo" />.</param>
	public StylusPointPropertyInfo GetPropertyInfo(StylusPointProperty stylusPointProperty)
	{
		if (stylusPointProperty == null)
		{
			throw new ArgumentNullException("stylusPointProperty");
		}
		return GetPropertyInfo(stylusPointProperty.Id);
	}

	internal StylusPointPropertyInfo GetPropertyInfo(Guid guid)
	{
		int num = IndexOf(guid);
		if (-1 == num)
		{
			throw new ArgumentException("stylusPointProperty");
		}
		return _stylusPointPropertyInfos[num];
	}

	internal int GetPropertyIndex(Guid guid)
	{
		return IndexOf(guid);
	}

	/// <summary>Gets all the properties of the <see cref="T:System.Windows.Input.StylusPointDescription" />.</summary>
	/// <returns>A collection that contains all of the <see cref="T:System.Windows.Input.StylusPointPropertyInfo" /> objects in the <see cref="T:System.Windows.Input.StylusPointDescription" />.</returns>
	public ReadOnlyCollection<StylusPointPropertyInfo> GetStylusPointProperties()
	{
		return new ReadOnlyCollection<StylusPointPropertyInfo>(_stylusPointPropertyInfos);
	}

	internal Guid[] GetStylusPointPropertyIds()
	{
		Guid[] array = new Guid[_stylusPointPropertyInfos.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = _stylusPointPropertyInfos[i].Id;
		}
		return array;
	}

	internal int GetInputArrayLengthPerPoint()
	{
		int num = ((_buttonCount > 0) ? 1 : 0);
		int num2 = _stylusPointPropertyInfos.Length - _buttonCount + num;
		if (!ContainsTruePressure)
		{
			num2--;
		}
		return num2;
	}

	internal int GetExpectedAdditionalDataCount()
	{
		int num = ((_buttonCount > 0) ? 1 : 0);
		return _stylusPointPropertyInfos.Length - _buttonCount + num - 3;
	}

	internal int GetOutputArrayLengthPerPoint()
	{
		int num = GetInputArrayLengthPerPoint();
		if (!ContainsTruePressure)
		{
			num++;
		}
		return num;
	}

	internal int GetButtonBitPosition(StylusPointProperty buttonProperty)
	{
		if (!buttonProperty.IsButton)
		{
			throw new InvalidOperationException();
		}
		int num = 0;
		for (int i = _stylusPointPropertyInfos.Length - _buttonCount; i < _stylusPointPropertyInfos.Length; i++)
		{
			if (_stylusPointPropertyInfos[i].Id == buttonProperty.Id)
			{
				return num;
			}
			if (_stylusPointPropertyInfos[i].IsButton)
			{
				num++;
			}
		}
		return -1;
	}

	/// <summary>Returns a value that indicates whether the specified <see cref="T:System.Windows.Input.StylusPointDescription" /> objects are identical.</summary>
	/// <returns>true if the <see cref="T:System.Windows.Input.StylusPointDescription" /> objects are identical; otherwise, false.</returns>
	/// <param name="stylusPointDescription1">The first <see cref="T:System.Windows.Input.StylusPointDescription" /> to check.</param>
	/// <param name="stylusPointDescription2">The second <see cref="T:System.Windows.Input.StylusPointDescription" /> to check.</param>
	public static bool AreCompatible(StylusPointDescription stylusPointDescription1, StylusPointDescription stylusPointDescription2)
	{
		if (stylusPointDescription1 == null || stylusPointDescription2 == null)
		{
			throw new ArgumentNullException("stylusPointDescription");
		}
		if (stylusPointDescription1._stylusPointPropertyInfos.Length != stylusPointDescription2._stylusPointPropertyInfos.Length)
		{
			return false;
		}
		for (int i = 3; i < stylusPointDescription1._stylusPointPropertyInfos.Length; i++)
		{
			if (!StylusPointPropertyInfo.AreCompatible(stylusPointDescription1._stylusPointPropertyInfos[i], stylusPointDescription2._stylusPointPropertyInfos[i]))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Returns the intersection of the specified <see cref="T:System.Windows.Input.StylusPointDescription" /> objects.</summary>
	/// <returns>A <see cref="T:System.Windows.Input.StylusPointDescription" /> that contains the properties that are present if both of the specified <see cref="T:System.Windows.Input.StylusPointDescription" /> objects.</returns>
	/// <param name="stylusPointDescriptionPreserveInfo">The second <see cref="T:System.Windows.Input.StylusPointDescription" /> to intersect.</param>
	public static StylusPointDescription GetCommonDescription(StylusPointDescription stylusPointDescription, StylusPointDescription stylusPointDescriptionPreserveInfo)
	{
		if (stylusPointDescription == null)
		{
			throw new ArgumentNullException("stylusPointDescription");
		}
		if (stylusPointDescriptionPreserveInfo == null)
		{
			throw new ArgumentNullException("stylusPointDescriptionPreserveInfo");
		}
		List<StylusPointPropertyInfo> list = new List<StylusPointPropertyInfo>();
		list.Add(stylusPointDescriptionPreserveInfo._stylusPointPropertyInfos[0]);
		list.Add(stylusPointDescriptionPreserveInfo._stylusPointPropertyInfos[1]);
		list.Add(stylusPointDescriptionPreserveInfo._stylusPointPropertyInfos[2]);
		for (int i = 3; i < stylusPointDescription._stylusPointPropertyInfos.Length; i++)
		{
			for (int j = 3; j < stylusPointDescriptionPreserveInfo._stylusPointPropertyInfos.Length; j++)
			{
				if (StylusPointPropertyInfo.AreCompatible(stylusPointDescription._stylusPointPropertyInfos[i], stylusPointDescriptionPreserveInfo._stylusPointPropertyInfos[j]))
				{
					list.Add(stylusPointDescriptionPreserveInfo._stylusPointPropertyInfos[j]);
				}
			}
		}
		return new StylusPointDescription(list);
	}

	/// <summary>Returns a value that indicates whether the current <see cref="T:System.Windows.Input.StylusPointDescription" /> is a subset of the specified <see cref="T:System.Windows.Input.StylusPointDescription" />.</summary>
	/// <returns>true if the current <see cref="T:System.Windows.Input.StylusPointDescription" /> is a subset of the specified <see cref="T:System.Windows.Input.StylusPointDescription" />; otherwise, false.</returns>
	/// <param name="stylusPointDescriptionSuperset">The <see cref="T:System.Windows.Input.StylusPointDescription" /> against which to check whether the current <see cref="T:System.Windows.Input.StylusPointDescription" /> is a subset.</param>
	public bool IsSubsetOf(StylusPointDescription stylusPointDescriptionSuperset)
	{
		if (stylusPointDescriptionSuperset == null)
		{
			throw new ArgumentNullException("stylusPointDescriptionSuperset");
		}
		if (stylusPointDescriptionSuperset._stylusPointPropertyInfos.Length < _stylusPointPropertyInfos.Length)
		{
			return false;
		}
		for (int i = 0; i < _stylusPointPropertyInfos.Length; i++)
		{
			Guid id = _stylusPointPropertyInfos[i].Id;
			if (-1 == stylusPointDescriptionSuperset.IndexOf(id))
			{
				return false;
			}
		}
		return true;
	}

	private int IndexOf(Guid propertyId)
	{
		for (int i = 0; i < _stylusPointPropertyInfos.Length; i++)
		{
			if (_stylusPointPropertyInfos[i].Id == propertyId)
			{
				return i;
			}
		}
		return -1;
	}
}
