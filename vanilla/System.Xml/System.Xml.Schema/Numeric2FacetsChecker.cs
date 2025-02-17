using System.Collections;

namespace System.Xml.Schema;

internal class Numeric2FacetsChecker : FacetsChecker
{
	internal override Exception CheckValueFacets(object value, XmlSchemaDatatype datatype)
	{
		double value2 = datatype.ValueConverter.ToDouble(value);
		return CheckValueFacets(value2, datatype);
	}

	internal override Exception CheckValueFacets(double value, XmlSchemaDatatype datatype)
	{
		RestrictionFacets restriction = datatype.Restriction;
		RestrictionFlags restrictionFlags = restriction?.Flags ?? ((RestrictionFlags)0);
		XmlValueConverter valueConverter = datatype.ValueConverter;
		if ((restrictionFlags & RestrictionFlags.MaxInclusive) != 0 && value > valueConverter.ToDouble(restriction.MaxInclusive))
		{
			return new XmlSchemaException("The MaxInclusive constraint failed.", string.Empty);
		}
		if ((restrictionFlags & RestrictionFlags.MaxExclusive) != 0 && value >= valueConverter.ToDouble(restriction.MaxExclusive))
		{
			return new XmlSchemaException("The MaxExclusive constraint failed.", string.Empty);
		}
		if ((restrictionFlags & RestrictionFlags.MinInclusive) != 0 && value < valueConverter.ToDouble(restriction.MinInclusive))
		{
			return new XmlSchemaException("The MinInclusive constraint failed.", string.Empty);
		}
		if ((restrictionFlags & RestrictionFlags.MinExclusive) != 0 && value <= valueConverter.ToDouble(restriction.MinExclusive))
		{
			return new XmlSchemaException("The MinExclusive constraint failed.", string.Empty);
		}
		if ((restrictionFlags & RestrictionFlags.Enumeration) != 0 && !MatchEnumeration(value, restriction.Enumeration, valueConverter))
		{
			return new XmlSchemaException("The Enumeration constraint failed.", string.Empty);
		}
		return null;
	}

	internal override Exception CheckValueFacets(float value, XmlSchemaDatatype datatype)
	{
		double value2 = value;
		return CheckValueFacets(value2, datatype);
	}

	internal override bool MatchEnumeration(object value, ArrayList enumeration, XmlSchemaDatatype datatype)
	{
		return MatchEnumeration(datatype.ValueConverter.ToDouble(value), enumeration, datatype.ValueConverter);
	}

	private bool MatchEnumeration(double value, ArrayList enumeration, XmlValueConverter valueConverter)
	{
		for (int i = 0; i < enumeration.Count; i++)
		{
			if (value == valueConverter.ToDouble(enumeration[i]))
			{
				return true;
			}
		}
		return false;
	}
}
