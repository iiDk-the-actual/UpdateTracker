using System;
using System.Threading.Tasks;
using GorillaNetworking;
using PlayFab;
using UnityEngine;
using UnityEngine.Networking;

public class TextureFromURL : MonoBehaviour
{
	private void OnEnable()
	{
		if (this.data.Length == 0)
		{
			return;
		}
		if (this.source == TextureFromURL.Source.TitleData)
		{
			this.LoadFromTitleData();
			return;
		}
		this.applyRemoteTexture(this.data);
	}

	private async void LoadFromTitleData()
	{
		int attempt = 0;
		while (attempt < this.maxTitleDataAttempts && PlayFabTitleDataCache.Instance == null)
		{
			await Task.Delay(1000);
			attempt++;
		}
		if (PlayFabTitleDataCache.Instance != null)
		{
			PlayFabTitleDataCache.Instance.GetTitleData(this.data, new Action<string>(this.OnTitleDataRequestComplete), new Action<PlayFabError>(this.OnPlayFabError), false);
		}
	}

	private void OnDisable()
	{
		if (this.texture != null)
		{
			Object.Destroy(this.texture);
			this.texture = null;
		}
	}

	private void OnPlayFabError(PlayFabError error)
	{
	}

	private void OnTitleDataRequestComplete(string imageUrl)
	{
		imageUrl = imageUrl.Replace("\\r", "\r").Replace("\\n", "\n");
		if (imageUrl[0] == '"' && imageUrl[imageUrl.Length - 1] == '"')
		{
			imageUrl = imageUrl.Substring(1, imageUrl.Length - 2);
		}
		this.applyRemoteTexture(imageUrl);
	}

	private async void applyRemoteTexture(string imageUrl)
	{
		Texture2D texture2D = await this.GetRemoteTexture(imageUrl);
		this.texture = texture2D;
		if (this.texture != null)
		{
			this._renderer.material.mainTexture = this.texture;
		}
	}

	private async Task<Texture2D> GetRemoteTexture(string url)
	{
		Texture2D texture2D;
		using (UnityWebRequest wr = UnityWebRequestTexture.GetTexture(url))
		{
			UnityWebRequestAsyncOperation asyncOp = wr.SendWebRequest();
			while (!asyncOp.isDone)
			{
				await Task.Delay(1000);
			}
			if (wr.result == UnityWebRequest.Result.Success)
			{
				texture2D = DownloadHandlerTexture.GetContent(wr);
			}
			else
			{
				texture2D = null;
			}
		}
		return texture2D;
	}

	[SerializeField]
	private Renderer _renderer;

	[SerializeField]
	private TextureFromURL.Source source;

	[Tooltip("If Source is set to 'TitleData' Data should be the id of the title data entry that defines an image URL. If Source is set to 'URL' Data should be a URL that points to an image.")]
	[SerializeField]
	private string data;

	private Texture2D texture;

	private int maxTitleDataAttempts = 10;

	private enum Source
	{
		TitleData,
		URL
	}
}
