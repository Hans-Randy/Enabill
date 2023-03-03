using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Enabill.Web.Models
{
	public class InvoiceExportToCSVWriter
	{
		public static void WriteExportData(DataTable sourceTable, TextWriter writer, bool includeHeaders)
		{
			if (includeHeaders)
			{
				var headerValues = new List<string>();

				foreach (DataColumn column in sourceTable.Columns)
				{
					headerValues.Add(QuoteValue(column.ColumnName));
				}

				writer.WriteLine(string.Join(",", headerValues.ToArray()));
			}

			string[] items = null;

			foreach (DataRow row in sourceTable.Rows)
			{
				items = row.ItemArray.Select(o => QuoteValue(o.ToString())).ToArray();
				writer.WriteLine(string.Join(",", items));
			}

			writer.Flush();
		}

		private static string QuoteValue(string valueType) => string.Concat("\"", valueType.Replace("\"", "\"\""), "\"");
	}
}