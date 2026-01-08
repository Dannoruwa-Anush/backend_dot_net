using iTextSharp.text;
using iTextSharp.text.pdf;
using WebApplication1.Models;
using WebApplication1.Services.IService;
using WebApplication1.Utils.Project_Enums;

namespace WebApplication1.Services.ServiceImpl
{
    public class DocumentGenerationServiceImpl : IDocumentGenerationService
    {
        private readonly IWebHostEnvironment _env;

        // Constructor
        public DocumentGenerationServiceImpl(IWebHostEnvironment env)
        {
            // Dependency injection
            _env = env;
        }

        public async Task<string> GenerateInvoicePdfAsync(CustomerOrder order, Invoice invoice)
        {
            string folderPath = Path.Combine(_env.WebRootPath, "invoices");
            Directory.CreateDirectory(folderPath);

            string fileName =
                $"invoice_{invoice.InvoiceID}_{invoice.InvoiceType}_{DateTime.Now:yyyyMMddHHmmss}.pdf";

            string filePath = Path.Combine(folderPath, fileName);

            await Task.Run(() =>
            {
                using FileStream fs = new FileStream(filePath, FileMode.Create);
                using Document doc = new Document(PageSize.A4, 36, 36, 36, 36);

                PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                doc.Open();

                AddWatermark(writer, invoice.InvoiceStatus.ToString());

                AddHeader(doc);
                AddCustomerSection(doc, order);
                AddInvoiceSummary(doc, order, invoice);
                AddInvoiceSpecificSection(doc, order, invoice);

                doc.Close();
            });

            return $"invoices/{fileName}";
        }

        // =========================================================
        // Header
        // =========================================================
        private void AddHeader(Document doc)
        {
            var fontTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            doc.Add(new Paragraph("BNPL Shop Mart", fontTitle)
            {
                Alignment = Element.ALIGN_CENTER
            });

            doc.Add(new Paragraph(
                "BNPL Shop Mart\nNo 123, Colombo\n011 111 2222\n\n",
                fontNormal)
            {
                Alignment = Element.ALIGN_CENTER
            });
        }

        // =========================================================
        // Customer Section
        // =========================================================
        private void AddCustomerSection(Document doc, CustomerOrder order)
        {
            var fontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            var customer = order.Customer
                ?? throw new InvalidOperationException("Customer not loaded");

            PdfPTable table = new PdfPTable(1);
            table.WidthPercentage = 100;
            table.DefaultCell.Border = Rectangle.NO_BORDER;

            table.AddCell(new Phrase("Customer Details", fontBold));
            table.AddCell(new Phrase(customer.CustomerName, fontNormal));
            table.AddCell(new Phrase(customer.Address, fontNormal));
            table.AddCell(new Phrase(customer.PhoneNo, fontNormal));

            doc.Add(table);
            doc.Add(new Paragraph("\n"));
        }

        // =========================================================
        // Invoice Summary
        // =========================================================
        private void AddInvoiceSummary(
            Document doc,
            CustomerOrder order,
            Invoice invoice)
        {
            var fontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            PdfPTable table = new PdfPTable(2);
            table.WidthPercentage = 60;
            table.HorizontalAlignment = Element.ALIGN_LEFT;

            table.AddCell(new Phrase("Invoice No", fontBold));
            table.AddCell(new Phrase(invoice.InvoiceID.ToString(), fontNormal));

            table.AddCell(new Phrase("Order No", fontBold));
            table.AddCell(new Phrase(order.OrderID.ToString(), fontNormal));

            table.AddCell(new Phrase("Invoice Type", fontBold));
            table.AddCell(new Phrase(invoice.InvoiceType.ToString(), fontNormal));

            table.AddCell(new Phrase("Invoice Date", fontBold));
            table.AddCell(new Phrase(DateTime.Now.ToString("yyyy-MM-dd"), fontNormal));

            table.AddCell(new Phrase("Invoice Amount (Rs.)", fontBold));
            table.AddCell(new Phrase($"{invoice.InvoiceAmount:F2}", fontNormal));

            doc.Add(table);
            doc.Add(new Paragraph("\n"));
        }

