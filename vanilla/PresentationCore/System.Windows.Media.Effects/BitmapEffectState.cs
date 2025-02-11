namespace System.Windows.Media.Effects;

internal class BitmapEffectState
{
	private BitmapEffect _bitmapEffect;

	private BitmapEffectInput _bitmapEffectInput;

	public BitmapEffect BitmapEffect
	{
		get
		{
			return _bitmapEffect;
		}
		set
		{
			_bitmapEffect = value;
		}
	}

	public BitmapEffectInput BitmapEffectInput
	{
		get
		{
			return _bitmapEffectInput;
		}
		set
		{
			_bitmapEffectInput = value;
		}
	}
}
