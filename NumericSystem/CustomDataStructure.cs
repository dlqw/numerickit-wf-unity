using JetBrains.Annotations;
using UnityEngine;

namespace Framework.NumericSystem
{
    public abstract class CustomDataStructure
    {
        public static CustomDataStructure operator +(CustomDataStructure a, CustomDataStructure b)
        {
            Debug.Log("调用基类加法运算符");
            return a.Add(a, b);
        }

        protected abstract CustomDataStructure Add(CustomDataStructure a, CustomDataStructure b);

        public static CustomDataStructure operator -(CustomDataStructure a, CustomDataStructure b)
        {
            Debug.Log("调用基类减法运算符");
            return a.Sub(a, b);
        }

        protected abstract CustomDataStructure Sub(CustomDataStructure a, CustomDataStructure b);

        public static CustomDataStructure operator *(CustomDataStructure a, CustomDataStructure b)
        {
            Debug.Log("调用基类乘法运算符");
            return a.Mul(a, b);
        }

        protected abstract CustomDataStructure Mul(CustomDataStructure a, CustomDataStructure b);

        public static CustomDataStructure operator /(CustomDataStructure a, CustomDataStructure b)
        {
            Debug.Log("调用基类除法运算符");
            return a.Div(a, b);
        }

        protected abstract CustomDataStructure Div(CustomDataStructure a, CustomDataStructure b);

        public static CustomDataStructure operator %(CustomDataStructure a, CustomDataStructure b)
        {
            Debug.Log("调用基类取模运算符");
            return a.Mod(a, b);
        }

        protected abstract CustomDataStructure Mod(CustomDataStructure a, CustomDataStructure b);

        public static CustomDataStructure operator ^(CustomDataStructure a, CustomDataStructure b)
        {
            Debug.Log("调用基类按位异或运算符");
            return a.Xor(a, b);
        }

        protected abstract CustomDataStructure Xor(CustomDataStructure a, CustomDataStructure b);

        public static CustomDataStructure operator <<(CustomDataStructure a, int b)
        {
            Debug.Log("调用基类左移运算符");
            return a.Shl(a, b);
        }

        protected abstract CustomDataStructure Shl(CustomDataStructure a, int b);

        public static CustomDataStructure operator >> (CustomDataStructure a, int b)
        {
            Debug.Log("调用基类右移运算符");
            return a.Shr(a, b);
        }

        protected abstract CustomDataStructure Shr(CustomDataStructure a, int b);

        public static CustomDataStructure operator ~(CustomDataStructure a)
        {
            Debug.Log("调用基类按位取反运算符");
            return a.Not(a);
        }

        protected abstract CustomDataStructure Not(CustomDataStructure a);

        public static CustomDataStructure operator ++(CustomDataStructure a)
        {
            Debug.Log("调用基类自增运算符");
            return a.Inc(a);
        }

        protected abstract CustomDataStructure Inc(CustomDataStructure a);

        public static CustomDataStructure operator --(CustomDataStructure a)
        {
            Debug.Log("调用基类自减运算符");
            return a.Dec(a);
        }

        protected abstract CustomDataStructure Dec(CustomDataStructure a);

        public static CustomDataStructure operator +(CustomDataStructure a)
        {
            Debug.Log("调用基类正运算符");
            return a.Pos(a);
        }

        protected abstract CustomDataStructure Pos(CustomDataStructure a);

        public static CustomDataStructure operator -(CustomDataStructure a)
        {
            Debug.Log("调用基类负运算符");
            return a.Neg(a);
        }

        protected abstract CustomDataStructure Neg(CustomDataStructure a);

        public static bool operator ==([CanBeNull] CustomDataStructure a, CustomDataStructure b)
        {
            Debug.Log("调用基类等于运算符");
            if (a == null || b == null)
                return a == b;
            return a.Eq(a, b);
        }

        protected abstract bool Eq(CustomDataStructure a, CustomDataStructure b);

        public static bool operator !=(CustomDataStructure a, CustomDataStructure b)
        {
            Debug.Log("调用基类不等于运算符");
            return !(a == b);
        }

        public abstract bool Ne(CustomDataStructure a, CustomDataStructure b);

        public static CustomDataStructure operator <(CustomDataStructure a, CustomDataStructure b)
        {
            Debug.Log("调用基类小于运算符");
            return a.Lt(a, b);
        }

        protected abstract CustomDataStructure Lt(CustomDataStructure a, CustomDataStructure b);

        public static CustomDataStructure operator >(CustomDataStructure a, CustomDataStructure b)
        {
            Debug.Log("调用基类大于运算符");
            return a.Gt(a, b);
        }

        protected abstract CustomDataStructure Gt(CustomDataStructure a, CustomDataStructure b);

        public static CustomDataStructure operator <=(CustomDataStructure a, CustomDataStructure b)
        {
            Debug.Log("调用基类小于等于运算符");
            return a.Le(a, b);
        }

        protected abstract CustomDataStructure Le(CustomDataStructure a, CustomDataStructure b);

        public static CustomDataStructure operator >=(CustomDataStructure a, CustomDataStructure b)
        {
            Debug.Log("调用基类大于等于运算符");
            return a.Ge(a, b);
        }

        protected abstract CustomDataStructure Ge(CustomDataStructure a, CustomDataStructure b);

        public static CustomDataStructure operator &(CustomDataStructure a, CustomDataStructure b)
        {
            Debug.Log("调用基类逻辑与运算符");
            return a.And(a, b);
        }

        protected abstract CustomDataStructure And(CustomDataStructure a, CustomDataStructure b);

        public static CustomDataStructure operator |(CustomDataStructure a, CustomDataStructure b)
        {
            Debug.Log("调用基类逻辑或运算符");
            return a.Or(a, b);
        }

        protected abstract CustomDataStructure Or(CustomDataStructure a, CustomDataStructure b);

        public static CustomDataStructure operator !(CustomDataStructure a)
        {
            Debug.Log("调用基类逻辑非运算符");
            return a.NotLogic(a);
        }

        protected abstract CustomDataStructure NotLogic(CustomDataStructure a);

        public static bool operator true(CustomDataStructure a)
        {
            Debug.Log("调用基类逻辑真运算符");
            return a.True(a);
        }

        protected abstract bool True(CustomDataStructure a);

        public static bool operator false(CustomDataStructure a)
        {
            Debug.Log("调用基类逻辑假运算符");
            return a.False(a);
        }

        protected abstract bool False(CustomDataStructure a);
    }
}