        // =========================================================
        // Invoice Content By Type
        // =========================================================
        private void AddInvoiceSpecificSection(
            Document doc,
            CustomerOrder order,
            Invoice invoice)
        {
            switch (invoice.InvoiceType)
            {
                case InvoiceTypeEnum.Full_Payment:
                    AddOrderItemsTable(doc, order);
                    break;

                case InvoiceTypeEnum.Bnpl_Initial_Payment:
                    AddBnplInitialDetails(doc, order);
                    break;

                case InvoiceTypeEnum.Bnpl_Installment_Payment:
                    AddSettlementSummary(doc, order);
                    break;
            }
        }

        // =========================================================
        // Order Items Table (Full Payment)
        // =========================================================
        private void AddOrderItemsTable(Document doc, CustomerOrder order)
        {
            var fontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            PdfPTable table = new PdfPTable(5)
            {
                WidthPercentage = 100
            };
            table.SetWidths(new float[] { 8, 40, 15, 10, 20 });

            string[] headers = { "No", "Product", "Unit Price", "Qty", "Sub Total" };
            foreach (var h in headers)
            {
                table.AddCell(new PdfPCell(new Phrase(h, fontBold))
                {
                    BackgroundColor = new BaseColor(230, 230, 230),
                    HorizontalAlignment = Element.ALIGN_CENTER
                });
            }

            int index = 1;
            foreach (var item in order.CustomerOrderElectronicItems)
            {
                table.AddCell(index.ToString());
                table.AddCell(item.ElectronicItem.ElectronicItemName);
                table.AddCell($"{item.UnitPrice:F2}");
                table.AddCell(item.Quantity.ToString());
                table.AddCell($"{item.SubTotal:F2}");
                index++;
            }

            doc.Add(table);

            doc.Add(new Paragraph(
                $"\nTotal Amount: Rs. {order.TotalAmount:F2}",
                fontBold)
            {
                Alignment = Element.ALIGN_RIGHT
            });
        }

        // =========================================================
        // BNPL Initial Payment Section
        // =========================================================
        private void AddBnplInitialDetails(Document doc, CustomerOrder order)
        {
            var fontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            var plan = order.BNPL_PLAN
                ?? throw new InvalidOperationException("BNPL plan not loaded");

            doc.Add(new Paragraph("BNPL Initial Payment Details", fontBold));

            doc.Add(new Paragraph($"Initial Payment: Rs. {plan.Bnpl_InitialPayment:F2}", fontNormal));
            doc.Add(new Paragraph($"Total Installments: {plan.Bnpl_TotalInstallmentCount}", fontNormal));
            doc.Add(new Paragraph($"Installment Amount: Rs. {plan.Bnpl_AmountPerInstallment:F2}", fontNormal));
            doc.Add(new Paragraph($"Total Payable: Rs. {order.TotalAmount:F2}", fontNormal));
        }

        // =========================================================
        // BNPL Installment Settlement Section
        // =========================================================
        private void AddSettlementSummary(Document doc, CustomerOrder order)
        {
            var fontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            var settlement =
                order.BNPL_PLAN?.BNPL_PlanSettlementSummaries
                    .Single(s => s.IsLatest)
                ?? throw new InvalidOperationException("Settlement summary not found");

            doc.Add(new Paragraph("Installment Settlement Summary", fontBold));

            doc.Add(new Paragraph($"Installment No: {settlement.CurrentInstallmentNo}", fontNormal));
            doc.Add(new Paragraph($"Base Arrears: Rs. {settlement.Total_InstallmentBaseArrears:F2}", fontNormal));
            doc.Add(new Paragraph($"Late Interest: Rs. {settlement.Total_LateInterest:F2}", fontNormal));
            doc.Add(new Paragraph($"Total Payable: Rs. {settlement.Total_PayableSettlement:F2}", fontNormal));
        }

        // =========================================================
        // Watermark
        // =========================================================
        private void AddWatermark(PdfWriter writer, string text)
        {
            PdfContentByte canvas = writer.DirectContentUnder;

            BaseFont font = BaseFont.CreateFont(
                BaseFont.HELVETICA_BOLD,
                BaseFont.WINANSI,
                BaseFont.EMBEDDED);

            canvas.SaveState();
            canvas.SetColorFill(new BaseColor(200, 200, 200));
            canvas.SetFontAndSize(font, 60);

            canvas.ShowTextAligned(
                Element.ALIGN_CENTER,
                text.ToUpper(),
                297,
                421,
                45);

            canvas.RestoreState();
        }
    }
}