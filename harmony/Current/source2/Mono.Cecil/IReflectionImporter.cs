using System;
using System.Reflection;

namespace Mono.Cecil;

internal interface IReflectionImporter
{
	AssemblyNameReference ImportReference(AssemblyName reference);

	TypeReference ImportReference(Type type, IGenericParameterProvider context);

	FieldReference ImportReference(FieldInfo field, IGenericParameterProvider context);

	MethodReference ImportReference(MethodBase method, IGenericParameterProvider context);
}
