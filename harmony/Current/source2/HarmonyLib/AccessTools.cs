using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;
using System.Threading;
using MonoMod.Core.Platforms;
using MonoMod.Utils;

namespace HarmonyLib;

public static class AccessTools
{
	public delegate ref F FieldRef<in T, F>(T instance = default(T));

	public delegate ref F StructFieldRef<T, F>(ref T instance) where T : struct;

	public delegate ref F FieldRef<F>();

	public static readonly BindingFlags all = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty;

	public static readonly BindingFlags allDeclared = all | BindingFlags.DeclaredOnly;

	private static readonly Dictionary<Type, FastInvokeHandler> addHandlerCache = new Dictionary<Type, FastInvokeHandler>();

	private static readonly ReaderWriterLockSlim addHandlerCacheLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

	public static bool IsMonoRuntime { get; } = (object)Type.GetType("Mono.Runtime") != null;

	public static bool IsNetFrameworkRuntime { get; } = Type.GetType("System.Runtime.InteropServices.RuntimeInformation", throwOnError: false)?.GetProperty("FrameworkDescription").GetValue(null, null).ToString()
		.StartsWith(".NET Framework") ?? (!IsMonoRuntime);

	public static bool IsNetCoreRuntime { get; } = Type.GetType("System.Runtime.InteropServices.RuntimeInformation", throwOnError: false)?.GetProperty("FrameworkDescription").GetValue(null, null).ToString()
		.StartsWith(".NET Core") ?? false;

	public static IEnumerable<Assembly> AllAssemblies()
	{
		return from a in AppDomain.CurrentDomain.GetAssemblies()
			where !a.FullName.StartsWith("Microsoft.VisualStudio")
			select a;
	}

	public static Type TypeByName(string name)
	{
		Type type = Type.GetType(name, throwOnError: false);
		if ((object)type == null)
		{
			type = AllTypes().FirstOrDefault((Type t) => t.FullName == name);
		}
		if ((object)type == null)
		{
			type = AllTypes().FirstOrDefault((Type t) => t.Name == name);
		}
		if ((object)type == null)
		{
			FileLog.Debug("AccessTools.TypeByName: Could not find type named " + name);
		}
		return type;
	}

	public static Type[] GetTypesFromAssembly(Assembly assembly)
	{
		try
		{
			return assembly.GetTypes();
		}
		catch (ReflectionTypeLoadException ex)
		{
			FileLog.Debug($"AccessTools.GetTypesFromAssembly: assembly {assembly} => {ex}");
			return ex.Types.Where((Type type) => (object)type != null).ToArray();
		}
	}

	public static IEnumerable<Type> AllTypes()
	{
		return AllAssemblies().SelectMany(GetTypesFromAssembly);
	}

	public static IEnumerable<Type> InnerTypes(Type type)
	{
		return type.GetNestedTypes(all);
	}

	public static T FindIncludingBaseTypes<T>(Type type, Func<Type, T> func) where T : class
	{
		do
		{
			T val = func(type);
			if (val != null)
			{
				return val;
			}
			type = type.BaseType;
		}
		while ((object)type != null);
		return null;
	}

	public static T FindIncludingInnerTypes<T>(Type type, Func<Type, T> func) where T : class
	{
		T val = func(type);
		if (val != null)
		{
			return val;
		}
		Type[] nestedTypes = type.GetNestedTypes(all);
		foreach (Type type2 in nestedTypes)
		{
			val = FindIncludingInnerTypes(type2, func);
			if (val != null)
			{
				break;
			}
		}
		return val;
	}

	public static MethodInfo Identifiable(this MethodInfo method)
	{
		return (PlatformTriple.Current.GetIdentifiable(method) as MethodInfo) ?? method;
	}

	public static FieldInfo DeclaredField(Type type, string name)
	{
		if ((object)type == null)
		{
			FileLog.Debug("AccessTools.DeclaredField: type is null");
			return null;
		}
		if (name == null)
		{
			FileLog.Debug("AccessTools.DeclaredField: name is null");
			return null;
		}
		FieldInfo field = type.GetField(name, allDeclared);
		if ((object)field == null)
		{
			FileLog.Debug($"AccessTools.DeclaredField: Could not find field for type {type} and name {name}");
		}
		return field;
	}

	public static FieldInfo DeclaredField(string typeColonName)
	{
		Tools.TypeAndName typeAndName = Tools.TypColonName(typeColonName);
		FieldInfo field = typeAndName.type.GetField(typeAndName.name, allDeclared);
		if ((object)field == null)
		{
			FileLog.Debug($"AccessTools.DeclaredField: Could not find field for type {typeAndName.type} and name {typeAndName.name}");
		}
		return field;
	}

	public static FieldInfo Field(Type type, string name)
	{
		if ((object)type == null)
		{
			FileLog.Debug("AccessTools.Field: type is null");
			return null;
		}
		if (name == null)
		{
			FileLog.Debug("AccessTools.Field: name is null");
			return null;
		}
		FieldInfo fieldInfo = FindIncludingBaseTypes(type, (Type t) => t.GetField(name, all));
		if ((object)fieldInfo == null)
		{
			FileLog.Debug($"AccessTools.Field: Could not find field for type {type} and name {name}");
		}
		return fieldInfo;
	}

	public static FieldInfo Field(string typeColonName)
	{
		Tools.TypeAndName info = Tools.TypColonName(typeColonName);
		FieldInfo fieldInfo = FindIncludingBaseTypes(info.type, (Type t) => t.GetField(info.name, all));
		if ((object)fieldInfo == null)
		{
			FileLog.Debug($"AccessTools.Field: Could not find field for type {info.type} and name {info.name}");
		}
		return fieldInfo;
	}

	public static FieldInfo DeclaredField(Type type, int idx)
	{
		if ((object)type == null)
		{
			FileLog.Debug("AccessTools.DeclaredField: type is null");
			return null;
		}
		FieldInfo fieldInfo = GetDeclaredFields(type).ElementAtOrDefault(idx);
		if ((object)fieldInfo == null)
		{
			FileLog.Debug($"AccessTools.DeclaredField: Could not find field for type {type} and idx {idx}");
		}
		return fieldInfo;
	}

