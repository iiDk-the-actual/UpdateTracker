using System;
using System.Collections.Generic;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class FriendDisplay : MonoBehaviour
{
	public bool InRemoveMode
	{
		get
		{
			return this.inRemoveMode;
		}
	}

	private void Start()
	{
		this.InitFriendCards();
		this.InitLocalPlayerCard();
		this.UpdateLocalPlayerPrivacyButtons();
		this.triggerNotifier.TriggerEnterEvent += this.TriggerEntered;
		this.triggerNotifier.TriggerExitEvent += this.TriggerExited;
		NetworkSystem.Instance.OnJoinedRoomEvent += this.OnJoinedRoom;
	}

	private void OnDestroy()
	{
		if (NetworkSystem.Instance != null)
		{
			NetworkSystem.Instance.OnJoinedRoomEvent -= this.OnJoinedRoom;
		}
		if (this.triggerNotifier != null)
		{
			this.triggerNotifier.TriggerEnterEvent -= this.TriggerEntered;
			this.triggerNotifier.TriggerExitEvent -= this.TriggerExited;
		}
	}

	public void TriggerEntered(TriggerEventNotifier notifier, Collider other)
	{
		if (other == GTPlayer.Instance.headCollider)
		{
			FriendSystem.Instance.OnFriendListRefresh += this.OnGetFriendsReceived;
			FriendSystem.Instance.RefreshFriendsList();
			this.PopulateLocalPlayerCard();
			this.localPlayerAtDisplay = true;
			if (this.InRemoveMode)
			{
				this.ToggleRemoveFriendMode();
			}
		}
	}

	public void TriggerExited(TriggerEventNotifier notifier, Collider other)
	{
		if (other == GTPlayer.Instance.headCollider)
		{
			FriendSystem.Instance.OnFriendListRefresh -= this.OnGetFriendsReceived;
			this.ClearFriendCards();
			this.ClearLocalPlayerCard();
			this.ClearPageButtons();
			this.localPlayerAtDisplay = false;
			if (this.InRemoveMode)
			{
				this.ToggleRemoveFriendMode();
			}
		}
	}

	private void OnJoinedRoom()
	{
		this.Refresh();
	}

	private void Refresh()
	{
		if (this.localPlayerAtDisplay)
		{
			FriendSystem.Instance.RefreshFriendsList();
			this.PopulateLocalPlayerCard();
		}
	}

	public void LocalPlayerFullyVisiblePress()
	{
		FriendSystem.Instance.SetLocalPlayerPrivacy(FriendSystem.PlayerPrivacy.Visible);
		this.UpdateLocalPlayerPrivacyButtons();
		this.PopulateLocalPlayerCard();
	}

	public void LocalPlayerPublicOnlyPress()
	{
		FriendSystem.Instance.SetLocalPlayerPrivacy(FriendSystem.PlayerPrivacy.PublicOnly);
		this.UpdateLocalPlayerPrivacyButtons();
		this.PopulateLocalPlayerCard();
	}

	public void LocalPlayerFullyHiddenPress()
	{
		FriendSystem.Instance.SetLocalPlayerPrivacy(FriendSystem.PlayerPrivacy.Hidden);
		this.UpdateLocalPlayerPrivacyButtons();
		this.PopulateLocalPlayerCard();
	}

	private void UpdateLocalPlayerPrivacyButtons()
	{
		FriendSystem.PlayerPrivacy localPlayerPrivacy = FriendSystem.Instance.LocalPlayerPrivacy;
		this.SetButtonAppearance(this._localPlayerFullyVisibleButton, localPlayerPrivacy == FriendSystem.PlayerPrivacy.Visible);
		this.SetButtonAppearance(this._localPlayerPublicOnlyButton, localPlayerPrivacy == FriendSystem.PlayerPrivacy.PublicOnly);
		this.SetButtonAppearance(this._localPlayerFullyHiddenButton, localPlayerPrivacy == FriendSystem.PlayerPrivacy.Hidden);
	}

	private void UpdatePageButtons(int selectedPage)
	{
		for (int i = 0; i < this.totalPages; i++)
		{
			if (FriendBackendController.Instance.FriendsList.Count > this.cardsPerPage * Mathf.Max(i, 1))
			{
				this.SetPageButtonAppearance(this.PageButtons[i], (i == selectedPage) ? FriendDisplay.ButtonState.Alert : FriendDisplay.ButtonState.Active);
			}
			else
			{
				this.SetPageButtonAppearance(this.PageButtons[i], false);
			}
		}
	}

	private void SetButtonAppearance(MeshRenderer buttonRenderer, bool active)
	{
		this.SetButtonAppearance(buttonRenderer, active ? FriendDisplay.ButtonState.Active : FriendDisplay.ButtonState.Default);
	}

	private void SetButtonAppearance(MeshRenderer buttonRenderer, FriendDisplay.ButtonState state)
	{
		Material[] array;
		switch (state)
		{
		case FriendDisplay.ButtonState.Default:
			array = this._buttonDefaultMaterials;
			break;
		case FriendDisplay.ButtonState.Active:
			array = this._buttonActiveMaterials;
			break;
		case FriendDisplay.ButtonState.Alert:
			array = this._buttonAlertMaterials;
			break;
		default:
			throw new ArgumentOutOfRangeException("state", state, null);
		}
		buttonRenderer.sharedMaterials = array;
	}

	private void ClearPageButtons()
	{
		for (int i = 0; i < this.PageButtons.Length; i++)
		{
			this.SetPageButtonAppearance(this.PageButtons[i], false);
		}
	}

	private void SetPageButtonAppearance(MeshRenderer buttonRenderer, bool active)
	{
		this.SetPageButtonAppearance(buttonRenderer, active ? FriendDisplay.ButtonState.Active : FriendDisplay.ButtonState.Default);
	}

	private void SetPageButtonAppearance(MeshRenderer buttonRenderer, FriendDisplay.ButtonState state)
	{
		bool flag;
		switch (state)
		{
		case FriendDisplay.ButtonState.Default:
			flag = false;
			break;
		case FriendDisplay.ButtonState.Active:
			flag = true;
			break;
		case FriendDisplay.ButtonState.Alert:
			flag = true;
			break;
		default:
			throw new ArgumentOutOfRangeException("state", state, null);
		}
		buttonRenderer.enabled = flag;
		Material[] array;
		switch (state)
		{
		case FriendDisplay.ButtonState.Default:
			array = this._pageButtonDefaultMaterials;
			break;
		case FriendDisplay.ButtonState.Active:
			array = this._pageButtonActiveMaterials;
			break;
		case FriendDisplay.ButtonState.Alert:
			array = this._pageButtonAlerttMaterials;
			break;
		default:
			throw new ArgumentOutOfRangeException("state", state, null);
		}
		buttonRenderer.sharedMaterials = array;
		Transform transform = buttonRenderer.transform;
		Vector3 vector;
		switch (state)
		{
		case FriendDisplay.ButtonState.Default:
			vector = new Vector3(buttonRenderer.transform.localPosition.x, buttonRenderer.transform.localPosition.y, this.pageButtonInactiveZPos);
			break;
		case FriendDisplay.ButtonState.Active:
			vector = new Vector3(buttonRenderer.transform.localPosition.x, buttonRenderer.transform.localPosition.y, this.pageButtonActiveZPos);
			break;
		case FriendDisplay.ButtonState.Alert:
			vector = new Vector3(buttonRenderer.transform.localPosition.x, buttonRenderer.transform.localPosition.y, this.pageButtonActiveZPos);
			break;
		default:
			throw new ArgumentOutOfRangeException("state", state, null);
		}
		transform.localPosition = vector;
		BoxCollider component = buttonRenderer.GetComponent<BoxCollider>();
		switch (state)
		{
		case FriendDisplay.ButtonState.Default:
			flag = false;
			break;
		case FriendDisplay.ButtonState.Active:
			flag = true;
			break;
		case FriendDisplay.ButtonState.Alert:
			flag = true;
			break;
		default:
			throw new ArgumentOutOfRangeException("state", state, null);
		}
		component.enabled = flag;
	}

	public void ToggleRemoveFriendMode()
	{
		this.inRemoveMode = !this.inRemoveMode;
		FriendCard[] array = this.friendCards;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetRemoveEnabled(this.inRemoveMode);
		}
		this.SetButtonAppearance(this._removeFriendButton, this.inRemoveMode ? FriendDisplay.ButtonState.Alert : FriendDisplay.ButtonState.Default);
	}

	private void InitFriendCards()
	{
		float num = this.gridWidth / (float)this.gridDimension;
		float num2 = this.gridHeight / (float)this.gridDimension;
		Vector3 right = this.gridRoot.right;
		Vector3 vector = -this.gridRoot.up;
		Vector3 vector2 = this.gridRoot.position - right * (this.gridWidth * 0.5f - num * 0.5f) - vector * (this.gridHeight * 0.5f - num2 * 0.5f);
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < this.gridDimension; i++)
		{
			for (int j = 0; j < this.gridDimension; j++)
			{
				FriendCard friendCard = this.friendCards[num4];
				friendCard.gameObject.SetActive(true);
				friendCard.transform.localScale = Vector3.one * (num / friendCard.Width);
				friendCard.transform.position = vector2 + right * num * (float)j + vector * num2 * (float)i;
				friendCard.transform.rotation = this.gridRoot.transform.rotation;
				friendCard.Init(this);
				friendCard.SetButton(this._friendCardButtons[num3++], this._buttonDefaultMaterials, this._buttonActiveMaterials, this._buttonAlertMaterials, this._friendCardButtonText[num4]);
				friendCard.SetEmpty();
				num4++;
			}
		}
	}

	public void RandomizeFriendCards()
	{
		for (int i = 0; i < this.friendCards.Length; i++)
		{
			this.friendCards[i].Randomize();
		}
	}

	private void ClearFriendCards()
	{
		for (int i = 0; i < this.friendCards.Length; i++)
		{
			this.friendCards[i].SetEmpty();
		}
	}

	public void OnGetFriendsReceived(List<FriendBackendController.Friend> friendsList)
	{
		this.PopulateFriendCards(friendsList);
		this.UpdateLocalPlayerPrivacyButtons();
		this.PopulateLocalPlayerCard();
		this.UpdatePageButtons(0);
	}

	private void PopulateFriendCards(List<FriendBackendController.Friend> friendsList)
	{
		int num = Mathf.Min(this.friendCards.Length, friendsList.Count);
		int num2 = 0;
		while (num2 < num && friendsList[num2] != null)
		{
			this.friendCards[num2].Populate(friendsList[num2]);
			num2++;
		}
	}

	public void GoToFriendPage(int currentPage)
	{
		this.UpdatePageButtons(currentPage);
		for (int i = 0; i < this.friendCards.Length; i++)
		{
			this.friendCards[i].SetEmpty();
		}
		int num = currentPage * this.cardsPerPage;
		Mathf.Min(num + this.cardsPerPage, FriendBackendController.Instance.FriendsList.Count);
		int num2 = 0;
		int num3 = 0;
		while (num3 < this.friendCards.Length && FriendBackendController.Instance.FriendsList.Count > num + num2)
		{
			this.friendCards[num3].Populate(FriendBackendController.Instance.FriendsList[num + num2]);
			num2++;
			num3++;
		}
	}

	private void InitLocalPlayerCard()
	{
		this._localPlayerCard.Init(this);
		this.ClearLocalPlayerCard();
	}

	private void PopulateLocalPlayerCard()
	{
		string text = PhotonNetworkController.Instance.CurrentRoomZone.GetName<GTZone>().ToUpper();
		this._localPlayerCard.SetName(NetworkSystem.Instance.LocalPlayer.NickName.ToUpper());
		if (!PhotonNetwork.InRoom || string.IsNullOrEmpty(NetworkSystem.Instance.RoomName) || NetworkSystem.Instance.RoomName.Length <= 0)
		{
			this._localPlayerCard.SetRoom("OFFLINE");
			this._localPlayerCard.SetZone("");
			return;
		}
		bool flag = NetworkSystem.Instance.RoomName[0] == '@';
		bool flag2 = !NetworkSystem.Instance.SessionIsPrivate;
		if (FriendSystem.Instance.LocalPlayerPrivacy == FriendSystem.PlayerPrivacy.Hidden || (FriendSystem.Instance.LocalPlayerPrivacy == FriendSystem.PlayerPrivacy.PublicOnly && !flag2))
		{
			this._localPlayerCard.SetRoom("OFFLINE");
			this._localPlayerCard.SetZone("");
			return;
		}
		if (flag)
		{
			this._localPlayerCard.SetRoom(NetworkSystem.Instance.RoomName.Substring(1).ToUpper());
			this._localPlayerCard.SetZone("CUSTOM");
			return;
		}
		if (!flag2)
		{
			this._localPlayerCard.SetRoom(NetworkSystem.Instance.RoomName.ToUpper());
			this._localPlayerCard.SetZone("PRIVATE");
			return;
		}
		this._localPlayerCard.SetRoom(NetworkSystem.Instance.RoomName.ToUpper());
		this._localPlayerCard.SetZone(text);
	}

	private void ClearLocalPlayerCard()
	{
		this._localPlayerCard.SetEmpty();
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.white;
		float num = this.gridWidth * 0.5f;
		float num2 = this.gridHeight * 0.5f;
		float num3 = num;
		float num4 = num2;
		Vector3 vector = this.gridRoot.position + this.gridRoot.rotation * new Vector3(-num3, num4, 0f);
		Vector3 vector2 = this.gridRoot.position + this.gridRoot.rotation * new Vector3(num3, num4, 0f);
		Vector3 vector3 = this.gridRoot.position + this.gridRoot.rotation * new Vector3(-num3, -num4, 0f);
		Vector3 vector4 = this.gridRoot.position + this.gridRoot.rotation * new Vector3(num3, -num4, 0f);
		for (int i = 0; i <= this.gridDimension; i++)
		{
			float num5 = (float)i / (float)this.gridDimension;
			Vector3 vector5 = Vector3.Lerp(vector, vector2, num5);
			Vector3 vector6 = Vector3.Lerp(vector3, vector4, num5);
			Gizmos.DrawLine(vector5, vector6);
			Vector3 vector7 = Vector3.Lerp(vector, vector3, num5);
			Vector3 vector8 = Vector3.Lerp(vector2, vector4, num5);
			Gizmos.DrawLine(vector7, vector8);
		}
	}

	[FormerlySerializedAs("gridCenter")]
	[SerializeField]
	private FriendCard[] friendCards = new FriendCard[9];

	[SerializeField]
	private Transform gridRoot;

	[SerializeField]
	private float gridWidth = 2f;

	[SerializeField]
	private float gridHeight = 1f;

	[SerializeField]
	private int gridDimension = 3;

	[SerializeField]
	private TriggerEventNotifier triggerNotifier;

	[FormerlySerializedAs("_joinButtons")]
	[Header("Buttons")]
	[SerializeField]
	private GorillaPressableDelayButton[] _friendCardButtons;

	[SerializeField]
	private TextMeshProUGUI[] _friendCardButtonText;

	[SerializeField]
	private MeshRenderer _localPlayerFullyVisibleButton;

	[SerializeField]
	private MeshRenderer _localPlayerPublicOnlyButton;

	[SerializeField]
	private MeshRenderer _localPlayerFullyHiddenButton;

	[SerializeField]
	private MeshRenderer _removeFriendButton;

	[SerializeField]
	private FriendCard _localPlayerCard;

	[SerializeField]
	private MeshRenderer[] PageButtons;

	[SerializeField]
	private Material[] _buttonDefaultMaterials;

	[SerializeField]
	private Material[] _buttonActiveMaterials;

	[SerializeField]
	private Material[] _buttonAlertMaterials;

	[SerializeField]
	private Material[] _pageButtonDefaultMaterials;

	[SerializeField]
	private Material[] _pageButtonActiveMaterials;

	[SerializeField]
	private Material[] _pageButtonAlerttMaterials;

	private int cardsPerPage = 9;

	private int totalPages = 5;

	[SerializeField]
	private float pageButtonInactiveZPos;

	[SerializeField]
	private float pageButtonActiveZPos;

	private MeshRenderer[] _joinButtonRenderers;

	private bool inRemoveMode;

	private bool localPlayerAtDisplay;

	public enum ButtonState
	{
		Default,
		Active,
		Alert
	}
}
