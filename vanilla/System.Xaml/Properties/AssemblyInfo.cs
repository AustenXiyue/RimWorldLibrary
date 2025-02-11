using System;
using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Windows.Markup;
using System.Xaml.Permissions;

[assembly: Dependency("mscorlib,", LoadHint.Always)]
[assembly: Dependency("System,", LoadHint.Always)]
[assembly: Dependency("System.Xml,", LoadHint.Sometimes)]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml", "System.Windows.Markup")]
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyConfiguration("Release")]
[assembly: AssemblyCopyright("© Microsoft Corporation. All rights reserved.")]
[assembly: AssemblyFileVersion("8.0.824.36607")]
[assembly: AssemblyInformationalVersion("8.0.8-servicing.24366.7+883fc207bb50622d4458ff09ae6a62548783826a")]
[assembly: AssemblyProduct("System.Xaml")]
[assembly: AssemblyTitle("System.Xaml")]
[assembly: AssemblyMetadata("RepositoryUrl", "https://github.com/dotnet/wpf")]
[assembly: NeutralResourcesLanguage("en-US")]
[assembly: CLSCompliant(true)]
[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32 | DllImportSearchPath.AssemblyDirectory)]
[assembly: AssemblyDefaultAlias("System.Xaml")]
[assembly: AssemblyMetadata("FileVersion", "8.0.824.36607")]
[assembly: AssemblyMetadata("BuiltBy", "7f7dccf3c000000")]
[assembly: AssemblyMetadata("Repository", "https://github.com/dotnet/wpf")]
[assembly: AssemblyMetadata("Commit", "883fc207bb50622d4458ff09ae6a62548783826a")]
[assembly: AssemblyMetadata("Language", "C#")]
[assembly: AssemblyVersion("8.0.0.0")]
[assembly: TypeForwardedTo(typeof(ValueSerializerAttribute))]
[assembly: TypeForwardedTo(typeof(XamlAccessLevel))]
[assembly: TypeForwardedTo(typeof(XamlLoadPermission))]
[module: RefSafetyRules(11)]
