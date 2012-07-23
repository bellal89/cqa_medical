using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Iveonik.Stemmers;

namespace cqa_medical.BodyAnalisys
{
	class BodyPart
	{
		public readonly string[] Names;
		private List<BodyPart> subParts = new List<BodyPart>();

		private int questionsCount = 0;
		private long lastId = 0;

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

		public void SetLastId(long id)
		{
			lastId = id;
		}

		public bool LastIdEquals(long id)
		{
			return lastId == id;
		}

		public void Inc(long id)
		{
			if (LastIdEquals(id)) return;
			SetLastId(id);
			questionsCount++;
			if (Parent != null) Parent.Inc(id);
		}

		public int GetQuestionsCount()
		{
			return questionsCount;
		}

		public static BodyPart GetBodyPartsFromFile(String filename)
		{
			var tabParser = new TabulationParser(new RussianStemmer());
			var data = tabParser.ParseFromFile(filename).ToList();
			var first = data.First();
			if (first.IndicatorAmount != 0)
				throw new Exception("Wrong incapsulation in " + filename);

			var human = new BodyPart(null, first.StemmedWords.ToArray());
			var current = human;
			int currentTabs = first.IndicatorAmount;
			foreach (var q in data.Skip(1))
			{
				if (q.IndicatorAmount > currentTabs)
				{
					current = current.AddSubPartAndReturnItUsing( q.StemmedWords);
					currentTabs = q.IndicatorAmount;
				}
				else if (q.IndicatorAmount < currentTabs)
				{
					int numberIterations = currentTabs - q.IndicatorAmount;
					for (int i = 0; i < numberIterations; ++i )
						current = current.Parent;
					current = current.Parent.AddSubPartAndReturnItUsing( q.StemmedWords);
					currentTabs = q.IndicatorAmount;
				}
				else 
				{
					current = current.Parent.AddSubPartAndReturnItUsing(q.StemmedWords);
				}

			}
			return human;
		}

		public Dictionary<string, BodyPart> ToDictionary()
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

		public override string ToString()
		{
			return String.Join(" ", Names);
		}

		private string GetPartString(int allQuestionsCount)
		{
			return String.Join(" ", Names) + "\t" + questionsCount + "\t" + Math.Round((decimal)questionsCount * 100 / allQuestionsCount, 2) + "%";
		}

		public string ToString(int allQuestionsCount)
		{
			return ToInternalString("", allQuestionsCount);
		}

		private string ToInternalString(string tabs, int allQuestionsCount)
		{
			var result = tabs + GetPartString(allQuestionsCount) + "\n";
			if (subParts.Any())
			{
				return subParts.Aggregate(result, (current, part) => current + part.ToInternalString(tabs + "\t", allQuestionsCount));
			}
			return result;
		}

		public string ToExcelString(int allQuestionsCount)
		{
			var result = GetPartString(allQuestionsCount) + "\n";
			return result + String.Join("", subParts.Select(part => part.ToExcelString(allQuestionsCount)));
		}
	}
}
