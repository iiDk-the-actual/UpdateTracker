using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Modio;
using Modio.Errors;
using Modio.Images;
using Modio.Mods;
using Modio.Unity;
using TMPro;
using UnityEngine;

public class NewMapsDisplay : MonoBehaviour
{
	public void OnEnable()
	{
		this.mapImage.gameObject.SetActive(false);
		this.mapInfoTMP.text = "";
		this.mapInfoTMP.gameObject.SetActive(false);
		UGCPermissionManager.SubscribeToUGCEnabled(new Action(this.OnUGCEnabled));
		UGCPermissionManager.SubscribeToUGCDisabled(new Action(this.OnUGCDisabled));
		if (!UGCPermissionManager.IsUGCDisabled)
		{
			if (!ModIOManager.IsInitialized() || !ModIOManager.TryGetNewMapsModId(out this.newMapsModId))
			{
				this.initCoroutine = base.StartCoroutine(this.DelayedInitialize());
			}
			else
			{
				if (this.newMapsModId == ModId.Null)
				{
					return;
				}
				this.Initialize();
			}
		}
		this.loadingText.gameObject.SetActive(true);
	}

	public void OnDisable()
	{
		if (this.initCoroutine != null)
		{
			base.StopCoroutine(this.initCoroutine);
			this.initCoroutine = null;
		}
		this.newMapsModProfile = null;
		this.newMapDatas.Clear();
		this.slideshowActive = false;
		this.slideshowIndex = 0;
		this.lastSlideshowUpdate = 0f;
		this.mapImage.gameObject.SetActive(false);
		this.mapInfoTMP.text = "";
		this.mapInfoTMP.gameObject.SetActive(false);
		this.loadingText.text = this.loadingString;
		this.loadingText.gameObject.SetActive(false);
		UGCPermissionManager.UnsubscribeFromUGCEnabled(new Action(this.OnUGCEnabled));
		UGCPermissionManager.UnsubscribeFromUGCDisabled(new Action(this.OnUGCDisabled));
	}

	private void OnUGCEnabled()
	{
		if (this.newMapDatas.IsNullOrEmpty<NewMapsDisplay.NewMapData>())
		{
			if (!ModIOManager.IsInitialized() || !ModIOManager.TryGetNewMapsModId(out this.newMapsModId))
			{
				this.initCoroutine = base.StartCoroutine(this.DelayedInitialize());
				return;
			}
			if (this.newMapsModId == ModId.Null)
			{
				return;
			}
			this.Initialize();
		}
	}

	private void OnUGCDisabled()
	{
		this.mapImage.gameObject.SetActive(false);
		this.mapInfoTMP.text = "";
		this.mapInfoTMP.gameObject.SetActive(false);
		this.loadingText.text = this.ugcDisabledString;
		this.loadingText.gameObject.SetActive(true);
	}

	private IEnumerator DelayedInitialize()
	{
		while (!ModIOManager.TryGetNewMapsModId(out this.newMapsModId))
		{
			yield return new WaitForSecondsRealtime(1f);
		}
		this.initCoroutine = null;
		if (this.newMapsModId == ModId.Null)
		{
			yield break;
		}
		this.Initialize();
		yield break;
	}

