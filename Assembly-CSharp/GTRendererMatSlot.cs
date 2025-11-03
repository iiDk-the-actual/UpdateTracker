using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public struct GTRendererMatSlot
{
	public bool isValid { readonly get; private set; }

	public bool TryInitialize()
	{
		this.isValid = this.renderer != null;
		if (!this.isValid)
		{
			return false;
		}
		List<Material> list;
		bool isValid;
		using (ListPool<Material>.Get(out list))
		{
			this.renderer.GetSharedMaterials(list);
			this.isValid = this.slot > 0 && this.slot < list.Count && list[this.slot] != null;
			isValid = this.isValid;
		}
		return isValid;
	}

	public Renderer renderer;

	public int slot;
}
