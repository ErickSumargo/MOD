using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace Opticus
{
    public partial class Main : Form
    {
        /*----------------------------------------Declaring Local Variables-----------------------------------------*/

        int sizeX, sizeY;
        int frameRate;
        int N;
        int counter;
        int coordinate;
        int R;
        int index1, index2, index3, index4, index5, index6, index7, index8, index9;

        double timeProcess_System, timeProcess_LOTS, timeProcess_Shadow,
               timeProcess_Sobel, timeProcess_FBS, timeProcess_Sobel_FBS;

        bool initialization;
        bool sh_Activated, cf_Activated, ls_Activated;
        bool full;
        bool ready_showDetail;
        bool updateInfo_Shown;
        bool ls_Off;
        bool controlShowed;
        bool modeShowed;
        bool chartShowed;

        string path;

        const int WM_CLOSE = 0x0010;
        [System.Runtime.InteropServices.DllImport("user32.dll", SetLastError = true)]

        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]

        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        /*----------------------------------------------------------------------------------------------------------*/

        /*------------------------------------------Declaring Main Classes------------------------------------------*/

        Webcam webcam;

        ColorConversion colorConversion;

        Opticus opticus;

        QualityMeasurement qualityMeasurement;

        Detail detail;

        /*----------------------------------------------------------------------------------------------------------*/

        /*-------------------------------------------Declaring SubClasses-------------------------------------------*/

        Bitmap RF, RGB_RF, Gray_RF;

        List<Bitmap> RGBFrames, GrayFrames;

        List<double> timeProcess_System_List;

        FileStream fileStream;

        Panel[] area_frameBoxes;
        PictureBox[] frameBoxes;
        Label[] frameBoxes_Name;

        DialogResult dialogResult;

        DateTime ls_timeOff;

        TimeSpan timeSpan;

        /*----------------------------------------------------------------------------------------------------------*/

        public Main()
        {
            InitializeComponent();

            /*---------------------------------------Defining Local Variables---------------------------------------*/

            sizeX = 160; sizeY = 120;
            frameRate = 30;
            counter = 0;
            coordinate = 0;
            R = 0;
            index1 = index2 = index3 = index4 = index5 = index6 = index7 = index8 = index9 = -1;

            initialization = false;
            sh_Activated = false;
            cf_Activated = false;
            ls_Activated = false;
            full = false;
            ready_showDetail = false;
            updateInfo_Shown = false;
            ls_Off = false;
            controlShowed = false;
            modeShowed = false;
            chartShowed = false;

            /*------------------------------------------------------------------------------------------------------*/

            /*-----------------------------------------Defining Main Classes----------------------------------------*/

            qualityMeasurement = new QualityMeasurement();

            /*------------------------------------------------------------------------------------------------------*/

            /*-----------------------------------------Defining Sub Classes-----------------------------------------*/

            RGBFrames = new List<Bitmap>();
            GrayFrames = new List<Bitmap>();

            timeProcess_System_List = new List<double>();

            area_frameBoxes = new Panel[4] { area_frameBox1, area_frameBox2, area_frameBox3, area_frameBox4 };
            frameBoxes = new PictureBox[4] { frameBox1, frameBox2, frameBox3, frameBox4 };
            frameBoxes_Name = new Label[4] { frameBox1_Name, frameBox2_Name, frameBox3_Name, frameBox4_Name };

            /*------------------------------------------------------------------------------------------------------*/

            ClearReportDocumentation();
        }

        public void WebcamOn()
        {
            N = (int)numericUpDown_N.Value;

            Transfer.N = N;
            Transfer.DS = (int)numericUpDown_DS.Value;

            Transfer.ρ = (double)numericUpDown_ρ.Value;
            Transfer.δ = (double)numericUpDown_δ.Value;
            Transfer.τs = (int)numericUpDown_τs.Value;
            Transfer.τh = (int)numericUpDown_τh.Value;

            Transfer.SS = (int)numericUpDown_SS.Value;
            Transfer.MF_W = (int)numericUpDown_MF_W.Value;
            Transfer.FBS_W = (int)numericUpDown_FBS_W.Value;

            Transfer.LA = (int)numericUpDown_LA.Value;

            Transfer.sizeX = sizeX;
            Transfer.sizeY = sizeY;

            colorConversion = new ColorConversion();

            webcam = new Webcam(new Size(sizeX, sizeY), frameRate);
            webcam.Start();

            Timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (webcam.cameraReady)
                {
                    RF = new Bitmap(webcam.RF);
                    RGB_RF = new Bitmap(webcam.RF);

                    Gray_RF = new Bitmap(webcam.RF);
                    colorConversion.GrayScale(Gray_RF);

                    if (counter < N)
                    {
                        RGBFrames.Add(RGB_RF);
                        GrayFrames.Add(Gray_RF);

                        counter++;

                        RGB_RF.Save(@"Dataset/Background Sequence/BS-" + (counter).ToString() + ".png");
                    }

                    else if (!initialization)
                    {
                        InitializationValueTransfering();

                        opticus = new Opticus();
                        opticus.Initialization();

                        initialization = true;

                        Transfer.RGB_B.Save(@"Dataset/RGBBackground/RGB_B-" + (N + 1).ToString() + ".png");
                        Transfer.Gray_B.Save(@"Dataset/GrayBackground/Gray_B-" + (N + 1).ToString() + ".png");
                        Transfer.HSL_B.Save(@"Dataset/HSLBackground/HSL_B-" + (N + 1).ToString() + ".png");
                    }

                    if (initialization)
                    {
                        FrameTransfering();

                        var stopWatch_System = Stopwatch.StartNew();
                        Detection();
                        stopWatch_System.Stop();
                        timeProcess_System = stopWatch_System.ElapsedMilliseconds;

                        timeProcess_System_List.Add(timeProcess_System);
                        Transfer.timeProcess_System_List = timeProcess_System_List;

                        if (!ready_showDetail)
                        {
                            checkBox_sh_Activated.Enabled = true;
                            checkBox_cf_Activated.Enabled = true;
                            checkBox_ls_Activated.Enabled = true;

                            button_detail.Enabled = true;
                            button_clear.Enabled = true;
                            button_reset.Enabled = true;

                            button_detail.BackColor = Color.FromArgb(0, 119, 234);
                            button_clear.BackColor = Color.FromArgb(0, 119, 234);
                            button_reset.BackColor = Color.FromArgb(0, 119, 234);

                            ready_showDetail = true;
                        }

                        ShowProcessed();
                        CheckQuality();
                        UpdateChart();
                        CreateReportDocumentation();

                        opticus.Reset();
                    }
                }
            }

            catch (Exception)
            {
                MessageBox.Show("Error(s)'re raised during process", "Detection Failed!");
            }
        }

        public void InitializationValueTransfering()
        {
            Transfer.RGBFrames = RGBFrames;
            Transfer.GrayFrames = GrayFrames;
        }

        public void FrameTransfering()
        {
            Transfer.RF = RF;
            Transfer.RGB_RF = RGB_RF;
            Transfer.Gray_RF = Gray_RF;
        }

        public void Detection()
        {
            var stopWatch_LOTS = Stopwatch.StartNew();
            opticus.Detection();
            stopWatch_LOTS.Stop();
            timeProcess_LOTS = stopWatch_LOTS.ElapsedMilliseconds;

            if (sh_Activated)
            {
                var stopWatch_Shadow = Stopwatch.StartNew();
                opticus.ShadowDetection();
                stopWatch_Shadow.Stop();
                timeProcess_Shadow = stopWatch_Shadow.ElapsedMilliseconds;
            }

            if (cf_Activated)
            {
                var stopWatch_Sobel = Stopwatch.StartNew();
                opticus.Segmentation();
                stopWatch_Sobel.Stop();
                timeProcess_Sobel = stopWatch_Sobel.ElapsedMilliseconds;

                var stopWatch_FBS = Stopwatch.StartNew();
                opticus.Morphology();
                stopWatch_FBS.Stop();
                timeProcess_FBS = stopWatch_FBS.ElapsedMilliseconds;

                opticus.CamouflagelessResult();
                timeProcess_Sobel_FBS = timeProcess_Sobel + timeProcess_FBS;
            }

            if (ls_Activated)
            {
                opticus.Correlation();

                if (ls_Off == false)
                {
                    if (Transfer.PC < Transfer.LA)
                    {
                        if (!updateInfo_Shown)
                        {
                            updateInfo_Shown = true;

                            dialogResult = MessageBox.Show("Do you want to update the model background? [P'C (" + string.Format("{0:#,##0.##}", Transfer.PC) + "%) < LA (" + Transfer.LA + "%)]",
                                                    "UpdateBackground", MessageBoxButtons.OKCancel);

                            if (dialogResult == DialogResult.None || dialogResult == DialogResult.Cancel)
                            {
                                updateInfo_Shown = false;
                                ls_Off = true;
                            }

                            else if (dialogResult == DialogResult.OK)
                            {
                                updateInfo_Shown = false;
                                ls_Off = true;

                                opticus.BackgroundUpdate();
                            }

                            ls_timeOff = DateTime.Now;
                        }
                    }

                    else
                    {
                        if (updateInfo_Shown)
                        {
                            updateInfo_Shown = false;

                            IntPtr mbWnd = FindWindow("#32770", "UpdateBackground");

                            if (mbWnd != IntPtr.Zero)
                            {
                                SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                            }
                        }
                    }
                }

                else
                {
                    timeSpan = DateTime.Now.Subtract(ls_timeOff);

                    if (timeSpan.TotalSeconds >= 2)
                    {
                        ls_Off = false;
                    }
                }
            }

            opticus.Merge();

            opticus.ConnectedComponentRegion();
        }

        public void ShowProcessed()
        {
            for (int i = 0; i < 4; i++)
            {
                if (frameBoxes[i].Image != null)
                {
                    if (frameBoxes_Name[i].Text == "Running Frame")
                    {
                        frameBoxes[i].Image = RF;
                    }

                    if (frameBoxes_Name[i].Text == "Absolute Difference")
                    {
                        frameBoxes[i].Image = Transfer.Binary_AD;
                    }

                    if (frameBoxes_Name[i].Text == "Detected Shadow")
                    {
                        frameBoxes[i].Image = Transfer.Binary_DS;
                    }

                    if (frameBoxes_Name[i].Text == "Sobel")
                    {
                        frameBoxes[i].Image = Transfer.Binary_S_N;
                    }

                    if (frameBoxes_Name[i].Text == "Sobel + MF")
                    {
                        frameBoxes[i].Image = Transfer.Binary_S;
                    }

                    if (frameBoxes_Name[i].Text == "FBS")
                    {
                        frameBoxes[i].Image = Transfer.Binary_FBS;
                    }

                    if (frameBoxes_Name[i].Text == "(Sobel + MF) + FBS")
                    {
                        frameBoxes[i].Image = Transfer.Binary_CL;
                    }

                    if (frameBoxes_Name[i].Text == "Merged Feature(s)")
                    {
                        frameBoxes[i].Image = Transfer.Binary_CCL;
                    }

                    if (frameBoxes_Name[i].Text == "Processed Frame")
                    {
                        frameBoxes[i].Image = Transfer.PF;
                    }
                }
            }

            pictureBox_RF.Image = RF;

            pictureBox_AD.Image = Transfer.Binary_AD;

            if (sh_Activated)
            {
                pictureBox_DS.Image = Transfer.Binary_DS;
            }

            if (cf_Activated)
            {
                pictureBox_S.Image = Transfer.Binary_S_N;

                pictureBox_S_MF.Image = Transfer.Binary_S;

                pictureBox_FBS.Image = Transfer.Binary_FBS;

                pictureBox_S_MF_FBS.Image = Transfer.Binary_CL;
            }

            pictureBox_M.Image = Transfer.Binary_CCL;

            pictureBox_PF.Image = Transfer.PF;
        }

        public void CheckQuality()
        {
            Transfer.checkingLOTS = true;
            qualityMeasurement.ROIDetection(Transfer.Binary_AD);
            Transfer.checkingLOTS = false;

            if (sh_Activated)
            {
                Transfer.checkingShadow = true;
                qualityMeasurement.ROIDetection(Transfer.Binary_SR);
                Transfer.checkingShadow = false;
            }

            if (cf_Activated)
            {
                Transfer.checkingCamouflage = true;

                Transfer.checkingSobel = true;
                qualityMeasurement.ROIDetection(Transfer.Binary_S);
                Transfer.checkingSobel = false;

                Transfer.checkingFBS = true;
                qualityMeasurement.ROIDetection(Transfer.Binary_FBS);
                Transfer.checkingFBS = false;

                Transfer.checkingSobel_FBS = true;
                qualityMeasurement.ROIDetection(Transfer.Binary_CL);
                Transfer.checkingSobel_FBS = false;

                Transfer.checkingCamouflage = false;
            }

            Transfer.checkingMerge = true;
            qualityMeasurement.ROIDetection(Transfer.Binary_M);
            Transfer.checkingMerge = false;
        }

        public void UpdateChart()
        {
            chart_timeProcess_System.Series[0].Points.AddXY(coordinate, timeProcess_System);
            chart_errorClassification.Series[0].Points.AddXY(coordinate, Transfer.errorClassification_Merge);
            chart_FAR.Series[0].Points.AddXY(coordinate, Transfer.FAR_Merge);
            chart_η.Series[0].Points.AddXY(coordinate, Transfer.η);

            coordinate++;
        }

        public void CreateReportDocumentation()
        {
            RF.Save(@"Dataset/Frame Sequence/FS-" + (R + 1).ToString() + ".png");
            Transfer.Gray_RF.Save(@"Dataset/Gray_currentFrame/G_CF-" + (R + 1).ToString() + ".png");
            Transfer.Gray_AD.Save(@"Dataset/Gray_absoluteDifference/G_AD-" + (R + 1).ToString() + ".png");
            Transfer.Binary_AD.Save(@"Dataset/Binary_absoluteDifference/B_AD-" + (R + 1).ToString() + ".png");

            if (sh_Activated)
            {
                Transfer.HSL_RF.Save(@"Dataset/HSL_currentFrame/HSL_CF-" + (R + 1).ToString() + ".png");
                Transfer.Binary_DS.Save(@"Dataset/Binary_detectedShadow/B_DS-" + (R + 1).ToString() + ".png");
                Transfer.Binary_SR.Save(@"Dataset/Binary_shadowRegion/B_SR-" + (R + 1).ToString() + ".png");
                Transfer.Binary_SL.Save(@"Dataset/Binary_shadowless/B_SL-" + (R + 1).ToString() + ".png");
            }

            if (cf_Activated)
            {
                Transfer.Gray_S.Save(@"Dataset/Gray_segmentedImage/G_SI-" + (R + 1).ToString() + ".png");
                Transfer.Binary_S_N.Save(@"Dataset/Binary_segmentedImage_noised/B_SI_N-" + (R + 1).ToString() + ".png");
                Transfer.Binary_S.Save(@"Dataset/Binary_segmentedImage/B_SI-" + (R + 1).ToString() + ".png");
                Transfer.Binary_FBS.Save(@"Dataset/Binary_dilatedImage/B_DI-" + (R + 1).ToString() + ".png");
                Transfer.Binary_CL.Save(@"Dataset/Binary_camouflaged/B_C-" + (R + 1).ToString() + ".png");
            }

            Transfer.Binary_M.Save(@"Dataset/Binary_processedImage/B_PI-" + (R + 1).ToString() + ".png");
            Transfer.Binary_CCL.Save(@"Dataset/processedImage/PI-" + (R + 1).ToString() + ".png");
            Transfer.PF.Save(@"Dataset/processedFrame/PF-" + (R + 1).ToString() + ".png");

            path = @"Dataset\Report\report-" + (R + 1).ToString() + ".txt";

            var report = File.Create(path);
            report.Close();

            using (fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write))
            using (StreamWriter writer = new StreamWriter(fileStream))
            {
                writer.WriteLine("LOTS Result [N = " + Transfer.N + ", DS = " + Transfer.DS + "]");
                writer.WriteLine("============================================================");
                writer.WriteLine("Time\t\t= " + timeProcess_LOTS.ToString() + "ms");
                writer.WriteLine("Correct\t\t= " + Transfer.correct_LOTS.ToString());
                writer.WriteLine("Insertion\t= " + Transfer.insertion_LOTS.ToString());
                writer.WriteLine("TP\t\t= " + Transfer.TP_LOTS.ToString());
                writer.WriteLine("FP\t\t= " + Transfer.FP_LOTS.ToString());
                writer.WriteLine("%E\t\t= " + Transfer.errorClassification_LOTS.ToString() + "%");
                writer.WriteLine("FAR\t\t= " + Transfer.FAR_LOTS.ToString() + "%");
                writer.WriteLine("");

                if (sh_Activated)
                {
                    writer.WriteLine("Shadow Handling Result [ρ = " + Transfer.ρ + ", δ = " + Transfer.δ + ", τs = " + Transfer.τs + ", τh = " + Transfer.τh + "]");
                    writer.WriteLine("============================================================");
                    writer.WriteLine("Time\t\t= " + timeProcess_Shadow.ToString() + "ms");
                    writer.WriteLine("Correct\t\t= " + Transfer.correct_Shadow.ToString());
                    writer.WriteLine("Insertion\t= " + Transfer.insertion_Shadow.ToString());
                    writer.WriteLine("TP\t\t= " + Transfer.TP_Shadow.ToString());
                    writer.WriteLine("FP\t\t= " + Transfer.FP_Shadow.ToString());
                    writer.WriteLine("%η\t\t= " + Transfer.η.ToString() + "%");
                    writer.WriteLine("");
                }

                if (cf_Activated)
                {
                    writer.WriteLine("Camouflage Handling Result");
                    writer.WriteLine("============================================================");
                    writer.WriteLine("");

                    writer.WriteLine("Sobel + Median Filter Result [SS = " + Transfer.SS + ", MF (W)= " + Transfer.MF_W + "×" + Transfer.MF_W + "]");
                    writer.WriteLine("------------------------------------------------------------");
                    writer.WriteLine("Time\t\t= " + timeProcess_Sobel.ToString() + "ms");
                    writer.WriteLine("Correct\t\t= " + Transfer.correct_Sobel.ToString());
                    writer.WriteLine("Insertion\t= " + Transfer.insertion_Sobel.ToString());
                    writer.WriteLine("TP\t\t= " + Transfer.TP_Sobel.ToString());
                    writer.WriteLine("FP\t\t= " + Transfer.FP_Sobel.ToString());
                    writer.WriteLine("%E\t\t= " + Transfer.errorClassification_Sobel.ToString() + "%");
                    writer.WriteLine("FAR\t\t= " + Transfer.FAR_Sobel.ToString() + "%");
                    writer.WriteLine("");

                    writer.WriteLine("FBS Result [FBS (W) = " + Transfer.FBS_W + "×" + Transfer.FBS_W + "]");
                    writer.WriteLine("------------------------------------------------------------");
                    writer.WriteLine("Time\t\t= " + timeProcess_FBS.ToString() + "ms");
                    writer.WriteLine("Correct\t\t= " + Transfer.correct_FBS.ToString());
                    writer.WriteLine("Insertion\t= " + Transfer.insertion_FBS.ToString());
                    writer.WriteLine("TP\t\t= " + Transfer.TP_FBS.ToString());
                    writer.WriteLine("FP\t\t= " + Transfer.FP_FBS.ToString());
                    writer.WriteLine("%E\t\t= " + Transfer.errorClassification_FBS.ToString() + "%");
                    writer.WriteLine("FAR\t\t= " + Transfer.FAR_FBS.ToString() + "%");
                    writer.WriteLine("");

                    writer.WriteLine("(Sobel + Median Filter) + FBS Result");
                    writer.WriteLine("------------------------------------------------------------");
                    writer.WriteLine("Time\t\t= " + timeProcess_Sobel_FBS.ToString() + "ms");
                    writer.WriteLine("Correct\t\t= " + Transfer.correct_Sobel_FBS.ToString());
                    writer.WriteLine("Insertion\t= " + Transfer.insertion_Sobel_FBS.ToString());
                    writer.WriteLine("TP\t\t= " + Transfer.TP_Sobel_FBS.ToString());
                    writer.WriteLine("FP\t\t= " + Transfer.FP_Sobel_FBS.ToString());
                    writer.WriteLine("%E\t\t= " + Transfer.errorClassification_Sobel_FBS.ToString() + "%");
                    writer.WriteLine("FAR\t\t= " + Transfer.FAR_Sobel_FBS.ToString() + "%");
                    writer.WriteLine("");
                }

                if (ls_Activated)
                {
                    writer.WriteLine("Light Switch Response [LA = " + Transfer.LA + "%]");
                    writer.WriteLine("============================================================");
                    writer.WriteLine("Person's Correlation\t= " + Transfer.PC.ToString() + "%");
                    writer.WriteLine("");
                }

                writer.WriteLine("Feature Merge Result");
                writer.WriteLine("============================================================");
                writer.WriteLine("Time\t\t= " + timeProcess_System.ToString() + "ms");
                writer.WriteLine("Correct\t\t= " + Transfer.correct_Merge.ToString());
                writer.WriteLine("Insertion\t= " + Transfer.insertion_Merge.ToString());
                writer.WriteLine("TP\t\t= " + Transfer.TP_Merge.ToString());
                writer.WriteLine("FP\t\t= " + Transfer.FP_Merge.ToString());
                writer.WriteLine("%E\t\t= " + Transfer.errorClassification_Merge.ToString() + "%");
                writer.WriteLine("FAR\t\t= " + Transfer.FAR_Merge.ToString() + "%");

                writer.Close();
                fileStream.Close();
            }

            R++;
        }

        public void ClearReportDocumentation()
        {
            Array.ForEach(Directory.GetFiles(@"Dataset\Background Sequence\"), File.Delete);
            Array.ForEach(Directory.GetFiles(@"Dataset\Binary_absoluteDifference\"), File.Delete);
            Array.ForEach(Directory.GetFiles(@"Dataset\Binary_camouflaged\"), File.Delete);
            Array.ForEach(Directory.GetFiles(@"Dataset\Binary_detectedShadow\"), File.Delete);
            Array.ForEach(Directory.GetFiles(@"Dataset\Binary_dilatedImage\"), File.Delete);
            Array.ForEach(Directory.GetFiles(@"Dataset\Binary_processedImage\"), File.Delete);
            Array.ForEach(Directory.GetFiles(@"Dataset\Binary_segmentedImage\"), File.Delete);
            Array.ForEach(Directory.GetFiles(@"Dataset\Binary_segmentedImage_noised\"), File.Delete);
            Array.ForEach(Directory.GetFiles(@"Dataset\Binary_shadowless\"), File.Delete);
            Array.ForEach(Directory.GetFiles(@"Dataset\Binary_shadowRegion\"), File.Delete);
            Array.ForEach(Directory.GetFiles(@"Dataset\Favourite\"), File.Delete);
            Array.ForEach(Directory.GetFiles(@"Dataset\Frame Sequence\"), File.Delete);
            Array.ForEach(Directory.GetFiles(@"Dataset\Gray_absoluteDifference\"), File.Delete);
            Array.ForEach(Directory.GetFiles(@"Dataset\Gray_currentFrame\"), File.Delete);
            Array.ForEach(Directory.GetFiles(@"Dataset\Gray_segmentedImage\"), File.Delete);
            Array.ForEach(Directory.GetFiles(@"Dataset\GrayBackground\"), File.Delete);
            Array.ForEach(Directory.GetFiles(@"Dataset\HSL_currentFrame\"), File.Delete);
            Array.ForEach(Directory.GetFiles(@"Dataset\HSLBackground\"), File.Delete);
            Array.ForEach(Directory.GetFiles(@"Dataset\Report\"), File.Delete);
            Array.ForEach(Directory.GetFiles(@"Dataset\processedFrame\"), File.Delete);
            Array.ForEach(Directory.GetFiles(@"Dataset\processedImage\"), File.Delete);
            Array.ForEach(Directory.GetFiles(@"Dataset\RGBBackground\"), File.Delete);
        }

        /*---------------------------------------------UI Interactions---------------------------------------------*/

        /*---------------------------------------------NumericUpDown UI--------------------------------------------*/

        private void numericUpDown_N_ValueChanged(object sender, EventArgs e)
        {
            Transfer.N = (int)numericUpDown_N.Value;
        }

        private void numericUpDown_DS_ValueChanged(object sender, EventArgs e)
        {
            Transfer.DS = (int)numericUpDown_DS.Value;
        }

        private void numericUpDown_ρ_ValueChanged(object sender, EventArgs e)
        {
            Transfer.ρ = (double)numericUpDown_ρ.Value;
        }

        private void numericUpDown_δ_ValueChanged(object sender, EventArgs e)
        {
            Transfer.δ = (double)numericUpDown_δ.Value;
        }

        private void numericUpDown_τs_ValueChanged(object sender, EventArgs e)
        {
            Transfer.τs = (double)numericUpDown_τs.Value;
        }

        private void numericUpDown_τh_ValueChanged(object sender, EventArgs e)
        {
            Transfer.τh = (double)numericUpDown_τh.Value;
        }

        private void numericUpDown_SS_ValueChanged(object sender, EventArgs e)
        {
            Transfer.SS = (int)numericUpDown_SS.Value;
        }

        private void numericUpDown_MF_W_ValueChanged(object sender, EventArgs e)
        {
            Transfer.MF_W = (int)numericUpDown_MF_W.Value;
        }

        private void numericUpDown_FBS_W_ValueChanged(object sender, EventArgs e)
        {
            Transfer.FBS_W = (int)numericUpDown_FBS_W.Value;
        }

        private void numericUpDown_LA_ValueChanged(object sender, EventArgs e)
        {
            Transfer.LA = (int)numericUpDown_LA.Value;
        }

        /*------------------------------------------------------------------------------------------------------*/

        /*----------------------------------------------CheckBox UI---------------------------------------------*/

        private void checkBox_sh_Activated_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_sh_Activated.Checked)
            {
                sh_Activated = true;

                numericUpDown_ρ.Enabled = true;
                numericUpDown_δ.Enabled = true;
                numericUpDown_τs.Enabled = true;
                numericUpDown_τh.Enabled = true;

                numericUpDown_ρ.BackColor = Color.White;
                numericUpDown_ρ.ForeColor = Color.Black;
                numericUpDown_δ.BackColor = Color.White;
                numericUpDown_δ.ForeColor = Color.Black;
                numericUpDown_τs.BackColor = Color.White;
                numericUpDown_τs.ForeColor = Color.Black;
                numericUpDown_τh.BackColor = Color.White;
                numericUpDown_τh.ForeColor = Color.Black;

                if (!full)
                {
                    checkBox_DS.Enabled = true;
                }

                else
                {
                    checkBox_DS.Enabled = false;
                }
            }

            else
            {
                sh_Activated = false;

                numericUpDown_ρ.Enabled = false;
                numericUpDown_δ.Enabled = false;
                numericUpDown_τs.Enabled = false;
                numericUpDown_τh.Enabled = false;

                numericUpDown_ρ.BackColor = Color.LightGray;
                numericUpDown_ρ.ForeColor = Color.DimGray;
                numericUpDown_δ.BackColor = Color.LightGray;
                numericUpDown_δ.ForeColor = Color.DimGray;
                numericUpDown_τs.BackColor = Color.LightGray;
                numericUpDown_τs.ForeColor = Color.DimGray;
                numericUpDown_τh.BackColor = Color.LightGray;
                numericUpDown_τh.ForeColor = Color.DimGray;

                checkBox_DS.Checked = false;
                checkBox_DS.Enabled = false;

                pictureBox_DS.Image = null;
            }

            Transfer.sh_Activated = sh_Activated;
        }

        private void checkBox_cf_Activated_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_cf_Activated.Checked)
            {
                cf_Activated = true;

                numericUpDown_SS.Enabled = true;
                numericUpDown_MF_W.Enabled = true;
                numericUpDown_FBS_W.Enabled = true;

                numericUpDown_SS.BackColor = Color.White;
                numericUpDown_SS.ForeColor = Color.Black;
                numericUpDown_MF_W.BackColor = Color.White;
                numericUpDown_MF_W.ForeColor = Color.Black;
                numericUpDown_FBS_W.BackColor = Color.White;
                numericUpDown_FBS_W.ForeColor = Color.Black;

                if (!full)
                {
                    checkBox_S.Enabled = true;
                    checkBox_S_MF.Enabled = true;
                    checkBox_FBS.Enabled = true;
                    checkBox_S_MF_FBS.Enabled = true;
                }

                else
                {
                    checkBox_S.Enabled = false;
                    checkBox_S_MF.Enabled = false;
                    checkBox_FBS.Enabled = false;
                    checkBox_S_MF_FBS.Enabled = false;
                }
            }

            else
            {
                cf_Activated = false;

                numericUpDown_SS.Enabled = false;
                numericUpDown_MF_W.Enabled = false;
                numericUpDown_FBS_W.Enabled = false;

                numericUpDown_SS.BackColor = Color.LightGray;
                numericUpDown_SS.ForeColor = Color.DimGray;
                numericUpDown_MF_W.BackColor = Color.LightGray;
                numericUpDown_MF_W.ForeColor = Color.DimGray;
                numericUpDown_FBS_W.BackColor = Color.LightGray;
                numericUpDown_FBS_W.ForeColor = Color.DimGray;

                checkBox_S.Checked = false;
                checkBox_S.Enabled = false;

                checkBox_S_MF.Checked = false;
                checkBox_S_MF.Enabled = false;

                checkBox_FBS.Checked = false;
                checkBox_FBS.Enabled = false;

                checkBox_S_MF_FBS.Checked = false;
                checkBox_S_MF_FBS.Enabled = false;

                pictureBox_S.Image = null;
                pictureBox_S_MF.Image = null;
                pictureBox_FBS.Image = null;
                pictureBox_S_MF_FBS.Image = null;
            }

            Transfer.cf_Activated = cf_Activated;
        }

        private void checkBox_ls_Activated_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_ls_Activated.Checked)
            {
                ls_Activated = true;

                numericUpDown_LA.Enabled = true;

                numericUpDown_LA.BackColor = Color.White;
                numericUpDown_LA.ForeColor = Color.Black;
            }

            else
            {
                ls_Activated = false;

                numericUpDown_LA.Enabled = false;

                numericUpDown_LA.BackColor = Color.LightGray;
                numericUpDown_LA.ForeColor = Color.DimGray;
            }

            Transfer.ls_Activated = ls_Activated;
        }

        private void checkBox_RF_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_RF.Checked)
            {
                checkedChanged_Effect("Running Frame", pictureBox_RF, ref index1);
            }

            else
            {
                uncheckedChanged_Effect(ref index1);
            }
        }

        private void checkBox_AD_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_AD.Checked)
            {
                checkedChanged_Effect("Absolute Difference", pictureBox_AD, ref index2);
            }

            else
            {
                uncheckedChanged_Effect(ref index2);
            }
        }

        private void checkBox_DS_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_DS.Checked)
            {
                checkedChanged_Effect("Detected Shadow", pictureBox_DS, ref index3);
            }

            else
            {
                uncheckedChanged_Effect(ref index3);
            }
        }

        private void checkBox_S_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_S.Checked)
            {
                checkedChanged_Effect("Sobel", pictureBox_S, ref index4);
            }

            else
            {
                uncheckedChanged_Effect(ref index4);
            }
        }

        private void checkBox_S_MF_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_S_MF.Checked)
            {
                checkedChanged_Effect("Sobel + MF", pictureBox_S_MF, ref index5);
            }

            else
            {
                uncheckedChanged_Effect(ref index5);
            }
        }

        private void checkBox_FBS_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_FBS.Checked)
            {
                checkedChanged_Effect("FBS", pictureBox_FBS, ref index6);
            }

            else
            {
                uncheckedChanged_Effect(ref index6);
            }
        }

        private void checkBox_S_MF_FBS_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_S_MF_FBS.Checked)
            {
                checkedChanged_Effect("(Sobel + MF) + FBS", pictureBox_S_MF_FBS, ref index7);
            }

            else
            {
                uncheckedChanged_Effect(ref index7);
            }
        }

        private void checkBox_M_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_M.Checked)
            {
                checkedChanged_Effect("Merged Feature(s)", pictureBox_M, ref index8);
            }

            else
            {
                uncheckedChanged_Effect(ref index8);
            }
        }

        private void checkBox_PF_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox_PF.Checked)
            {
                checkedChanged_Effect("Processed Frame", pictureBox_PF, ref index9);
            }

            else
            {
                uncheckedChanged_Effect(ref index9);
            }
        }

        public void checkedChanged_Effect(string frameBox_Name, PictureBox _modeFrame, ref int index)
        {
            full = true;

            for (int i = 0; i < 4; i++)
            {
                if (frameBoxes[i].Image == null)
                {
                    frameBoxes_Name[i].Text = frameBox_Name;
                    frameBoxes[i].Image = _modeFrame.Image;

                    index = i;

                    break;
                }
            }

            for (int i = 0; i < 4; i++)
            {
                if (frameBoxes[i].Image == null)
                {
                    full = false;

                    break;
                }
            }

            if (full)
            {
                foreach (Control p in this.Controls)
                {
                    if (p is Panel && p.Name == "area_mode")
                    {
                        foreach (Control cb in p.Controls)
                        {
                            if (cb is CheckBox)
                            {
                                if (!((CheckBox)cb).Checked)
                                {
                                    cb.Enabled = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        public void uncheckedChanged_Effect(ref int index)
        {
            full = false;

            frameBoxes_Name[index].Text = "Frame Box " + (index + 1).ToString();
            frameBoxes[index].Image = null;

            index = -1;

            foreach (Control p in this.Controls)
            {
                if (p is Panel)
                {
                    foreach (Control cb in p.Controls)
                    {
                        if (cb is CheckBox)
                        {
                            cb.Enabled = true;
                        }
                    }
                }
            }

            if (!sh_Activated)
            {
                checkBox_DS.Enabled = false;
            }

            if (!cf_Activated)
            {
                checkBox_S.Enabled = false;
                checkBox_S_MF.Enabled = false;
                checkBox_FBS.Enabled = false;
                checkBox_S_MF_FBS.Enabled = false;
            }
        }

        /*--------------------------------------------------------------------------------------------------------------*/

        /*---------------------------------------------------Button UI--------------------------------------------------*/

        private void button_control_Click(object sender, EventArgs e)
        {
            if (!controlShowed)
            {
                controlShowed = true;

                area_control.Visible = true;

                area_control.BringToFront();

                if (modeShowed)
                {
                    area_mode.Location = new Point(500, 0);
                }

                button_control.BackColor = Color.FromArgb(224, 224, 224);
            }

            else
            {
                controlShowed = false;

                area_control.Visible = false;

                if (modeShowed)
                {
                    area_mode.Location = new Point(100, 0);
                }

                button_control.BackColor = Color.FromArgb(255, 255, 255);
            }
        }

        private void button_mode_Click(object sender, EventArgs e)
        {
            if (!modeShowed)
            {
                modeShowed = true;

                area_mode.Visible = true;

                area_mode.BringToFront();

                if (controlShowed)
                {
                    area_mode.Location = new Point(500, 0);
                }

                else
                {
                    area_mode.Location = new Point(100, 0);
                }

                button_mode.BackColor = Color.FromArgb(224, 224, 224);
            }

            else
            {
                modeShowed = false;

                area_mode.Visible = false;

                button_mode.BackColor = Color.FromArgb(255, 255, 255);
            }
        }

        private void button_chart_Click(object sender, EventArgs e)
        {
            if (!chartShowed)
            {
                chartShowed = true;

                area_frameBox1.Visible = false;
                area_frameBox2.Visible = false;
                area_frameBox3.Visible = false;
                area_frameBox4.Visible = false;

                chart_timeProcess_System.Visible = true;
                chart_η.Visible = true;
                chart_errorClassification.Visible = true;
                chart_FAR.Visible = true;

                button_chart.BackColor = Color.FromArgb(224, 224, 224);
            }

            else
            {
                chartShowed = false;

                area_frameBox1.Visible = true;
                area_frameBox2.Visible = true;
                area_frameBox3.Visible = true;
                area_frameBox4.Visible = true;

                chart_timeProcess_System.Visible = false;
                chart_η.Visible = false;
                chart_errorClassification.Visible = false;
                chart_FAR.Visible = false;

                button_chart.BackColor = Color.FromArgb(255, 255, 255);
            }
        }

        private void button_start_Click(object sender, EventArgs e)
        {
            Thread.Sleep(2000);

            WebcamOn();

            numericUpDown_N.Enabled = false;

            numericUpDown_ρ.Enabled = false;
            numericUpDown_δ.Enabled = false;
            numericUpDown_τs.Enabled = false;
            numericUpDown_τh.Enabled = false;

            numericUpDown_SS.Enabled = false;
            numericUpDown_MF_W.Enabled = false;
            numericUpDown_FBS_W.Enabled = false;

            numericUpDown_LA.Enabled = false;

            numericUpDown_N.BackColor = Color.LightGray;
            numericUpDown_N.ForeColor = Color.DimGray;

            numericUpDown_ρ.BackColor = Color.LightGray;
            numericUpDown_ρ.ForeColor = Color.DimGray;
            numericUpDown_δ.BackColor = Color.LightGray;
            numericUpDown_δ.ForeColor = Color.DimGray;
            numericUpDown_τs.BackColor = Color.LightGray;
            numericUpDown_τs.ForeColor = Color.DimGray;
            numericUpDown_τh.BackColor = Color.LightGray;
            numericUpDown_τh.ForeColor = Color.DimGray;

            numericUpDown_SS.BackColor = Color.LightGray;
            numericUpDown_SS.ForeColor = Color.DimGray;
            numericUpDown_MF_W.BackColor = Color.LightGray;
            numericUpDown_MF_W.ForeColor = Color.DimGray;
            numericUpDown_FBS_W.BackColor = Color.LightGray;
            numericUpDown_FBS_W.ForeColor = Color.DimGray;

            numericUpDown_LA.BackColor = Color.LightGray;
            numericUpDown_LA.ForeColor = Color.DimGray;

            checkBox_RF.Enabled = true;
            checkBox_AD.Enabled = true;
            checkBox_M.Enabled = true;
            checkBox_PF.Enabled = true;

            button_start.Enabled = false;
            button_start.BackColor = Color.LightGray;
        }

        private void button_detail_Click(object sender, EventArgs e)
        {
            detail = new Detail();
            detail.Show();
        }

        private void button_clear_Click(object sender, EventArgs e)
        {
            dialogResult = MessageBox.Show("Are you sure want to clear all system data?",
                                            "Clear Data", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                Clear();
            }
        }

        private void button_reset_Click(object sender, EventArgs e)
        {
            dialogResult = MessageBox.Show("Are you sure want to reset the system?",
                                            "Restart", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            {
                webcam.Stop();
                Timer.Stop();

                counter = 0;
                coordinate = 0;
                R = 0;

                initialization = false;
                sh_Activated = false;
                cf_Activated = false;
                ls_Activated = false;
                full = true;
                ready_showDetail = false;
                updateInfo_Shown = false;

                RGBFrames.Clear();
                GrayFrames.Clear();

                timeProcess_System_List.Clear();

                opticus.Reset();

                Clear();

                numericUpDown_N.Enabled = true;

                numericUpDown_ρ.Enabled = true;
                numericUpDown_δ.Enabled = true;
                numericUpDown_τs.Enabled = true;
                numericUpDown_τh.Enabled = true;

                numericUpDown_SS.Enabled = true;
                numericUpDown_MF_W.Enabled = true;
                numericUpDown_FBS_W.Enabled = true;

                numericUpDown_LA.Enabled = true;

                numericUpDown_N.BackColor = Color.White;
                numericUpDown_N.ForeColor = Color.Black;

                numericUpDown_ρ.BackColor = Color.White;
                numericUpDown_ρ.ForeColor = Color.Black;
                numericUpDown_δ.BackColor = Color.White;
                numericUpDown_δ.ForeColor = Color.Black;
                numericUpDown_τs.BackColor = Color.White;
                numericUpDown_τs.ForeColor = Color.Black;
                numericUpDown_τh.BackColor = Color.White;
                numericUpDown_τh.ForeColor = Color.Black;

                numericUpDown_SS.BackColor = Color.White;
                numericUpDown_SS.ForeColor = Color.Black;
                numericUpDown_MF_W.BackColor = Color.White;
                numericUpDown_MF_W.ForeColor = Color.Black;
                numericUpDown_FBS_W.BackColor = Color.White;
                numericUpDown_FBS_W.ForeColor = Color.Black;

                numericUpDown_LA.BackColor = Color.White;
                numericUpDown_LA.ForeColor = Color.Black;

                numericUpDown_N.Value = 30;
                numericUpDown_DS.Value = 25;

                numericUpDown_ρ.Value = (decimal)0.2;
                numericUpDown_δ.Value = (decimal)1.0;
                numericUpDown_τs.Value = 20;
                numericUpDown_τh.Value = 150;

                numericUpDown_SS.Value = 10;
                numericUpDown_MF_W.Value = 3;
                numericUpDown_FBS_W.Value = 5;

                numericUpDown_LA.Value = 30;

                button_start.Enabled = true;
                button_detail.Enabled = false;
                button_clear.Enabled = false;
                button_reset.Enabled = false;

                button_start.BackColor = Color.FromArgb(0, 119, 234);
                button_detail.BackColor = Color.LightGray;
                button_clear.BackColor = Color.LightGray;
                button_reset.BackColor = Color.LightGray;

                pictureBox_RF.Image = null;
                pictureBox_AD.Image = null;
                pictureBox_DS.Image = null;
                pictureBox_S.Image = null;
                pictureBox_S_MF.Image = null;
                pictureBox_FBS.Image = null;
                pictureBox_S_MF_FBS.Image = null;
                pictureBox_M.Image = null;
                pictureBox_PF.Image = null;

                checkBox_sh_Activated.Checked = false;
                checkBox_sh_Activated.Enabled = false;

                checkBox_cf_Activated.Enabled = false;
                checkBox_cf_Activated.Checked = false;

                checkBox_ls_Activated.Checked = false;
                checkBox_ls_Activated.Enabled = false;

                checkBox_RF.Checked = false;
                checkBox_RF.Enabled = false;

                checkBox_AD.Checked = false;
                checkBox_AD.Enabled = false;

                checkBox_DS.Checked = false;
                checkBox_DS.Enabled = false;

                checkBox_S.Enabled = false;
                checkBox_S.Checked = false;

                checkBox_S_MF.Enabled = false;
                checkBox_S_MF.Checked = false;

                checkBox_FBS.Checked = false;
                checkBox_FBS.Enabled = false;

                checkBox_S_MF_FBS.Enabled = false;
                checkBox_S_MF_FBS.Checked = false;

                checkBox_M.Checked = false;
                checkBox_M.Enabled = false;

                checkBox_PF.Checked = false;
                checkBox_PF.Enabled = false;

                chart_timeProcess_System.Series[0].Points.Clear();
                chart_errorClassification.Series[0].Points.Clear();
                chart_FAR.Series[0].Points.Clear();
                chart_η.Series[0].Points.Clear();
            }
        }

        public void Clear()
        {
            for (int i = 0; i < 4; i++)
            {
                area_frameBoxes[i].Text = "Frame Box " + (i + 1).ToString();
                frameBoxes[i].Image = null;
            }

            foreach (Control p in this.Controls)
            {
                if (p is Panel)
                {
                    foreach (Control cb in p.Controls)
                    {
                        if (cb is CheckBox)
                        {
                            if (!sh_Activated)
                            {
                                checkBox_DS.Enabled = false;
                            }

                            if (!cf_Activated)
                            {
                                checkBox_S.Enabled = false;
                                checkBox_S_MF.Enabled = false;
                                checkBox_FBS.Enabled = false;
                                checkBox_S_MF_FBS.Enabled = false;
                            }

                            else
                            {
                                cb.Enabled = true;
                            }

                            ((CheckBox)cb).Checked = false;
                        }
                    }
                }
            }

            index1 = index2 = index3 = index4 = index5 = index6 = index7 = index8 = index9 = -1;

            chart_timeProcess_System.Series[0].Points.Clear();
            chart_errorClassification.Series[0].Points.Clear();
            chart_FAR.Series[0].Points.Clear();

            Transfer.timeProcess_System_List.Clear();
            Transfer.errorClassification_List.Clear();
            Transfer.FAR_List.Clear();

            if (sh_Activated)
            {
                chart_η.Series[0].Points.Clear();

                Transfer.η_List.Clear();
            }

            coordinate = 0;
        }

        /*---------------------------------------------------------------------------------------------------------*/

        /*-------------------------------------------------Form UI-------------------------------------------------*/

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                webcam.Stop();
            }

            catch (Exception) { }
        }

        /*---------------------------------------------------------------------------------------------------------*/

        /*---------------------------------------------------------------------------------------------------------*/
    }
}