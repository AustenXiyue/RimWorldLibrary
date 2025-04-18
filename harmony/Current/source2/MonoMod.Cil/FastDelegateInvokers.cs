using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mono.Cecil.Cil;
using MonoMod.Utils;

namespace MonoMod.Cil;

internal static class FastDelegateInvokers
{
	private delegate void VoidVal1<T0>(T0 _0);

	private delegate TResult TypeVal1<TResult, T0>(T0 _0);

	private delegate void VoidRef1<T0>(ref T0 _0);

	private delegate TResult TypeRef1<TResult, T0>(ref T0 _0);

	private delegate void VoidVal2<T0, T1>(T0 _0, T1 _1);

	private delegate TResult TypeVal2<TResult, T0, T1>(T0 _0, T1 _1);

	private delegate void VoidRef2<T0, T1>(ref T0 _0, T1 _1);

	private delegate TResult TypeRef2<TResult, T0, T1>(ref T0 _0, T1 _1);

	private delegate void VoidVal3<T0, T1, T2>(T0 _0, T1 _1, T2 _2);

	private delegate TResult TypeVal3<TResult, T0, T1, T2>(T0 _0, T1 _1, T2 _2);

	private delegate void VoidRef3<T0, T1, T2>(ref T0 _0, T1 _1, T2 _2);

	private delegate TResult TypeRef3<TResult, T0, T1, T2>(ref T0 _0, T1 _1, T2 _2);

	private delegate void VoidVal4<T0, T1, T2, T3>(T0 _0, T1 _1, T2 _2, T3 _3);

	private delegate TResult TypeVal4<TResult, T0, T1, T2, T3>(T0 _0, T1 _1, T2 _2, T3 _3);

	private delegate void VoidRef4<T0, T1, T2, T3>(ref T0 _0, T1 _1, T2 _2, T3 _3);

	private delegate TResult TypeRef4<TResult, T0, T1, T2, T3>(ref T0 _0, T1 _1, T2 _2, T3 _3);

