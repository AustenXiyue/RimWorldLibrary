using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Runtime.InteropServices;

namespace System.ComponentModel;

/// <summary>Provides functionality required by all components. </summary>
[RootDesignerSerializer("System.ComponentModel.Design.Serialization.RootCodeDomSerializer, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.ComponentModel.Design.Serialization.CodeDomSerializer, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", true)]
[Designer("System.Windows.Forms.Design.ComponentDocumentDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(IRootDesigner))]
[ComVisible(true)]
[Designer("System.ComponentModel.Design.ComponentDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(IDesigner))]
[TypeConverter(typeof(ComponentConverter))]
public interface IComponent : IDisposable
{
	/// <summary>Gets or sets the <see cref="T:System.ComponentModel.ISite" /> associated with the <see cref="T:System.ComponentModel.IComponent" />.</summary>
	/// <returns>The <see cref="T:System.ComponentModel.ISite" /> object associated with the component; or null, if the component does not have a site.</returns>
	ISite Site { get; set; }

	/// <summary>Represents the method that handles the <see cref="E:System.ComponentModel.IComponent.Disposed" /> event of a component.</summary>
	event EventHandler Disposed;
}
