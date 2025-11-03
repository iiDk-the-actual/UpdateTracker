using System;
using UnityEngine;

public class TransformFollowXScene : MonoBehaviour
{
	private void Awake()
	{
		this.prevPos = base.transform.position;
	}

	private void Start()
	{
		this.refToFollow.TryResolve<Transform>(out this.transformToFollow);
	}

	private void LateUpdate()
	{
		this.prevPos = base.transform.position;
		base.transform.rotation = this.transformToFollow.rotation;
		base.transform.position = this.transformToFollow.position + this.transformToFollow.rotation * this.offset;
	}

	public XSceneRef refToFollow;

	private Transform transformToFollow;

	public Vector3 offset;

	public Vector3 prevPos;
}
