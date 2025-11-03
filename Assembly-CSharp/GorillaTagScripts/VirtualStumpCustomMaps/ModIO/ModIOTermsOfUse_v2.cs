using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Modio;
using Modio.Customizations;
using UnityEngine;

namespace GorillaTagScripts.VirtualStumpCustomMaps.ModIO
{
	public class ModIOTermsOfUse_v2 : LegalAgreements
	{
		protected override void Awake()
		{
			if (ModIOTermsOfUse_v2.modioTermsInstance != null)
			{
				Debug.LogError("Trying to set [LegalAgreements] instance but it is not null", this);
				base.gameObject.SetActive(false);
				return;
			}
			ModIOTermsOfUse_v2.modioTermsInstance = this;
			this.stickHeldDuration = 0f;
			this.scrollSpeed = this._minScrollSpeed;
			base.enabled = false;
		}

		public async Task<Error> ShowTerms()
		{
			Error error = Error.None;
			ValueTuple<Error, TermsOfUse> valueTuple = await TermsOfUse.Get();
			error = valueTuple.Item1;
			this.termsOfUse = valueTuple.Item2;
			Error error2;
			if (error)
			{
				GTDev.LogError<string>(string.Format("[ModIOTermsOfUse::ShowTerms] Failed to get TermsOfUse from Mod.io: {0}", error), null);
				error2 = error;
			}
			else
			{
				ValueTuple<Error, Agreement> valueTuple2 = await Agreement.GetAgreement(AgreementType.TermsOfUse, false);
				error = valueTuple2.Item1;
				this.fullTermsOfUse = valueTuple2.Item2;
				if (error)
				{
					GTDev.LogError<string>(string.Format("[ModIOTermsOfUse::ShowTerms] Failed to get full Terms of Use Agreement from Mod.io: {0}", error), null);
					error2 = error;
				}
				else
				{
					valueTuple2 = await Agreement.GetAgreement(AgreementType.PrivacyPolicy, false);
					error = valueTuple2.Item1;
					this.fullPrivacyPolicy = valueTuple2.Item2;
					if (error)
					{
						GTDev.LogError<string>(string.Format("[ModIOTermsOfUse::ShowTerms] Failed to get full Privacy Policy Agreement from Mod.io: {0}", error), null);
						error2 = error;
					}
					else
					{
						base.enabled = true;
						await this.StartLegalAgreements();
						error2 = Error.None;
					}
				}
			}
			return error2;
		}

		public override async Task StartLegalAgreements()
		{
			if (!this.legalAgreementsStarted)
			{
				this.legalAgreementsStarted = true;
				PrivateUIRoom.ForceStartOverlay();
				PrivateUIRoom.AddUI(this.uiParent);
				this._pressAndHoldToConfirmButton.SetText(this.confirmString);
				this._pressAndHoldToConfirmButton.gameObject.SetActive(false);
				HandRayController.Instance.EnableHandRays();
				this.UpdateTextFromTerms();
				await base.WaitForAcknowledgement();
				this.scrollBar.value = 1f;
				PrivateUIRoom.StopForcedOverlay();
				PrivateUIRoom.RemoveUI(this.uiParent);
				Object.Destroy(base.gameObject);
			}
		}

		private void UpdateTextFromTerms()
		{
			this.tmpTitle.text = "Mod.io Terms of Use";
			this.tmpBody.text = "Loading...";
			this.cachedTermsText = this.termsOfUse.TermsText + "\n\n";
			this.cachedTermsText = this.cachedTermsText + this.FormatAgreementText(this.fullTermsOfUse) + "\n\n\n";
			this.cachedTermsText += this.FormatAgreementText(this.fullPrivacyPolicy);
			this.tmpBody.text = this.cachedTermsText;
		}