	public static PropertyInfo DeclaredProperty(Type type, string name)
	{
		if ((object)type == null)
		{
			FileLog.Debug("AccessTools.DeclaredProperty: type is null");
			return null;
		}
		if (name == null)
		{
			FileLog.Debug("AccessTools.DeclaredProperty: name is null");
			return null;
		}
		PropertyInfo property = type.GetProperty(name, allDeclared);
		if ((object)property == null)
		{
			FileLog.Debug($"AccessTools.DeclaredProperty: Could not find property for type {type} and name {name}");
		}
		return property;
	}

	public static PropertyInfo DeclaredProperty(string typeColonName)
	{
		Tools.TypeAndName typeAndName = Tools.TypColonName(typeColonName);
		PropertyInfo property = typeAndName.type.GetProperty(typeAndName.name, allDeclared);
		if ((object)property == null)
		{
			FileLog.Debug($"AccessTools.DeclaredProperty: Could not find property for type {typeAndName.type} and name {typeAndName.name}");
		}
		return property;
	}

	public static PropertyInfo DeclaredIndexer(Type type, Type[] parameters = null)
	{
		if ((object)type == null)
		{
			FileLog.Debug("AccessTools.DeclaredIndexer: type is null");
			return null;
		}
		try
		{
			PropertyInfo propertyInfo = ((parameters == null) ? type.GetProperties(allDeclared).SingleOrDefault((PropertyInfo property) => property.GetIndexParameters().Length != 0) : type.GetProperties(allDeclared).FirstOrDefault((PropertyInfo property) => (from param in property.GetIndexParameters()
				select param.ParameterType).SequenceEqual(parameters)));
			if ((object)propertyInfo == null)
			{
				FileLog.Debug($"AccessTools.DeclaredIndexer: Could not find indexer for type {type} and parameters {parameters?.Description()}");
			}
			return propertyInfo;
		}
		catch (InvalidOperationException inner)
		{
			throw new AmbiguousMatchException("Multiple possible indexers were found.", inner);
		}
	}

	public static MethodInfo DeclaredPropertyGetter(Type type, string name)
	{
		return DeclaredProperty(type, name)?.GetGetMethod(nonPublic: true);
	}

	public static MethodInfo DeclaredPropertyGetter(string typeColonName)
	{
		return DeclaredProperty(typeColonName)?.GetGetMethod(nonPublic: true);
	}

	public static MethodInfo DeclaredIndexerGetter(Type type, Type[] parameters = null)
	{
		return DeclaredIndexer(type, parameters)?.GetGetMethod(nonPublic: true);
	}

	public static MethodInfo DeclaredPropertySetter(Type type, string name)
	{
		return DeclaredProperty(type, name)?.GetSetMethod(nonPublic: true);
	}

	public static MethodInfo DeclaredPropertySetter(string typeColonName)
	{
		return DeclaredProperty(typeColonName)?.GetSetMethod(nonPublic: true);
	}

	public static MethodInfo DeclaredIndexerSetter(Type type, Type[] parameters)
	{
		return DeclaredIndexer(type, parameters)?.GetSetMethod(nonPublic: true);
	}

	public static PropertyInfo Property(Type type, string name)
	{
		if ((object)type == null)
		{
			FileLog.Debug("AccessTools.Property: type is null");
			return null;
		}
		if (name == null)
		{
			FileLog.Debug("AccessTools.Property: name is null");
			return null;
		}
		PropertyInfo propertyInfo = FindIncludingBaseTypes(type, (Type t) => t.GetProperty(name, all));
		if ((object)propertyInfo == null)
		{
			FileLog.Debug($"AccessTools.Property: Could not find property for type {type} and name {name}");
		}
		return propertyInfo;
	}

	public static PropertyInfo Property(string typeColonName)
	{
		Tools.TypeAndName info = Tools.TypColonName(typeColonName);
		PropertyInfo propertyInfo = FindIncludingBaseTypes(info.type, (Type t) => t.GetProperty(info.name, all));
		if ((object)propertyInfo == null)
		{
			FileLog.Debug($"AccessTools.Property: Could not find property for type {info.type} and name {info.name}");
		}
		return propertyInfo;
	}

	public static PropertyInfo Indexer(Type type, Type[] parameters = null)
	{
		if ((object)type == null)
		{
			FileLog.Debug("AccessTools.Indexer: type is null");
			return null;
		}
		Func<Type, PropertyInfo> func = ((parameters == null) ? ((Func<Type, PropertyInfo>)((Type t) => t.GetProperties(all).SingleOrDefault((PropertyInfo property) => property.GetIndexParameters().Length != 0))) : ((Func<Type, PropertyInfo>)((Type t) => t.GetProperties(all).FirstOrDefault((PropertyInfo property) => (from param in property.GetIndexParameters()
			select param.ParameterType).SequenceEqual(parameters)))));
		try
		{
			PropertyInfo propertyInfo = FindIncludingBaseTypes(type, func);
			if ((object)propertyInfo == null)
			{
				FileLog.Debug($"AccessTools.Indexer: Could not find indexer for type {type} and parameters {parameters?.Description()}");
			}
			return propertyInfo;
		}
		catch (InvalidOperationException inner)
		{
			throw new AmbiguousMatchException("Multiple possible indexers were found.", inner);
		}
	}

	public static MethodInfo PropertyGetter(Type type, string name)
	{
		return Property(type, name)?.GetGetMethod(nonPublic: true);
	}

	public static MethodInfo PropertyGetter(string typeColonName)
	{
		return Property(typeColonName)?.GetGetMethod(nonPublic: true);
	}

	public static MethodInfo IndexerGetter(Type type, Type[] parameters = null)
	{
		return Indexer(type, parameters)?.GetGetMethod(nonPublic: true);
	}

	public static MethodInfo PropertySetter(Type type, string name)
	{
		return Property(type, name)?.GetSetMethod(nonPublic: true);
	}

	public static MethodInfo PropertySetter(string typeColonName)
	{
		return Property(typeColonName)?.GetSetMethod(nonPublic: true);
	}

	public static MethodInfo IndexerSetter(Type type, Type[] parameters = null)
	{
		return Indexer(type, parameters)?.GetSetMethod(nonPublic: true);
	}

