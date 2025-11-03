using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using LitJson;
using PlayFab;
using UnityEngine;

namespace GorillaNetworking
{
	public class CreditsView : MonoBehaviour
	{
		private int TotalPages
		{
			get
			{
				return this.creditsSections.Sum((CreditsSection section) => this.PagesPerSection(section));
			}
		}

		private void Start()
		{
			this.creditsSections = new CreditsSection[]
			{
				new CreditsSection
				{
					Title = "DEV TEAM",
					Entries = new List<string>
					{
						"Anton \"NtsFranz\" Franzluebbers", "Carlo Grossi Jr", "Cody O'Quinn", "David Neubelt", "David \"AA_DavidY\" Yee", "Derek \"DunkTrain\" Arabian", "Elie Arabian", "John Sleeper", "Haunted Army", "Kerestell Smith",
						"Keith \"ElectronicWall\" Taylor", "Laura \"Poppy\" Lorian", "Lilly Tothill", "Matt \"Crimity\" Ostgard", "Nick Taylor", "Ross Furmidge", "Sasha \"Kayze\" Sanders"
					}
				},
				new CreditsSection
				{
					Title = "SPECIAL THANKS",
					Entries = new List<string> { "The \"Sticks\"", "Alpha Squad", "Meta", "Scout House", "Mighty PR", "Caroline Arabian", "Clarissa & Declan", "Calum Haigh", "EZ ICE", "Gwen" }
				},
				new CreditsSection
				{
					Title = "MUSIC BY",
					Entries = new List<string> { "Stunshine", "David Anderson Kirk", "Jaguar Jen", "Audiopfeil", "Owlobe" }
				}
			};
			PlayFabTitleDataCache.Instance.GetTitleData("CreditsData", delegate(string result)
			{
				this.creditsSections = JsonMapper.ToObject<CreditsSection[]>(result);
			}, delegate(PlayFabError error)
			{
				Debug.Log("Error fetching credits data: " + error.ErrorMessage);
			}, false);
		}

		private int PagesPerSection(CreditsSection section)
		{
			return (int)Math.Ceiling((double)section.Entries.Count / (double)this.pageSize);
		}

		private IEnumerable<string> PageOfSection(CreditsSection section, int page)
		{
			return section.Entries.Skip(this.pageSize * page).Take(this.pageSize);
		}

		[return: TupleElementNames(new string[] { "creditsSection", "subPage" })]
		private ValueTuple<CreditsSection, int> GetPageEntries(int page)
		{
			int num = 0;
			foreach (CreditsSection creditsSection in this.creditsSections)
			{
				int num2 = this.PagesPerSection(creditsSection);
				if (num + num2 > page)
				{
					int num3 = page - num;
					return new ValueTuple<CreditsSection, int>(creditsSection, num3);
				}
				num += num2;
			}
			return new ValueTuple<CreditsSection, int>(this.creditsSections.First<CreditsSection>(), 0);
		}

		public void ProcessButtonPress(GorillaKeyboardBindings buttonPressed)
		{
			if (buttonPressed == GorillaKeyboardBindings.enter)
			{
				this.currentPage++;
				this.currentPage %= this.TotalPages;
			}
		}

		public string GetScreenText()
		{
			return this.GetPage(this.currentPage);
		}

		private string GetPage(int page)
		{
			ValueTuple<CreditsSection, int> pageEntries = this.GetPageEntries(page);
			CreditsSection item = pageEntries.Item1;
			int item2 = pageEntries.Item2;
			IEnumerable<string> enumerable = this.PageOfSection(item, item2);
			string text = "CREDITS";
			string text2;
			LocalisationManager.TryGetKeyForCurrentLocale("CREDITS", out text2, text);
			text = "(CONT)";
			string text3;
			LocalisationManager.TryGetKeyForCurrentLocale("CREDITS_CONTINUED", out text3, text);
			string text4 = text2 + " - " + ((item2 == 0) ? item.Title : (item.Title + " " + text3));
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(text4);
			stringBuilder.AppendLine();
			foreach (string text5 in enumerable)
			{
				stringBuilder.AppendLine(text5);
			}
			for (int i = 0; i < this.pageSize - enumerable.Count<string>(); i++)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendLine();
			text = "PRESS ENTER TO CHANGE PAGES";
			string text6;
			LocalisationManager.TryGetKeyForCurrentLocale("CREDITS_PRESS_ENTER", out text6, text);
			stringBuilder.AppendLine(text6);
			return stringBuilder.ToString();
		}

		private const string CREDITS_KEY = "CREDITS";

		private const string CREDITS_PRESS_ENTER_KEY = "CREDITS_PRESS_ENTER";

		private const string CREDITS_CONTINUED_KEY = "CREDITS_CONTINUED";

		private CreditsSection[] creditsSections;

		public int pageSize = 7;

		private int currentPage;

		private const string PlayFabKey = "CreditsData";
	}
}
