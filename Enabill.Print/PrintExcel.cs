using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.UserModel.Contrib;
using NPOI.HSSF.Util;

namespace Enabill.Print
{
	public class SpreadSheetExporter
	{
		private HSSFWorkbook _workbook = new HSSFWorkbook();
		private HSSFFont _font;
		private HSSFFont _boldfont;
		private HSSFSheet _worksheet;
		private string[] _headings;
		private ArrayList _rowsArray = new ArrayList();

		public SpreadSheetExporter(string sheetName)
		{
			this.InitializeWorkbook();
			this._worksheet = this._workbook.CreateSheet(sheetName);
		}

		public void SetCurrentSpreadsheet(string worksheetName) => this._worksheet = this._workbook.GetSheet(worksheetName);

		public SpreadSheetExporter(string[] sheetList)
		{
			this.InitializeWorkbook();

			foreach (string sheet in sheetList)
			{
				this._worksheet = this._workbook.CreateSheet(sheet);
			}
		}

		private void InitializeWorkbook()
		{
			this._workbook = new HSSFWorkbook();
			this._font = this._workbook.CreateFont();
			this._font.FontName = "Arial";
			this._font.FontHeightInPoints = 10;

			this._boldfont = this._workbook.CreateFont();
			this._boldfont.FontName = "Arial";
			this._boldfont.FontHeightInPoints = 10;
			this._boldfont.Boldweight = HSSFFont.BOLDWEIGHT_BOLD;

			////create a entry of DocumentSummaryInformation
			var dsi = PropertySetFactory.CreateDocumentSummaryInformation();
			//dsi.Company = "Saratoga";
			dsi.Company = Code.Constants.COMPANYNAME;
			this._workbook.DocumentSummaryInformation = dsi;

			////create a entry of SummaryInformation
			var si = PropertySetFactory.CreateSummaryInformation();
			si.Subject = Code.Constants.COMPANYNAME;
			this._workbook.SummaryInformation = si;
		}

		// TODO: Move all style specific code to separate
		// independent classes.

		#region Format Cells

		#region Font Styling

		private void SetFontSize(int row, short fontHeightInPoints, HSSFCellStyle cellStyle)
		{
			var font = this._workbook.CreateFont(); // Create a new font in the workbook
			font.FontHeightInPoints = fontHeightInPoints;
			cellStyle.SetFont(font);  // Attach the font to the Style

			foreach (HSSFCell cell in this._worksheet.GetRow(row))
			{
				cell.CellStyle = cellStyle; // Assign style to all the cells
			}
		}

		public void SetFontWeight(int row, HSSFCellStyle cellStyle)
		{
			var font = this._workbook.CreateFont(); // Create a new font in the workbook

			font.Boldweight = HSSFFont.BOLDWEIGHT_BOLD;

			cellStyle.SetFont(font);  // Attach the font to the Style

			foreach (HSSFCell cell in this._worksheet.GetRow(row))
			{
				cell.CellStyle = cellStyle; // Assign style to all the cells
			}
		}

		#endregion Font Styling

		private void FormatCell(int row, int column, HSSFCellStyle cellStyle)
		{
			var _row = this._worksheet.GetRow(row);
			var cell = _row.GetCell(column) ?? _row.CreateCell(column);

			cellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("Text");
			cell.CellStyle = cellStyle;
		}

		private void FormatCustomCell(int row, int column, HSSFCellStyle cellStyle)
		{
			var _row = this._worksheet.GetRow(row);
			HSSFCell cell = null;
			var format = this._workbook.CreateDataFormat();

			cell = _row.GetCell(column) ?? _row.CreateCell(column);

			cellStyle.DataFormat = format.GetFormat("yyyy-mm-dd");
			cell.CellStyle = cellStyle;
		}

		public void MergeCell(int firstRow, int lastRow, int firstColumn, int lastColumn)
		{
			var region = new CellRangeAddress(firstRow, lastRow, firstColumn, lastColumn);
			this._worksheet.AddMergedRegion(region);
		}

