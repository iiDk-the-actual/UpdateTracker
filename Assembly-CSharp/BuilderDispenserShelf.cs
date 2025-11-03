using System;
using System.Collections.Generic;
using GorillaTagScripts;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public class BuilderDispenserShelf : MonoBehaviour
{
	private void BuildDispenserPool()
	{
		this.dispenserPool = new List<BuilderDispenser>(12);
		this.activeDispensers = new List<BuilderDispenser>(6);
		this.AddToDispenserPool(6);
	}

	private void AddToDispenserPool(int count)
	{
		if (this.dispenserPrefab == null)
		{
			return;
		}
		for (int i = 0; i < count; i++)
		{
			BuilderDispenser builderDispenser = Object.Instantiate<BuilderDispenser>(this.dispenserPrefab, this.shelfCenter);
			builderDispenser.gameObject.SetActive(false);
			builderDispenser.table = this.table;
			builderDispenser.shelfID = this.shelfID;
			this.dispenserPool.Add(builderDispenser);
		}
	}

	private void ActivateDispensers()
	{
		this.piecesInSet.Clear();
		foreach (BuilderPieceSet.BuilderPieceSubset builderPieceSubset in this.currentGroup.pieceSubsets)
		{
			if (this._includedCategories.Contains(builderPieceSubset.pieceCategory))
			{
				this.piecesInSet.AddRange(builderPieceSubset.pieceInfos);
			}
		}
		if (this.piecesInSet.Count <= 0)
		{
			return;
		}
		int count = this.piecesInSet.Count;
		if (this.dispenserPool.Count < count)
		{
			this.AddToDispenserPool(count - this.dispenserPool.Count);
		}
		this.activeDispensers.Clear();
		for (int i = 0; i < this.dispenserPool.Count; i++)
		{
			if (i < count)
			{
				BuilderDispenser builderDispenser = this.dispenserPool[i];
				builderDispenser.gameObject.SetActive(true);
				float num = this.shelfWidth / -2f + this.shelfWidth / (float)(count * 2) + this.shelfWidth / (float)count * (float)i;
				builderDispenser.transform.localPosition = new Vector3(num, 0f, 0f);
				builderDispenser.AssignPieceType(this.piecesInSet[i], this.currentGroup.defaultMaterial.GetHashCode());
				this.activeDispensers.Add(builderDispenser);
			}
			else
			{
				this.dispenserPool[i].ClearDispenser();
				this.dispenserPool[i].gameObject.SetActive(false);
			}
		}
		this.dispenserToUpdate = 0;
	}

	public void Setup()
	{
		this.InitIfNeeded();
		foreach (BuilderDispenser builderDispenser in this.dispenserPool)
		{
			builderDispenser.table = this.table;
			builderDispenser.shelfID = this.shelfID;
		}
	}

	private void InitIfNeeded()
	{
		if (this.initialized)
		{
			return;
		}
		this.setSelector.Setup(this._includedCategories);
		this.currentGroup = this.setSelector.GetSelectedGroup();
		this.setSelector.OnSelectedGroup.AddListener(new UnityAction<int>(this.OnSelectedSetChange));
		this.BuildDispenserPool();
		this.ActivateDispensers();
		this.initialized = true;
	}

	private void OnDestroy()
	{
		if (this.setSelector != null)
		{
			this.setSelector.OnSelectedGroup.RemoveListener(new UnityAction<int>(this.OnSelectedSetChange));
		}
	}

	public void OnSelectedSetChange(int displayGroupID)
	{
		if (this.table.GetTableState() != BuilderTable.TableState.Ready)
		{
			return;
		}
		this.table.RequestShelfSelection(this.shelfID, displayGroupID, false);
	}

	public void SetSelection(int displayGroupID)
	{
		this.setSelector.SetSelection(displayGroupID);
		BuilderPieceSet.BuilderDisplayGroup selectedGroup = this.setSelector.GetSelectedGroup();
		if ((this.initialized && this.currentGroup == null) || selectedGroup.displayName != this.currentGroup.displayName)
		{
			this.currentGroup = selectedGroup;
			if (this.table.GetTableState() == BuilderTable.TableState.Ready)
			{
				if (!this.animatingShelf)
				{
					this.StartShelfSwap();
					return;
				}
			}
			else
			{
				this.animatingShelf = false;
				this.ImmediateShelfSwap();
			}
		}
	}

	public int GetSelectedDisplayGroupID()
	{
		return this.setSelector.GetSelectedGroup().GetDisplayGroupIdentifier();
	}

	private void ImmediateShelfSwap()
	{
		foreach (BuilderDispenser builderDispenser in this.activeDispensers)
		{
			builderDispenser.ClearDispenser();
		}
		this.ActivateDispensers();
	}

	private void StartShelfSwap()
	{
		this.dispenserToClear = 0;
		this.timeToClearShelf = (double)(Time.time + 0.15f);
		this.resetAnimation.Rewind();
		foreach (BuilderDispenser builderDispenser in this.activeDispensers)
		{
			builderDispenser.ParentPieceToShelf(this.resetAnimation.transform);
		}
		this.resetAnimation.Play();
		this.animatingShelf = true;
	}

	public void UpdateShelf()
	{
		if (this.animatingShelf && (double)Time.time > this.timeToClearShelf)
		{
			if (this.dispenserToClear < this.activeDispensers.Count)
			{
				if (this.dispenserToClear == 0)
				{
					this.resetSoundBank.Play();
				}
				this.activeDispensers[this.dispenserToClear].ClearDispenser();
				this.dispenserToClear++;
				return;
			}
			if (!this.resetAnimation.isPlaying)
			{
				this.playSpawnSetSound = true;
				this.ActivateDispensers();
				this.animatingShelf = false;
			}
		}
	}

	public void UpdateShelfSliced()
	{
		if (!PhotonNetwork.LocalPlayer.IsMasterClient)
		{
			return;
		}
		if (!this.initialized)
		{
			return;
		}
		if (this.animatingShelf)
		{
			return;
		}
		if (this.shouldVerifySetSelection)
		{
			BuilderPieceSet.BuilderDisplayGroup selectedGroup = this.setSelector.GetSelectedGroup();
			if (selectedGroup == null || !BuilderSetManager.instance.DoesAnyPlayerInRoomOwnPieceSet(selectedGroup.setID))
			{
				int defaultGroupID = this.setSelector.GetDefaultGroupID();
				if (defaultGroupID != -1)
				{
					this.OnSelectedSetChange(defaultGroupID);
				}
			}
			this.shouldVerifySetSelection = false;
		}
		if (this.activeDispensers.Count > 0)
		{
			this.activeDispensers[this.dispenserToUpdate].UpdateDispenser();
			this.dispenserToUpdate = (this.dispenserToUpdate + 1) % this.activeDispensers.Count;
		}
	}

	public void VerifySetSelection()
	{
		this.shouldVerifySetSelection = true;
	}

	public void OnShelfPieceCreated(BuilderPiece piece, bool playfx)
	{
		if (this.playSpawnSetSound && playfx)
		{
			this.audioSource.GTPlayOneShot(this.spawnNewSetSound, 1f);
			this.playSpawnSetSound = false;
		}
		foreach (BuilderDispenser builderDispenser in this.activeDispensers)
		{
			builderDispenser.ShelfPieceCreated(piece, playfx);
		}
	}

	public void OnShelfPieceRecycled(BuilderPiece piece)
	{
		foreach (BuilderDispenser builderDispenser in this.activeDispensers)
		{
			builderDispenser.ShelfPieceRecycled(piece);
		}
	}

	public void OnClearTable()
	{
		if (!this.initialized)
		{
			return;
		}
		foreach (BuilderDispenser builderDispenser in this.activeDispensers)
		{
			builderDispenser.OnClearTable();
		}
		base.StopAllCoroutines();
		if (this.animatingShelf)
		{
			this.resetAnimation.Rewind();
			this.animatingShelf = false;
		}
	}

	public void ClearShelf()
	{
		foreach (BuilderDispenser builderDispenser in this.activeDispensers)
		{
			builderDispenser.ClearDispenser();
		}
	}

	[Header("Set Selection")]
	[SerializeField]
	private BuilderSetSelector setSelector;

	public List<BuilderPieceSet.BuilderPieceCategory> _includedCategories;

	[Header("Dispenser Shelf Properties")]
	public Transform shelfCenter;

	public float shelfWidth = 1.4f;

	public Animation resetAnimation;

	[SerializeField]
	private SoundBankPlayer resetSoundBank;

	[SerializeField]
	private AudioClip spawnNewSetSound;

	[SerializeField]
	private AudioSource audioSource;

	private bool playSpawnSetSound;

	[HideInInspector]
	public BuilderTable table;

	public int shelfID = -1;

	private BuilderPieceSet.BuilderDisplayGroup currentGroup;

	private bool initialized;

	public BuilderDispenser dispenserPrefab;

	private List<BuilderDispenser> dispenserPool;

	private List<BuilderDispenser> activeDispensers;

	private List<BuilderPieceSet.PieceInfo> piecesInSet = new List<BuilderPieceSet.PieceInfo>(10);

	private bool animatingShelf;

	private double timeToClearShelf = double.MaxValue;

	private int dispenserToClear;

	private int dispenserToUpdate;

	private bool shouldVerifySetSelection;
}
