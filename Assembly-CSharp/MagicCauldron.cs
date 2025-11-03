using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Fusion;
using GorillaLocomotion;
using GorillaLocomotion.Gameplay;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Scripting;

[NetworkBehaviourWeaved(4)]
public class MagicCauldron : NetworkComponent
{
	private new void Awake()
	{
		this.currentIngredients.Clear();
		this.witchesComponent.Clear();
		this.currentStateElapsedTime = 0f;
		this.currentRecipeIndex = -1;
		this.ingredientIndex = -1;
		if (this.flyingWitchesContainer != null)
		{
			for (int i = 0; i < this.flyingWitchesContainer.transform.childCount; i++)
			{
				NoncontrollableBroomstick componentInChildren = this.flyingWitchesContainer.transform.GetChild(i).gameObject.GetComponentInChildren<NoncontrollableBroomstick>();
				this.witchesComponent.Add(componentInChildren);
				if (componentInChildren)
				{
					componentInChildren.gameObject.SetActive(false);
				}
			}
		}
		if (this.reusableFXContext == null)
		{
			this.reusableFXContext = new MagicCauldron.IngrediantFXContext();
		}
		if (this.reusableIngrediantArgs == null)
		{
			this.reusableIngrediantArgs = new MagicCauldron.IngredientArgs();
		}
		this.reusableFXContext.fxCallBack = new MagicCauldron.IngrediantFXContext.Callback(this.OnIngredientAdd);
	}

	private new void Start()
	{
		this.ChangeState(MagicCauldron.CauldronState.notReady);
	}

	private void LateUpdate()
	{
		this.UpdateState();
	}

	private IEnumerator LevitationSpellCoroutine()
	{
		GTPlayer.Instance.SetHalloweenLevitation(this.levitationStrength, this.levitationDuration, this.levitationBlendOutDuration, this.levitationBonusStrength, this.levitationBonusOffAtYSpeed, this.levitationBonusFullAtYSpeed);
		yield return new WaitForSeconds(this.levitationSpellDuration);
		GTPlayer.Instance.SetHalloweenLevitation(0f, this.levitationDuration, this.levitationBlendOutDuration, 0f, this.levitationBonusOffAtYSpeed, this.levitationBonusFullAtYSpeed);
		yield break;
	}

	private void ChangeState(MagicCauldron.CauldronState state)
	{
		this.currentState = state;
		if (base.IsMine)
		{
			this.currentStateElapsedTime = 0f;
		}
		bool flag = state == MagicCauldron.CauldronState.summoned;
		foreach (NoncontrollableBroomstick noncontrollableBroomstick in this.witchesComponent)
		{
			if (noncontrollableBroomstick.gameObject.activeSelf != flag)
			{
				noncontrollableBroomstick.gameObject.SetActive(flag);
			}
		}
		if (this.currentState == MagicCauldron.CauldronState.summoned && Vector3.Distance(GTPlayer.Instance.transform.position, base.transform.position) < this.levitationRadius)
		{
			base.StartCoroutine(this.LevitationSpellCoroutine());
		}
		switch (this.currentState)
		{
		case MagicCauldron.CauldronState.notReady:
			this.currentIngredients.Clear();
			this.UpdateCauldronColor(this.CauldronNotReadyColor);
			return;
		case MagicCauldron.CauldronState.ready:
			this.UpdateCauldronColor(this.CauldronActiveColor);
			return;
		case MagicCauldron.CauldronState.recipeCollecting:
			if (this.ingredientIndex >= 0 && this.ingredientIndex < this.allIngredients.Length)
			{
				this.UpdateCauldronColor(this.allIngredients[this.ingredientIndex].color);
				return;
			}
			break;
		case MagicCauldron.CauldronState.recipeActivated:
			if (this.audioSource && this.recipes[this.currentRecipeIndex].successAudio)
			{
				this.audioSource.GTPlayOneShot(this.recipes[this.currentRecipeIndex].successAudio, 1f);
			}
			if (this.successParticle)
			{
				this.successParticle.Play();
				return;
			}
			break;
		case MagicCauldron.CauldronState.summoned:
			break;
		case MagicCauldron.CauldronState.failed:
			this.currentIngredients.Clear();
			this.UpdateCauldronColor(this.CauldronFailedColor);
			this.audioSource.GTPlayOneShot(this.recipeFailedAudio, 1f);
			return;
		case MagicCauldron.CauldronState.cooldown:
			this.currentIngredients.Clear();
			this.UpdateCauldronColor(this.CauldronFailedColor);
			break;
		default:
			return;
		}
	}

