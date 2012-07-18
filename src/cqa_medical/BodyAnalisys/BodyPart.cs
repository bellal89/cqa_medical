using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cqa_medical.BodyAnalisys
{
	class BodyPart
	{
		public readonly string[] Names;
		private List<BodyPart> subParts = new List<BodyPart>();

		private int questionsCount = 0;

		public readonly BodyPart Parent;

		public BodyPart(BodyPart parent, string[] names)
		{
			Parent = parent;
			Names = names;
		}

		public void SetSubParts(List<BodyPart> parts)
		{
			subParts = parts;
		}

		public List<BodyPart> GetSubParts()
		{
			return subParts;
		}

		public void AddSubPart(BodyPart part)
		{
			subParts.Add(part);
		}

		public BodyPart AddSubPartAndReturnItUsing(IEnumerable<string> names)
		{
			var q = new BodyPart(this, names.ToArray());
			this.AddSubPart(q);
			return q;
		}

		private string GetTabulation(BodyPart from, BodyPart q)
		{
			string s = "";
			BodyPart temp = q;
			while (temp != from && temp.Parent != null)
			{
				temp = temp.Parent;
				s += "\t";
			}
			return s;
		}

		public string ToString(BodyPart q, int allQuestionsCount)
		{
			var result = String.Join(" ", Names) + "\t" + questionsCount +  "\t" + Math.Round((decimal)questionsCount * 100 / allQuestionsCount, 2) + "%\n";
			return result + String.Join("",subParts.Select(part => GetTabulation(part, q) + part.ToString(part, allQuestionsCount)));
		}

		private string ToInternalString(string result)
		{
			result += String.Join(" ", Names) + "\t" + questionsCount + "\n";
			foreach (var part in subParts)
			{
				return "\t" + part.ToInternalString(result);
			}
			return result;
		}

		public override string ToString()
		{
			return String.Join(" ", Names);
		}
		public  string ToExelString()
		{
			var result = String.Join(" ", Names) + "\t" + questionsCount + "\n";
			return result + String.Join("", subParts.Select(part => part.ToExelString()));
		}

		public void Inc()
		{
			questionsCount++;
			if (Parent != null) Parent.Inc();
		}

		public int GetQuestionsCount()
		{
			return questionsCount;
		}

		public static BodyPart GetBodyPartsFromFile(String filename)
		{
			var data = TabulationParser.ParseFromFile(filename);
			var first = data.First();
			if (first.IndicatorAmount != 0)
				throw new Exception("Wrong incapsulation in " + filename);

			var human = new BodyPart(null, first.StemmedStrings.ToArray());
			var current = human;
			int currentTabs = first.IndicatorAmount;
			foreach (var q in data.Skip(1))
			{
				if (q.IndicatorAmount > currentTabs)
				{
					current = current.AddSubPartAndReturnItUsing( q.StemmedStrings);
					currentTabs = q.IndicatorAmount;
				}
				else if (q.IndicatorAmount < currentTabs)
				{
					int numberIterations = currentTabs - q.IndicatorAmount;
					for (int i = 0; i < numberIterations; ++i )
						current = current.Parent;
					current = current.Parent.AddSubPartAndReturnItUsing( q.StemmedStrings);
					currentTabs = q.IndicatorAmount;
				}
				else 
				{
					current = current.Parent.AddSubPartAndReturnItUsing(q.StemmedStrings);
				}

			}
			return human;
		}

		public Dictionary<string, BodyPart> GetDictionary()
		{
			return getDict(new Dictionary<string, BodyPart>(), this);
		}

		private Dictionary<string, BodyPart> getDict(Dictionary<string, BodyPart> dict, BodyPart part)
		{
			foreach (var name in part.Names)
			{
				dict.Add(name, part);
			}
			if (part.GetSubParts().Count == 0)
			{
				return dict;
			}
			foreach (var bodyPart in part.GetSubParts())
			{
				getDict(dict, bodyPart);
			}
			return dict;
		}
	}
}
