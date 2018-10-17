using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;

namespace Opticus
{
    class Opticus
    {
        /*----------------------------------------Declaring Local Variables-----------------------------------------*/

        int sizeX, sizeY;
        int N;
        int DS;
        int SS;
        int areaBounding;
        int active;
        int step;

        int[,] TD;
        int[,] TS;

        double PC;
        double alpha;

        byte[] pixels_Gray_AD;
        byte[] pixels_Gray_B;
        byte[] pixels_Gray_RF;
        byte[] pixels_Binary_SR;
        byte[] pixels_Binary_SL;
        byte[] pixels_Binary_CL;
        byte[] pixels_Binary_S;
        byte[] pixels_Binary_FBS;
        byte[] pixels_Binary_M;

        /*----------------------------------------------------------------------------------------------------------*/

        /*------------------------------------------Declaring Main Classes------------------------------------------*/

        ColorConversion colorConversion;

        Segmentation segmentation;

        Morphology morphology;

        ShadowDetection shadowDetection;

        ConnectedComponentLabelling connectedComponentLabelling;

        Correlation correlation;

        /*----------------------------------------------------------------------------------------------------------*/

        /*-------------------------------------------Declaring SubClasses-------------------------------------------*/

        Bitmap RF, RGB_B, RGB_RF, HSL_B, HSL_RF, Gray_B, Gray_RF, Gray_AD, Gray_S,
               Binary_AD, Binary_DS, Binary_SR, Binary_SL, Binary_S, Binary_FBS, Binary_CL, Binary_M, Binary_CCL, PF;

        LockBitmap lbm_RGB_B, lbm_Gray_B, lbm_Gray_RF, lbm_Gray_AD, lbm_Binary_SR, lbm_Binary_SL,
                   lbm_Binary_S, lbm_Binary_FBS, lbm_Binary_CL, lbm_Binary_M;

        Canvas canvas;

        Graphics graphics;

        Pen pen;
        Pen pen_TP, pen_FP;

        List<Bitmap> RGBFrames, GrayFrames;

        List<int> RIntensity, GIntensity, BIntensity;
        List<int> GrayIntensity;
        List<int> BinaryIntensity;

        List<int> xMin, yMin, xMax, yMax;

        List<double> PC_List;

        /*----------------------------------------------------------------------------------------------------------*/

        public Opticus()
        {
            /*---------------------------------------Defining Local Variables---------------------------------------*/

            sizeX = Transfer.sizeX;
            sizeY = Transfer.sizeY;
            DS = Transfer.DS;
            SS = Transfer.SS;

            TD = new int[sizeX, sizeY];
            TS = new int[sizeX, sizeY];

            /*------------------------------------------------------------------------------------------------------*/

            /*-----------------------------------------Defining Main Classes----------------------------------------*/

            colorConversion = new ColorConversion();

            connectedComponentLabelling = new ConnectedComponentLabelling();

            /*------------------------------------------------------------------------------------------------------*/

            /*-----------------------------------------Defining Sub Classes-----------------------------------------*/

            canvas = new Canvas();

            RGB_B = canvas.Blank(sizeX, sizeY);
            Gray_B = canvas.Blank(sizeX, sizeY);
            Gray_AD = canvas.Blank(sizeX, sizeY);
            Gray_S = canvas.Blank(sizeX, sizeY);
            Binary_AD = canvas.Blank(sizeX, sizeY);
            Binary_DS = canvas.Blank(sizeX, sizeY);
            Binary_SR = canvas.Blank(sizeX, sizeY);
            Binary_S = canvas.Blank(sizeX, sizeY);
            Binary_FBS = canvas.Blank(sizeX, sizeY);
            Binary_CL = canvas.Blank(sizeX, sizeY);
            Binary_M = canvas.Blank(sizeX, sizeY);

            canvas = new Canvas();

            pen_TP = new Pen(Color.Aqua, 1.0f);
            pen_FP = new Pen(Color.Red, 1.0f);

            RIntensity = new List<int>();
            GIntensity = new List<int>();
            BIntensity = new List<int>();
            GrayIntensity = new List<int>();
            BinaryIntensity = new List<int>();

            xMin = new List<int>();
            yMin = new List<int>();
            xMax = new List<int>();
            yMax = new List<int>();

            PC_List = new List<double>();

            /*------------------------------------------------------------------------------------------------------*/
        }

        /*------------------------------------------Initialization Process------------------------------------------*/

