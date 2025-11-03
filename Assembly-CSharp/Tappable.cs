using System;
using System.Diagnostics;
using Photon.Pun;
using UnityEngine;

public class Tappable : MonoBehaviour
{
	public void Validate()
	{
		this.CalculateId(true);
	}

	protected virtual void OnEnable()
	{
		if (!this.useStaticId)
		{
			this.CalculateId(false);
		}
		TappableManager.Register(this);
	}

	protected virtual void OnDisable()
	{
		TappableManager.Unregister(this);
	}

	public virtual bool CanTap(bool isLeftHand)
	{
		return true;
	}

	public void OnTap(float tapStrength)
	{
		if (!NetworkSystem.Instance.InRoom)
		{
			return;
		}
		if (!this.manager)
		{
			return;
		}
		this.manager.photonView.RPC("SendOnTapRPC", RpcTarget.All, new object[] { this.tappableId, tapStrength });
	}

	public void OnGrab()
	{
		if (!NetworkSystem.Instance.InRoom)
		{
			return;
		}
		if (!this.manager)
		{
			return;
		}
		this.manager.photonView.RPC("SendOnGrabRPC", RpcTarget.All, new object[] { this.tappableId });
	}

	public void OnRelease()
	{
		if (!NetworkSystem.Instance.InRoom)
		{
			return;
		}
		if (!this.manager)
		{
			return;
		}
		this.manager.photonView.RPC("SendOnReleaseRPC", RpcTarget.All, new object[] { this.tappableId });
	}

	public virtual void OnTapLocal(float tapStrength, float tapTime, PhotonMessageInfoWrapped sender)
	{
	}

	public virtual void OnGrabLocal(float tapTime, PhotonMessageInfoWrapped sender)
	{
	}

	public virtual void OnReleaseLocal(float tapTime, PhotonMessageInfoWrapped sender)
	{
	}

	private void EdRecalculateId()
	{
		this.CalculateId(true);
	}

	private void CalculateId(bool force = false)
	{
		Transform transform = base.transform;
		int hashCode = TransformUtils.ComputePathHash(transform).ToId128().GetHashCode();
		int staticHash = base.GetType().Name.GetStaticHash();
		int hashCode2 = transform.position.QuantizedId128().GetHashCode();
		int num = StaticHash.Compute(hashCode, staticHash, hashCode2);
		if (this.useStaticId)
		{
			if (string.IsNullOrEmpty(this.staticId) || force)
			{
				int instanceID = transform.GetInstanceID();
				int num2 = StaticHash.Compute(num, instanceID);
				this.staticId = string.Format("#ID_{0:X8}", num2);
			}
			this.tappableId = this.staticId.GetStaticHash();
			return;
		}
		this.tappableId = (Application.isPlaying ? num : 0);
	}

	[Conditional("UNITY_EDITOR")]
	private void OnValidate()
	{
		this.CalculateId(false);
	}

	public int tappableId;

	public string staticId;

	public bool useStaticId;

	[Tooltip("If true, tap cooldown will be ignored.  Tapping will be allowed/disallowed based on result of CanTap()")]
	public bool overrideTapCooldown;

	[Space]
	public TappableManager manager;

	public RpcTarget rpcTarget;
}
