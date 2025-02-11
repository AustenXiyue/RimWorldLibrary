using System.ComponentModel;
using System.IO;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using MS.Internal;

namespace System.Windows.Baml2006;

internal class DeferredBinaryDeserializerExtension : MarkupExtension
{
	private class DeferredBinaryDeserializerExtensionContext : ITypeDescriptorContext, IServiceProvider, IFreezeFreezables
	{
		private IServiceProvider _serviceProvider;

		private IFreezeFreezables _freezer;

		private bool _canFreeze;

		IContainer ITypeDescriptorContext.Container => null;

		object ITypeDescriptorContext.Instance => null;

		PropertyDescriptor ITypeDescriptorContext.PropertyDescriptor => null;

		bool IFreezeFreezables.FreezeFreezables => _canFreeze;

		public DeferredBinaryDeserializerExtensionContext(IServiceProvider serviceProvider, IFreezeFreezables freezer, bool canFreeze)
		{
			_freezer = freezer;
			_canFreeze = canFreeze;
			_serviceProvider = serviceProvider;
		}

		object IServiceProvider.GetService(Type serviceType)
		{
			if (serviceType == typeof(IFreezeFreezables))
			{
				return this;
			}
			return _serviceProvider.GetService(serviceType);
		}

		void ITypeDescriptorContext.OnComponentChanged()
		{
		}

		bool ITypeDescriptorContext.OnComponentChanging()
		{
			return false;
		}

		bool IFreezeFreezables.TryFreeze(string value, Freezable freezable)
		{
			return _freezer.TryFreeze(value, freezable);
		}

		Freezable IFreezeFreezables.TryGetFreezable(string value)
		{
			return _freezer.TryGetFreezable(value);
		}
	}

	private IFreezeFreezables _freezer;

	private bool _canFreeze;

	private readonly BinaryReader _reader;

	private readonly Stream _stream;

	private readonly int _converterId;

	public DeferredBinaryDeserializerExtension(IFreezeFreezables freezer, BinaryReader reader, int converterId, int dataByteSize)
	{
		_freezer = freezer;
		_canFreeze = freezer.FreezeFreezables;
		byte[] buffer = reader.ReadBytes(dataByteSize);
		_stream = new MemoryStream(buffer);
		_reader = new BinaryReader(_stream);
		_converterId = converterId;
	}

	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		_stream.Position = 0L;
		return _converterId switch
		{
			744 => SolidColorBrush.DeserializeFrom(_reader, new DeferredBinaryDeserializerExtensionContext(serviceProvider, _freezer, _canFreeze)), 
			746 => Parsers.DeserializeStreamGeometry(_reader), 
			747 => Point3DCollection.DeserializeFrom(_reader), 
			748 => PointCollection.DeserializeFrom(_reader), 
			752 => Vector3DCollection.DeserializeFrom(_reader), 
			_ => throw new NotImplementedException(), 
		};
	}
}
