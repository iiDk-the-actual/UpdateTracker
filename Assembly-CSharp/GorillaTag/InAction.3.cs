using System;

namespace GorillaTag
{
	public delegate void InAction<T1, T2, T3>(in T1 obj1, in T2 obj2, in T3 obj3);
}
