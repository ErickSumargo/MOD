using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;

namespace Opticus
{
    class QualityMeasurement
    {
        /*----------------------------------------Declaring Local Variables-----------------------------------------*/
        
        int TP, FP;
        int active;
        int areaBounding;
        int step;
        
        double correct, insertion;
        double errorClassification, FAR;
        double η;

        byte[] pixels_Binary_target;

        /*----------------------------------------------------------------------------------------------------------*/

        /*------------------------------------------Declaring Main Classes------------------------------------------*/

        ConnectedComponentLabelling connectedComponentLabelling;

        /*----------------------------------------------------------------------------------------------------------*/

        /*-------------------------------------------Declaring SubClasses-------------------------------------------*/

        LockBitmap lbm_Binary_target;

        List<int> xMin, yMin, xMax, yMax;

        /*----------------------------------------------------------------------------------------------------------*/

        public QualityMeasurement()
        {
            connectedComponentLabelling = new ConnectedComponentLabelling();

            Transfer.errorClassification_List = new List<double>();
            Transfer.FAR_List = new List<double>();
            Transfer.η_List = new List<double>();

            xMin = new List<int>(); yMin = new List<int>();
            xMax = new List<int>(); yMax = new List<int>();
        }

        public void ROIDetection(Bitmap Binary_target)
        {
            TP = 0; FP = 0;
            correct = 0; insertion = 0;

            connectedComponentLabelling.Process(Binary_target);

            lbm_Binary_target = new LockBitmap(Binary_target);
            lbm_Binary_target.LockBits();

            step = lbm_Binary_target.step;

            pixels_Binary_target = lbm_Binary_target.Pixels;

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
                        ROIDetectionThreaded(xMin[xMin.Count - 1], yMin[yMin.Count - 1], xMax[xMax.Count - 1] / 2, yMax[yMax.Count - 1] / 2);
                    },
                    () =>
                    {
                        ROIDetectionThreaded(xMax[xMax.Count - 1] / 2, yMin[yMin.Count - 1], xMax[xMax.Count - 1], yMax[yMax.Count - 1] / 2);
                    },
                    () =>
                    {
                        ROIDetectionThreaded(xMin[xMin.Count - 1], yMax[yMax.Count - 1] / 2, xMax[xMax.Count - 1] / 2, yMax[yMax.Count - 1]);
                    },
                    () =>
                    {
                        ROIDetectionThreaded(xMax[xMax.Count - 1] / 2, yMax[yMax.Count - 1] / 2, xMax[xMax.Count - 1], yMax[yMax.Count - 1]);
                    }
                );

                if ((double)active / (double)areaBounding >= 0.25)
                {
                    correct += active;
                    TP++;
                }

                else
                {
                    if (active > 0)
                    {
                        insertion += active;
                        FP++;
                    }
                }
            }

            lbm_Binary_target.UnlockBits();

            xMin.Clear(); yMin.Clear();
            xMax.Clear(); yMax.Clear();

            ROIQuality();
        }

        public void ROIDetectionThreaded(int xStart, int yStart, int xEnd, int yEnd)
        {
            for (int i = xStart; i < xEnd; i++)
            {
                for (int j = yStart; j < yEnd; j++)
                {
                    int layer = ((j * Transfer.sizeX) + i) * step;

                    if (pixels_Binary_target[layer] == 255)
                    {
                        active++;
                    }
                }
            }
        }

        public void ROIQuality()
        {
            if (!Transfer.checkingShadow)
            {
                if (correct == 0 && insertion == 0)
                {
                    errorClassification = 0;
                }

                else
                {
                    errorClassification = ((double)insertion / (double)(correct + insertion)) * 100;
                }

                if (TP == 0 && FP == 0)
                {
                    FAR = 0;
                }

                else
                {
                    FAR = ((double)FP / (double)(TP + FP)) * 100;
                }
            }

            else
            {
                if (correct == 0 && insertion == 0)
                {
                    η = 100;
                }

                else
                {
                    η = ((double)correct / (double)(correct + insertion)) * 100;
                }
            }

            QualityResultTransfering();
        }

        public void QualityResultTransfering()
        {
            if (Transfer.checkingLOTS)
            {
                Transfer.TP_LOTS = TP;
                Transfer.FP_LOTS = FP;
                Transfer.correct_LOTS = correct;
                Transfer.insertion_LOTS = insertion;
                Transfer.errorClassification_LOTS = errorClassification;
                Transfer.FAR_LOTS = FAR;
            }

            else if (Transfer.checkingShadow)
            {
                Transfer.TP_Shadow = TP;
                Transfer.FP_Shadow = FP;
                Transfer.correct_Shadow = correct;
                Transfer.insertion_Shadow = insertion;
                Transfer.η = η;

                Transfer.η_List.Add(η);
            }

            else if (Transfer.checkingCamouflage)
            {
                if (Transfer.checkingSobel)
                {
                    Transfer.TP_Sobel = TP;
                    Transfer.FP_Sobel = FP;
                    Transfer.correct_Sobel = correct;
                    Transfer.insertion_Sobel = insertion;
                    Transfer.errorClassification_Sobel = errorClassification;
                    Transfer.FAR_Sobel = FAR;
                }

                else if (Transfer.checkingFBS)
                {

                    Transfer.TP_FBS = TP;
                    Transfer.FP_FBS = FP;
                    Transfer.correct_FBS = correct;
                    Transfer.insertion_FBS = insertion;
                    Transfer.errorClassification_FBS = errorClassification;
                    Transfer.FAR_FBS = FAR;
                }

                else if (Transfer.checkingSobel_FBS)
                {
                    Transfer.TP_Sobel_FBS = TP;
                    Transfer.FP_Sobel_FBS = FP;
                    Transfer.correct_Sobel_FBS = correct;
                    Transfer.insertion_Sobel_FBS = insertion;
                    Transfer.errorClassification_Sobel_FBS = errorClassification;
                    Transfer.FAR_Sobel_FBS = FAR;
                }
            }

            else if (Transfer.checkingMerge)
            {
                Transfer.TP_Merge = TP;
                Transfer.FP_Merge = FP;
                Transfer.correct_Merge = correct;
                Transfer.insertion_Merge = insertion;
                Transfer.errorClassification_Merge = errorClassification;
                Transfer.FAR_Merge = FAR;

                Transfer.errorClassification_List.Add(errorClassification);
                Transfer.FAR_List.Add(FAR);
            }
        }
    }
}