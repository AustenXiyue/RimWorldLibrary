using System;

namespace HarmonyLib;

[Obsolete("Use AccessTools.FieldRefAccess<T, S> for fields and AccessTools.MethodDelegate<Func<T, S>> for property getters")]
public delegate S GetterHandler<in T, out S>(T source);
