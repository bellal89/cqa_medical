using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace cqa_medical.Statistics
{
	/// <summary>
	/// Identifies gender by last name or first name endings
	/// </summary>
	public class GenderDetector
	{
		public enum Gender
		{
			Male,
			Female,
			Unknown
		}
		
		private readonly Func<string, Gender>[] firstNameRules = new Func<string, Gender>[]
		                                                         	{
																		name => GetGenderByDictionary(Program.FemaleNamesFileName, name, Gender.Female),
																		name => GetGenderByDictionary(Program.MaleNamesFileName, name, Gender.Male),
		                                                         		name =>
		                                                         			{
		                                                         				string[] femaleNameEndings = 
																					{
																						"а", "я", "натали"
		                                                         				    };
		                                                         				string[] maleNameEndings =
		                                                         					{
		                                                         						"б", "в", "г", "д", "ж", "з", "й", "к",
		                                                         						"л", "м", "н", "п", "р", "с", "т", "ф",
		                                                         						"х", "ц", "ч", "ш", "щ", "рь"
		                                                         					};
																			
		                                                         				if (IsEndsWith(femaleNameEndings, name))
		                                                         					return Gender.Female;
		                                                         				return IsEndsWith(maleNameEndings, name) ? Gender.Male : Gender.Unknown;
		                                                         			}
		                                                         	};

		private readonly Func<string, Gender>[] lastNameRules = new Func<string, Gender>[]
		                                                        	{
		                                                        		name =>
		                                                        			{
		                                                        				string[] femaleLastNameEndings = { "ая", "aya", "ина", "ina", "ева", "eva", "ова", "ova" };
		                                                        				// Достоевская, Поперечная, Толстая, Куприна, Кошелева, Петрова
		                                                        				string[] maleLastNameEndings = { "ий", "ый", "ой", "ин", "in", "ев", "ev", "ов", "ov" };
		                                                        				// Достоевский, Поперечный, Толстой, Куприн, Кошелев, Петров
																			
		                                                        				if (IsEndsWith(femaleLastNameEndings, name))
		                                                        					return Gender.Female;
		                                                        				return IsEndsWith(maleLastNameEndings, name) ? Gender.Male : Gender.Unknown;
		                                                        			}
		                                                        	};

		private static bool IsEndsWith(IEnumerable<string> endings, string name)
		{
			return endings.Any(letter => name.ToLower().EndsWith(letter.ToString(CultureInfo.InvariantCulture)));
		}

		private static Gender GetGenderByDictionary(string dictionaryPath, string name, Gender gender)
		{
			var names = File.ReadAllLines(dictionaryPath).Select(s => s.ToLower().Trim());
			return names.Contains(name) ? gender : Gender.Unknown;
		}

		// Tries to define by the last name. In case of fail - by the first name.
		public Gender Detect(string name)
		{
			var names = Regex.Split(name.ToLower(), @"\W+").Where(n => n != "").ToList();
			
			if (names.Count == 1)
				return DetectByRules(names[0], firstNameRules);
			if (names.Count < 2)
				return Gender.Unknown;

			var firstName = names[0];
			var lastName = names[1];

			var gender = DetectByRules(lastName, lastNameRules);
			var finalGender = gender != Gender.Unknown ? gender : DetectByRules(firstName, firstNameRules);
			return finalGender;
		}

		public Gender DetectByRules(string name, Func<string, Gender>[] rules)
		{
			if (name.Length < 2)
				return Gender.Unknown;
			foreach (var gender in rules.Select(rule => rule.Invoke(name)).Where(gender => gender != Gender.Unknown))
			{
				return gender;
			}
			return Gender.Unknown;
		}
	}
}