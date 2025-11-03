using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

public static class UnsafeUtils
{
	public unsafe static readonly ref T[] GetInternalArray<T>(this List<T> list)
	{
		if (list == null)
		{
			return Unsafe.NullRef<T[]>();
		}
		return ref Unsafe.As<List<T>, StrongBox<T[]>>(ref list)->Value;
	}

	public unsafe static readonly ref T[] GetInvocationListUnsafe<T>(this T @delegate) where T : MulticastDelegate
	{
		if (@delegate == null)
		{
			return Unsafe.NullRef<T[]>();
		}
		return Unsafe.As<Delegate[], T[]>(ref Unsafe.As<T, UnsafeUtils._MultiDelegateFields>(ref @delegate)->delegates);
	}

	[StructLayout(LayoutKind.Sequential)]
	private class _MultiDelegateFields : UnsafeUtils._DelegateFields
	{
		public Delegate[] delegates;
	}

	[StructLayout(LayoutKind.Sequential)]
	private class _DelegateFields
	{
		public IntPtr method_ptr;

		public IntPtr invoke_impl;

		public object m_target;

		public IntPtr method;

		public IntPtr delegate_trampoline;

		public IntPtr extra_arg;

		public IntPtr method_code;

		public IntPtr interp_method;

		public IntPtr interp_invoke_impl;

		public MethodInfo method_info;

		public MethodInfo original_method_info;

		public UnsafeUtils._DelegateData data;

		public bool method_is_virtual;
	}

	[StructLayout(LayoutKind.Sequential)]
	private class _DelegateData
	{
		public Type target_type;

		public string method_name;

		public bool curried_first_arg;
	}
}
