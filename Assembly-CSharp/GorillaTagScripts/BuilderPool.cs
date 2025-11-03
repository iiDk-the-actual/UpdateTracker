using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaTagScripts
{
	public class BuilderPool : MonoBehaviour
	{
		private void Awake()
		{
			if (BuilderPool.instance == null)
			{
				BuilderPool.instance = this;
				return;
			}
			Object.Destroy(this);
		}

		public void Setup()
		{
			if (this.isSetup)
			{
				return;
			}
			this.piecePools = new List<List<BuilderPiece>>(512);
			this.piecePoolLookup = new Dictionary<int, int>(512);
			this.bumpGlowPool = new List<BuilderBumpGlow>(256);
			this.AddToGlowBumpPool(256);
			this.snapOverlapPool = new List<SnapOverlap>(4096);
			this.AddToSnapOverlapPool(4096);
			this.isSetup = true;
		}

		public void BuildFromShelves(List<BuilderShelf> shelves)
		{
			for (int i = 0; i < shelves.Count; i++)
			{
				BuilderShelf builderShelf = shelves[i];
				for (int j = 0; j < builderShelf.buildPieceSpawns.Count; j++)
				{
					BuilderShelf.BuildPieceSpawn buildPieceSpawn = builderShelf.buildPieceSpawns[j];
					this.AddToPool(buildPieceSpawn.buildPiecePrefab.name.GetStaticHash(), buildPieceSpawn.count);
				}
			}
		}

		public IEnumerator BuildFromPieceSets()
		{
			if (this.hasBuiltPieceSets)
			{
				yield break;
			}
			this.hasBuiltPieceSets = true;
			List<BuilderPieceSet> allPieceSets = BuilderSetManager.instance.GetAllPieceSets();
			foreach (BuilderPieceSet builderPieceSet in allPieceSets)
			{
				bool isStarterSet = BuilderSetManager.instance.GetStarterSetsConcat().Contains(builderPieceSet.playfabID);
				bool isFallbackSet = builderPieceSet.SetName.Equals("HIDDEN");
				foreach (BuilderPieceSet.BuilderPieceSubset builderPieceSubset in builderPieceSet.subsets)
				{
					foreach (BuilderPieceSet.PieceInfo pieceInfo in builderPieceSubset.pieceInfos)
					{
						int pieceType = pieceInfo.piecePrefab.name.GetStaticHash();
						int count;
						if (!this.piecePoolLookup.TryGetValue(pieceType, out count))
						{
							count = this.piecePools.Count;
							this.piecePools.Add(new List<BuilderPiece>(128));
							this.piecePoolLookup.Add(pieceType, count);
							if (!isFallbackSet)
							{
								int numToCreate = (isStarterSet ? 32 : 8);
								int i = 0;
								while (i < numToCreate)
								{
									i += 2;
									this.AddToPool(pieceType, 2);
									yield return null;
								}
							}
						}
						yield return null;
					}
					List<BuilderPieceSet.PieceInfo>.Enumerator enumerator3 = default(List<BuilderPieceSet.PieceInfo>.Enumerator);
				}
				List<BuilderPieceSet.BuilderPieceSubset>.Enumerator enumerator2 = default(List<BuilderPieceSet.BuilderPieceSubset>.Enumerator);
			}
			List<BuilderPieceSet>.Enumerator enumerator = default(List<BuilderPieceSet>.Enumerator);
			yield break;
			yield break;
		}

		private void AddToPool(int pieceType, int count)
		{
			int count2;
			if (!this.piecePoolLookup.TryGetValue(pieceType, out count2))
			{
				count2 = this.piecePools.Count;
				this.piecePools.Add(new List<BuilderPiece>(count * 8));
				this.piecePoolLookup.Add(pieceType, count2);
				Debug.LogWarningFormat("Creating Pool for piece {0} of size {1}. Is this piece not in a piece set?", new object[]
				{
					pieceType,
					count * 8
				});
			}
			BuilderPiece piecePrefab = BuilderSetManager.instance.GetPiecePrefab(pieceType);
			if (piecePrefab == null)
			{
				return;
			}
			List<BuilderPiece> list = this.piecePools[count2];
			for (int i = 0; i < count; i++)
			{
				BuilderPiece builderPiece = Object.Instantiate<BuilderPiece>(piecePrefab);
				builderPiece.OnCreatedByPool();
				builderPiece.gameObject.SetActive(false);
				list.Add(builderPiece);
			}
		}

		public BuilderPiece CreatePiece(int pieceType, bool assertNotEmpty)
		{
			int count;
			if (!this.piecePoolLookup.TryGetValue(pieceType, out count))
			{
				if (assertNotEmpty)
				{
					Debug.LogErrorFormat("No Pool Found for {0} Adding 4", new object[] { pieceType });
				}
				count = this.piecePools.Count;
				this.AddToPool(pieceType, 4);
			}
			List<BuilderPiece> list = this.piecePools[count];
			if (list.Count == 0)
			{
				if (assertNotEmpty)
				{
					Debug.LogErrorFormat("Pool for {0} is Empty Adding 4", new object[] { pieceType });
				}
				this.AddToPool(pieceType, 4);
			}
			BuilderPiece builderPiece = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
			return builderPiece;
		}

		public void DestroyPiece(BuilderPiece piece)
		{
			if (piece == null)
			{
				Debug.LogError("Why is a null piece being destroyed");
				return;
			}
			int num;
			if (!this.piecePoolLookup.TryGetValue(piece.pieceType, out num))
			{
				Debug.LogErrorFormat("No Pool Found for {0} Cannot return to pool", new object[] { piece.pieceType });
				return;
			}
			List<BuilderPiece> list = this.piecePools[num];
			if (list.Count == 128)
			{
				piece.OnReturnToPool();
				Object.Destroy(piece.gameObject);
				return;
			}
			piece.gameObject.SetActive(false);
			piece.transform.SetParent(null);
			piece.transform.SetPositionAndRotation(Vector3.up * 10000f, Quaternion.identity);
			piece.OnReturnToPool();
			list.Add(piece);
		}

		private void AddToGlowBumpPool(int count)
		{
			if (this.bumpGlowPrefab == null)
			{
				return;
			}
			for (int i = 0; i < count; i++)
			{
				BuilderBumpGlow builderBumpGlow = Object.Instantiate<BuilderBumpGlow>(this.bumpGlowPrefab);
				builderBumpGlow.gameObject.SetActive(false);
				this.bumpGlowPool.Add(builderBumpGlow);
			}
		}

		public BuilderBumpGlow CreateGlowBump()
		{
			if (this.bumpGlowPool.Count == 0)
			{
				this.AddToGlowBumpPool(4);
			}
			BuilderBumpGlow builderBumpGlow = this.bumpGlowPool[this.bumpGlowPool.Count - 1];
			this.bumpGlowPool.RemoveAt(this.bumpGlowPool.Count - 1);
			return builderBumpGlow;
		}

		public void DestroyBumpGlow(BuilderBumpGlow bump)
		{
			if (bump == null)
			{
				return;
			}
			bump.gameObject.SetActive(false);
			bump.transform.SetPositionAndRotation(Vector3.up * 10000f, Quaternion.identity);
			this.bumpGlowPool.Add(bump);
		}

		private void AddToSnapOverlapPool(int count)
		{
			this.snapOverlapPool.Capacity = this.snapOverlapPool.Capacity + count;
			for (int i = 0; i < count; i++)
			{
				this.snapOverlapPool.Add(new SnapOverlap());
			}
		}

		public SnapOverlap CreateSnapOverlap(BuilderAttachGridPlane otherPlane, SnapBounds bounds)
		{
			if (this.snapOverlapPool.Count == 0)
			{
				this.AddToSnapOverlapPool(1024);
			}
			SnapOverlap snapOverlap = this.snapOverlapPool[this.snapOverlapPool.Count - 1];
			this.snapOverlapPool.RemoveAt(this.snapOverlapPool.Count - 1);
			snapOverlap.otherPlane = otherPlane;
			snapOverlap.bounds = bounds;
			snapOverlap.nextOverlap = null;
			return snapOverlap;
		}

		public void DestroySnapOverlap(SnapOverlap snapOverlap)
		{
			snapOverlap.otherPlane = null;
			snapOverlap.nextOverlap = null;
			this.snapOverlapPool.Add(snapOverlap);
		}

		private void OnDestroy()
		{
			for (int i = 0; i < this.piecePools.Count; i++)
			{
				if (this.piecePools[i] != null)
				{
					foreach (BuilderPiece builderPiece in this.piecePools[i])
					{
						if (builderPiece != null)
						{
							Object.Destroy(builderPiece);
						}
					}
					this.piecePools[i].Clear();
				}
			}
			this.piecePoolLookup.Clear();
			foreach (BuilderBumpGlow builderBumpGlow in this.bumpGlowPool)
			{
				Object.Destroy(builderBumpGlow);
			}
			this.bumpGlowPool.Clear();
		}

		public List<List<BuilderPiece>> piecePools;

		public Dictionary<int, int> piecePoolLookup;

		[HideInInspector]
		public List<BuilderBumpGlow> bumpGlowPool;

		public BuilderBumpGlow bumpGlowPrefab;

		[HideInInspector]
		public List<SnapOverlap> snapOverlapPool;

		public static BuilderPool instance;

		private const int POOl_CAPACITY = 128;

		private const int INITIAL_INSTANCE_COUNT_STARTER = 32;

		private const int INITIAL_INSTANCE_COUNT_PREMIUM = 8;

		private bool isSetup;

		private bool hasBuiltPieceSets;
	}
}
