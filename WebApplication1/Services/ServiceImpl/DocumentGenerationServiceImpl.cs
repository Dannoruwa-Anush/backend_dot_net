using iTextSharp.text;
using iTextSharp.text.pdf;
using WebApplication1.DTOs.ResponseDto;
using WebApplication1.Services.IService;

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

        public async Task<string> GenerateInvoicePdfAsync(CustomerOrderResponseDto order, InvoiceResponseDto invoice)
        {
            string folderPath = Path.Combine(_env.WebRootPath, "invoices");
            Directory.CreateDirectory(folderPath);

            string fileName = $"invoice_{order.OrderID}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            string filePath = Path.Combine(folderPath, fileName);

            await Task.Run(() =>
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    Document doc = new Document(PageSize.A4, 36, 36, 36, 36);
                    PdfWriter writer = PdfWriter.GetInstance(doc, fs);
                    doc.Open();

                    AddWatermark(writer, invoice.InvoiceStatus.ToString());

                    var fontTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                    var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                    var fontBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

                    var title = new Paragraph("BNPL Shop Mart", fontTitle)
                    {
                        Alignment = Element.ALIGN_CENTER
                    };
                    doc.Add(title);
                    doc.Add(new Paragraph("BNPL Shop Mart, No 123, Colombo\n011 111 2222\n\n", fontNormal)
                    {
                        Alignment = Element.ALIGN_CENTER
                    });

                    doc.Add(new Paragraph($"Invoice No: #{order.OrderID}\nDate: {DateTime.Now:yyyy-MM-dd}\n\n", fontNormal));

                    var customer = order.CustomerResponseDto;
                    PdfPTable customerTable = new PdfPTable(1);
                    customerTable.DefaultCell.Border = Rectangle.NO_BORDER;
                    customerTable.AddCell(new Phrase("Customer", fontBold));
                    customerTable.AddCell(new Phrase(customer!.CustomerName, fontNormal));
                    customerTable.AddCell(new Phrase(customer.Address, fontNormal));
                    customerTable.AddCell(new Phrase(customer.PhoneNo, fontNormal));
                    doc.Add(customerTable);

                    doc.Add(new Paragraph("\n"));

                    PdfPTable table = new PdfPTable(5);
                    table.WidthPercentage = 100;
                    table.SetWidths(new float[] { 8, 40, 15, 10, 20 });

                    string[] headers = { "No", "Product Name", "Unit Price", "Qty", "Sub Total" };
                    foreach (var h in headers)
                    {
                        var cell = new PdfPCell(new Phrase(h, fontBold))
                        {
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            BackgroundColor = new BaseColor(230, 230, 230)
                        };
                        table.AddCell(cell);
                    }

                    int index = 1;
                    foreach (var item in order.CustomerOrderElectronicItemResponseDto)
                    {
                        table.AddCell(new Phrase(index.ToString(), fontNormal));
                        table.AddCell(new Phrase(item.ElectronicItemResponseDto.ElectronicItemName, fontNormal));
                        table.AddCell(new Phrase($"{item.UnitPrice:F2}", fontNormal));
                        table.AddCell(new Phrase(item.Quantity.ToString(), fontNormal));
                        table.AddCell(new Phrase($"{item.SubTotal:F2}", fontNormal));
                        index++;
                    }

                    doc.Add(table);

                    var total = new Paragraph($"\nTotal: (Rs.) {order.TotalAmount:F2}", fontBold)
                    {
                        Alignment = Element.ALIGN_RIGHT
                    };
                    doc.Add(total);

                    doc.Close();
                }
            });

            return $"invoices/{fileName}";
        }

        // Helper Method : Add Watermark
        private void AddWatermark(PdfWriter writer, string text)
        {
            PdfContentByte canvas =
                writer.DirectContentUnder;

            BaseFont font =
                BaseFont.CreateFont(
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