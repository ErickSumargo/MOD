using System;
using System.Drawing;
using System.Windows.Forms;

namespace Opticus
{
    public partial class Detail : Form
    {
        /*----------------------------------------Declaring Local Variables-----------------------------------------*/

        double sum_timeProcess;
        double sum_errorClassification;
        double sum_FAR;
        double sum_η;
        double sum_PC;

        private const uint SB_HORZ = 0;
        private const uint SB_VERT = 0;
        private const uint ESB_DISABLE_BOTH = 0x3;
        private const uint ESB_ENABLE_BOTH = 0x0;
        [System.Runtime.InteropServices.DllImport("user32.dll")]

        static public extern bool ShowScrollBar(System.IntPtr hWnd, int wBar, bool bShow);

        /*----------------------------------------------------------------------------------------------------------*/

        public Detail()
        {
            InitializeComponent();

            sum_timeProcess = 0;
            sum_errorClassification = 0;
            sum_FAR = 0;
            sum_η = 0;
            sum_PC = 0;

            CreateListTimeProcess();
            UpdateChartTimeProcess();
            UpdateListTimeProcess();

            CreateListErrorClassification();
            UpdateChartErrorClassification();
            UpdateListErrorClassification();

            CreateListFAR();
            UpdateChartFAR();
            UpdateListFAR();

            if (Transfer.sh_Activated)
            {
                CreateListη();
                UpdateChartη();
                UpdateListη();
            }

            if (Transfer.ls_Activated)
            {
                CreateListPC();
                UpdateChartPC();
                UpdateListPC();
            }

            LoadMeanResult();
        }

        public void CreateListTimeProcess()
        {
            listView_timeResulted.View = View.Details;
            listView_timeResulted.Columns.Add("");
            listView_timeResulted.Columns.Add("", 65, HorizontalAlignment.Center);
            listView_timeResulted.Columns.Add("", 190, HorizontalAlignment.Center);
            listView_timeResulted.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            listView_timeResulted.Columns[0].Width = 0;

            ShowScrollBar(listView_timeResulted.Handle, 1, true);
        }

        public void UpdateChartTimeProcess()
        {
            for (int i = 0; i < Transfer.timeProcess_System_List.Count; i++)
            {
                chart_timeResulted.Series[0].Points.AddXY((i + 1), Transfer.timeProcess_System_List[i]);
            }
        }

        public void UpdateListTimeProcess()
        {
            for (int i = 0; i < Transfer.timeProcess_System_List.Count; i++)
            {
                listView_timeResulted.Items.Add("");
                listView_timeResulted.Items[i].SubItems.Add((i).ToString());
                listView_timeResulted.Items[i].SubItems.Add(Transfer.timeProcess_System_List[i].ToString() + " ms");

                sum_timeProcess += Transfer.timeProcess_System_List[i];
            }
        }

        public void CreateListErrorClassification()
        {
            listView_errorClassification.View = View.Details;
            listView_errorClassification.Columns.Add("");
            listView_errorClassification.Columns.Add("", 65, HorizontalAlignment.Center);
            listView_errorClassification.Columns.Add("", 190, HorizontalAlignment.Center);
            listView_errorClassification.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            listView_errorClassification.Columns[0].Width = 0;

            ShowScrollBar(listView_errorClassification.Handle, 1, true);
        }

        public void UpdateChartErrorClassification()
        {
            for (int i = 0; i < Transfer.errorClassification_List.Count; i++)
            {
                chart_errorClassification.Series[0].Points.AddXY((i + 1), Transfer.errorClassification_List[i]);
            }
        }

        public void UpdateListErrorClassification()
        {
            for (int i = 0; i < Transfer.errorClassification_List.Count; i++)
            {
                listView_errorClassification.Items.Add("");
                listView_errorClassification.Items[i].SubItems.Add((i).ToString());
                listView_errorClassification.Items[i].SubItems.Add(string.Format("{0:#,##0.##}", Transfer.errorClassification_List[i]) + " %");

                sum_errorClassification += Transfer.errorClassification_List[i];
            }
        }

        public void CreateListFAR()
        {
            listView_FAR.View = View.Details;
            listView_FAR.Columns.Add("");
            listView_FAR.Columns.Add("", 65, HorizontalAlignment.Center);
            listView_FAR.Columns.Add("", 190, HorizontalAlignment.Center);
            listView_FAR.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            listView_FAR.Columns[0].Width = 0;

            ShowScrollBar(listView_FAR.Handle, 1, true);
        }