		private string FormatAgreementText(Agreement agreement)
		{
			string text = string.Concat(new string[]
			{
				agreement.Name,
				"\n\nEffective Date: ",
				agreement.DateLive.ToLongDateString(),
				"\n\n",
				agreement.Content
			});
			text = Regex.Replace(text, "<!--[^>]*(-->)", "");
			text = text.Replace("<h1>", "<b>");
			text = text.Replace("</h1>", "</b>");
			text = text.Replace("<h2>", "<b>");
			text = text.Replace("</h2>", "</b>");
			text = text.Replace("<h3>", "<b>");
			text = text.Replace("</h3>", "</b>");
			text = text.Replace("<hr>", "");
			text = text.Replace("<br>", "\n");
			text = text.Replace("</li>", "</indent>\n");
			text = text.Replace("<strong>", "<b>");
			text = text.Replace("</strong>", "</b>");
			text = text.Replace("<em>", "<i>");
			text = text.Replace("</em>", "</i>");
			text = Regex.Replace(text, "<a[^>]*>{1}", "");
			text = text.Replace("</a>", "");
			Match match = Regex.Match(text, "<p[^>]*align:center[^>]*>{1}");
			while (match.Success)
			{
				text = text.Remove(match.Index, match.Length);
				text = text.Insert(match.Index, "\n<align=\"center\">");
				int num = text.IndexOf("</p>", match.Index, StringComparison.Ordinal);
				text = text.Remove(num, 4);
				text = text.Insert(num, "</align>");
				match = Regex.Match(text, "<p[^>]*align:center[^>]*>{1}");
			}
			text = text.Replace("<p>", "\n");
			text = text.Replace("</p>", "");
			text = Regex.Replace(text, "<ol[^>]*>{1}", "<ol>");
			int num2 = text.IndexOf("<ol>", StringComparison.OrdinalIgnoreCase);
			bool flag = num2 != -1;
			while (flag)
			{
				int num3 = text.IndexOf("</ol>", num2, StringComparison.OrdinalIgnoreCase);
				text = text.Remove(num2, "<ol>".Length);
				int num4 = text.IndexOf("<li>", num2, StringComparison.OrdinalIgnoreCase);
				bool flag2 = num4 != -1;
				int num5 = 0;
				while (flag2)
				{
					text = text.Remove(num4, "<li>".Length);
					text = text.Insert(num4, this.GetStringForListItemIdx_LowerAlpha(num5++));
					num3 = text.IndexOf("</ol>", num2, StringComparison.OrdinalIgnoreCase);
					num4 = text.IndexOf("<li>", num2, StringComparison.OrdinalIgnoreCase);
					flag2 = num4 != -1 && num4 < num3;
				}
				text = text.Remove(num3, "</ol>".Length);
				num2 = text.IndexOf("<ol>", StringComparison.OrdinalIgnoreCase);
				flag = num2 != -1;
			}
			text = Regex.Replace(text, "<ul[^>]*>{1}", "<ul>");
			int num6 = text.IndexOf("<ul>", StringComparison.OrdinalIgnoreCase);
			bool flag3 = num6 != -1;
			while (flag3)
			{
				int num7 = text.IndexOf("</ul>", num6, StringComparison.OrdinalIgnoreCase);
				text = text.Remove(num6, "<ul>".Length);
				int num8 = text.IndexOf("<li>", num6, StringComparison.OrdinalIgnoreCase);
				bool flag4 = num8 != -1;
				while (flag4)
				{
					text = text.Remove(num8, "<li>".Length);
					text = text.Insert(num8, "  - <indent=5%>");
					num7 = text.IndexOf("</ul>", num6, StringComparison.OrdinalIgnoreCase);
					num8 = text.IndexOf("<li>", num6, StringComparison.OrdinalIgnoreCase);
					flag4 = num8 != -1 && num8 < num7;
				}
				text = text.Remove(num7, "</ul>".Length);
				num6 = text.IndexOf("<ul>", StringComparison.OrdinalIgnoreCase);
				flag3 = num6 != -1;
			}
			text = Regex.Replace(text, "<table[^>]*>{1}", "");
			text = text.Replace("<tbody>", "");
			text = text.Replace("<tr>", "");
			text = text.Replace("<td>", "");
			text = text.Replace("<center>", "");
			text = text.Replace("</table>", "");
			text = text.Replace("</tbody>", "");
			text = text.Replace("</tr>", "\n");
			text = text.Replace("</td>", "");
			return text.Replace("</center>", "");
		}

		private string GetStringForListItemIdx_LowerAlpha(int idx)
		{
			switch (idx)
			{
			case 0:
				return "  a. <indent=5%>";
			case 1:
				return "  b. <indent=5%>";
			case 2:
				return "  c. <indent=5%>";
			case 3:
				return "  d. <indent=5%>";
			case 4:
				return "  e. <indent=5%>";
			case 5:
				return "  f. <indent=5%>";
			case 6:
				return "  g. <indent=5%>";
			case 7:
				return "  h. <indent=5%>";
			case 8:
				return "  i. <indent=5%>";
			case 9:
				return "  j. <indent=5%>";
			case 10:
				return "  k. <indent=5%>";
			case 11:
				return "  l. <indent=5%>";
			case 12:
				return "  m. <indent=5%>";
			case 13:
				return "  n. <indent=5%>";
			case 14:
				return "  o. <indent=5%>";
			case 15:
				return "  p. <indent=5%>";
			case 16:
				return "  q. <indent=5%>";
			case 17:
				return "  r. <indent=5%>";
			case 18:
				return "  s. <indent=5%>";
			case 19:
				return "  t. <indent=5%>";
			case 20:
				return "  u. <indent=5%>";
			case 21:
				return "  v. <indent=5%>";
			case 22:
				return "  w. <indent=5%>";
			case 23:
				return "  x. <indent=5%>";
			case 24:
				return "  y. <indent=5%>";
			case 25:
				return "  z. <indent=5%>";
			default:
				return "";
			}
		}

		[SerializeField]
		private string confirmString = "Press and Hold to Confirm";

		private static ModIOTermsOfUse_v2 modioTermsInstance;

		private TermsOfUse termsOfUse;

		private Agreement fullTermsOfUse;

		private Agreement fullPrivacyPolicy;

		private string cachedTermsText;
	}
}
