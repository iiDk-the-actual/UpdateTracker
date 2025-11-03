using System;
using UnityEngine;

public class Example : MonoBehaviour
{
	private void OnDrawGizmos()
	{
		if (this.debugPoint)
		{
			DebugExtension.DrawPoint(this.debugPoint_Position, this.debugPoint_Color, this.debugPoint_Scale);
		}
		if (this.debugBounds)
		{
			DebugExtension.DrawBounds(new Bounds(new Vector3(10f, 0f, 0f), this.debugBounds_Size), this.debugBounds_Color);
		}
		if (this.debugCircle)
		{
			DebugExtension.DrawCircle(new Vector3(20f, 0f, 0f), this.debugCircle_Up, this.debugCircle_Color, this.debugCircle_Radius);
		}
		if (this.debugWireSphere)
		{
			Gizmos.color = this.debugWireSphere_Color;
			Gizmos.DrawWireSphere(new Vector3(30f, 0f, 0f), this.debugWireSphere_Radius);
		}
		if (this.debugCylinder)
		{
			DebugExtension.DrawCylinder(new Vector3(40f, 0f, 0f), this.debugCylinder_End, this.debugCylinder_Color, this.debugCylinder_Radius);
		}
		if (this.debugCone)
		{
			DebugExtension.DrawCone(new Vector3(50f, 0f, 0f), this.debugCone_Direction, this.debugCone_Color, this.debugCone_Angle);
		}
		if (this.debugArrow)
		{
			DebugExtension.DrawArrow(new Vector3(60f, 0f, 0f), this.debugArrow_Direction, this.debugArrow_Color);
		}
		if (this.debugCapsule)
		{
			DebugExtension.DrawCapsule(new Vector3(70f, 0f, 0f), this.debugCapsule_End, this.debugCapsule_Color, this.debugCapsule_Radius);
		}
	}

	private void Update()
	{
		DebugExtension.DebugPoint(this.debugPoint_Position, this.debugPoint_Color, this.debugPoint_Scale, 0f, true);
		DebugExtension.DebugBounds(new Bounds(new Vector3(10f, 0f, 0f), this.debugBounds_Size), this.debugBounds_Color, 0f, true);
		DebugExtension.DebugCircle(new Vector3(20f, 0f, 0f), this.debugCircle_Up, this.debugCircle_Color, this.debugCircle_Radius, 0f, true);
		DebugExtension.DebugWireSphere(new Vector3(30f, 0f, 0f), this.debugWireSphere_Color, this.debugWireSphere_Radius, 0f, true);
		DebugExtension.DebugCylinder(new Vector3(40f, 0f, 0f), this.debugCylinder_End, this.debugCylinder_Color, this.debugCylinder_Radius, 0f, true);
		DebugExtension.DebugCone(new Vector3(50f, 0f, 0f), this.debugCone_Direction, this.debugCone_Color, this.debugCone_Angle, 0f, true);
		DebugExtension.DebugArrow(new Vector3(60f, 0f, 0f), this.debugArrow_Direction, this.debugArrow_Color, 0f, true);
		DebugExtension.DebugCapsule(new Vector3(70f, 0f, 0f), this.debugCapsule_End, this.debugCapsule_Color, this.debugCapsule_Radius, 0f, true);
	}

	public bool debugPoint;

	public Vector3 debugPoint_Position;

	public float debugPoint_Scale;

	public Color debugPoint_Color;

	public bool debugBounds;

	public Vector3 debugBounds_Position;

	public Vector3 debugBounds_Size;

	public Color debugBounds_Color;

	public bool debugCircle;

	public Vector3 debugCircle_Up;

	public float debugCircle_Radius;

	public Color debugCircle_Color;

	public bool debugWireSphere;

	public float debugWireSphere_Radius;

	public Color debugWireSphere_Color;

	public bool debugCylinder;

	public Vector3 debugCylinder_End;

	public float debugCylinder_Radius;

	public Color debugCylinder_Color;

	public bool debugCone;

	public Vector3 debugCone_Direction;

	public float debugCone_Angle;

	public Color debugCone_Color;

	public bool debugArrow;

	public Vector3 debugArrow_Direction;

	public Color debugArrow_Color;

	public bool debugCapsule;

	public Vector3 debugCapsule_End;

	public float debugCapsule_Radius;

	public Color debugCapsule_Color;
}
