using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using System.Windows.Markup;

[assembly: Dependency("System,", LoadHint.Always)]
[assembly: Dependency("System.Xaml,", LoadHint.Sometimes)]
[assembly: InternalsVisibleTo("DirectWriteForwarder, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: InternalsVisibleTo("PresentationCore, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: InternalsVisibleTo("PresentationFramework, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: InternalsVisibleTo("PresentationUI, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: InternalsVisibleTo("PresentationFramework.Royale, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: InternalsVisibleTo("PresentationFramework.Luna, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: InternalsVisibleTo("PresentationFramework.Aero, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: InternalsVisibleTo("PresentationFramework.Aero2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: InternalsVisibleTo("PresentationFramework.AeroLite, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: InternalsVisibleTo("PresentationFramework.Classic, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: InternalsVisibleTo("ReachFramework, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: InternalsVisibleTo("System.Windows.Presentation, PublicKey=00000000000000000400000000000000")]
[assembly: InternalsVisibleTo("PresentationFramework-SystemCore, PublicKey=00000000000000000400000000000000")]
[assembly: InternalsVisibleTo("PresentationFramework-SystemData, PublicKey=00000000000000000400000000000000")]
[assembly: InternalsVisibleTo("PresentationFramework-SystemDrawing, PublicKey=00000000000000000400000000000000")]
[assembly: InternalsVisibleTo("PresentationFramework-SystemXml, PublicKey=00000000000000000400000000000000")]
[assembly: InternalsVisibleTo("PresentationFramework-SystemXmlLinq, PublicKey=00000000000000000400000000000000")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "System.Windows")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "System.Windows.Input")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "System.Windows.Media")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "System.Diagnostics")]
[assembly: XmlnsPrefix("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "av")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml", "System.Windows.Markup")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml/composite-font", "System.Windows.Media")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/netfx/2007/xaml/presentation", "System.Windows")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/netfx/2007/xaml/presentation", "System.Windows.Input")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/netfx/2007/xaml/presentation", "System.Windows.Media")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/netfx/2007/xaml/presentation", "System.Diagnostics")]
[assembly: XmlnsPrefix("http://schemas.microsoft.com/netfx/2007/xaml/presentation", "wpf")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/netfx/2009/xaml/presentation", "System.Windows")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/netfx/2009/xaml/presentation", "System.Windows.Input")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/netfx/2009/xaml/presentation", "System.Windows.Media")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/netfx/2009/xaml/presentation", "System.Diagnostics")]
[assembly: XmlnsPrefix("http://schemas.microsoft.com/netfx/2009/xaml/presentation", "wpf")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/xps/2005/06", "System.Windows")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/xps/2005/06", "System.Windows.Input")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/xps/2005/06", "System.Windows.Media")]
[assembly: XmlnsPrefix("http://schemas.microsoft.com/xps/2005/06", "metro")]
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyCopyright("© Microsoft Corporation. All rights reserved.")]
[assembly: AssemblyFileVersion("8.0.824.36607")]
[assembly: AssemblyInformationalVersion("8.0.8-servicing.24366.7+883fc207bb50622d4458ff09ae6a62548783826a")]
[assembly: AssemblyProduct("WindowsBase")]
[assembly: AssemblyTitle("WindowsBase")]
[assembly: AssemblyMetadata("RepositoryUrl", "https://github.com/dotnet/wpf")]
[assembly: NeutralResourcesLanguage("en-US")]
[assembly: CLSCompliant(true)]
[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32 | DllImportSearchPath.AssemblyDirectory)]
[assembly: AssemblyDefaultAlias("WindowsBase")]
[assembly: AssemblyMetadata("FileVersion", "8.0.824.36607")]
[assembly: AssemblyMetadata("BuiltBy", "7f7dccf3c000000")]
[assembly: AssemblyMetadata("Repository", "https://github.com/dotnet/wpf")]
[assembly: AssemblyMetadata("Commit", "883fc207bb50622d4458ff09ae6a62548783826a")]
[assembly: AssemblyMetadata("Language", "C#")]
[assembly: AssemblyVersion("8.0.0.0")]
[assembly: TypeForwardedTo(typeof(ObservableCollection<>))]
[assembly: TypeForwardedTo(typeof(ReadOnlyObservableCollection<>))]
[assembly: TypeForwardedTo(typeof(INotifyCollectionChanged))]
[assembly: TypeForwardedTo(typeof(NotifyCollectionChangedAction))]
[assembly: TypeForwardedTo(typeof(NotifyCollectionChangedEventArgs))]
[assembly: TypeForwardedTo(typeof(NotifyCollectionChangedEventHandler))]
[assembly: TypeForwardedTo(typeof(FileFormatException))]
[assembly: TypeForwardedTo(typeof(CompressionOption))]
[assembly: TypeForwardedTo(typeof(EncryptionOption))]
[assembly: TypeForwardedTo(typeof(Package))]
[assembly: TypeForwardedTo(typeof(PackagePart))]
[assembly: TypeForwardedTo(typeof(PackagePartCollection))]
[assembly: TypeForwardedTo(typeof(PackageProperties))]
[assembly: TypeForwardedTo(typeof(PackageRelationship))]
[assembly: TypeForwardedTo(typeof(PackageRelationshipCollection))]
[assembly: TypeForwardedTo(typeof(PackageRelationshipSelector))]
[assembly: TypeForwardedTo(typeof(PackageRelationshipSelectorType))]
[assembly: TypeForwardedTo(typeof(PackUriHelper))]
[assembly: TypeForwardedTo(typeof(TargetMode))]
[assembly: TypeForwardedTo(typeof(ZipPackage))]
[assembly: TypeForwardedTo(typeof(ZipPackagePart))]
[assembly: TypeForwardedTo(typeof(MediaPermission))]
[assembly: TypeForwardedTo(typeof(MediaPermissionAttribute))]
[assembly: TypeForwardedTo(typeof(MediaPermissionAudio))]
[assembly: TypeForwardedTo(typeof(MediaPermissionImage))]
[assembly: TypeForwardedTo(typeof(MediaPermissionVideo))]
[assembly: TypeForwardedTo(typeof(WebBrowserPermission))]
[assembly: TypeForwardedTo(typeof(WebBrowserPermissionAttribute))]
[assembly: TypeForwardedTo(typeof(WebBrowserPermissionLevel))]
[assembly: TypeForwardedTo(typeof(AmbientAttribute))]
[assembly: TypeForwardedTo(typeof(ArrayExtension))]
[assembly: TypeForwardedTo(typeof(ConstructorArgumentAttribute))]
[assembly: TypeForwardedTo(typeof(ContentPropertyAttribute))]
[assembly: TypeForwardedTo(typeof(ContentWrapperAttribute))]
[assembly: TypeForwardedTo(typeof(DateTimeValueSerializer))]
[assembly: TypeForwardedTo(typeof(DependsOnAttribute))]
[assembly: TypeForwardedTo(typeof(DictionaryKeyPropertyAttribute))]
[assembly: TypeForwardedTo(typeof(IComponentConnector))]
[assembly: TypeForwardedTo(typeof(INameScope))]
[assembly: TypeForwardedTo(typeof(IProvideValueTarget))]
[assembly: TypeForwardedTo(typeof(IUriContext))]
[assembly: TypeForwardedTo(typeof(IValueSerializerContext))]
[assembly: TypeForwardedTo(typeof(IXamlTypeResolver))]
[assembly: TypeForwardedTo(typeof(MarkupExtension))]
[assembly: TypeForwardedTo(typeof(MarkupExtensionReturnTypeAttribute))]
[assembly: TypeForwardedTo(typeof(NameScopePropertyAttribute))]
[assembly: TypeForwardedTo(typeof(NullExtension))]
[assembly: TypeForwardedTo(typeof(RootNamespaceAttribute))]
[assembly: TypeForwardedTo(typeof(RuntimeNamePropertyAttribute))]
[assembly: TypeForwardedTo(typeof(StaticExtension))]
[assembly: TypeForwardedTo(typeof(TrimSurroundingWhitespaceAttribute))]
[assembly: TypeForwardedTo(typeof(TypeExtension))]
[assembly: TypeForwardedTo(typeof(UidPropertyAttribute))]
[assembly: TypeForwardedTo(typeof(UsableDuringInitializationAttribute))]
[assembly: TypeForwardedTo(typeof(ValueSerializer))]
[assembly: TypeForwardedTo(typeof(ValueSerializerAttribute))]
[assembly: TypeForwardedTo(typeof(WhitespaceSignificantCollectionAttribute))]
[assembly: TypeForwardedTo(typeof(XmlLangPropertyAttribute))]
[assembly: TypeForwardedTo(typeof(XmlnsCompatibleWithAttribute))]
[assembly: TypeForwardedTo(typeof(XmlnsDefinitionAttribute))]
[assembly: TypeForwardedTo(typeof(XmlnsPrefixAttribute))]
[module: RefSafetyRules(11)]
