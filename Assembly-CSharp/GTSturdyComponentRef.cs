using System;
using GorillaExtensions;
using UnityEngine;

[Serializable]
public struct GTSturdyComponentRef<T> where T : Component
{
	public Transform BaseXform
	{
		get
		{
			return this._baseXform;
		}
		set
		{
			this._baseXform = value;
		}
	}

	public T Value
	{
		get
		{
			if (!this._value)
			{
				return this._value;
			}
			if (string.IsNullOrEmpty(this._relativePath))
			{
				return default(T);
			}
			Transform transform;
			if (!this._baseXform.TryFindByPath(this._relativePath, out transform, false))
			{
				return default(T);
			}
			this._value = transform.GetComponent<T>();
			return this._value;
		}
		set
		{
			this._value = value;
			this._relativePath = ((!value) ? this._baseXform.GetRelativePath(value.transform) : string.Empty);
		}
	}

	public static implicit operator T(GTSturdyComponentRef<T> sturdyRef)
	{
		return sturdyRef.Value;
	}

	public static implicit operator GTSturdyComponentRef<T>(T component)
	{
		return new GTSturdyComponentRef<T>
		{
			Value = component
		};
	}

	[SerializeField]
	private T _value;

	[SerializeField]
	private string _relativePath;

	[SerializeField]
	private Transform _baseXform;
}
