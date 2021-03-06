﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using cqa_medical.DataInput;
using cqa_medical.DataInput.Stemmers;

namespace cqa_medical.UtilitsNamespace
{
	public abstract class LDADataGenerator
	{
		protected readonly QuestionList QuestionList;
		protected readonly string VocabularyStorePath;
		protected readonly string DocumentsStorePath;

		protected LDADataGenerator(QuestionList questionList, string vocabularyStorePath, string documentsStorePath)
		{
			QuestionList = questionList;
			VocabularyStorePath = vocabularyStorePath;
			DocumentsStorePath = documentsStorePath;
		}

		public void GenerateDocuments()
		{
			GenerateDocuments(QuestionList.GetAllQuestions().Count());
		}

		abstract public void GenerateDocuments(int count);
	}

	class InferFormatLDAGenerator : LDADataGenerator
	{
		public InferFormatLDAGenerator(QuestionList questionList, string vocabularyStorePath, string documentsStorePath) :
			base(questionList, vocabularyStorePath, documentsStorePath)
		{
		}

		public override void GenerateDocuments(int count)
		{
			var allWordCountsByDocuments = new List<Dictionary<int, int>>();
			var wordToId = new Dictionary<string, int>();
			var i = 0;
			foreach (var question in QuestionList.GetAllQuestions().Take(count))
			{
				var wordIdToCountInDocument = new Dictionary<int, int>();

				var statisticGenerator = new DistributionCreator<string>(question.WholeText.SplitInWordsAndStripHTML());
				statisticGenerator.AddData(question.GetAnswers().SelectMany(t => t.Text.SplitInWordsAndStripHTML()));

				var wordToCountInDocument = statisticGenerator.GetData();
				foreach (var word in wordToCountInDocument.Keys)
				{
					if (!wordToId.ContainsKey(word))
					{
						wordToId[word] = i++;
					}
					wordIdToCountInDocument[wordToId[word]] = wordToCountInDocument[word];
				}
				allWordCountsByDocuments.Add(wordIdToCountInDocument);
			}

			// Vocabulary storing
			File.WriteAllText(VocabularyStorePath,
							  String.Join(Environment.NewLine, wordToId.OrderBy(item => item.Value).Select(item => item.Key)));

			// Word counts per document storing
			File.WriteAllText(DocumentsStorePath,
							  String.Join(Environment.NewLine,
										  allWordCountsByDocuments.Select(item => item.Count + " " + String.Join(" ", item.Select(idToCount => idToCount.Key + ":" + idToCount.Value))
										  )));
		}
	}

	public class GibbsFormatLDAGenerator : LDADataGenerator
	{
		private readonly string documentIdsFilePath;

		public GibbsFormatLDAGenerator(QuestionList questionList, string documentIdsFilePath, string documentsFilePath) :
			base(questionList, "", documentsFilePath)
		{
			this.documentIdsFilePath = documentIdsFilePath;
		}

		public override void GenerateDocuments(int count)
		{
			var documents = GetPureQuestionAnswersTexts(count).ToArray();

			//Storing docIds
			File.WriteAllText(documentIdsFilePath, String.Join("\n", documents.Select(d => d.Key)));

			// Storing documents
			File.WriteAllText(DocumentsStorePath,
							  documents.Length
								+ "\n"
								+ String.Join("\n", documents.Select(d => d.Value)));
		}

		private Dictionary<long, string> GetPureQuestionAnswersTexts(int count)
		{
			var statistics = new Statistics.Statistics(QuestionList);
			var orderedFrequentWords = statistics.WordFrequencyDistribution(new EmptyStemmer()).Where(item => item.Value >= 10).OrderBy(item => item.Value);
			var frequentWords = orderedFrequentWords.Take(orderedFrequentWords.Count() - 70).ToDictionary(item => item.Key, item => item.Value);

			return QuestionList.GetAllQuestions()
					.Take(count)
					.Select(q => Tuple.Create(q, q.WholeText))
					.Select(item => Tuple.Create(item.Item1, item.Item2 + " " + String.Join(" ", item.Item1.GetAnswers().Select(a => a.Text))))
					.Select(item => Tuple.Create(item.Item1, item.Item2.SplitInWordsAndStripHTML()))
					.Select(item => Tuple.Create(item.Item1, String.Join("\t", item.Item2.Where(frequentWords.ContainsKey))))
					.Where(item => item.Item2.Length > 0)
					.ToDictionary(item => item.Item1.Id, item => item.Item2);
		}
	}

	[TestFixture]
	public class LDADataGeneratorTest
	{
		[Test]
		public static void TestGeneration()
		{
			var generators = new LDADataGenerator[2];
			generators[0] = new InferFormatLDAGenerator(Program.TestDefaultQuestionList, "testVoc.txt", "testCounts.txt");
			generators[1] = new GibbsFormatLDAGenerator(Program.TestDefaultQuestionList, "testGibbsDocIds2.txt", "testGibbsDocs2.txt");
			foreach (var gen in generators)
			{
				gen.GenerateDocuments();
			}
		}

		[Test, Explicit]
		public static void GibbsStemmedLDADataGeneration()
		{
			LDADataGenerator generator = new GibbsFormatLDAGenerator(Program.DefaultQuestionList, "GibbsDocIds.txt", "GibbsDocsTab.txt");
			generator.GenerateDocuments();
		}

		[Test, Explicit]
		public static void LDADataGeneration()
		{
			LDADataGenerator generator = new InferFormatLDAGenerator(Program.DefaultNotStemmedQuestionList, "notStemmedVoc.txt", "notStemmedCounts.txt");
			generator.GenerateDocuments();
		}

		[Test, Explicit]
		public static void LDAStemmedDataGeneration()
		{
			LDADataGenerator generator = new InferFormatLDAGenerator(Program.DefaultQuestionList, "voc.txt", "counts.txt");
			generator.GenerateDocuments();
		}

	}
}