		private void SetBorder(HSSFCellStyle cellStyle)
		{
			cellStyle.BorderTop = HSSFBorderFormatting.BORDER_THIN;
			cellStyle.BorderRight = HSSFBorderFormatting.BORDER_THIN;
			cellStyle.BorderBottom = HSSFBorderFormatting.BORDER_THIN;
			cellStyle.BorderLeft = HSSFBorderFormatting.BORDER_THIN;
		}

		private void TextAlign(int row, int column, HSSFCellStyle cellStyle)
		{
			var _row = this._worksheet.CreateRow(row);
			var _cell = _row.CreateCell(column);

			cellStyle.Alignment = HSSFCellStyle.ALIGN_CENTER;

			_cell.CellStyle = cellStyle;
		}

		public void AutoFitColumns(int nrColumns)
		{
			for (int i = 0; i <= nrColumns; i++)
			{
				this._worksheet.AutoSizeColumn((short)i);
			}
		}

		#endregion Format Cells

		#region WRITE

		public void WriteLine(int row, string[] values)
		{
			var r = this._worksheet.CreateRow(row);

			for (int i = 0; i < values.Length; i++)
			{
				var cellStyle = this._workbook.CreateCellStyle();
				cellStyle.SetFont(this._font);
				HSSFCellUtil.CreateCell(r, i, values[i], cellStyle);
			}
		}

		public void WriteLineBoldFont(int row, string[] values)
		{
			var r = this._worksheet.CreateRow(row);

			for (int i = 0; i < values.Length; i++)
			{
				var cellStyle = this._workbook.CreateCellStyle();
				cellStyle.SetFont(this._boldfont);
				HSSFCellUtil.CreateCell(r, i, values[i], cellStyle);
				//Set dataformat also does not seem to be available
				//int cellValue;
				//bool cellValueIsInt = int.TryParse(values[i].ToString(), out cellValue);
				//if (cellValueIsInt)
				//     cellStyle.DataFormat = HSSFDataFormat.GetBuiltinFormat("Number");
			}
		}

		public void WriteLine(int row, int column, string value)
		{
			var r = this._worksheet.GetRow(row) ?? this._worksheet.CreateRow(row);

			HSSFCellUtil.CreateCell(r, column, value);
		}

		public void WriteLineBoldFont(int row, int column, string value)
		{
			var r = this._worksheet.GetRow(row) ?? this._worksheet.CreateRow(row);

			var cellStyle = this._workbook.CreateCellStyle();
			cellStyle.SetFont(this._boldfont);
			HSSFCellUtil.CreateCell(r, column, value, cellStyle);
			//method does not seem to be available cellStyle.setFillBackgroundColor(HSSFColor.GREY_25_PERCENT.index);
		}

		public void WriteLine(int row, int column, string value, params short?[] style)
		{
			var r = this._worksheet.GetRow(row);
			var cellStyle = this._workbook.CreateCellStyle();

			//if (r == null) r = _worksheet.CreateRow(row);
			//if (style == null)
			//{
			//    this.WriteLine(row, column, value);
			//}
			//else
			//{
			//    for (int i = 0; i < style.Length; i++)
			//    {
			//        if (style[i] == OMCellStyle.BORDER_ALL)
			//        {
			//            this.SetBorder(cellStyle);
			//        }
			//        else if (style[i] == OMFont.FONT_WEIGHT_BOLD)
			//        {
			//            this.SetFontWeight(row, cellStyle);
			//        }
			//        else if (style[i] == OMCellStyle.ALIGN_CENTER)
			//        {
			//            this.TextAlign(row, column, cellStyle);
			//        }
			//        else if (style[i] == OMDataFormat.TEXT)
			//        {
			//            this.FormatCell(row, column, cellStyle);
			//        }
			//        else if (style[i] == OMDataFormat.CCYYMMDD)
			//        {
			//            this.FormatCustomCell(row, column, cellStyle);
			//        }
			//    }
			//    HSSFCellUtil.CreateCell(r, column, value, cellStyle);
			//}
		}

