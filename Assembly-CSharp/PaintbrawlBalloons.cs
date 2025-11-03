using System;
using Photon.Realtime;
using UnityEngine;

public class PaintbrawlBalloons : MonoBehaviour
{
	protected void Awake()
	{
		this.matPropBlock = new MaterialPropertyBlock();
		this.renderers = new Renderer[this.balloons.Length];
		this.balloonsCachedActiveState = new bool[this.balloons.Length];
		for (int i = 0; i < this.balloons.Length; i++)
		{
			this.renderers[i] = this.balloons[i].GetComponentInChildren<Renderer>();
			this.balloonsCachedActiveState[i] = this.balloons[i].activeSelf;
		}
		this.colorShaderPropID = ShaderProps._Color;
	}

	protected void OnEnable()
	{
		this.UpdateBalloonColors();
	}

	protected void LateUpdate()
	{
		if (GorillaGameManager.instance != null && (this.bMgr != null || GorillaGameManager.instance.gameObject.GetComponent<GorillaPaintbrawlManager>() != null))
		{
			if (this.bMgr == null)
			{
				this.bMgr = GorillaGameManager.instance.gameObject.GetComponent<GorillaPaintbrawlManager>();
			}
			int playerLives = this.bMgr.GetPlayerLives(this.myRig.creator);
			for (int i = 0; i < this.balloons.Length; i++)
			{
				bool flag = playerLives >= i + 1;
				if (flag != this.balloonsCachedActiveState[i])
				{
					this.balloonsCachedActiveState[i] = flag;
					this.balloons[i].SetActive(flag);
					if (!flag)
					{
						this.PopBalloon(i);
					}
				}
			}
		}
		else if (GorillaGameManager.instance != null)
		{
			base.gameObject.SetActive(false);
		}
		this.UpdateBalloonColors();
	}

	private void PopBalloon(int i)
	{
		GameObject gameObject = ObjectPools.instance.Instantiate(this.balloonPopFXPrefab, true);
		gameObject.transform.position = this.balloons[i].transform.position;
		GorillaColorizableBase componentInChildren = gameObject.GetComponentInChildren<GorillaColorizableBase>();
		if (componentInChildren != null)
		{
			componentInChildren.SetColor(this.teamColor);
		}
	}

	public void UpdateBalloonColors()
	{
		if (this.bMgr != null && this.myRig.creator != null)
		{
			if (this.bMgr.OnRedTeam(this.myRig.creator))
			{
				this.teamColor = this.orangeColor;
			}
			else if (this.bMgr.OnBlueTeam(this.myRig.creator))
			{
				this.teamColor = this.blueColor;
			}
			else
			{
				this.teamColor = (this.myRig ? this.myRig.playerColor : this.defaultColor);
			}
		}
		if (this.teamColor != this.lastColor)
		{
			this.lastColor = this.teamColor;
			foreach (Renderer renderer in this.renderers)
			{
				if (renderer)
				{
					foreach (Material material in renderer.materials)
					{
						if (!(material == null))
						{
							if (material.HasProperty(ShaderProps._BaseColor))
							{
								material.SetColor(ShaderProps._BaseColor, this.teamColor);
							}
							if (material.HasProperty(ShaderProps._Color))
							{
								material.SetColor(ShaderProps._Color, this.teamColor);
							}
						}
					}
				}
			}
		}
	}

	public VRRig myRig;

	public GameObject[] balloons;

	public Color orangeColor;

	public Color blueColor;

	public Color defaultColor;

	public Color lastColor;

	public GameObject balloonPopFXPrefab;

	[HideInInspector]
	public GorillaPaintbrawlManager bMgr;

	public Player myPlayer;

	private int colorShaderPropID;

	private MaterialPropertyBlock matPropBlock;

	private bool[] balloonsCachedActiveState;

	private Renderer[] renderers;

	private Color teamColor;
}
