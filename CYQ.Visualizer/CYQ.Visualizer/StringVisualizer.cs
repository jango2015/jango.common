using CYQ.Data.Table;
using Microsoft.VisualStudio.DebuggerVisualizers;
using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

[assembly: System.Diagnostics.DebuggerVisualizer(
typeof(CYQ.Visualizer.StringVisualizer),
typeof(VisualizerObjectSource),
Target = typeof(System.String),
Description = "Json Visualizer")]
namespace CYQ.Visualizer
{
    public class StringVisualizer : DialogDebuggerVisualizer
    {
        override protected void Show(IDialogVisualizerService windowService, IVisualizerObjectProvider objectProvider)
        {
            MDataTable dt = MDataTable.CreateFrom(objectProvider.GetObject() as string);
            FormCreate.BindTable(windowService, dt, null);
        }
    }
}