	public static MethodInfo DeclaredMethod(Type type, string name, Type[] parameters = null, Type[] generics = null)
	{
		if ((object)type == null)
		{
			FileLog.Debug("AccessTools.DeclaredMethod: type is null");
			return null;
		}
		if (name == null)
		{
			FileLog.Debug("AccessTools.DeclaredMethod: name is null");
			return null;
		}
		ParameterModifier[] modifiers = new ParameterModifier[0];
		MethodInfo methodInfo = ((parameters != null) ? type.GetMethod(name, allDeclared, null, parameters, modifiers) : type.GetMethod(name, allDeclared));
		if ((object)methodInfo == null)
		{
			FileLog.Debug($"AccessTools.DeclaredMethod: Could not find method for type {type} and name {name} and parameters {parameters?.Description()}");
			return null;
		}
		if (generics != null)
		{
			methodInfo = methodInfo.MakeGenericMethod(generics);
		}
		return methodInfo;
	}

	public static MethodInfo DeclaredMethod(string typeColonName, Type[] parameters = null, Type[] generics = null)
	{
		Tools.TypeAndName typeAndName = Tools.TypColonName(typeColonName);
		return DeclaredMethod(typeAndName.type, typeAndName.name, parameters, generics);
	}

	public static MethodInfo Method(Type type, string name, Type[] parameters = null, Type[] generics = null)
	{
		if ((object)type == null)
		{
			FileLog.Debug("AccessTools.Method: type is null");
			return null;
		}
		if (name == null)
		{
			FileLog.Debug("AccessTools.Method: name is null");
			return null;
		}
		ParameterModifier[] modifiers = new ParameterModifier[0];
		MethodInfo methodInfo;
		if (parameters == null)
		{
			try
			{
				methodInfo = FindIncludingBaseTypes(type, (Type t) => t.GetMethod(name, all));
			}
			catch (AmbiguousMatchException inner)
			{
				methodInfo = FindIncludingBaseTypes(type, (Type t) => t.GetMethod(name, all, null, Array.Empty<Type>(), modifiers));
				if ((object)methodInfo == null)
				{
					throw new AmbiguousMatchException($"Ambiguous match in Harmony patch for {type}:{name}", inner);
				}
			}
		}
		else
		{
			methodInfo = FindIncludingBaseTypes(type, (Type t) => t.GetMethod(name, all, null, parameters, modifiers));
		}
		if ((object)methodInfo == null)
		{
			FileLog.Debug($"AccessTools.Method: Could not find method for type {type} and name {name} and parameters {parameters?.Description()}");
			return null;
		}
		if (generics != null)
		{
			methodInfo = methodInfo.MakeGenericMethod(generics);
		}
		return methodInfo;
	}

	public static MethodInfo Method(string typeColonName, Type[] parameters = null, Type[] generics = null)
	{
		Tools.TypeAndName typeAndName = Tools.TypColonName(typeColonName);
		return Method(typeAndName.type, typeAndName.name, parameters, generics);
	}

	public static MethodInfo EnumeratorMoveNext(MethodBase method)
	{
		if ((object)method == null)
		{
			FileLog.Debug("AccessTools.EnumeratorMoveNext: method is null");
			return null;
		}
		IEnumerable<KeyValuePair<OpCode, object>> source = from pair in PatchProcessor.ReadMethodBody(method)
			where pair.Key == OpCodes.Newobj
			select pair;
		if (source.Count() != 1)
		{
			FileLog.Debug("AccessTools.EnumeratorMoveNext: " + method.FullDescription() + " contains no Newobj opcode");
			return null;
		}
		ConstructorInfo constructorInfo = source.First().Value as ConstructorInfo;
		if (constructorInfo == null)
		{
			FileLog.Debug("AccessTools.EnumeratorMoveNext: " + method.FullDescription() + " contains no constructor");
			return null;
		}
		Type declaringType = constructorInfo.DeclaringType;
		if (declaringType == null)
		{
			FileLog.Debug("AccessTools.EnumeratorMoveNext: " + method.FullDescription() + " refers to a global type");
			return null;
		}
		return Method(declaringType, "MoveNext");
	}

	public static MethodInfo AsyncMoveNext(MethodBase method)
	{
		if ((object)method == null)
		{
			FileLog.Debug("AccessTools.AsyncMoveNext: method is null");
			return null;
		}
		AsyncStateMachineAttribute customAttribute = method.GetCustomAttribute<AsyncStateMachineAttribute>();
		if (customAttribute == null)
		{
			FileLog.Debug("AccessTools.AsyncMoveNext: Could not find AsyncStateMachine for " + method.FullDescription());
			return null;
		}
		Type stateMachineType = customAttribute.StateMachineType;
		MethodInfo methodInfo = DeclaredMethod(stateMachineType, "MoveNext");
		if ((object)methodInfo == null)
		{
			FileLog.Debug("AccessTools.AsyncMoveNext: Could not find async method body for " + method.FullDescription());
			return null;
		}
		return methodInfo;
	}

	public static List<string> GetMethodNames(Type type)
	{
		if ((object)type == null)
		{
			FileLog.Debug("AccessTools.GetMethodNames: type is null");
			return new List<string>();
		}
		return (from m in GetDeclaredMethods(type)
			select m.Name).ToList();
	}

	public static List<string> GetMethodNames(object instance)
	{
		if (instance == null)
		{
			FileLog.Debug("AccessTools.GetMethodNames: instance is null");
			return new List<string>();
		}
		return GetMethodNames(instance.GetType());
	}

	public static List<string> GetFieldNames(Type type)
	{
		if ((object)type == null)
		{
			FileLog.Debug("AccessTools.GetFieldNames: type is null");
			return new List<string>();
		}
		return (from f in GetDeclaredFields(type)
			select f.Name).ToList();
	}

	public static List<string> GetFieldNames(object instance)
	{
		if (instance == null)
		{
			FileLog.Debug("AccessTools.GetFieldNames: instance is null");
			return new List<string>();
		}
		return GetFieldNames(instance.GetType());
	}

