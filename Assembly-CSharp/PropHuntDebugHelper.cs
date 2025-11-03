using System;
using System.Collections;
using GorillaTag.CosmeticSystem;
using TMPro;
using UnityEngine;

public class PropHuntDebugHelper : MonoBehaviour
{
	protected void Awake()
	{
		if (PropHuntDebugHelper.instance != null)
		{
			Object.Destroy(this);
			return;
		}
		PropHuntDebugHelper.instance = this;
	}

	private IEnumerator Start()
	{
		yield return null;
		yield return null;
		this._propHuntManager = Object.FindAnyObjectByType<GorillaPropHuntGameManager>();
		if (this._propHuntManager != null)
		{
			Debug.Log("PropHuntDebugHelper :: Found number of props " + PropHuntPools.AllPropCosmeticIds.Length.ToString());
			this._cachedAllPropIDs = PropHuntPools.AllPropCosmeticIds;
			this._localPropHuntHandFollower = VRRig.LocalRig.GetComponent<PropHuntHandFollower>();
			this.UpdatePropsText();
		}
		yield break;
	}

	public void UpdatePropsText()
	{
		string selectedPropID = this.GetSelectedPropID(this._selectedPropIndex);
		string text = string.Empty;
		if (this._selectedPropIndex != -1)
		{
			CosmeticSO cosmeticSO = this._allCosmetics.SearchForCosmeticSO(selectedPropID);
			if (cosmeticSO != null)
			{
				text = cosmeticSO.info.displayName;
			}
		}
		this._propsText.text = "Current Prop: " + this.GetCurrentPropInfo() + "\n" + string.Format("Selected Prop: {0} - {1} ({2}/{3})", new object[]
		{
			selectedPropID,
			text,
			this._selectedPropIndex,
			this._cachedAllPropIDs.Length
		});
	}

	private string GetCurrentPropInfo()
	{
		return string.Empty;
	}

	private string GetSelectedPropID(int index)
	{
		if (index <= -1)
		{
			return "None";
		}
		return this._cachedAllPropIDs[index];
	}

	[ContextMenu("Prev Prop")]
	public void PrevProp()
	{
		this._selectedPropIndex--;
		if (this._selectedPropIndex < -1)
		{
			this._selectedPropIndex = this._cachedAllPropIDs.Length - 1;
		}
		string text = ((this._selectedPropIndex > -1) ? this.GetSelectedPropID(this._selectedPropIndex) : string.Empty);
		this.SendForcePropHandRPC(text);
		this.UpdatePropsText();
	}

	[ContextMenu("Next Prop")]
	public void NextProp()
	{
		this._selectedPropIndex++;
		if (this._selectedPropIndex >= this._cachedAllPropIDs.Length)
		{
			this._selectedPropIndex = -1;
		}
		string text = ((this._selectedPropIndex > -1) ? this.GetSelectedPropID(this._selectedPropIndex) : string.Empty);
		this.SendForcePropHandRPC(text);
		this.UpdatePropsText();
	}

	private void SendForcePropHandRPC(string newPropId)
	{
	}

	[ContextMenu("Toggle Round")]
	public void ToggleRound()
	{
	}

	[OnEnterPlay_SetNull]
	public static PropHuntDebugHelper instance;

	[SerializeField]
	private GorillaPropHuntGameManager _propHuntManager;

	[SerializeField]
	private PropHuntHandFollower _localPropHuntHandFollower;

	[SerializeField]
	private TextMeshPro _propsText;

	[SerializeField]
	private AllCosmeticsArraySO _allCosmetics;

	private string[] _cachedAllPropIDs;

	private int _selectedPropIndex = -1;
}
