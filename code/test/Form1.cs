using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;

using ExcelDataReader;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.CodeDom.Compiler;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Font = iTextSharp.text.Font;

namespace Dart
{
    public partial class Form1 : Form
    {
        System.Threading.Thread thread1, thread2;
        System.Diagnostics.Process p1, p2;
        string filePath;
        string py_args;
        private DateTime tick1, tick2;

        private string dbPath = Application.StartupPath;
        private int timer_interval = 1000; // 每秒檢查一次資料庫
        private double interval = 100;
        public Form1()
        {
            InitializeComponent();

            // 打開程式後，跳出Form2，確認帳號密碼
            Form2 form2;
            form2 = new Form2();
            form2.ShowDialog();
            
            label5.Text = form2.username;
            refreshDart();     
         }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (textBox1.Text != "")
            {
                filePath = "D:\\專題\\程式\\test\\py\\dart.py";
                py_args = textBox1.Text.Replace(" ",",");
                RunPythonScript(filePath, py_args);
            }
            else
            {
                richTextBox1.Text = "";
            }
        }

        public void RunPythonScript(string script_path, string script_args)
        {

            p1 = new System.Diagnostics.Process();

            //沒有配環境變數的話，需要指定python.exe的絕對路徑
            p1.StartInfo.FileName = "C:\\Users\\andy9\\AppData\\Local\\Programs\\Python\\Python38\\python.exe";
            string sArguments = script_path + " " + script_args;

            sArguments += " ";
            p1.StartInfo.Arguments = sArguments;
            p1.StartInfo.UseShellExecute = false;
            p1.StartInfo.RedirectStandardOutput = true;
            p1.StartInfo.RedirectStandardInput = true;
            p1.StartInfo.RedirectStandardError = true;
            p1.StartInfo.CreateNoWindow = true;

            p1.OutputDataReceived += new System.Diagnostics.DataReceivedEventHandler(OutputHandler);

            p1.Start();

            // Asynchronously read the standard output of the spawned process. 
            // This raises OutputDataReceived events for each line of output.
            p1.BeginOutputReadLine();

            p1.WaitForExit();


        }

        private void OutputHandler(object sendingProcess, System.Diagnostics.DataReceivedEventArgs outLine)
        {
            if (outLine.Data != null)
            {
                BeginInvoke(new MethodInvoker(() => {
                    string temp = outLine.Data;
                    string[] opt = temp.Split('\t');
                    this.ShowLog(opt);
                }));
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            filePath = "D:\\專題\\程式\\test\\py\\toexcel.py";
            string temp=richTextBox1.Text;            
            py_args = textBox1.Text + '@' + temp + '@' + textBox2.Text + '@' + label5.Text;
            
            RunPythonScript(filePath, py_args);

            MessageBox.Show("已匯出");
            richTextBox1.Text = "";
            refreshDart();
        }

        DataTableCollection tableCollection;

        private void button1_Click(object sender, EventArgs e)
        {
            string sql = "";
            if (textBox3.Text != "")
            {
                sql += "hbnumber like '%" + textBox3.Text + "%'";
            }
            if (textBox4.Text != "")
            {
                if (sql != "") sql += " And ";
                sql += "editor like '%" + textBox4.Text + "%'";
            }
            //if (sql != "") sql += " And ";
            //sql += "where date >='" + dateTimePicker1.Text + "' And date<='" + dateTimePicker2.Text + "'";
            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = String.Format(sql);
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            refreshDart();
            textBox3.Text = "";
            textBox4.Text = "";
            dateTimePicker1.Value = DateTime.Today;
            dateTimePicker2.Value = DateTime.Today;
        }

        private void 登出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            refreshDart();
            textBox3.Text = "";
            textBox4.Text = "";
        }

