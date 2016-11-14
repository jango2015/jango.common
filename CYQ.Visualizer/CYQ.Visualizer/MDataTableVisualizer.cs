using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.DebuggerVisualizers;
using System.Windows.Forms;
using System.Drawing;
using CYQ.Data.Table;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Data;
using CYQ.Visualizer;


[assembly: System.Diagnostics.DebuggerVisualizer(
typeof(CYQ.Visualizer.MDataTableVisualizer),
typeof(EnumerableVisualizerObjectSource),
Target = typeof(CYQ.Data.Table.MDataTable),
Description = "MDataTable Visualizer")]

[assembly: System.Diagnostics.DebuggerVisualizer(
typeof(CYQ.Visualizer.MDataRowVisualizer),
typeof(VisualizerObjectSource),
Target = typeof(CYQ.Data.Table.MDataRow),
Description = "MDataRow Visualizer")]

[assembly: System.Diagnostics.DebuggerVisualizer(
typeof(CYQ.Visualizer.MDataColumnVisualizer),
typeof(VisualizerObjectSource),
Target = typeof(CYQ.Data.Table.MDataColumn),
Description = "MDataColumn Visualizer")]

[assembly: System.Diagnostics.DebuggerVisualizer(
typeof(CYQ.Visualizer.MDataRowCollectionVisualizer),
typeof(VisualizerObjectSource),
Target = typeof(CYQ.Data.Table.MDataRowCollection),
Description = "MDataRowCollection Visualizer")]

namespace CYQ.Visualizer
{

    public class MDataTableVisualizer : DialogDebuggerVisualizer
    {
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            MDataTable dt = objectProvider.GetObject() as MDataTable;
            FormCreate.BindTable(windowService, dt, null);
        }
    }
    public class MDataRowVisualizer : DialogDebuggerVisualizer
    {
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            MDataRow row = objectProvider.GetObject() as MDataRow;
            if (row != null)
            {
                MDataTable dt = row.ToTable();
                string title = string.Format("TableName : {0}    Columns£º {1}", row.TableName, row.Columns.Count);
                FormCreate.BindTable(windowService, dt, title);
            }
        }
    }
    public class MDataColumnVisualizer : DialogDebuggerVisualizer
    {
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            MDataColumn mdc = objectProvider.GetObject() as MDataColumn;
            if (mdc != null)
            {
                MDataTable dt = mdc.ToTable();
                string title = string.Format("TableName : {0}    Columns£º {1}", dt.TableName, mdc.Count);
                FormCreate.BindTable(windowService, dt, title);
            }
        }
    }
    public class MDataRowCollectionVisualizer : DialogDebuggerVisualizer
    {
        protected override void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            MDataTable dt = objectProvider.GetObject() as MDataRowCollection;
            FormCreate.BindTable(windowService, dt, null);
        }
    }

   
}
