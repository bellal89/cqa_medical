using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.LDA;

namespace cqa_medical.Statistics
{
	internal struct Sequence
	{
		public readonly int[] Items;

		public Sequence(int count)
		{
			Items = new int[count];
		}

		public Sequence(int count, IList<int> items)
		{
			Items = new int[count];
			for (var i = 0; i < count; i++)
			{
				Items[i] = items[i];
			}
		}

		public override bool Equals(object obj)
		{
			if (!(obj is Sequence))
				return false;
			var seq = (Sequence) obj;
			return Items.SequenceEqual(seq.Items);
		}

		public override string ToString()
		{
			return String.Join("-", Items);
		}

		public override int GetHashCode()
		{
			return String.Join("", Items).GetHashCode();
		}
	}

	class UserTopicsWalking
	{
		private readonly TopicsStatistics topicStatistics;
		public Dictionary<string, IEnumerable<Question>> UserQuestions { get; private set; }
		private readonly Dictionary<Sequence, int> topicMoves;
		private readonly Dictionary<int, int> topicDistribution = new Dictionary<int, int>();
		private readonly int userQuestionsCount;
		private const double Threshold = 0.1;
		private const int HoursBetween = 72;

		public UserTopicsWalking(QuestionList questionList, ICollection<string> categories, TopicsStatistics topicStatistics)
		{
			this.topicStatistics = topicStatistics;

			UserQuestions = questionList.GetAllQuestions().Where(q => categories.Contains(q.Category))
				.GroupBy(q => q.AuthorEmail, (userEmail, hisQuestions) => new { userEmail, hisQuestions })
				.Where(u => u.hisQuestions.Count() >= 2)
				.ToDictionary(u => u.userEmail, u => u.hisQuestions);
			foreach (var questions in UserQuestions.Values)
			{
				foreach (var question in questions)
				{
					var topic = topicStatistics.GetTopicByQuestionId(question.Id, Threshold);
					if (topic == null) continue;

					if (topicDistribution.ContainsKey(topic.Item1))
					{
						topicDistribution[topic.Item1]++;
					}
					else
					{
						topicDistribution[topic.Item1] = 1;
					}
				}
			}
			userQuestionsCount = topicDistribution.Sum(it => it.Value);
			topicMoves = GetTopicSequenceFrequences(2);
		}

		private static IEnumerable<List<Question>> GetSubSequences(IList<Question> sequence, int sequenceLength)
		{
			var subSequences = new List<List<Question>>();
			for (var i = 0; i < sequence.Count - sequenceLength + 1; i++)
			{
				var subSequence = new List<Question>();
				for (var j = i; j < i + sequenceLength; j++)
				{
					subSequence.Add(sequence[j]);
				}
				subSequences.Add(subSequence);
			}
			return subSequences;
		}

		private static IEnumerable<IEnumerable<Question>> GetSessionSequences(IEnumerable<Question> questions, int hoursBetween)
		{
			var timedQuestions = questions.OrderBy(q => q.DateAdded).ToList();
			var sessions = new List<Tuple<int, int>>();
			var sessionBegin = 0;

			for (var i = 1; i < timedQuestions.Count; i++)
			{
				if ((timedQuestions[i].DateAdded - timedQuestions[i - 1].DateAdded).TotalHours <= hoursBetween)
					continue;
				if (i - 1 != sessionBegin)
				{
					sessions.Add(Tuple.Create(sessionBegin, i - 1));
				}
				sessionBegin = i;
			}

			if (timedQuestions.Count - 1 != sessionBegin)
			{
				sessions.Add(Tuple.Create(sessionBegin, timedQuestions.Count));
			}

			return sessions.Select(s => timedQuestions.TakeWhile((q, i) => (i >= s.Item1 && i <= s.Item2)));
		}


		public List<List<Question>> GetUserSessionQuestionSequences(int sequenceLength)
		{
			var userQuestionSequences = new List<List<Question>>();
			foreach (var questions in UserQuestions)
			{
				var sessionQuestionSequencies = GetSessionSequences(questions.Value, HoursBetween);
				foreach (var sequence in sessionQuestionSequencies)
				{

					var subSequences = GetSubSequences(sequence.ToList(), sequenceLength);
					userQuestionSequences.AddRange(subSequences);
				}
			}
			return userQuestionSequences;
		}

