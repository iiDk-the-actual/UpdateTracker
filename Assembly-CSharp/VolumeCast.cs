using System;
using System.Collections.Generic;
using Drawing;
using GorillaTag;
using UnityEngine;

public class VolumeCast : MonoBehaviourGizmos
{
	public bool CheckOverlaps()
	{
		Transform transform = base.transform;
		Vector3 lossyScale = transform.lossyScale;
		Quaternion rotation = transform.rotation;
		int num = (int)this.physicsMask;
		QueryTriggerInteraction queryTriggerInteraction = (this.includeTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore);
		Vector3 vector;
		Vector3 vector2;
		float num2;
		VolumeCast.GetEndsAndRadius(transform, this.center, this.height, this.radius, out vector, out vector2, out num2);
		VolumeCast.VolumeShape volumeShape = this.shape;
		Vector3 vector3;
		Vector3 vector4;
		if (volumeShape != VolumeCast.VolumeShape.Box)
		{
			if (volumeShape != VolumeCast.VolumeShape.Cylinder)
			{
				return false;
			}
			vector3 = (vector + vector2) * 0.5f;
			vector4 = new Vector3(num2, Vector3.Distance(vector, vector2) * 0.5f, num2);
		}
		else
		{
			vector3 = transform.TransformPoint(this.center);
			vector4 = Vector3.Scale(lossyScale, this.size * 0.5f).Abs();
		}
		Array.Clear(this._boxOverlaps, 0, 8);
		this._boxHits = Physics.OverlapBoxNonAlloc(vector3, vector4, this._boxOverlaps, rotation, num, queryTriggerInteraction);
		if (this.shape != VolumeCast.VolumeShape.Cylinder)
		{
			return this._colliding = this._boxHits > 0;
		}
		this._hits = 0;
		Array.Clear(this._capOverlaps, 0, 8);
		Array.Clear(this._overlaps, 0, 8);
		this._capHits = Physics.OverlapCapsuleNonAlloc(vector, vector2, num2, this._capOverlaps, num, queryTriggerInteraction);
		this._set.Clear();
		int num3 = Math.Max(this._capHits, this._boxHits);
		Collider[] array = ((this._capHits < this._boxHits) ? this._capOverlaps : this._boxOverlaps);
		Collider[] array2 = ((this._capHits < this._boxHits) ? this._boxOverlaps : this._capOverlaps);
		for (int i = 0; i < num3; i++)
		{
			Collider collider = array[i];
			if (collider && !this._set.Add(collider))
			{
				Collider[] overlaps = this._overlaps;
				int num4 = this._hits;
				this._hits = num4 + 1;
				overlaps[num4] = collider;
			}
			Collider collider2 = array2[i];
			if (collider2 && !this._set.Add(collider2))
			{
				Collider[] overlaps2 = this._overlaps;
				int num4 = this._hits;
				this._hits = num4 + 1;
				overlaps2[num4] = collider2;
			}
		}
		return this._colliding = this._hits > 0;
	}

	private static void GetEndsAndRadius(Transform t, Vector3 center, float height, float radius, out Vector3 a, out Vector3 b, out float r)
	{
		float num = height * 0.5f;
		Vector3 lossyScale = t.lossyScale;
		a = t.TransformPoint(center + Vector3.down * num);
		b = t.TransformPoint(center + Vector3.up * num);
		r = Math.Max(Math.Abs(lossyScale.x), Math.Abs(lossyScale.z)) * radius;
	}

	public VolumeCast.VolumeShape shape;

	[Space]
	public Vector3 center;

	public Vector3 size = Vector3.one;

	public float height = 1f;

	public float radius = 1f;

	private const int MAX_HITS = 8;

	[Space]
	public UnityLayerMask physicsMask = UnityLayerMask.Everything;

	public bool includeTriggers;

	[Space]
	[SerializeField]
	private bool _simulateInEditMode;

	[DebugReadout]
	[NonSerialized]
	private int _capHits;

	[DebugReadout]
	[NonSerialized]
	private Collider[] _capOverlaps = new Collider[8];

	[DebugReadout]
	[NonSerialized]
	private int _boxHits;

	[DebugReadout]
	[NonSerialized]
	private Collider[] _boxOverlaps = new Collider[8];

	[DebugReadout]
	[NonSerialized]
	private int _hits;

	[DebugReadout]
	[NonSerialized]
	private Collider[] _overlaps = new Collider[8];

	[DebugReadout]
	[NonSerialized]
	private bool _colliding;

	[NonSerialized]
	private HashSet<Collider> _set = new HashSet<Collider>(8);

	public enum VolumeShape
	{
		Box,
		Cylinder
	}
}
