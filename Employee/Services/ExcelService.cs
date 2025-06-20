using ClosedXML.Excel;
using Employee.Models;
using System.IO;

public class ExcelService
{
    public byte[] GenerateOrderExcel(Order order)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Order Details");

        int row = 1;

        // Header
        worksheet.Cell(row++, 1).Value = "Order Detail";
        worksheet.Range("A1:D1").Merge().Style.Font.SetBold().Font.FontSize = 16;

        row += 1;
        worksheet.Cell(row++, 1).Value = $"Order ID: {order.Id}";
        worksheet.Cell(row++, 1).Value = $"Client: {order.client?.Name ?? "N/A"}";
        worksheet.Cell(row++, 1).Value = $"Email: {order.client?.Email ?? "N/A"}";
        worksheet.Cell(row++, 1).Value = $"Address: {order.Address ?? "N/A"}";

        row += 2;

        // Table Headers
        //worksheet.Cell(row, 1).Value = "S.No.";
        worksheet.Cell(row, 1).Value = "Product";
        worksheet.Cell(row, 2).Value = "Rate";
        worksheet.Cell(row, 3).Value = "Qty";
        worksheet.Cell(row, 4).Value = "Item Total";
        worksheet.Range(row, 1, row, 4).Style.Font.SetBold();
        worksheet.Range(row, 1, row, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        worksheet.Range(row, 1, row, 4).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
        row++;

        int serial = 1;
        foreach (var item in order.items)
        {
            //worksheet.Cell(row, 1).Value = serial++;
            worksheet.Cell(row, 1).Value = item.ProductName ?? "N/A";
            worksheet.Cell(row, 2).Value = item.ProductRate;
            worksheet.Cell(row, 3).Value = item.Quantity;
            worksheet.Cell(row, 4).Value = item.ItemTotal;

            worksheet.Range(row, 1, row, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            worksheet.Range(row, 1, row, 4).Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            row++;
        }

        row += 2;

        worksheet.Cell(row++, 3).Value = "Subtotal:";
        worksheet.Cell(row - 1, 4).Value = order.OrderTotal;

        worksheet.Cell(row++, 3).Value = "Tax (if any):";
        worksheet.Cell(row - 1, 4).Value = 0.0;

        worksheet.Cell(row++, 3).Value = "Discount:";
        worksheet.Cell(row - 1, 4).Value = 0.0;

        worksheet.Cell(row++, 3).Value = "Grand Total:";
        worksheet.Cell(row - 1, 4).Value = order.OrderTotal;
        worksheet.Cell(row - 1, 4).Style.Font.SetBold();

        // Auto-fit
        worksheet.Columns().AdjustToContents();

        // Save to memory stream
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
