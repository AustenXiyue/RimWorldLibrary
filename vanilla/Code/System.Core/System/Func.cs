namespace System;

/// <summary>Encapsulates a method that has nine parameters and returns a value of the type specified by the <paramref name="TResult" /> parameter.</summary>
/// <returns>The return value of the method that this delegate encapsulates.</returns>
/// <param name="arg1">The first parameter of the method that this delegate encapsulates.</param>
/// <param name="arg2">The second parameter of the method that this delegate encapsulates.</param>
/// <param name="arg3">The third parameter of the method that this delegate encapsulates.</param>
/// <param name="arg4">The fourth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg5">The fifth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg6">The sixth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg7">The seventh parameter of the method that this delegate encapsulates.</param>
/// <param name="arg8">The eighth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg9">The ninth parameter of the method that this delegate encapsulates.</param>
/// <typeparam name="T1">The type of the first parameter of the method that this delegate encapsulates.This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
/// <typeparam name="T2">The type of the second parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T3">The type of the third parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T4">The type of the fourth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T5">The type of the fifth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T6">The type of the sixth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T7">The type of the seventh parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T8">The type of the eighth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T9">The type of the ninth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="TResult">The type of the return value of the method that this delegate encapsulates.This type parameter is covariant. That is, you can use either the type you specified or any type that is more derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
/// <filterpriority>2</filterpriority>
public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);
/// <summary>Encapsulates a method that has 10 parameters and returns a value of the type specified by the <paramref name="TResult" /> parameter.</summary>
/// <returns>The return value of the method that this delegate encapsulates.</returns>
/// <param name="arg1">The first parameter of the method that this delegate encapsulates.</param>
/// <param name="arg2">The second parameter of the method that this delegate encapsulates.</param>
/// <param name="arg3">The third parameter of the method that this delegate encapsulates.</param>
/// <param name="arg4">The fourth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg5">The fifth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg6">The sixth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg7">The seventh parameter of the method that this delegate encapsulates.</param>
/// <param name="arg8">The eighth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg9">The ninth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg10">The tenth parameter of the method that this delegate encapsulates.</param>
/// <typeparam name="T1">The type of the first parameter of the method that this delegate encapsulates.This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
/// <typeparam name="T2">The type of the second parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T3">The type of the third parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T4">The type of the fourth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T5">The type of the fifth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T6">The type of the sixth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T7">The type of the seventh parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T8">The type of the eighth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T9">The type of the ninth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T10">The type of the tenth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="TResult">The type of the return value of the method that this delegate encapsulates.This type parameter is covariant. That is, you can use either the type you specified or any type that is more derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
/// <filterpriority>2</filterpriority>
public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);
/// <summary>Encapsulates a method that has 11 parameters and returns a value of the type specified by the <paramref name="TResult" /> parameter.</summary>
/// <returns>The return value of the method that this delegate encapsulates.</returns>
/// <param name="arg1">The first parameter of the method that this delegate encapsulates.</param>
/// <param name="arg2">The second parameter of the method that this delegate encapsulates.</param>
/// <param name="arg3">The third parameter of the method that this delegate encapsulates.</param>
/// <param name="arg4">The fourth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg5">The fifth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg6">The sixth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg7">The seventh parameter of the method that this delegate encapsulates.</param>
/// <param name="arg8">The eighth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg9">The ninth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg10">The tenth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg11">The eleventh parameter of the method that this delegate encapsulates.</param>
/// <typeparam name="T1">The type of the first parameter of the method that this delegate encapsulates.This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
/// <typeparam name="T2">The type of the second parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T3">The type of the third parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T4">The type of the fourth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T5">The type of the fifth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T6">The type of the sixth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T7">The type of the seventh parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T8">The type of the eighth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T9">The type of the ninth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T10">The type of the tenth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T11">The type of the eleventh parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="TResult">The type of the return value of the method that this delegate encapsulates.This type parameter is covariant. That is, you can use either the type you specified or any type that is more derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
/// <filterpriority>2</filterpriority>
public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11);
/// <summary>Encapsulates a method that has 12 parameters and returns a value of the type specified by the <paramref name="TResult" /> parameter.</summary>
/// <returns>The return value of the method that this delegate encapsulates.</returns>
/// <param name="arg1">The first parameter of the method that this delegate encapsulates.</param>
/// <param name="arg2">The second parameter of the method that this delegate encapsulates.</param>
/// <param name="arg3">The third parameter of the method that this delegate encapsulates.</param>
/// <param name="arg4">The fourth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg5">The fifth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg6">The sixth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg7">The seventh parameter of the method that this delegate encapsulates.</param>
/// <param name="arg8">The eighth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg9">The ninth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg10">The tenth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg11">The eleventh parameter of the method that this delegate encapsulates.</param>
/// <param name="arg12">The twelfth parameter of the method that this delegate encapsulates.</param>
/// <typeparam name="T1">The type of the first parameter of the method that this delegate encapsulates.This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
/// <typeparam name="T2">The type of the second parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T3">The type of the third parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T4">The type of the fourth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T5">The type of the fifth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T6">The type of the sixth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T7">The type of the seventh parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T8">The type of the eighth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T9">The type of the ninth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T10">The type of the tenth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T11">The type of the eleventh parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T12">The type of the twelfth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="TResult">The type of the return value of the method that this delegate encapsulates.This type parameter is covariant. That is, you can use either the type you specified or any type that is more derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
/// <filterpriority>2</filterpriority>
public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12);
/// <summary>Encapsulates a method that has 13 parameters and returns a value of the type specified by the <paramref name="TResult" /> parameter.</summary>
/// <returns>The return value of the method that this delegate encapsulates.</returns>
/// <param name="arg1">The first parameter of the method that this delegate encapsulates.</param>
/// <param name="arg2">The second parameter of the method that this delegate encapsulates.</param>
/// <param name="arg3">The third parameter of the method that this delegate encapsulates.</param>
/// <param name="arg4">The fourth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg5">The fifth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg6">The sixth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg7">The seventh parameter of the method that this delegate encapsulates.</param>
/// <param name="arg8">The eighth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg9">The ninth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg10">The tenth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg11">The eleventh parameter of the method that this delegate encapsulates.</param>
/// <param name="arg12">The twelfth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg13">The thirteenth parameter of the method that this delegate encapsulates.</param>
/// <typeparam name="T1">The type of the first parameter of the method that this delegate encapsulates.This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
/// <typeparam name="T2">The type of the second parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T3">The type of the third parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T4">The type of the fourth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T5">The type of the fifth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T6">The type of the sixth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T7">The type of the seventh parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T8">The type of the eighth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T9">The type of the ninth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T10">The type of the tenth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T11">The type of the eleventh parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T12">The type of the twelfth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T13">The type of the thirteenth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="TResult">The type of the return value of the method that this delegate encapsulates.This type parameter is covariant. That is, you can use either the type you specified or any type that is more derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
/// <filterpriority>2</filterpriority>
public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13);
/// <summary>Encapsulates a method that has 14 parameters and returns a value of the type specified by the <paramref name="TResult" /> parameter.</summary>
/// <returns>The return value of the method that this delegate encapsulates.</returns>
/// <param name="arg1">The first parameter of the method that this delegate encapsulates.</param>
/// <param name="arg2">The second parameter of the method that this delegate encapsulates.</param>
/// <param name="arg3">The third parameter of the method that this delegate encapsulates.</param>
/// <param name="arg4">The fourth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg5">The fifth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg6">The sixth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg7">The seventh parameter of the method that this delegate encapsulates.</param>
/// <param name="arg8">The eighth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg9">The ninth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg10">The tenth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg11">The eleventh parameter of the method that this delegate encapsulates.</param>
/// <param name="arg12">The twelfth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg13">The thirteenth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg14">The fourteenth parameter of the method that this delegate encapsulates.</param>
/// <typeparam name="T1">The type of the first parameter of the method that this delegate encapsulates.This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
/// <typeparam name="T2">The type of the second parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T3">The type of the third parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T4">The type of the fourth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T5">The type of the fifth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T6">The type of the sixth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T7">The type of the seventh parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T8">The type of the eighth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T9">The type of the ninth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T10">The type of the tenth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T11">The type of the eleventh parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T12">The type of the twelfth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T13">The type of the thirteenth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T14">The type of the fourteenth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="TResult">The type of the return value of the method that this delegate encapsulates.This type parameter is covariant. That is, you can use either the type you specified or any type that is more derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
/// <filterpriority>2</filterpriority>
public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14);
/// <summary>Encapsulates a method that has 15 parameters and returns a value of the type specified by the <paramref name="TResult" /> parameter.</summary>
/// <returns>The return value of the method that this delegate encapsulates.</returns>
/// <param name="arg1">The first parameter of the method that this delegate encapsulates.</param>
/// <param name="arg2">The second parameter of the method that this delegate encapsulates.</param>
/// <param name="arg3">The third parameter of the method that this delegate encapsulates.</param>
/// <param name="arg4">The fourth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg5">The fifth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg6">The sixth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg7">The seventh parameter of the method that this delegate encapsulates.</param>
/// <param name="arg8">The eighth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg9">The ninth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg10">The tenth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg11">The eleventh parameter of the method that this delegate encapsulates.</param>
/// <param name="arg12">The twelfth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg13">The thirteenth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg14">The fourteenth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg15">The fifteenth parameter of the method that this delegate encapsulates.</param>
/// <typeparam name="T1">The type of the first parameter of the method that this delegate encapsulates.This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
/// <typeparam name="T2">The type of the second parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T3">The type of the third parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T4">The type of the fourth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T5">The type of the fifth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T6">The type of the sixth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T7">The type of the seventh parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T8">The type of the eighth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T9">The type of the ninth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T10">The type of the tenth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T11">The type of the eleventh parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T12">The type of the twelfth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T13">The type of the thirteenth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T14">The type of the fourteenth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T15">The type of the fifteenth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="TResult">The type of the return value of the method that this delegate encapsulates.This type parameter is covariant. That is, you can use either the type you specified or any type that is more derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
/// <filterpriority>2</filterpriority>
public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15);
/// <summary>Encapsulates a method that has 16 parameters and returns a value of the type specified by the <paramref name="TResult" /> parameter.</summary>
/// <returns>The return value of the method that this delegate encapsulates.</returns>
/// <param name="arg1">The first parameter of the method that this delegate encapsulates.</param>
/// <param name="arg2">The second parameter of the method that this delegate encapsulates.</param>
/// <param name="arg3">The third parameter of the method that this delegate encapsulates.</param>
/// <param name="arg4">The fourth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg5">The fifth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg6">The sixth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg7">The seventh parameter of the method that this delegate encapsulates.</param>
/// <param name="arg8">The eighth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg9">The ninth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg10">The tenth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg11">The eleventh parameter of the method that this delegate encapsulates.</param>
/// <param name="arg12">The twelfth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg13">The thirteenth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg14">The fourteenth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg15">The fifteenth parameter of the method that this delegate encapsulates.</param>
/// <param name="arg16">The sixteenth parameter of the method that this delegate encapsulates.</param>
/// <typeparam name="T1">The type of the first parameter of the method that this delegate encapsulates.This type parameter is contravariant. That is, you can use either the type you specified or any type that is less derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
/// <typeparam name="T2">The type of the second parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T3">The type of the third parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T4">The type of the fourth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T5">The type of the fifth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T6">The type of the sixth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T7">The type of the seventh parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T8">The type of the eighth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T9">The type of the ninth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T10">The type of the tenth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T11">The type of the eleventh parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T12">The type of the twelfth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T13">The type of the thirteenth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T14">The type of the fourteenth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T15">The type of the fifteenth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="T16">The type of the sixteenth parameter of the method that this delegate encapsulates.</typeparam>
/// <typeparam name="TResult">The type of the return value of the method that this delegate encapsulates.This type parameter is covariant. That is, you can use either the type you specified or any type that is more derived. For more information about covariance and contravariance, see Covariance and Contravariance in Generics.</typeparam>
/// <filterpriority>2</filterpriority>
public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15, in T16, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16);
