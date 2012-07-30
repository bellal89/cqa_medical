using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace cqa_medical.Utilits
{
	public  interface IFormatParse<out T>
	{
		 T FormatStringParse(string formattedString);
		 string FormatStringWrite();
	}
}
