using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public sealed class ComponentFunctionReference<TResult>
{
	public bool IsValid
	{
		get
		{
			return this._selection.component || !string.IsNullOrEmpty(this._selection.methodName);
		}
	}

	private IEnumerable<ValueDropdownItem<ComponentFunctionReference<TResult>.MethodRef>> GetMethodOptions()
	{
		if (this._target == null)
		{
			yield break;
		}
		yield return new ValueDropdownItem<ComponentFunctionReference<TResult>.MethodRef>("NONE", default(ComponentFunctionReference<TResult>.MethodRef));
		Type type = typeof(GameObject);
		BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		foreach (MethodInfo methodInfo in type.GetMethods(flags))
		{
			if (methodInfo.GetParameters().Length == 0 && methodInfo.ReturnType == typeof(TResult))
			{
				string text = type.Name + "/" + methodInfo.Name;
				yield return new ValueDropdownItem<ComponentFunctionReference<TResult>.MethodRef>(text, new ComponentFunctionReference<TResult>.MethodRef(this._target, methodInfo));
			}
		}
		MethodInfo[] array = null;
		foreach (Component comp in this._target.GetComponents<Component>())
		{
			type = comp.GetType();
			foreach (MethodInfo methodInfo2 in type.GetMethods(flags))
			{
				if (methodInfo2.GetParameters().Length == 0 && methodInfo2.ReturnType == typeof(TResult))
				{
					string text2 = type.Name + "/" + methodInfo2.Name;
					yield return new ValueDropdownItem<ComponentFunctionReference<TResult>.MethodRef>(text2, new ComponentFunctionReference<TResult>.MethodRef(comp, methodInfo2));
				}
			}
			array = null;
			comp = null;
		}
		Component[] array2 = null;
		yield break;
	}

	public TResult Invoke()
	{
		if (this._cached == null)
		{
			this.Cache();
		}
		if (this._cached == null)
		{
			return default(TResult);
		}
		return this._cached();
	}

	public void Cache()
	{
		this._cached = null;
		if (this._selection.component == null || string.IsNullOrEmpty(this._selection.methodName))
		{
			return;
		}
		MethodInfo method = this._selection.component.GetType().GetMethod(this._selection.methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, Type.EmptyTypes, null);
		if (method != null)
		{
			this._cached = (Func<TResult>)Delegate.CreateDelegate(typeof(Func<TResult>), this._selection.component, method);
		}
	}

	[SerializeField]
	private GameObject _target;

	[SerializeField]
	private ComponentFunctionReference<TResult>.MethodRef _selection;

	private Func<TResult> _cached;

	[Serializable]
	private struct MethodRef
	{
		public MethodRef(Object obj, MethodInfo m)
		{
			this.component = obj;
			this.methodName = m.Name;
		}

		public Object component;

		public string methodName;
	}
}
