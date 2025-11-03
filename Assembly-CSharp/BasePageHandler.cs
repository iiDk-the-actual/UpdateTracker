using System;
using UnityEngine;

public abstract class BasePageHandler : MonoBehaviour
{
	private protected int selectedIndex { protected get; private set; }

	private protected int currentPage { protected get; private set; }

	private protected int pages { protected get; private set; }

	private protected int maxEntires { protected get; private set; }

	protected abstract int pageSize { get; }

	protected abstract int entriesCount { get; }

	protected virtual void Start()
	{
		Debug.Log("base page handler " + this.entriesCount.ToString() + " " + this.pageSize.ToString());
		this.pages = this.entriesCount / this.pageSize + 1;
		this.maxEntires = this.pages * this.pageSize;
	}

	public void SelectEntryOnPage(int entryIndex)
	{
		int num = entryIndex + this.pageSize * this.currentPage;
		if (num > this.entriesCount)
		{
			return;
		}
		this.selectedIndex = num;
		this.PageEntrySelected(entryIndex, this.selectedIndex);
	}

	public void SelectEntryFromIndex(int index)
	{
		this.selectedIndex = index;
		this.currentPage = this.selectedIndex / this.pageSize;
		int num = index - this.pageSize * this.currentPage;
		this.PageEntrySelected(num, index);
		this.SetPage(this.currentPage);
	}

	public void ChangePage(bool left)
	{
		int num = (left ? (-1) : 1);
		this.SetPage(Mathf.Abs((this.currentPage + num) % this.pages));
	}

	public void SetPage(int page)
	{
		if (page > this.pages)
		{
			return;
		}
		this.currentPage = page;
		int num = this.pageSize * page;
		this.ShowPage(this.currentPage, num, Mathf.Min(num + this.pageSize, this.entriesCount));
	}

	protected abstract void ShowPage(int selectedPage, int startIndex, int endIndex);

	protected abstract void PageEntrySelected(int pageEntry, int selectionIndex);
}
