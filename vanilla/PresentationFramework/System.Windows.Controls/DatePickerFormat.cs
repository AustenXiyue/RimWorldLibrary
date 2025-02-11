namespace System.Windows.Controls;

/// <summary>Specifies date formats for a <see cref="T:System.Windows.Controls.DatePicker" />.</summary>
public enum DatePickerFormat
{
	/// <summary>Specifies that the date should be displayed by using unabbreviated days of the week and month names. This value displays a string that is equal to the string that is returned by the <see cref="M:System.DateTime.ToLongDateString" /> method.</summary>
	Long,
	/// <summary>Specifies that the date should be displayed by using abbreviated days of the week and month names. This value displays a string that is equal to the string that is returned by the <see cref="M:System.DateTime.ToShortDateString" /> method.</summary>
	Short
}
