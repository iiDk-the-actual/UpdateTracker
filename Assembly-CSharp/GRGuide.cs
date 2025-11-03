using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GRGuide : MonoBehaviourTick
{
	private void Awake()
	{
		this.path = new NavMeshPath();
		this.showing = false;
		for (int i = 0; i < this.show.Count; i++)
		{
			this.show[i].SetActive(false);
		}
		this.hasPath = false;
		this.numPathCorners = 0;
		this.pathCorners = new Vector3[512];
		this.connectorCorners = new List<Vector3>(64);
	}

	public override void Tick()
	{
		bool flag = GRPlayer.Get(VRRig.LocalRig).State == GRPlayer.GRPlayerState.Ghost;
		Vector3 position = VRRig.LocalRig.transform.position;
		float sqrMagnitude = (position - base.transform.position).sqrMagnitude;
		if (flag && (!this.hasPath || sqrMagnitude > 36f))
		{
			this.hasPath = false;
			Vector3 vector;
			Quaternion quaternion;
			NavMeshHit navMeshHit;
			NavMeshHit navMeshHit2;
			if (GhostReactor.instance.levelGenerator.GetExitFromCurrentSection(position, out vector, out quaternion, this.connectorCorners) && NavMesh.SamplePosition(position, out navMeshHit, 5f, -1) && NavMesh.SamplePosition(vector, out navMeshHit2, 5f, -1) && NavMesh.CalculatePath(navMeshHit.position, navMeshHit2.position, -1, this.path) && this.path.status == NavMeshPathStatus.PathComplete)
			{
				this.numPathCorners = this.path.GetCornersNonAlloc(this.pathCorners);
				for (int i = this.connectorCorners.Count - 1; i >= 0; i--)
				{
					this.pathCorners[this.numPathCorners] = this.connectorCorners[i];
					this.numPathCorners++;
				}
				if (this.numPathCorners > 0)
				{
					base.transform.position = this.pathCorners[0];
					this.hasPath = true;
				}
			}
		}
		if (!flag)
		{
			this.hasPath = false;
		}
		if (this.showing != this.hasPath)
		{
			this.showing = this.hasPath;
			for (int j = 0; j < this.show.Count; j++)
			{
				this.show[j].SetActive(this.showing);
			}
			if (this.audioSource != null)
			{
				if (this.showing)
				{
					this.audioSource.Play();
				}
				else
				{
					this.audioSource.Stop();
				}
			}
		}
		if (this.hasPath)
		{
			int num;
			Vector3 closestPointOnPath = GRGuide.GetClosestPointOnPath(position, this.pathCorners, this.numPathCorners, out num);
			float num2 = 2.5f;
			Vector3 vector2 = closestPointOnPath;
			for (int k = num; k < this.numPathCorners; k++)
			{
				Vector3 vector3 = this.pathCorners[k] - vector2;
				float magnitude = vector3.magnitude;
				if (num2 <= magnitude)
				{
					vector2 += vector3 * (num2 / magnitude);
					break;
				}
				num2 -= magnitude;
				vector2 = this.pathCorners[k];
			}
			base.transform.position = vector2;
		}
	}

	private static Vector3 GetClosestPointOnPath(Vector3 pos, Vector3[] pathCorners, int numPathCorners, out int nextCorner)
	{
		nextCorner = 0;
		if (numPathCorners == 0)
		{
			return pos;
		}
		if (numPathCorners == 1)
		{
			return pathCorners[0];
		}
		float num = float.MaxValue;
		Vector3 vector = Vector3.zero;
		for (int i = 0; i < numPathCorners - 1; i++)
		{
			Vector3 vector2 = pathCorners[i];
			Vector3 vector3 = pathCorners[i + 1];
			Vector3 vector4 = GRGuide.ClosestPointOnLine(vector2, vector3, pos);
			float sqrMagnitude = (vector4 - pos).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				vector = vector4;
				nextCorner = i + 1;
			}
		}
		return vector;
	}

	public static Vector3 ClosestPointOnLine(Vector3 vA, Vector3 vB, Vector3 vPoint)
	{
		Vector3 vector = vPoint - vA;
		Vector3 normalized = (vB - vA).normalized;
		float num = Vector3.Distance(vA, vB);
		float num2 = Vector3.Dot(normalized, vector);
		if (num2 <= 0f)
		{
			return vA;
		}
		if (num2 >= num)
		{
			return vB;
		}
		Vector3 vector2 = normalized * num2;
		return vA + vector2;
	}

	public Transform tempTarget;

	public List<GameObject> show;

	public AudioSource audioSource;

	private bool showing;

	private bool hasPath;

	private NavMeshPath path;

	private int numPathCorners;

	private Vector3[] pathCorners;

	private List<Vector3> connectorCorners;
}
