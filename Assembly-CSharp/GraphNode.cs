using System;
using System.Collections.Generic;

public class GraphNode<T>
{
	public T Value { get; set; }

	public List<GraphNode<T>> Parents { get; } = new List<GraphNode<T>>();

	public List<GraphNode<T>> Children { get; } = new List<GraphNode<T>>();

	public int ChildCount
	{
		get
		{
			return this.Children.Count;
		}
	}

	public GraphNode(T value)
	{
		this.Value = value;
	}

	public GraphNode(T value, GraphNode<T> parent)
	{
		this.Value = value;
		this.Parents.Add(parent);
	}

	public int GetSubtreeWidth(int depthLimit = 2147483647)
	{
		if (this.ChildCount == 0 || depthLimit == 0)
		{
			return 1;
		}
		int num = 0;
		foreach (GraphNode<T> graphNode in this.Children)
		{
			num += graphNode.GetSubtreeWidth(depthLimit - 1);
		}
		return num;
	}

	public GraphNode<T> AddChild(T value)
	{
		return this.AddChild(new GraphNode<T>(value));
	}

	public GraphNode<T> AddChild(GraphNode<T> child)
	{
		if (child.Parents.Contains(this))
		{
			throw new InvalidOperationException("Cannot add child more than once");
		}
		this.Children.Add(child);
		child.Parents.Add(this);
		return child;
	}

	public void RemoveChild(GraphNode<T> child)
	{
		if (this.Children.Remove(child))
		{
			child.Parents.Remove(this);
		}
	}

	public IEnumerable<GraphNode<T>> TraversePreOrder()
	{
		yield return this;
		foreach (GraphNode<T> graphNode in this.Children)
		{
			foreach (GraphNode<T> graphNode2 in graphNode.TraversePreOrder())
			{
				yield return graphNode2;
			}
			IEnumerator<GraphNode<T>> enumerator2 = null;
		}
		List<GraphNode<T>>.Enumerator enumerator = default(List<GraphNode<T>>.Enumerator);
		yield break;
		yield break;
	}

	public IEnumerable<GraphNode<T>> TraversePreOrderDistinct(HashSet<GraphNode<T>> visited = null)
	{
		if (visited == null)
		{
			visited = new HashSet<GraphNode<T>>();
		}
		if (!visited.Contains(this))
		{
			yield return this;
			visited.Add(this);
			foreach (GraphNode<T> graphNode in this.Children)
			{
				foreach (GraphNode<T> graphNode2 in graphNode.TraversePreOrderDistinct(visited))
				{
					yield return graphNode2;
				}
				IEnumerator<GraphNode<T>> enumerator2 = null;
			}
			List<GraphNode<T>>.Enumerator enumerator = default(List<GraphNode<T>>.Enumerator);
		}
		yield break;
		yield break;
	}

	public IEnumerable<GraphNode<T>> TraverseBreadthFirst()
	{
		Queue<GraphNode<T>> queue = new Queue<GraphNode<T>>();
		queue.Enqueue(this);
		while (queue.Count > 0)
		{
			GraphNode<T> current = queue.Dequeue();
			yield return current;
			foreach (GraphNode<T> graphNode in current.Children)
			{
				queue.Enqueue(graphNode);
			}
			current = null;
		}
		yield break;
	}

	public IEnumerable<GraphNode<T>> TraverseBreadthFirstDistinct()
	{
		Queue<GraphNode<T>> queue = new Queue<GraphNode<T>>();
		HashSet<GraphNode<T>> visited = new HashSet<GraphNode<T>>();
		queue.Enqueue(this);
		while (queue.Count > 0)
		{
			GraphNode<T> current = queue.Dequeue();
			if (!visited.Contains(current))
			{
				visited.Add(current);
				yield return current;
				foreach (GraphNode<T> graphNode in current.Children)
				{
					queue.Enqueue(graphNode);
				}
				current = null;
			}
		}
		yield break;
	}

	public int GetGraphDepth()
	{
		if (this.Children.Count == 0)
		{
			return 1;
		}
		int num = 0;
		foreach (GraphNode<T> graphNode in this.Children)
		{
			num = Math.Max(num, graphNode.GetGraphDepth());
		}
		return num + 1;
	}

	public int GetNodeDepth()
	{
		if (this.Parents.Count == 0)
		{
			return 1;
		}
		int num = 0;
		foreach (GraphNode<T> graphNode in this.Parents)
		{
			num = Math.Max(num, graphNode.GetNodeDepth());
		}
		return num + 1;
	}
}
