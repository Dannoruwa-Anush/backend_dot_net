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

                // Add watermark first
                AddWatermark(writer, invoice.InvoiceStatus.ToString());

                // Build sections
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

            PdfPTable header = new PdfPTable(2)
            {
                WidthPercentage = 100
            };
            header.SetWidths(new float[] { 60, 40 });

            // Company Info
            PdfPCell companyCell = new PdfPCell
            {
                Border = Rectangle.NO_BORDER
            };
            companyCell.AddElement(new Paragraph("BNPL SHOP MART", titleFont));
            companyCell.AddElement(new Paragraph("No 123, Colombo", normalFont));
            companyCell.AddElement(new Paragraph("Tel: 011 111 2222", normalFont));
            companyCell.AddElement(new Paragraph("Email: info@bnplshopmart.com", normalFont));
            header.AddCell(companyCell);

            // Invoice Title
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

            var customer = order.Customer ?? throw new InvalidOperationException("Customer not loaded");

            PdfPTable customerTable = new PdfPTable(1)
            {
                WidthPercentage = 100
            };

            PdfPCell headerCell = new PdfPCell(new Phrase("Bill To", bold))
            {
                BackgroundColor = new BaseColor(240, 240, 240),
                Padding = 6
            };
            customerTable.AddCell(headerCell);

            customerTable.AddCell(new Phrase(customer.CustomerName, normal));
            customerTable.AddCell(new Phrase(customer.Address, normal));
            customerTable.AddCell(new Phrase("Tel: " + customer.PhoneNo, normal));

            doc.Add(customerTable);
            doc.Add(new Paragraph("\n"));
        }

        // =========================================================
        // Invoice Summary (Meta)
        // =========================================================
        private void AddInvoiceSummary(Document doc, CustomerOrder order, Invoice invoice)
        {
            var bold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            var normal = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            PdfPTable summaryTable = new PdfPTable(2)
            {
                WidthPercentage = 40,
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            summaryTable.SetWidths(new float[] { 50, 50 });

            void AddRow(string label, string value)
            {
                summaryTable.AddCell(new PdfPCell(new Phrase(label, bold)) { Border = Rectangle.NO_BORDER });
                summaryTable.AddCell(new PdfPCell(new Phrase(value, normal)) { Border = Rectangle.NO_BORDER });
            }

            AddRow("Invoice No", invoice.InvoiceID.ToString());
            AddRow("Order No", order.OrderID.ToString());
            AddRow("Invoice Type", invoice.InvoiceType.ToString());
            AddRow("Invoice Date", DateTime.Now.ToString("yyyy-MM-dd"));

            doc.Add(summaryTable);
            doc.Add(new Paragraph("\n"));
        }

        // =========================================================
        // Invoice Content By Type
        // =========================================================
        private void AddInvoiceSpecificSection(Document doc, CustomerOrder order, Invoice invoice)
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
        // Full Payment Table
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

            string[] headers = { "No", "Product", "Unit Price (Rs)", "Qty", "Sub Total (Rs)" };
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
                table.AddCell(new PdfPCell(new Phrase(index.ToString(), fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase(item.ElectronicItem.ElectronicItemName, fontNormal)));
                table.AddCell(new PdfPCell(new Phrase($"{item.UnitPrice:F2}", fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                table.AddCell(new PdfPCell(new Phrase(item.Quantity.ToString(), fontNormal)) { HorizontalAlignment = Element.ALIGN_CENTER });
                table.AddCell(new PdfPCell(new Phrase($"{item.SubTotal:F2}", fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });
                index++;
            }

            doc.Add(table);

            // GRAND TOTAL
            PdfPTable totalTable = new PdfPTable(2)
            {
                WidthPercentage = 30,
                HorizontalAlignment = Element.ALIGN_RIGHT
            };
            totalTable.SetWidths(new float[] { 50, 50 });

            PdfPCell totalLabelCell = new PdfPCell(new Phrase("GRAND TOTAL", fontBold))
            {
                Border = Rectangle.TOP_BORDER,
                Padding = 6
            };
            PdfPCell totalValueCell = new PdfPCell(new Phrase($"Rs. {order.TotalAmount:F2}", fontBold))
            {
                Border = Rectangle.TOP_BORDER,
                Padding = 6,
                HorizontalAlignment = Element.ALIGN_RIGHT
            };

            totalTable.AddCell(totalLabelCell);
            totalTable.AddCell(totalValueCell);

            doc.Add(totalTable);
        }

        // =========================================================
        // BNPL Initial Payment
        // =========================================================
        private void AddBnplInitialDetails(Document doc, CustomerOrder order)
        {
            var fontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            var plan = order.BNPL_PLAN ?? throw new InvalidOperationException("BNPL plan not loaded");

            doc.Add(new Paragraph("BNPL Initial Payment Details", fontBold));
            doc.Add(new Paragraph("\n"));

            PdfPTable table = new PdfPTable(2)
            {
                WidthPercentage = 60
            };
            table.SetWidths(new float[] { 50, 50 });

            void AddRow(string label, string value)
            {
                table.AddCell(new PdfPCell(new Phrase(label, fontBold)) { BackgroundColor = new BaseColor(230, 230, 230) });
                table.AddCell(new PdfPCell(new Phrase(value, fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            }

            AddRow("Plan Name", plan.BNPL_PlanType?.Bnpl_PlanTypeName ?? "N/A");
            AddRow("Initial Payment", $"Rs. {plan.Bnpl_InitialPayment:F2}");
            AddRow("Installment Amount", $"Rs. {plan.Bnpl_AmountPerInstallment:F2}");
            AddRow("Total Installments", plan.Bnpl_TotalInstallmentCount.ToString());
            AddRow("Remaining Installments", plan.Bnpl_RemainingInstallmentCount.ToString());
            AddRow("BNPL Start Date", plan.Bnpl_StartDate?.ToString("yyyy-MM-dd") ?? "-");
            AddRow("Next Due Date", plan.Bnpl_NextDueDate?.ToString("yyyy-MM-dd") ?? "-");

            doc.Add(table);
        }

        // =========================================================
        // BNPL Installment Payment (Settlement Summary)
        // =========================================================
        private void AddSettlementSummary(Document doc, CustomerOrder order)
        {
            var fontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            var settlement =
                order.BNPL_PLAN?.BNPL_PlanSettlementSummaries.SingleOrDefault(s => s.IsLatest)
                ?? throw new InvalidOperationException("Settlement summary not found");

            var plan = order.BNPL_PLAN!;

            doc.Add(new Paragraph("Installment Settlement Summary", fontBold));
            doc.Add(new Paragraph("\n"));

            // BNPL Plan Table
            PdfPTable bnplTable = new PdfPTable(2)
            {
                WidthPercentage = 60
            };
            bnplTable.SetWidths(new float[] { 50, 50 });

            void AddRow(string label, string value)
            {
                bnplTable.AddCell(new PdfPCell(new Phrase(label, fontBold)) { BackgroundColor = new BaseColor(230, 230, 230) });
                bnplTable.AddCell(new PdfPCell(new Phrase(value, fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });
            }

            AddRow("Plan Name", plan.BNPL_PlanType?.Bnpl_PlanTypeName ?? "N/A");
            AddRow("Current Installment", $"{settlement.CurrentInstallmentNo} of {plan.Bnpl_TotalInstallmentCount}");
            AddRow("Installment Amount", $"Rs. {plan.Bnpl_AmountPerInstallment:F2}");
            AddRow("Remaining Installments", plan.Bnpl_RemainingInstallmentCount.ToString());
            AddRow("BNPL Start Date", plan.Bnpl_StartDate?.ToString("yyyy-MM-dd") ?? "-");
            AddRow("Next Due Date", plan.Bnpl_NextDueDate?.ToString("yyyy-MM-dd") ?? "-");

            doc.Add(bnplTable);
            doc.Add(new Paragraph("\n"));

            // Payment Summary Table
            PdfPTable payTable = new PdfPTable(2)
            {
                WidthPercentage = 50,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            payTable.SetWidths(new float[] { 70, 30 });

            payTable.AddCell(new PdfPCell(new Phrase("Description", fontBold)) { BackgroundColor = new BaseColor(230, 230, 230) });
            payTable.AddCell(new PdfPCell(new Phrase("Amount (Rs)", fontBold)) { BackgroundColor = new BaseColor(230, 230, 230), HorizontalAlignment = Element.ALIGN_RIGHT });

            payTable.AddCell("Base Amount Paid");
            payTable.AddCell(new PdfPCell(new Phrase($"{settlement.Paid_AgainstNotYetDueCurrentInstallmentBaseAmount:F2}", fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });

            payTable.AddCell("Arrears Applied");
            payTable.AddCell(new PdfPCell(new Phrase($"{settlement.Paid_AgainstTotalArrears:F2}", fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });

            payTable.AddCell("Late Interest Applied");
            payTable.AddCell(new PdfPCell(new Phrase($"{settlement.Paid_AgainstTotalLateInterest:F2}", fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });

            payTable.AddCell("Overpayment Carried Forward");
            payTable.AddCell(new PdfPCell(new Phrase($"{settlement.Total_OverpaymentCarriedToNext:F2}", fontNormal)) { HorizontalAlignment = Element.ALIGN_RIGHT });

            doc.Add(new Paragraph("Payment Summary (This Invoice)", fontBold));
            doc.Add(payTable);
            doc.Add(new Paragraph($"\nPayment Status: {settlement.Bnpl_PlanSettlementSummary_Status}", fontNormal));
        }

        // =========================================================
        // Watermark
        // =========================================================
        private void AddWatermark(PdfWriter writer, string text)
        {
            PdfContentByte canvas = writer.DirectContentUnder;
            BaseFont font = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.WINANSI, BaseFont.EMBEDDED);

            canvas.SaveState();
            canvas.SetColorFill(new BaseColor(200, 200, 200));
            canvas.SetFontAndSize(font, 60);
            canvas.ShowTextAligned(Element.ALIGN_CENTER, text.ToUpper(), 297, 421, 45);
            canvas.RestoreState();
        }
    }
}