	public static List<string> GetPropertyNames(Type type)
	{
		if ((object)type == null)
		{
			FileLog.Debug("AccessTools.GetPropertyNames: type is null");
			return new List<string>();
		}
		return (from f in GetDeclaredProperties(type)
			select f.Name).ToList();
	}

	public static List<string> GetPropertyNames(object instance)
	{
		if (instance == null)
		{
			FileLog.Debug("AccessTools.GetPropertyNames: instance is null");
			return new List<string>();
		}
		return GetPropertyNames(instance.GetType());
	}

	public static Type GetUnderlyingType(this MemberInfo member)
	{
		return member.MemberType switch
		{
			MemberTypes.Event => ((EventInfo)member).EventHandlerType, 
			MemberTypes.Field => ((FieldInfo)member).FieldType, 
			MemberTypes.Method => ((MethodInfo)member).ReturnType, 
			MemberTypes.Property => ((PropertyInfo)member).PropertyType, 
			_ => throw new ArgumentException("Member must be of type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"), 
		};
	}

	public static bool IsDeclaredMember<T>(this T member) where T : MemberInfo
	{
		return member.DeclaringType == member.ReflectedType;
	}

	public static T GetDeclaredMember<T>(this T member) where T : MemberInfo
	{
		if ((object)member.DeclaringType == null || member.IsDeclaredMember())
		{
			return member;
		}
		int metadataToken = member.MetadataToken;
		MemberInfo[] array = member.DeclaringType?.GetMembers(all) ?? Array.Empty<MemberInfo>();
		MemberInfo[] array2 = array;
		foreach (MemberInfo memberInfo in array2)
		{
			if (memberInfo.MetadataToken == metadataToken)
			{
				return (T)memberInfo;
			}
		}
		return member;
	}

	public static ConstructorInfo DeclaredConstructor(Type type, Type[] parameters = null, bool searchForStatic = false)
	{
		if ((object)type == null)
		{
			FileLog.Debug("AccessTools.DeclaredConstructor: type is null");
			return null;
		}
		if (parameters == null)
		{
			parameters = Array.Empty<Type>();
		}
		BindingFlags bindingAttr = (searchForStatic ? (allDeclared & ~BindingFlags.Instance) : (allDeclared & ~BindingFlags.Static));
		return type.GetConstructor(bindingAttr, null, parameters, Array.Empty<ParameterModifier>());
	}

	public static ConstructorInfo Constructor(Type type, Type[] parameters = null, bool searchForStatic = false)
	{
		if ((object)type == null)
		{
			FileLog.Debug("AccessTools.ConstructorInfo: type is null");
			return null;
		}
		if (parameters == null)
		{
			parameters = Array.Empty<Type>();
		}
		BindingFlags flags = (searchForStatic ? (all & ~BindingFlags.Instance) : (all & ~BindingFlags.Static));
		return FindIncludingBaseTypes(type, (Type t) => t.GetConstructor(flags, null, parameters, Array.Empty<ParameterModifier>()));
	}

	public static List<ConstructorInfo> GetDeclaredConstructors(Type type, bool? searchForStatic = null)
	{
		if ((object)type == null)
		{
			FileLog.Debug("AccessTools.GetDeclaredConstructors: type is null");
			return new List<ConstructorInfo>();
		}
		BindingFlags bindingFlags = allDeclared;
		if (searchForStatic.HasValue)
		{
			bindingFlags = (searchForStatic.Value ? (bindingFlags & ~BindingFlags.Instance) : (bindingFlags & ~BindingFlags.Static));
		}
		return (from method in type.GetConstructors(bindingFlags)
			where method.DeclaringType == type
			select method).ToList();
	}

	public static List<MethodInfo> GetDeclaredMethods(Type type)
	{
		if ((object)type == null)
		{
			FileLog.Debug("AccessTools.GetDeclaredMethods: type is null");
			return new List<MethodInfo>();
		}
		MethodInfo[] methods = type.GetMethods(allDeclared);
		List<MethodInfo> list = new List<MethodInfo>(methods.Length);
		list.AddRange(methods);
		return list;
	}

	public static List<PropertyInfo> GetDeclaredProperties(Type type)
	{
		if ((object)type == null)
		{
			FileLog.Debug("AccessTools.GetDeclaredProperties: type is null");
			return new List<PropertyInfo>();
		}
		PropertyInfo[] properties = type.GetProperties(allDeclared);
		List<PropertyInfo> list = new List<PropertyInfo>(properties.Length);
		list.AddRange(properties);
		return list;
	}

	public static List<FieldInfo> GetDeclaredFields(Type type)
	{
		if ((object)type == null)
		{
			FileLog.Debug("AccessTools.GetDeclaredFields: type is null");
			return new List<FieldInfo>();
		}
		FieldInfo[] fields = type.GetFields(allDeclared);
		List<FieldInfo> list = new List<FieldInfo>(fields.Length);
		list.AddRange(fields);
		return list;
	}

	public static Type GetReturnedType(MethodBase methodOrConstructor)
	{
		if ((object)methodOrConstructor == null)
		{
			FileLog.Debug("AccessTools.GetReturnedType: methodOrConstructor is null");
			return null;
		}
		if (methodOrConstructor is ConstructorInfo)
		{
			return typeof(void);
		}
		return ((MethodInfo)methodOrConstructor).ReturnType;
	}

	public static Type Inner(Type type, string name)
	{
		if ((object)type == null)
		{
			FileLog.Debug("AccessTools.Inner: type is null");
			return null;
		}
		if (name == null)
		{
			FileLog.Debug("AccessTools.Inner: name is null");
			return null;
		}
		return FindIncludingBaseTypes(type, (Type t) => t.GetNestedType(name, all));
	}

	public static Type FirstInner(Type type, Func<Type, bool> predicate)
	{
		if ((object)type == null)
		{
			FileLog.Debug("AccessTools.FirstInner: type is null");
			return null;
		}
		if (predicate == null)
		{
			FileLog.Debug("AccessTools.FirstInner: predicate is null");
			return null;
		}
		return type.GetNestedTypes(all).FirstOrDefault((Type subType) => predicate(subType));
	}

