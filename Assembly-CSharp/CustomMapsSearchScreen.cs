using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GorillaExtensions;
using GorillaTagScripts.VirtualStumpCustomMaps.UI;
using Modio;
using Modio.API;
using Modio.Mods;
using TMPro;
using UnityEngine;

public class CustomMapsSearchScreen : CustomMapsTerminalScreen
{
	public override void Show()
	{
		base.Show();
		this.searchPhraseText.gameObject.SetActive(true);
		this.customMapsGalleryView.gameObject.SetActive(false);
		this.searchMessageText.gameObject.SetActive(false);
		this.leftPageButton.gameObject.SetActive(false);
		this.rightPageButton.gameObject.SetActive(false);
		this.searchedMods.Clear();
		this.filteredSearchedMods.Clear();
		this.displayedMods.Clear();
		this.searchPhraseText.text = this.defaultSearchString;
		this.searchPhrase = string.Empty;
		this.currentSearchModsRequestPage = 0;
		this.currentModPage = 0;
	}

	public override void Hide()
	{
		base.Hide();
		this.customMapsGalleryView.ShowTileText(false, true);
	}

	public override void Initialize()
	{
	}

	public void ReturnFromDetailsScreen()
	{
		base.Show();
		this.customMapsGalleryView.ShowTileText(true, true);
	}

	public override void PressButton(CustomMapKeyboardBinding pressedButton)
	{
		if (Time.time < this.showTime + this.activationTime)
		{
			return;
		}
		if (!CustomMapsTerminal.IsDriver)
		{
			return;
		}
		if (CustomMapKeyboardBinding.tile1 <= pressedButton && pressedButton <= CustomMapKeyboardBinding.tile6 && !this.customMapsGalleryView.IsNull())
		{
			this.customMapsGalleryView.ShowDetailsForEntry(pressedButton - CustomMapKeyboardBinding.tile1);
		}
		if (pressedButton < CustomMapKeyboardBinding.up)
		{
			string text = this.searchPhrase;
			int num = (int)pressedButton;
			this.searchPhrase = text + num.ToString();
			this.RefreshSearchText();
			return;
		}
		if (pressedButton > CustomMapKeyboardBinding.option3 && pressedButton < CustomMapKeyboardBinding.at)
		{
			this.searchPhrase += pressedButton.ToString();
			this.RefreshSearchText();
			return;
		}
		if (pressedButton != CustomMapKeyboardBinding.delete)
		{
			if (pressedButton != CustomMapKeyboardBinding.enter)
			{
				switch (pressedButton)
				{
				case CustomMapKeyboardBinding.goback:
					if (this.loadingSearchMods)
					{
						return;
					}
					CustomMapsTerminal.ReturnFromSearchScreen();
					break;
				case CustomMapKeyboardBinding.left:
					this.currentModPage--;
					this.RefreshScreenState();
					break;
				case CustomMapKeyboardBinding.right:
					this.currentModPage++;
					this.RefreshScreenState();
					break;
				}
			}
			else
			{
				if (this.loadingSearchMods)
				{
					return;
				}
				this.searchedMods.Clear();
				this.filteredSearchedMods.Clear();
				this.currentSearchModsRequestPage = 0;
				this.searchMessageText.gameObject.SetActive(true);
				this.searchMessageText.text = this.searchingString;
				this.RetrieveMods();
			}
		}
		else if (!this.searchPhrase.IsNullOrEmpty())
		{
			this.searchPhrase = this.searchPhrase.Remove(this.searchPhrase.Length - 1);
		}
		this.RefreshSearchText();
	}

	private void RefreshSearchText()
	{
		if (this.searchPhrase.IsNullOrEmpty())
		{
			this.searchPhraseText.text = this.defaultSearchString;
			return;
		}
		this.searchPhraseText.text = this.searchPhrase;
	}

	private async Task RetrieveMods()
	{
		if (!this.loadingSearchMods)
		{
			this.loadingSearchMods = true;
			int num = this.currentSearchModsRequestPage;
			this.currentSearchModsRequestPage = num + 1;
			ModSearchFilter modSearchFilter = new ModSearchFilter(num, this.numModsPerRequest);
			Filtering filtering = Filtering.Like;
			modSearchFilter.ClearSearchPhrases(filtering);
			if (!this.searchPhrase.IsNullOrEmpty())
			{
				modSearchFilter.AddSearchPhrase(this.searchPhrase, filtering);
			}
			ValueTuple<Error, ModioPage<Mod>> valueTuple = await ModIOManager.GetMods(modSearchFilter.GetModsFilter());
			Error item = valueTuple.Item1;
			ModioPage<Mod> item2 = valueTuple.Item2;
			this.loadingSearchMods = false;
			if (item)
			{
				this.errorLoadingSearchMods = true;
				this.errorMessage = item.GetMessage();
				GTDev.LogError<string>("[CustomMapsListScreen::OnAvailableModsRetrieved] Failed to retrieve mods. Error: " + item.GetMessage(), null);
			}
			else
			{
				this.totalSearchMods = (int)item2.TotalSearchResults;
				this.searchedMods.AddRange(item2.Data);
				this.FilterSearchMods();
			}
			this.RefreshScreenState();
		}
	}

