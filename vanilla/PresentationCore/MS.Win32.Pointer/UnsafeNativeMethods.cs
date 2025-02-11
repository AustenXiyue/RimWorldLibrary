using System;
using System.Runtime.InteropServices;

namespace MS.Win32.Pointer;

internal class UnsafeNativeMethods
{
	[Flags]
	internal enum TOUCH_FLAGS : uint
	{
		TOUCH_FLAG_NONE = 0u
	}

	[Flags]
	internal enum TOUCH_MASK : uint
	{
		TOUCH_MASK_NONE = 0u,
		TOUCH_MASK_CONTACTAREA = 1u,
		TOUCH_MASK_ORIENTATION = 2u,
		TOUCH_MASK_PRESSURE = 4u
	}

	[Flags]
	internal enum PEN_MASK : uint
	{
		PEN_MASK_NONE = 0u,
		PEN_MASK_PRESSURE = 1u,
		PEN_MASK_ROTATION = 2u,
		PEN_MASK_TILT_X = 4u,
		PEN_MASK_TILT_Y = 8u
	}

	[Flags]
	internal enum PEN_FLAGS : uint
	{
		PEN_FLAG_NONE = 0u,
		PEN_FLAG_BARREL = 1u,
		PEN_FLAG_INVERTED = 2u,
		PEN_FLAG_ERASER = 4u
	}

	internal enum POINTER_DEVICE_CURSOR_TYPE : uint
	{
		POINTER_DEVICE_CURSOR_TYPE_UNKNOWN = 0u,
		POINTER_DEVICE_CURSOR_TYPE_TIP = 1u,
		POINTER_DEVICE_CURSOR_TYPE_ERASER = 2u,
		POINTER_DEVICE_CURSOR_TYPE_MAX = uint.MaxValue
	}

	internal enum POINTER_DEVICE_TYPE : uint
	{
		POINTER_DEVICE_TYPE_INTEGRATED_PEN = 1u,
		POINTER_DEVICE_TYPE_EXTERNAL_PEN = 2u,
		POINTER_DEVICE_TYPE_TOUCH = 3u,
		POINTER_DEVICE_TYPE_TOUCH_PAD = 4u,
		POINTER_DEVICE_TYPE_MAX = uint.MaxValue
	}

	internal enum POINTER_INPUT_TYPE : uint
	{
		PT_POINTER = 1u,
		PT_TOUCH,
		PT_PEN,
		PT_MOUSE,
		PT_TOUCHPAD
	}

	[Flags]
	internal enum POINTER_FLAGS : uint
	{
		POINTER_FLAG_NONE = 0u,
		POINTER_FLAG_NEW = 1u,
		POINTER_FLAG_INRANGE = 2u,
		POINTER_FLAG_INCONTACT = 4u,
		POINTER_FLAG_FIRSTBUTTON = 0x10u,
		POINTER_FLAG_SECONDBUTTON = 0x20u,
		POINTER_FLAG_THIRDBUTTON = 0x40u,
		POINTER_FLAG_FOURTHBUTTON = 0x80u,
		POINTER_FLAG_FIFTHBUTTON = 0x100u,
		POINTER_FLAG_PRIMARY = 0x2000u,
		POINTER_FLAG_CONFIDENCE = 0x4000u,
		POINTER_FLAG_CANCELED = 0x8000u,
		POINTER_FLAG_DOWN = 0x10000u,
		POINTER_FLAG_UPDATE = 0x20000u,
		POINTER_FLAG_UP = 0x40000u,
		POINTER_FLAG_WHEEL = 0x80000u,
		POINTER_FLAG_HWHEEL = 0x100000u,
		POINTER_FLAG_CAPTURECHANGED = 0x200000u,
		POINTER_FLAG_HASTRANSFORM = 0x400000u
	}

	internal enum POINTER_BUTTON_CHANGE_TYPE : uint
	{
		POINTER_CHANGE_NONE,
		POINTER_CHANGE_FIRSTBUTTON_DOWN,
		POINTER_CHANGE_FIRSTBUTTON_UP,
		POINTER_CHANGE_SECONDBUTTON_DOWN,
		POINTER_CHANGE_SECONDBUTTON_UP,
		POINTER_CHANGE_THIRDBUTTON_DOWN,
		POINTER_CHANGE_THIRDBUTTON_UP,
		POINTER_CHANGE_FOURTHBUTTON_DOWN,
		POINTER_CHANGE_FOURTHBUTTON_UP,
		POINTER_CHANGE_FIFTHBUTTON_DOWN,
		POINTER_CHANGE_FIFTHBUTTON_UP
	}

