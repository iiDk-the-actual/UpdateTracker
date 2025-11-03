using System;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class HierarchyFlattenerReparentXform : MonoBehaviour
{
	protected void Awake()
	{
		if (base.enabled)
		{
			this._DoIt();
		}
	}

	protected void OnEnable()
	{
		this._DoIt();
	}

	private void _DoIt()
	{
		if (this._didIt)
		{
			return;
		}
		if (this.newParent != null)
		{
			base.transform.SetParent(this.newParent, true);
		}
		Object.Destroy(this);
	}

	public Transform newParent;

	private bool _didIt;
}
