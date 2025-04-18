using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace HarmonyLib;

public static class AccessToolsExtensions
{
	public static IEnumerable<Type> InnerTypes(this Type type)
	{
		return AccessTools.InnerTypes(type);
	}

	public static T FindIncludingBaseTypes<T>(this Type type, Func<Type, T> func) where T : class
	{
		return AccessTools.FindIncludingBaseTypes(type, func);
	}

	public static T FindIncludingInnerTypes<T>(this Type type, Func<Type, T> func) where T : class
	{
		return AccessTools.FindIncludingInnerTypes(type, func);
	}

	public static FieldInfo DeclaredField(this Type type, string name)
	{
		return AccessTools.DeclaredField(type, name);
	}

	public static FieldInfo Field(this Type type, string name)
	{
		return AccessTools.Field(type, name);
	}

	public static FieldInfo DeclaredField(this Type type, int idx)
	{
		return AccessTools.DeclaredField(type, idx);
	}

	public static PropertyInfo DeclaredProperty(this Type type, string name)
	{
		return AccessTools.DeclaredProperty(type, name);
	}

	public static PropertyInfo DeclaredIndexer(this Type type, Type[] parameters = null)
	{
		return AccessTools.DeclaredIndexer(type, parameters);
	}

	public static MethodInfo DeclaredPropertyGetter(this Type type, string name)
	{
		return AccessTools.DeclaredPropertyGetter(type, name);
	}

	public static MethodInfo DeclaredIndexerGetter(this Type type, Type[] parameters = null)
	{
		return AccessTools.DeclaredIndexerGetter(type, parameters);
	}

	public static MethodInfo DeclaredPropertySetter(this Type type, string name)
	{
		return AccessTools.DeclaredPropertySetter(type, name);
	}

	public static MethodInfo DeclaredIndexerSetter(this Type type, Type[] parameters)
	{
		return AccessTools.DeclaredIndexerSetter(type, parameters);
	}

	public static PropertyInfo Property(this Type type, string name)
	{
		return AccessTools.Property(type, name);
	}

	public static PropertyInfo Indexer(this Type type, Type[] parameters = null)
	{
		return AccessTools.Indexer(type, parameters);
	}

	public static MethodInfo PropertyGetter(this Type type, string name)
	{
		return AccessTools.PropertyGetter(type, name);
	}

	public static MethodInfo IndexerGetter(this Type type, Type[] parameters = null)
	{
		return AccessTools.IndexerGetter(type, parameters);
	}

	public static MethodInfo PropertySetter(this Type type, string name)
	{
		return AccessTools.PropertySetter(type, name);
	}

	public static MethodInfo IndexerSetter(this Type type, Type[] parameters = null)
	{
		return AccessTools.IndexerSetter(type, parameters);
	}

	public static MethodInfo DeclaredMethod(this Type type, string name, Type[] parameters = null, Type[] generics = null)
	{
		return AccessTools.DeclaredMethod(type, name, parameters, generics);
	}

	public static MethodInfo Method(this Type type, string name, Type[] parameters = null, Type[] generics = null)
	{
		return AccessTools.Method(type, name, parameters, generics);
	}

	public static List<string> GetMethodNames(this Type type)
	{
		return AccessTools.GetMethodNames(type);
	}

	public static List<string> GetFieldNames(this Type type)
	{
		return AccessTools.GetFieldNames(type);
	}

	public static List<string> GetPropertyNames(this Type type)
	{
		return AccessTools.GetPropertyNames(type);
	}

	public static ConstructorInfo DeclaredConstructor(this Type type, Type[] parameters = null, bool searchForStatic = false)
	{
		return AccessTools.DeclaredConstructor(type, parameters, searchForStatic);
	}

	public static ConstructorInfo Constructor(this Type type, Type[] parameters = null, bool searchForStatic = false)
	{
		return AccessTools.Constructor(type, parameters, searchForStatic);
	}

	public static List<ConstructorInfo> GetDeclaredConstructors(this Type type, bool? searchForStatic = null)
	{
		return AccessTools.GetDeclaredConstructors(type, searchForStatic);
	}

	public static List<MethodInfo> GetDeclaredMethods(this Type type)
	{
		return AccessTools.GetDeclaredMethods(type);
	}

	public static List<PropertyInfo> GetDeclaredProperties(this Type type)
	{
		return AccessTools.GetDeclaredProperties(type);
	}

	public static List<FieldInfo> GetDeclaredFields(this Type type)
	{
		return AccessTools.GetDeclaredFields(type);
	}

	public static Type Inner(this Type type, string name)
	{
		return AccessTools.Inner(type, name);
	}

	public static Type FirstInner(this Type type, Func<Type, bool> predicate)
	{
		return AccessTools.FirstInner(type, predicate);
	}

	public static MethodInfo FirstMethod(this Type type, Func<MethodInfo, bool> predicate)
	{
		return AccessTools.FirstMethod(type, predicate);
	}

	public static ConstructorInfo FirstConstructor(this Type type, Func<ConstructorInfo, bool> predicate)
	{
		return AccessTools.FirstConstructor(type, predicate);
	}

	public static PropertyInfo FirstProperty(this Type type, Func<PropertyInfo, bool> predicate)
	{
		return AccessTools.FirstProperty(type, predicate);
	}

	public static AccessTools.FieldRef<object, F> FieldRefAccess<F>(this Type type, string fieldName)
	{
		return AccessTools.FieldRefAccess<F>(type, fieldName);
	}

	public static ref F StaticFieldRefAccess<F>(this Type type, string fieldName)
	{
		return ref AccessTools.StaticFieldRefAccess<F>(type, fieldName);
	}

	public static void ThrowMissingMemberException(this Type type, params string[] names)
	{
		AccessTools.ThrowMissingMemberException(type, names);
	}

	public static object GetDefaultValue(this Type type)
	{
		return AccessTools.GetDefaultValue(type);
	}

	public static object CreateInstance(this Type type)
	{
		return AccessTools.CreateInstance(type);
	}

	public static bool IsStruct(this Type type)
	{
		return AccessTools.IsStruct(type);
	}

	public static bool IsClass(this Type type)
	{
		return AccessTools.IsClass(type);
	}

	public static bool IsValue(this Type type)
	{
		return AccessTools.IsValue(type);
	}

	public static bool IsInteger(this Type type)
	{
		return AccessTools.IsInteger(type);
	}

	public static bool IsFloatingPoint(this Type type)
	{
		return AccessTools.IsFloatingPoint(type);
	}

	public static bool IsNumber(this Type type)
	{
		return AccessTools.IsNumber(type);
	}

	public static bool IsVoid(this Type type)
	{
		return AccessTools.IsVoid(type);
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static bool IsStatic(this Type type)
	{
		return AccessTools.IsStatic(type);
	}
}
