using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.UtilitsNamespace;
using TopicIdAndConfidenceInItTuple = System.Tuple<int, double>;
using TopicIdAndConfidenceInItDictionary = System.Collections.Generic.Dictionary<int, double>;
using UserTopicConfidenceDictionary = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<int, double>>;


namespace cqa_medical.LDA
{
	internal class Topic
	{
		public static Dictionary<long, double[]> GetConfidenceDistribution(string docIdsFile, string topicsFile)
		{
			var result = new Dictionary<long, double[]>();
			var ids = File.ReadAllLines(docIdsFile).Select(long.Parse).ToArray();
			int i = 0;
			using (var stream = new StreamReader(topicsFile))
			{
				string line;
				while(null != (line = stream.ReadLine()) )
				{
					var topicConfidences = 
						line
							.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)
							.Select(d => Double.Parse(d, CultureInfo.InvariantCulture)).ToArray();
					result.Add(ids[i], topicConfidences);
					i++;
				}
			}
			return result;
		}

		/// <param name="confidences"></param>
		/// <returns>message.ID -> (Topic.ID, Topic.Confidence)</returns>
		public static Dictionary<long, TopicIdAndConfidenceInItTuple[]> CleanConfidences(Dictionary<long, double[]> confidences)
		{
			var q = confidences.ToDictionary(k => k.Key, k => k.Value
				.Select((a,i) => Tuple.Create(i,a))
				.OrderByDescending(d => d.Item2)
				.Take(3)
				.ToArray());
			return q;
		}
		public static void GetUsersTopicDistribution(QuestionList ql, Dictionary<long, TopicIdAndConfidenceInItTuple[]> dict, out Dictionary<string, TopicIdAndConfidenceInItDictionary> questioners, out Dictionary<string, TopicIdAndConfidenceInItDictionary> answerers)
		{

			questioners = new Dictionary<string, TopicIdAndConfidenceInItDictionary>();
			answerers = new Dictionary<string, TopicIdAndConfidenceInItDictionary>();
			foreach (var e in dict)
			{
				var question = ql.GetQuestion(e.Key);
				foreach (TopicIdAndConfidenceInItTuple q in e.Value)
				{
					// добавить пользователя, 
					if (!questioners.ContainsKey(question.AuthorEmail))
						questioners.Add(question.AuthorEmail, new []{q} .ToDictionary(w => w.Item1, w => w.Item2 ));
					else
						questioners[question.AuthorEmail].UpdateOrAdd(q.Item1, v => v + q.Item2, q.Item2);

					foreach (var answer in question.GetAnswers())
					{
						if (!answerers.ContainsKey(answer.AuthorEmail))
							answerers.Add(answer.AuthorEmail, new[] { q }.ToDictionary(w => w.Item1, w => w.Item2));
						else
							answerers[answer.AuthorEmail].UpdateOrAdd(q.Item1, v => v + q.Item2, q.Item2);
					}
				}
			}
		}

		public static UserTopicConfidenceDictionary FilterConfidence(UserTopicConfidenceDictionary dict)
		{

			var result = new UserTopicConfidenceDictionary();
			foreach (var pair in dict)
			{
				var confs = pair.Value.Where(d => d.Value > 0.5).ToArray();
				if (confs.Any())
				{
					result.Add(pair.Key, confs.ToDictionary());
				}

			}
			return result.OrderByDescending(k => k.Value.Values.Max()).ToDictionary();
		}

	}

	[TestFixture]
	internal class TopicsTest
	{
		[Test,Explicit]
		public void UserTopicActivity()
		{
			var confidenceDistribution = Topic.GetConfidenceDistribution(Program.GibbsDocIdsFileName, Program.ThetaFileName);
			var cleanedConfidences = Topic.CleanConfidences(confidenceDistribution);
			File.WriteAllLines("tr.txt", cleanedConfidences.Select( k => k.Key + "\t"  + String.Join("\t",k.Value.Select(t => t.Item1 + " " + t.Item2)) ));
			
			UserTopicConfidenceDictionary questioners;
			UserTopicConfidenceDictionary answerers;
			Topic.GetUsersTopicDistribution(Program.DefaultQuestionList, cleanedConfidences, out questioners, out answerers);

			var questionersFiltered = Topic.FilterConfidence(questioners);
			var answerersFiltered = Topic.FilterConfidence(answerers);
			var result = new Dictionary<string, KeyValuePair<int,double>[]>();
			foreach (var q in answerersFiltered)
			{
				var d = q.Value.TakeWhile(k => k.Value > 10).ToArray();
				if (d.Any())
					result.Add(q.Key,d.OrderByDescending(k => k.Value).ToArray());
			}
			File.WriteAllLines("expertAns.txt", result.Select(k => k.Key + "\t" + String.Join("\t", k.Value.Select(t => t.Key + " " + t.Value))));
		}
	}
}
