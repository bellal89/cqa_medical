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

		public void Inc()
		{
			questionsCount++;
			Parent.Inc();
		}

		public int GetQuestionsCount()
		{
			return questionsCount;
		}

	}
}
