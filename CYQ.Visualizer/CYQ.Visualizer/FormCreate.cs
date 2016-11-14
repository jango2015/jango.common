using CYQ.Data.Table;
using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CYQ.Visualizer
{
    internal static class FormCreate
    {
        public static Form CreateForm(string title)
        {
            Form form = new Form();
            form.Text = title;
            form.ClientSize = new Size(600, 400);
            form.FormBorderStyle = FormBorderStyle.Sizable;
            form.HorizontalScroll.Enabled = true;
            form.VerticalScroll.Enabled = true;
            return form;
        }
        public static DataGridView CreateGrid(Form parent)
        {
            DataGridView dg = new DataGridView();
            dg.Parent = parent;
            dg.ReadOnly = true;
            dg.Dock = DockStyle.Fill;
            dg.ScrollBars = ScrollBars.Both;
            //dg.AutoSize = true;
            // dg.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.ColumnHeader;
            return dg;
        }
        private static void AutoSizeColumn(DataGridView dgv)
        {
            int width = 0;
            //使列自使用宽度
            //对于DataGridView的每一个列都调整
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                //将每一列都调整为自动适应模式
                dgv.AutoResizeColumn(i, DataGridViewAutoSizeColumnMode.AllCells);
                //记录整个DataGridView的宽度
                width += dgv.Columns[i].Width;
            }
            //判断调整后的宽度与原来设定的宽度的关系，如果是调整后的宽度大于原来设定的宽度，
            //则将DataGridView的列自动调整模式设置为显示的列即可，
            //如果是小于原来设定的宽度，将模式改为填充。
            if (width > dgv.Size.Width)
            {
                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
                dgv.Columns[0].Frozen = true;
                if (dgv.Columns.Count > 1)
                {
                    //冻结某列 从左开始 0，1，2
                    dgv.Columns[1].Frozen = true;
                }
            }
            else
            {
                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }

        }
        public static void BindTable(IDialogVisualizerService windowService, MDataTable dt, string title)
        {
            if (dt == null) { return; }
            if (string.IsNullOrEmpty(title))
            {
                title = string.Format("TableName : {0}    Rows： {1}    Columns： {2}", dt.TableName, dt.Rows.Count, dt.Columns.Count);
            }
            Form form = FormCreate.CreateForm(title);
            DataGridView dg = FormCreate.CreateGrid(form);
            try
            {
                if (dt.Rows.Count > 200)
                {
                    dt = dt.Select(200, null);
                }
                //插入行号
                dt.Columns.Insert(0, new MCellStruct("[No.]", System.Data.SqlDbType.Int));
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i][0].Value = i + 1;
                }
                dt.Bind(dg);
                AutoSizeColumn(dg);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }

            windowService.ShowDialog(form);
        }
    }
}
