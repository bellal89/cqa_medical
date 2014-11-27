using System;
using System.Collections.Generic;

namespace cqa_medical.UtilitsNamespace
{
	[Serializable]
	public class MailUser
	{
		public string Email { get; private set; }
		public DateTime BirthDate { get; set; }
		public string Name { get; set; }
		public string Geo { get; set; }
		public Dictionary<string, string> Info = new Dictionary<string, string>();

		public MailUser(string email)
		{
			Email = email;
		}
	}
}