	private void UpdateState()
	{
		if (base.IsMine)
		{
			this.currentStateElapsedTime += Time.deltaTime;
			switch (this.currentState)
			{
			case MagicCauldron.CauldronState.notReady:
			case MagicCauldron.CauldronState.ready:
				break;
			case MagicCauldron.CauldronState.recipeCollecting:
				if (this.currentStateElapsedTime >= this.maxTimeToAddAllIngredients && !this.CheckIngredients())
				{
					this.ChangeState(MagicCauldron.CauldronState.failed);
					return;
				}
				break;
			case MagicCauldron.CauldronState.recipeActivated:
				if (this.currentStateElapsedTime >= this.waitTimeToSummonWitches)
				{
					this.ChangeState(MagicCauldron.CauldronState.summoned);
					return;
				}
				break;
			case MagicCauldron.CauldronState.summoned:
				if (this.currentStateElapsedTime >= this.summonWitchesDuration)
				{
					this.ChangeState(MagicCauldron.CauldronState.cooldown);
					return;
				}
				break;
			case MagicCauldron.CauldronState.failed:
				if (this.currentStateElapsedTime >= this.recipeFailedDuration)
				{
					this.ChangeState(MagicCauldron.CauldronState.ready);
					return;
				}
				break;
			case MagicCauldron.CauldronState.cooldown:
				if (this.currentStateElapsedTime >= this.cooldownDuration)
				{
					this.ChangeState(MagicCauldron.CauldronState.ready);
				}
				break;
			default:
				return;
			}
		}
	}

	public void OnEventStart()
	{
		this.ChangeState(MagicCauldron.CauldronState.ready);
	}

	public void OnEventEnd()
	{
		this.ChangeState(MagicCauldron.CauldronState.notReady);
	}

	[PunRPC]
	public void OnIngredientAdd(int _ingredientIndex, PhotonMessageInfo info)
	{
		this.OnIngredientAddShared(_ingredientIndex, info);
	}

	[Rpc(RpcSources.StateAuthority, RpcTargets.All)]
	public unsafe void RPC_OnIngredientAdd(int _ingredientIndex, RpcInfo info = default(RpcInfo))
	{
		if (!this.InvokeRpc)
		{
			NetworkBehaviourUtils.ThrowIfBehaviourNotInitialized(this);
			if (base.Runner.Stage != SimulationStages.Resimulate)
			{
				int localAuthorityMask = base.Object.GetLocalAuthorityMask();
				if ((localAuthorityMask & 1) == 0)
				{
					NetworkBehaviourUtils.NotifyLocalSimulationNotAllowedToSendRpc("System.Void MagicCauldron::RPC_OnIngredientAdd(System.Int32,Fusion.RpcInfo)", base.Object, 1);
				}
				else
				{
					int num = 8;
					num += 4;
					if (!SimulationMessage.CanAllocateUserPayload(num))
					{
						NetworkBehaviourUtils.NotifyRpcPayloadSizeExceeded("System.Void MagicCauldron::RPC_OnIngredientAdd(System.Int32,Fusion.RpcInfo)", num);
					}
					else
					{
						if (base.Runner.HasAnyActiveConnections())
						{
							SimulationMessage* ptr = SimulationMessage.Allocate(base.Runner.Simulation, num);
							byte* ptr2 = (byte*)(ptr + 28 / sizeof(SimulationMessage));
							*(RpcHeader*)ptr2 = RpcHeader.Create(base.Object.Id, this.ObjectIndex, 1);
							int num2 = 8;
							*(int*)(ptr2 + num2) = _ingredientIndex;
							num2 += 4;
							ptr->Offset = num2 * 8;
							base.Runner.SendRpc(ptr);
						}
						if ((localAuthorityMask & 7) != 0)
						{
							info = RpcInfo.FromLocal(base.Runner, RpcChannel.Reliable, RpcHostMode.SourceIsServer);
							goto IL_0012;
						}
					}
				}
			}
			return;
		}
		this.InvokeRpc = false;
		IL_0012:
		this.OnIngredientAddShared(_ingredientIndex, info);
	}