		public IEnumerable<List<Tuple<int, Question>>> GetTopicQuestionPairSequences(int sequenceLength)
		{
			var questionSequences = GetUserSessionQuestionSequences(sequenceLength);
			var topicLists =
				questionSequences.Select(
					seq => seq.Select(q => Tuple.Create(topicStatistics.GetTopicByQuestionId(q.Id, Threshold), q))
					       	.Where(t => t.Item1 != null)
					       	.Select(t => Tuple.Create(t.Item1.Item1, t.Item2))
					       	.ToList())
					.Where(
						seq => !seq.Select(it => it.Item1).Contains(-1) && seq.Count != seq.Count(it => it.Item1 == seq.First().Item1));
			return topicLists;
		}

		public Dictionary<Sequence, int> GetTopicSequenceFrequences(int sequenceLength)
		{
			var topicQuestionPairsSequences =
				GetTopicQuestionPairSequences(sequenceLength).Select(seq => seq.Select(pair => pair.Item1).ToList());
			var topicSequences = new Dictionary<Sequence, int>();
			foreach (var topicList in topicQuestionPairsSequences)
			{
				var seq = new Sequence(topicList.Count, topicList);
				if (!topicSequences.ContainsKey(seq))
				{
					topicSequences[seq] = 0;
				}
				topicSequences[seq]++;
			}
			return topicSequences;
		}

		public Dictionary<Sequence, List<List<Question>>> GetTopicMoveQuestions(int lowFrequencyBoundary)
		{
			var moveQuestions = new Dictionary<Sequence, List<List<Question>>>();
			if (topicMoves.Count == 0) return moveQuestions;
			var sequenceLength = topicMoves.First().Key.Items.Length;
			
			var topicQuestionPairSequences = GetTopicQuestionPairSequences(sequenceLength);
			var res = new Dictionary<Sequence, List<List<Question>>>();
			foreach (var sequence in topicQuestionPairSequences)
			{
				var topicSequence = new Sequence(sequence.Count, sequence.Select(pair => pair.Item1).ToList());
				if (!res.ContainsKey(topicSequence))
					res[topicSequence] = new List<List<Question>>();
				res[topicSequence].Add(sequence.Select(pair => pair.Item2).ToList());
			}
			return res.Where(kv => kv.Value.Count >= lowFrequencyBoundary).ToDictionary(kv => kv.Key, kv => kv.Value);
		}

		public double[,] GetStochasticMatrix()
		{
			var matrix = new double[Program.TopicsCount, Program.TopicsCount];
			foreach (var move in topicMoves.Keys)
			{
				matrix[move.Items[0], move.Items[1]] = GetGivenProbability(move);
			}
			return matrix;
		}

		private double GetGivenProbability(Sequence move)
		{
			if (!topicMoves.ContainsKey(move))
			{
				return 0;
			}
			return ((double)topicMoves[move]) * userQuestionsCount / (topicDistribution[move.Items[0]] * topicDistribution[move.Items[1]]);
		}

		public double[,] MatrixPower(double [,] matrix, int pow)
		{
			return pow == 1 ? matrix : MatrixMultiply(matrix, MatrixPower(matrix, pow - 1));
		}

		private static double[,] MatrixMultiply(double[,] m1, double[,] m2)
		{
			Assert.AreEqual(m1.GetLength(1), m2.GetLength(0));
			var height = m1.GetLength(0);
			var width = m2.GetLength(1);

			var res = new double[height, width];
			
			for (var i = 0; i < height; i++)
			{
				for (var j = 0; j < width; j++)
				{
					res[i, j] = 0;
					for (var k = 0; k < m2.GetLength(0); k++)
					{
						res[i,j] += m1[i, k]*m2[k, j];
					}
				}
			}
			return res;
		}

