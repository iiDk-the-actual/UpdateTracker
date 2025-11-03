using System;
using System.Collections.Generic;
using UnityEngine;

public class BalloonString : MonoBehaviour, IGorillaSliceableSimple
{
	private void Awake()
	{
		this.lineRenderer = base.GetComponent<LineRenderer>();
		this.vertices = new List<Vector3>(this.numSegments + 1);
		if (this.startPositionXf != null && this.endPositionXf != null)
		{
			this.vertices.Add(this.startPositionXf.position);
			int num = this.vertices.Count - 2;
			for (int i = 0; i < num; i++)
			{
				float num2 = (float)((i + 1) / (this.vertices.Count - 1));
				Vector3 vector = Vector3.Lerp(this.startPositionXf.position, this.endPositionXf.position, num2);
				this.vertices.Add(vector);
			}
			this.vertices.Add(this.endPositionXf.position);
		}
	}

	private void UpdateDynamics()
	{
		this.vertices[0] = this.startPositionXf.position;
		this.vertices[this.vertices.Count - 1] = this.endPositionXf.position;
	}

	private void UpdateRenderPositions()
	{
		this.lineRenderer.SetPosition(0, this.startPositionXf.transform.position);
		this.lineRenderer.SetPosition(1, this.endPositionXf.transform.position);
	}

	public void OnEnable()
	{
		GorillaSlicerSimpleManager.RegisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
	}

	public void OnDisable()
	{
		GorillaSlicerSimpleManager.UnregisterSliceable(this, GorillaSlicerSimpleManager.UpdateStep.LateUpdate);
	}

	public void SliceUpdate()
	{
		if (this.startPositionXf != null && this.endPositionXf != null)
		{
			this.UpdateDynamics();
			this.UpdateRenderPositions();
		}
	}

	public Transform startPositionXf;

	public Transform endPositionXf;

	private List<Vector3> vertices;

	public int numSegments = 1;

	private LineRenderer lineRenderer;
}
