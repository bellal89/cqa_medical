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
		private readonly QuestionList questionList;
		private readonly TopicsStatistics topicStatistics;
		private readonly Dictionary<string, IEnumerable<Question>> userQuestions;
		private readonly Dictionary<Sequence, int> topicMoves;
		private readonly Dictionary<int, int> topicDistribution = new Dictionary<int, int>();
		private readonly int userQuestionsCount;
		private const double Threshold = 0.1;

		public UserTopicsWalking(QuestionList questionList, ICollection<string> categories, TopicsStatistics topicStatistics)
		{
			this.questionList = questionList;
			this.topicStatistics = topicStatistics;

			userQuestions = questionList.GetAllQuestions().Where(q => categories.Contains(q.Category))
				.GroupBy(q => q.AuthorEmail, (userEmail, hisQuestions) => new { userEmail, hisQuestions })
				.Where(u => u.hisQuestions.Count() >= 2)
				.ToDictionary(u => u.userEmail, u => u.hisQuestions);
			foreach (var questions in userQuestions.Values)
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
			topicMoves = topicStatistics.GetUsersTopicSequences(userQuestions, 2, Threshold);
		}

		public Dictionary<string, IEnumerable<Question>> GetUserQuestions()
		{
			return userQuestions;
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

		public Dictionary<Sequence, IEnumerable<Tuple<Question, Question>>> GetMoveQuestions(IEnumerable<Sequence> moves)
		{
			var questionToTopic = userQuestions.Values.Select(
				qs => qs.Select(q => new {id = q.Id, topic = topicStatistics.GetTopicByQuestionId(q.Id, Threshold)}));

			throw new NotImplementedException();
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

			topicStatistics = new TopicsStatistics(Program.DefaultQuestionList);
			userTopicsWalking = new UserTopicsWalking(Program.DefaultQuestionList, categories, topicStatistics);
		}

		[Test]
		public void TestCreation()
		{
			Assert.AreEqual(10965, userTopicsWalking.GetUserQuestions().Count);
		}

		[Test]
		public void TestQuestionSequencies()
		{
			throw new NotImplementedException();
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
		public void GenerateUserTopicsWalkingGraph()
		{
			userTopicsWalking.GenerateTopicWalkingGraph("UserTopicsWalkingGraph.gv", new HashSet<int>(topicExclusions), 20);
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
