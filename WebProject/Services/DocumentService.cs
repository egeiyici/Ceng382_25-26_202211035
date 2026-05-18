using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WebProject.Data;

namespace WebProject.Services
{
    public class DocumentService
    {
        private readonly ApplicationDbContext _context;

        public DocumentService(ApplicationDbContext context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]> GenerateReceiptPdfAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.SelectedOptions)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new Exception("Catering order not found.");
            }

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);

                    page.Header()
                        .Text("Food4Everyone - Catering Order Receipt")
                        .FontSize(22)
                        .Bold();

                    page.Content().Column(column =>
                    {
                        column.Spacing(10);

                        column.Item().Text($"Catering Order ID: {order.Id}");
                        column.Item().Text($"Customer: {order.User?.Email}");
                        column.Item().Text($"Order Date: {order.CreatedAt}");
                        column.Item().Text($"Order Status: {order.Status}");

                        column.Item().LineHorizontal(1);

                        column.Item().Text("Selected Catering Packages")
                            .FontSize(16)
                            .Bold();

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Package").Bold();
                                header.Cell().Text("People").Bold();
                                header.Cell().Text("Qty").Bold();
                                header.Cell().Text("Base Total").Bold();
                                header.Cell().Text("Features").Bold();
                                header.Cell().Text("Line Total").Bold();
                            });

                            foreach (var item in order.OrderItems)
                            {
                                var options = item.SelectedOptions.Any()
                                    ? string.Join(", ", item.SelectedOptions.Select(o => o.OptionName))
                                    : "No additional features";

                                table.Cell().Text(item.MenuItem?.Name ?? "Unknown Package");
                                table.Cell().Text(item.PersonCount.ToString());
                                table.Cell().Text(item.Quantity.ToString());
                                table.Cell().Text($"{item.PackageBaseTotal:0.00} TL");
                                table.Cell().Text(options);
                                table.Cell().Text($"{item.LineTotal:0.00} TL");
                            }
                        });

                        column.Item().LineHorizontal(1);

                        column.Item()
                            .AlignRight()
                            .Text($"Total Catering Order Amount: {order.TotalPrice:0.00} TL")
                            .FontSize(16)
                            .Bold();
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("Thank you for using Food4Everyone Catering Platform.");
                });
            }).GeneratePdf();

            return pdf;
        }

        public async Task<byte[]> GenerateAgreementPdfAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.SelectedOptions)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new Exception("Catering order not found.");
            }

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Size(PageSizes.A4);

                    page.Header()
                        .Text("Food4Everyone - Catering Service Agreement")
                        .FontSize(22)
                        .Bold();

                    page.Content().Column(column =>
                    {
                        column.Spacing(12);

                        column.Item().Text($"Agreement for Catering Order #{order.Id}")
                            .FontSize(16)
                            .Bold();

                        column.Item().Text($"Customer: {order.User?.Email}");
                        column.Item().Text($"Created At: {order.CreatedAt}");
                        column.Item().Text($"Total Amount: {order.TotalPrice:0.00} TL");
                        column.Item().Text($"Order Status: {order.Status}");

                        column.Item().LineHorizontal(1);

                        column.Item().Text("Agreement Terms")
                            .FontSize(16)
                            .Bold();

                        column.Item().Text(
                            "This dynamically generated agreement confirms that the customer has placed a catering package order through Food4Everyone. " +
                            "The selected catering packages, guest count, quantities, package features, customer information, and total amount are generated from the database. " +
                            "This document is created specifically for this order and is not a static file.");

                        column.Item().Text(
                            "The catering company is responsible for preparing or providing the selected catering package according to the selected guest count, package features, and order details. " +
                            "The customer confirms the order by completing the simulated payment flow.");

                        column.Item().LineHorizontal(1);

                        column.Item().Text("Included Catering Packages")
                            .FontSize(16)
                            .Bold();

                        foreach (var item in order.OrderItems)
                        {
                            var options = item.SelectedOptions.Any()
                                ? string.Join(", ", item.SelectedOptions.Select(o => o.OptionName))
                                : "No additional features";

                            column.Item().Text(
                                $"- {item.MenuItem?.Name} | People: {item.PersonCount} | Quantity: {item.Quantity} | Options: {options} | Total: {item.LineTotal:0.00} TL");
                        }

                        column.Item().LineHorizontal(1);

                        column.Item().Text("Customer Confirmation")
                            .FontSize(14)
                            .Bold();

                        column.Item().Text(
                            "By completing the simulated payment process, the customer confirms the catering package order and acknowledges the generated agreement.");
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("Food4Everyone Catering Agreement Document");
                });
            }).GeneratePdf();

            return pdf;
        }
    }
}