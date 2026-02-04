using System.Text.Json;
using iTextSharp.text;
using iTextSharp.text.pdf;
using WebApplication1.DTOs.ResponseDto.BnplSnapshotPayingSimulation;
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

        // Invoice PDF generator
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

                AddWatermark(writer, "INVOICE: " + invoice.InvoiceStatus.ToString());

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
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            PdfPTable header = new PdfPTable(2) { WidthPercentage = 100 };
            header.SetWidths(new float[] { 60, 40 });

            PdfPCell companyCell = new PdfPCell { Border = Rectangle.NO_BORDER };
            companyCell.AddElement(new Paragraph("BNPL SHOP MART", titleFont));
            companyCell.AddElement(new Paragraph("No 123, Colombo", normalFont));
            companyCell.AddElement(new Paragraph("Tel: 011 111 2222", normalFont));
            companyCell.AddElement(new Paragraph("Email: info@bnplshopmart.com", normalFont));
            header.AddCell(companyCell);

            PdfPCell invoiceCell = new PdfPCell
            {
                Border = Rectangle.NO_BORDER,
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            invoiceCell.AddElement(new Paragraph("INVOICE", titleFont));
            header.AddCell(invoiceCell);

            doc.Add(header);
            doc.Add(new Paragraph("\n"));
        }

        // =========================================================
        // Customer Section
        // =========================================================
        private void AddCustomerSection(Document doc, CustomerOrder order)
        {
            var bold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            var normal = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            var customer = order.Customer
                ?? throw new InvalidOperationException("Customer not loaded");

            PdfPTable table = new PdfPTable(1) { WidthPercentage = 100 };

            table.AddCell(new PdfPCell(new Phrase("Bill To", bold))
            {
                BackgroundColor = new BaseColor(240, 240, 240),
                Padding = 6
            });

            table.AddCell(new Phrase(customer.CustomerName, normal));
            table.AddCell(new Phrase(customer.Address, normal));
            table.AddCell(new Phrase($"Tel: {customer.PhoneNo}", normal));

            doc.Add(table);
            doc.Add(new Paragraph("\n"));
        }

        // =========================================================
        // Invoice Summary
        // =========================================================
        private void AddInvoiceSummary(Document doc, CustomerOrder order, Invoice invoice)
        {
            var bold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            var normal = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            PdfPTable table = new PdfPTable(2)
            {
                WidthPercentage = 40,
                HorizontalAlignment = Element.ALIGN_RIGHT
            };

            void Row(string label, string value)
            {
                table.AddCell(new PdfPCell(new Phrase(label, bold)) { Border = Rectangle.NO_BORDER });
                table.AddCell(new PdfPCell(new Phrase(value, normal)) { Border = Rectangle.NO_BORDER });
            }

            Row("Invoice No", invoice.InvoiceID.ToString());
            Row("Order No", order.OrderID.ToString());
            Row("Invoice Type", invoice.InvoiceType.ToString());
            Row("Invoice Date", DateTime.Now.ToString("yyyy-MM-dd"));

            doc.Add(table);
            doc.Add(new Paragraph("\n"));
        }

        // =========================================================
        // Invoice Content Switch
        // =========================================================
        private void AddInvoiceSpecificSection(Document doc, CustomerOrder order, Invoice invoice)
        {
            switch (invoice.InvoiceType)
            {
                case InvoiceTypeEnum.Full_Pay:
                    AddOrderItemsTable(doc, order);
                    break;

                case InvoiceTypeEnum.Bnpl_Initial_Pay:
                    AddBnplInitialDetails(doc, order);
                    break;

                case InvoiceTypeEnum.Bnpl_Installment_Pay:
                    AddSettlementSummary(doc, invoice);
                    break;
            }
        }

        // =========================================================
        // Full Payment
        // =========================================================
        private void AddOrderItemsTable(Document doc, CustomerOrder order)
        {
            var bold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            var normal = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            PdfPTable table = new PdfPTable(5) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 8, 40, 15, 10, 20 });

            string[] headers = { "No", "Product", "Unit Price", "Qty", "Sub Total" };
            foreach (var h in headers)
            {
                table.AddCell(new PdfPCell(new Phrase(h, bold))
                {
                    BackgroundColor = new BaseColor(230, 230, 230),
                    HorizontalAlignment = Element.ALIGN_CENTER
                });
            }

            int i = 1;
            foreach (var item in order.CustomerOrderElectronicItems)
            {
                table.AddCell(i++.ToString());
                table.AddCell(item.ElectronicItem.ElectronicItemName);
                table.AddCell(item.UnitPrice.ToString("F2"));
                table.AddCell(item.Quantity.ToString());
                table.AddCell(item.SubTotal.ToString("F2"));
            }

            doc.Add(table);
        }

        // =========================================================
        // BNPL Initial
        // =========================================================
        private void AddBnplInitialDetails(Document doc, CustomerOrder order)
        {
            var bold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            var normal = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            var plan = order.BNPL_PLAN
                ?? throw new InvalidOperationException("BNPL plan not loaded");

            PdfPTable table = new PdfPTable(2) { WidthPercentage = 60 };

            void Row(string l, string v)
            {
                table.AddCell(new PdfPCell(new Phrase(l, bold)) { BackgroundColor = new BaseColor(230, 230, 230) });
                table.AddCell(new PdfPCell(new Phrase(v, normal)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            }

            Row("Plan Name", plan.BNPL_PlanType?.Bnpl_PlanTypeName ?? "N/A");
            Row("Initial Payment", plan.Bnpl_InitialPayment.ToString("F2"));
            Row("Installment Amount", plan.Bnpl_AmountPerInstallment.ToString("F2"));

            doc.Add(table);
        }

        // =========================================================
        // BNPL INSTALLMENT (FROZEN SNAPSHOT)
        // =========================================================
        private void AddSettlementSummary(Document doc, Invoice invoice)
        {
            var snapshot = JsonSerializer.Deserialize<BnplLatestSnapshotSettledResultDto>(
                invoice.SettlementSnapshotJson!, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException("Settlement snapshot missing");

            PdfPTable table = new PdfPTable(2) { WidthPercentage = 60 };

            void Row(string label, decimal value)
            {
                table.AddCell(new PdfPCell(new Phrase(label)) { BackgroundColor = new BaseColor(230, 230, 230) });
                table.AddCell(new PdfPCell(new Phrase(value.ToString("F2")))
                {
                    HorizontalAlignment = Element.ALIGN_RIGHT
                });
            }

            Row("Base Paid", snapshot.TotalPaidCurrentInstallmentBase);
            Row("Arrears Paid", snapshot.TotalPaidArrears);
            Row("Interest Paid", snapshot.TotalPaidLateInterest);
            Row("Overpayment", snapshot.OverPaymentCarriedToNextInstallment);

            doc.Add(table);
        }

        // =========================================================
        // Watermark
        // =========================================================
        private void AddWatermark(PdfWriter writer, string text)
        {
            PdfContentByte canvas = writer.DirectContentUnder;
            BaseFont font = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.WINANSI, BaseFont.EMBEDDED);

            canvas.SaveState();
            canvas.SetFontAndSize(font, 60);
            canvas.SetColorFill(new BaseColor(200, 200, 200));
            canvas.ShowTextAligned(Element.ALIGN_CENTER, text.ToUpper(), 297, 421, 45);
            canvas.RestoreState();
        }

        // Payment Receipt PDF generator
        public async Task<string> GeneratePaymentReceiptPdfAsync(CustomerOrder order, Cashflow cashflow)
        {
            if (cashflow.CashflowPaymentNature != CashflowPaymentNatureEnum.Payment)
                throw new InvalidOperationException("Cashflow is not a payment");

            string folderPath = Path.Combine(_env.WebRootPath, "receipts/payments");
            Directory.CreateDirectory(folderPath);

            string fileName = $"receipt_pay_{cashflow.CashflowID}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            string filePath = Path.Combine(folderPath, fileName);

            await Task.Run(() =>
            {
                using FileStream fs = new FileStream(filePath, FileMode.Create);
                using Document doc = new Document(PageSize.A4, 36, 36, 36, 36);

                PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                doc.Open();

                AddWatermark(writer, "PAYMENT RECEIPT");

                AddReceiptHeader(doc, "PAYMENT RECEIPT");
                AddCustomerSection(doc, order);
                AddReceiptSummary(doc, order, cashflow, isRefund: false);
                
                doc.Add(new Paragraph("\n"));
                AddOrderItemsTable(doc, order);
                doc.Add(new Paragraph("\n"));
                
                AddPaymentDetails(doc, cashflow);

                doc.Close();
            });

            return $"receipts/payments/{fileName}";
        }

        private void AddReceiptHeader(Document doc, string titleText)
        {
            var title = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            doc.Add(new Paragraph(titleText, title));
            doc.Add(new Paragraph("\n"));
        }

        private void AddReceiptSummary(Document doc, CustomerOrder order, Cashflow cashflow, bool isRefund)
        {
            var bold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            var normal = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            PdfPTable table = new PdfPTable(2) { WidthPercentage = 50 };

            void Row(string label, string value)
            {
                table.AddCell(new PdfPCell(new Phrase(label, bold)) { Border = Rectangle.NO_BORDER });
                table.AddCell(new PdfPCell(new Phrase(value, normal)) { Border = Rectangle.NO_BORDER });
            }

            Row("Receipt No", $"R-{cashflow.CashflowID}");
            Row("Invoice No", cashflow.Invoice?.InvoiceID.ToString() ?? "N/A");
            Row("Order No", order.OrderID.ToString());
            Row(isRefund ? "Refund Amount" : "Paid Amount", cashflow.AmountPaid.ToString("F2"));
            Row("Currency", "LKR");
            Row(isRefund ? "Refund Date" : "Payment Date", cashflow.CreatedAt.ToString("yyyy-MM-dd HH:mm") ?? "-");
            Row("Status", isRefund ? "REFUNDED" : "PAID");

            doc.Add(table);
        }

        private void AddPaymentDetails(Document doc, Cashflow cashflow)
        {
            doc.Add(new Paragraph($"Payment Amount: {cashflow.AmountPaid:F2}"));
            doc.Add(new Paragraph($"Transaction Ref: {cashflow.CashflowRef}"));
        }

        // Refund Receipt PDF generator
        public async Task<string> GenerateRefundReceiptPdfAsync(CustomerOrder order, Cashflow cashflow)
        {
            if (cashflow.CashflowPaymentNature != CashflowPaymentNatureEnum.Refund)
                throw new InvalidOperationException("Cashflow is not a refund");

            string folderPath = Path.Combine(_env.WebRootPath, "receipts/refunds");
            Directory.CreateDirectory(folderPath);

            string fileName = $"receipt_ref_{cashflow.CashflowID}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            string filePath = Path.Combine(folderPath, fileName);

            await Task.Run(() =>
            {
                using FileStream fs = new FileStream(filePath, FileMode.Create);
                using Document doc = new Document(PageSize.A4, 36, 36, 36, 36);

                PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                doc.Open();

                AddWatermark(writer, "REFUND RECEIPT");

                AddReceiptHeader(doc, "REFUND RECEIPT");
                AddCustomerSection(doc, order);
                AddReceiptSummary(doc, order, cashflow, isRefund: true);

                doc.Add(new Paragraph("\n"));
                AddOrderItemsTable(doc, order);
                doc.Add(new Paragraph("\n"));

                AddPaymentDetails(doc, cashflow);

                doc.Close();
            });

            return $"receipts/refunds/{fileName}";
        }
    }
}