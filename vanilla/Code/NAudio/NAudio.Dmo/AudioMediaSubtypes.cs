using System;

namespace NAudio.Dmo;

internal class AudioMediaSubtypes
{
	public static readonly Guid MEDIASUBTYPE_PCM = new Guid("00000001-0000-0010-8000-00AA00389B71");

	public static readonly Guid MEDIASUBTYPE_PCMAudioObsolete = new Guid("e436eb8a-524f-11ce-9f53-0020af0ba770");

	public static readonly Guid MEDIASUBTYPE_MPEG1Packet = new Guid("e436eb80-524f-11ce-9f53-0020af0ba770");

	public static readonly Guid MEDIASUBTYPE_MPEG1Payload = new Guid("e436eb81-524f-11ce-9f53-0020af0ba770");

	public static readonly Guid MEDIASUBTYPE_MPEG2_AUDIO = new Guid("e06d802b-db46-11cf-b4d1-00805f6cbbea");

	public static readonly Guid MEDIASUBTYPE_DVD_LPCM_AUDIO = new Guid("e06d8032-db46-11cf-b4d1-00805f6cbbea");

	public static readonly Guid MEDIASUBTYPE_DRM_Audio = new Guid("00000009-0000-0010-8000-00aa00389b71");

	public static readonly Guid MEDIASUBTYPE_IEEE_FLOAT = new Guid("00000003-0000-0010-8000-00aa00389b71");

	public static readonly Guid MEDIASUBTYPE_DOLBY_AC3 = new Guid("e06d802c-db46-11cf-b4d1-00805f6cbbea");

	public static readonly Guid MEDIASUBTYPE_DOLBY_AC3_SPDIF = new Guid("00000092-0000-0010-8000-00aa00389b71");

	public static readonly Guid MEDIASUBTYPE_RAW_SPORT = new Guid("00000240-0000-0010-8000-00aa00389b71");

	public static readonly Guid MEDIASUBTYPE_SPDIF_TAG_241h = new Guid("00000241-0000-0010-8000-00aa00389b71");

	public static readonly Guid WMMEDIASUBTYPE_MP3 = new Guid("00000055-0000-0010-8000-00AA00389B71");

	public static readonly Guid MEDIASUBTYPE_WAVE = new Guid("e436eb8b-524f-11ce-9f53-0020af0ba770");

	public static readonly Guid MEDIASUBTYPE_AU = new Guid("e436eb8c-524f-11ce-9f53-0020af0ba770");

	public static readonly Guid MEDIASUBTYPE_AIFF = new Guid("e436eb8d-524f-11ce-9f53-0020af0ba770");

	public static readonly Guid[] AudioSubTypes = new Guid[13]
	{
		MEDIASUBTYPE_PCM, MEDIASUBTYPE_PCMAudioObsolete, MEDIASUBTYPE_MPEG1Packet, MEDIASUBTYPE_MPEG1Payload, MEDIASUBTYPE_MPEG2_AUDIO, MEDIASUBTYPE_DVD_LPCM_AUDIO, MEDIASUBTYPE_DRM_Audio, MEDIASUBTYPE_IEEE_FLOAT, MEDIASUBTYPE_DOLBY_AC3, MEDIASUBTYPE_DOLBY_AC3_SPDIF,
		MEDIASUBTYPE_RAW_SPORT, MEDIASUBTYPE_SPDIF_TAG_241h, WMMEDIASUBTYPE_MP3
	};

	public static readonly string[] AudioSubTypeNames = new string[13]
	{
		"PCM", "PCM Obsolete", "MPEG1Packet", "MPEG1Payload", "MPEG2_AUDIO", "DVD_LPCM_AUDIO", "DRM_Audio", "IEEE_FLOAT", "DOLBY_AC3", "DOLBY_AC3_SPDIF",
		"RAW_SPORT", "SPDIF_TAG_241h", "MP3"
	};

	public static string GetAudioSubtypeName(Guid subType)
	{
		for (int i = 0; i < AudioSubTypes.Length; i++)
		{
			if (subType == AudioSubTypes[i])
			{
				return AudioSubTypeNames[i];
			}
		}
		return subType.ToString();
	}
}
