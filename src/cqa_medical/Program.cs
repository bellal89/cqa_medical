using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using cqa_medical.DataInput;
using cqa_medical.Statistics;

namespace cqa_medical
{
    class Program
    {
        static void Main(string[] args)
        {
			const string questionsFileName = "../../Files/qst_25.csv";
			const string answersFileName = "../../Files/ans_25.csv";
			const string statisticsDirectory = "../../StatOutput/";
			var questionList = new QuestionList();

			var parser = new Parser(questionsFileName, answersFileName);
			parser.Parse(questionList.AddQuestion, questionList.AddAnswer);
			var statistics = new Statistics.Statistics(questionList);

        	IEnumerable<MethodInfo> infos = statistics.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(m => m.GetCustomAttributes(typeof(StatisticsAttribute), true).Any());

        	foreach (var info in infos)
        	{
        		Console.WriteLine(info.Name);
        		var data = info.Invoke(statistics, new object[0]).ToString();
				File.WriteAllText(statisticsDirectory + info.Name + ".txt", data);
        	}
        }
    }

}
