using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Security.Permissions;
using System.Text;

namespace System.ComponentModel;

/// <summary>Represents a mask-parsing service that can be used by any number of controls that support masking, such as the <see cref="T:System.Windows.Forms.MaskedTextBox" /> control.</summary>
[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
public class MaskedTextProvider : ICloneable
{
	private enum CaseConversion
	{
		None,
		ToLower,
		ToUpper
	}

	[Flags]
	private enum CharType
	{
		EditOptional = 1,
		EditRequired = 2,
		Separator = 4,
		Literal = 8,
		Modifier = 0x10
	}

	private class CharDescriptor
	{
		public int MaskPosition;

		public CaseConversion CaseConversion;

		public CharType CharType;

		public bool IsAssigned;

		public CharDescriptor(int maskPos, CharType charType)
		{
			MaskPosition = maskPos;
			CharType = charType;
		}

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "MaskPosition[{0}] <CaseConversion.{1}><CharType.{2}><IsAssigned: {3}", MaskPosition, CaseConversion, CharType, IsAssigned);
		}
	}

	private const char spaceChar = ' ';

	private const char defaultPromptChar = '_';

	private const char nullPasswordChar = '\0';

	private const bool defaultAllowPrompt = true;

	private const int invalidIndex = -1;

	private const byte editAny = 0;

	private const byte editUnassigned = 1;

	private const byte editAssigned = 2;

	private const bool forward = true;

	private const bool backward = false;

	private static int ASCII_ONLY = BitVector32.CreateMask();

	private static int ALLOW_PROMPT_AS_INPUT = BitVector32.CreateMask(ASCII_ONLY);

	private static int INCLUDE_PROMPT = BitVector32.CreateMask(ALLOW_PROMPT_AS_INPUT);

	private static int INCLUDE_LITERALS = BitVector32.CreateMask(INCLUDE_PROMPT);

	private static int RESET_ON_PROMPT = BitVector32.CreateMask(INCLUDE_LITERALS);

	private static int RESET_ON_LITERALS = BitVector32.CreateMask(RESET_ON_PROMPT);

	private static int SKIP_SPACE = BitVector32.CreateMask(RESET_ON_LITERALS);

	private static Type maskTextProviderType = typeof(MaskedTextProvider);

	private BitVector32 flagState;

	private CultureInfo culture;

	private StringBuilder testString;

	private int assignedCharCount;

	private int requiredCharCount;

	private int requiredEditChars;

	private int optionalEditChars;

	private string mask;

	private char passwordChar;

	private char promptChar;

	private List<CharDescriptor> stringDescriptor;

	/// <summary>Gets a value indicating whether the prompt character should be treated as a valid input character or not.</summary>
	/// <returns>true if the user can enter <see cref="P:System.ComponentModel.MaskedTextProvider.PromptChar" /> into the control; otherwise, false. The default is true. </returns>
	public bool AllowPromptAsInput => flagState[ALLOW_PROMPT_AS_INPUT];

	/// <summary>Gets the number of editable character positions that have already been successfully assigned an input value.</summary>
	/// <returns>An <see cref="T:System.Int32" /> containing the number of editable character positions in the input mask that have already been assigned a character value in the formatted string.</returns>
	public int AssignedEditPositionCount => assignedCharCount;

	/// <summary>Gets the number of editable character positions in the input mask that have not yet been assigned an input value.</summary>
	/// <returns>An <see cref="T:System.Int32" /> containing the number of editable character positions that not yet been assigned a character value.</returns>
	public int AvailableEditPositionCount => EditPositionCount - assignedCharCount;

	/// <summary>Gets the culture that determines the value of the localizable separators and placeholders in the input mask.</summary>
	/// <returns>A <see cref="T:System.Globalization.CultureInfo" /> containing the culture information associated with the input mask.</returns>
	public CultureInfo Culture => culture;

	/// <summary>Gets the default password character used obscure user input. </summary>
	/// <returns>A <see cref="T:System.Char" /> that represents the default password character.</returns>
	public static char DefaultPasswordChar => '*';

	/// <summary>Gets the number of editable positions in the formatted string.</summary>
	/// <returns>An <see cref="T:System.Int32" /> containing the number of editable positions in the formatted string.</returns>
	public int EditPositionCount => optionalEditChars + requiredEditChars;

	/// <summary>Gets a newly created enumerator for the editable positions in the formatted string. </summary>
	/// <returns>An <see cref="T:System.Collections.IEnumerator" /> that supports enumeration over the editable positions in the formatted string.</returns>
	public IEnumerator EditPositions
	{
		get
		{
			List<int> list = new List<int>();
			int num = 0;
			foreach (CharDescriptor item in stringDescriptor)
			{
				if (IsEditPosition(item))
				{
					list.Add(num);
				}
				num++;
			}
			return ((IEnumerable)list).GetEnumerator();
		}
	}

	/// <summary>Gets or sets a value that indicates whether literal characters in the input mask should be included in the formatted string.</summary>
	/// <returns>true if literals are included; otherwise, false. The default is true. </returns>
	public bool IncludeLiterals
	{
		get
		{
			return flagState[INCLUDE_LITERALS];
		}
		set
		{
			flagState[INCLUDE_LITERALS] = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether <see cref="P:System.Windows.Forms.MaskedTextBox.PromptChar" /> is used to represent the absence of user input when displaying the formatted string. </summary>
	/// <returns>true if the prompt character is used to represent the positions where no user input was provided; otherwise, false. The default is true.</returns>
	public bool IncludePrompt
	{
		get
		{
			return flagState[INCLUDE_PROMPT];
		}
		set
		{
			flagState[INCLUDE_PROMPT] = value;
		}
	}

	/// <summary>Gets a value indicating whether the mask accepts characters outside of the ASCII character set.</summary>
	/// <returns>true if only ASCII is accepted; false if <see cref="T:System.ComponentModel.MaskedTextProvider" /> can accept any arbitrary Unicode character. The default is false.</returns>
	public bool AsciiOnly => flagState[ASCII_ONLY];

	/// <summary>Gets or sets a value that determines whether password protection should be applied to the formatted string.</summary>
	/// <returns>true if the input string is to be treated as a password string; otherwise, false. The default is false.</returns>
	public bool IsPassword
	{
		get
		{
			return passwordChar != '\0';
		}
		set
		{
			if (IsPassword != value)
			{
				passwordChar = (value ? DefaultPasswordChar : '\0');
			}
		}
	}

	/// <summary>Gets the upper bound of the range of invalid indexes.</summary>
	/// <returns>A value representing the largest invalid index, as determined by the provider implementation. For example, if the lowest valid index is 0, this property will return -1.</returns>
	public static int InvalidIndex => -1;

	/// <summary>Gets the index in the mask of the rightmost input character that has been assigned to the mask.</summary>
	/// <returns>If at least one input character has been assigned to the mask, an <see cref="T:System.Int32" /> containing the index of rightmost assigned position; otherwise, if no position has been assigned, <see cref="P:System.ComponentModel.MaskedTextProvider.InvalidIndex" />.</returns>
	public int LastAssignedPosition => FindAssignedEditPositionFrom(testString.Length - 1, direction: false);

	/// <summary>Gets the length of the mask, absent any mask modifier characters.</summary>
	/// <returns>An <see cref="T:System.Int32" /> containing the number of positions in the mask, excluding characters that modify mask input. </returns>
	public int Length => testString.Length;

	/// <summary>Gets the input mask.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the full mask.</returns>
	public string Mask => mask;

	/// <summary>Gets a value indicating whether all required inputs have been entered into the formatted string.</summary>
	/// <returns>true if all required input has been entered into the mask; otherwise, false.</returns>
	public bool MaskCompleted => requiredCharCount == requiredEditChars;

	/// <summary>Gets a value indicating whether all required and optional inputs have been entered into the formatted string. </summary>
	/// <returns>true if all required and optional inputs have been entered; otherwise, false. </returns>
	public bool MaskFull => assignedCharCount == EditPositionCount;

	/// <summary>Gets or sets the character to be substituted for the actual input characters.</summary>
	/// <returns>The <see cref="T:System.Char" /> value used as the password character.</returns>
	/// <exception cref="T:System.InvalidOperationException">The password character specified when setting this property is the same as the current prompt character, <see cref="P:System.ComponentModel.MaskedTextProvider.PromptChar" />. The two are required to be different.</exception>
	/// <exception cref="T:System.ArgumentException">The character specified when setting this property is not a valid password character, as determined by the <see cref="M:System.ComponentModel.MaskedTextProvider.IsValidPasswordChar(System.Char)" /> method.</exception>
	public char PasswordChar
	{
		get
		{
			return passwordChar;
		}
		set
		{
			if (value == promptChar)
			{
				throw new InvalidOperationException(global::SR.GetString("The PasswordChar and PromptChar values cannot be the same."));
			}
			if (!IsValidPasswordChar(value) && value != 0)
			{
				throw new ArgumentException(global::SR.GetString("The specified character value is not allowed for this property."));
			}
			if (value != passwordChar)
			{
				passwordChar = value;
			}
		}
	}

	/// <summary>Gets or sets the character used to represent the absence of user input for all available edit positions.</summary>
	/// <returns>The character used to prompt the user for input. The default is an underscore (_). </returns>
	/// <exception cref="T:System.InvalidOperationException">The prompt character specified when setting this property is the same as the current password character, <see cref="P:System.ComponentModel.MaskedTextProvider.PasswordChar" />. The two are required to be different.</exception>
	/// <exception cref="T:System.ArgumentException">The character specified when setting this property is not a valid password character, as determined by the <see cref="M:System.ComponentModel.MaskedTextProvider.IsValidPasswordChar(System.Char)" /> method.</exception>
	public char PromptChar
	{
		get
		{
			return promptChar;
		}
		set
		{
			if (value == passwordChar)
			{
				throw new InvalidOperationException(global::SR.GetString("The PasswordChar and PromptChar values cannot be the same."));
			}
			if (!IsPrintableChar(value))
			{
				throw new ArgumentException(global::SR.GetString("The specified character value is not allowed for this property."));
			}
			if (value == promptChar)
			{
				return;
			}
			promptChar = value;
			for (int i = 0; i < testString.Length; i++)
			{
				CharDescriptor charDescriptor = stringDescriptor[i];
				if (IsEditPosition(i) && !charDescriptor.IsAssigned)
				{
					testString[i] = promptChar;
				}
			}
		}
	}

	/// <summary>Gets or sets a value that determines how an input character that matches the prompt character should be handled.</summary>
	/// <returns>true if the prompt character entered as input causes the current editable position in the mask to be reset; otherwise, false to indicate that the prompt character is to be processed as a normal input character. The default is true.</returns>
	public bool ResetOnPrompt
	{
		get
		{
			return flagState[RESET_ON_PROMPT];
		}
		set
		{
			flagState[RESET_ON_PROMPT] = value;
		}
	}

	/// <summary>Gets or sets a value that determines how a space input character should be handled.</summary>
	/// <returns>true if the space input character causes the current editable position in the mask to be reset; otherwise, false to indicate that it is to be processed as a normal input character. The default is true.</returns>
	public bool ResetOnSpace
	{
		get
		{
			return flagState[SKIP_SPACE];
		}
		set
		{
			flagState[SKIP_SPACE] = value;
		}
	}

	/// <summary>Gets or sets a value indicating whether literal character positions in the mask can be overwritten by their same values.</summary>
	/// <returns>true to allow literals to be added back; otherwise, false to not allow the user to overwrite literal characters. The default is true.</returns>
	public bool SkipLiterals
	{
		get
		{
			return flagState[RESET_ON_LITERALS];
		}
		set
		{
			flagState[RESET_ON_LITERALS] = value;
		}
	}

	/// <summary>Gets the element at the specified position in the formatted string.</summary>
	/// <returns>The <see cref="T:System.Char" /> at the specified position in the formatted string.</returns>
	/// <param name="index">A zero-based index of the element to retrieve. </param>
	/// <exception cref="T:System.IndexOutOfRangeException">
	///   <paramref name="index" /> is less than zero or greater than or equal to the <see cref="P:System.ComponentModel.MaskedTextProvider.Length" /> of the mask.</exception>
	public char this[int index]
	{
		get
		{
			if (index < 0 || index >= testString.Length)
			{
				throw new IndexOutOfRangeException(index.ToString(CultureInfo.CurrentCulture));
			}
			return testString[index];
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.MaskedTextProvider" /> class using the specified mask.</summary>
	/// <param name="mask">A <see cref="T:System.String" /> that represents the input mask. </param>
	public MaskedTextProvider(string mask)
		: this(mask, null, allowPromptAsInput: true, '_', '\0', restrictToAscii: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.MaskedTextProvider" /> class using the specified mask and ASCII restriction value.</summary>
	/// <param name="mask">A <see cref="T:System.String" /> that represents the input mask. </param>
	/// <param name="restrictToAscii">true to restrict input to ASCII-compatible characters; otherwise false to allow the entire Unicode set. </param>
	public MaskedTextProvider(string mask, bool restrictToAscii)
		: this(mask, null, allowPromptAsInput: true, '_', '\0', restrictToAscii)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.MaskedTextProvider" /> class using the specified mask and culture.</summary>
	/// <param name="mask">A <see cref="T:System.String" /> that represents the input mask. </param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" /> that is used to set region-sensitive separator characters.</param>
	public MaskedTextProvider(string mask, CultureInfo culture)
		: this(mask, culture, allowPromptAsInput: true, '_', '\0', restrictToAscii: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.MaskedTextProvider" /> class using the specified mask, culture, and ASCII restriction value.</summary>
	/// <param name="mask">A <see cref="T:System.String" /> that represents the input mask. </param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" /> that is used to set region-sensitive separator characters.</param>
	/// <param name="restrictToAscii">true to restrict input to ASCII-compatible characters; otherwise false to allow the entire Unicode set. </param>
	public MaskedTextProvider(string mask, CultureInfo culture, bool restrictToAscii)
		: this(mask, culture, allowPromptAsInput: true, '_', '\0', restrictToAscii)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.MaskedTextProvider" /> class using the specified mask, password character, and prompt usage value.</summary>
	/// <param name="mask">A <see cref="T:System.String" /> that represents the input mask. </param>
	/// <param name="passwordChar">A <see cref="T:System.Char" /> that will be displayed for characters entered into a password string.</param>
	/// <param name="allowPromptAsInput">true to allow the prompt character as input; otherwise false. </param>
	public MaskedTextProvider(string mask, char passwordChar, bool allowPromptAsInput)
		: this(mask, null, allowPromptAsInput, '_', passwordChar, restrictToAscii: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.MaskedTextProvider" /> class using the specified mask, culture, password character, and prompt usage value.</summary>
	/// <param name="mask">A <see cref="T:System.String" /> that represents the input mask. </param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" /> that is used to set region-sensitive separator characters.</param>
	/// <param name="passwordChar">A <see cref="T:System.Char" /> that will be displayed for characters entered into a password string.</param>
	/// <param name="allowPromptAsInput">true to allow the prompt character as input; otherwise false. </param>
	public MaskedTextProvider(string mask, CultureInfo culture, char passwordChar, bool allowPromptAsInput)
		: this(mask, culture, allowPromptAsInput, '_', passwordChar, restrictToAscii: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.MaskedTextProvider" /> class using the specified mask, culture, prompt usage value, prompt character, password character, and ASCII restriction value.</summary>
	/// <param name="mask">A <see cref="T:System.String" /> that represents the input mask. </param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" /> that is used to set region-sensitive separator characters.</param>
	/// <param name="allowPromptAsInput">A <see cref="T:System.Boolean" /> value that specifies whether the prompt character should be allowed as a valid input character. </param>
	/// <param name="promptChar">A <see cref="T:System.Char" /> that will be displayed as a placeholder for user input.</param>
	/// <param name="passwordChar">A <see cref="T:System.Char" /> that will be displayed for characters entered into a password string.</param>
	/// <param name="restrictToAscii">true to restrict input to ASCII-compatible characters; otherwise false to allow the entire Unicode set. </param>
	/// <exception cref="T:System.ArgumentException">The mask parameter is null or <see cref="F:System.String.Empty" />.-or-The mask contains one or more non-printable characters. </exception>
	public MaskedTextProvider(string mask, CultureInfo culture, bool allowPromptAsInput, char promptChar, char passwordChar, bool restrictToAscii)
	{
		if (string.IsNullOrEmpty(mask))
		{
			throw new ArgumentException(global::SR.GetString("The Mask value cannot be null or empty."), "mask");
		}
		for (int i = 0; i < mask.Length; i++)
		{
			if (!IsPrintableChar(mask[i]))
			{
				throw new ArgumentException(global::SR.GetString("The specified mask contains invalid characters."));
			}
		}
		if (culture == null)
		{
			culture = CultureInfo.CurrentCulture;
		}
		flagState = default(BitVector32);
		this.mask = mask;
		this.promptChar = promptChar;
		this.passwordChar = passwordChar;
		if (culture.IsNeutralCulture)
		{
			CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
			foreach (CultureInfo cultureInfo in cultures)
			{
				if (culture.Equals(cultureInfo.Parent))
				{
					this.culture = cultureInfo;
					break;
				}
			}
			if (this.culture == null)
			{
				this.culture = CultureInfo.InvariantCulture;
			}
		}
		else
		{
			this.culture = culture;
		}
		if (!this.culture.IsReadOnly)
		{
			this.culture = CultureInfo.ReadOnly(this.culture);
		}
		flagState[ALLOW_PROMPT_AS_INPUT] = allowPromptAsInput;
		flagState[ASCII_ONLY] = restrictToAscii;
		flagState[INCLUDE_PROMPT] = false;
		flagState[INCLUDE_LITERALS] = true;
		flagState[RESET_ON_PROMPT] = true;
		flagState[SKIP_SPACE] = true;
		flagState[RESET_ON_LITERALS] = true;
		Initialize();
	}

	private void Initialize()
	{
		testString = new StringBuilder();
		stringDescriptor = new List<CharDescriptor>();
		CaseConversion caseConversion = CaseConversion.None;
		bool flag = false;
		int num = 0;
		CharType charType = CharType.Literal;
		string text = string.Empty;
		for (int i = 0; i < mask.Length; i++)
		{
			char c = mask[i];
			if (!flag)
			{
				switch (c)
				{
				case '.':
					text = culture.NumberFormat.NumberDecimalSeparator;
					charType = CharType.Separator;
					break;
				case ',':
					text = culture.NumberFormat.NumberGroupSeparator;
					charType = CharType.Separator;
					break;
				case ':':
					text = culture.DateTimeFormat.TimeSeparator;
					charType = CharType.Separator;
					break;
				case '/':
					text = culture.DateTimeFormat.DateSeparator;
					charType = CharType.Separator;
					break;
				case '$':
					text = culture.NumberFormat.CurrencySymbol;
					charType = CharType.Separator;
					break;
				case '<':
					caseConversion = CaseConversion.ToLower;
					continue;
				case '>':
					caseConversion = CaseConversion.ToUpper;
					continue;
				case '|':
					caseConversion = CaseConversion.None;
					continue;
				case '\\':
					flag = true;
					charType = CharType.Literal;
					continue;
				case '&':
				case '0':
				case 'A':
				case 'L':
					requiredEditChars++;
					c = promptChar;
					charType = CharType.EditRequired;
					break;
				case '#':
				case '9':
				case '?':
				case 'C':
				case 'a':
					optionalEditChars++;
					c = promptChar;
					charType = CharType.EditOptional;
					break;
				default:
					charType = CharType.Literal;
					break;
				}
			}
			else
			{
				flag = false;
			}
			CharDescriptor charDescriptor = new CharDescriptor(i, charType);
			if (IsEditPosition(charDescriptor))
			{
				charDescriptor.CaseConversion = caseConversion;
			}
			if (charType != CharType.Separator)
			{
				text = c.ToString();
			}
			string text2 = text;
			foreach (char value in text2)
			{
				testString.Append(value);
				stringDescriptor.Add(charDescriptor);
				num++;
			}
		}
		testString.Capacity = testString.Length;
	}

	/// <summary>Creates a copy of the current <see cref="T:System.ComponentModel.MaskedTextProvider" />.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.MaskedTextProvider" /> object this method creates, cast as an object.</returns>
	public object Clone()
	{
		Type type = GetType();
		MaskedTextProvider maskedTextProvider;
		if (type == maskTextProviderType)
		{
			maskedTextProvider = new MaskedTextProvider(Mask, Culture, AllowPromptAsInput, PromptChar, PasswordChar, AsciiOnly);
		}
		else
		{
			object[] args = new object[6] { Mask, Culture, AllowPromptAsInput, PromptChar, PasswordChar, AsciiOnly };
			maskedTextProvider = SecurityUtils.SecureCreateInstance(type, args) as MaskedTextProvider;
		}
		maskedTextProvider.ResetOnPrompt = false;
		maskedTextProvider.ResetOnSpace = false;
		maskedTextProvider.SkipLiterals = false;
		for (int i = 0; i < testString.Length; i++)
		{
			CharDescriptor charDescriptor = stringDescriptor[i];
			if (IsEditPosition(charDescriptor) && charDescriptor.IsAssigned)
			{
				maskedTextProvider.Replace(testString[i], i);
			}
		}
		maskedTextProvider.ResetOnPrompt = ResetOnPrompt;
		maskedTextProvider.ResetOnSpace = ResetOnSpace;
		maskedTextProvider.SkipLiterals = SkipLiterals;
		maskedTextProvider.IncludeLiterals = IncludeLiterals;
		maskedTextProvider.IncludePrompt = IncludePrompt;
		return maskedTextProvider;
	}

	/// <summary>Adds the specified input character to the end of the formatted string.</summary>
	/// <returns>true if the input character was added successfully; otherwise false.</returns>
	/// <param name="input">A <see cref="T:System.Char" /> value to be appended to the formatted string. </param>
	public bool Add(char input)
	{
		int testPosition;
		MaskedTextResultHint resultHint;
		return Add(input, out testPosition, out resultHint);
	}

	/// <summary>Adds the specified input character to the end of the formatted string, and then outputs position and descriptive information.</summary>
	/// <returns>true if the input character was added successfully; otherwise false.</returns>
	/// <param name="input">A <see cref="T:System.Char" /> value to be appended to the formatted string.</param>
	/// <param name="testPosition">The zero-based position in the formatted string where the attempt was made to add the character. An output parameter.</param>
	/// <param name="resultHint">A <see cref="T:System.ComponentModel.MaskedTextResultHint" /> that succinctly describes the result of the operation. An output parameter.</param>
	public bool Add(char input, out int testPosition, out MaskedTextResultHint resultHint)
	{
		int lastAssignedPosition = LastAssignedPosition;
		if (lastAssignedPosition == testString.Length - 1)
		{
			testPosition = testString.Length;
			resultHint = MaskedTextResultHint.UnavailableEditPosition;
			return false;
		}
		testPosition = lastAssignedPosition + 1;
		testPosition = FindEditPositionFrom(testPosition, direction: true);
		if (testPosition == -1)
		{
			resultHint = MaskedTextResultHint.UnavailableEditPosition;
			testPosition = testString.Length;
			return false;
		}
		if (!TestSetChar(input, testPosition, out resultHint))
		{
			return false;
		}
		return true;
	}

	/// <summary>Adds the characters in the specified input string to the end of the formatted string.</summary>
	/// <returns>true if all the characters from the input string were added successfully; otherwise false to indicate that no characters were added.</returns>
	/// <param name="input">A <see cref="T:System.String" /> containing character values to be appended to the formatted string. </param>
	/// <exception cref="T:System.ArgumentNullException">The<paramref name=" input" /> parameter is null.</exception>
	public bool Add(string input)
	{
		int testPosition;
		MaskedTextResultHint resultHint;
		return Add(input, out testPosition, out resultHint);
	}

	/// <summary>Adds the characters in the specified input string to the end of the formatted string, and then outputs position and descriptive information.</summary>
	/// <returns>true if all the characters from the input string were added successfully; otherwise false to indicate that no characters were added.</returns>
	/// <param name="input">A <see cref="T:System.String" /> containing character values to be appended to the formatted string. </param>
	/// <param name="testPosition">The zero-based position in the formatted string where the attempt was made to add the character. An output parameter.</param>
	/// <param name="resultHint">A <see cref="T:System.ComponentModel.MaskedTextResultHint" /> that succinctly describes the result of the operation. An output parameter.</param>
	public bool Add(string input, out int testPosition, out MaskedTextResultHint resultHint)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		testPosition = LastAssignedPosition + 1;
		if (input.Length == 0)
		{
			resultHint = MaskedTextResultHint.NoEffect;
			return true;
		}
		return TestSetString(input, testPosition, out testPosition, out resultHint);
	}

	/// <summary>Clears all the editable input characters from the formatted string, replacing them with prompt characters.</summary>
	public void Clear()
	{
		Clear(out var _);
	}

	/// <summary>Clears all the editable input characters from the formatted string, replacing them with prompt characters, and then outputs descriptive information.</summary>
	/// <param name="resultHint">A <see cref="T:System.ComponentModel.MaskedTextResultHint" /> that succinctly describes the result of the operation. An output parameter. </param>
	public void Clear(out MaskedTextResultHint resultHint)
	{
		if (assignedCharCount == 0)
		{
			resultHint = MaskedTextResultHint.NoEffect;
			return;
		}
		resultHint = MaskedTextResultHint.Success;
		for (int i = 0; i < testString.Length; i++)
		{
			ResetChar(i);
		}
	}

	/// <summary>Returns the position of the first assigned editable position after the specified position using the specified search direction.</summary>
	/// <returns>If successful, an <see cref="T:System.Int32" /> representing the zero-based position of the first assigned editable position encountered; otherwise <see cref="P:System.ComponentModel.MaskedTextProvider.InvalidIndex" />.</returns>
	/// <param name="position">The zero-based position in the formatted string to start the search.</param>
	/// <param name="direction">A <see cref="T:System.Boolean" /> indicating the search direction; either true to search forward or false to search backward.</param>
	public int FindAssignedEditPositionFrom(int position, bool direction)
	{
		if (assignedCharCount == 0)
		{
			return -1;
		}
		int startPosition;
		int endPosition;
		if (direction)
		{
			startPosition = position;
			endPosition = testString.Length - 1;
		}
		else
		{
			startPosition = 0;
			endPosition = position;
		}
		return FindAssignedEditPositionInRange(startPosition, endPosition, direction);
	}

	/// <summary>Returns the position of the first assigned editable position between the specified positions using the specified search direction.</summary>
	/// <returns>If successful, an <see cref="T:System.Int32" /> representing the zero-based position of the first assigned editable position encountered; otherwise <see cref="P:System.ComponentModel.MaskedTextProvider.InvalidIndex" />.</returns>
	/// <param name="startPosition">The zero-based position in the formatted string where the search starts.</param>
	/// <param name="endPosition">The zero-based position in the formatted string where the search ends.</param>
	/// <param name="direction">A <see cref="T:System.Boolean" /> indicating the search direction; either true to search forward or false to search backward.</param>
	public int FindAssignedEditPositionInRange(int startPosition, int endPosition, bool direction)
	{
		if (assignedCharCount == 0)
		{
			return -1;
		}
		return FindEditPositionInRange(startPosition, endPosition, direction, 2);
	}

	/// <summary>Returns the position of the first editable position after the specified position using the specified search direction.</summary>
	/// <returns>If successful, an <see cref="T:System.Int32" /> representing the zero-based position of the first editable position encountered; otherwise <see cref="P:System.ComponentModel.MaskedTextProvider.InvalidIndex" />.</returns>
	/// <param name="position">The zero-based position in the formatted string to start the search.</param>
	/// <param name="direction">A <see cref="T:System.Boolean" /> indicating the search direction; either true to search forward or false to search backward.</param>
	public int FindEditPositionFrom(int position, bool direction)
	{
		int startPosition;
		int endPosition;
		if (direction)
		{
			startPosition = position;
			endPosition = testString.Length - 1;
		}
		else
		{
			startPosition = 0;
			endPosition = position;
		}
		return FindEditPositionInRange(startPosition, endPosition, direction);
	}

	/// <summary>Returns the position of the first editable position between the specified positions using the specified search direction.</summary>
	/// <returns>If successful, an <see cref="T:System.Int32" /> representing the zero-based position of the first editable position encountered; otherwise <see cref="P:System.ComponentModel.MaskedTextProvider.InvalidIndex" />.</returns>
	/// <param name="startPosition">The zero-based position in the formatted string where the search starts.</param>
	/// <param name="endPosition">The zero-based position in the formatted string where the search ends.</param>
	/// <param name="direction">A <see cref="T:System.Boolean" /> indicating the search direction; either true to search forward or false to search backward.</param>
	public int FindEditPositionInRange(int startPosition, int endPosition, bool direction)
	{
		CharType charTypeFlags = CharType.EditOptional | CharType.EditRequired;
		return FindPositionInRange(startPosition, endPosition, direction, charTypeFlags);
	}

	private int FindEditPositionInRange(int startPosition, int endPosition, bool direction, byte assignedStatus)
	{
		do
		{
			int num = FindEditPositionInRange(startPosition, endPosition, direction);
			if (num == -1)
			{
				break;
			}
			CharDescriptor charDescriptor = stringDescriptor[num];
			switch (assignedStatus)
			{
			case 1:
				if (!charDescriptor.IsAssigned)
				{
					return num;
				}
				break;
			case 2:
				if (charDescriptor.IsAssigned)
				{
					return num;
				}
				break;
			default:
				return num;
			}
			if (direction)
			{
				startPosition++;
			}
			else
			{
				endPosition--;
			}
		}
		while (startPosition <= endPosition);
		return -1;
	}

	/// <summary>Returns the position of the first non-editable position after the specified position using the specified search direction.</summary>
	/// <returns>If successful, an <see cref="T:System.Int32" /> representing the zero-based position of the first literal position encountered; otherwise <see cref="P:System.ComponentModel.MaskedTextProvider.InvalidIndex" />.</returns>
	/// <param name="position">The zero-based position in the formatted string to start the search.</param>
	/// <param name="direction">A <see cref="T:System.Boolean" /> indicating the search direction; either true to search forward or false to search backward.</param>
	public int FindNonEditPositionFrom(int position, bool direction)
	{
		int startPosition;
		int endPosition;
		if (direction)
		{
			startPosition = position;
			endPosition = testString.Length - 1;
		}
		else
		{
			startPosition = 0;
			endPosition = position;
		}
		return FindNonEditPositionInRange(startPosition, endPosition, direction);
	}

	/// <summary>Returns the position of the first non-editable position between the specified positions using the specified search direction.</summary>
	/// <returns>If successful, an <see cref="T:System.Int32" /> representing the zero-based position of the first literal position encountered; otherwise <see cref="P:System.ComponentModel.MaskedTextProvider.InvalidIndex" />.</returns>
	/// <param name="startPosition">The zero-based position in the formatted string where the search starts.</param>
	/// <param name="endPosition">The zero-based position in the formatted string where the search ends.</param>
	/// <param name="direction">A <see cref="T:System.Boolean" /> indicating the search direction; either true to search forward or false to search backward.</param>
	public int FindNonEditPositionInRange(int startPosition, int endPosition, bool direction)
	{
		CharType charTypeFlags = CharType.Separator | CharType.Literal;
		return FindPositionInRange(startPosition, endPosition, direction, charTypeFlags);
	}

	private int FindPositionInRange(int startPosition, int endPosition, bool direction, CharType charTypeFlags)
	{
		if (startPosition < 0)
		{
			startPosition = 0;
		}
		if (endPosition >= testString.Length)
		{
			endPosition = testString.Length - 1;
		}
		if (startPosition > endPosition)
		{
			return -1;
		}
		while (startPosition <= endPosition)
		{
			int num = (direction ? startPosition++ : endPosition--);
			CharDescriptor charDescriptor = stringDescriptor[num];
			if ((charDescriptor.CharType & charTypeFlags) == charDescriptor.CharType)
			{
				return num;
			}
		}
		return -1;
	}

	/// <summary>Returns the position of the first unassigned editable position after the specified position using the specified search direction.</summary>
	/// <returns>If successful, an <see cref="T:System.Int32" /> representing the zero-based position of the first unassigned editable position encountered; otherwise <see cref="P:System.ComponentModel.MaskedTextProvider.InvalidIndex" />.</returns>
	/// <param name="position">The zero-based position in the formatted string to start the search.</param>
	/// <param name="direction">A <see cref="T:System.Boolean" /> indicating the search direction; either true to search forward or false to search backward.</param>
	public int FindUnassignedEditPositionFrom(int position, bool direction)
	{
		int startPosition;
		int endPosition;
		if (direction)
		{
			startPosition = position;
			endPosition = testString.Length - 1;
		}
		else
		{
			startPosition = 0;
			endPosition = position;
		}
		return FindEditPositionInRange(startPosition, endPosition, direction, 1);
	}

	/// <summary>Returns the position of the first unassigned editable position between the specified positions using the specified search direction.</summary>
	/// <returns>If successful, an <see cref="T:System.Int32" /> representing the zero-based position of the first unassigned editable position encountered; otherwise <see cref="P:System.ComponentModel.MaskedTextProvider.InvalidIndex" />.</returns>
	/// <param name="startPosition">The zero-based position in the formatted string where the search starts.</param>
	/// <param name="endPosition">The zero-based position in the formatted string where the search ends.</param>
	/// <param name="direction">A <see cref="T:System.Boolean" /> indicating the search direction; either true to search forward or false to search backward.</param>
	public int FindUnassignedEditPositionInRange(int startPosition, int endPosition, bool direction)
	{
		int num;
		while (true)
		{
			num = FindEditPositionInRange(startPosition, endPosition, direction, 0);
			if (num == -1)
			{
				return -1;
			}
			if (!stringDescriptor[num].IsAssigned)
			{
				break;
			}
			if (direction)
			{
				startPosition++;
			}
			else
			{
				endPosition--;
			}
		}
		return num;
	}

	/// <summary>Determines whether the specified <see cref="T:System.ComponentModel.MaskedTextResultHint" /> denotes success or failure.</summary>
	/// <returns>true if the specified <see cref="T:System.ComponentModel.MaskedTextResultHint" /> value represents a success; otherwise, false if it represents failure.</returns>
	/// <param name="hint">A <see cref="T:System.ComponentModel.MaskedTextResultHint" /> value typically obtained as an output parameter from a previous operation. </param>
	public static bool GetOperationResultFromHint(MaskedTextResultHint hint)
	{
		return hint > MaskedTextResultHint.Unknown;
	}

	/// <summary>Inserts the specified character at the specified position within the formatted string.</summary>
	/// <returns>true if the insertion was successful; otherwise, false.</returns>
	/// <param name="input">The <see cref="T:System.Char" /> to be inserted. </param>
	/// <param name="position">The zero-based position in the formatted string to insert the character.</param>
	public bool InsertAt(char input, int position)
	{
		if (position < 0 || position >= testString.Length)
		{
			return false;
		}
		return InsertAt(input.ToString(), position);
	}

	/// <summary>Inserts the specified character at the specified position within the formatted string, returning the last insertion position and the status of the operation.</summary>
	/// <returns>true if the insertion was successful; otherwise, false.</returns>
	/// <param name="input">The <see cref="T:System.Char" /> to be inserted. </param>
	/// <param name="position">The zero-based position in the formatted string to insert the character.</param>
	/// <param name="testPosition">If the method is successful, the last position where a character was inserted; otherwise, the first position where the insertion failed. An output parameter.</param>
	/// <param name="resultHint">A <see cref="T:System.ComponentModel.MaskedTextResultHint" /> that succinctly describes the result of the insertion operation. An output parameter.</param>
	public bool InsertAt(char input, int position, out int testPosition, out MaskedTextResultHint resultHint)
	{
		return InsertAt(input.ToString(), position, out testPosition, out resultHint);
	}

	/// <summary>Inserts the specified string at a specified position within the formatted string. </summary>
	/// <returns>true if the insertion was successful; otherwise, false.</returns>
	/// <param name="input">The <see cref="T:System.String" /> to be inserted. </param>
	/// <param name="position">The zero-based position in the formatted string to insert the input string.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="input" /> parameter is null.</exception>
	public bool InsertAt(string input, int position)
	{
		int testPosition;
		MaskedTextResultHint resultHint;
		return InsertAt(input, position, out testPosition, out resultHint);
	}

	/// <summary>Inserts the specified string at a specified position within the formatted string, returning the last insertion position and the status of the operation. </summary>
	/// <returns>true if the insertion was successful; otherwise, false.</returns>
	/// <param name="input">The <see cref="T:System.String" /> to be inserted. </param>
	/// <param name="position">The zero-based position in the formatted string to insert the input string.</param>
	/// <param name="testPosition">If the method is successful, the last position where a character was inserted; otherwise, the first position where the insertion failed. An output parameter.</param>
	/// <param name="resultHint">A <see cref="T:System.ComponentModel.MaskedTextResultHint" /> that succinctly describes the result of the insertion operation. An output parameter.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="input" /> parameter is null.</exception>
	public bool InsertAt(string input, int position, out int testPosition, out MaskedTextResultHint resultHint)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		if (position < 0 || position >= testString.Length)
		{
			testPosition = position;
			resultHint = MaskedTextResultHint.PositionOutOfRange;
			return false;
		}
		return InsertAtInt(input, position, out testPosition, out resultHint, testOnly: false);
	}

	private bool InsertAtInt(string input, int position, out int testPosition, out MaskedTextResultHint resultHint, bool testOnly)
	{
		if (input.Length == 0)
		{
			testPosition = position;
			resultHint = MaskedTextResultHint.NoEffect;
			return true;
		}
		if (!TestString(input, position, out testPosition, out resultHint))
		{
			return false;
		}
		int num = FindEditPositionFrom(position, direction: true);
		bool flag = FindAssignedEditPositionInRange(num, testPosition, direction: true) != -1;
		int lastAssignedPosition = LastAssignedPosition;
		if (flag && testPosition == testString.Length - 1)
		{
			resultHint = MaskedTextResultHint.UnavailableEditPosition;
			testPosition = testString.Length;
			return false;
		}
		int num2 = FindEditPositionFrom(testPosition + 1, direction: true);
		if (flag)
		{
			MaskedTextResultHint resultHint2 = MaskedTextResultHint.Unknown;
			while (true)
			{
				if (num2 == -1)
				{
					resultHint = MaskedTextResultHint.UnavailableEditPosition;
					testPosition = testString.Length;
					return false;
				}
				if (stringDescriptor[num].IsAssigned && !TestChar(testString[num], num2, out resultHint2))
				{
					resultHint = resultHint2;
					testPosition = num2;
					return false;
				}
				if (num == lastAssignedPosition)
				{
					break;
				}
				num = FindEditPositionFrom(num + 1, direction: true);
				num2 = FindEditPositionFrom(num2 + 1, direction: true);
			}
			if (resultHint2 > resultHint)
			{
				resultHint = resultHint2;
			}
		}
		if (testOnly)
		{
			return true;
		}
		if (flag)
		{
			while (num >= position)
			{
				if (stringDescriptor[num].IsAssigned)
				{
					SetChar(testString[num], num2);
				}
				else
				{
					ResetChar(num2);
				}
				num2 = FindEditPositionFrom(num2 - 1, direction: false);
				num = FindEditPositionFrom(num - 1, direction: false);
			}
		}
		SetString(input, position);
		return true;
	}

	private static bool IsAscii(char c)
	{
		if (c >= '!')
		{
			return c <= '~';
		}
		return false;
	}

	private static bool IsAciiAlphanumeric(char c)
	{
		if ((c < '0' || c > '9') && (c < 'A' || c > 'Z'))
		{
			if (c >= 'a')
			{
				return c <= 'z';
			}
			return false;
		}
		return true;
	}

	private static bool IsAlphanumeric(char c)
	{
		if (!char.IsLetter(c))
		{
			return char.IsDigit(c);
		}
		return true;
	}

	private static bool IsAsciiLetter(char c)
	{
		if (c < 'A' || c > 'Z')
		{
			if (c >= 'a')
			{
				return c <= 'z';
			}
			return false;
		}
		return true;
	}

	/// <summary>Determines whether the specified position is available for assignment.</summary>
	/// <returns>true if the specified position in the formatted string is editable and has not been assigned to yet; otherwise false.</returns>
	/// <param name="position">The zero-based position in the mask to test.</param>
	public bool IsAvailablePosition(int position)
	{
		if (position < 0 || position >= testString.Length)
		{
			return false;
		}
		CharDescriptor charDescriptor = stringDescriptor[position];
		if (IsEditPosition(charDescriptor))
		{
			return !charDescriptor.IsAssigned;
		}
		return false;
	}

	/// <summary>Determines whether the specified position is editable. </summary>
	/// <returns>true if the specified position in the formatted string is editable; otherwise false.</returns>
	/// <param name="position">The zero-based position in the mask to test.</param>
	public bool IsEditPosition(int position)
	{
		if (position < 0 || position >= testString.Length)
		{
			return false;
		}
		return IsEditPosition(stringDescriptor[position]);
	}

	private static bool IsEditPosition(CharDescriptor charDescriptor)
	{
		if (charDescriptor.CharType != CharType.EditRequired)
		{
			return charDescriptor.CharType == CharType.EditOptional;
		}
		return true;
	}

	private static bool IsLiteralPosition(CharDescriptor charDescriptor)
	{
		if (charDescriptor.CharType != CharType.Literal)
		{
			return charDescriptor.CharType == CharType.Separator;
		}
		return true;
	}

	private static bool IsPrintableChar(char c)
	{
		if (!char.IsLetterOrDigit(c) && !char.IsPunctuation(c) && !char.IsSymbol(c))
		{
			return c == ' ';
		}
		return true;
	}

	/// <summary>Determines whether the specified character is a valid input character.</summary>
	/// <returns>true if the specified character contains a valid input value; otherwise false.</returns>
	/// <param name="c">The <see cref="T:System.Char" /> value to test.</param>
	public static bool IsValidInputChar(char c)
	{
		return IsPrintableChar(c);
	}

	/// <summary>Determines whether the specified character is a valid mask character.</summary>
	/// <returns>true if the specified character contains a valid mask value; otherwise false.</returns>
	/// <param name="c">The <see cref="T:System.Char" /> value to test.</param>
	public static bool IsValidMaskChar(char c)
	{
		return IsPrintableChar(c);
	}

	/// <summary>Determines whether the specified character is a valid password character.</summary>
	/// <returns>true if the specified character contains a valid password value; otherwise false.</returns>
	/// <param name="c">The <see cref="T:System.Char" /> value to test.</param>
	public static bool IsValidPasswordChar(char c)
	{
		if (!IsPrintableChar(c))
		{
			return c == '\0';
		}
		return true;
	}

	/// <summary>Removes the last assigned character from the formatted string.</summary>
	/// <returns>true if the character was successfully removed; otherwise, false.</returns>
	public bool Remove()
	{
		int testPosition;
		MaskedTextResultHint resultHint;
		return Remove(out testPosition, out resultHint);
	}

	/// <summary>Removes the last assigned character from the formatted string, and then outputs the removal position and descriptive information.</summary>
	/// <returns>true if the character was successfully removed; otherwise, false.</returns>
	/// <param name="testPosition">The zero-based position in the formatted string where the character was actually removed. An output parameter.</param>
	/// <param name="resultHint">A <see cref="T:System.ComponentModel.MaskedTextResultHint" /> that succinctly describes the result of the operation. An output parameter.</param>
	public bool Remove(out int testPosition, out MaskedTextResultHint resultHint)
	{
		int lastAssignedPosition = LastAssignedPosition;
		if (lastAssignedPosition == -1)
		{
			testPosition = 0;
			resultHint = MaskedTextResultHint.NoEffect;
			return true;
		}
		ResetChar(lastAssignedPosition);
		testPosition = lastAssignedPosition;
		resultHint = MaskedTextResultHint.Success;
		return true;
	}

	/// <summary>Removes the assigned character at the specified position from the formatted string.</summary>
	/// <returns>true if the character was successfully removed; otherwise, false.</returns>
	/// <param name="position">The zero-based position of the assigned character to remove.</param>
	public bool RemoveAt(int position)
	{
		return RemoveAt(position, position);
	}

	/// <summary>Removes the assigned characters between the specified positions from the formatted string.</summary>
	/// <returns>true if the character was successfully removed; otherwise, false.</returns>
	/// <param name="startPosition">The zero-based index of the first assigned character to remove.</param>
	/// <param name="endPosition">The zero-based index of the last assigned character to remove.</param>
	public bool RemoveAt(int startPosition, int endPosition)
	{
		int testPosition;
		MaskedTextResultHint resultHint;
		return RemoveAt(startPosition, endPosition, out testPosition, out resultHint);
	}

	/// <summary>Removes the assigned characters between the specified positions from the formatted string, and then outputs the removal position and descriptive information.</summary>
	/// <returns>true if the character was successfully removed; otherwise, false.</returns>
	/// <param name="startPosition">The zero-based index of the first assigned character to remove.</param>
	/// <param name="endPosition">The zero-based index of the last assigned character to remove.</param>
	/// <param name="testPosition">If successful, the zero-based position in the formatted string of where the characters were actually removed; otherwise, the first position where the operation failed. An output parameter.</param>
	/// <param name="resultHint">A <see cref="T:System.ComponentModel.MaskedTextResultHint" /> that succinctly describes the result of the operation. An output parameter.</param>
	public bool RemoveAt(int startPosition, int endPosition, out int testPosition, out MaskedTextResultHint resultHint)
	{
		if (endPosition >= testString.Length)
		{
			testPosition = endPosition;
			resultHint = MaskedTextResultHint.PositionOutOfRange;
			return false;
		}
		if (startPosition < 0 || startPosition > endPosition)
		{
			testPosition = startPosition;
			resultHint = MaskedTextResultHint.PositionOutOfRange;
			return false;
		}
		return RemoveAtInt(startPosition, endPosition, out testPosition, out resultHint, testOnly: false);
	}

	private bool RemoveAtInt(int startPosition, int endPosition, out int testPosition, out MaskedTextResultHint resultHint, bool testOnly)
	{
		int lastAssignedPosition = LastAssignedPosition;
		int num = FindEditPositionInRange(startPosition, endPosition, direction: true);
		resultHint = MaskedTextResultHint.NoEffect;
		if (num == -1 || num > lastAssignedPosition)
		{
			testPosition = startPosition;
			return true;
		}
		testPosition = startPosition;
		bool num2 = endPosition < lastAssignedPosition;
		if (FindAssignedEditPositionInRange(startPosition, endPosition, direction: true) != -1)
		{
			resultHint = MaskedTextResultHint.Success;
		}
		if (num2)
		{
			int num3 = FindEditPositionFrom(endPosition + 1, direction: true);
			int num4 = num3;
			startPosition = num;
			while (true)
			{
				char c = testString[num3];
				CharDescriptor charDescriptor = stringDescriptor[num3];
				if ((c != PromptChar || charDescriptor.IsAssigned) && !TestChar(c, num, out var resultHint2))
				{
					resultHint = resultHint2;
					testPosition = num;
					return false;
				}
				if (num3 == lastAssignedPosition)
				{
					break;
				}
				num3 = FindEditPositionFrom(num3 + 1, direction: true);
				num = FindEditPositionFrom(num + 1, direction: true);
			}
			if (MaskedTextResultHint.SideEffect > resultHint)
			{
				resultHint = MaskedTextResultHint.SideEffect;
			}
			if (testOnly)
			{
				return true;
			}
			num3 = num4;
			num = startPosition;
			while (true)
			{
				char c2 = testString[num3];
				CharDescriptor charDescriptor2 = stringDescriptor[num3];
				if (c2 == PromptChar && !charDescriptor2.IsAssigned)
				{
					ResetChar(num);
				}
				else
				{
					SetChar(c2, num);
					ResetChar(num3);
				}
				if (num3 == lastAssignedPosition)
				{
					break;
				}
				num3 = FindEditPositionFrom(num3 + 1, direction: true);
				num = FindEditPositionFrom(num + 1, direction: true);
			}
			startPosition = num + 1;
		}
		if (startPosition <= endPosition)
		{
			ResetString(startPosition, endPosition);
		}
		return true;
	}

	/// <summary>Replaces a single character at or beyond the specified position with the specified character value.</summary>
	/// <returns>true if the character was successfully replaced; otherwise, false.</returns>
	/// <param name="input">The <see cref="T:System.Char" /> value that replaces the existing value.</param>
	/// <param name="position">The zero-based position to search for the first editable character to replace.</param>
	public bool Replace(char input, int position)
	{
		int testPosition;
		MaskedTextResultHint resultHint;
		return Replace(input, position, out testPosition, out resultHint);
	}

	/// <summary>Replaces a single character at or beyond the specified position with the specified character value, and then outputs the removal position and descriptive information.</summary>
	/// <returns>true if the character was successfully replaced; otherwise, false.</returns>
	/// <param name="input">The <see cref="T:System.Char" /> value that replaces the existing value.</param>
	/// <param name="position">The zero-based position to search for the first editable character to replace.</param>
	/// <param name="testPosition">If successful, the zero-based position in the formatted string where the last character was actually replaced; otherwise, the first position where the operation failed. An output parameter.</param>
	/// <param name="resultHint">A <see cref="T:System.ComponentModel.MaskedTextResultHint" /> that succinctly describes the result of the replacement operation. An output parameter.</param>
	public bool Replace(char input, int position, out int testPosition, out MaskedTextResultHint resultHint)
	{
		if (position < 0 || position >= testString.Length)
		{
			testPosition = position;
			resultHint = MaskedTextResultHint.PositionOutOfRange;
			return false;
		}
		testPosition = position;
		if (!TestEscapeChar(input, testPosition))
		{
			testPosition = FindEditPositionFrom(testPosition, direction: true);
		}
		if (testPosition == -1)
		{
			resultHint = MaskedTextResultHint.UnavailableEditPosition;
			testPosition = position;
			return false;
		}
		if (!TestSetChar(input, testPosition, out resultHint))
		{
			return false;
		}
		return true;
	}

	/// <summary>Replaces a single character between the specified starting and ending positions with the specified character value, and then outputs the removal position and descriptive information.</summary>
	/// <returns>true if the character was successfully replaced; otherwise, false.</returns>
	/// <param name="input">The <see cref="T:System.Char" /> value that replaces the existing value.</param>
	/// <param name="startPosition">The zero-based position in the formatted string where the replacement starts. </param>
	/// <param name="endPosition">The zero-based position in the formatted string where the replacement ends. </param>
	/// <param name="testPosition">If successful, the zero-based position in the formatted string where the last character was actually replaced; otherwise, the first position where the operation failed. An output parameter.</param>
	/// <param name="resultHint">A <see cref="T:System.ComponentModel.MaskedTextResultHint" /> that succinctly describes the result of the replacement operation. An output parameter.</param>
	public bool Replace(char input, int startPosition, int endPosition, out int testPosition, out MaskedTextResultHint resultHint)
	{
		if (endPosition >= testString.Length)
		{
			testPosition = endPosition;
			resultHint = MaskedTextResultHint.PositionOutOfRange;
			return false;
		}
		if (startPosition < 0 || startPosition > endPosition)
		{
			testPosition = startPosition;
			resultHint = MaskedTextResultHint.PositionOutOfRange;
			return false;
		}
		if (startPosition == endPosition)
		{
			testPosition = startPosition;
			return TestSetChar(input, startPosition, out resultHint);
		}
		return Replace(input.ToString(), startPosition, endPosition, out testPosition, out resultHint);
	}

	/// <summary>Replaces a range of editable characters starting at the specified position with the specified string.</summary>
	/// <returns>true if all the characters were successfully replaced; otherwise, false.</returns>
	/// <param name="input">The <see cref="T:System.String" /> value used to replace the existing editable characters.</param>
	/// <param name="position">The zero-based position to search for the first editable character to replace.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="input" /> parameter is null.</exception>
	public bool Replace(string input, int position)
	{
		int testPosition;
		MaskedTextResultHint resultHint;
		return Replace(input, position, out testPosition, out resultHint);
	}

	/// <summary>Replaces a range of editable characters starting at the specified position with the specified string, and then outputs the removal position and descriptive information.</summary>
	/// <returns>true if all the characters were successfully replaced; otherwise, false.</returns>
	/// <param name="input">The <see cref="T:System.String" /> value used to replace the existing editable characters.</param>
	/// <param name="position">The zero-based position to search for the first editable character to replace.</param>
	/// <param name="testPosition">If successful, the zero-based position in the formatted string where the last character was actually replaced; otherwise, the first position where the operation failed. An output parameter.</param>
	/// <param name="resultHint">A <see cref="T:System.ComponentModel.MaskedTextResultHint" /> that succinctly describes the result of the replacement operation. An output parameter.</param>
	public bool Replace(string input, int position, out int testPosition, out MaskedTextResultHint resultHint)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		if (position < 0 || position >= testString.Length)
		{
			testPosition = position;
			resultHint = MaskedTextResultHint.PositionOutOfRange;
			return false;
		}
		if (input.Length == 0)
		{
			return RemoveAt(position, position, out testPosition, out resultHint);
		}
		if (!TestSetString(input, position, out testPosition, out resultHint))
		{
			return false;
		}
		return true;
	}

	/// <summary>Replaces a range of editable characters between the specified starting and ending positions with the specified string, and then outputs the removal position and descriptive information.</summary>
	/// <returns>true if all the characters were successfully replaced; otherwise, false.</returns>
	/// <param name="input">The <see cref="T:System.String" /> value used to replace the existing editable characters.</param>
	/// <param name="startPosition">The zero-based position in the formatted string where the replacement starts. </param>
	/// <param name="endPosition">The zero-based position in the formatted string where the replacement ends. </param>
	/// <param name="testPosition">If successful, the zero-based position in the formatted string where the last character was actually replaced; otherwise, the first position where the operation failed. An output parameter.</param>
	/// <param name="resultHint">A <see cref="T:System.ComponentModel.MaskedTextResultHint" /> that succinctly describes the result of the replacement operation. An output parameter.</param>
	public bool Replace(string input, int startPosition, int endPosition, out int testPosition, out MaskedTextResultHint resultHint)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		if (endPosition >= testString.Length)
		{
			testPosition = endPosition;
			resultHint = MaskedTextResultHint.PositionOutOfRange;
			return false;
		}
		if (startPosition < 0 || startPosition > endPosition)
		{
			testPosition = startPosition;
			resultHint = MaskedTextResultHint.PositionOutOfRange;
			return false;
		}
		if (input.Length == 0)
		{
			return RemoveAt(startPosition, endPosition, out testPosition, out resultHint);
		}
		if (!TestString(input, startPosition, out testPosition, out resultHint))
		{
			return false;
		}
		if (assignedCharCount > 0)
		{
			MaskedTextResultHint resultHint2;
			if (testPosition < endPosition)
			{
				if (!RemoveAtInt(testPosition + 1, endPosition, out var testPosition2, out resultHint2, testOnly: false))
				{
					testPosition = testPosition2;
					resultHint = resultHint2;
					return false;
				}
				if (resultHint2 == MaskedTextResultHint.Success && resultHint != resultHint2)
				{
					resultHint = MaskedTextResultHint.SideEffect;
				}
			}
			else if (testPosition > endPosition)
			{
				int lastAssignedPosition = LastAssignedPosition;
				int position = testPosition + 1;
				int position2 = endPosition + 1;
				while (true)
				{
					position2 = FindEditPositionFrom(position2, direction: true);
					position = FindEditPositionFrom(position, direction: true);
					if (position == -1)
					{
						testPosition = testString.Length;
						resultHint = MaskedTextResultHint.UnavailableEditPosition;
						return false;
					}
					if (!TestChar(testString[position2], position, out resultHint2))
					{
						testPosition = position;
						resultHint = resultHint2;
						return false;
					}
					if (resultHint2 == MaskedTextResultHint.Success && resultHint != resultHint2)
					{
						resultHint = MaskedTextResultHint.Success;
					}
					if (position2 == lastAssignedPosition)
					{
						break;
					}
					position2++;
					position++;
				}
				while (position > testPosition)
				{
					SetChar(testString[position2], position);
					position2 = FindEditPositionFrom(position2 - 1, direction: false);
					position = FindEditPositionFrom(position - 1, direction: false);
				}
			}
		}
		SetString(input, startPosition);
		return true;
	}

	private void ResetChar(int testPosition)
	{
		CharDescriptor charDescriptor = stringDescriptor[testPosition];
		if (IsEditPosition(testPosition) && charDescriptor.IsAssigned)
		{
			charDescriptor.IsAssigned = false;
			testString[testPosition] = promptChar;
			assignedCharCount--;
			if (charDescriptor.CharType == CharType.EditRequired)
			{
				requiredCharCount--;
			}
		}
	}

	private void ResetString(int startPosition, int endPosition)
	{
		startPosition = FindAssignedEditPositionFrom(startPosition, direction: true);
		if (startPosition != -1)
		{
			endPosition = FindAssignedEditPositionFrom(endPosition, direction: false);
			while (startPosition <= endPosition)
			{
				startPosition = FindAssignedEditPositionFrom(startPosition, direction: true);
				ResetChar(startPosition);
				startPosition++;
			}
		}
	}

	/// <summary>Sets the formatted string to the specified input string.</summary>
	/// <returns>true if all the characters were successfully set; otherwise, false.</returns>
	/// <param name="input">The <see cref="T:System.String" /> value used to set the formatted string.</param>
	/// <exception cref="T:System.ArgumentNullException">The<paramref name=" input" /> parameter is null.</exception>
	public bool Set(string input)
	{
		int testPosition;
		MaskedTextResultHint resultHint;
		return Set(input, out testPosition, out resultHint);
	}

	/// <summary>Sets the formatted string to the specified input string, and then outputs the removal position and descriptive information.</summary>
	/// <returns>true if all the characters were successfully set; otherwise, false.</returns>
	/// <param name="input">The <see cref="T:System.String" /> value used to set the formatted string.</param>
	/// <param name="testPosition">If successful, the zero-based position in the formatted string where the last character was actually set; otherwise, the first position where the operation failed. An output parameter.</param>
	/// <param name="resultHint">A <see cref="T:System.ComponentModel.MaskedTextResultHint" /> that succinctly describes the result of the set operation. An output parameter.</param>
	/// <exception cref="T:System.ArgumentNullException">The<paramref name=" input" /> parameter is null.</exception>
	public bool Set(string input, out int testPosition, out MaskedTextResultHint resultHint)
	{
		if (input == null)
		{
			throw new ArgumentNullException("input");
		}
		resultHint = MaskedTextResultHint.Unknown;
		testPosition = 0;
		if (input.Length == 0)
		{
			Clear(out resultHint);
			return true;
		}
		if (!TestSetString(input, testPosition, out testPosition, out resultHint))
		{
			return false;
		}
		int num = FindAssignedEditPositionFrom(testPosition + 1, direction: true);
		if (num != -1)
		{
			ResetString(num, testString.Length - 1);
		}
		return true;
	}

	private void SetChar(char input, int position)
	{
		CharDescriptor charDescriptor = stringDescriptor[position];
		SetChar(input, position, charDescriptor);
	}

	private void SetChar(char input, int position, CharDescriptor charDescriptor)
	{
		_ = stringDescriptor[position];
		if (TestEscapeChar(input, position, charDescriptor))
		{
			ResetChar(position);
			return;
		}
		if (char.IsLetter(input))
		{
			if (char.IsUpper(input))
			{
				if (charDescriptor.CaseConversion == CaseConversion.ToLower)
				{
					input = culture.TextInfo.ToLower(input);
				}
			}
			else if (charDescriptor.CaseConversion == CaseConversion.ToUpper)
			{
				input = culture.TextInfo.ToUpper(input);
			}
		}
		testString[position] = input;
		if (!charDescriptor.IsAssigned)
		{
			charDescriptor.IsAssigned = true;
			assignedCharCount++;
			if (charDescriptor.CharType == CharType.EditRequired)
			{
				requiredCharCount++;
			}
		}
	}

	private void SetString(string input, int testPosition)
	{
		foreach (char input2 in input)
		{
			if (!TestEscapeChar(input2, testPosition))
			{
				testPosition = FindEditPositionFrom(testPosition, direction: true);
			}
			SetChar(input2, testPosition);
			testPosition++;
		}
	}

	private bool TestChar(char input, int position, out MaskedTextResultHint resultHint)
	{
		if (!IsPrintableChar(input))
		{
			resultHint = MaskedTextResultHint.InvalidInput;
			return false;
		}
		CharDescriptor charDescriptor = stringDescriptor[position];
		if (IsLiteralPosition(charDescriptor))
		{
			if (SkipLiterals && input == testString[position])
			{
				resultHint = MaskedTextResultHint.CharacterEscaped;
				return true;
			}
			resultHint = MaskedTextResultHint.NonEditPosition;
			return false;
		}
		if (input == promptChar)
		{
			if (ResetOnPrompt)
			{
				if (IsEditPosition(charDescriptor) && charDescriptor.IsAssigned)
				{
					resultHint = MaskedTextResultHint.SideEffect;
				}
				else
				{
					resultHint = MaskedTextResultHint.CharacterEscaped;
				}
				return true;
			}
			if (!AllowPromptAsInput)
			{
				resultHint = MaskedTextResultHint.PromptCharNotAllowed;
				return false;
			}
		}
		if (input == ' ' && ResetOnSpace)
		{
			if (IsEditPosition(charDescriptor) && charDescriptor.IsAssigned)
			{
				resultHint = MaskedTextResultHint.SideEffect;
			}
			else
			{
				resultHint = MaskedTextResultHint.CharacterEscaped;
			}
			return true;
		}
		switch (mask[charDescriptor.MaskPosition])
		{
		case '#':
			if (!char.IsDigit(input) && input != '-' && input != '+' && input != ' ')
			{
				resultHint = MaskedTextResultHint.DigitExpected;
				return false;
			}
			break;
		case '0':
			if (!char.IsDigit(input))
			{
				resultHint = MaskedTextResultHint.DigitExpected;
				return false;
			}
			break;
		case '9':
			if (!char.IsDigit(input) && input != ' ')
			{
				resultHint = MaskedTextResultHint.DigitExpected;
				return false;
			}
			break;
		case 'L':
			if (!char.IsLetter(input))
			{
				resultHint = MaskedTextResultHint.LetterExpected;
				return false;
			}
			if (!IsAsciiLetter(input) && AsciiOnly)
			{
				resultHint = MaskedTextResultHint.AsciiCharacterExpected;
				return false;
			}
			break;
		case '?':
			if (!char.IsLetter(input) && input != ' ')
			{
				resultHint = MaskedTextResultHint.LetterExpected;
				return false;
			}
			if (!IsAsciiLetter(input) && AsciiOnly)
			{
				resultHint = MaskedTextResultHint.AsciiCharacterExpected;
				return false;
			}
			break;
		case '&':
			if (!IsAscii(input) && AsciiOnly)
			{
				resultHint = MaskedTextResultHint.AsciiCharacterExpected;
				return false;
			}
			break;
		case 'C':
			if (!IsAscii(input) && AsciiOnly && input != ' ')
			{
				resultHint = MaskedTextResultHint.AsciiCharacterExpected;
				return false;
			}
			break;
		case 'A':
			if (!IsAlphanumeric(input))
			{
				resultHint = MaskedTextResultHint.AlphanumericCharacterExpected;
				return false;
			}
			if (!IsAciiAlphanumeric(input) && AsciiOnly)
			{
				resultHint = MaskedTextResultHint.AsciiCharacterExpected;
				return false;
			}
			break;
		case 'a':
			if (!IsAlphanumeric(input) && input != ' ')
			{
				resultHint = MaskedTextResultHint.AlphanumericCharacterExpected;
				return false;
			}
			if (!IsAciiAlphanumeric(input) && AsciiOnly)
			{
				resultHint = MaskedTextResultHint.AsciiCharacterExpected;
				return false;
			}
			break;
		}
		if (input == testString[position] && charDescriptor.IsAssigned)
		{
			resultHint = MaskedTextResultHint.NoEffect;
		}
		else
		{
			resultHint = MaskedTextResultHint.Success;
		}
		return true;
	}

	private bool TestEscapeChar(char input, int position)
	{
		CharDescriptor charDex = stringDescriptor[position];
		return TestEscapeChar(input, position, charDex);
	}

	private bool TestEscapeChar(char input, int position, CharDescriptor charDex)
	{
		if (IsLiteralPosition(charDex))
		{
			if (SkipLiterals)
			{
				return input == testString[position];
			}
			return false;
		}
		if ((ResetOnPrompt && input == promptChar) || (ResetOnSpace && input == ' '))
		{
			return true;
		}
		return false;
	}

	private bool TestSetChar(char input, int position, out MaskedTextResultHint resultHint)
	{
		if (TestChar(input, position, out resultHint))
		{
			if (resultHint == MaskedTextResultHint.Success || resultHint == MaskedTextResultHint.SideEffect)
			{
				SetChar(input, position);
			}
			return true;
		}
		return false;
	}

	private bool TestSetString(string input, int position, out int testPosition, out MaskedTextResultHint resultHint)
	{
		if (TestString(input, position, out testPosition, out resultHint))
		{
			SetString(input, position);
			return true;
		}
		return false;
	}

	private bool TestString(string input, int position, out int testPosition, out MaskedTextResultHint resultHint)
	{
		resultHint = MaskedTextResultHint.Unknown;
		testPosition = position;
		if (input.Length == 0)
		{
			return true;
		}
		MaskedTextResultHint resultHint2 = resultHint;
		foreach (char input2 in input)
		{
			if (testPosition >= testString.Length)
			{
				resultHint = MaskedTextResultHint.UnavailableEditPosition;
				return false;
			}
			if (!TestEscapeChar(input2, testPosition))
			{
				testPosition = FindEditPositionFrom(testPosition, direction: true);
				if (testPosition == -1)
				{
					testPosition = testString.Length;
					resultHint = MaskedTextResultHint.UnavailableEditPosition;
					return false;
				}
			}
			if (!TestChar(input2, testPosition, out resultHint2))
			{
				resultHint = resultHint2;
				return false;
			}
			if (resultHint2 > resultHint)
			{
				resultHint = resultHint2;
			}
			testPosition++;
		}
		testPosition--;
		return true;
	}

	/// <summary>Returns the formatted string in a displayable form.</summary>
	/// <returns>The formatted <see cref="T:System.String" /> that includes prompts and mask literals.</returns>
	public string ToDisplayString()
	{
		if (!IsPassword || assignedCharCount == 0)
		{
			return testString.ToString();
		}
		StringBuilder stringBuilder = new StringBuilder(testString.Length);
		for (int i = 0; i < testString.Length; i++)
		{
			CharDescriptor charDescriptor = stringDescriptor[i];
			stringBuilder.Append((IsEditPosition(charDescriptor) && charDescriptor.IsAssigned) ? passwordChar : testString[i]);
		}
		return stringBuilder.ToString();
	}

	/// <summary>Returns the formatted string that includes all the assigned character values.</summary>
	/// <returns>The formatted <see cref="T:System.String" /> that includes all the assigned character values.</returns>
	public override string ToString()
	{
		return ToString(ignorePasswordChar: true, IncludePrompt, IncludeLiterals, 0, testString.Length);
	}

	/// <summary>Returns the formatted string, optionally including password characters.</summary>
	/// <returns>The formatted <see cref="T:System.String" /> that includes literals, prompts, and optionally password characters.</returns>
	/// <param name="ignorePasswordChar">true to return the actual editable characters; otherwise, false to indicate that the <see cref="P:System.ComponentModel.MaskedTextProvider.PasswordChar" /> property is to be honored.</param>
	public string ToString(bool ignorePasswordChar)
	{
		return ToString(ignorePasswordChar, IncludePrompt, IncludeLiterals, 0, testString.Length);
	}

	/// <summary>Returns a substring of the formatted string.</summary>
	/// <returns>If successful, a substring of the formatted <see cref="T:System.String" />, which includes all the assigned character values; otherwise the <see cref="F:System.String.Empty" /> string.</returns>
	/// <param name="startPosition">The zero-based position in the formatted string where the output begins. </param>
	/// <param name="length">The number of characters to return.</param>
	public string ToString(int startPosition, int length)
	{
		return ToString(ignorePasswordChar: true, IncludePrompt, IncludeLiterals, startPosition, length);
	}

	/// <summary>Returns a substring of the formatted string, optionally including password characters.</summary>
	/// <returns>If successful, a substring of the formatted <see cref="T:System.String" />, which includes literals, prompts, and optionally password characters; otherwise the <see cref="F:System.String.Empty" /> string.</returns>
	/// <param name="ignorePasswordChar">true to return the actual editable characters; otherwise, false to indicate that the <see cref="P:System.ComponentModel.MaskedTextProvider.PasswordChar" /> property is to be honored.</param>
	/// <param name="startPosition">The zero-based position in the formatted string where the output begins. </param>
	/// <param name="length">The number of characters to return.</param>
	public string ToString(bool ignorePasswordChar, int startPosition, int length)
	{
		return ToString(ignorePasswordChar, IncludePrompt, IncludeLiterals, startPosition, length);
	}

	/// <summary>Returns the formatted string, optionally including prompt and literal characters.</summary>
	/// <returns>The formatted <see cref="T:System.String" /> that includes all the assigned character values and optionally includes literals and prompts.</returns>
	/// <param name="includePrompt">true to include prompt characters in the return string; otherwise, false.</param>
	/// <param name="includeLiterals">true to include literal characters in the return string; otherwise, false.</param>
	public string ToString(bool includePrompt, bool includeLiterals)
	{
		return ToString(ignorePasswordChar: true, includePrompt, includeLiterals, 0, testString.Length);
	}

	/// <summary>Returns a substring of the formatted string, optionally including prompt and literal characters.</summary>
	/// <returns>If successful, a substring of the formatted <see cref="T:System.String" />, which includes all the assigned character values and optionally includes literals and prompts; otherwise the <see cref="F:System.String.Empty" /> string.</returns>
	/// <param name="includePrompt">true to include prompt characters in the return string; otherwise, false.</param>
	/// <param name="includeLiterals">true to include literal characters in the return string; otherwise, false.</param>
	/// <param name="startPosition">The zero-based position in the formatted string where the output begins. </param>
	/// <param name="length">The number of characters to return.</param>
	public string ToString(bool includePrompt, bool includeLiterals, int startPosition, int length)
	{
		return ToString(ignorePasswordChar: true, includePrompt, includeLiterals, startPosition, length);
	}

	/// <summary>Returns a substring of the formatted string, optionally including prompt, literal, and password characters.</summary>
	/// <returns>If successful, a substring of the formatted <see cref="T:System.String" />, which includes all the assigned character values and optionally includes literals, prompts, and password characters; otherwise the <see cref="F:System.String.Empty" /> string.</returns>
	/// <param name="ignorePasswordChar">true to return the actual editable characters; otherwise, false to indicate that the <see cref="P:System.ComponentModel.MaskedTextProvider.PasswordChar" /> property is to be honored.</param>
	/// <param name="includePrompt">true to include prompt characters in the return string; otherwise, false.</param>
	/// <param name="includeLiterals">true to return literal characters in the return string; otherwise, false.</param>
	/// <param name="startPosition">The zero-based position in the formatted string where the output begins. </param>
	/// <param name="length">The number of characters to return.</param>
	public string ToString(bool ignorePasswordChar, bool includePrompt, bool includeLiterals, int startPosition, int length)
	{
		if (length <= 0)
		{
			return string.Empty;
		}
		if (startPosition < 0)
		{
			startPosition = 0;
		}
		if (startPosition >= testString.Length)
		{
			return string.Empty;
		}
		int num = testString.Length - startPosition;
		if (length > num)
		{
			length = num;
		}
		if ((!IsPassword || ignorePasswordChar) && includePrompt && includeLiterals)
		{
			return testString.ToString(startPosition, length);
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num2 = startPosition + length - 1;
		if (!includePrompt)
		{
			int num3 = (includeLiterals ? FindNonEditPositionInRange(startPosition, num2, direction: false) : InvalidIndex);
			int num4 = FindAssignedEditPositionInRange((num3 == InvalidIndex) ? startPosition : num3, num2, direction: false);
			num2 = ((num4 != InvalidIndex) ? num4 : num3);
			if (num2 == InvalidIndex)
			{
				return string.Empty;
			}
		}
		for (int i = startPosition; i <= num2; i++)
		{
			char value = testString[i];
			CharDescriptor charDescriptor = stringDescriptor[i];
			switch (charDescriptor.CharType)
			{
			case CharType.EditOptional:
			case CharType.EditRequired:
				if (charDescriptor.IsAssigned)
				{
					if (IsPassword && !ignorePasswordChar)
					{
						stringBuilder.Append(passwordChar);
						continue;
					}
				}
				else if (!includePrompt)
				{
					stringBuilder.Append(' ');
					continue;
				}
				break;
			case CharType.Separator:
			case CharType.Literal:
				if (!includeLiterals)
				{
					continue;
				}
				break;
			}
			stringBuilder.Append(value);
		}
		return stringBuilder.ToString();
	}

	/// <summary>Tests whether the specified character could be set successfully at the specified position.</summary>
	/// <returns>true if the specified character is valid for the specified position; otherwise, false.</returns>
	/// <param name="input">The <see cref="T:System.Char" /> value to test.</param>
	/// <param name="position">The position in the mask to test the input character against.</param>
	/// <param name="hint">A <see cref="T:System.ComponentModel.MaskedTextResultHint" /> that succinctly describes the result of the operation. An output parameter.</param>
	public bool VerifyChar(char input, int position, out MaskedTextResultHint hint)
	{
		hint = MaskedTextResultHint.NoEffect;
		if (position < 0 || position >= testString.Length)
		{
			hint = MaskedTextResultHint.PositionOutOfRange;
			return false;
		}
		return TestChar(input, position, out hint);
	}

	/// <summary>Tests whether the specified character would be escaped at the specified position.</summary>
	/// <returns>true if the specified character would be escaped at the specified position; otherwise, false.</returns>
	/// <param name="input">The <see cref="T:System.Char" /> value to test.</param>
	/// <param name="position">The position in the mask to test the input character against.</param>
	public bool VerifyEscapeChar(char input, int position)
	{
		if (position < 0 || position >= testString.Length)
		{
			return false;
		}
		return TestEscapeChar(input, position);
	}

	/// <summary>Tests whether the specified string could be set successfully.</summary>
	/// <returns>true if the specified string represents valid input; otherwise, false.</returns>
	/// <param name="input">The <see cref="T:System.String" /> value to test.</param>
	public bool VerifyString(string input)
	{
		int testPosition;
		MaskedTextResultHint resultHint;
		return VerifyString(input, out testPosition, out resultHint);
	}

	/// <summary>Tests whether the specified string could be set successfully, and then outputs position and descriptive information.</summary>
	/// <returns>true if the specified string represents valid input; otherwise, false. </returns>
	/// <param name="input">The <see cref="T:System.String" /> value to test.</param>
	/// <param name="testPosition">If successful, the zero-based position of the last character actually tested; otherwise, the first position where the test failed. An output parameter.</param>
	/// <param name="resultHint">A <see cref="T:System.ComponentModel.MaskedTextResultHint" /> that succinctly describes the result of the test operation. An output parameter.</param>
	public bool VerifyString(string input, out int testPosition, out MaskedTextResultHint resultHint)
	{
		testPosition = 0;
		if (input == null || input.Length == 0)
		{
			resultHint = MaskedTextResultHint.NoEffect;
			return true;
		}
		return TestString(input, 0, out testPosition, out resultHint);
	}
}
