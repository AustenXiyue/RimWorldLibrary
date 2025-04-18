using System;

namespace Steamworks;

public static class SteamInput
{
	public static bool Init(bool bExplicitlyCallRunFrame)
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamInput_Init(CSteamAPIContext.GetSteamInput(), bExplicitlyCallRunFrame);
	}

	public static bool Shutdown()
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamInput_Shutdown(CSteamAPIContext.GetSteamInput());
	}

	public static bool SetInputActionManifestFilePath(string pchInputActionManifestAbsolutePath)
	{
		InteropHelp.TestIfAvailableClient();
		using InteropHelp.UTF8StringHandle pchInputActionManifestAbsolutePath2 = new InteropHelp.UTF8StringHandle(pchInputActionManifestAbsolutePath);
		return NativeMethods.ISteamInput_SetInputActionManifestFilePath(CSteamAPIContext.GetSteamInput(), pchInputActionManifestAbsolutePath2);
	}

	public static void RunFrame(bool bReservedValue = true)
	{
		InteropHelp.TestIfAvailableClient();
		NativeMethods.ISteamInput_RunFrame(CSteamAPIContext.GetSteamInput(), bReservedValue);
	}

	public static bool BWaitForData(bool bWaitForever, uint unTimeout)
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamInput_BWaitForData(CSteamAPIContext.GetSteamInput(), bWaitForever, unTimeout);
	}

	public static bool BNewDataAvailable()
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamInput_BNewDataAvailable(CSteamAPIContext.GetSteamInput());
	}

	public static int GetConnectedControllers(InputHandle_t[] handlesOut)
	{
		InteropHelp.TestIfAvailableClient();
		if (handlesOut != null && handlesOut.Length != 16)
		{
			throw new ArgumentException("handlesOut must be the same size as Constants.STEAM_INPUT_MAX_COUNT!");
		}
		return NativeMethods.ISteamInput_GetConnectedControllers(CSteamAPIContext.GetSteamInput(), handlesOut);
	}

	public static void EnableDeviceCallbacks()
	{
		InteropHelp.TestIfAvailableClient();
		NativeMethods.ISteamInput_EnableDeviceCallbacks(CSteamAPIContext.GetSteamInput());
	}

	public static void EnableActionEventCallbacks(SteamInputActionEventCallbackPointer pCallback)
	{
		InteropHelp.TestIfAvailableClient();
		NativeMethods.ISteamInput_EnableActionEventCallbacks(CSteamAPIContext.GetSteamInput(), pCallback);
	}

	public static InputActionSetHandle_t GetActionSetHandle(string pszActionSetName)
	{
		InteropHelp.TestIfAvailableClient();
		using InteropHelp.UTF8StringHandle pszActionSetName2 = new InteropHelp.UTF8StringHandle(pszActionSetName);
		return (InputActionSetHandle_t)NativeMethods.ISteamInput_GetActionSetHandle(CSteamAPIContext.GetSteamInput(), pszActionSetName2);
	}

	public static void ActivateActionSet(InputHandle_t inputHandle, InputActionSetHandle_t actionSetHandle)
	{
		InteropHelp.TestIfAvailableClient();
		NativeMethods.ISteamInput_ActivateActionSet(CSteamAPIContext.GetSteamInput(), inputHandle, actionSetHandle);
	}

	public static InputActionSetHandle_t GetCurrentActionSet(InputHandle_t inputHandle)
	{
		InteropHelp.TestIfAvailableClient();
		return (InputActionSetHandle_t)NativeMethods.ISteamInput_GetCurrentActionSet(CSteamAPIContext.GetSteamInput(), inputHandle);
	}

	public static void ActivateActionSetLayer(InputHandle_t inputHandle, InputActionSetHandle_t actionSetLayerHandle)
	{
		InteropHelp.TestIfAvailableClient();
		NativeMethods.ISteamInput_ActivateActionSetLayer(CSteamAPIContext.GetSteamInput(), inputHandle, actionSetLayerHandle);
	}

	public static void DeactivateActionSetLayer(InputHandle_t inputHandle, InputActionSetHandle_t actionSetLayerHandle)
	{
		InteropHelp.TestIfAvailableClient();
		NativeMethods.ISteamInput_DeactivateActionSetLayer(CSteamAPIContext.GetSteamInput(), inputHandle, actionSetLayerHandle);
	}

	public static void DeactivateAllActionSetLayers(InputHandle_t inputHandle)
	{
		InteropHelp.TestIfAvailableClient();
		NativeMethods.ISteamInput_DeactivateAllActionSetLayers(CSteamAPIContext.GetSteamInput(), inputHandle);
	}

	public static int GetActiveActionSetLayers(InputHandle_t inputHandle, InputActionSetHandle_t[] handlesOut)
	{
		InteropHelp.TestIfAvailableClient();
		if (handlesOut != null && handlesOut.Length != 16)
		{
			throw new ArgumentException("handlesOut must be the same size as Constants.STEAM_INPUT_MAX_ACTIVE_LAYERS!");
		}
		return NativeMethods.ISteamInput_GetActiveActionSetLayers(CSteamAPIContext.GetSteamInput(), inputHandle, handlesOut);
	}

	public static InputDigitalActionHandle_t GetDigitalActionHandle(string pszActionName)
	{
		InteropHelp.TestIfAvailableClient();
		using InteropHelp.UTF8StringHandle pszActionName2 = new InteropHelp.UTF8StringHandle(pszActionName);
		return (InputDigitalActionHandle_t)NativeMethods.ISteamInput_GetDigitalActionHandle(CSteamAPIContext.GetSteamInput(), pszActionName2);
	}

	public static InputDigitalActionData_t GetDigitalActionData(InputHandle_t inputHandle, InputDigitalActionHandle_t digitalActionHandle)
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamInput_GetDigitalActionData(CSteamAPIContext.GetSteamInput(), inputHandle, digitalActionHandle);
	}

	public static int GetDigitalActionOrigins(InputHandle_t inputHandle, InputActionSetHandle_t actionSetHandle, InputDigitalActionHandle_t digitalActionHandle, EInputActionOrigin[] originsOut)
	{
		InteropHelp.TestIfAvailableClient();
		if (originsOut != null && originsOut.Length != 8)
		{
			throw new ArgumentException("originsOut must be the same size as Constants.STEAM_INPUT_MAX_ORIGINS!");
		}
		return NativeMethods.ISteamInput_GetDigitalActionOrigins(CSteamAPIContext.GetSteamInput(), inputHandle, actionSetHandle, digitalActionHandle, originsOut);
	}

	public static string GetStringForDigitalActionName(InputDigitalActionHandle_t eActionHandle)
	{
		InteropHelp.TestIfAvailableClient();
		return InteropHelp.PtrToStringUTF8(NativeMethods.ISteamInput_GetStringForDigitalActionName(CSteamAPIContext.GetSteamInput(), eActionHandle));
	}

	public static InputAnalogActionHandle_t GetAnalogActionHandle(string pszActionName)
	{
		InteropHelp.TestIfAvailableClient();
		using InteropHelp.UTF8StringHandle pszActionName2 = new InteropHelp.UTF8StringHandle(pszActionName);
		return (InputAnalogActionHandle_t)NativeMethods.ISteamInput_GetAnalogActionHandle(CSteamAPIContext.GetSteamInput(), pszActionName2);
	}

	public static InputAnalogActionData_t GetAnalogActionData(InputHandle_t inputHandle, InputAnalogActionHandle_t analogActionHandle)
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamInput_GetAnalogActionData(CSteamAPIContext.GetSteamInput(), inputHandle, analogActionHandle);
	}

	public static int GetAnalogActionOrigins(InputHandle_t inputHandle, InputActionSetHandle_t actionSetHandle, InputAnalogActionHandle_t analogActionHandle, EInputActionOrigin[] originsOut)
	{
		InteropHelp.TestIfAvailableClient();
		if (originsOut != null && originsOut.Length != 8)
		{
			throw new ArgumentException("originsOut must be the same size as Constants.STEAM_INPUT_MAX_ORIGINS!");
		}
		return NativeMethods.ISteamInput_GetAnalogActionOrigins(CSteamAPIContext.GetSteamInput(), inputHandle, actionSetHandle, analogActionHandle, originsOut);
	}

	public static string GetGlyphPNGForActionOrigin(EInputActionOrigin eOrigin, ESteamInputGlyphSize eSize, uint unFlags)
	{
		InteropHelp.TestIfAvailableClient();
		return InteropHelp.PtrToStringUTF8(NativeMethods.ISteamInput_GetGlyphPNGForActionOrigin(CSteamAPIContext.GetSteamInput(), eOrigin, eSize, unFlags));
	}

	public static string GetGlyphSVGForActionOrigin(EInputActionOrigin eOrigin, uint unFlags)
	{
		InteropHelp.TestIfAvailableClient();
		return InteropHelp.PtrToStringUTF8(NativeMethods.ISteamInput_GetGlyphSVGForActionOrigin(CSteamAPIContext.GetSteamInput(), eOrigin, unFlags));
	}

	public static string GetGlyphForActionOrigin_Legacy(EInputActionOrigin eOrigin)
	{
		InteropHelp.TestIfAvailableClient();
		return InteropHelp.PtrToStringUTF8(NativeMethods.ISteamInput_GetGlyphForActionOrigin_Legacy(CSteamAPIContext.GetSteamInput(), eOrigin));
	}

	public static string GetStringForActionOrigin(EInputActionOrigin eOrigin)
	{
		InteropHelp.TestIfAvailableClient();
		return InteropHelp.PtrToStringUTF8(NativeMethods.ISteamInput_GetStringForActionOrigin(CSteamAPIContext.GetSteamInput(), eOrigin));
	}

	public static string GetStringForAnalogActionName(InputAnalogActionHandle_t eActionHandle)
	{
		InteropHelp.TestIfAvailableClient();
		return InteropHelp.PtrToStringUTF8(NativeMethods.ISteamInput_GetStringForAnalogActionName(CSteamAPIContext.GetSteamInput(), eActionHandle));
	}

	public static void StopAnalogActionMomentum(InputHandle_t inputHandle, InputAnalogActionHandle_t eAction)
	{
		InteropHelp.TestIfAvailableClient();
		NativeMethods.ISteamInput_StopAnalogActionMomentum(CSteamAPIContext.GetSteamInput(), inputHandle, eAction);
	}

	public static InputMotionData_t GetMotionData(InputHandle_t inputHandle)
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamInput_GetMotionData(CSteamAPIContext.GetSteamInput(), inputHandle);
	}

	public static void TriggerVibration(InputHandle_t inputHandle, ushort usLeftSpeed, ushort usRightSpeed)
	{
		InteropHelp.TestIfAvailableClient();
		NativeMethods.ISteamInput_TriggerVibration(CSteamAPIContext.GetSteamInput(), inputHandle, usLeftSpeed, usRightSpeed);
	}

	public static void TriggerVibrationExtended(InputHandle_t inputHandle, ushort usLeftSpeed, ushort usRightSpeed, ushort usLeftTriggerSpeed, ushort usRightTriggerSpeed)
	{
		InteropHelp.TestIfAvailableClient();
		NativeMethods.ISteamInput_TriggerVibrationExtended(CSteamAPIContext.GetSteamInput(), inputHandle, usLeftSpeed, usRightSpeed, usLeftTriggerSpeed, usRightTriggerSpeed);
	}

	public static void TriggerSimpleHapticEvent(InputHandle_t inputHandle, EControllerHapticLocation eHapticLocation, byte nIntensity, char nGainDB, byte nOtherIntensity, char nOtherGainDB)
	{
		InteropHelp.TestIfAvailableClient();
		NativeMethods.ISteamInput_TriggerSimpleHapticEvent(CSteamAPIContext.GetSteamInput(), inputHandle, eHapticLocation, nIntensity, nGainDB, nOtherIntensity, nOtherGainDB);
	}

	public static void SetLEDColor(InputHandle_t inputHandle, byte nColorR, byte nColorG, byte nColorB, uint nFlags)
	{
		InteropHelp.TestIfAvailableClient();
		NativeMethods.ISteamInput_SetLEDColor(CSteamAPIContext.GetSteamInput(), inputHandle, nColorR, nColorG, nColorB, nFlags);
	}

	public static void Legacy_TriggerHapticPulse(InputHandle_t inputHandle, ESteamControllerPad eTargetPad, ushort usDurationMicroSec)
	{
		InteropHelp.TestIfAvailableClient();
		NativeMethods.ISteamInput_Legacy_TriggerHapticPulse(CSteamAPIContext.GetSteamInput(), inputHandle, eTargetPad, usDurationMicroSec);
	}

	public static void Legacy_TriggerRepeatedHapticPulse(InputHandle_t inputHandle, ESteamControllerPad eTargetPad, ushort usDurationMicroSec, ushort usOffMicroSec, ushort unRepeat, uint nFlags)
	{
		InteropHelp.TestIfAvailableClient();
		NativeMethods.ISteamInput_Legacy_TriggerRepeatedHapticPulse(CSteamAPIContext.GetSteamInput(), inputHandle, eTargetPad, usDurationMicroSec, usOffMicroSec, unRepeat, nFlags);
	}

	public static bool ShowBindingPanel(InputHandle_t inputHandle)
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamInput_ShowBindingPanel(CSteamAPIContext.GetSteamInput(), inputHandle);
	}

	public static ESteamInputType GetInputTypeForHandle(InputHandle_t inputHandle)
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamInput_GetInputTypeForHandle(CSteamAPIContext.GetSteamInput(), inputHandle);
	}

	public static InputHandle_t GetControllerForGamepadIndex(int nIndex)
	{
		InteropHelp.TestIfAvailableClient();
		return (InputHandle_t)NativeMethods.ISteamInput_GetControllerForGamepadIndex(CSteamAPIContext.GetSteamInput(), nIndex);
	}

	public static int GetGamepadIndexForController(InputHandle_t ulinputHandle)
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamInput_GetGamepadIndexForController(CSteamAPIContext.GetSteamInput(), ulinputHandle);
	}

	public static string GetStringForXboxOrigin(EXboxOrigin eOrigin)
	{
		InteropHelp.TestIfAvailableClient();
		return InteropHelp.PtrToStringUTF8(NativeMethods.ISteamInput_GetStringForXboxOrigin(CSteamAPIContext.GetSteamInput(), eOrigin));
	}

	public static string GetGlyphForXboxOrigin(EXboxOrigin eOrigin)
	{
		InteropHelp.TestIfAvailableClient();
		return InteropHelp.PtrToStringUTF8(NativeMethods.ISteamInput_GetGlyphForXboxOrigin(CSteamAPIContext.GetSteamInput(), eOrigin));
	}

	public static EInputActionOrigin GetActionOriginFromXboxOrigin(InputHandle_t inputHandle, EXboxOrigin eOrigin)
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamInput_GetActionOriginFromXboxOrigin(CSteamAPIContext.GetSteamInput(), inputHandle, eOrigin);
	}

	public static EInputActionOrigin TranslateActionOrigin(ESteamInputType eDestinationInputType, EInputActionOrigin eSourceOrigin)
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamInput_TranslateActionOrigin(CSteamAPIContext.GetSteamInput(), eDestinationInputType, eSourceOrigin);
	}

	public static bool GetDeviceBindingRevision(InputHandle_t inputHandle, out int pMajor, out int pMinor)
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamInput_GetDeviceBindingRevision(CSteamAPIContext.GetSteamInput(), inputHandle, out pMajor, out pMinor);
	}

	public static uint GetRemotePlaySessionID(InputHandle_t inputHandle)
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamInput_GetRemotePlaySessionID(CSteamAPIContext.GetSteamInput(), inputHandle);
	}

	public static ushort GetSessionInputConfigurationSettings()
	{
		InteropHelp.TestIfAvailableClient();
		return NativeMethods.ISteamInput_GetSessionInputConfigurationSettings(CSteamAPIContext.GetSteamInput());
	}
}
