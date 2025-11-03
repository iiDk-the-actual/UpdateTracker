using System;
using System.Text;
using GorillaNetworking;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class GRUIPromotionBot : MonoBehaviourTick
{
	public string FormattedUserInfo()
	{
		GRPlayer grplayer = GRPlayer.Get(this.currentPlayerActorNumber);
		if (grplayer == null)
		{
			return "ERROR";
		}
		ValueTuple<int, int, int, int> gradePointDetails = GhostReactorProgression.GetGradePointDetails(grplayer.CurrentProgression.redeemedPoints);
		int item = gradePointDetails.Item3;
		int item2 = gradePointDetails.Item4;
		NetPlayer player = NetworkSystem.Instance.GetPlayer(this.currentPlayerActorNumber);
		string titleNameAndGrade = GhostReactorProgression.GetTitleNameAndGrade(grplayer.CurrentProgression.redeemedPoints);
		int num = 1000 + grplayer.ShiftCreditCapIncreases * 100;
		int num2 = grplayer.CurrentProgression.points - grplayer.CurrentProgression.redeemedPoints + item2;
		string text = ((player != null) ? player.SanitizedNickName : "RANDO MONKE");
		this.cachedStringBuilder.Clear();
		this.cachedStringBuilder.Append("<color=#808080>EMPLOYEE:</color>     " + text + "\n");
		this.cachedStringBuilder.Append("<color=#808080>TITLE:</color>        " + titleNameAndGrade + "\n");
		this.cachedStringBuilder.Append(string.Format("<color=#808080>XP:</color>           {0}/{1}\n", num2, item));
		if (grplayer == GRPlayer.GetLocal())
		{
			this.cachedStringBuilder.Append(string.Format("<color=#808080>CREDITS:</color>      <color=#00ff00>⑭ {0}</color>\n", grplayer.ShiftCredits));
			this.cachedStringBuilder.Append(string.Format("<color=#808080>CREDIT LIMIT:</color> <color=#00a000>⑭ {0}</color>\n", num));
			if (this.reactor != null && this.reactor.toolProgression != null)
			{
				int numberOfResearchPoints = this.reactor.toolProgression.GetNumberOfResearchPoints();
				this.cachedStringBuilder.Append(string.Format("<color=#808080>JUICE:</color>        <color=purple>⑮ {0}</color>\n", numberOfResearchPoints));
			}
			if (ProgressionManager.Instance != null)
			{
				int shinyRocksTotal = ProgressionManager.Instance.GetShinyRocksTotal();
				this.cachedStringBuilder.Append(string.Format("<color=#808080>SHINY ROCKS:</color>  <color=white>⑯ {0}</color>\n", shinyRocksTotal));
			}
		}
		return this.cachedStringBuilder.ToString();
	}

	public bool ActivePlayerEligibleForPromotion()
	{
		GRPlayer grplayer = GRPlayer.Get(this.currentPlayerActorNumber);
		if (grplayer == null)
		{
			return false;
		}
		ValueTuple<int, int, int, int> gradePointDetails = GhostReactorProgression.GetGradePointDetails(grplayer.CurrentProgression.redeemedPoints);
		int item = gradePointDetails.Item3;
		int item2 = gradePointDetails.Item4;
		return item - item2 < grplayer.CurrentProgression.points - grplayer.CurrentProgression.redeemedPoints;
	}

	public void Init(GhostReactor _reactor)
	{
		this.reactor = _reactor;
		this.currentPlayerActorNumber = -1;
		this.currentState = GRUIPromotionBot.PromotionBotState.WaitingForLogin;
	}

	public void Refresh()
	{
		this.RefreshPlayerData();
	}

	public override void Tick()
	{
		if (this.reactor == null || this.reactor.grManager == null || !this.reactor.grManager.IsAuthority())
		{
			return;
		}
		float time = Time.time;
		if (this.currentPlayerActorNumber != -1 && (this.timeLastDistanceCheck > time || time > this.timeLastDistanceCheck + this.timeBetweenDistanceChecks))
		{
			GRPlayer grplayer = GRPlayer.Get(this.currentPlayerActorNumber);
			if (grplayer == null || (base.transform.position - grplayer.transform.position).sqrMagnitude > this.distanceForAutoLogout * this.distanceForAutoLogout)
			{
				this.SwitchState(GRUIPromotionBot.PromotionBotState.WaitingForLogin, false);
			}
		}
	}

	public bool CheckIsActivePlayer()
	{
		Object @object = GRPlayer.Get(VRRig.LocalRig);
		GRPlayer grplayer = GRPlayer.Get(this.currentPlayerActorNumber);
		return @object == grplayer;
	}

	public void UpPressed()
	{
		if (!this.CheckIsActivePlayer())
		{
			return;
		}
		GRUIPromotionBot.PromotionBotState promotionBotState = this.currentState;
		if (promotionBotState != GRUIPromotionBot.PromotionBotState.ChooseCreditIncrease)
		{
			if (promotionBotState == GRUIPromotionBot.PromotionBotState.ChoosePurchaseCredits)
			{
				this.SwitchState(GRUIPromotionBot.PromotionBotState.ChooseCreditIncrease, false);
				return;
			}
		}
		else
		{
			this.SwitchState(GRUIPromotionBot.PromotionBotState.ChoosePromotion, false);
		}
	}

	public void DownPressed()
	{
		if (!this.CheckIsActivePlayer())
		{
			return;
		}
		GRUIPromotionBot.PromotionBotState promotionBotState = this.currentState;
		if (promotionBotState == GRUIPromotionBot.PromotionBotState.ChoosePromotion)
		{
			this.SwitchState(GRUIPromotionBot.PromotionBotState.ChooseCreditIncrease, false);
			return;
		}
		if (promotionBotState != GRUIPromotionBot.PromotionBotState.ChooseCreditIncrease)
		{
			return;
		}
		this.SwitchState(GRUIPromotionBot.PromotionBotState.ChoosePurchaseCredits, false);
	}

	public void YesPressed()
	{
		if (!this.CheckIsActivePlayer())
		{
			return;
		}
		switch (this.currentState)
		{
		case GRUIPromotionBot.PromotionBotState.ChoosePromotion:
			this.AttemptPromotion();
			return;
		case GRUIPromotionBot.PromotionBotState.ChooseCreditIncrease:
			this.AttemptPurchaseShiftCreditIncrease();
			return;
		case GRUIPromotionBot.PromotionBotState.ChoosePurchaseCredits:
			this.SwitchState(GRUIPromotionBot.PromotionBotState.ConfirmPurchaseCredits, false);
			return;
		case GRUIPromotionBot.PromotionBotState.ConfirmPurchaseCredits:
			this.SwitchState(GRUIPromotionBot.PromotionBotState.ChoosePurchaseCredits, false);
			return;
		default:
			return;
		}
	}

	public void NoPressed()
	{
		if (!this.CheckIsActivePlayer())
		{
			return;
		}
		GRUIPromotionBot.PromotionBotState promotionBotState = this.currentState;
		if (promotionBotState - GRUIPromotionBot.PromotionBotState.ChoosePromotion > 2)
		{
			if (promotionBotState == GRUIPromotionBot.PromotionBotState.ConfirmPurchaseCredits)
			{
				this.AttemptPurchaseShiftCreditRefillToMax();
				return;
			}
		}
		else
		{
			this.SwitchState(GRUIPromotionBot.PromotionBotState.WaitingForLogin, false);
		}
	}

	public void SwitchState(GRUIPromotionBot.PromotionBotState newState, bool fromRPC = false)
	{
		GRPlayer grplayer = GRPlayer.Get(this.currentPlayerActorNumber);
		GRPlayer grplayer2 = GRPlayer.Get(VRRig.LocalRig);
		if (grplayer2 == null)
		{
			return;
		}
		this.RefreshPlayerData();
		GRUIPromotionBot.PromotionBotState promotionBotState = this.currentState;
		this.currentState = newState;
		this.SetScreenVisibility();
		this.SetMenuText(newState);
		switch (newState)
		{
		case GRUIPromotionBot.PromotionBotState.ChoosePromotion:
			if (this.ActivePlayerEligibleForPromotion())
			{
				this.descriptionText.text = "<color=#c0c0c0>     YOU ARE ELIGIBLE FOR A PROMOTION!\n     PRESS 'YES' TO CONTINUE</color>";
			}
			else
			{
				this.descriptionText.text = "<color=#c04040>     YOU ARE NOT ELIGIBLE FOR A PROMOTION\n     EARN MORE XP BY COMPLETING SHIFT GOALS</color>";
			}
			break;
		case GRUIPromotionBot.PromotionBotState.ChooseCreditIncrease:
			if (grplayer.ShiftCreditCapIncreases != grplayer.ShiftCreditCapIncreasesMax)
			{
				this.descriptionText.text = "<color=#c0c0c0>     INCREASE CREDIT LIMIT BY <color=#00ff00>⑭ 100</color>\n     FOR <color=purple>⑮ 2</color> JUICE?</color>";
			}
			else
			{
				this.descriptionText.text = "<color=#c0c0c0>     CREDIT LIMIT CAN'T BE INCREASED AT THIS TIME\n</color>";
			}
			break;
		case GRUIPromotionBot.PromotionBotState.ChoosePurchaseCredits:
			if (grplayer == null)
			{
				this.descriptionText.text = "No active player";
			}
			else
			{
				int purchaseToCreditCapAmount = this.GetPurchaseToCreditCapAmount();
				if (purchaseToCreditCapAmount > 0)
				{
					this.descriptionText.text = string.Format("<color=#c0c0c0>     PURCHASE <color=#00ff00>+⑭{0}</color> CREDITS\n     FOR <color=white>100 SHINY ROCKS?</color>", purchaseToCreditCapAmount);
				}
				else
				{
					this.descriptionText.text = "<color=#c0c0c0>     YOU ARE AT FULL CREDITS";
				}
			}
			break;
		case GRUIPromotionBot.PromotionBotState.ConfirmPurchaseCredits:
		{
			int purchaseToCreditCapAmount2 = this.GetPurchaseToCreditCapAmount();
			this.descriptionText.text = string.Format("<color=#c0c0c0>     CONFIRM PURCHASE <color=#00ff00>+⑭{0}</color>\n     FOR <color=white>100 SHINY ROCKS?</color>", purchaseToCreditCapAmount2);
			break;
		}
		}
		if (this.currentState == GRUIPromotionBot.PromotionBotState.ConfirmPurchaseCredits)
		{
			this.yesText.text = "<size=0.4>CANCEL</size>";
			this.noText.text = "<size=0.4>CONFIRM</size>";
		}
		else
		{
			if (this.yesText.text != "YES")
			{
				this.yesText.text = "YES";
			}
			if (this.noText.text != "NO")
			{
				this.noText.text = "NO";
			}
		}
		if (this.reactor != null && this.reactor.grManager != null && !fromRPC && (grplayer == grplayer2 || this.reactor.grManager.IsAuthority()))
		{
			this.reactor.grManager.PromotionBotActivePlayerRequest((int)this.currentState);
		}
	}

	public int GetPurchaseToCreditCapAmount()
	{
		GRPlayer grplayer = GRPlayer.Get(this.currentPlayerActorNumber);
		int shiftCredits = grplayer.ShiftCredits;
		int num = 1000 + grplayer.ShiftCreditCapIncreases * 100;
		return Math.Max(0, num - shiftCredits);
	}

	public void CelebratePromotion()
	{
		GRPlayer grplayer = GRPlayer.Get(this.currentPlayerActorNumber);
		if (grplayer == null)
		{
			return;
		}
		this.particlesGO.SetActive(false);
		this.particlesGO.SetActive(true);
		this.levelUpSound.Play();
		this.popSound.Play();
		PlayerGameEvents.MiscEvent(GRUIPromotionBot.EVENT_PROMOTED, 1);
		grplayer.SendRankUpTelemetry(GhostReactorProgression.GetTitleNameAndGrade(grplayer.CurrentProgression.redeemedPoints));
	}

	public void SetMenuText(GRUIPromotionBot.PromotionBotState menuState)
	{
		switch (menuState)
		{
		case GRUIPromotionBot.PromotionBotState.ChoosePromotion:
			this.menuText.text = "-> REQUEST PROMOTION\n   INCREASE CREDIT LIMIT\n   BRIBE ACCOUNTING FOR CREDITS\n";
			return;
		case GRUIPromotionBot.PromotionBotState.ChooseCreditIncrease:
			this.menuText.text = "   REQUEST PROMOTION\n-> INCREASE CREDIT LIMIT\n   BRIBE ACCOUNTING FOR CREDITS\n";
			return;
		case GRUIPromotionBot.PromotionBotState.ChoosePurchaseCredits:
		case GRUIPromotionBot.PromotionBotState.ConfirmPurchaseCredits:
			this.menuText.text = "   REQUEST PROMOTION\n   INCREASE CREDIT LIMIT\n-> BRIBE ACCOUNTING FOR CREDITS\n";
			return;
		default:
			return;
		}
	}

	public void SetScreenVisibility()
	{
		this.startScreenText.gameObject.SetActive(this.currentState == GRUIPromotionBot.PromotionBotState.WaitingForLogin);
		this.userInfo.gameObject.SetActive(this.currentState > GRUIPromotionBot.PromotionBotState.WaitingForLogin);
		this.menuText.gameObject.SetActive(this.currentState > GRUIPromotionBot.PromotionBotState.WaitingForLogin);
		this.descriptionText.gameObject.SetActive(this.currentState > GRUIPromotionBot.PromotionBotState.WaitingForLogin);
		this.purchaseSuccessText.gameObject.SetActive(false);
	}

	public void RefreshPlayerData()
	{
		this.userInfo.text = this.FormattedUserInfo();
	}

	public void OnPurchaseCallback(bool success)
	{
		if (success)
		{
			this.purchaseSuccessText.text = "<color=#80ff80>     PURCHASE SUCCEEDED!</color>";
			this.RefreshPlayerData();
			this.purchaseSuccessText.gameObject.SetActive(true);
			UnityEvent onSucceeded = this.scanner.onSucceeded;
			if (onSucceeded == null)
			{
				return;
			}
			onSucceeded.Invoke();
			return;
		}
		else
		{
			this.purchaseSuccessText.text = "<color=#ff8080>     FAILED PURCHASE. NO CHARGE.</color>";
			this.RefreshPlayerData();
			this.purchaseSuccessText.gameObject.SetActive(true);
			UnityEvent onFailed = this.scanner.onFailed;
			if (onFailed == null)
			{
				return;
			}
			onFailed.Invoke();
			return;
		}
	}

	public void OnJuiceUpdated()
	{
		this.RefreshPlayerData();
	}

	public void OnGetShiftCredit(string mothershipId, int credit)
	{
		GRPlayer grplayer = GRPlayer.Get(this.currentPlayerActorNumber);
		if (grplayer != null && grplayer.mothershipId == mothershipId)
		{
			this.RefreshPlayerData();
		}
	}

	public void OnShinyRocksUpdated()
	{
		GRPlayer grplayer = GRPlayer.Get(this.currentPlayerActorNumber);
		if (grplayer != null && grplayer.gamePlayer.IsLocal())
		{
			this.RefreshPlayerData();
		}
	}

	public void OnGetShiftCreditCapData(string mothershipId, int creditCap, int creditCapMax)
	{
		GRPlayer grplayer = GRPlayer.Get(this.currentPlayerActorNumber);
		if (grplayer != null && grplayer.mothershipId == mothershipId)
		{
			this.RefreshPlayerData();
		}
	}

	public void AttemptPromotion()
	{
		GRPlayer grplayer = GRPlayer.Get(this.currentPlayerActorNumber);
		if (grplayer && grplayer.AttemptPromotion() && this.reactor != null && this.reactor.grManager != null)
		{
			this.CelebratePromotion();
			this.RefreshPlayerData();
			this.RefreshActivePlayerBadge();
			string titleName = GhostReactorProgression.GetTitleName(grplayer.CurrentProgression.redeemedPoints);
			int grade = GhostReactorProgression.GetGrade(grplayer.CurrentProgression.redeemedPoints);
			this.purchaseSuccessText.text = string.Format("CONGRATULATIONS, {0} {1}!", titleName, grade);
			this.purchaseSuccessText.gameObject.SetActive(true);
		}
	}

	public void AttemptPurchaseShiftCreditIncrease()
	{
		GRPlayer grplayer = GRPlayer.Get(this.currentPlayerActorNumber);
		if (grplayer == null)
		{
			Debug.Log("AttemptPurchaseShiftCreditIncrease currentPlayer null");
			return;
		}
		if (grplayer.ShiftCreditCapIncreases == grplayer.ShiftCreditCapIncreasesMax)
		{
			return;
		}
		Debug.Log(string.Format("AttemptPurchaseShiftCreditIncrease currentPlayer ShiftCreditCapIncreases {0} ShiftCreditCapIncreasesMax {1}", grplayer.ShiftCreditCapIncreases, grplayer.ShiftCreditCapIncreasesMax));
		int num = 2;
		if (grplayer != null && grplayer.gamePlayer.IsLocal() && grplayer.ShiftCreditCapIncreases < grplayer.ShiftCreditCapIncreasesMax && this.reactor.toolProgression.GetNumberOfResearchPoints() >= num && ProgressionManager.Instance != null)
		{
			ProgressionManager.Instance.PurchaseShiftCreditCapIncrease();
		}
		this.RefreshPlayerData();
	}

	public void AttemptPurchaseShiftCreditRefillToMax()
	{
		if (this.GetPurchaseToCreditCapAmount() == 0)
		{
			return;
		}
		GRPlayer grplayer = GRPlayer.Get(this.currentPlayerActorNumber);
		if (grplayer == null)
		{
			Debug.Log("AttemptPurchaseShiftCreditIncrease currentPlayer null");
			return;
		}
		int num = 1000;
		int num2 = 100;
		int num3 = num + grplayer.ShiftCreditCapIncreases * num2;
		Debug.Log(string.Format("AttemptPurchaseShiftCreditIncrease currentPlayer ShiftCredits {0} ShiftCreditMax {1}", grplayer.ShiftCredits, num3));
		if (grplayer != null && grplayer.gamePlayer.IsLocal() && grplayer.ShiftCredits < num3)
		{
			int num4 = 100;
			if (ProgressionManager.Instance != null && ProgressionManager.Instance.GetShinyRocksTotal() >= num4)
			{
				ProgressionManager.Instance.PurchaseShiftCredit();
			}
		}
		this.RefreshPlayerData();
		this.SwitchState(GRUIPromotionBot.PromotionBotState.ChoosePurchaseCredits, false);
	}

	public void PlayerSwipedID()
	{
		if (this.reactor == null || this.reactor.grManager == null)
		{
			return;
		}
		if (this.currentPlayerActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
		{
			UnityEvent onSucceeded = this.scanner.onSucceeded;
			if (onSucceeded == null)
			{
				return;
			}
			onSucceeded.Invoke();
			return;
		}
		else if (this.currentPlayerActorNumber != -1 && GRPlayer.Get(this.currentPlayerActorNumber) != null)
		{
			UnityEvent onFailed = this.scanner.onFailed;
			if (onFailed == null)
			{
				return;
			}
			onFailed.Invoke();
			return;
		}
		else
		{
			this.reactor.grManager.PromotionBotActivePlayerRequest(6);
			UnityEvent onSucceeded2 = this.scanner.onSucceeded;
			if (onSucceeded2 == null)
			{
				return;
			}
			onSucceeded2.Invoke();
			return;
		}
	}

	public void RefreshActivePlayerBadge()
	{
		if (this.currentPlayerActorNumber == -1)
		{
			return;
		}
		GRPlayer grplayer = GRPlayer.Get(this.currentPlayerActorNumber);
		if (grplayer != null && this.currentPlayerActorNumber != -1)
		{
			NetPlayer netPlayerByID = NetworkSystem.Instance.GetNetPlayerByID(this.currentPlayerActorNumber);
			if (netPlayerByID != null && grplayer.badge != null)
			{
				grplayer.badge.RefreshText(netPlayerByID);
			}
		}
	}

	public void SetActivePlayerStateChange(int actorNumber, int state)
	{
		if (state == 0)
		{
			this.RefreshActivePlayerBadge();
			actorNumber = -1;
		}
		bool flag = this.currentPlayerActorNumber == PhotonNetwork.LocalPlayer.ActorNumber;
		bool flag2 = actorNumber == PhotonNetwork.LocalPlayer.ActorNumber;
		if (flag && !flag2)
		{
			if (ProgressionManager.Instance != null)
			{
				ProgressionManager.Instance.OnPurchaseShiftCredit -= this.OnPurchaseCallback;
				ProgressionManager.Instance.OnPurchaseShiftCreditCapIncrease -= this.OnPurchaseCallback;
				ProgressionManager.Instance.OnInventoryUpdated -= this.OnJuiceUpdated;
				ProgressionManager.Instance.OnGetShiftCredit -= this.OnGetShiftCredit;
				ProgressionManager.Instance.OnGetShiftCreditCapData -= this.OnGetShiftCreditCapData;
			}
			if (CosmeticsController.instance != null)
			{
				CosmeticsController instance = CosmeticsController.instance;
				instance.OnGetCurrency = (Action)Delegate.Remove(instance.OnGetCurrency, new Action(this.OnShinyRocksUpdated));
			}
		}
		else if (!flag && flag2)
		{
			if (ProgressionManager.Instance != null)
			{
				ProgressionManager.Instance.OnPurchaseShiftCredit += this.OnPurchaseCallback;
				ProgressionManager.Instance.OnPurchaseShiftCreditCapIncrease += this.OnPurchaseCallback;
				ProgressionManager.Instance.OnInventoryUpdated += this.OnJuiceUpdated;
				ProgressionManager.Instance.OnGetShiftCredit += this.OnGetShiftCredit;
				ProgressionManager.Instance.OnGetShiftCreditCapData += this.OnGetShiftCreditCapData;
			}
			if (CosmeticsController.instance != null)
			{
				CosmeticsController instance2 = CosmeticsController.instance;
				instance2.OnGetCurrency = (Action)Delegate.Combine(instance2.OnGetCurrency, new Action(this.OnShinyRocksUpdated));
			}
		}
		this.currentPlayerActorNumber = actorNumber;
		this.SwitchState((GRUIPromotionBot.PromotionBotState)state, true);
	}

	public int GetCurrentPlayerActorNumber()
	{
		return this.currentPlayerActorNumber;
	}

	private static string EVENT_PROMOTED = "GRPromoted";

	private GhostReactor reactor;

	public TMP_Text startScreenText;

	public TMP_Text userInfo;

	public TMP_Text menuText;

	public TMP_Text descriptionText;

	public TMP_Text yesText;

	public TMP_Text noText;

	public TMP_Text purchaseSuccessText;

	public IDCardScanner scanner;

	public GameObject particlesGO;

	public AudioSource levelUpSound;

	public AudioSource popSound;

	private string defaultText = "-N/A-\n-N/A-\n-N/A-\n-N/A-\n-N/A-\n\n-N/A-";

	private string promotionTextStr1 = "CONGRATULATIONS\n ";

	private string promotionTextStr2 = ".\n\nYOU ARE NOW A GRADE ";

	private string promotionTextStr3 = ".\n\nYOU MAY TAKE TWO UNPAID MINUTES TO CELEBRATE, THEN RETURN TO WORK.";

	private string inertButtonText = "-";

	private string buttonReturnText = "-RETURN-";

	private string requestPromotionText = "REQUEST PROMOTION";

	public const string newLine = "\n";

	public int currentPlayerActorNumber;

	public GRUIPromotionBot.PromotionBotState currentState;

	public float timeOutTime;

	public float distanceForAutoLogout = 2.5f;

	private StringBuilder cachedStringBuilder = new StringBuilder(512);

	private float timeLastDistanceCheck;

	private float timeBetweenDistanceChecks = 0.5f;

	public enum PromotionBotState
	{
		WaitingForLogin,
		ChoosePromotion,
		ChooseCreditIncrease,
		ChoosePurchaseCredits,
		ConfirmPurchaseCredits,
		CelebratePromotion,
		TryingLogIn
	}
}