        public void UpdateChartFAR()
        {
            for (int i = 0; i < Transfer.FAR_List.Count; i++)
            {
                chart_FAR.Series[0].Points.AddXY((i + 1), Transfer.FAR_List[i]);
            }
        }

        public void UpdateListFAR()
        {
            for (int i = 0; i < Transfer.FAR_List.Count; i++)
            {
                listView_FAR.Items.Add("");
                listView_FAR.Items[i].SubItems.Add((i).ToString());
                listView_FAR.Items[i].SubItems.Add(string.Format("{0:#,##0.##}", Transfer.FAR_List[i]) + " %");

                sum_FAR += Transfer.FAR_List[i];
            }
        }

        public void CreateListη()
        {
            listView_η.View = View.Details;
            listView_η.Columns.Add("");
            listView_η.Columns.Add("", 65, HorizontalAlignment.Center);
            listView_η.Columns.Add("", 190, HorizontalAlignment.Center);
            listView_η.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            listView_η.Columns[0].Width = 0;

            ShowScrollBar(listView_η.Handle, 1, true);
        }

        public void UpdateChartη()
        {
            for (int i = 0; i < Transfer.η_List.Count; i++)
            {
                chart_η.Series[0].Points.AddXY((i + 1), Transfer.η_List[i]);
            }
        }

        public void UpdateListη()
        {
            for (int i = 0; i < Transfer.η_List.Count; i++)
            {
                listView_η.Items.Add("");
                listView_η.Items[i].SubItems.Add((i).ToString());
                listView_η.Items[i].SubItems.Add(string.Format("{0:#,##0.##}", Transfer.η_List[i]) + " %");

                sum_η += Transfer.η_List[i];
            }
        }

        public void CreateListPC()
        {
            listView_PC.View = View.Details;
            listView_PC.Columns.Add("");
            listView_PC.Columns.Add("", 65, HorizontalAlignment.Center);
            listView_PC.Columns.Add("", 190, HorizontalAlignment.Center);
            listView_PC.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            listView_PC.Columns[0].Width = 0;

            ShowScrollBar(listView_PC.Handle, 1, true);
        }

        public void UpdateChartPC()
        {
            for (int i = 0; i < Transfer.PC_List.Count; i++)
            {
                chart_PC.Series[0].Points.AddXY((i + 1), Transfer.PC_List[i]);
            }
        }

        public void UpdateListPC()
        {
            for (int i = 0; i < Transfer.PC_List.Count; i++)
            {
                listView_PC.Items.Add("");
                listView_PC.Items[i].SubItems.Add((i).ToString());
                listView_PC.Items[i].SubItems.Add(string.Format("{0:#,##0.##}", Transfer.PC_List[i]) + " %");

                sum_PC += Transfer.PC_List[i];
            }
        }

        public void LoadMeanResult()
        {
            label_meanTime.Text = string.Format("{0:#,##0.##}", sum_timeProcess / Transfer.timeProcess_System_List.Count) + " ms";
            label_meanError.Text = string.Format("{0:#,##0.##}", sum_errorClassification / Transfer.errorClassification_List.Count) + "%";
            label_meanFAR.Text = string.Format("{0:#,##0.##}", sum_FAR / Transfer.FAR_List.Count) + "%";

            if (Transfer.sh_Activated)
            {
                label_meanη.Text = string.Format("{0:#,##0.##}", sum_η / Transfer.η_List.Count) + "%";
            }

            if (Transfer.ls_Activated)
            {
                label_meanPC.Text = string.Format("{0:#,##0.##}", sum_PC / Transfer.PC_List.Count) + "%";
            }

            label_meanTime.Location = new Point((190 - label_meanTime.Width) / 2, 5);
            label_meanError.Location = new Point((190 - label_meanError.Width) / 2, 5);
            label_meanFAR.Location = new Point((190 - label_meanFAR.Width) / 2, 5);
            label_meanη.Location = new Point((190 - label_meanη.Width) / 2, 5);
            label_meanPC.Location = new Point((190 - label_meanPC.Width) / 2, 5);
        }
    }
}