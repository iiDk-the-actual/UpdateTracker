using System;
using System.Collections.Generic;
using System.Linq;
using GorillaExtensions;
using UnityEngine;

[DefaultExecutionOrder(1300)]
public class GTPosRotConstraintManager : MonoBehaviour
{
	protected void Awake()
	{
		if (GTPosRotConstraintManager.hasInstance && GTPosRotConstraintManager.instance != this)
		{
			Object.Destroy(this);
			return;
		}
		GTPosRotConstraintManager.SetInstance(this);
	}

	protected void OnDestroy()
	{
		if (GTPosRotConstraintManager.instance == this)
		{
			GTPosRotConstraintManager.hasInstance = false;
			GTPosRotConstraintManager.instance = null;
		}
	}

	public void InvokeConstraint(GorillaPosRotConstraint constraint, int index)
	{
		Transform source = constraint.source;
		Transform follower = constraint.follower;
		Vector3 vector = source.position + source.TransformVector(constraint.positionOffset);
		Quaternion quaternion = source.rotation * constraint.rotationOffset;
		follower.SetPositionAndRotation(vector, quaternion);
	}

	protected void LateUpdate()
	{
		if (this.constraintsToDisable.Count <= 0)
		{
			return;
		}
		for (int i = this.constraintsToDisable.Count - 1; i >= 0; i--)
		{
			for (int j = 0; j < this.constraintsToDisable[i].constraints.Length; j++)
			{
				Transform follower = this.constraintsToDisable[i].constraints[j].follower;
				if (this.originalParent.ContainsKey(follower))
				{
					follower.SetParent(this.originalParent[follower], true);
					follower.localRotation = this.originalRot[follower];
					follower.localPosition = this.originalOffset[follower];
					follower.localScale = this.originalScale[follower];
					this.InvokeConstraint(this.constraintsToDisable[i].constraints[j], i);
				}
			}
			this.constraintsToDisable.RemoveAt(i);
		}
	}

	public static void CreateManager()
	{
		GTPosRotConstraintManager gtposRotConstraintManager = new GameObject("GTPosRotConstraintManager").AddComponent<GTPosRotConstraintManager>();
		GTPosRotConstraintManager.constraints.Clear();
		GTPosRotConstraintManager.componentRanges.Clear();
		GTPosRotConstraintManager.SetInstance(gtposRotConstraintManager);
	}

	private static void SetInstance(GTPosRotConstraintManager manager)
	{
		GTPosRotConstraintManager.instance = manager;
		GTPosRotConstraintManager.hasInstance = true;
		GTPosRotConstraintManager.instance.originalParent = new Dictionary<Transform, Transform>();
		GTPosRotConstraintManager.instance.originalOffset = new Dictionary<Transform, Vector3>();
		GTPosRotConstraintManager.instance.originalScale = new Dictionary<Transform, Vector3>();
		GTPosRotConstraintManager.instance.originalRot = new Dictionary<Transform, Quaternion>();
		GTPosRotConstraintManager.instance.constraintsToDisable = new List<GTPosRotConstraints>();
		if (Application.isPlaying)
		{
			manager.transform.SetParent(null, false);
			Object.DontDestroyOnLoad(manager);
		}
	}