	internal enum InteractionMeasurementUnits : uint
	{
		HiMetric,
		Screen
	}

	internal enum INTERACTION_CONTEXT_PROPERTY : uint
	{
		INTERACTION_CONTEXT_PROPERTY_MEASUREMENT_UNITS = 1u,
		INTERACTION_CONTEXT_PROPERTY_INTERACTION_UI_FEEDBACK = 2u,
		INTERACTION_CONTEXT_PROPERTY_FILTER_POINTERS = 3u,
		INTERACTION_CONTEXT_PROPERTY_MAX = uint.MaxValue
	}

	[Flags]
	internal enum CROSS_SLIDE_FLAGS : uint
	{
		CROSS_SLIDE_FLAGS_NONE = 0u,
		CROSS_SLIDE_FLAGS_SELECT = 1u,
		CROSS_SLIDE_FLAGS_SPEED_BUMP = 2u,
		CROSS_SLIDE_FLAGS_REARRANGE = 4u,
		CROSS_SLIDE_FLAGS_MAX = uint.MaxValue
	}

	internal enum MANIPULATION_RAILS_STATE : uint
	{
		MANIPULATION_RAILS_STATE_UNDECIDED = 0u,
		MANIPULATION_RAILS_STATE_FREE = 1u,
		MANIPULATION_RAILS_STATE_RAILED = 2u,
		MANIPULATION_RAILS_STATE_MAX = uint.MaxValue
	}

	[Flags]
	internal enum INTERACTION_FLAGS : uint
	{
		INTERACTION_FLAG_NONE = 0u,
		INTERACTION_FLAG_BEGIN = 1u,
		INTERACTION_FLAG_END = 2u,
		INTERACTION_FLAG_CANCEL = 4u,
		INTERACTION_FLAG_INERTIA = 8u,
		INTERACTION_FLAG_MAX = uint.MaxValue
	}

	internal enum INTERACTION_ID : uint
	{
		INTERACTION_ID_NONE = 0u,
		INTERACTION_ID_MANIPULATION = 1u,
		INTERACTION_ID_TAP = 2u,
		INTERACTION_ID_SECONDARY_TAP = 3u,
		INTERACTION_ID_HOLD = 4u,
		INTERACTION_ID_DRAG = 5u,
		INTERACTION_ID_CROSS_SLIDE = 6u,
		INTERACTION_ID_MAX = uint.MaxValue
	}

