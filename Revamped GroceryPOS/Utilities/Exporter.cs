using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System.Diagnostics;
using Revamped_GroceryPOS.Components;

namespace Revamped_GroceryPOS.Utilities
{
    class Exporter
    {
        public static void ExportSalesReportToPDF(
        DateOnly fromDate,
        DateOnly toDate,
        List<Transaction> transactions,
        int totalTransactions,
        double totalIncome,
        int totalItems,
        string filePath)
        {
            Document doc = new Document();
            Section section = doc.AddSection();

            // Title
            Paragraph title = section.AddParagraph("Sales Report");
            title.Format.Font.Size = 18;
            title.Format.Font.Bold = true;
            title.Format.SpaceAfter = "0.5cm";

            // Date range
            Paragraph dateRange = section.AddParagraph($"Date Range: {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}");
            dateRange.Format.Font.Size = 12;
            dateRange.Format.SpaceAfter = "0.5cm";

            // Summary
            Paragraph summary = section.AddParagraph(
                $"Total Transactions: {totalTransactions}\n" +
                $"Total Items Sold: {totalItems}\n" +
                $"Total Income: ₱{totalIncome:N2}");
            summary.Format.Font.Size = 11;
            summary.Format.SpaceAfter = "1cm";

            // Table setup
            var table = section.AddTable();
            table.Borders.Width = 0.75;
            table.AddColumn(Unit.FromCentimeter(3)); // ID
            table.AddColumn(Unit.FromCentimeter(5)); // Date/Time
            table.AddColumn(Unit.FromCentimeter(4)); // Total Amount

            // Header row
            var headerRow = table.AddRow();
            headerRow.Shading.Color = Colors.LightGray;
            headerRow.Cells[0].AddParagraph("Transaction ID");
            headerRow.Cells[1].AddParagraph("Date/Time");
            headerRow.Cells[2].AddParagraph("Total");

            // Add transaction rows
            foreach (var t in transactions)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph(t.TransactionID.ToString());
                row.Cells[1].AddParagraph(t.TransactionDateTime.ToString("yyyy-MM-dd HH:mm"));
                row.Cells[2].AddParagraph($"₱{t.TotalAmount:F2}");
            }

            // Render and save
            PdfDocumentRenderer renderer = new PdfDocumentRenderer(true);
            renderer.Document = doc;
            renderer.RenderDocument();
            renderer.PdfDocument.Save(filePath);

            // Optional: auto-open
            Process.Start("explorer.exe", filePath);
        }
    }
}
