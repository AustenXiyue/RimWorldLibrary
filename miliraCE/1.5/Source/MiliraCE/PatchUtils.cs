using System;
using System.Linq.Expressions;
using System.Reflection;

namespace MiliraCE
{
    internal class PatchUtils
    {
        internal static Func<TObject, TValue> BuildGetterExpression<TObject, TValue>(FieldInfo fieldInfo)
        {
            return BuildGetterExpression<Func<TObject, TValue>>(typeof(TObject), fieldInfo);
        }

        internal static TFunc BuildGetterExpression<TFunc>(Type objectType, FieldInfo objectFieldInfo)
        {
            var paramObj = Expression.Parameter(objectType);
            var field = Expression.Field(paramObj, objectFieldInfo);
            return Expression.Lambda<TFunc>(field, paramObj).Compile();
        }

        internal static Action<TObject, TValue> BuildSetterExpression<TObject, TValue>(FieldInfo fieldInfo)
        {
            return BuildSetterExpression<Action<TObject, TValue>>(typeof(TObject), fieldInfo, typeof(TValue));
        }

        internal static TAction BuildSetterExpression<TAction>(Type objectType, FieldInfo objectFieldInfo, Type valueType)
        {
            var paramObj = Expression.Parameter(objectType);
            var field = Expression.Field(paramObj, objectFieldInfo);
            var paramValue = Expression.Parameter(valueType);
            var assignment = Expression.Assign(field, paramValue);
            return Expression.Lambda<TAction>(assignment, paramObj, paramValue).Compile();
        }
    }
}
