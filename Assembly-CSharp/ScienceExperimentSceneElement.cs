using System;
using GorillaTag;
using UnityEngine;

public class ScienceExperimentSceneElement : MonoBehaviour, ITickSystemPost
{
	bool ITickSystemPost.PostTickRunning { get; set; }

	void ITickSystemPost.PostTick()
	{
		base.transform.position = this.followElement.position;
		base.transform.rotation = this.followElement.rotation;
		base.transform.localScale = this.followElement.localScale;
	}

	private void Start()
	{
		this.followElement = ScienceExperimentManager.instance.GetElement(this.elementID);
		TickSystem<object>.AddPostTickCallback(this);
	}

	private void OnDestroy()
	{
		TickSystem<object>.RemovePostTickCallback(this);
	}

	public ScienceExperimentElementID elementID;

	private Transform followElement;
}
