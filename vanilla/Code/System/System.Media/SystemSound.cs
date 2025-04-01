using System.IO;
using Unity;

namespace System.Media;

/// <summary>Represents a system sound type.</summary>
/// <filterpriority>2</filterpriority>
/// <completionlist cref="T:System.Media.SystemSounds" />
public class SystemSound
{
	private Stream resource;

	internal SystemSound(string tag)
	{
		resource = typeof(SystemSound).Assembly.GetManifestResourceStream(tag + ".wav");
	}

	/// <summary>Plays the system sound type.</summary>
	public void Play()
	{
		new SoundPlayer(resource).Play();
	}

	internal SystemSound()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