		/// <summary>
		/// User topics walking graph for Gephi generation
		/// </summary>
		/// <param name="fileToSave"></param>
		/// <param name="topicExclusions"></param>
		/// <param name="scale">[0 .. 5) - more detailed, [5 .. ) - less detailed graph</param>
		public void GenerateTopicWalkingGraph(string fileToSave, HashSet<int> topicExclusions, int scale = 5)
		{
			Console.WriteLine(String.Join("\n", topicMoves.OrderByDescending(it => it.Value)));

			var topicConverter = new TopicConverter(Program.TopicsWordsFileName);
			

			var graphEdges = topicMoves
				.Where(it => it.Value > scale)
				.Where(it => !topicExclusions.Contains(it.Key.Items[0]) && !topicExclusions.Contains(it.Key.Items[1]))
				.Select(it => new { seq = it.Key, chance = GetGivenProbability(it.Key)})
				.Select(row => new Edge{SourceId = row.seq.Items[0], DestinationId = row.seq.Items[1], Weight = row.chance})
				.ToList();

			var topics = graphEdges.Aggregate(new HashSet<int>(), (acc, e) =>
			                                         	{
			                                         		acc.Add(e.SourceId);
			                                         		acc.Add(e.DestinationId);
			                                         		return acc;
			                                         	});
			var graphVertices = topics.Select(t => new Vertex
			                                  	{
			                                  		Id = t, 
													Label = String.Join(", ", topicConverter.GetTopicWords(t, 10)), 
													Weight = topicDistribution[t]
			                                  	});
			var graph = new GraphBuilder(graphVertices, graphEdges);

			graph.ExportToGVFormat(fileToSave, "TopikWalkingGraph", isOriented:true);
		}
	}

	[TestFixture]
	public class UserStatisticsTest
	{
		private UserTopicsWalking userTopicsWalking;
		private HashSet<string> categories;
		private int[] topicExclusions;
		private TopicsStatistics topicStatistics;

		[SetUp]
		public void SetUp()
		{
			categories = new HashSet<string>
			             	{
			             		"illness",
			             		"treatment",
			             		"kidhealth",
			             		"doctor"
			             	};
			topicExclusions = new[]
			                {
			                    1, 6, 7, 11, 13, 15, 16, 17, 21, 24, 34, 36, 38, 41, 47, 49, 50, 51, 57, 60, 61, 64, 65, 76,
			                    77, 78, 83, 87, 89
			                };

			topicStatistics = new TopicsStatistics(Program.DefaultNotStemmedQuestionList);
			userTopicsWalking = new UserTopicsWalking(Program.DefaultNotStemmedQuestionList, categories, topicStatistics);
		}

		[Test]
		public void TestCreation()
		{
			Assert.AreEqual(10965, userTopicsWalking.UserQuestions.Count);
		}

		[Test, Explicit]
		public void GenerateUserTopicsWalkingGraph()
		{
			userTopicsWalking.GenerateTopicWalkingGraph("UserTopicsWalkingGraph.gv", new HashSet<int>(topicExclusions), 3);
		}
		
		[Test]
		public void TestQuestionSequencies()
		{
			const int bottomFrequencyThreshold = 7;
			File.WriteAllText("topicMovesQuestions_" + bottomFrequencyThreshold + ".txt",
			                  String.Join("\n\n\n",
			                              userTopicsWalking.GetTopicMoveQuestions(bottomFrequencyThreshold).Select(
			                              	mq =>
			                              	mq.Key + "\n\n" +
			                              	String.Join("\n\n",
			                              	            mq.Value.Select(qs => String.Join("\n", qs.Select(q => q.WholeText)))))));
		}

		[Test]
		public void TestMatrixPower()
		{
			var m = new double[,]{{1, 2},{4,3}};
			var product = userTopicsWalking.MatrixPower(m, 5);
			Assert.AreEqual(1041, product[0, 0]);
			Assert.AreEqual(1042, product[0, 1]);
			Assert.AreEqual(2084, product[1, 0]);
			Assert.AreEqual(2083, product[1, 1]);
		}

		[Test, Explicit]
		public void GetProbabilisticMatrix()
		{
			var m = userTopicsWalking.GetStochasticMatrix();
			
			double k = 0; 
			for (var i = 0; i < m.GetLength(0); i++) { k += m[0, i]; }
			Console.WriteLine(k);
			
			var m50 = userTopicsWalking.MatrixPower(m, 50);

			k = 0;
			var distrib = "";
			for (var i = 0; i < m50.GetLength(0); i++)
			{
				k += m50[0, i];
				distrib += "_" + i + "\t" + m50[0, i] + "\n";
			}
			Console.WriteLine(k);
			
			
			File.WriteAllText("EqualizedTopicsDistrib.txt", distrib);
		}
	}
}