	public static MethodInfo FirstMethod(Type type, Func<MethodInfo, bool> predicate)
	{
		if ((object)type == null)
		{
			FileLog.Debug("AccessTools.FirstMethod: type is null");
			return null;
		}
		if (predicate == null)
		{
			FileLog.Debug("AccessTools.FirstMethod: predicate is null");
			return null;
		}
		return type.GetMethods(allDeclared).FirstOrDefault((MethodInfo method) => predicate(method));
	}

	public static ConstructorInfo FirstConstructor(Type type, Func<ConstructorInfo, bool> predicate)
	{
		if ((object)type == null)
		{
			FileLog.Debug("AccessTools.FirstConstructor: type is null");
			return null;
		}
		if (predicate == null)
		{
			FileLog.Debug("AccessTools.FirstConstructor: predicate is null");
			return null;
		}
		return type.GetConstructors(allDeclared).FirstOrDefault((ConstructorInfo constructor) => predicate(constructor));
	}

	public static PropertyInfo FirstProperty(Type type, Func<PropertyInfo, bool> predicate)
	{
		if ((object)type == null)
		{
			FileLog.Debug("AccessTools.FirstProperty: type is null");
			return null;
		}
		if (predicate == null)
		{
			FileLog.Debug("AccessTools.FirstProperty: predicate is null");
			return null;
		}
		return type.GetProperties(allDeclared).FirstOrDefault((PropertyInfo property) => predicate(property));
	}

	public static Type[] GetTypes(object[] parameters)
	{
		if (parameters == null)
		{
			return Array.Empty<Type>();
		}
		return parameters.Select((object p) => (p != null) ? p.GetType() : typeof(object)).ToArray();
	}

	public static object[] ActualParameters(MethodBase method, object[] inputs)
	{
		List<Type> inputTypes = inputs.Select((object obj) => obj?.GetType()).ToList();
		return (from p in method.GetParameters()
			select p.ParameterType).Select(delegate(Type pType)
		{
			int num = inputTypes.FindIndex((Type inType) => (object)inType != null && pType.IsAssignableFrom(inType));
			return (num >= 0) ? inputs[num] : GetDefaultValue(pType);
		}).ToArray();
	}

