using System;
using UnityEngine;

public class XfToXfLine : MonoBehaviour
{
	private void Awake()
	{
		this.lineRenderer = base.GetComponent<LineRenderer>();
	}

	private void Update()
	{
		this.lineRenderer.SetPosition(0, this.pt0.transform.position);
		this.lineRenderer.SetPosition(1, this.pt1.transform.position);
	}

	public Transform pt0;

	public Transform pt1;

	private LineRenderer lineRenderer;
}