        public void Initialization()
        {
            LoadInitializationValue();

            lbm_RGB_B = new LockBitmap(RGB_B);
            lbm_RGB_B.LockBits();

            lbm_Gray_B = new LockBitmap(Gray_B);
            lbm_Gray_B.LockBits();

            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    for (int k = 0; k < N; k++)
                    {
                        Color c_RGBFrames = RGBFrames[k].GetPixel(i, j);
                        RIntensity.Add(c_RGBFrames.R);
                        GIntensity.Add(c_RGBFrames.G);
                        BIntensity.Add(c_RGBFrames.B);

                        Color c_GrayFrames = GrayFrames[k].GetPixel(i, j);
                        GrayIntensity.Add((c_GrayFrames.R + c_GrayFrames.G + c_GrayFrames.B) / 3);
                    }

                    lbm_RGB_B.SetPixel(i, j, Color.FromArgb(RIntensity.Min(), GIntensity.Min(), BIntensity.Min()));
                    lbm_Gray_B.SetPixel(i, j, Color.FromArgb(GrayIntensity.Min(), GrayIntensity.Min(), GrayIntensity.Min()));

                    TD[i, j] = Math.Abs(GrayIntensity.Max() - GrayIntensity.Min());
                    TS[i, j] = TD[i, j];

                    RIntensity.Clear();
                    GIntensity.Clear();
                    BIntensity.Clear();

                    GrayIntensity.Clear();
                }
            }

            lbm_RGB_B.UnlockBits();
            lbm_Gray_B.UnlockBits();

            HSL_B = new Bitmap(RGB_B);
            colorConversion.HSL(HSL_B);

            Transfer.TD = TD;
            Transfer.TS = TS;

