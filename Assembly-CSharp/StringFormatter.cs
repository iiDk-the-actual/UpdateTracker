using System;
using System.Collections.Generic;
using System.Text;

public class StringFormatter
{
	public StringFormatter(string[] spans, int[] indices)
	{
		this.spans = spans;
		this.indices = indices;
	}

	public string Format(string term1)
	{
		StringFormatter.builder.Clear();
		StringFormatter.builder.Append(this.spans[0]);
		for (int i = 1; i < this.spans.Length; i++)
		{
			StringFormatter.builder.Append(term1);
			StringFormatter.builder.Append(this.spans[i]);
		}
		return StringFormatter.builder.ToString();
	}

	public string Format(Func<string> term1)
	{
		StringFormatter.builder.Clear();
		StringFormatter.builder.Append(this.spans[0]);
		for (int i = 1; i < this.spans.Length; i++)
		{
			StringFormatter.builder.Append(term1());
			StringFormatter.builder.Append(this.spans[i]);
		}
		return StringFormatter.builder.ToString();
	}

	public string Format(string term1, string term2)
	{
		StringFormatter.builder.Clear();
		StringFormatter.builder.Append(this.spans[0]);
		for (int i = 1; i < this.spans.Length; i++)
		{
			StringFormatter.builder.Append((this.indices[i - 1] == 0) ? term1 : term2);
			StringFormatter.builder.Append(this.spans[i]);
		}
		return StringFormatter.builder.ToString();
	}

	public string Format(string term1, string term2, string term3)
	{
		StringFormatter.builder.Clear();
		StringFormatter.builder.Append(this.spans[0]);
		for (int i = 1; i < this.spans.Length; i++)
		{
			int num = this.indices[i - 1];
			if (num != 0)
			{
				if (num != 1)
				{
					StringFormatter.builder.Append(term3);
				}
				else
				{
					StringFormatter.builder.Append(term2);
				}
			}
			else
			{
				StringFormatter.builder.Append(term1);
			}
			StringFormatter.builder.Append(this.spans[i]);
		}
		return StringFormatter.builder.ToString();
	}

	public string Format(Func<string> term1, Func<string> term2)
	{
		StringFormatter.builder.Clear();
		StringFormatter.builder.Append(this.spans[0]);
		for (int i = 1; i < this.spans.Length; i++)
		{
			if (this.indices[i - 1] == 0)
			{
				StringFormatter.builder.Append(term1());
			}
			else
			{
				StringFormatter.builder.Append(term2());
			}
			StringFormatter.builder.Append(this.spans[i]);
		}
		return StringFormatter.builder.ToString();
	}

	public string Format(Func<string> term1, Func<string> term2, Func<string> term3)
	{
		StringFormatter.builder.Clear();
		StringFormatter.builder.Append(this.spans[0]);
		for (int i = 1; i < this.spans.Length; i++)
		{
			int num = this.indices[i - 1];
			if (num != 0)
			{
				if (num != 1)
				{
					StringFormatter.builder.Append(term3());
				}
				else
				{
					StringFormatter.builder.Append(term2());
				}
			}
			else
			{
				StringFormatter.builder.Append(term1());
			}
			StringFormatter.builder.Append(this.spans[i]);
		}
		return StringFormatter.builder.ToString();
	}

	public string Format(Func<string> term1, Func<string> term2, Func<string> term3, Func<string> term4)
	{
		StringFormatter.builder.Clear();
		StringFormatter.builder.Append(this.spans[0]);
		for (int i = 1; i < this.spans.Length; i++)
		{
			switch (this.indices[i - 1])
			{
			case 0:
				StringFormatter.builder.Append(term1());
				break;
			case 1:
				StringFormatter.builder.Append(term2());
				break;
			case 2:
				StringFormatter.builder.Append(term3());
				break;
			default:
				StringFormatter.builder.Append(term4());
				break;
			}
			StringFormatter.builder.Append(this.spans[i]);
		}
		return StringFormatter.builder.ToString();
	}

	public string Format(params string[] terms)
	{
		StringFormatter.builder.Clear();
		StringFormatter.builder.Append(this.spans[0]);
		for (int i = 1; i < this.spans.Length; i++)
		{
			StringFormatter.builder.Append(terms[this.indices[i - 1]]);
			StringFormatter.builder.Append(this.spans[i]);
		}
		return StringFormatter.builder.ToString();
	}

	public string Format(params Func<string>[] terms)
	{
		StringFormatter.builder.Clear();
		StringFormatter.builder.Append(this.spans[0]);
		for (int i = 1; i < this.spans.Length; i++)
		{
			StringFormatter.builder.Append(terms[this.indices[i - 1]]());
			StringFormatter.builder.Append(this.spans[i]);
		}
		return StringFormatter.builder.ToString();
	}

	public static StringFormatter Parse(string input)
	{
		int num = 0;
		List<string> list = new List<string>();
		List<int> list2 = new List<int>();
		for (;;)
		{
			int num2 = input.IndexOf('%', num);
			if (num2 == -1)
			{
				break;
			}
			list.Add(input.Substring(num, num2 - num));
			list2.Add((int)(input[num2 + 1] - '0'));
			num = num2 + 2;
		}
		list.Add(input.Substring(num));
		return new StringFormatter(list.ToArray(), list2.ToArray());
	}

	private static StringBuilder builder = new StringBuilder();

	private string[] spans;

	private int[] indices;
}
