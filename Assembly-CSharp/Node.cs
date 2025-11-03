using System;
using System.Collections.Generic;

public class Node<T>
{
	public T Value { get; set; }

	public Node<T> Parent { get; private set; }

	public List<Node<T>> Children { get; } = new List<Node<T>>();

	public Node(T value)
	{
		this.Value = value;
	}

	public Node<T> AddChild(T value)
	{
		Node<T> node = new Node<T>(value)
		{
			Parent = this
		};
		this.Children.Add(node);
		return node;
	}

	public Node<T> AddChild(Node<T> child)
	{
		Node<T> parent = child.Parent;
		if (parent != null)
		{
			parent.RemoveChild(child);
		}
		this.Children.Add(child);
		child.Parent = this;
		return child;
	}

	public void RemoveChild(Node<T> child)
	{
		if (this.Children.Remove(child))
		{
			child.Parent = null;
		}
	}

	public IEnumerable<Node<T>> TraversePreOrder()
	{
		yield return this;
		foreach (Node<T> node in this.Children)
		{
			foreach (Node<T> node2 in node.TraversePreOrder())
			{
				yield return node2;
			}
			IEnumerator<Node<T>> enumerator2 = null;
		}
		List<Node<T>>.Enumerator enumerator = default(List<Node<T>>.Enumerator);
		yield break;
		yield break;
	}

	public IEnumerable<Node<T>> TraverseBreadthFirst()
	{
		Queue<Node<T>> queue = new Queue<Node<T>>();
		queue.Enqueue(this);
		while (queue.Count > 0)
		{
			Node<T> current = queue.Dequeue();
			yield return current;
			foreach (Node<T> node in current.Children)
			{
				queue.Enqueue(node);
			}
			current = null;
		}
		yield break;
	}

	public List<Node<T>> GetPath()
	{
		List<Node<T>> list = new List<Node<T>> { this };
		for (Node<T> node = this.Parent; node != null; node = node.Parent)
		{
			list.Insert(0, node);
		}
		return list;
	}
}
