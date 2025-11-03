using System;
using System.Collections.Generic;
using GorillaTagScripts;
using Photon.Pun;
using UnityEngine;

public class BuilderShelf : MonoBehaviour
{
	public void Init()
	{
		this.shelfSlot = 0;
		this.buildPieceSpawnIndex = 0;
		this.spawnCount = 0;
		this.count = 0;
		this.spawnCosts = new List<BuilderResources>(this.buildPieceSpawns.Count);
		for (int i = 0; i < this.buildPieceSpawns.Count; i++)
		{
			this.count += this.buildPieceSpawns[i].count;
			BuilderPiece component = this.buildPieceSpawns[i].buildPiecePrefab.GetComponent<BuilderPiece>();
			this.spawnCosts.Add(component.cost);
		}
	}

	public bool HasOpenSlot()
	{
		return this.shelfSlot < this.count;
	}

	public void BuildNextPiece(BuilderTable table)
	{
		if (!this.HasOpenSlot())
		{
			return;
		}
		BuilderShelf.BuildPieceSpawn buildPieceSpawn = this.buildPieceSpawns[this.buildPieceSpawnIndex];
		BuilderResources builderResources = this.spawnCosts[this.buildPieceSpawnIndex];
		while (!table.HasEnoughUnreservedResources(builderResources) && this.buildPieceSpawnIndex < this.buildPieceSpawns.Count - 1)
		{
			int num = buildPieceSpawn.count - this.spawnCount;
			this.shelfSlot += num;
			this.spawnCount = 0;
			this.buildPieceSpawnIndex++;
			buildPieceSpawn = this.buildPieceSpawns[this.buildPieceSpawnIndex];
			builderResources = this.spawnCosts[this.buildPieceSpawnIndex];
		}
		if (!table.HasEnoughUnreservedResources(builderResources))
		{
			int num2 = buildPieceSpawn.count - this.spawnCount;
			this.shelfSlot += num2;
			this.spawnCount = 0;
			return;
		}
		int staticHash = buildPieceSpawn.buildPiecePrefab.name.GetStaticHash();
		int num3 = (string.IsNullOrEmpty(buildPieceSpawn.materialID) ? (-1) : buildPieceSpawn.materialID.GetHashCode());
		Vector3 vector;
		Quaternion quaternion;
		this.GetSpawnLocation(this.shelfSlot, buildPieceSpawn, out vector, out quaternion);
		int num4 = table.CreatePieceId();
		table.CreatePiece(staticHash, num4, vector, quaternion, num3, BuilderPiece.State.OnShelf, PhotonNetwork.LocalPlayer);
		this.spawnCount++;
		this.shelfSlot++;
		if (this.spawnCount >= buildPieceSpawn.count)
		{
			this.buildPieceSpawnIndex++;
			this.spawnCount = 0;
		}
	}

	public void InitCount()
	{
		this.count = 0;
		for (int i = 0; i < this.buildPieceSpawns.Count; i++)
		{
			this.count += this.buildPieceSpawns[i].count;
		}
	}

	public void BuildItems(BuilderTable table)
	{
		int num = 0;
		this.InitCount();
		for (int i = 0; i < this.buildPieceSpawns.Count; i++)
		{
			BuilderShelf.BuildPieceSpawn buildPieceSpawn = this.buildPieceSpawns[i];
			if (buildPieceSpawn != null && buildPieceSpawn.count != 0)
			{
				int staticHash = buildPieceSpawn.buildPiecePrefab.name.GetStaticHash();
				int num2 = (string.IsNullOrEmpty(buildPieceSpawn.materialID) ? (-1) : buildPieceSpawn.materialID.GetHashCode());
				int num3 = 0;
				while (num3 < buildPieceSpawn.count && num < this.count)
				{
					Vector3 vector;
					Quaternion quaternion;
					this.GetSpawnLocation(num, buildPieceSpawn, out vector, out quaternion);
					int num4 = table.CreatePieceId();
					table.CreatePiece(staticHash, num4, vector, quaternion, num2, BuilderPiece.State.OnShelf, PhotonNetwork.LocalPlayer);
					num++;
					num3++;
				}
			}
		}
	}

	public void GetSpawnLocation(int slot, BuilderShelf.BuildPieceSpawn spawn, out Vector3 spawnPosition, out Quaternion spawnRotation)
	{
		if (this.center == null)
		{
			this.center = base.transform;
		}
		Vector3 vector = spawn.positionOffset;
		Vector3 vector2 = spawn.rotationOffset;
		BuilderPiece component = spawn.buildPiecePrefab.GetComponent<BuilderPiece>();
		if (component != null)
		{
			vector = component.desiredShelfOffset;
			vector2 = component.desiredShelfRotationOffset;
		}
		spawnRotation = this.center.rotation * Quaternion.Euler(vector2);
		float num = (float)slot * this.separation - (float)(this.count - 1) * this.separation / 2f;
		spawnPosition = this.center.position + this.center.rotation * (spawn.localAxis * num + vector);
	}

	private int count;

	public float separation;

	public Transform center;

	public Material overrideMaterial;

	public List<BuilderShelf.BuildPieceSpawn> buildPieceSpawns;

	private List<BuilderResources> spawnCosts;

	private int shelfSlot;

	private int buildPieceSpawnIndex;

	private int spawnCount;

	[Serializable]
	public class BuildPieceSpawn
	{
		public GameObject buildPiecePrefab;

		public string materialID;

		public int count = 1;

		public Vector3 localAxis = Vector3.right;

		[Tooltip("Use BuilderPiece:desiredShelfOffset instead")]
		public Vector3 positionOffset;

		[Tooltip("Use BuilderPiece:desiredShelfRotationOffset instead")]
		public Vector3 rotationOffset;

		[Tooltip("Optional Editor Visual")]
		public Mesh previewMesh;
	}
}
