using System;
using UnityEngine;

public class SyncToPlayerColor : MonoBehaviour
{
	protected virtual void Awake()
	{
		this.rig = base.GetComponentInParent<VRRig>();
		this._colorFunc = new Action<Color>(this.UpdateColor);
	}

	protected virtual void Start()
	{
		this.UpdateColor(this.rig.playerColor);
		this.rig.OnColorInitialized(this._colorFunc);
	}

	protected virtual void OnEnable()
	{
		this.rig.OnColorChanged += this._colorFunc;
	}

	protected virtual void OnDisable()
	{
		this.rig.OnColorChanged -= this._colorFunc;
	}

	public virtual void UpdateColor(Color color)
	{
		if (!this.target)
		{
			return;
		}
		if (this.colorPropertiesToSync == null)
		{
			return;
		}
		for (int i = 0; i < this.colorPropertiesToSync.Length; i++)
		{
			ShaderHashId shaderHashId = this.colorPropertiesToSync[i];
			this.target.SetColor(shaderHashId, color);
		}
	}

	public VRRig rig;

	public Material target;

	public ShaderHashId[] colorPropertiesToSync = new ShaderHashId[] { "_BaseColor" };

	private Action<Color> _colorFunc;
}
