using System;
using Modio;
using Modio.Errors;
using Modio.Mods;
using Modio.Unity;
using TMPro;
using UnityEngine;

public class CustomMapsModTile : CustomMapsScreenTouchPoint
{
	public Mod CurrentMod
	{
		get
		{
			return this.currentMod;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		this.defaultLogo = this.touchPointRenderer.sprite;
		this.highlight.SetActive(false);
	}

	public void ShowTileText(bool show, bool useMapName)
	{
		if (!show)
		{
			this.ratingsText.gameObject.SetActive(false);
			this.mapNameText.gameObject.SetActive(false);
			this.thumsbUp.SetActive(false);
			return;
		}
		if (useMapName)
		{
			this.mapNameText.gameObject.SetActive(true);
			this.ratingsText.gameObject.SetActive(false);
			this.thumsbUp.SetActive(false);
			return;
		}
		this.ratingsText.gameObject.SetActive(true);
		this.thumsbUp.SetActive(true);
		this.mapNameText.gameObject.SetActive(false);
	}

	public void ActivateTile(bool useMapName)
	{
		this.isActive = true;
		base.gameObject.SetActive(true);
		this.ShowTileText(true, useMapName);
		CustomMapsScreenTouchPoint.pressTime = Time.time;
	}

	public void DeactivateTile()
	{
		this.isActive = false;
		base.gameObject.SetActive(false);
		this.highlight.SetActive(false);
		this.ShowTileText(false, false);
		this.ResetLogo();
	}

	public override void PressButtonColourUpdate()
	{
	}

	protected override void OnButtonPressedEvent()
	{
	}

	public async void SetMod(Mod mod, bool useMapName)
	{
		this.ActivateTile(useMapName);
		this.touchPointRenderer.sprite = this.defaultLogo;
		this.highlight.SetActive(false);
		this.currentMod = mod;
		if (this.IsCurrentModHidden())
		{
			this.mapNameText.text = "HIDDEN MAP";
			this.ratingsText.text = "0%";
		}
		else
		{
			this.mapNameText.text = this.currentMod.Name;
			long num = this.currentMod.Stats.RatingsNegative + this.currentMod.Stats.RatingsPositive;
			string text;
			if (num < 1000L)
			{
				text = string.Format("({0})", num);
			}
			else if (num < 1000000L)
			{
				num = (long)Mathf.FloorToInt((float)(num / 100L));
				text = string.Format("({0}K)", num / 10L);
			}
			else
			{
				num = (long)Mathf.FloorToInt((float)(num / 100L));
				text = string.Format("({0}mil)", num / 10000L);
			}
			this.ratingsText.text = this.currentMod.Stats.RatingsPercent.ToString() + "% " + text;
			if (this.isDownloadingThumbnail)
			{
				this.newDownloadRequest = true;
			}
			else
			{
				this.isDownloadingThumbnail = true;
				Error error = new Error(ErrorCode.NONE);
				Texture2D tex = new Texture2D(320, 180);
				try
				{
					ValueTuple<Error, Texture2D> valueTuple = await mod.Logo.DownloadAsTexture2D(Mod.LogoResolution.X320_Y180);
					error = valueTuple.Item1;
					tex = valueTuple.Item2;
				}
				catch (Exception ex)
				{
					GTDev.Log<string>(string.Format("CustomMapsModTile::DownloadThumbnail error {0}", ex), null);
				}
				this.isDownloadingThumbnail = false;
				if (this.newDownloadRequest)
				{
					this.newDownloadRequest = false;
					this.SetMod(this.currentMod, useMapName);
				}
				else if (error)
				{
					GTDev.LogError<string>(string.Format("CustomMapsListScreen::DownloadThumbnail {0}", error), null);
				}
				else
				{
					this.touchPointRenderer.sprite = Sprite.Create(tex, new Rect(0f, 0f, 320f, 180f), new Vector2(0.5f, 0.5f));
				}
			}
		}
	}

	public void ResetLogo()
	{
		this.touchPointRenderer.sprite = this.defaultLogo;
	}

	public void ShowDetails()
	{
		CustomMapsTerminal.ShowDetailsScreen(this.currentMod);
	}

	public void HighlightTile()
	{
		this.highlight.SetActive(true);
	}

	public bool IsCurrentModHidden()
	{
		return this.currentMod.Creator == null || (!ModIOManager.IsLoggedIn() && this.currentMod.IsHidden());
	}

	[SerializeField]
	private TMP_Text ratingsText;

	[SerializeField]
	private TMP_Text mapNameText;

	[SerializeField]
	private GameObject thumsbUp;

	[SerializeField]
	private GameObject highlight;

	private const float LOGO_WIDTH = 320f;

	private const float LOGO_HEIGHT = 180f;

	private Mod currentMod;

	private Sprite defaultLogo;

	private bool isDownloadingThumbnail;

	private bool newDownloadRequest;

	private bool isActive;
}