        private void 製作者ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form3 form3 = new Form3();
            form3.Show();
        }

        private void 護理紀錄總表預覽ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(TabPage1);
        }

        private void 匯出護理紀錄ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tabControl1.SelectTab(TabPage2);
        }

        private void 使用方式ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form5 form5 = new Form5();
            form5.Show();
        }



        private void refreshDart()
        {
            filePath = "D:\\專題\\程式\\test\\data\\Dart.xlsx";
            var stream = System.IO.File.Open(filePath,
                                            System.IO.FileMode.Open,
                                            System.IO.FileAccess.Read);
            ExcelDataReader.IExcelDataReader reader =
              ExcelDataReader.ExcelReaderFactory.CreateReader(stream);

            var conf = new ExcelDataSetConfiguration
            {
                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                {
                    UseHeaderRow = true
                }
            };

            DataSet result = reader.AsDataSet(conf);
            tableCollection = result.Tables;
            DataTable dt = tableCollection["Dart01"];
            dataGridView1.DataSource = dt;
            dataGridView2.DataSource = dt;
            reader.Dispose();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            refreshDart();
        }
        
        private void ShowLog(string[] i_input)
        {
            string temp= i_input[0].Replace(" ", "");
            temp =temp.Replace("[", "");
            temp = temp.Replace("]", "");
            temp = temp.Replace("'", "");
            temp = temp.Replace(@"\n", "");
            richTextBox1.Text = temp;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ExportTOPdf(dataGridView2);
        }


        public static void ExportTOPdf(DataGridView datagridview)
        {

            ///设置导出字体
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            string FontPath = path + "\\MSJH.ttc";
            int FontSize = 12;
            if (File.Exists(FontPath))
            {
                FontPath += ",1";
            }
            else
            {
                MessageBox.Show("无法导出，因为无法取得中文宋体字型。");
                return;
            }


            Boolean cc = false;
            string strFileName;
            SaveFileDialog savFile = new SaveFileDialog();
            savFile.Filter = "PDF文件|.pdf";
            savFile.ShowDialog();
            if (savFile.FileName != "")
            {
                strFileName = savFile.FileName;
            }
            else
            {
                //MessageBox.Show("终止导出", "终止导出", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            //strFileName = "D:\\專題\\程式\\test\\pdf\\templete.pdf";
            //初始化一个目标文档类       
            //Document document = new Document();
            //竖排模式,大小为A4，四周边距均为25
            //Document document = new Document(PageSize.A4, 25, 25, 25, 25);
            //横排模式,大小为A4，四周边距均为25
            Document document = new Document(PageSize.A4.Rotate(), 25, 25, 25, 25);

            //调用PDF的写入方法流
            //注意FileMode-Create表示如果目标文件不存在，则创建，如果已存在，则覆盖。
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(strFileName, FileMode.Create));

            //创建PDF文档中的字体
            BaseFont baseFont = BaseFont.CreateFont(
                FontPath,
                BaseFont.IDENTITY_H,
                BaseFont.NOT_EMBEDDED);

            //根据字体路径和字体大小属性创建字体
            Font font = new Font(baseFont, FontSize);

            // 添加页脚
            // HeaderFooter footer = new HeaderFooter(new Phrase("-- ", font), new Phrase(" --", font));
            //footer.Border = Rectangle.NO_BORDER;        // 不显示两条横线
            //footer.Alignment = Rectangle.UNDEFINED;  // 让页码居中
            //document.Footer = footer;

            //打开目标文档对象
            document.Open();

            Phrase simplePhr1 = new Phrase("簡單3句話，這是第1句。。。", font);
            Paragraph simplePara = new Paragraph();
            simplePara.Add(simplePhr1);
            document.Add(simplePara);



            int ColCount = 0;

            //根据数据表内容创建一个PDF格式的表
            for (int j = 0; j < datagridview.Columns.Count; j++)
            {
                if (datagridview.Columns[j].Visible == true)
                {
                    ColCount++;
                }
            }
            PdfPTable table = new PdfPTable(ColCount);

            // GridView的所有数据全输出
            //datagridview.AllowPaging = false;

            // ---------------------------------------------------------------------------
            // 添加表头
            // ---------------------------------------------------------------------------
            // 设置表头背景色
            //table.DefaultCell.BackgroundColor = Color.GRAY;  // OK
            //table.DefaultCell.BackgroundColor = (iTextSharp.text.Color)System.Drawing.Color.FromName("#3399FF"); // NG
            //table.DefaultCell.BackgroundColor = iTextSharp.text.Color;

            table.DefaultCell.BackgroundColor = BaseColor.LightGray;
            // 添加表头，每一页都有表头
            for (int j = 0; j < datagridview.Columns.Count; j++)
            {
                if (datagridview.Columns[j].Visible == true)
                {
                    table.AddCell(new Phrase(datagridview.Columns[j].HeaderText, font));
                }
            }

            // 告诉程序这行是表头，这样页数大于1时程序会自动为你加上表头。
            table.HeaderRows = 1;
            //
            // ---------------------------------------------------------------------------
            // 添加数据
            // ---------------------------------------------------------------------------
            // 设置表体背景色
            table.DefaultCell.BackgroundColor = BaseColor.White;
            //遍历原gridview的数据行
            //写内容  
            for (int j = 0; j < datagridview.Rows.Count; j++)
            {

                for (int k = 0; k < datagridview.Columns.Count; k++)
                {
                    if (datagridview.Rows[j].Cells[k].Visible == true)
                    {
                        try
                        {
                            string value = "";
                            if (datagridview.Rows[j].Cells[k].Value != null)
                            {
                                value = datagridview.Rows[j].Cells[k].Value.ToString();
                            }
                            table.AddCell(new Phrase(value, font));
                        }
                        catch (Exception e)
                        {

                            //MessageBox.Show(e.Message);
                            cc = true;
                        }

                    }
                }

            }
            
            //在目标文档中添加转化后的表数据
            document.Add(table);
            
            //关闭目标文件
            document.Close();

            //关闭写入流
            writer.Close();

            // Dialog
            if (!cc)
            {
                MessageBox.Show("已生成PDF文件。", "生成成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
