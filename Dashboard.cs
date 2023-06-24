using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iText.Forms.Form.Element;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Annot;
using iText.Kernel.Pdf.Filespec;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace PDFProject_GUI
{
    public partial class Dashboard : Form
    {
        int totalPdfPage = 0;
        int no = 0;
        DataTable dataTable = new DataTable();
        string folderPath;

        public Dashboard()
        {
            InitializeComponent();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btn_browse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            // Show the folder dialog and check if the user clicked OK
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                // Get the selected folder path
                string folderPath = folderBrowserDialog.SelectedPath;

                // Do something with the selected folder path
                textBox1.Text = folderPath;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            totalPdfPage = 0;
            totalPdfPage = 0;
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                dataTable = new DataTable();

                dataTable.Columns.Add("Nama", typeof(string));
                dataTable.Columns.Add("Tanggal Lahir", typeof(string));
                dataTable.Columns.Add("No MR", typeof(string));
                dataTable.Columns.Add("ID Badge", typeof(string));
                dataTable.Columns.Add("Jumlah Lembar", typeof(int));

                folderPath = textBox1.Text;
                textBox1.Text = "";
                string[] pdfFiles = Directory.GetFiles(folderPath, "*.pdf", SearchOption.AllDirectories);

                foreach(string file in pdfFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    PdfDocument pdfDoc = new PdfDocument(new PdfReader(file));
                    int pageCount = pdfDoc.GetNumberOfPages();
                    totalPdfPage += pageCount;

                    if (fileName.Split("_").Length == 3)
                    {
                        dataTable.Rows.Add(fileName.Split("_")[0], fileName.Split("_")[1], fileName.Split("_")[2], "none", pageCount);
                    }else if(fileName.Split("_").Length == 2){
                        dataTable.Rows.Add(fileName.Split("_")[0], fileName.Split("_")[1], "none", "none", pageCount);
                    }
                    else 
                    {
                        dataTable.Rows.Add(fileName.Split("_")[0], fileName.Split("_")[1], fileName.Split("_")[2], fileName.Split("_")[3], pageCount);
                    }

                }

                dataTable.Rows.Add("", "", "", "Total:", totalPdfPage);
                box_lembar.Text = $"{totalPdfPage}";

                // Set the DataTable as the data source for the DataGridView
                dataGridView1.DataSource = dataTable;

            }
            else
            {
                MessageBox.Show("Silahkan Pilih Lokasi Penyimpanan File PDF");
            }
        }

        private void btn_export_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count == 0)
            { 
                MessageBox.Show("Data yang di export belum ada");
                return;
            }
            if (!string.IsNullOrEmpty(box_filename.Text))
            {
                using (ExcelPackage excelPackage = new ExcelPackage())
                {
                    // Create a new worksheet
                    ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Master Data");

                    // Load data from the DataTable to the worksheet
                    worksheet.Cells["A1"].LoadFromDataTable(dataTable, true);

                    // Mendapatkan kolom terakhir yang berisi data
                    int lastColumn = worksheet.Dimension.End.Column;

                    // Mengubah nilai di kolom terakhir pada baris terakhir
                    int lastRow = worksheet.Dimension.End.Row;
                    int lastRowMinOne = lastRow - 1;
                    worksheet.Cells[lastRow, lastColumn].Formula = "SUM(E2:E" + lastRowMinOne + ")";

                    // Mengatur gaya sel untuk setiap baris menjadi terpusat
                    for (int row = 1; row <= lastRow; row++)
                    {
                        worksheet.Row(row).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }

                    // Make the header row bold
                    ExcelRange headerRow = worksheet.Cells["A1:E1"];
                    headerRow.Style.Font.Bold = true;


                    // Apply borders to cells
                    ExcelRange range = worksheet.Cells[worksheet.Dimension.Address];
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    // Menggabungkan sel-sel kolom
                    worksheet.Cells[lastRow, 1, lastRow, 4].Merge = true;

                    worksheet.Cells["A" + lastRow].Value = "Total:";
                    worksheet.Cells["A" + lastRow].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Row(lastRow).Style.Font.Bold = true;

                    // Auto-fit Column
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    string namaFile = box_filename.Text;

                    // Save the Excel package in the current directory
                    string filePath = Path.Combine(folderPath, namaFile + ".xlsx");
                    excelPackage.SaveAs(new FileInfo(filePath));

                    // Show a prompt box to open or not open the Excel file
                    DialogResult result = MessageBox.Show("Apakah Mau Membuka File Excel?", "Open Excel File", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        // Open the Excel file using the default associated application
                        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                    }
                    else
                    {
                        MessageBox.Show("File Excel disimpan pada: " + folderPath + "\\" + namaFile + ".xlsx");
                    }

                }
            }
            else
            {
                MessageBox.Show("Masukkan Nama File Terlebih Dahulu");
            }
              
        }
    }
}
