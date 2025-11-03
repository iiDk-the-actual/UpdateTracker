using System;
using GorillaLocomotion.Gameplay;
using Photon.Pun;
using UnityEngine;

public class SizeLayerChangerGrabable : MonoBehaviour, IGorillaGrabable
{
	public bool MomentaryGrabOnly()
	{
		return this.momentaryGrabOnly;
	}

	bool IGorillaGrabable.CanBeGrabbed(GorillaGrabber grabber)
	{
		return true;
	}

	void IGorillaGrabable.OnGrabbed(GorillaGrabber g, out Transform grabbedObject, out Vector3 grabbedLocalPosiiton)
	{
		if (this.grabChangesSizeLayer)
		{
			RigContainer rigContainer;
			VRRigCache.Instance.TryGetVrrig(PhotonNetwork.LocalPlayer, out rigContainer);
			rigContainer.Rig.sizeManager.currentSizeLayerMaskValue = this.grabbedSizeLayerMask.Mask;
		}
		grabbedObject = base.transform;
		grabbedLocalPosiiton = base.transform.InverseTransformPoint(g.transform.position);
	}

	void IGorillaGrabable.OnGrabReleased(GorillaGrabber g)
	{
		if (this.releaseChangesSizeLayer)
		{
			RigContainer rigContainer;
			VRRigCache.Instance.TryGetVrrig(PhotonNetwork.LocalPlayer, out rigContainer);
			rigContainer.Rig.sizeManager.currentSizeLayerMaskValue = this.releasedSizeLayerMask.Mask;
		}
	}

	string IGorillaGrabable.get_name()
	{
		return base.name;
	}

	[SerializeField]
	private bool grabChangesSizeLayer = true;

	[SerializeField]
	private bool releaseChangesSizeLayer = true;

	[SerializeField]
	private SizeLayerMask grabbedSizeLayerMask;

	[SerializeField]
	private SizeLayerMask releasedSizeLayerMask;

	[SerializeField]
	private bool momentaryGrabOnly = true;
}
