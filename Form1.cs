using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace 平差作业
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            initable();
        }
        private DataTable table = new DataTable();
        private MyAlog alog = new MyAlog();

        void initable()
        {
            table.Clear();
            table.Columns.Add("端点号", typeof(string));
            table.Columns.Add("高差观测值(m)", typeof(double));
            table.Columns.Add("测段距离(km)", typeof(double));
            table.Columns.Add("序号", typeof(double));
        }

        private void 打开OToolStripButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Title = "测绘平差数据文件";
            open.Filter = "文本文件|*.txt|所有文件|*.*";

            if (open.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (var sr = new StreamReader(open.FileName))
                    {
                        // 清空现有静态数据
                        MyAlog.data.Clear();
                        MyAlog.YZPoints.Clear();

                        // 读取总点数
                        sr.ReadLine(); // 跳过标题
                        alog.SumPoint = int.Parse(sr.ReadLine().Trim());

                        // 读取已知点
                        sr.ReadLine(); // 跳过标题
                        for (int i = 0; i < 2; i++)
                        {
                            if (!sr.EndOfStream)
                            {
                                string[] parts = sr.ReadLine().Split(',');
                                MyPoint point = new MyPoint(parts[0].Trim(), double.Parse(parts[1].Trim()));
                                MyAlog.YZPoints.Add(point);
                            }
                        }

                        // 读取测段数据
                        sr.ReadLine(); // 跳过标题
                        while (!sr.EndOfStream)
                        {
                            string line = sr.ReadLine();
                            if (!string.IsNullOrEmpty(line))
                            {
                                string[] parts = line.Split(',');
                                MyData data1 = new MyData(
                                    parts[0].Trim(),
                                    double.Parse(parts[1].Trim()),
                                    double.Parse(parts[2].Trim()),
                                    double.Parse(parts[3].Trim())
                                );
                                MyAlog.data.Add(data1);
                            }
                        }
                    }

                    // 刷新数据表格
                    table.Clear();
                    foreach (var item in MyAlog.data)
                    {
                        DataRow row = table.NewRow();
                        row["端点号"] = item.DDnumber;
                        row["高差观测值(m)"] = item.high;
                        row["测段距离(km)"] = item.distance;
                        row["序号"] = item.number;
                        table.Rows.Add(row);
                    }
                    dataGridView1.DataSource = table;

                    // 显示已知点信息
                    StringBuilder info = new StringBuilder();
                    info.AppendLine($"总点数: {alog.SumPoint}");
                    info.AppendLine("已知点信息:");
                    info.AppendLine($"  {MyAlog.YZPoints[0].id}: {MyAlog.YZPoints[0].high:F3}m");
                    info.AppendLine($"  {MyAlog.YZPoints[1].id}: {MyAlog.YZPoints[1].high:F3}m");
                    richTextBox2.Text = info.ToString();

                    MessageBox.Show("数据加载成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"数据加载失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            // 添加数据验证
            if (MyAlog.YZPoints.Count != 2)
            {
                MessageBox.Show("已知点数据必须为2个！", "数据错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (MyAlog.data.Count != 7)
            {
                MessageBox.Show("测段数据必须为7个！", "数据错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 验证已知点高程
            double H1 = MyAlog.YZPoints[0].high;
            double H2 = MyAlog.YZPoints[1].high;
            if (H1 <= 0 || H2 <= 0)
            {
                MessageBox.Show("已知点高程不能为负数！", "数据错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 验证测段距离
            foreach (var item in MyAlog.data)
            {
                if (item.distance <= 0)
                {
                    MessageBox.Show("测段距离不能为负数！", "数据错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void 打印PToolStripButton_Click(object sender, EventArgs e)
        {
            if (MyAlog.data.Count < 7)
            {
                MessageBox.Show("数据不足，无法进行平差计算！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string result = alog.PrintBG();
                richTextBox1.Text = result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"计算出错: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void 保存SToolStripButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(richTextBox1.Text))
            {
                MessageBox.Show("请先计算平差结果！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog save = new SaveFileDialog();
            save.Title = "保存平差报告";
            save.Filter = "文本文件|*.txt|Word文档|*.docx|所有文件|*.*";
            save.DefaultExt = "txt";

            if (save.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(save.FileName, richTextBox1.Text);
                    MessageBox.Show("报告保存成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"保存失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }



    }
}