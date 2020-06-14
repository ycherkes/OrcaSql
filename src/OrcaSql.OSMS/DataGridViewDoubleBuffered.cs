using System;
using System.Windows.Forms;

namespace OrcaSql.OSMS
{
    public class DataGridViewDoubleBuffered : DataGridView
    {
        protected override void OnHandleCreated(EventArgs e)
        {
            DoubleBuffered = true;
            base.OnHandleCreated(e);
        }
    }
}