	private delegate void VoidVal5<T0, T1, T2, T3, T4>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4);

	private delegate TResult TypeVal5<TResult, T0, T1, T2, T3, T4>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4);

	private delegate void VoidRef5<T0, T1, T2, T3, T4>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4);

	private delegate TResult TypeRef5<TResult, T0, T1, T2, T3, T4>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4);

	private delegate void VoidVal6<T0, T1, T2, T3, T4, T5>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5);

	private delegate TResult TypeVal6<TResult, T0, T1, T2, T3, T4, T5>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5);

	private delegate void VoidRef6<T0, T1, T2, T3, T4, T5>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5);

	private delegate TResult TypeRef6<TResult, T0, T1, T2, T3, T4, T5>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5);

	private delegate void VoidVal7<T0, T1, T2, T3, T4, T5, T6>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6);

	private delegate TResult TypeVal7<TResult, T0, T1, T2, T3, T4, T5, T6>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6);

	private delegate void VoidRef7<T0, T1, T2, T3, T4, T5, T6>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6);

	private delegate TResult TypeRef7<TResult, T0, T1, T2, T3, T4, T5, T6>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6);

	private delegate void VoidVal8<T0, T1, T2, T3, T4, T5, T6, T7>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7);

	private delegate TResult TypeVal8<TResult, T0, T1, T2, T3, T4, T5, T6, T7>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7);

	private delegate void VoidRef8<T0, T1, T2, T3, T4, T5, T6, T7>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7);

	private delegate TResult TypeRef8<TResult, T0, T1, T2, T3, T4, T5, T6, T7>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7);

	private delegate void VoidVal9<T0, T1, T2, T3, T4, T5, T6, T7, T8>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8);

	private delegate TResult TypeVal9<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8);

	private delegate void VoidRef9<T0, T1, T2, T3, T4, T5, T6, T7, T8>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8);

	private delegate TResult TypeRef9<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8);

	private delegate void VoidVal10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9);

	private delegate TResult TypeVal10<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9);

	private delegate void VoidRef10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9);

	private delegate TResult TypeRef10<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9);

	private delegate void VoidVal11<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10);

	private delegate TResult TypeVal11<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10);

	private delegate void VoidRef11<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10);

	private delegate TResult TypeRef11<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10);

	private delegate void VoidVal12<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11);

	private delegate TResult TypeVal12<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11);

	private delegate void VoidRef12<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11);

	private delegate TResult TypeRef12<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11);

	private delegate void VoidVal13<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12);

	private delegate TResult TypeVal13<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12);

	private delegate void VoidRef13<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12);

	private delegate TResult TypeRef13<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12);

	private delegate void VoidVal14<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, T13 _13);

	private delegate TResult TypeVal14<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, T13 _13);

	private delegate void VoidRef14<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, T13 _13);

	private delegate TResult TypeRef14<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, T13 _13);

	private delegate void VoidVal15<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, T13 _13, T14 _14);

	private delegate TResult TypeVal15<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, T13 _13, T14 _14);

	private delegate void VoidRef15<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, T13 _13, T14 _14);

	private delegate TResult TypeRef15<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, T13 _13, T14 _14);

	private delegate void VoidVal16<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, T13 _13, T14 _14, T15 _15);

	private delegate TResult TypeVal16<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, T13 _13, T14 _14, T15 _15);

	private delegate void VoidRef16<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, T13 _13, T14 _14, T15 _15);

	private delegate TResult TypeRef16<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, T13 _13, T14 _14, T15 _15);

	private static readonly (MethodInfo, Type)[] invokers = GetInvokers();

	private const int MaxFastInvokerParams = 16;

	private static readonly ConditionalWeakTable<Type, Tuple<MethodInfo?, Type>> invokerCache = new ConditionalWeakTable<Type, Tuple<MethodInfo, Type>>();

	[GetFastDelegateInvokersArray(16)]
	private static (MethodInfo, Type)[] GetInvokers()
	{
		return new(MethodInfo, Type)[64]
		{
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidVal1", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidVal1<>)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeVal1", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeVal1<, >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidRef1", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidRef1<>)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeRef1", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeRef1<, >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidVal2", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidVal2<, >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeVal2", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeVal2<, , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidRef2", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidRef2<, >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeRef2", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeRef2<, , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidVal3", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidVal3<, , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeVal3", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeVal3<, , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidRef3", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidRef3<, , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeRef3", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeRef3<, , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidVal4", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidVal4<, , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeVal4", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeVal4<, , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidRef4", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidRef4<, , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeRef4", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeRef4<, , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidVal5", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidVal5<, , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeVal5", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeVal5<, , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidRef5", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidRef5<, , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeRef5", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeRef5<, , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidVal6", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidVal6<, , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeVal6", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeVal6<, , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidRef6", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidRef6<, , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeRef6", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeRef6<, , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidVal7", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidVal7<, , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeVal7", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeVal7<, , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidRef7", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidRef7<, , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeRef7", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeRef7<, , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidVal8", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidVal8<, , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeVal8", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeVal8<, , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidRef8", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidRef8<, , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeRef8", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeRef8<, , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidVal9", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidVal9<, , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeVal9", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeVal9<, , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidRef9", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidRef9<, , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeRef9", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeRef9<, , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidVal10", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidVal10<, , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeVal10", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeVal10<, , , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidRef10", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidRef10<, , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeRef10", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeRef10<, , , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidVal11", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidVal11<, , , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeVal11", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeVal11<, , , , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidRef11", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidRef11<, , , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeRef11", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeRef11<, , , , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidVal12", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidVal12<, , , , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeVal12", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeVal12<, , , , , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidRef12", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidRef12<, , , , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeRef12", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeRef12<, , , , , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidVal13", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidVal13<, , , , , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeVal13", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeVal13<, , , , , , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidRef13", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidRef13<, , , , , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeRef13", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeRef13<, , , , , , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidVal14", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidVal14<, , , , , , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeVal14", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeVal14<, , , , , , , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidRef14", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidRef14<, , , , , , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeRef14", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeRef14<, , , , , , , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidVal15", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidVal15<, , , , , , , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeVal15", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeVal15<, , , , , , , , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidRef15", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidRef15<, , , , , , , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeRef15", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeRef15<, , , , , , , , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidVal16", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidVal16<, , , , , , , , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeVal16", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeVal16<, , , , , , , , , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeVoidRef16", BindingFlags.Static | BindingFlags.NonPublic), typeof(VoidRef16<, , , , , , , , , , , , , , , >)),
			(typeof(FastDelegateInvokers).GetMethod("InvokeTypeRef16", BindingFlags.Static | BindingFlags.NonPublic), typeof(TypeRef16<, , , , , , , , , , , , , , , , >))
		};
	}

	private static (MethodInfo Invoker, Type Delegate)? TryGetInvokerForSig(MethodSignature sig)
	{
		if (sig.ParameterCount == 0)
		{
			return null;
		}
		if (sig.ParameterCount > 16)
		{
			return null;
		}
		if (sig.ReturnType.IsByRef || sig.ReturnType.IsByRefLike())
		{
			return null;
		}
		if (sig.FirstParameter.IsByRefLike())
		{
			return null;
		}
		if (sig.Parameters.Skip(1).Any((Type t) => t.IsByRef || t.IsByRefLike()))
		{
			return null;
		}
		int num = 0;
		num |= ((sig.ReturnType != typeof(void)) ? 1 : 0);
		num |= (sig.FirstParameter.IsByRef ? 2 : 0);
		num |= sig.ParameterCount - 1 << 2;
		(MethodInfo, Type) tuple = invokers[num];
		MethodInfo item = tuple.Item1;
		Type item2 = tuple.Item2;
		Type[] array = new Type[sig.ParameterCount + (num & 1)];
		int num2 = 0;
		if ((num & 1) != 0)
		{
			array[num2++] = sig.ReturnType;
		}
		foreach (Type parameter in sig.Parameters)
		{
			Type type = parameter;
			if (type.IsByRef)
			{
				type = type.GetElementType();
			}
			array[num2++] = type;
		}
		Helpers.Assert(num2 == array.Length, null, "i == typeParams.Length");
		return (item.MakeGenericMethod(array), item2.MakeGenericType(array));
	}

	public static (MethodInfo Invoker, Type Delegate)? GetDelegateInvoker(Type delegateType)
	{
		Helpers.ThrowIfArgumentNull(delegateType, "delegateType");
		if (!typeof(Delegate).IsAssignableFrom(delegateType))
		{
			throw new ArgumentException("Argument not a delegate type", "delegateType");
		}
		Tuple<MethodInfo, Type> value = invokerCache.GetValue(delegateType, delegate(Type delegateType)
		{
			MethodInfo method = delegateType.GetMethod("Invoke");
			MethodSignature methodSignature = MethodSignature.ForMethod(method, ignoreThis: true);
			if (methodSignature.ParameterCount == 0)
			{
				return new Tuple<MethodInfo, Type>(null, delegateType);
			}
			(MethodInfo, Type)? tuple = TryGetInvokerForSig(methodSignature);
			if (tuple.HasValue)
			{
				(MethodInfo, Type) valueOrDefault = tuple.GetValueOrDefault();
				return new Tuple<MethodInfo, Type>(valueOrDefault.Item1, valueOrDefault.Item2);
			}
			Type[] array = new Type[methodSignature.ParameterCount + 1];
			int num = 0;
			foreach (Type parameter in methodSignature.Parameters)
			{
				array[num++] = parameter;
			}
			array[methodSignature.ParameterCount] = delegateType;
			using DynamicMethodDefinition dynamicMethodDefinition = new DynamicMethodDefinition("MMIL:Invoke<" + method.DeclaringType?.FullName + ">", method.ReturnType, array);
			ILProcessor iLProcessor = dynamicMethodDefinition.GetILProcessor();
			iLProcessor.Emit(OpCodes.Ldarg, methodSignature.ParameterCount);
			for (num = 0; num < methodSignature.ParameterCount; num++)
			{
				iLProcessor.Emit(OpCodes.Ldarg, num);
			}
			iLProcessor.Emit(OpCodes.Callvirt, method);
			iLProcessor.Emit(OpCodes.Ret);
			return new Tuple<MethodInfo, Type>(dynamicMethodDefinition.Generate(), delegateType);
		});
		if ((object)value.Item1 == null)
		{
			return null;
		}
		return (value.Item1, value.Item2);
	}

	private static void InvokeVoidVal1<T0>(T0 _0, VoidVal1<T0> del)
	{
		Helpers.ThrowIfNull(del, "del")(_0);
	}

	private static TResult InvokeTypeVal1<TResult, T0>(T0 _0, TypeVal1<TResult, T0> del)
	{
		return Helpers.ThrowIfNull(del, "del")(_0);
	}

	private static void InvokeVoidRef1<T0>(ref T0 _0, VoidRef1<T0> del)
	{
		Helpers.ThrowIfNull(del, "del")(ref _0);
	}

	private static TResult InvokeTypeRef1<TResult, T0>(ref T0 _0, TypeRef1<TResult, T0> del)
	{
		return Helpers.ThrowIfNull(del, "del")(ref _0);
	}

	private static void InvokeVoidVal2<T0, T1>(T0 _0, T1 _1, VoidVal2<T0, T1> del)
	{
		Helpers.ThrowIfNull(del, "del")(_0, _1);
	}

	private static TResult InvokeTypeVal2<TResult, T0, T1>(T0 _0, T1 _1, TypeVal2<TResult, T0, T1> del)
	{
		return Helpers.ThrowIfNull(del, "del")(_0, _1);
	}

	private static void InvokeVoidRef2<T0, T1>(ref T0 _0, T1 _1, VoidRef2<T0, T1> del)
	{
		Helpers.ThrowIfNull(del, "del")(ref _0, _1);
	}

	private static TResult InvokeTypeRef2<TResult, T0, T1>(ref T0 _0, T1 _1, TypeRef2<TResult, T0, T1> del)
	{
		return Helpers.ThrowIfNull(del, "del")(ref _0, _1);
	}

	private static void InvokeVoidVal3<T0, T1, T2>(T0 _0, T1 _1, T2 _2, VoidVal3<T0, T1, T2> del)
	{
		Helpers.ThrowIfNull(del, "del")(_0, _1, _2);
	}

	private static TResult InvokeTypeVal3<TResult, T0, T1, T2>(T0 _0, T1 _1, T2 _2, TypeVal3<TResult, T0, T1, T2> del)
	{
		return Helpers.ThrowIfNull(del, "del")(_0, _1, _2);
	}

	private static void InvokeVoidRef3<T0, T1, T2>(ref T0 _0, T1 _1, T2 _2, VoidRef3<T0, T1, T2> del)
	{
		Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2);
	}

	private static TResult InvokeTypeRef3<TResult, T0, T1, T2>(ref T0 _0, T1 _1, T2 _2, TypeRef3<TResult, T0, T1, T2> del)
	{
		return Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2);
	}

	private static void InvokeVoidVal4<T0, T1, T2, T3>(T0 _0, T1 _1, T2 _2, T3 _3, VoidVal4<T0, T1, T2, T3> del)
	{
		Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3);
	}

	private static TResult InvokeTypeVal4<TResult, T0, T1, T2, T3>(T0 _0, T1 _1, T2 _2, T3 _3, TypeVal4<TResult, T0, T1, T2, T3> del)
	{
		return Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3);
	}

	private static void InvokeVoidRef4<T0, T1, T2, T3>(ref T0 _0, T1 _1, T2 _2, T3 _3, VoidRef4<T0, T1, T2, T3> del)
	{
		Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3);
	}

	private static TResult InvokeTypeRef4<TResult, T0, T1, T2, T3>(ref T0 _0, T1 _1, T2 _2, T3 _3, TypeRef4<TResult, T0, T1, T2, T3> del)
	{
		return Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3);
	}

	private static void InvokeVoidVal5<T0, T1, T2, T3, T4>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, VoidVal5<T0, T1, T2, T3, T4> del)
	{
		Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3, _4);
	}

	private static TResult InvokeTypeVal5<TResult, T0, T1, T2, T3, T4>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, TypeVal5<TResult, T0, T1, T2, T3, T4> del)
	{
		return Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3, _4);
	}

	private static void InvokeVoidRef5<T0, T1, T2, T3, T4>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, VoidRef5<T0, T1, T2, T3, T4> del)
	{
		Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3, _4);
	}

	private static TResult InvokeTypeRef5<TResult, T0, T1, T2, T3, T4>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, TypeRef5<TResult, T0, T1, T2, T3, T4> del)
	{
		return Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3, _4);
	}

	private static void InvokeVoidVal6<T0, T1, T2, T3, T4, T5>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, VoidVal6<T0, T1, T2, T3, T4, T5> del)
	{
		Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3, _4, _5);
	}

	private static TResult InvokeTypeVal6<TResult, T0, T1, T2, T3, T4, T5>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, TypeVal6<TResult, T0, T1, T2, T3, T4, T5> del)
	{
		return Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3, _4, _5);
	}

	private static void InvokeVoidRef6<T0, T1, T2, T3, T4, T5>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, VoidRef6<T0, T1, T2, T3, T4, T5> del)
	{
		Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3, _4, _5);
	}

	private static TResult InvokeTypeRef6<TResult, T0, T1, T2, T3, T4, T5>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, TypeRef6<TResult, T0, T1, T2, T3, T4, T5> del)
	{
		return Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3, _4, _5);
	}

	private static void InvokeVoidVal7<T0, T1, T2, T3, T4, T5, T6>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, VoidVal7<T0, T1, T2, T3, T4, T5, T6> del)
	{
		Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3, _4, _5, _6);
	}

	private static TResult InvokeTypeVal7<TResult, T0, T1, T2, T3, T4, T5, T6>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, TypeVal7<TResult, T0, T1, T2, T3, T4, T5, T6> del)
	{
		return Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3, _4, _5, _6);
	}

	private static void InvokeVoidRef7<T0, T1, T2, T3, T4, T5, T6>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, VoidRef7<T0, T1, T2, T3, T4, T5, T6> del)
	{
		Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3, _4, _5, _6);
	}

	private static TResult InvokeTypeRef7<TResult, T0, T1, T2, T3, T4, T5, T6>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, TypeRef7<TResult, T0, T1, T2, T3, T4, T5, T6> del)
	{
		return Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3, _4, _5, _6);
	}

	private static void InvokeVoidVal8<T0, T1, T2, T3, T4, T5, T6, T7>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, VoidVal8<T0, T1, T2, T3, T4, T5, T6, T7> del)
	{
		Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3, _4, _5, _6, _7);
	}

	private static TResult InvokeTypeVal8<TResult, T0, T1, T2, T3, T4, T5, T6, T7>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, TypeVal8<TResult, T0, T1, T2, T3, T4, T5, T6, T7> del)
	{
		return Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3, _4, _5, _6, _7);
	}

	private static void InvokeVoidRef8<T0, T1, T2, T3, T4, T5, T6, T7>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, VoidRef8<T0, T1, T2, T3, T4, T5, T6, T7> del)
	{
		Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3, _4, _5, _6, _7);
	}

	private static TResult InvokeTypeRef8<TResult, T0, T1, T2, T3, T4, T5, T6, T7>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, TypeRef8<TResult, T0, T1, T2, T3, T4, T5, T6, T7> del)
	{
		return Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3, _4, _5, _6, _7);
	}

	private static void InvokeVoidVal9<T0, T1, T2, T3, T4, T5, T6, T7, T8>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, VoidVal9<T0, T1, T2, T3, T4, T5, T6, T7, T8> del)
	{
		Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3, _4, _5, _6, _7, _8);
	}

	private static TResult InvokeTypeVal9<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, TypeVal9<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8> del)
	{
		return Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3, _4, _5, _6, _7, _8);
	}

	private static void InvokeVoidRef9<T0, T1, T2, T3, T4, T5, T6, T7, T8>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, VoidRef9<T0, T1, T2, T3, T4, T5, T6, T7, T8> del)
	{
		Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3, _4, _5, _6, _7, _8);
	}

	private static TResult InvokeTypeRef9<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, TypeRef9<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8> del)
	{
		return Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3, _4, _5, _6, _7, _8);
	}

	private static void InvokeVoidVal10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, VoidVal10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> del)
	{
		Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3, _4, _5, _6, _7, _8, _9);
	}

	private static TResult InvokeTypeVal10<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, TypeVal10<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> del)
	{
		return Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3, _4, _5, _6, _7, _8, _9);
	}

	private static void InvokeVoidRef10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, VoidRef10<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> del)
	{
		Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3, _4, _5, _6, _7, _8, _9);
	}

	private static TResult InvokeTypeRef10<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, TypeRef10<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9> del)
	{
		return Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3, _4, _5, _6, _7, _8, _9);
	}

	private static void InvokeVoidVal11<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, VoidVal11<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> del)
	{
		Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10);
	}

	private static TResult InvokeTypeVal11<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, TypeVal11<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> del)
	{
		return Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10);
	}

	private static void InvokeVoidRef11<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, VoidRef11<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> del)
	{
		Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10);
	}

	private static TResult InvokeTypeRef11<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, TypeRef11<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> del)
	{
		return Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10);
	}

	private static void InvokeVoidVal12<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, VoidVal12<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> del)
	{
		Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11);
	}

	private static TResult InvokeTypeVal12<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, TypeVal12<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> del)
	{
		return Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11);
	}

	private static void InvokeVoidRef12<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, VoidRef12<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> del)
	{
		Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11);
	}

	private static TResult InvokeTypeRef12<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, TypeRef12<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> del)
	{
		return Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11);
	}

	private static void InvokeVoidVal13<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, VoidVal13<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> del)
	{
		Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12);
	}

	private static TResult InvokeTypeVal13<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, TypeVal13<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> del)
	{
		return Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12);
	}

	private static void InvokeVoidRef13<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, VoidRef13<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> del)
	{
		Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12);
	}

	private static TResult InvokeTypeRef13<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, TypeRef13<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> del)
	{
		return Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12);
	}

	private static void InvokeVoidVal14<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, T13 _13, VoidVal14<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> del)
	{
		Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12, _13);
	}

	private static TResult InvokeTypeVal14<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, T13 _13, TypeVal14<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> del)
	{
		return Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12, _13);
	}

	private static void InvokeVoidRef14<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, T13 _13, VoidRef14<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> del)
	{
		Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12, _13);
	}

	private static TResult InvokeTypeRef14<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, T13 _13, TypeRef14<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> del)
	{
		return Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12, _13);
	}

	private static void InvokeVoidVal15<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, T13 _13, T14 _14, VoidVal15<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> del)
	{
		Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12, _13, _14);
	}

	private static TResult InvokeTypeVal15<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, T13 _13, T14 _14, TypeVal15<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> del)
	{
		return Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12, _13, _14);
	}

	private static void InvokeVoidRef15<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, T13 _13, T14 _14, VoidRef15<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> del)
	{
		Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12, _13, _14);
	}

	private static TResult InvokeTypeRef15<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, T13 _13, T14 _14, TypeRef15<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> del)
	{
		return Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12, _13, _14);
	}

	private static void InvokeVoidVal16<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, T13 _13, T14 _14, T15 _15, VoidVal16<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> del)
	{
		Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12, _13, _14, _15);
	}

	private static TResult InvokeTypeVal16<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, T13 _13, T14 _14, T15 _15, TypeVal16<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> del)
	{
		return Helpers.ThrowIfNull(del, "del")(_0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12, _13, _14, _15);
	}

	private static void InvokeVoidRef16<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, T13 _13, T14 _14, T15 _15, VoidRef16<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> del)
	{
		Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12, _13, _14, _15);
	}

	private static TResult InvokeTypeRef16<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(ref T0 _0, T1 _1, T2 _2, T3 _3, T4 _4, T5 _5, T6 _6, T7 _7, T8 _8, T9 _9, T10 _10, T11 _11, T12 _12, T13 _13, T14 _14, T15 _15, TypeRef16<TResult, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> del)
	{
		return Helpers.ThrowIfNull(del, "del")(ref _0, _1, _2, _3, _4, _5, _6, _7, _8, _9, _10, _11, _12, _13, _14, _15);
	}
}
