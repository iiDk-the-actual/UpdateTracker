using System;
using UnityEngine;

public class OwnerRig : MonoBehaviour, IVariable<VRRig>, IVariable, IRigAware
{
	public void TryFindRig()
	{
		this._rig = base.GetComponentInParent<VRRig>();
		if (this._rig != null)
		{
			return;
		}
		this._rig = base.GetComponentInChildren<VRRig>();
	}

	public VRRig Get()
	{
		return this._rig;
	}

	public void Set(VRRig value)
	{
		this._rig = value;
	}

	public void Set(GameObject obj)
	{
		this._rig = ((obj != null) ? obj.GetComponentInParent<VRRig>() : null);
	}

	void IRigAware.SetRig(VRRig rig)
	{
		this._rig = rig;
	}

	public static implicit operator bool(OwnerRig or)
	{
		return or != null && !(or == null) && or._rig != null && !(or._rig == null);
	}

	public static implicit operator VRRig(OwnerRig or)
	{
		if (!or)
		{
			return null;
		}
		return or._rig;
	}

	[SerializeField]
	private VRRig _rig;
}
