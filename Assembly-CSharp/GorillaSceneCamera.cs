using System;
using UnityEngine;

public class GorillaSceneCamera : MonoBehaviour
{
	public void SetSceneCamera(int sceneIndex)
	{
		base.transform.position = this.sceneTransforms[sceneIndex].scenePosition;
		base.transform.eulerAngles = this.sceneTransforms[sceneIndex].sceneRotation;
	}

	public GorillaSceneTransform[] sceneTransforms;
}
