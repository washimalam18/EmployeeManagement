using Employee.Models;
using System.IO;
using System.Linq;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace Employee.Services
{
    public class WordService
    {
        public byte[] GenerateOrderWord(Order order)
        {
            if (order == null || order.items == null || !order.items.Any())
                return null;

            using (var memoryStream = new MemoryStream())
            {
                using (var document = DocX.Create(memoryStream))
                {
                    // Title
                    document.InsertParagraph("Order Detail")
                            .FontSize(20)
                            .Bold()
                            .UnderlineStyle(UnderlineStyle.singleLine)
                            .Alignment = Alignment.center;

                    document.InsertParagraph();

                    // Order Info
                    document.InsertParagraph($"Order ID: {order.Id}");
                    document.InsertParagraph($"Client: {order.client?.Name ?? "N/A"}");
                    document.InsertParagraph($"Email: {order.client?.Email ?? "N/A"}");
                    document.InsertParagraph($"Address: {order.Address ?? "N/A"}");

                    document.InsertParagraph();

                    // Item Table
                    var table = document.AddTable(order.items.Count + 1, 5);
                    table.Design = TableDesign.TableGrid;

                    table.Rows[0].Cells[0].Paragraphs[0].Append("S.No.").Bold();
                    table.Rows[0].Cells[1].Paragraphs[0].Append("Product").Bold();
                    table.Rows[0].Cells[2].Paragraphs[0].Append("Rate").Bold();
                    table.Rows[0].Cells[3].Paragraphs[0].Append("Qty").Bold();
                    table.Rows[0].Cells[4].Paragraphs[0].Append("Item Total").Bold();

                    int serial = 1;
                    for (int i = 0; i < order.items.Count; i++)
                    {
                        var item = order.items[i];
                        table.Rows[i + 1].Cells[0].Paragraphs[0].Append(serial++.ToString());
                        table.Rows[i + 1].Cells[1].Paragraphs[0].Append(item.ProductName ?? "N/A");
                        table.Rows[i + 1].Cells[2].Paragraphs[0].Append($"{item.ProductRate:C}");
                        table.Rows[i + 1].Cells[3].Paragraphs[0].Append(item.Quantity.ToString());
                        table.Rows[i + 1].Cells[4].Paragraphs[0].Append($"{item.ItemTotal:C}");
                    }

                    document.InsertTable(table);

                    document.InsertParagraph();

                    // 🔲 Order Summary Box Table
                    var summaryTable = document.AddTable(5, 2);
                    summaryTable.Design = TableDesign.TableGrid;
                    summaryTable.AutoFit = AutoFit.Contents;

                    summaryTable.Rows[0].MergeCells(0, 1);
                    summaryTable.Rows[0].Cells[0].Paragraphs[0]
                        .Append("Order Summary")
                        .FontSize(16)
                        .Bold()
                        .Alignment = Alignment.center;

                    summaryTable.Rows[1].Cells[0].Paragraphs[0].Append("Subtotal:");
                    summaryTable.Rows[1].Cells[1].Paragraphs[0].Append($"{order.OrderTotal:C}").Alignment = Alignment.right;

                    summaryTable.Rows[2].Cells[0].Paragraphs[0].Append("Tax (if any):");
                    summaryTable.Rows[2].Cells[1].Paragraphs[0].Append("00.00").Alignment = Alignment.right;

                    summaryTable.Rows[3].Cells[0].Paragraphs[0].Append("Discount:");
                    summaryTable.Rows[3].Cells[1].Paragraphs[0].Append("00.00").Alignment = Alignment.right;

                    summaryTable.Rows[4].Cells[0].Paragraphs[0].Append("Grand Total:").Bold();
                    summaryTable.Rows[4].Cells[1].Paragraphs[0].Append($"{order.OrderTotal:C}").Bold().Alignment = Alignment.right;

                    document.InsertParagraph();
                    document.InsertTable(summaryTable);

                    document.Save();
                }

                memoryStream.Position = 0;
                return memoryStream.ToArray();
            }
        }
    }
}
