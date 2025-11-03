using System;
using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class BuilderSetSelector : MonoBehaviour
{
	private void Start()
	{
		this.zoneRenderers.Clear();
		foreach (GorillaPressableButton gorillaPressableButton in this.groupButtons)
		{
			this.zoneRenderers.Add(gorillaPressableButton.buttonRenderer);
			TMP_Text myTmpText = gorillaPressableButton.myTmpText;
			Renderer renderer = ((myTmpText != null) ? myTmpText.GetComponent<Renderer>() : null);
			if (renderer != null)
			{
				this.zoneRenderers.Add(renderer);
			}
		}
		this.zoneRenderers.Add(this.previousPageButton.buttonRenderer);
		this.zoneRenderers.Add(this.nextPageButton.buttonRenderer);
		TMP_Text myTmpText2 = this.previousPageButton.myTmpText;
		Renderer renderer2 = ((myTmpText2 != null) ? myTmpText2.GetComponent<Renderer>() : null);
		if (renderer2 != null)
		{
			this.zoneRenderers.Add(renderer2);
		}
		TMP_Text myTmpText3 = this.nextPageButton.myTmpText;
		renderer2 = ((myTmpText3 != null) ? myTmpText3.GetComponent<Renderer>() : null);
		if (renderer2 != null)
		{
			this.zoneRenderers.Add(renderer2);
		}
		foreach (Renderer renderer3 in this.zoneRenderers)
		{
			renderer3.enabled = false;
		}
		this.inBuilderZone = false;
		ZoneManagement instance = ZoneManagement.instance;
		instance.onZoneChanged = (Action)Delegate.Combine(instance.onZoneChanged, new Action(this.OnZoneChanged));
		this.OnZoneChanged();
	}

	public void Setup(List<BuilderPieceSet.BuilderPieceCategory> categories)
	{
		List<BuilderPieceSet.BuilderDisplayGroup> liveDisplayGroups = BuilderSetManager.instance.GetLiveDisplayGroups();
		this.numLiveDisplayGroups = liveDisplayGroups.Count;
		this.includedGroups = new List<BuilderPieceSet.BuilderDisplayGroup>(liveDisplayGroups.Count);
		this._includedCategories = categories;
		foreach (BuilderPieceSet.BuilderDisplayGroup builderDisplayGroup in liveDisplayGroups)
		{
			if (this.DoesDisplayGroupHaveIncludedCategories(builderDisplayGroup))
			{
				this.includedGroups.Add(builderDisplayGroup);
			}
		}
		BuilderSetManager.instance.OnOwnedSetsUpdated.AddListener(new UnityAction(this.RefreshUnlockedGroups));
		BuilderSetManager.instance.OnLiveSetsUpdated.AddListener(new UnityAction(this.RefreshUnlockedGroups));
		this.groupsPerPage = this.groupButtons.Length;
		this.totalPages = this.includedGroups.Count / this.groupsPerPage;
		if (this.includedGroups.Count % this.groupsPerPage > 0)
		{
			this.totalPages++;
		}
		this.previousPageButton.gameObject.SetActive(this.totalPages > 1);
		this.nextPageButton.gameObject.SetActive(this.totalPages > 1);
		this.previousPageButton.myTmpText.enabled = this.totalPages > 1;
		this.nextPageButton.myTmpText.enabled = this.totalPages > 1;
		this.pageIndex = 0;
		this.currentGroup = this.includedGroups[this.includedGroupIndex];
		this.previousPageButton.onPressButton.AddListener(new UnityAction(this.OnPreviousPageClicked));
		this.nextPageButton.onPressButton.AddListener(new UnityAction(this.OnNextPageClicked));
		GorillaPressableButton[] array = this.groupButtons;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].onPressed += this.OnSetButtonPressed;
		}
		this.UpdateLabels();
	}

	private void OnDestroy()
	{
		if (this.previousPageButton != null)
		{
			this.previousPageButton.onPressButton.RemoveListener(new UnityAction(this.OnPreviousPageClicked));
		}
		if (this.nextPageButton != null)
		{
			this.nextPageButton.onPressButton.RemoveListener(new UnityAction(this.OnNextPageClicked));
		}
		if (BuilderSetManager.instance != null)
		{
			BuilderSetManager.instance.OnOwnedSetsUpdated.RemoveListener(new UnityAction(this.RefreshUnlockedGroups));
			BuilderSetManager.instance.OnLiveSetsUpdated.RemoveListener(new UnityAction(this.RefreshUnlockedGroups));
		}
		foreach (GorillaPressableButton gorillaPressableButton in this.groupButtons)
		{
			if (!(gorillaPressableButton == null))
			{
				gorillaPressableButton.onPressed -= this.OnSetButtonPressed;
			}
		}
		if (ZoneManagement.instance != null)
		{
			ZoneManagement instance = ZoneManagement.instance;
			instance.onZoneChanged = (Action)Delegate.Remove(instance.onZoneChanged, new Action(this.OnZoneChanged));
		}
	}

	private void OnZoneChanged()
	{
		bool flag = ZoneManagement.instance.IsZoneActive(GTZone.monkeBlocks);
		if (flag && !this.inBuilderZone)
		{
			using (List<Renderer>.Enumerator enumerator = this.zoneRenderers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Renderer renderer = enumerator.Current;
					renderer.enabled = true;
				}
				goto IL_008B;
			}
		}
		if (!flag && this.inBuilderZone)
		{
			foreach (Renderer renderer2 in this.zoneRenderers)
			{
				renderer2.enabled = false;
			}
		}
		IL_008B:
		this.inBuilderZone = flag;
	}

	private void OnSetButtonPressed(GorillaPressableButton button, bool isLeft)
	{
		int num = 0;
		for (int i = 0; i < this.groupButtons.Length; i++)
		{
			if (button.Equals(this.groupButtons[i]))
			{
				num = i;
				break;
			}
		}
		int num2 = this.pageIndex * this.groupsPerPage + num;
		if (num2 < this.includedGroups.Count)
		{
			BuilderPieceSet.BuilderDisplayGroup builderDisplayGroup = this.includedGroups[num2];
			if (this.currentGroup == null || builderDisplayGroup.displayName != this.currentGroup.displayName)
			{
				UnityEvent<int> onSelectedGroup = this.OnSelectedGroup;
				if (onSelectedGroup == null)
				{
					return;
				}
				onSelectedGroup.Invoke(builderDisplayGroup.GetDisplayGroupIdentifier());
			}
		}
	}

	private void RefreshUnlockedGroups()
	{
		List<BuilderPieceSet.BuilderDisplayGroup> liveDisplayGroups = BuilderSetManager.instance.GetLiveDisplayGroups();
		if (liveDisplayGroups.Count != this.numLiveDisplayGroups)
		{
			string text = ((this.currentGroup != null) ? this.currentGroup.displayName : "");
			this.numLiveDisplayGroups = liveDisplayGroups.Count;
			this.includedGroups.EnsureCapacity(this.numLiveDisplayGroups);
			this.includedGroups.Clear();
			int num = 0;
			foreach (BuilderPieceSet.BuilderDisplayGroup builderDisplayGroup in liveDisplayGroups)
			{
				if (this.DoesDisplayGroupHaveIncludedCategories(builderDisplayGroup))
				{
					if (builderDisplayGroup.displayName.Equals(text))
					{
						num = this.includedGroups.Count;
					}
					this.includedGroups.Add(builderDisplayGroup);
				}
			}
			if (this.includedGroups.Count < 1)
			{
				this.currentGroup = null;
			}
			else
			{
				this.includedGroupIndex = num;
				this.currentGroup = this.includedGroups[this.includedGroupIndex];
			}
			this.totalPages = this.includedGroups.Count / this.groupsPerPage;
			if (this.includedGroups.Count % this.groupsPerPage > 0)
			{
				this.totalPages++;
			}
			this.previousPageButton.gameObject.SetActive(this.totalPages > 1);
			this.nextPageButton.gameObject.SetActive(this.totalPages > 1);
			this.previousPageButton.myTmpText.enabled = this.totalPages > 1;
			this.nextPageButton.myTmpText.enabled = this.totalPages > 1;
		}
		this.UpdateLabels();
	}

	private void OnPreviousPageClicked()
	{
		this.RefreshUnlockedGroups();
		int num = Mathf.Clamp(this.pageIndex - 1, 0, this.totalPages - 1);
		if (num != this.pageIndex)
		{
			this.pageIndex = num;
			this.UpdateLabels();
		}
	}

	private void OnNextPageClicked()
	{
		this.RefreshUnlockedGroups();
		int num = Mathf.Clamp(this.pageIndex + 1, 0, this.totalPages - 1);
		if (num != this.pageIndex)
		{
			this.pageIndex = num;
			this.UpdateLabels();
		}
	}

	public void SetSelection(int groupID)
	{
		if (BuilderSetManager.instance == null)
		{
			return;
		}
		BuilderPieceSet.BuilderDisplayGroup newGroup = BuilderSetManager.instance.GetDisplayGroupFromIndex(groupID);
		if (newGroup == null)
		{
			return;
		}
		this.currentGroup = newGroup;
		this.includedGroupIndex = this.includedGroups.FindIndex((BuilderPieceSet.BuilderDisplayGroup x) => x.displayName == newGroup.displayName);
		this.UpdateLabels();
	}

	private void UpdateLabels()
	{
		for (int i = 0; i < this.groupLabels.Length; i++)
		{
			int num = this.pageIndex * this.groupsPerPage + i;
			if (num < this.includedGroups.Count && this.includedGroups[num] != null)
			{
				if (!this.groupButtons[i].gameObject.activeSelf)
				{
					this.groupButtons[i].gameObject.SetActive(true);
					this.groupButtons[i].myTmpText.gameObject.SetActive(true);
				}
				if (this.groupButtons[i].myTmpText.text != this.includedGroups[num].displayName)
				{
					this.groupButtons[i].myTmpText.text = this.includedGroups[num].displayName;
				}
				if (BuilderSetManager.instance.IsPieceSetOwnedLocally(this.includedGroups[num].setID))
				{
					bool flag = this.currentGroup != null && this.includedGroups[num].displayName == this.currentGroup.displayName;
					if (flag != this.groupButtons[i].isOn || !this.groupButtons[i].enabled)
					{
						this.groupButtons[i].isOn = flag;
						this.groupButtons[i].buttonRenderer.material = (flag ? this.groupButtons[i].pressedMaterial : this.groupButtons[i].unpressedMaterial);
					}
					this.groupButtons[i].enabled = true;
				}
				else
				{
					if (this.groupButtons[i].enabled)
					{
						this.groupButtons[i].buttonRenderer.material = this.disabledMaterial;
					}
					this.groupButtons[i].enabled = false;
				}
			}
			else
			{
				if (this.groupButtons[i].gameObject.activeSelf)
				{
					this.groupButtons[i].gameObject.SetActive(false);
					this.groupButtons[i].myTmpText.gameObject.SetActive(false);
				}
				if (this.groupButtons[i].isOn || this.groupButtons[i].enabled)
				{
					this.groupButtons[i].isOn = false;
					this.groupButtons[i].enabled = false;
				}
			}
		}
		bool flag2 = this.pageIndex > 0 && this.totalPages > 1;
		bool flag3 = this.pageIndex < this.totalPages - 1 && this.totalPages > 1;
		if (this.previousPageButton.myTmpText.enabled != flag2)
		{
			this.previousPageButton.myTmpText.enabled = flag2;
		}
		if (this.nextPageButton.myTmpText.enabled != flag3)
		{
			this.nextPageButton.myTmpText.enabled = flag3;
		}
	}

	public bool DoesDisplayGroupHaveIncludedCategories(BuilderPieceSet.BuilderDisplayGroup set)
	{
		foreach (BuilderPieceSet.BuilderPieceSubset builderPieceSubset in set.pieceSubsets)
		{
			if (this._includedCategories.Contains(builderPieceSubset.pieceCategory))
			{
				return true;
			}
		}
		return false;
	}

	public BuilderPieceSet.BuilderDisplayGroup GetSelectedGroup()
	{
		return this.currentGroup;
	}

	public int GetDefaultGroupID()
	{
		if (this.includedGroups == null || this.includedGroups.Count < 1)
		{
			return -1;
		}
		BuilderPieceSet.BuilderDisplayGroup builderDisplayGroup = this.includedGroups[0];
		if (!BuilderSetManager.instance.IsPieceSetOwnedLocally(builderDisplayGroup.setID))
		{
			foreach (BuilderPieceSet.BuilderDisplayGroup builderDisplayGroup2 in this.includedGroups)
			{
				if (BuilderSetManager.instance.IsPieceSetOwnedLocally(builderDisplayGroup2.setID))
				{
					return builderDisplayGroup2.GetDisplayGroupIdentifier();
				}
			}
			Debug.LogWarning("No default group available for shelf");
			return -1;
		}
		return builderDisplayGroup.GetDisplayGroupIdentifier();
	}

	private List<BuilderPieceSet.BuilderDisplayGroup> includedGroups;

	private int numLiveDisplayGroups;

	[SerializeField]
	private Material disabledMaterial;

	[Header("UI")]
	[FormerlySerializedAs("setLabels")]
	[SerializeField]
	private Text[] groupLabels;

	[Header("Buttons")]
	[FormerlySerializedAs("setButtons")]
	[SerializeField]
	private GorillaPressableButton[] groupButtons;

	[SerializeField]
	private GorillaPressableButton previousPageButton;

	[SerializeField]
	private GorillaPressableButton nextPageButton;

	private List<BuilderPieceSet.BuilderPieceCategory> _includedCategories;

	private int includedGroupIndex;

	private BuilderPieceSet.BuilderDisplayGroup currentGroup;

	private int pageIndex;

	private int groupsPerPage = 3;

	private int totalPages = 1;

	private List<Renderer> zoneRenderers = new List<Renderer>(10);

	private bool inBuilderZone;

	[HideInInspector]
	public UnityEvent<int> OnSelectedGroup;
}