	private void OnIngredientAddShared(int _ingredientIndex, PhotonMessageInfoWrapped info)
	{
		GorillaNot.IncrementRPCCall(info, "OnIngredientAdd");
		RigContainer rigContainer;
		if (!VRRigCache.Instance.TryGetVrrig(info.Sender, out rigContainer))
		{
			return;
		}
		this.reusableFXContext.playerSettings = rigContainer.Rig.fxSettings;
		this.reusableIngrediantArgs.key = _ingredientIndex;
		FXSystem.PlayFX<MagicCauldron.IngredientArgs>(FXType.HWIngredients, this.reusableFXContext, this.reusableIngrediantArgs, info);
	}

	private void OnIngredientAdd(int _ingredientIndex)
	{
		if (this.audioSource)
		{
			this.audioSource.GTPlayOneShot(this.ingredientAddedAudio, 1f);
		}
		if (!RoomSystem.AmITheHost)
		{
			return;
		}
		if (_ingredientIndex < 0 || _ingredientIndex >= this.allIngredients.Length || (this.currentState != MagicCauldron.CauldronState.ready && this.currentState != MagicCauldron.CauldronState.recipeCollecting))
		{
			return;
		}
		MagicIngredientType magicIngredientType = this.allIngredients[_ingredientIndex];
		Debug.Log(string.Format("Received ingredient RPC {0} = {1}", _ingredientIndex, magicIngredientType));
		MagicIngredientType magicIngredientType2 = null;
		if (this.recipes[0].recipeIngredients.Count > this.currentIngredients.Count)
		{
			magicIngredientType2 = this.recipes[0].recipeIngredients[this.currentIngredients.Count];
		}
		if (!(magicIngredientType == magicIngredientType2))
		{
			Debug.Log(string.Format("Failure: Expected ingredient {0}, got {1} from recipe[{2}]", magicIngredientType2, magicIngredientType, this.currentIngredients.Count));
			this.ChangeState(MagicCauldron.CauldronState.failed);
			return;
		}
		this.ingredientIndex = _ingredientIndex;
		this.currentIngredients.Add(magicIngredientType);
		if (this.CheckIngredients())
		{
			this.ChangeState(MagicCauldron.CauldronState.recipeActivated);
			return;
		}
		if (this.currentState == MagicCauldron.CauldronState.ready)
		{
			this.ChangeState(MagicCauldron.CauldronState.recipeCollecting);
			return;
		}
		this.UpdateCauldronColor(magicIngredientType.color);
	}

	private bool CheckIngredients()
	{
		foreach (MagicCauldron.Recipe recipe in this.recipes)
		{
			if (this.currentIngredients.SequenceEqual(recipe.recipeIngredients))
			{
				this.currentRecipeIndex = this.recipes.IndexOf(recipe);
				return true;
			}
		}
		return false;
	}

