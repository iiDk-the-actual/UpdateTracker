using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace GorillaTag
{
	[Serializable]
	public struct GTDirectAssetRef<T> : IEquatable<T> where T : Object
	{
		public T obj
		{
			get
			{
				return this._obj;
			}
			set
			{
				this._obj = value;
				this.edAssetPath = null;
			}
		}

		public GTDirectAssetRef(T theObj)
		{
			this._obj = theObj;
			this.edAssetPath = null;
		}

		public static implicit operator T(GTDirectAssetRef<T> refObject)
		{
			return refObject.obj;
		}

		public static implicit operator GTDirectAssetRef<T>(T other)
		{
			return new GTDirectAssetRef<T>
			{
				obj = other
			};
		}

		public bool Equals(T other)
		{
			return this.obj == other;
		}

		public override bool Equals(object other)
		{
			T t = other as T;
			return t != null && this.Equals(t);
		}

		public override int GetHashCode()
		{
			if (!(this.obj != null))
			{
				return 0;
			}
			return this.obj.GetHashCode();
		}

		public static bool operator ==(GTDirectAssetRef<T> left, T right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(GTDirectAssetRef<T> left, T right)
		{
			return !(left == right);
		}

		[SerializeField]
		[HideInInspector]
		internal T _obj;

		[FormerlySerializedAs("assetPath")]
		public string edAssetPath;
	}
}
