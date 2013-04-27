//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using NUnit.Framework;
//using cqa_medical.DataInput;
//using cqa_medical.UtilitsNamespace;
//using cqa_medical.UtilitsNamespace.Parsers;
//
//namespace cqa_medical.Statistics
//{
//	class DesToMedTableEvaluator
//	{
//		private readonly InvertedIndexUnit[] deseases;
//		private readonly InvertedIndexUnit[] medicaments;
//		private readonly List<Mkb10Desease> mkb10Deseases;
//		private readonly DeseasesToMedicamentsTable table;
//
//		public DesToMedTableEvaluator(string deseasesFileName, string deseaseNamesFileName, string medicamentNamesFileName)
//		{
//			mkb10Deseases =
//				Mkb10PageParser.GetStemmedDeseases(
//					Mkb10PageParser.GetDeseasesFrom(deseasesFileName, deseaseNamesFileName, medicamentNamesFileName),
//					Program.FilesDirectory + "Mkb10/").ToList();
//
//			mkb10Deseases = JoinSomeDeseases(mkb10Deseases);
//			deseases = Mkb10PageParser.GetMkb10DeseasesFuzzyIndex(mkb10Deseases).ToArray();
//			medicaments = Mkb10PageParser.GetMkb10MedicamentsFuzzyIndex(mkb10Deseases, false).ToArray();
//			table = new DeseasesToMedicamentsTable(deseases, medicaments, 1);
//		}
//
//		private static List<Mkb10Desease> JoinSomeDeseases(List<Mkb10Desease> deseases)
//		{
//			var mkb10DeseasesUnion = new Mkb10DeseasesUnion();
//			return mkb10DeseasesUnion.GetJoinedDeseases(deseases);
//		}
//
//		public double GetMark1()
//		{
//			var rightItemsCount = GetRightItemCounts().Sum();
//			return ((double)rightItemsCount)/table.GetDeseaseMedicamentQuestions().Sum(des => des.Value.Count);
//		}
//
//		public double GetMark2()
//		{
//			var rightItemsCount = GetRightItemCounts().Sum();
//			var questionList = Program.DefaultQuestionList;
//			var rightItemsAnswersCount = table.GetDeseaseMedicamentQuestions().SelectMany(it => it.Value).Sum(q => questionList.GetQuestion(q).GetAnswers().Count);
//			return ((double) rightItemsCount)/rightItemsAnswersCount;
//		}
//
//		public IEnumerable<int> GetRightItemCounts()
//		{
//			var rightMedicaments = GetRightMedicaments();
//
//			var rightItemCounts = new List<int>();
//			foreach (var desMedCount in table.MedicalGuide)
//			{
//				if (!rightMedicaments.ContainsKey(desMedCount.Key.Item1))
//					continue;
//				if (!rightMedicaments[desMedCount.Key.Item1].Contains(desMedCount.Key.Item2))
//					continue;
//				rightItemCounts.Add(desMedCount.Value);
//			}
//			return rightItemCounts;
//		}
//
//		public IEnumerable<int> GetWrongItemCounts()
//		{
//			var rightMedicaments = GetRightMedicaments();
//
//			var wrongItemCounts = new List<int>();
//			var popularWrongDesMed = new List<Tuple<string, int>>();
//			foreach (var desMedCount in table.MedicalGuide)
//			{
//				if (!rightMedicaments.ContainsKey(desMedCount.Key.Item1) ||
//					!rightMedicaments[desMedCount.Key.Item1].Contains(desMedCount.Key.Item2))
//				{
//					wrongItemCounts.Add(desMedCount.Value);
//					if (desMedCount.Value > 20)
//					{
//						popularWrongDesMed.Add(
//							Tuple.Create(desMedCount.Key.Item1 + "\n" + desMedCount.Key.Item2 + "\n" + desMedCount.Value, desMedCount.Value));
//					}
//				}
//			}
//			File.WriteAllText("WrongDesMed.txt", String.Join("\n\n", popularWrongDesMed.OrderByDescending(desMed => desMed.Item2).Select(desMed => desMed.Item1)));
//			return wrongItemCounts;
//		}
//
//		private Dictionary<string, HashSet<string>> GetRightMedicaments()
//		{
//			return mkb10Deseases.ToDictionary(des => des.Name + " (" + String.Join(", ", des.Synonyms) + ")",
//			                                                  des =>
//			                                                  new HashSet<string>(
//			                                                  	des.TradeNames.Select(
//			                                                  		tn => tn.Name + "(" + String.Join(", ", tn.ActiveSubstances) + ")")));
//		}
//
//		public Dictionary<string, List<Tuple<List<string>, long, string, long, string>>> GetHandyDesMedQuestions(int highBound, params string[] filters)
//		{
//			var filterSet = new HashSet<string>(filters);
//			var deseaseMedicamentQuestions =
//				table.GetDeseaseMedicamentQuestions().Where(
//					desmed => desmed.Key.Trim().ToLower().SplitIntoWords().Intersect(filterSet).Any()).ToDictionary(kv => kv.Key,
//					                                                                                                kv => kv.Value);
//			var qaPairs = GetQuestionAnswerPairs(deseaseMedicamentQuestions);
//			if (qaPairs.SelectMany(kv => kv.Value).Count() > highBound)
//			{
//				var usedAnswers = new HashSet<long>();
//				var randomlySelectedQA = new List<Tuple<string, Tuple<List<string>, long, string, long, string>>>();
//				var rand = new Random();
//				var qaPairsList = qaPairs.SelectMany(kv => kv.Value.Select(qa => Tuple.Create(kv.Key, qa))).ToList();
//				for(var i = 0; i < highBound; i++)
//				{
//					int id;
//					do
//					{
//						id = rand.Next(qaPairsList.Count - 1);
//					} while (usedAnswers.Contains(qaPairsList[id].Item2.Item4));
//					randomlySelectedQA.Add(qaPairsList[id]);
//					usedAnswers.Add(qaPairsList[id].Item2.Item4);
//				}
//				return randomlySelectedQA
//					.GroupBy(qa => qa.Item1, (key, qaItems) => Tuple.Create(key, qaItems.Select(it => it.Item2).ToList()))
//					.ToDictionary(it => it.Item1, it => it.Item2);
//			}
//			return qaPairs;
//		}
//
//		public Dictionary<string, List<Tuple<List<string>, long, string, long, string>>> GetRandomDesMedQuestions(int randomDeseasesCount, int questionsPerDeseaseCount)
//		{
//			var deseaseMedicamentQuestions = table.GetDeseaseMedicamentQuestions();
//			var desMedQuestions = deseaseMedicamentQuestions.Where(desMed => desMed.Value.Count >= 20).ToList();
//
//			var desMajority = GetMajority(desMedQuestions);
//
//			var rnd = new Random(DateTime.Now.Millisecond);
//			var randomDeseases = new Dictionary<string, List<long>>();
//			var i = 0; 
//			while(i < randomDeseasesCount)
//			{
//				var des = desMajority.ElementAt(rnd.Next(desMajority.Count - 1));
//				if (randomDeseases.ContainsKey(des.Key))
//					continue;
//				randomDeseases[des.Key] = new List<long>();
//
//				for (var j = 0; j < questionsPerDeseaseCount; j++ )
//					randomDeseases[des.Key].Add(des.Value.ElementAt(rnd.Next(des.Value.Count - 1)));
//
//				i++;
//			}
//			return GetQuestionAnswerPairs(randomDeseases);
//		}
//
//		private static List<KeyValuePair<string, List<long>>> GetMajority(ICollection<KeyValuePair<string, List<long>>> desMedQuestions)
//		{
//			return
//				desMedQuestions.OrderByDescending(desMed => desMed.Value.Count).Take((int) (0.75* desMedQuestions.Count)).Reverse().Take(
//					(int)(0.5 * desMedQuestions.Count)).ToList();
//		}
//
//		private Dictionary<string, List<Tuple<List<string>, long, string, long, string>>> GetQuestionAnswerPairs(Dictionary<string, List<long>> desToQuestions)
//		{
//			var answersMedIndex = Mkb10PageParser.GetMkb10MedicamentsFuzzyIndex(mkb10Deseases, false, false).ToList();
//			var ansToMed = new Dictionary<long, List<string>>();
//
//			foreach (var med in answersMedIndex)
//			{
//				foreach (var id in med.Ids)
//				{
//					if (!ansToMed.ContainsKey(id))
//						ansToMed[id] = new List<string>();
//					ansToMed[id].Add(med.Word);
//				}
//			}
//
//			var questionList = Program.DefaultNotStemmedQuestionList;
//
//			var desQuestionsList = desToQuestions.SelectMany(des => des.Value.SelectMany(
//												qId =>
//												questionList.GetQuestion(qId).GetAnswers().Where(a => ansToMed.ContainsKey(a.Id)).Select(
//														a => Tuple.Create(des.Key, ansToMed[a.Id], a.QuestionId, MarkItem(des.Key, questionList.GetQuestion(a.QuestionId).WholeText, "des"), a.Id, MarkItems(ansToMed[a.Id], a.Text, "med")))).ToList());
//			// Make unique answer ids (Item 5) and return:
//			return
//				desQuestionsList.GroupBy(it => it.Item5, (ansId, values) => values.First()).GroupBy(it => it.Item1,
//				                                                                                    (desName, values) =>
//				                                                                                    Tuple.Create(desName, values)).
//					ToDictionary(it => it.Item1,
//					             it => it.Item2.Select(qa => Tuple.Create(qa.Item2, qa.Item3, qa.Item4, qa.Item5, qa.Item6)).ToList());
//		}
//
//		private static string MarkItems(IEnumerable<string> medList, string text, string className)
//		{
//			return MarkItem(String.Join(",", medList), text, className);
//		}
//
//		private static string MarkItem(string desease, string text, string className)
//		{
//			var stemmer = Program.DefaultMyStemmer;
//
//			var desNames = desease.TrimEnd(')').Split(new[] {'(', ','}, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
//
//			var textTokens = SplitIntoTokens(text);
//			var textSet = new HashSet<string>(textTokens.Where(it => it.Item2).Select(it => stemmer.Stem(it.Item1)));
//			var newText = new List<string>();
//			
//			foreach (var desName in desNames)
//			{
//				var desSet = new HashSet<string>(desName.SplitIntoWords());
//				if(desSet.Intersect(textSet).Count() != desSet.Count)
//					continue;
//				foreach (var token in textTokens)
//				{
//					if(!token.Item2)
//					{
//						newText.Add(token.Item1);
//						continue;
//					}
//					if (desSet.Contains(stemmer.Stem(token.Item1)))
//					{
//						newText.Add("<b class='" + className + "'>" + token.Item1 + "</b>");
//						desSet.Remove(token.Item1);
//					}
//					else
//						newText.Add(token.Item1);
//				}
//				return String.Join("", newText);
//			}
//			return text;
//		}
//
//		private static List<Tuple<string, bool>> SplitIntoTokens(string text)
//		{
//			var isWord = false;
//			var token = "";
//			var tokens = new List<Tuple<string, bool>>();
//			foreach (var symbol in text)
//			{
//				if(Char.IsLetterOrDigit(symbol) && !isWord || !Char.IsLetterOrDigit(symbol) && isWord)
//				{
//					tokens.Add(Tuple.Create(token, isWord));
//					token = "";
//					isWord = !isWord;
//				}
//				token += symbol;
//			}
//			if(token != "")
//			{
//				tokens.Add(Tuple.Create(token, isWord));
//			}
//			return tokens;
//		}
//
//		public IEnumerable<Tuple<string, long>> GetRandomDesNotMedQuestions(int count)
//		{
//			var desQuestions = new List<Tuple<string, long>>();
//			var deseaseNotMedicamentQuestions = table.GetDeseaseNotMedicamentQuestions();
//			Console.WriteLine("Desease-not-medicament questions count = " + deseaseNotMedicamentQuestions.Count);
//			var desToQuestions = deseaseNotMedicamentQuestions.Where(des => des.Value.Count > 20).ToList();
//			var desMajority = GetMajority(desToQuestions);
//
//			var rnd = new Random(DateTime.Now.Millisecond);
//			for (var i = 0; i < count; i++)
//			{
//				var des = desMajority.ElementAt(rnd.Next(desMajority.Count - 1));
//				desQuestions.Add(Tuple.Create(des.Key, des.Value.ElementAt(rnd.Next(des.Value.Count - 1))));
//			}
//
//			return desQuestions;
//		}
//
//		public IEnumerable<string> GetTopMkb10Deseases(int count)
//		{
//			var topDeseases = deseases.Select(des => Tuple.Create(des.Word, des.Ids.Count)).OrderByDescending(it => it.Item2).Take(count);
//			return topDeseases.Select(it => it.Item1).Select(des =>
//			                                                 	{
//			                                                 		var parts = des.TrimEnd(')').Split(new[] {'('},
//			                                                 		                                   StringSplitOptions.
//			                                                 		                                   	RemoveEmptyEntries);
//			                                                 		if (parts.Length < 2) return parts[0];
//			                                                 		return parts[0] + "\n\t" +
//			                                                 		       String.Join("\n\t",
//			                                                 		                   parts[1].Split(',').Select(syn => syn.Trim()));
//			                                                 	});
//		}
//	}
//
//	[TestFixture]
//	public class DesToMedEvaluatorTest
//	{
//		const string DeseasesFileName = Program.FilesDirectory + "Mkb-10-Handy/Mkb10-handy-deseases.txt";
//		const string DeseaseNamesFileName = Program.FilesDirectory + "Mkb-10-Handy/Mkb10-handy-deseaseNames.txt";
//		const string MedicamentNamesFileName = Program.FilesDirectory + "Mkb-10-Handy/Mkb10-handy-medicamentNames.txt";
//
//		readonly DesToMedTableEvaluator evaluator = new DesToMedTableEvaluator(DeseasesFileName, DeseaseNamesFileName, MedicamentNamesFileName);
//
//		[Test]
//		public void TestCreation()
//		{
//			Console.WriteLine( "Correct Qdes-Amed / All Qdes-Amed = " + evaluator.GetMark1());
//			Console.WriteLine("Correct Qdes-Amed / All Qdes-A = " + evaluator.GetMark2());
//			Console.WriteLine("Wrong Qdes-Amed:\n" + String.Join("\n", evaluator.GetWrongItemCounts().OrderByDescending(cnt => cnt)));
//		}
//
//		[Test, Explicit]
//		public void CreateRandomDeseasesSet()
//		{
//			var randomDeses = evaluator.GetRandomDesMedQuestions(5, 10);
//
//			File.WriteAllLines("Random-deseases.txt",
//			                   randomDeses.SelectMany(
//			                   	des =>
//			                   	des.Value.Select(
//			                   		it =>
//									"0\t" +
//			                   		des.Key + "\t" + string.Join("\t+\t", it.Item1) + "\t" + it.Item2 + "\t" + it.Item3 + "\t" + it.Item4 +
//			                   		"\t" + it.Item5)));
//		}
//
//		[Test, Explicit]
//		public void CreateHandyDeseasesSet()
//		{
//			var randomDeses = evaluator.GetHandyDesMedQuestions(500, "аллергия", "насморк", "ринит", "ангина", "тонзиллит",
//			                                                    "гастрит", "понос", "диарея", "грипп", "орз", "молочница",
//			                                                    "герпес", "геморрой", "дисбактериоз", "отит");
//			
//			File.WriteAllLines("Handy-deseases.txt",
//							   randomDeses.SelectMany(
//								des =>
//								des.Value.Select(
//									it =>
//									"0\t" +
//									des.Key + "\t" + 
//									string.Join(" + ", it.Item1) + "\t" + 
//									it.Item2 + "\t" + it.Item3 + "\t" + it.Item4 + "\t" + it.Item5)));
//		}
//
//		[Test, Explicit]
//		public void GetRandomDesNotMedQuestions()
//		{
//			File.WriteAllText("Random-des-not-med.txt",
//			                  String.Join("\n\n===\n\n",
//			                              evaluator.GetRandomDesNotMedQuestions(250).Select(
//			                              	pair =>
//			                              	pair.Item1 + "\n---\n" +
//			                              	Program.DefaultNotStemmedQuestionList.GetQuestion(pair.Item2).WholeText + "\n---\n" +
//			                              	String.Join("\n",
//			                              	            Program.DefaultNotStemmedQuestionList.GetQuestion(pair.Item2).GetAnswers()
//			                              	            	.Select(a => a.Text)))));
//		}
//
//		[Test]
//		public void GetSynToDeseases()
//		{
//			var synonimsToDeseasesRelation = evaluator.GetSynonimsToDeseeasesRelation().OrderByDescending(syn => syn.Value.Count);
//			File.WriteAllLines("SynToDesCount.txt", synonimsToDeseasesRelation.Select(syn => syn.Key + "\t" + syn.Value.Count));
//			File.WriteAllText("SynToDes.txt", String.Join("\n===\n", synonimsToDeseasesRelation.Select(syn => syn.Key + "\n\n" + String.Join("\n", syn.Value))));
//		}
//
//		[Test]
//		public void GenerateTopMkb10DeseasesList()
//		{
//			File.WriteAllText("Mkb10-des-top100.txt", String.Join("\n\n", evaluator.GetTopMkb10Deseases(100)));
//		}
//	}
//}
