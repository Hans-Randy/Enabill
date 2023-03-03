using System.Text;

namespace Enabill.Print
{
	internal class PrintUtils
	{
		public static string HTMLToText(string htmlText)
		{
			if (string.IsNullOrEmpty(htmlText))
				return "";

			char[] chars = htmlText.ToCharArray();
			char[] newChars = new char[chars.Length];
			bool isTag = false;
			bool isText = false;
			char lastChar = ' ';
			string currentTag = "";
			var sb = new StringBuilder();

			foreach (char c in chars)
			{
				// if we are in a tag the only thing that can chance it is the close tag '>' character
				if (isTag)
				{
					currentTag += c;
					if (c == '>')
					{
						isTag = false;
						isText = !(currentTag.IndexOf("<style", System.StringComparison.OrdinalIgnoreCase) >= 0);
					}
					continue;
				}

				// if we are in text, then only '<' can change it
				if (c == '<')
				{
					currentTag = "<";
					isTag = true;
					isText = false;
					continue;
				}

				if (isText)
				{
					// else we are in text and must use it
					//if (!(lastChar == ' ' && c == lastChar))
					//{
					sb.Append(c);
					lastChar = c;
					//newChars[i] = c;
					//i++;
					//}
				}
			}

			//sb.Append(newChars);
			sb = sb.Replace("\r\n", "\n")
				.Replace("\n\n", "\n")
				.Replace("&nbsp;", " ")
				.Replace("     ", " ")
				.Replace("    ", " ")
				.Replace("   ", " ")
				.Replace("  ", " ")
				.Replace("  ", " ")
				.Replace("&quot;", "\"")
				.Replace("&gt;", ">")
				.Replace("&lt;", "<")
				.Replace("&#39;", "'")
				.Replace("&rsquo;", "'");
			//System.Web.UI.WebControls.StringArrayConverter sa = new StringArrayConverter();

			return sb.ToString();
		}

		public static string HTMLToCleanHTML(string htmlText)
		{
			char[] chars = htmlText.ToCharArray();
			char[] newChars = new char[chars.Length];
			bool isTag = false;
			bool tagClosed = false;
			bool isText = false;
			char lastChar = ' ';
			string currentTag = "";
			var sb = new StringBuilder();

			foreach (char c in chars)
			{
				// if we are in a tag the only thing that can chance it is the close tag '>' character
				if (isTag)
				{
					currentTag += c;
					if ((c == ' ' || c == ':') && !tagClosed)
					{
						sb.Append('>');
						tagClosed = true;
					}

					if (c == '>')
					{
						isTag = false;
						isText = !(currentTag.IndexOf("<style", System.StringComparison.OrdinalIgnoreCase) >= 0);
					}

					if (!tagClosed)
						sb.Append(c);
					continue;
				}

				// if we are in text, then only '<' can change it
				if (c == '<')
				{
					currentTag = "<";
					sb.Append(c);
					isTag = true;
					tagClosed = false;
					isText = false;
					continue;
				}

				if (isText)
				{
					sb.Append(c);
					lastChar = c;
				}
			}

			//sb.Append(newChars);
			sb = sb.Replace("> <", "><")
				.Replace(">&nbsp;<", "><")
				.Replace("<o>&nbsp;</o>", "")
				.Replace("<o></o>", "")
				.Replace("<span>&nbsp;</span>", "")
				.Replace("<span></span>", "")
				.Replace("<i></i>", "")
				.Replace("<p></p>", "")
				.Replace("\n\n", "\n");

			//sb = sb.Replace("&nbsp;", " ");
			//sb = sb.Replace("     ", " ");
			//sb = sb.Replace("    ", " ");
			//sb = sb.Replace("   ", " ");
			//sb = sb.Replace("  ", " ");
			//sb = sb.Replace("  ", " ");
			//System.Web.UI.WebControls.StringArrayConverter sa = new StringArrayConverter();

			return sb.ToString();
		}
	}
}