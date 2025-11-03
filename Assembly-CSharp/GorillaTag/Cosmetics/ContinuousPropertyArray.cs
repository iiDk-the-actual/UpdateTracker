using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaTag.Cosmetics
{
	[Serializable]
	public class ContinuousPropertyArray
	{
		public int Count
		{
			get
			{
				return this.list.Length;
			}
		}

		private void InitIfNeeded()
		{
			if (this.initialized)
			{
				return;
			}
			this.initialized = true;
			this.inverseMaximum = 1f / this.maxExpectedValue;
			for (int i = 0; i < this.list.Length; i++)
			{
				this.list[i].Init();
			}
			if (Application.isPlaying)
			{
				for (int j = 0; j < this.list.Length; j++)
				{
					this.list[j].InitThreshold();
				}
			}
			this.uniqueShaderPropertyIndices = new List<int>();
			this.mpb = new MaterialPropertyBlock();
			ContinuousPropertyArray.PropertyComparer propertyComparer = new ContinuousPropertyArray.PropertyComparer();
			Array.Sort<ContinuousProperty>(this.list, propertyComparer);
			if (this.list[0].IsShaderProperty_Cached)
			{
				for (int k = 0; k < this.list.Length; k++)
				{
					if (!this.list[k].IsShaderProperty_Cached)
					{
						this.uniqueShaderPropertyIndices.Add(k);
						return;
					}
					if (k == this.list.Length - 1 || (k > 0 && propertyComparer.Compare(this.list[k - 1], this.list[k]) != 0))
					{
						this.uniqueShaderPropertyIndices.Add(k);
					}
				}
			}
		}

		public void ApplyAll(bool leftHand, float value)
		{
			this.ApplyAll(value);
		}

		public void ApplyAll(float f)
		{
			f *= this.inverseMaximum;
			if (this.list.Length == 0)
			{
				return;
			}
			this.InitIfNeeded();
			int num = int.MaxValue;
			if (this.uniqueShaderPropertyIndices.Count > 0)
			{
				num = 0;
				((Renderer)this.list[0].Target).GetPropertyBlock(this.mpb, this.list[0].IntValue);
			}
			for (int i = 0; i < this.list.Length; i++)
			{
				this.list[i].Apply(f, this.mpb);
				if (num < this.uniqueShaderPropertyIndices.Count && i >= this.uniqueShaderPropertyIndices[num] - 1)
				{
					((Renderer)this.list[i].Target).SetPropertyBlock(this.mpb, this.list[0].IntValue);
					if (++num < this.uniqueShaderPropertyIndices.Count)
					{
						((Renderer)this.list[i + 1].Target).GetPropertyBlock(this.mpb, this.list[i + 1].IntValue);
					}
				}
			}
		}

		[Tooltip("Divides the input value by this number before being fed into the property array. Unless you know what you're doing, you should probably leave this at 1. You can accomplish the same thing by changing the maximum X value for all the curves/gradients, this is just a shorthand.")]
		[SerializeField]
		private float maxExpectedValue = 1f;

		private float inverseMaximum;

		[SerializeField]
		private ContinuousProperty[] list;

		private List<int> uniqueShaderPropertyIndices;

		private MaterialPropertyBlock mpb;

		private bool initialized;

		private class PropertyComparer : IComparer<ContinuousProperty>
		{
			public int Compare(ContinuousProperty x, ContinuousProperty y)
			{
				if (!x.IsShaderProperty_Cached || !y.IsShaderProperty_Cached)
				{
					return y.IsShaderProperty_Cached.CompareTo(x.IsShaderProperty_Cached);
				}
				int num = x.GetTargetInstanceID() ^ x.IntValue;
				int num2 = y.GetTargetInstanceID() ^ y.IntValue;
				return num.CompareTo(num2);
			}
		}
	}
}
