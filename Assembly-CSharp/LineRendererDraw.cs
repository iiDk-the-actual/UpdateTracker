using System;
using UnityEngine;

public class LineRendererDraw : MonoBehaviour
{
	public void SetUpLine(Transform[] points)
	{
		this.lr.positionCount = points.Length;
		this.points = points;
	}

	private void LateUpdate()
	{
		for (int i = 0; i < this.points.Length; i++)
		{
			this.lr.SetPosition(i, this.points[i].position);
		}
	}

	public void Enable(bool enable)
	{
		this.lr.enabled = enable;
	}

	public LineRenderer lr;

	public Transform[] points;
}
