using System;
using UnityEngine;

public class SkeletonPathingNode : MonoBehaviour
{
	private void Awake()
	{
		base.gameObject.SetActive(false);
	}

	public bool ejectionPoint;

	public SkeletonPathingNode[] connectedNodes;

	public float distanceToExitNode;
}