	private async Task<Error> Initialize()
	{
		Error error2;
		if (!this.requestingNewMapsModProfile && !this.downloadingImages)
		{
			this.requestingNewMapsModProfile = true;
			this.loadingText.text = this.loadingString;
			Error error = await ModIOManager.Initialize();
			if (error)
			{
				error2 = error;
			}
			else if (!base.isActiveAndEnabled)
			{
				error2 = Error.None;
			}
			else
			{
				ValueTuple<Error, Mod> valueTuple = await ModIOManager.GetMod(this.newMapsModId, false, null);
				error = valueTuple.Item1;
				this.newMapsModProfile = valueTuple.Item2;
				if (error)
				{
					GTDev.LogWarning<string>("[NewMapsDisplay::OnGetNewMapsModProfile] Failed to get NewMaps ModProfile " + string.Format("from mod.io: {0}", error), null);
					error2 = error;
				}
				else
				{
					this.newMapDatas.Clear();
					string[] array = this.newMapsModProfile.MetadataBlob.Split(';', StringSplitOptions.None);
					string text = "";
					foreach (string text2 in array)
					{
						if (text2.StartsWith("mapInfo:"))
						{
							text = text2.Substring(8);
							break;
						}
					}
					string[] mapInfoList = (text.IsNullOrEmpty() ? null : text.Split(',', StringSplitOptions.None));
					this.lazyImage = new LazyImage<Texture2D>(ImageCacheTexture2D.Instance, delegate(Texture2D loadedImage)
					{
						this.downloadingImage = false;
						this.lastDownloadedImage = loadedImage;
					}, null);
					this.downloadingImages = true;
					for (int i = 0; i < this.newMapsModProfile.Gallery.Length; i++)
					{
						this.downloadingImage = true;
						this.lazyImage.SetImage<Mod.GalleryResolution>(this.newMapsModProfile.Gallery[i], Mod.GalleryResolution.X320_Y180);
						while (this.downloadingImage)
						{
							await Task.Yield();
						}
						string text3 = ((mapInfoList != null && mapInfoList.Length > i) ? mapInfoList[i] : "");
						NewMapsDisplay.NewMapData newMapData = new NewMapsDisplay.NewMapData
						{
							image = this.lastDownloadedImage,
							info = text3
						};
						this.newMapDatas.Add(newMapData);
						this.lastDownloadedImage = null;
					}
					this.downloadingImages = false;
					if (!base.isActiveAndEnabled)
					{
						error2 = Error.None;
					}
					else
					{
						this.StartSlideshow();
						this.requestingNewMapsModProfile = false;
						error2 = Error.None;
					}
				}
			}
		}
		else
		{
			error2 = new Error(ErrorCode.UNKNOWN, "Initialization already in progress.");
		}
		return error2;
	}

	private void StartSlideshow()
	{
		if (this.newMapDatas.IsNullOrEmpty<NewMapsDisplay.NewMapData>())
		{
			return;
		}
		this.slideshowIndex = 0;
		this.slideshowActive = true;
		this.UpdateSlideshow();
	}

	public void Update()
	{
		if (!this.slideshowActive || Time.time - this.lastSlideshowUpdate < this.slideshowUpdateInterval)
		{
			return;
		}
		this.UpdateSlideshow();
	}

	private void UpdateSlideshow()
	{
		this.loadingText.gameObject.SetActive(false);
		this.lastSlideshowUpdate = Time.time;
		Texture2D image = this.newMapDatas[this.slideshowIndex].image;
		if (image != null)
		{
			Sprite sprite;
			if (!this.cachedTextures.TryGetValue(image, out sprite))
			{
				sprite = Sprite.Create(image, new Rect(0f, 0f, (float)image.width, (float)image.height), new Vector2(0.5f, 0.5f));
				this.cachedTextures.Add(image, sprite);
			}
			this.mapImage.sprite = sprite;
			this.mapImage.gameObject.SetActive(true);
		}
		else
		{
			this.mapImage.gameObject.SetActive(false);
		}
		this.mapInfoTMP.text = this.newMapDatas[this.slideshowIndex].info;
		this.mapInfoTMP.gameObject.SetActive(true);
		this.slideshowIndex++;
		if (this.slideshowIndex >= this.newMapDatas.Count)
		{
			this.slideshowIndex = 0;
		}
	}

	[SerializeField]
	private SpriteRenderer mapImage;

	[SerializeField]
	private TMP_Text loadingText;

	[Tooltip("DEPRECATED")]
	[SerializeField]
	private TMP_Text modNameText;

	[Tooltip("DEPRECATED")]
	[SerializeField]
	private TMP_Text modCreatorLabelText;

	[Tooltip("DEPRECATED")]
	[SerializeField]
	private TMP_Text modCreatorText;

	[SerializeField]
	private TMP_Text mapInfoTMP;

	[SerializeField]
	private float slideshowUpdateInterval = 1f;

	[SerializeField]
	private string loadingString = "LOADING...";

	[SerializeField]
	private string ugcDisabledString = "UGC DISABLED BY K-ID SETTINGS";

	private ModId newMapsModId = ModId.Null;

	private Mod newMapsModProfile;

	private List<NewMapsDisplay.NewMapData> newMapDatas = new List<NewMapsDisplay.NewMapData>();

	private bool slideshowActive;

	private int slideshowIndex;

	private float lastSlideshowUpdate;

	private bool requestingNewMapsModProfile;

	private LazyImage<Texture2D> lazyImage;

	private bool downloadingImages;

	private bool downloadingImage;

	private Texture2D lastDownloadedImage;

	private Coroutine initCoroutine;

	private Dictionary<Texture2D, Sprite> cachedTextures = new Dictionary<Texture2D, Sprite>();

	private struct NewMapData
	{
		public Texture2D image;

		public string info;
	}
}
