using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Employee.Models;

public class PdfService
{
    public byte[] GenerateOrderPdf(Order order)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);

                page.Content().Column(col =>
                {
                    col.Item().Text("Order Detail").FontSize(20).Bold().AlignCenter().Underline();

                    col.Item().PaddingTop(20);
                    col.Item().Text($"Order ID  :  {order.Id}");
                    col.Item().Text($"Client  :  {order.client?.Name ?? "N/A"}");
                    col.Item().Text($"Email  :  {order.client?.Email ?? "N/A"}");
                    col.Item().Text($"Addres  :  {order.Address ?? "N/A"}");

                    // Table with Borders
                    col.Item().PaddingTop(30).Table(table =>
                    {
                        
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(40);   // S.No.
                            columns.RelativeColumn(3);    // Product
                            columns.ConstantColumn(80);   // Rate
                            columns.ConstantColumn(80);   // Qty
                            columns.ConstantColumn(100);  // Item Total
                        });

                        // Header with Borders
                        table.Header(header =>
                        {
                            
                            header.Cell().Element(CellStyle).Text("S.No.").Bold();
                            header.Cell().Element(CellStyle).Text("Product").Bold();
                            header.Cell().Element(CellStyle).Text("Rate").Bold();
                            header.Cell().Element(CellStyle).Text("Qty").Bold();
                            header.Cell().Element(CellStyle).Text("Item Total").Bold();
                        });

                        int serialNumber = 1;
                        foreach (var item in order.items)
                        {
                            table.Cell().Element(CellStyle).Text(serialNumber++.ToString());
                            table.Cell().Element(CellStyle).Text(item.ProductName ?? "N/A");
                            table.Cell().Element(CellStyle).Text($"{item.ProductRate:C}");
                            table.Cell().Element(CellStyle).Text(item.Quantity.ToString());
                            table.Cell().Element(CellStyle).Text($"{item.ItemTotal:C}");
                        }

                        // Local method for consistent styling
                        IContainer CellStyle(IContainer container)
                        {
                            return container
                                .Border(1)
                                .BorderColor(Colors.Grey.Medium)
                                .Padding(5);
                        }
                    });

                    // Order Summary Section as a Table
                    col.Item().PaddingTop(50).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();       // Title
                            columns.ConstantColumn(100);    // Value aligned right
                        });

                        // Header Row: Merge both columns
                        table.Cell().ColumnSpan(2).Element(CellStyle).AlignCenter().Text("Order Summary")
                             .FontSize(16).Bold();

                        // Subtotal
                        table.Cell().Element(CellStyle).Text("Subtotal:");
                        table.Cell().Element(CellStyle).AlignRight().Text($"{order.OrderTotal:C}");

                        // Tax
                        table.Cell().Element(CellStyle).Text("Tax (if any):");
                        table.Cell().Element(CellStyle).AlignRight().Text("00.00");

                        // Discount
                        table.Cell().Element(CellStyle).Text("Discount:");
                        table.Cell().Element(CellStyle).AlignRight().Text("00.00");

                        // Grand Total (Bold & Larger)
                        table.Cell().Element(CellStyle).Text("Grand Total:")
                             .FontSize(14).Bold();
                        table.Cell().Element(CellStyle).AlignRight().Text($"{order.OrderTotal:C}")
                             .FontSize(14).Bold();

                        // Reusable styling method
                        IContainer CellStyle(IContainer container)
                        {
                            return container
                                .Border(1)
                                .BorderColor(Colors.Grey.Medium)
                                .PaddingVertical(5)
                                .PaddingHorizontal(10);
                        }
                    });
                });
            });
        }).GeneratePdf();
    }
}