	public static void Register(GTPosRotConstraints component)
	{
		if (!GTPosRotConstraintManager.hasInstance)
		{
			GTPosRotConstraintManager.CreateManager();
		}
		int instanceID = component.GetInstanceID();
		if (GTPosRotConstraintManager.componentRanges.ContainsKey(instanceID))
		{
			return;
		}
		for (int i = 0; i < component.constraints.Length; i++)
		{
			if (!component.constraints[i].follower)
			{
				Debug.LogError("Cannot add constraints for GTPosRotConstraints component because the `follower` Transform is null " + string.Format("at index {0}. Path in scene: {1}", i, component.transform.GetPathQ()), component);
				return;
			}
			if (!component.constraints[i].source)
			{
				Debug.LogError("Cannot add constraints for GTPosRotConstraints component because the `source` Transform is null " + string.Format("at index {0}. Path in scene: {1}", i, component.transform.GetPathQ()), component);
				return;
			}
		}
		GTPosRotConstraintManager.Range range = new GTPosRotConstraintManager.Range
		{
			start = GTPosRotConstraintManager.constraints.Count,
			end = GTPosRotConstraintManager.constraints.Count + component.constraints.Length - 1
		};
		GTPosRotConstraintManager.componentRanges.Add(instanceID, range);
		GTPosRotConstraintManager.constraints.AddRange(component.constraints);
		if (GTPosRotConstraintManager.instance.constraintsToDisable.Contains(component))
		{
			GTPosRotConstraintManager.instance.constraintsToDisable.Remove(component);
		}
		for (int j = 0; j < component.constraints.Length; j++)
		{
			Transform follower = component.constraints[j].follower;
			if (GTPosRotConstraintManager.instance.originalParent.ContainsKey(follower))
			{
				component.constraints[j].follower.SetParent(GTPosRotConstraintManager.instance.originalParent[follower], true);
				follower.localRotation = GTPosRotConstraintManager.instance.originalRot[follower];
				follower.localPosition = GTPosRotConstraintManager.instance.originalOffset[follower];
				follower.localScale = GTPosRotConstraintManager.instance.originalScale[follower];
			}
			else
			{
				GTPosRotConstraintManager.instance.originalParent[follower] = follower.parent;
				GTPosRotConstraintManager.instance.originalRot[follower] = follower.localRotation;
				GTPosRotConstraintManager.instance.originalOffset[follower] = follower.localPosition;
				GTPosRotConstraintManager.instance.originalScale[follower] = follower.localScale;
			}
			GTPosRotConstraintManager.instance.InvokeConstraint(component.constraints[j], j);
			component.constraints[j].follower.SetParent(component.constraints[j].source);
		}
	}

	public static void Unregister(GTPosRotConstraints component)
	{
		int instanceID = component.GetInstanceID();
		GTPosRotConstraintManager.Range range;
		if (!GTPosRotConstraintManager.hasInstance || !GTPosRotConstraintManager.componentRanges.TryGetValue(instanceID, out range))
		{
			return;
		}
		GTPosRotConstraintManager.constraints.RemoveRange(range.start, 1 + range.end - range.start);
		GTPosRotConstraintManager.componentRanges.Remove(instanceID);
		foreach (int num in GTPosRotConstraintManager.componentRanges.Keys.ToArray<int>())
		{
			GTPosRotConstraintManager.Range range2 = GTPosRotConstraintManager.componentRanges[num];
			if (range2.start > range.end)
			{
				GTPosRotConstraintManager.componentRanges[num] = new GTPosRotConstraintManager.Range
				{
					start = range2.start - range.end + range.start - 1,
					end = range2.end - range.end + range.start - 1
				};
			}
		}
		if (!GTPosRotConstraintManager.instance.constraintsToDisable.Contains(component))
		{
			GTPosRotConstraintManager.instance.constraintsToDisable.Add(component);
		}
	}

	public static GTPosRotConstraintManager instance;

	public static bool hasInstance = false;

	private const int _kComponentsCapacity = 256;

	private const int _kConstraintsCapacity = 1024;

	[NonSerialized]
	public Dictionary<Transform, Transform> originalParent;

	[NonSerialized]
	public Dictionary<Transform, Vector3> originalOffset;

	[NonSerialized]
	public Dictionary<Transform, Vector3> originalScale;

	[NonSerialized]
	public Dictionary<Transform, Quaternion> originalRot;

	[NonSerialized]
	public List<GTPosRotConstraints> constraintsToDisable;

	[OnEnterPlay_Clear]
	private static readonly List<GorillaPosRotConstraint> constraints = new List<GorillaPosRotConstraint>(1024);

	[OnEnterPlay_Clear]
	public static readonly Dictionary<int, GTPosRotConstraintManager.Range> componentRanges = new Dictionary<int, GTPosRotConstraintManager.Range>(256);

	public struct Range
	{
		public int start;

		public int end;
	}
}