		public void WriteLine(int row, string[] values, params short?[] style)
		{
			var r = this._worksheet.CreateRow(row);
			var cellStyle = this._workbook.CreateCellStyle();

			//for (int i = 0; i < values.Length; i++)
			//{
			//    if (style == null)
			//    {
			//        HSSFCellUtil.CreateCell(r, i, values[i]);
			//    }
			//    else
			//    {
			//        for (int j = 0; j < style.Length; j++)
			//        {
			//            if (style[j] == OMCellStyle.BORDER_ALL)
			//            {
			//                this.SetBorder(cellStyle);
			//            }
			//            else if (style[j] == OMFont.FONT_WEIGHT_BOLD)
			//            {
			//                this.SetFontWeight(row, cellStyle);
			//            }
			//        }
			//        HSSFCellUtil.CreateCell(r, i, values[i], cellStyle);
			//    }
			//}
		}

		public void Write(FileStream writer) => this._workbook.Write(writer);

		public void Write(Stream stream) => this._workbook.Write(stream);

		#endregion WRITE

		public void SetHeadings(string[] headingsList) => this._headings = headingsList;

		public int GetHeadingCount() => this._headings.Length;

		public void AddRow(List<string> row) => this._rowsArray.Add(row);

		public int RowCount() => this._rowsArray.Count;

		public void GenerateSpreadSheet()
		{
			int rowCount = 0;

			if (this._headings?.Length > 0)
			{
				this.WriteLine(rowCount++, this._headings);
			}

			foreach (List<string> row in this._rowsArray)
			{
				this.WriteLine(rowCount++, row.ToArray());
			}
		}

		public byte[] GetBytes()
		{
			//Write the stream data of workbook to the root directory
			var ms = new MemoryStream();
			//FileStream file = new FileStream(@"test.xls", FileMode.Create);
			this._workbook.Write(ms);
			ms.Close();

			return ms.GetBuffer();
		}

		public string GetCell(string sheetname, int rowNumber, int columnNumber)
		{
			var sheet = this._workbook.GetSheet(sheetname);
			var rowOfSheet = sheet.GetRow(rowNumber);
			var cell = rowOfSheet.GetCell(columnNumber);

			return cell.ToString();
		}

		public void Serialize<T>(List<T> list, params string[] ignoreList)
		{
			var properties = new List<PropertyInfo>(typeof(T).GetProperties());

			object[] attrs;
			//properties = properties
			//    .Where(p =>
			//    {
			//        if (ignoreList != null && ignoreList.Contains(p.Name)) return false;

			//        attrs = p.GetCustomAttributes(typeof(ColumnAttribute), false);
			//        if (attrs == null || attrs.Length == 0)
			//            return true;

			//        return (attrs[0] as ColumnAttribute).Include;
			//    })
			//    .OrderBy(
			//    p =>
			//    {
			//        attrs = p.GetCustomAttributes(typeof(ColumnAttribute), false);
			//        if (attrs == null || attrs.Length == 0)
			//            return 1000;

			//        return (attrs[0] as ColumnAttribute).Column;
			//    }
			//    ).ToList();

			this.SetHeadings(properties.Select(p => p.Name).ToArray());

			foreach (var item in list)
				this.AddRow(this.GetRowValues(properties, item));

			this.GenerateSpreadSheet();
		}

		private List<string> GetRowValues(List<PropertyInfo> properties, object item)
		{
			var rowValues = new List<string>();

			foreach (var prop in properties)
				rowValues.Add(string.Empty + prop.GetValue(item, null));

			return rowValues;
		}
	}

	public static class Exten
	{
		public static byte[] SerializeToWorkBookFile<T>(this List<T> list, string sheetName, params string[] ignoreList)
		{
			if (list == null)
				return null;

			var wb = new SpreadSheetExporter(sheetName);
			wb.Serialize<T>(list, ignoreList);

			return wb.GetBytes();
		}
	}
}