            Transfer.RGB_B = RGB_B;
            Transfer.HSL_B = HSL_B;
            Transfer.Gray_B = Gray_B;
        }

        public void LoadInitializationValue()
        {
            N = Transfer.N;

            RGBFrames = Transfer.RGBFrames;
            GrayFrames = Transfer.GrayFrames;
        }

        /*----------------------------------------------------------------------------------------------------------*/

        /*---------------------------------------------Detection Process--------------------------------------------*/

        public void Detection()
        {
            LoadDetectionValue();

            lbm_Gray_AD = new LockBitmap(Gray_AD);
            lbm_Gray_AD.LockBits();

            lbm_Gray_B = new LockBitmap(Gray_B);
            lbm_Gray_B.LockBits();

            lbm_Gray_RF = new LockBitmap(Gray_RF);
            lbm_Gray_RF.LockBits();

            step = lbm_Gray_AD.step;

            pixels_Gray_AD = lbm_Gray_AD.Pixels;
            pixels_Gray_B = lbm_Gray_B.Pixels;
            pixels_Gray_RF = lbm_Gray_RF.Pixels;

            Parallel.Invoke(
                () =>
                {
                    DetectionThreaded(0, 0, sizeX / 2, sizeY / 2);
                },
                () =>
                {
                    DetectionThreaded(sizeX / 2, 0, sizeX, sizeY / 2);
                },
                () =>
                {
                    DetectionThreaded(0, sizeY / 2, sizeX / 2, sizeY);
                },
                () =>
                {
                    DetectionThreaded(sizeX / 2, sizeY / 2, sizeX, sizeY);
                }
            );

            lbm_Gray_AD.UnlockBits();
            lbm_Gray_B.UnlockBits();
            lbm_Gray_RF.UnlockBits();

            Binary_AD = new Bitmap(Gray_AD);
            colorConversion.BinaryScale_AbsoluteDifference(Binary_AD);

            Transfer.Gray_AD = Gray_AD;
            Transfer.Binary_AD = Binary_AD;
        }

        private void DetectionThreaded(int xStart, int yStart, int xEnd, int yEnd)
        {
            for (int i = xStart; i < xEnd; i++)
            {
                for (int j = yStart; j < yEnd; j++)
                {
                    int layer = ((j * sizeX) + i) * step;

                    int absoluteValue = Math.Abs(pixels_Gray_B[layer] - pixels_Gray_RF[layer]);

                    pixels_Gray_AD[layer] = pixels_Gray_AD[layer + 1] = pixels_Gray_AD[layer + 2] = (byte)absoluteValue;
                }
            }
        }

        public void LoadDetectionValue()
        {
            RF = new Bitmap(Transfer.RF);
            RGB_RF = new Bitmap(Transfer.RGB_RF);
            Gray_RF = new Bitmap(Transfer.Gray_RF);
        }

        /*----------------------------------------------------------------------------------------------------------*/

        /*-----------------------------------------Shadow Detection Process-----------------------------------------*/

        public void ShadowDetection()
        {
            if (!Transfer.initialization_shadowDetection)
            {
                shadowDetection = new ShadowDetection();

                Transfer.initialization_shadowDetection = true;
            }

            HSL_RF = new Bitmap(RGB_RF);
            colorConversion.HSL(HSL_RF);

            CurrentShadowDetectionValueTransfering();

            shadowDetection.Detect();

            ShadowlessResult();
        }

        public void CurrentShadowDetectionValueTransfering()
        {
            Transfer.HSL_RF = HSL_RF;
        }

        public void ShadowlessResult()
        {
            Binary_SL = new Bitmap(Binary_AD);

            lbm_Binary_SL = new LockBitmap(Binary_SL);
            lbm_Binary_SL.LockBits();

            lbm_Binary_SR = new LockBitmap(Transfer.Binary_SR);
            lbm_Binary_SR.LockBits();

            step = lbm_Binary_SL.step;

            pixels_Binary_SL = lbm_Binary_SL.Pixels;
            pixels_Binary_SR = lbm_Binary_SR.Pixels;

            Parallel.Invoke(
                () =>
                {
                    ShadowlessResultThreaded(0, 0, sizeX / 2, sizeY / 2);
                },
                () =>
                {
                    ShadowlessResultThreaded(sizeX / 2, 0, sizeX, sizeY / 2);
                },
                () =>
                {
                    ShadowlessResultThreaded(0, sizeY / 2, sizeX / 2, sizeY);
                },
                () =>
                {
                    ShadowlessResultThreaded(sizeX / 2, sizeY / 2, sizeX, sizeY);
                }
            );

            lbm_Binary_SR.UnlockBits();
            lbm_Binary_SL.UnlockBits();

            Transfer.Binary_SL = Binary_SL;
        }

        public void ShadowlessResultThreaded(int xStart, int yStart, int xEnd, int yEnd)
        {
            for (int i = xStart; i < xEnd; i++)
            {
                for (int j = yStart; j < yEnd; j++)
                {
                    int layer = (j * sizeX + i) * step;

                    if (pixels_Binary_SR[layer] == 255)
                    {
                        pixels_Binary_SL[layer] = pixels_Binary_SL[layer + 1] = pixels_Binary_SL[layer + 2] = 0;
                    }
                }
            }
        }

        /*----------------------------------------------------------------------------------------------------------*/

        /*-----------------------------------------------Sobel Process----------------------------------------------*/

        public void Segmentation()
        {
            if (!Transfer.initialization_Sobel)
            {
                segmentation = new Segmentation();

                Transfer.initialization_Sobel = true;
            }

            segmentation.Sobel();
        }

        /*----------------------------------------------------------------------------------------------------------*/

        /*------------------------------------------------FBS Process-----------------------------------------------*/

        public void Morphology()
        {
            if (!Transfer.initialization_FBS)
            {
                morphology = new Morphology();

                Transfer.initialization_FBS = true;
            }

            morphology.Dilation();
        }

        public void CamouflagelessResult()
        {
            Binary_CL = new Bitmap(Binary_AD);

            lbm_Binary_CL = new LockBitmap(Binary_CL);
            lbm_Binary_CL.LockBits();

            lbm_Binary_S = new LockBitmap(Transfer.Binary_S);
            lbm_Binary_S.LockBits();

            lbm_Binary_FBS = new LockBitmap(Transfer.Binary_FBS);
            lbm_Binary_FBS.LockBits();

            step = lbm_Binary_CL.step;

            pixels_Binary_CL = lbm_Binary_CL.Pixels;
            pixels_Binary_S = lbm_Binary_S.Pixels;
            pixels_Binary_FBS = lbm_Binary_FBS.Pixels;

            Parallel.Invoke(
                () =>
                {
                    CamouflagelessResultThreaded(0, 0, sizeX / 2, sizeY / 2);
                },
                () =>
                {
                    CamouflagelessResultThreaded(sizeX / 2, 0, sizeX, sizeY / 2);
                },
                () =>
                {
                    CamouflagelessResultThreaded(0, sizeY / 2, sizeX / 2, sizeY);
                },
                () =>
                {
                    CamouflagelessResultThreaded(sizeX / 2, sizeY / 2, sizeX, sizeY);
                }
            );

            lbm_Binary_CL.UnlockBits();
            lbm_Binary_S.UnlockBits();
            lbm_Binary_FBS.UnlockBits();

            Transfer.Binary_CL = Binary_CL;
        }

        public void CamouflagelessResultThreaded(int xStart, int yStart, int xEnd, int yEnd)
        {
            for (int i = xStart; i < xEnd; i++)
            {
                for (int j = yStart; j < yEnd; j++)
                {
                    int layer = (j * sizeX + i) * step;

                    if (pixels_Binary_S[layer] == 255 || pixels_Binary_FBS[layer] == 255)
                    {
                        pixels_Binary_CL[layer] = pixels_Binary_CL[layer + 1] = pixels_Binary_CL[layer + 2] = 255;
                    }
                }
            }
        }

        /*----------------------------------------------------------------------------------------------------------*/

        /*-----------------------------------------------Merge Process----------------------------------------------*/

        public void Merge()
        {
            if (Transfer.sh_Activated && Transfer.cf_Activated)
            {
                Binary_M = canvas.Blank(sizeX, sizeY);

                lbm_Binary_M = new LockBitmap(Binary_M);
                lbm_Binary_M.LockBits();

                lbm_Binary_SR = new LockBitmap(Transfer.Binary_SR);
                lbm_Binary_SR.LockBits();

                lbm_Binary_CL = new LockBitmap(Binary_CL);
                lbm_Binary_CL.LockBits();

                step = lbm_Binary_M.step;

                pixels_Binary_M = lbm_Binary_M.Pixels;
                pixels_Binary_SR = lbm_Binary_SR.Pixels;
                pixels_Binary_CL = lbm_Binary_CL.Pixels;

                Parallel.Invoke(
                    () =>
                    {
                        MergeThreaded(0, 0, sizeX / 2, sizeY / 2);
                    },
                    () =>
                    {
                        MergeThreaded(sizeX / 2, 0, sizeX, sizeY / 2);
                    },
                    () =>
                    {
                        MergeThreaded(0, sizeY / 2, sizeX / 2, sizeY);
                    },
                    () =>
                    {
                        MergeThreaded(sizeX / 2, sizeY / 2, sizeX, sizeY);
                    }
                );

                lbm_Binary_M.UnlockBits();
                lbm_Binary_SR.UnlockBits();
                lbm_Binary_CL.UnlockBits();
            }

            else if (Transfer.sh_Activated && !Transfer.cf_Activated)
            {
                Binary_M = new Bitmap(Binary_SL);
            }

            else if (!Transfer.sh_Activated && Transfer.cf_Activated)
            {
                Binary_M = new Bitmap(Binary_CL);
            }

            else
            {
                Binary_M = new Bitmap(Binary_AD);
            }

            Transfer.Binary_M = Binary_M;
        }

        public void MergeThreaded(int xStart, int yStart, int xEnd, int yEnd)
        {
            for (int i = xStart; i < xEnd; i++)
            {
                for (int j = yStart; j < yEnd; j++)
                {
                    int layer = (j * sizeX + i) * step;

                    if (pixels_Binary_SR[layer] == 0 && pixels_Binary_CL[layer] == 255)
                    {
                        pixels_Binary_M[layer] = pixels_Binary_M[layer + 1] = pixels_Binary_M[layer + 2] = 255;
                    }
                }
            }
        }

        /*----------------------------------------------------------------------------------------------------------*/

        /*------------------------------------------------CCL Process-----------------------------------------------*/

        public void ConnectedComponentRegion()
        {
            PF = new Bitmap(RF);
            Binary_CCL = new Bitmap(Binary_M);

            connectedComponentLabelling.Process(Binary_M);

            lbm_Binary_M = new LockBitmap(Binary_M);
            lbm_Binary_M.LockBits();

            step = lbm_Binary_M.step;

            pixels_Binary_M = lbm_Binary_M.Pixels;

            foreach (var coordinate in connectedComponentLabelling.patterns)
            {
                xMin.Add(coordinate.Value.Min(x => x.Position.X));
                yMin.Add(coordinate.Value.Min(y => y.Position.Y));
                xMax.Add(coordinate.Value.Max(x => x.Position.X));
                yMax.Add(coordinate.Value.Max(y => y.Position.Y));

                areaBounding = (xMax[xMax.Count - 1] - xMin[xMin.Count - 1]) * (yMax[yMax.Count - 1] - yMin[yMin.Count - 1]);

                active = 0;

                Parallel.Invoke(
                    () =>
                    {
                        ConnectedComponentRegionThreaded(xMin[xMin.Count - 1], yMin[yMin.Count - 1], xMax[xMax.Count - 1] / 2, yMax[yMax.Count - 1] / 2);
                    },
                    () =>
                    {
                        ConnectedComponentRegionThreaded(xMax[xMax.Count - 1] / 2, yMin[yMin.Count - 1], xMax[xMax.Count - 1], yMax[yMax.Count - 1] / 2);
                    },
                    () =>
                    {
                        ConnectedComponentRegionThreaded(xMin[xMin.Count - 1], yMax[yMax.Count - 1] / 2, xMax[xMax.Count - 1] / 2, yMax[yMax.Count - 1]);
                    },
                    () =>
                    {
                        ConnectedComponentRegionThreaded(xMax[xMax.Count - 1] / 2, yMax[yMax.Count - 1] / 2, xMax[xMax.Count - 1], yMax[yMax.Count - 1]);
                    }
                );

                if ((double)active / (double)areaBounding >= 0.25)
                {
                    pen = pen_TP;
                }

                else
                {
                    pen = pen_FP;
                }

                using (graphics = Graphics.FromImage(Binary_CCL))
                {
                    graphics.DrawRectangle(pen, xMin[xMin.Count - 1], yMin[yMin.Count - 1],
                        xMax[xMax.Count - 1] - xMin[xMin.Count - 1],
                        yMax[yMax.Count - 1] - yMin[yMin.Count - 1]);
                }

                using (graphics = Graphics.FromImage(PF))
                {
                    graphics.DrawRectangle(pen, xMin[xMin.Count - 1], yMin[yMin.Count - 1],
                        xMax[xMax.Count - 1] - xMin[xMin.Count - 1],
                        yMax[yMax.Count - 1] - yMin[yMin.Count - 1]);
                }
            }

            lbm_Binary_M.UnlockBits();

            xMin.Clear(); yMin.Clear();
            xMax.Clear(); yMax.Clear();

            Transfer.Binary_CCL = Binary_CCL;
            Transfer.PF = PF;
        }

        public void ConnectedComponentRegionThreaded(int xStart, int yStart, int xEnd, int yEnd)
        {
            for (int i = xStart; i < xEnd; i++)
            {
                for (int j = yStart; j < yEnd; j++)
                {
                    int layer = ((j * sizeX) + i) * step;

                    if (pixels_Binary_M[layer] == 255)
                    {
                        active++;
                    }
                }
            }
        }

        /*----------------------------------------------------------------------------------------------------------*/

        /*----------------------------------------Pearson's Correlation Process-------------------------------------*/

        public void Correlation()
        {
            correlation = new Correlation();

            correlation.Pearson();

            PC = Transfer.PC;
            PC_List.Add(PC);

            Transfer.PC_List = PC_List;
        }

        public void BackgroundUpdate()
        {
            alpha = Transfer.alpha;

            lbm_Gray_B = new LockBitmap(Gray_B);
            lbm_Gray_B.LockBits();

            lbm_Gray_RF = new LockBitmap(Gray_RF);
            lbm_Gray_RF.LockBits();

            step = lbm_Gray_B.step;

            pixels_Gray_B = lbm_Gray_B.Pixels;
            pixels_Gray_RF = lbm_Gray_RF.Pixels;

            Parallel.Invoke(
                () =>
                {
                    BackgroundUpdateThreaded(0, 0, sizeX / 2, sizeY / 2);
                },
                () =>
                {
                    BackgroundUpdateThreaded(sizeX / 2, 0, sizeX, sizeY / 2);
                },
                () =>
                {
                    BackgroundUpdateThreaded(0, sizeY / 2, sizeX / 2, sizeY);
                },
                () =>
                {
                    BackgroundUpdateThreaded(sizeX / 2, sizeY / 2, sizeX, sizeY);
                }
            );

            lbm_Gray_B.UnlockBits();
            lbm_Gray_RF.UnlockBits();
        }

        public void BackgroundUpdateThreaded(int xStart, int yStart, int xEnd, int yEnd)
        {
            for (int i = xStart; i < xEnd; i++)
            {
                for (int j = yStart; j < yEnd; j++)
                {
                    int layer = (j * sizeX + i) * step;

                    double adaptValue = (1 - alpha) * pixels_Gray_B[layer] + alpha * pixels_Gray_RF[layer];

                    pixels_Gray_B[layer] = pixels_Gray_B[layer + 1] = pixels_Gray_B[layer + 2] = (byte)adaptValue;
                }
            }
        }

        /*----------------------------------------------------------------------------------------------------------*/

        /*-----------------------------------------------Reset Process----------------------------------------------*/

        public void Reset()
        {
            Transfer.initialization_shadowDetection = false;
            Transfer.initialization_Sobel = false;
            Transfer.initialization_FBS = false;

            Gray_AD = canvas.Blank(sizeX, sizeY);
            Binary_AD = canvas.Blank(sizeX, sizeY);
            Binary_M = canvas.Blank(sizeX, sizeY);
        }

        /*----------------------------------------------------------------------------------------------------------*/
    }
}