	private void UpdateCauldronColor(Color color)
	{
		if (this.bubblesParticle)
		{
			if (this.bubblesParticle.isPlaying)
			{
				if (this.currentState == MagicCauldron.CauldronState.failed || this.currentState == MagicCauldron.CauldronState.notReady)
				{
					this.bubblesParticle.Stop();
				}
			}
			else
			{
				this.bubblesParticle.Play();
			}
		}
		this.currentColor = this.cauldronColor;
		if (this.currentColor == color)
		{
			return;
		}
		if (this.rendr)
		{
			this._liquid.AnimateColorFromTo(this.cauldronColor, color, 1f);
			this.cauldronColor = color;
		}
		if (this.bubblesParticle)
		{
			this.bubblesParticle.main.startColor = color;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		ThrowableSetDressing componentInParent = other.GetComponentInParent<ThrowableSetDressing>();
		if (componentInParent == null || componentInParent.IngredientTypeSO == null || componentInParent.InHand())
		{
			return;
		}
		if (componentInParent.IsLocalOwnedWorldShareable)
		{
			if (componentInParent.IngredientTypeSO != null && (this.currentState == MagicCauldron.CauldronState.ready || this.currentState == MagicCauldron.CauldronState.recipeCollecting))
			{
				int num = this.allIngredients.IndexOfRef(componentInParent.IngredientTypeSO);
				Debug.Log(string.Format("Sending ingredient RPC {0} = {1}", componentInParent.IngredientTypeSO, num));
				base.SendRPC("OnIngredientAdd", RpcTarget.Others, new object[] { num });
				this.OnIngredientAdd(num);
			}
			componentInParent.StartRespawnTimer(0f);
		}
		if (componentInParent.IngredientTypeSO != null && this.splashParticle)
		{
			this.splashParticle.Play();
		}
	}

	internal override void OnDisable()
	{
		NetworkBehaviourUtils.InternalOnDisable(this);
		base.OnDisable();
		this.currentIngredients.Clear();
	}

	[Networked]
	[NetworkedWeaved(0, 4)]
	private unsafe MagicCauldron.MagicCauldronData Data
	{
		get
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing MagicCauldron.Data. Networked properties can only be accessed when Spawned() has been called.");
			}
			return *(MagicCauldron.MagicCauldronData*)(this.Ptr + 0);
		}
		set
		{
			if (this.Ptr == null)
			{
				throw new InvalidOperationException("Error when accessing MagicCauldron.Data. Networked properties can only be accessed when Spawned() has been called.");
			}
			*(MagicCauldron.MagicCauldronData*)(this.Ptr + 0) = value;
		}
	}

	public override void WriteDataFusion()
	{
		this.Data = new MagicCauldron.MagicCauldronData(this.currentStateElapsedTime, this.currentRecipeIndex, this.currentState, this.ingredientIndex);
	}

	public override void ReadDataFusion()
	{
		this.ReadDataShared(this.Data.CurrentStateElapsedTime, this.Data.CurrentRecipeIndex, this.Data.CurrentState, this.Data.IngredientIndex);
	}

	protected override void WriteDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (!info.Sender.IsMasterClient)
		{
			return;
		}
		stream.SendNext(this.currentStateElapsedTime);
		stream.SendNext(this.currentRecipeIndex);
		stream.SendNext(this.currentState);
		stream.SendNext(this.ingredientIndex);
	}

	protected override void ReadDataPUN(PhotonStream stream, PhotonMessageInfo info)
	{
		if (!info.Sender.IsMasterClient)
		{
			return;
		}
		float num = (float)stream.ReceiveNext();
		int num2 = (int)stream.ReceiveNext();
		MagicCauldron.CauldronState cauldronState = (MagicCauldron.CauldronState)stream.ReceiveNext();
		int num3 = (int)stream.ReceiveNext();
		this.ReadDataShared(num, num2, cauldronState, num3);
	}

	private void ReadDataShared(float stateElapsedTime, int recipeIndex, MagicCauldron.CauldronState state, int ingredientIndex)
	{
		MagicCauldron.CauldronState cauldronState = this.currentState;
		this.currentStateElapsedTime = stateElapsedTime;
		this.currentRecipeIndex = recipeIndex;
		this.currentState = state;
		this.ingredientIndex = ingredientIndex;
		if (cauldronState != this.currentState)
		{
			this.ChangeState(this.currentState);
			return;
		}
		if (this.currentState == MagicCauldron.CauldronState.recipeCollecting && ingredientIndex != ingredientIndex && ingredientIndex >= 0 && ingredientIndex < this.allIngredients.Length)
		{
			this.UpdateCauldronColor(this.allIngredients[ingredientIndex].color);
		}
	}

	[WeaverGenerated]
	public override void CopyBackingFieldsToState(bool A_1)
	{
		base.CopyBackingFieldsToState(A_1);
		this.Data = this._Data;
	}

	[WeaverGenerated]
	public override void CopyStateToBackingFields()
	{
		base.CopyStateToBackingFields();
		this._Data = this.Data;
	}

	[NetworkRpcWeavedInvoker(1, 1, 7)]
	[Preserve]
	[WeaverGenerated]
	protected unsafe static void RPC_OnIngredientAdd@Invoker(NetworkBehaviour behaviour, SimulationMessage* message)
	{
		byte* ptr = (byte*)(message + 28 / sizeof(SimulationMessage));
		int num = 8;
		int num2 = *(int*)(ptr + num);
		num += 4;
		int num3 = num2;
		RpcInfo rpcInfo = RpcInfo.FromMessage(behaviour.Runner, message, RpcHostMode.SourceIsServer);
		behaviour.InvokeRpc = true;
		((MagicCauldron)behaviour).RPC_OnIngredientAdd(num3, rpcInfo);
	}

	public List<MagicCauldron.Recipe> recipes = new List<MagicCauldron.Recipe>();

	public float maxTimeToAddAllIngredients = 30f;

	public float summonWitchesDuration = 20f;

	public float recipeFailedDuration = 5f;

	public float cooldownDuration = 30f;

	public MagicIngredientType[] allIngredients;

	public GameObject flyingWitchesContainer;

	[SerializeField]
	private AudioSource audioSource;

	public AudioClip ingredientAddedAudio;

	public AudioClip recipeFailedAudio;

	public ParticleSystem bubblesParticle;

	public ParticleSystem successParticle;

	public ParticleSystem splashParticle;

	public Color CauldronActiveColor;

	public Color CauldronFailedColor;

	[Tooltip("only if we are using the time of day event")]
	public Color CauldronNotReadyColor;

	private readonly List<NoncontrollableBroomstick> witchesComponent = new List<NoncontrollableBroomstick>();

	private readonly List<MagicIngredientType> currentIngredients = new List<MagicIngredientType>();

	private float currentStateElapsedTime;

	private MagicCauldron.CauldronState currentState;

	[SerializeField]
	private Renderer rendr;

	private Color cauldronColor;

	private Color currentColor;

	private int currentRecipeIndex;

	private int ingredientIndex;

	private float waitTimeToSummonWitches = 2f;

	[Space]
	[SerializeField]
	private MagicCauldronLiquid _liquid;

	private MagicCauldron.IngrediantFXContext reusableFXContext = new MagicCauldron.IngrediantFXContext();

	private MagicCauldron.IngredientArgs reusableIngrediantArgs = new MagicCauldron.IngredientArgs();

	public bool testLevitationAlwaysOn;

	public float levitationRadius;

	public float levitationSpellDuration;

	public float levitationStrength;

	public float levitationDuration;

	public float levitationBlendOutDuration;

	public float levitationBonusStrength;

	public float levitationBonusOffAtYSpeed;

	public float levitationBonusFullAtYSpeed;

	[WeaverGenerated]
	[DefaultForProperty("Data", 0, 4)]
	[DrawIf("IsEditorWritable", true, CompareOperator.Equal, DrawIfMode.ReadOnly)]
	private MagicCauldron.MagicCauldronData _Data;

	private enum CauldronState
	{
		notReady,
		ready,
		recipeCollecting,
		recipeActivated,
		summoned,
		failed,
		cooldown
	}

	[Serializable]
	public struct Recipe
	{
		public List<MagicIngredientType> recipeIngredients;

		public AudioClip successAudio;
	}

	private class IngredientArgs : FXSArgs
	{
		public int key;
	}

	private class IngrediantFXContext : IFXContextParems<MagicCauldron.IngredientArgs>
	{
		FXSystemSettings IFXContextParems<MagicCauldron.IngredientArgs>.settings
		{
			get
			{
				return this.playerSettings;
			}
		}

		void IFXContextParems<MagicCauldron.IngredientArgs>.OnPlayFX(MagicCauldron.IngredientArgs args)
		{
			this.fxCallBack(args.key);
		}

		public FXSystemSettings playerSettings;

		public MagicCauldron.IngrediantFXContext.Callback fxCallBack;

		public delegate void Callback(int key);
	}

	[NetworkStructWeaved(4)]
	[StructLayout(LayoutKind.Explicit, Size = 16)]
	private struct MagicCauldronData : INetworkStruct
	{
		public float CurrentStateElapsedTime { readonly get; set; }

		public int CurrentRecipeIndex { readonly get; set; }

		public MagicCauldron.CauldronState CurrentState { readonly get; set; }

		public int IngredientIndex { readonly get; set; }

		public MagicCauldronData(float stateElapsedTime, int recipeIndex, MagicCauldron.CauldronState state, int ingredientIndex)
		{
			this.CurrentStateElapsedTime = stateElapsedTime;
			this.CurrentRecipeIndex = recipeIndex;
			this.CurrentState = state;
			this.IngredientIndex = ingredientIndex;
		}
	}
}
