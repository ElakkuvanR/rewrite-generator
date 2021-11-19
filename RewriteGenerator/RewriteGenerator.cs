using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RewriteGenerator
{
    public partial class RewriteGenerator : Form
    {
        public string FileName = string.Empty;

        private const string redirect_format = @"<rule name=""{0}"" stopProcessing=""true"">{1}{2}</rule>";
        private const string match_url = @"<match url=""^{0}""/>";
        private const string action = @"<action type=""Redirect"" url=""{0}""/>";
        public RewriteGenerator()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Read the Excel File
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpenExcel_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = "C:\\";
            openFileDialog1.Title = "Browe the Files";
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Filter = "All files (*.*)|*.*|All files (*.*)|*.*";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                FileName = openFileDialog1.FileName;
                txtFileName.Text = FileName;
            }
        }

        /// <summary>
        /// Process the Excel File to Generate the Sitemap
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnGenerateSitemap_Click(object sender, EventArgs e)
        {
            DataTableCollection tableCollection;
            StringBuilder redirects = new StringBuilder();
            using (var stream = File.Open(FileName, FileMode.Open, FileAccess.Read))
            {
                using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream))
                {
                    DataSet result = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = true }
                    });
                    tableCollection = result.Tables;
                    foreach (DataTable table in tableCollection)
                    {
                        DataRowCollection allRows = table.Rows;
                        int count = 1;
                        foreach (DataRow row in allRows)
                        {
                            string originalUrl = string.Format(match_url, GetReletiveUrl(row[1].ToString()) + "$");
                            string redirectUrl = string.Format(action, row[0].ToString());
                            redirects.AppendFormat(redirect_format,
                                txtRuleName.Text + "-" + count, originalUrl, redirectUrl);
                            count++;
                        }
                    }
                }
                stream.Close();
                txtResult.Text = redirects.ToString();
            }
        }

        private string GetReletiveUrl(string fullUrl)
        {
            var url = new Uri(fullUrl);
            return url.PathAndQuery.TrimStart('/');
        }
    }
}
