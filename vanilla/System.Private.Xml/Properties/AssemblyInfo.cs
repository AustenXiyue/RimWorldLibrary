using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

[assembly: UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026:RequiresUnreferencedCode", Target = "M:System.Xml.Serialization.ReflectionXmlSerializationReader.#cctor", Scope = "member", Justification = "The reason why this warns is because the two static properties call GetTypeDesc() which internally will call ImportTypeDesc() when the passed in type is not considered a primitive type. That said, for both properties here we are passing in string and XmlQualifiedName which are considered primitive, so they are trim safe.")]
[assembly: AssemblyMetadata("Serviceable", "True")]
[assembly: AssemblyMetadata("PreferInbox", "True")]
[assembly: AssemblyDefaultAlias("System.Private.Xml")]
[assembly: NeutralResourcesLanguage("en-US")]
[assembly: CLSCompliant(true)]
[assembly: AssemblyMetadata("IsTrimmable", "True")]
[assembly: DefaultDllImportSearchPaths(DllImportSearchPath.System32 | DllImportSearchPath.AssemblyDirectory)]
[assembly: AssemblyCompany("Microsoft Corporation")]
[assembly: AssemblyCopyright("© Microsoft Corporation. All rights reserved.")]
[assembly: AssemblyDescription("System.Private.Xml")]
[assembly: AssemblyFileVersion("8.0.824.36612")]
[assembly: AssemblyInformationalVersion("8.0.8+08338fcaa5c9b9a8190abb99222fed12aaba956c")]
[assembly: AssemblyProduct("Microsoft® .NET")]
[assembly: AssemblyTitle("System.Private.Xml")]
[assembly: AssemblyMetadata("RepositoryUrl", "https://github.com/dotnet/runtime")]
[assembly: SupportedOSPlatform("Windows")]
[assembly: AssemblyVersion("8.0.0.0")]
[module: RefSafetyRules(11)]
[module: NullablePublicOnly(false)]
[module: SkipLocalsInit]