	private void FilterSearchMods()
	{
		if (this.searchedMods.IsNullOrEmpty<Mod>())
		{
			return;
		}
		this.filteredSearchedMods.Clear();
		foreach (Mod mod in this.searchedMods)
		{
			ModId modId;
			if (ModIOManager.TryGetNewMapsModId(out modId) && mod.Id == modId)
			{
				this.totalSearchMods = Mathf.Max(0, this.totalSearchMods - 1);
			}
			else
			{
				this.filteredSearchedMods.Add(mod);
			}
		}
	}

	private void RefreshScreenState()
	{
		this.searchMessageText.gameObject.SetActive(false);
		this.customMapsGalleryView.ResetGallery();
		this.customMapsGalleryView.gameObject.SetActive(false);
		this.displayedMods.Clear();
		if (this.errorLoadingSearchMods)
		{
			this.searchMessageText.gameObject.SetActive(true);
			this.searchMessageText.text = this.errorMessage;
			this.leftPageButton.SetActive(false);
			this.rightPageButton.SetActive(false);
			return;
		}
		if (this.filteredSearchedMods.IsNullOrEmpty<Mod>())
		{
			this.searchMessageText.gameObject.SetActive(true);
			this.searchMessageText.text = this.noMapsFoundString;
			this.leftPageButton.SetActive(false);
			this.rightPageButton.SetActive(false);
			return;
		}
		int num = 0;
		int num2 = this.modsPerPage - 1;
		if (!this.IsOnFirstPage())
		{
			num = this.currentModPage * this.modsPerPage;
			num2 = num + this.modsPerPage - 1;
			this.leftPageButton.gameObject.SetActive(true);
		}
		else
		{
			this.leftPageButton.gameObject.SetActive(false);
		}
		if (!this.IsOnLastPage())
		{
			this.rightPageButton.gameObject.SetActive(true);
		}
		else
		{
			this.rightPageButton.gameObject.SetActive(false);
		}
		if (this.filteredSearchedMods.Count <= num2 && this.totalSearchMods > this.searchedMods.Count)
		{
			this.RetrieveMods();
			return;
		}
		int num3 = num;
		while (num3 <= num2 && this.filteredSearchedMods.Count > num3)
		{
			this.displayedMods.Add(this.filteredSearchedMods[num3]);
			num3++;
		}
		this.customMapsGalleryView.gameObject.SetActive(true);
		string text;
		if (!this.customMapsGalleryView.DisplayGallery(this.displayedMods, true, out text))
		{
			this.searchMessageText.gameObject.SetActive(true);
			this.searchMessageText.text = text;
			this.customMapsGalleryView.gameObject.SetActive(false);
			this.leftPageButton.SetActive(false);
			this.rightPageButton.SetActive(false);
		}
	}

	private int GetNumPages()
	{
		int num = this.totalSearchMods % this.modsPerPage;
		int num2 = this.totalSearchMods / this.modsPerPage;
		if (num > 0)
		{
			num2++;
		}
		return num2;
	}

	private bool IsOnFirstPage()
	{
		return this.currentModPage == 0;
	}

	private bool IsOnLastPage()
	{
		long num = (long)this.GetNumPages();
		return (long)(this.currentModPage + 1) == num;
	}

	[SerializeField]
	private TMP_Text searchPhraseText;

	[SerializeField]
	private TMP_Text searchMessageText;

	[SerializeField]
	private CustomMapsGalleryView customMapsGalleryView;

	[SerializeField]
	private GameObject leftPageButton;

	[SerializeField]
	private GameObject rightPageButton;

	[SerializeField]
	private string defaultSearchString = "SEARCH PHRASE";

	[SerializeField]
	private string noMapsFoundString = "NO RESULTS FOUND";

	[SerializeField]
	private string searchingString = "SEARCHING";

	[SerializeField]
	private int numModsPerRequest = 60;

	[SerializeField]
	private int modsPerPage = 6;

	private string searchPhrase = "";

	private List<Mod> searchedMods = new List<Mod>();

	private List<Mod> filteredSearchedMods = new List<Mod>();

	private List<Mod> displayedMods = new List<Mod>();

	private int currentSearchModsRequestPage;

	private bool loadingSearchMods;

	private bool errorLoadingSearchMods;

	private int totalSearchMods;

	private int currentModPage;

	private string errorMessage = "";
}