	[Flags]
	internal enum INTERACTION_CONFIGURATION_FLAGS : uint
	{
		INTERACTION_CONFIGURATION_FLAG_NONE = 0u,
		INTERACTION_CONFIGURATION_FLAG_MANIPULATION = 1u,
		INTERACTION_CONFIGURATION_FLAG_MANIPULATION_TRANSLATION_X = 2u,
		INTERACTION_CONFIGURATION_FLAG_MANIPULATION_TRANSLATION_Y = 4u,
		INTERACTION_CONFIGURATION_FLAG_MANIPULATION_ROTATION = 8u,
		INTERACTION_CONFIGURATION_FLAG_MANIPULATION_SCALING = 0x10u,
		INTERACTION_CONFIGURATION_FLAG_MANIPULATION_TRANSLATION_INERTIA = 0x20u,
		INTERACTION_CONFIGURATION_FLAG_MANIPULATION_ROTATION_INERTIA = 0x40u,
		INTERACTION_CONFIGURATION_FLAG_MANIPULATION_SCALING_INERTIA = 0x80u,
		INTERACTION_CONFIGURATION_FLAG_MANIPULATION_RAILS_X = 0x100u,
		INTERACTION_CONFIGURATION_FLAG_MANIPULATION_RAILS_Y = 0x200u,
		INTERACTION_CONFIGURATION_FLAG_MANIPULATION_EXACT = 0x400u,
		INTERACTION_CONFIGURATION_FLAG_CROSS_SLIDE = 1u,
		INTERACTION_CONFIGURATION_FLAG_CROSS_SLIDE_HORIZONTAL = 2u,
		INTERACTION_CONFIGURATION_FLAG_CROSS_SLIDE_SELECT = 4u,
		INTERACTION_CONFIGURATION_FLAG_CROSS_SLIDE_SPEED_BUMP = 8u,
		INTERACTION_CONFIGURATION_FLAG_CROSS_SLIDE_REARRANGE = 0x10u,
		INTERACTION_CONFIGURATION_FLAG_CROSS_SLIDE_EXACT = 0x20u,
		INTERACTION_CONFIGURATION_FLAG_TAP = 1u,
		INTERACTION_CONFIGURATION_FLAG_TAP_DOUBLE = 2u,
		INTERACTION_CONFIGURATION_FLAG_SECONDARY_TAP = 1u,
		INTERACTION_CONFIGURATION_FLAG_HOLD = 1u,
		INTERACTION_CONFIGURATION_FLAG_HOLD_MOUSE = 2u,
		INTERACTION_CONFIGURATION_FLAG_DRAG = 1u,
		INTERACTION_CONFIGURATION_FLAG_MAX = uint.MaxValue
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct POINTER_TOUCH_INFO
	{
		internal POINTER_INFO pointerInfo;

		internal TOUCH_FLAGS touchFlags;

		internal TOUCH_MASK touchMask;

		internal RECT rcContact;

		internal RECT rcContactRaw;

		internal uint orientation;

		internal uint pressure;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct POINTER_DEVICE_PROPERTY
	{
		internal int logicalMin;

		internal int logicalMax;

		internal int physicalMin;

		internal int physicalMax;

		internal uint unit;

		internal uint unitExponent;

		internal ushort usagePageId;

		internal ushort usageId;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct POINTER_DEVICE_CURSOR_INFO
	{
		internal uint cursorId;

		internal POINTER_DEVICE_CURSOR_TYPE cursor;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct POINTER_DEVICE_INFO
	{
		internal uint displayOrientation;

		internal nint device;

		internal POINTER_DEVICE_TYPE pointerDeviceType;

		internal nint monitor;

		internal uint startingCursorId;

		internal ushort maxActiveContacts;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 520)]
		internal string productString;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct POINT
	{
		internal int X;

		internal int Y;

		public override string ToString()
		{
			return $"X: {X}, Y: {Y}";
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct POINTER_PEN_INFO
	{
		internal POINTER_INFO pointerInfo;

		internal PEN_FLAGS penFlags;

		internal PEN_MASK penMask;

		internal uint pressure;

		internal uint rotation;

		internal int tiltX;

		internal int tiltY;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct POINTER_INFO
	{
		internal POINTER_INPUT_TYPE pointerType;

		internal uint pointerId;

		internal uint frameId;

		internal POINTER_FLAGS pointerFlags;

		internal nint sourceDevice;

		internal nint hwndTarget;

		internal POINT ptPixelLocation;

		internal POINT ptHimetricLocation;

		internal POINT ptPixelLocationRaw;

		internal POINT ptHimetricLocationRaw;

		internal uint dwTime;

		internal uint historyCount;

		internal int inputData;

		internal uint dwKeyStates;

		internal ulong PerformanceCount;

		internal POINTER_BUTTON_CHANGE_TYPE ButtonChangeType;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct RECT
	{
		internal int left;

		internal int top;

		internal int right;

		internal int bottom;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct MANIPULATION_VELOCITY
	{
		internal float velocityX;

		internal float velocityY;

		internal float velocityExpansion;

		internal float velocityAngular;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct MANIPULATION_TRANSFORM
	{
		internal float translationX;

		internal float translationY;

		internal float scale;

		internal float expansion;

		internal float rotation;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct INTERACTION_ARGUMENTS_MANIPULATION
	{
		internal MANIPULATION_TRANSFORM delta;

		internal MANIPULATION_TRANSFORM cumulative;

		internal MANIPULATION_VELOCITY velocity;

		internal MANIPULATION_RAILS_STATE railsState;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct INTERACTION_CONTEXT_CONFIGURATION
	{
		internal INTERACTION_ID interactionId;

		internal INTERACTION_CONFIGURATION_FLAGS enable;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct INTERACTION_ARGUMENTS_TAP
	{
		internal uint count;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct INTERACTION_ARGUMENTS_CROSS_SLIDE
	{
		internal CROSS_SLIDE_FLAGS flags;
	}

	[StructLayout(LayoutKind.Explicit)]
	internal struct INTERACTION_CONTEXT_OUTPUT_UNION
	{
		[FieldOffset(0)]
		internal INTERACTION_ARGUMENTS_MANIPULATION manipulation;

		[FieldOffset(0)]
		internal INTERACTION_ARGUMENTS_TAP tap;

		[FieldOffset(0)]
		internal INTERACTION_ARGUMENTS_CROSS_SLIDE crossSlide;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	internal struct INTERACTION_CONTEXT_OUTPUT
	{
		internal INTERACTION_ID interactionId;

		internal INTERACTION_FLAGS interactionFlags;

		internal POINTER_INPUT_TYPE inputType;

		internal float x;

		internal float y;

		internal INTERACTION_CONTEXT_OUTPUT_UNION arguments;
	}

	internal delegate void INTERACTION_CONTEXT_OUTPUT_CALLBACK(nint clientData, ref INTERACTION_CONTEXT_OUTPUT output);

	internal const int POINTER_DEVICE_PRODUCT_STRING_MAX = 520;

	[DllImport("user32.dll", SetLastError = true)]
	internal static extern bool GetPointerDevices([In][Out] ref uint deviceCount, [In][Out] POINTER_DEVICE_INFO[] devices);

	[DllImport("user32.dll", SetLastError = true)]
	internal static extern bool GetPointerDeviceCursors([In] nint device, [In][Out] ref uint cursorCount, [In][Out] POINTER_DEVICE_CURSOR_INFO[] cursors);

	[DllImport("user32.dll", SetLastError = true)]
	internal static extern bool GetPointerInfo([In] uint pointerId, [In][Out] ref POINTER_INFO pointerInfo);

	[DllImport("user32.dll", SetLastError = true)]
	internal static extern bool GetPointerInfoHistory([In] uint pointerId, [In][Out] ref uint entriesCount, [In][Out] POINTER_INFO[] pointerInfo);

	[DllImport("user32.dll", SetLastError = true)]
	internal static extern bool GetPointerDeviceProperties([In] nint device, [In][Out] ref uint propertyCount, [In][Out] POINTER_DEVICE_PROPERTY[] pointerProperties);

	[DllImport("user32.dll", SetLastError = true)]
	internal static extern bool GetPointerDeviceRects([In] nint device, [In][Out] ref RECT pointerDeviceRect, [In][Out] ref RECT displayRect);

	[DllImport("user32.dll", SetLastError = true)]
	internal static extern bool GetPointerCursorId([In] uint pointerId, [In][Out] ref uint cursorId);

	[DllImport("user32.dll", SetLastError = true)]
	internal static extern bool GetPointerPenInfo([In] uint pointerId, [In][Out] ref POINTER_PEN_INFO penInfo);

	[DllImport("user32.dll", SetLastError = true)]
	internal static extern bool GetPointerTouchInfo([In] uint pointerId, [In][Out] ref POINTER_TOUCH_INFO touchInfo);

	[DllImport("user32.dll", SetLastError = true)]
	internal static extern bool GetRawPointerDeviceData([In] uint pointerId, [In] uint historyCount, [In] uint propertiesCount, [In] POINTER_DEVICE_PROPERTY[] pProperties, [In][Out] int[] pValues);

	[DllImport("ninput.dll", SetLastError = true)]
	internal static extern void CreateInteractionContext(out nint interactionContext);

	[DllImport("ninput.dll", SetLastError = true)]
	internal static extern void DestroyInteractionContext([In] nint interactionContext);

	[DllImport("ninput.dll", SetLastError = true)]
	internal static extern void SetInteractionConfigurationInteractionContext([In] nint interactionContext, [In] uint configurationCount, [In] INTERACTION_CONTEXT_CONFIGURATION[] configuration);

	[DllImport("ninput.dll", PreserveSig = false, SetLastError = true)]
	internal static extern void RegisterOutputCallbackInteractionContext([In] nint interactionContext, [In] INTERACTION_CONTEXT_OUTPUT_CALLBACK outputCallback, [Optional][In] nint clientData);

	[DllImport("ninput.dll", SetLastError = true)]
	internal static extern void SetPropertyInteractionContext([In] nint interactionContext, [In] INTERACTION_CONTEXT_PROPERTY contextProperty, [In] uint value);

	[DllImport("ninput.dll", PreserveSig = false, SetLastError = true)]
	internal static extern void BufferPointerPacketsInteractionContext([In] nint interactionContext, [In] uint entriesCount, [In] POINTER_INFO[] pointerInfo);

	[DllImport("ninput.dll", PreserveSig = false, SetLastError = true)]
	internal static extern void ProcessBufferedPacketsInteractionContext([In] nint interactionContext);
}
