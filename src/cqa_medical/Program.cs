using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cqa_medical.DataInput;

namespace cqa_medical
{
    class Program
    {
        static void Main(string[] args)
        {
			var questionsFileName = "../../Files/qst_25.csv";
			var answersFileName = "../../Files/ans_25.csv";
			var questionList = new QuestionList();

			var parser = new Parser(questionsFileName, answersFileName);
			parser.Parse(questionList.AddQuestion, questionList.AddAnswer);
			Console.WriteLine("Done!");
        }
    }

}
