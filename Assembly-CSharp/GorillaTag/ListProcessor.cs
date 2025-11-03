using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaTag
{
	public class ListProcessor<T>
	{
		public int Count
		{
			get
			{
				return this.m_list.Count;
			}
		}

		public InAction<T> ItemProcessor
		{
			get
			{
				return this.m_itemProcessorDelegate;
			}
			set
			{
				this.m_itemProcessorDelegate = value;
			}
		}

		public ListProcessor()
			: this(10, null)
		{
		}

		public ListProcessor(int capacity, InAction<T> itemProcessorDelegate = null)
		{
			this.m_list = new List<T>(capacity);
			this.m_currentIndex = -1;
			this.m_listCount = -1;
			this.m_itemProcessorDelegate = itemProcessorDelegate;
		}

		public void Add(in T item)
		{
			this.m_listCount++;
			this.m_list.Add(item);
		}

		public void Remove(in T item)
		{
			int num = this.m_list.IndexOf(item);
			if (num < 0)
			{
				return;
			}
			if (num < this.m_currentIndex)
			{
				this.m_currentIndex--;
			}
			this.m_listCount--;
			this.m_list.RemoveAt(num);
		}

		public void Clear()
		{
			this.m_list.Clear();
			this.m_currentIndex = -1;
			this.m_listCount = -1;
		}

		public bool Contains(in T item)
		{
			return this.m_list.Contains(item);
		}

		public virtual void ProcessListSafe()
		{
			if (this.m_itemProcessorDelegate == null)
			{
				Debug.LogError("ListProcessor: ItemProcessor is null");
				return;
			}
			this.m_listCount = this.m_list.Count;
			this.m_currentIndex = 0;
			while (this.m_currentIndex < this.m_listCount)
			{
				try
				{
					InAction<T> itemProcessorDelegate = this.m_itemProcessorDelegate;
					T t = this.m_list[this.m_currentIndex];
					itemProcessorDelegate(in t);
				}
				catch (Exception ex)
				{
					Debug.LogError(ex.ToString());
				}
				this.m_currentIndex++;
			}
		}

		public virtual void ProcessList()
		{
			if (this.m_itemProcessorDelegate == null)
			{
				Debug.LogError("ListProcessor: ItemProcessor is null");
				return;
			}
			this.m_listCount = this.m_list.Count;
			this.m_currentIndex = 0;
			while (this.m_currentIndex < this.m_listCount)
			{
				InAction<T> itemProcessorDelegate = this.m_itemProcessorDelegate;
				T t = this.m_list[this.m_currentIndex];
				itemProcessorDelegate(in t);
				this.m_currentIndex++;
			}
		}

		protected readonly List<T> m_list;

		protected int m_currentIndex;

		protected int m_listCount;

		protected InAction<T> m_itemProcessorDelegate;
	}
}
