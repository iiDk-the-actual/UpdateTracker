using System;
using UnityEngine;

namespace GorillaTag
{
	[Serializable]
	public struct HashWrapper : IEquatable<int>
	{
		public HashWrapper(int hash = -1)
		{
			this.hashCode = hash;
		}

		public override int GetHashCode()
		{
			return this.hashCode;
		}

		public override bool Equals(object obj)
		{
			return this.hashCode.Equals(obj);
		}

		public bool Equals(int i)
		{
			return this.hashCode.Equals(i);
		}

		public static implicit operator int(in HashWrapper hash)
		{
			return hash.hashCode;
		}

		[SerializeField]
		private int hashCode;

		public const int NULL_HASH = -1;
	}
}
