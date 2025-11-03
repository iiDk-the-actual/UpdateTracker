using System;
using GorillaExtensions;
using GorillaNetworking;
using TMPro;
using UnityEngine;

namespace CosmeticRoom
{
	public class CurrencyBoard : MonoBehaviour
	{
		public void OnEnable()
		{
			CosmeticsController.instance.AddCurrencyBoard(this);
		}

		public void OnDisable()
		{
			CosmeticsController.instance.RemoveCurrencyBoard(this);
		}

		public void UpdateCurrencyBoard(bool checkedDaily, bool gotDaily, int currencyBalance, int secTilTomorrow)
		{
			if (this.dailyRocksTextTMP.IsNotNull())
			{
				this.dailyRocksTextTMP.text = (checkedDaily ? (gotDaily ? "SUCCESSFULLY GOT DAILY ROCKS!" : "WAITING TO GET DAILY ROCKS...") : "CHECKING DAILY ROCKS...");
			}
			if (this.currencyBoardTextTMP.IsNotNull())
			{
				this.currencyBoardTextTMP.text = string.Concat(new string[]
				{
					currencyBalance.ToString(),
					"\n\n",
					(secTilTomorrow / 3600).ToString(),
					" HR, ",
					(secTilTomorrow % 3600 / 60).ToString(),
					"MIN"
				});
			}
		}

		public TMP_Text dailyRocksTextTMP;

		public TMP_Text currencyBoardTextTMP;
	}
}