	public static FieldRef<T, F> FieldRefAccess<T, F>(string fieldName)
	{
		if (fieldName == null)
		{
			throw new ArgumentNullException("fieldName");
		}
		try
		{
			Type typeFromHandle = typeof(T);
			if (typeFromHandle.IsValueType)
			{
				throw new ArgumentException("T (FieldRefAccess instance type) must not be a value type");
			}
			return Tools.FieldRefAccess<T, F>(Tools.GetInstanceField(typeFromHandle, fieldName), needCastclass: false);
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"FieldRefAccess<{typeof(T)}, {typeof(F)}> for {fieldName} caused an exception", innerException);
		}
	}

	public static ref F FieldRefAccess<T, F>(T instance, string fieldName)
	{
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		if (fieldName == null)
		{
			throw new ArgumentNullException("fieldName");
		}
		try
		{
			Type typeFromHandle = typeof(T);
			if (typeFromHandle.IsValueType)
			{
				throw new ArgumentException("T (FieldRefAccess instance type) must not be a value type");
			}
			return ref Tools.FieldRefAccess<T, F>(Tools.GetInstanceField(typeFromHandle, fieldName), needCastclass: false)(instance);
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"FieldRefAccess<{typeof(T)}, {typeof(F)}> for {instance}, {fieldName} caused an exception", innerException);
		}
	}

	public static FieldRef<object, F> FieldRefAccess<F>(Type type, string fieldName)
	{
		if ((object)type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (fieldName == null)
		{
			throw new ArgumentNullException("fieldName");
		}
		try
		{
			FieldInfo fieldInfo = Field(type, fieldName);
			if ((object)fieldInfo == null)
			{
				throw new MissingFieldException(type.Name, fieldName);
			}
			if (!fieldInfo.IsStatic)
			{
				Type declaringType = fieldInfo.DeclaringType;
				if ((object)declaringType != null && declaringType.IsValueType)
				{
					throw new ArgumentException("Either FieldDeclaringType must be a class or field must be static");
				}
			}
			return Tools.FieldRefAccess<object, F>(fieldInfo, needCastclass: true);
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"FieldRefAccess<{typeof(F)}> for {type}, {fieldName} caused an exception", innerException);
		}
	}

	public static FieldRef<object, F> FieldRefAccess<F>(string typeColonName)
	{
		Tools.TypeAndName typeAndName = Tools.TypColonName(typeColonName);
		return FieldRefAccess<F>(typeAndName.type, typeAndName.name);
	}

	public static FieldRef<T, F> FieldRefAccess<T, F>(FieldInfo fieldInfo)
	{
		if ((object)fieldInfo == null)
		{
			throw new ArgumentNullException("fieldInfo");
		}
		try
		{
			Type typeFromHandle = typeof(T);
			if (typeFromHandle.IsValueType)
			{
				throw new ArgumentException("T (FieldRefAccess instance type) must not be a value type");
			}
			bool needCastclass = false;
			if (!fieldInfo.IsStatic)
			{
				Type declaringType = fieldInfo.DeclaringType;
				if ((object)declaringType != null)
				{
					if (declaringType.IsValueType)
					{
						throw new ArgumentException("Either FieldDeclaringType must be a class or field must be static");
					}
					needCastclass = Tools.FieldRefNeedsClasscast(typeFromHandle, declaringType);
				}
			}
			return Tools.FieldRefAccess<T, F>(fieldInfo, needCastclass);
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"FieldRefAccess<{typeof(T)}, {typeof(F)}> for {fieldInfo} caused an exception", innerException);
		}
	}

	public static ref F FieldRefAccess<T, F>(T instance, FieldInfo fieldInfo)
	{
		if (instance == null)
		{
			throw new ArgumentNullException("instance");
		}
		if ((object)fieldInfo == null)
		{
			throw new ArgumentNullException("fieldInfo");
		}
		try
		{
			Type typeFromHandle = typeof(T);
			if (typeFromHandle.IsValueType)
			{
				throw new ArgumentException("T (FieldRefAccess instance type) must not be a value type");
			}
			if (fieldInfo.IsStatic)
			{
				throw new ArgumentException("Field must not be static");
			}
			bool needCastclass = false;
			Type declaringType = fieldInfo.DeclaringType;
			if ((object)declaringType != null)
			{
				if (declaringType.IsValueType)
				{
					throw new ArgumentException("FieldDeclaringType must be a class");
				}
				needCastclass = Tools.FieldRefNeedsClasscast(typeFromHandle, declaringType);
			}
			return ref Tools.FieldRefAccess<T, F>(fieldInfo, needCastclass)(instance);
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"FieldRefAccess<{typeof(T)}, {typeof(F)}> for {instance}, {fieldInfo} caused an exception", innerException);
		}
	}

	public static StructFieldRef<T, F> StructFieldRefAccess<T, F>(string fieldName) where T : struct
	{
		if (fieldName == null)
		{
			throw new ArgumentNullException("fieldName");
		}
		try
		{
			return Tools.StructFieldRefAccess<T, F>(Tools.GetInstanceField(typeof(T), fieldName));
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"StructFieldRefAccess<{typeof(T)}, {typeof(F)}> for {fieldName} caused an exception", innerException);
		}
	}

	public static ref F StructFieldRefAccess<T, F>(ref T instance, string fieldName) where T : struct
	{
		if (fieldName == null)
		{
			throw new ArgumentNullException("fieldName");
		}
		try
		{
			return ref Tools.StructFieldRefAccess<T, F>(Tools.GetInstanceField(typeof(T), fieldName))(ref instance);
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"StructFieldRefAccess<{typeof(T)}, {typeof(F)}> for {instance}, {fieldName} caused an exception", innerException);
		}
	}

	public static StructFieldRef<T, F> StructFieldRefAccess<T, F>(FieldInfo fieldInfo) where T : struct
	{
		if ((object)fieldInfo == null)
		{
			throw new ArgumentNullException("fieldInfo");
		}
		try
		{
			Tools.ValidateStructField<T, F>(fieldInfo);
			return Tools.StructFieldRefAccess<T, F>(fieldInfo);
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"StructFieldRefAccess<{typeof(T)}, {typeof(F)}> for {fieldInfo} caused an exception", innerException);
		}
	}

	public static ref F StructFieldRefAccess<T, F>(ref T instance, FieldInfo fieldInfo) where T : struct
	{
		if ((object)fieldInfo == null)
		{
			throw new ArgumentNullException("fieldInfo");
		}
		try
		{
			Tools.ValidateStructField<T, F>(fieldInfo);
			return ref Tools.StructFieldRefAccess<T, F>(fieldInfo)(ref instance);
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"StructFieldRefAccess<{typeof(T)}, {typeof(F)}> for {instance}, {fieldInfo} caused an exception", innerException);
		}
	}

	public static ref F StaticFieldRefAccess<T, F>(string fieldName)
	{
		return ref StaticFieldRefAccess<F>(typeof(T), fieldName);
	}

	public static ref F StaticFieldRefAccess<F>(Type type, string fieldName)
	{
		try
		{
			FieldInfo fieldInfo = Field(type, fieldName);
			if ((object)fieldInfo == null)
			{
				throw new MissingFieldException(type.Name, fieldName);
			}
			return ref Tools.StaticFieldRefAccess<F>(fieldInfo)();
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"StaticFieldRefAccess<{typeof(F)}> for {type}, {fieldName} caused an exception", innerException);
		}
	}

	public static ref F StaticFieldRefAccess<F>(string typeColonName)
	{
		Tools.TypeAndName typeAndName = Tools.TypColonName(typeColonName);
		return ref StaticFieldRefAccess<F>(typeAndName.type, typeAndName.name);
	}

	public static ref F StaticFieldRefAccess<T, F>(FieldInfo fieldInfo)
	{
		if ((object)fieldInfo == null)
		{
			throw new ArgumentNullException("fieldInfo");
		}
		try
		{
			return ref Tools.StaticFieldRefAccess<F>(fieldInfo)();
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"StaticFieldRefAccess<{typeof(T)}, {typeof(F)}> for {fieldInfo} caused an exception", innerException);
		}
	}

	public static FieldRef<F> StaticFieldRefAccess<F>(FieldInfo fieldInfo)
	{
		if ((object)fieldInfo == null)
		{
			throw new ArgumentNullException("fieldInfo");
		}
		try
		{
			return Tools.StaticFieldRefAccess<F>(fieldInfo);
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"StaticFieldRefAccess<{typeof(F)}> for {fieldInfo} caused an exception", innerException);
		}
	}

	public static DelegateType MethodDelegate<DelegateType>(MethodInfo method, object instance = null, bool virtualCall = true) where DelegateType : Delegate
	{
		if ((object)method == null)
		{
			throw new ArgumentNullException("method");
		}
		Type typeFromHandle = typeof(DelegateType);
		if (method.IsStatic)
		{
			return (DelegateType)Delegate.CreateDelegate(typeFromHandle, method);
		}
		Type type = method.DeclaringType;
		if (type != null && type.IsInterface && !virtualCall)
		{
			throw new ArgumentException("Interface methods must be called virtually");
		}
		if (instance == null)
		{
			ParameterInfo[] parameters = typeFromHandle.GetMethod("Invoke").GetParameters();
			if (parameters.Length == 0)
			{
				Delegate.CreateDelegate(typeof(DelegateType), method);
				throw new ArgumentException("Invalid delegate type");
			}
			Type parameterType = parameters[0].ParameterType;
			if (type != null && type.IsInterface && parameterType.IsValueType)
			{
				InterfaceMapping interfaceMap = parameterType.GetInterfaceMap(type);
				method = interfaceMap.TargetMethods[Array.IndexOf(interfaceMap.InterfaceMethods, method)];
				type = parameterType;
			}
			if (type != null && virtualCall)
			{
				if (type.IsInterface)
				{
					return (DelegateType)Delegate.CreateDelegate(typeFromHandle, method);
				}
				if (parameterType.IsInterface)
				{
					InterfaceMapping interfaceMap2 = type.GetInterfaceMap(parameterType);
					MethodInfo method2 = interfaceMap2.InterfaceMethods[Array.IndexOf(interfaceMap2.TargetMethods, method)];
					return (DelegateType)Delegate.CreateDelegate(typeFromHandle, method2);
				}
				if (!type.IsValueType)
				{
					return (DelegateType)Delegate.CreateDelegate(typeFromHandle, method.GetBaseDefinition());
				}
			}
			ParameterInfo[] parameters2 = method.GetParameters();
			int num = parameters2.Length;
			Type[] array = new Type[num + 1];
			array[0] = type;
			for (int i = 0; i < num; i++)
			{
				array[i + 1] = parameters2[i].ParameterType;
			}
			DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition("OpenInstanceDelegate_" + method.Name, method.ReturnType, array);
			ILGenerator iLGenerator = dynamicMethodDefinition.GetILGenerator();
			if (type != null && type.IsValueType)
			{
				iLGenerator.Emit(OpCodes.Ldarga_S, 0);
			}
			else
			{
				iLGenerator.Emit(OpCodes.Ldarg_0);
			}
			for (int j = 1; j < array.Length; j++)
			{
				iLGenerator.Emit(OpCodes.Ldarg, j);
			}
			iLGenerator.Emit(OpCodes.Call, method);
			iLGenerator.Emit(OpCodes.Ret);
			return (DelegateType)dynamicMethodDefinition.Generate().CreateDelegate(typeFromHandle);
		}
		if (virtualCall)
		{
			return (DelegateType)Delegate.CreateDelegate(typeFromHandle, instance, method.GetBaseDefinition());
		}
		if (type != null && !type.IsInstanceOfType(instance))
		{
			Delegate.CreateDelegate(typeof(DelegateType), instance, method);
			throw new ArgumentException("Invalid delegate type");
		}
		if (IsMonoRuntime)
		{
			DynamicMethodDefinition dynamicMethodDefinition2 = new DynamicMethodDefinition("LdftnDelegate_" + method.Name, typeFromHandle, new Type[1] { typeof(object) });
			ILGenerator iLGenerator2 = dynamicMethodDefinition2.GetILGenerator();
			iLGenerator2.Emit(OpCodes.Ldarg_0);
			iLGenerator2.Emit(OpCodes.Ldftn, method);
			iLGenerator2.Emit(OpCodes.Newobj, typeFromHandle.GetConstructor(new Type[2]
			{
				typeof(object),
				typeof(IntPtr)
			}));
			iLGenerator2.Emit(OpCodes.Ret);
			return (DelegateType)dynamicMethodDefinition2.Generate().Invoke(null, new object[1] { instance });
		}
		return (DelegateType)Activator.CreateInstance(typeFromHandle, instance, method.MethodHandle.GetFunctionPointer());
	}

	public static DelegateType MethodDelegate<DelegateType>(string typeColonName, object instance = null, bool virtualCall = true) where DelegateType : Delegate
	{
		MethodInfo method = DeclaredMethod(typeColonName);
		return MethodDelegate<DelegateType>(method, instance, virtualCall);
	}

	public static DelegateType HarmonyDelegate<DelegateType>(object instance = null) where DelegateType : Delegate
	{
		HarmonyMethod mergedFromType = HarmonyMethodExtensions.GetMergedFromType(typeof(DelegateType));
		HarmonyMethod harmonyMethod = mergedFromType;
		MethodType valueOrDefault = harmonyMethod.methodType.GetValueOrDefault();
		if (!harmonyMethod.methodType.HasValue)
		{
			valueOrDefault = MethodType.Normal;
			harmonyMethod.methodType = valueOrDefault;
		}
		if (!(mergedFromType.GetOriginalMethod() is MethodInfo method))
		{
			throw new NullReferenceException($"Delegate {typeof(DelegateType)} has no defined original method");
		}
		return MethodDelegate<DelegateType>(method, instance, !mergedFromType.nonVirtualDelegate);
	}

	public static MethodBase GetOutsideCaller()
	{
		StackTrace stackTrace = new StackTrace(fNeedFileInfo: true);
		StackFrame[] frames = stackTrace.GetFrames();
		foreach (StackFrame stackFrame in frames)
		{
			MethodBase method = stackFrame.GetMethod();
			if (method.DeclaringType?.Namespace != typeof(Harmony).Namespace)
			{
				return method;
			}
		}
		throw new Exception("Unexpected end of stack trace");
	}

	public static void RethrowException(Exception exception)
	{
		ExceptionDispatchInfo.Capture(exception).Throw();
		throw exception;
	}

	public static void ThrowMissingMemberException(Type type, params string[] names)
	{
		string value = string.Join(",", GetFieldNames(type).ToArray());
		string value2 = string.Join(",", GetPropertyNames(type).ToArray());
		throw new MissingMemberException($"{string.Join(",", names)}; available fields: {value}; available properties: {value2}");
	}

	public static object GetDefaultValue(Type type)
	{
		if ((object)type == null)
		{
			FileLog.Debug("AccessTools.GetDefaultValue: type is null");
			return null;
		}
		if (type == typeof(void))
		{
			return null;
		}
		if (type.IsValueType)
		{
			return Activator.CreateInstance(type);
		}
		return null;
	}

	public static object CreateInstance(Type type)
	{
		if ((object)type == null)
		{
			throw new ArgumentNullException("type");
		}
		ConstructorInfo constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, CallingConventions.Any, Array.Empty<Type>(), null);
		if ((object)constructor != null)
		{
			return constructor.Invoke(null);
		}
		return FormatterServices.GetUninitializedObject(type);
	}

	public static T CreateInstance<T>()
	{
		object obj = CreateInstance(typeof(T));
		if (obj is T)
		{
			return (T)obj;
		}
		return default(T);
	}

	public static T MakeDeepCopy<T>(object source) where T : class
	{
		return MakeDeepCopy(source, typeof(T)) as T;
	}

	public static void MakeDeepCopy<T>(object source, out T result, Func<string, Traverse, Traverse, object> processor = null, string pathRoot = "")
	{
		result = (T)MakeDeepCopy(source, typeof(T), processor, pathRoot);
	}

	public static object MakeDeepCopy(object source, Type resultType, Func<string, Traverse, Traverse, object> processor = null, string pathRoot = "")
	{
		if (source == null || (object)resultType == null)
		{
			return null;
		}
		resultType = Nullable.GetUnderlyingType(resultType) ?? resultType;
		Type type = source.GetType();
		if (type.IsPrimitive)
		{
			return source;
		}
		if (type.IsEnum)
		{
			return Enum.ToObject(resultType, (int)source);
		}
		if (type.IsGenericType && resultType.IsGenericType)
		{
			addHandlerCacheLock.EnterUpgradeableReadLock();
			try
			{
				if (!addHandlerCache.TryGetValue(resultType, out var value))
				{
					MethodInfo methodInfo = FirstMethod(resultType, (MethodInfo m) => m.Name == "Add" && m.GetParameters().Length == 1);
					if ((object)methodInfo != null)
					{
						value = MethodInvoker.GetHandler(methodInfo);
					}
					addHandlerCacheLock.EnterWriteLock();
					try
					{
						addHandlerCache[resultType] = value;
					}
					finally
					{
						addHandlerCacheLock.ExitWriteLock();
					}
				}
				if (value != null)
				{
					object obj = Activator.CreateInstance(resultType);
					Type resultType2 = resultType.GetGenericArguments()[0];
					int num = 0;
					foreach (object item in source as IEnumerable)
					{
						string text = num++.ToString();
						string pathRoot2 = ((pathRoot.Length > 0) ? (pathRoot + "." + text) : text);
						object obj2 = MakeDeepCopy(item, resultType2, processor, pathRoot2);
						value(obj, obj2);
					}
					return obj;
				}
			}
			finally
			{
				addHandlerCacheLock.ExitUpgradeableReadLock();
			}
		}
		if (type.IsArray && resultType.IsArray)
		{
			Type elementType = resultType.GetElementType();
			int length = ((Array)source).Length;
			object[] array = Activator.CreateInstance(resultType, length) as object[];
			object[] array2 = source as object[];
			for (int i = 0; i < length; i++)
			{
				string text2 = i.ToString();
				string pathRoot3 = ((pathRoot.Length > 0) ? (pathRoot + "." + text2) : text2);
				array[i] = MakeDeepCopy(array2[i], elementType, processor, pathRoot3);
			}
			return array;
		}
		string @namespace = type.Namespace;
		if (@namespace == "System" || (@namespace != null && @namespace.StartsWith("System.")))
		{
			return source;
		}
		object obj3 = CreateInstance((resultType == typeof(object)) ? type : resultType);
		Traverse.IterateFields(source, obj3, delegate(string name, Traverse src, Traverse dst)
		{
			string text3 = ((pathRoot.Length > 0) ? (pathRoot + "." + name) : name);
			object source2 = ((processor != null) ? processor(text3, src, dst) : src.GetValue());
			dst.SetValue(MakeDeepCopy(source2, dst.GetValueType(), processor, text3));
		});
		return obj3;
	}

	public static bool IsStruct(Type type)
	{
		if (type == null)
		{
			return false;
		}
		if (type.IsValueType && !IsValue(type))
		{
			return !IsVoid(type);
		}
		return false;
	}

	public static bool IsClass(Type type)
	{
		if (type == null)
		{
			return false;
		}
		return !type.IsValueType;
	}

	public static bool IsValue(Type type)
	{
		if (type == null)
		{
			return false;
		}
		if (!type.IsPrimitive)
		{
			return type.IsEnum;
		}
		return true;
	}

	public static bool IsInteger(Type type)
	{
		if (type == null)
		{
			return false;
		}
		TypeCode typeCode = Type.GetTypeCode(type);
		if ((uint)(typeCode - 5) <= 7u)
		{
			return true;
		}
		return false;
	}

	public static bool IsFloatingPoint(Type type)
	{
		if (type == null)
		{
			return false;
		}
		TypeCode typeCode = Type.GetTypeCode(type);
		if ((uint)(typeCode - 13) <= 2u)
		{
			return true;
		}
		return false;
	}

	public static bool IsNumber(Type type)
	{
		if (!IsInteger(type))
		{
			return IsFloatingPoint(type);
		}
		return true;
	}

	public static bool IsVoid(Type type)
	{
		return type == typeof(void);
	}

	public static bool IsOfNullableType<T>(T instance)
	{
		return (object)Nullable.GetUnderlyingType(typeof(T)) != null;
	}

	public static bool IsStatic(MemberInfo member)
	{
		if ((object)member == null)
		{
			throw new ArgumentNullException("member");
		}
		switch (member.MemberType)
		{
		case MemberTypes.Constructor:
		case MemberTypes.Method:
			return ((MethodBase)member).IsStatic;
		case MemberTypes.Event:
			return IsStatic((EventInfo)member);
		case MemberTypes.Field:
			return ((FieldInfo)member).IsStatic;
		case MemberTypes.Property:
			return IsStatic((PropertyInfo)member);
		case MemberTypes.TypeInfo:
		case MemberTypes.NestedType:
			return IsStatic((Type)member);
		default:
			throw new ArgumentException($"Unknown member type: {member.MemberType}");
		}
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static bool IsStatic(Type type)
	{
		if ((object)type == null)
		{
			return false;
		}
		if (type.IsAbstract)
		{
			return type.IsSealed;
		}
		return false;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static bool IsStatic(PropertyInfo propertyInfo)
	{
		if ((object)propertyInfo == null)
		{
			throw new ArgumentNullException("propertyInfo");
		}
		return propertyInfo.GetAccessors(nonPublic: true)[0].IsStatic;
	}

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static bool IsStatic(EventInfo eventInfo)
	{
		if ((object)eventInfo == null)
		{
			throw new ArgumentNullException("eventInfo");
		}
		return eventInfo.GetAddMethod(nonPublic: true).IsStatic;
	}

	public static int CombinedHashCode(IEnumerable<object> objects)
	{
		int num = 352654597;
		int num2 = num;
		int num3 = 0;
		foreach (object @object in objects)
		{
			if (num3 % 2 == 0)
			{
				num = ((num << 5) + num + (num >> 27)) ^ @object.GetHashCode();
			}
			else
			{
				num2 = ((num2 << 5) + num2 + (num2 >> 27)) ^ @object.GetHashCode();
			}
			num3++;
		}
		return num + num2 * 1566083941;
	}
}
