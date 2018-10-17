using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Drawing;

namespace Opticus
{
    class Morphology
    {
        /*----------------------------------------Declaring Local Variables-----------------------------------------*/

        int sizeX, sizeY;
        int FBS_W;
        int halfMetric;
        int DS;
        int step;

        int[,] T;

        byte[] pixels_Gray_AD;
        byte[] pixels_Binary_AD;
        byte[] pixels_Binary_FBS;

        /*----------------------------------------------------------------------------------------------------------*/

        /*------------------------------------------Declaring Main Classes------------------------------------------*/

        ConnectedComponentLabelling connectedComponentLabelling;

        /*----------------------------------------------------------------------------------------------------------*/

        /*-------------------------------------------Declaring SubClasses-------------------------------------------*/

        Bitmap Binary_FBS, Gray_AD, Binary_AD;

        LockBitmap lbm_Binary_FBS, lbm_Gray_AD, lbm_Binary_AD;

        List<int> GrayIntensity;

        List<int> xMin, yMin, xMax, yMax;

        /*----------------------------------------------------------------------------------------------------------*/

        public Morphology()
        {
            sizeX = Transfer.sizeX;
            sizeY = Transfer.sizeY;
            FBS_W = Transfer.FBS_W;
            T = Transfer.TD;

            GrayIntensity = new List<int>();

            xMin = new List<int>();
            yMin = new List<int>();
            xMax = new List<int>();
            yMax = new List<int>();

            connectedComponentLabelling = new ConnectedComponentLabelling();
        }

        public void Dilation()
        {
            LoadMorphologyValue();

            halfMetric = FBS_W / 2;

            connectedComponentLabelling.Process(Binary_AD);

            lbm_Binary_FBS = new LockBitmap(Binary_FBS);
            lbm_Binary_FBS.LockBits();

            lbm_Gray_AD = new LockBitmap(Gray_AD);
            lbm_Gray_AD.LockBits();

            lbm_Binary_AD = new LockBitmap(Binary_AD);
            lbm_Binary_AD.LockBits();

            step = lbm_Binary_FBS.step;

            pixels_Binary_FBS = lbm_Binary_FBS.Pixels;
            pixels_Gray_AD = lbm_Gray_AD.Pixels;
            pixels_Binary_AD = lbm_Binary_AD.Pixels;

            foreach (var coordinate in connectedComponentLabelling.patterns)
            {
                xMin.Add(coordinate.Value.Min(x => x.Position.X));
                yMin.Add(coordinate.Value.Min(y => y.Position.Y));
                xMax.Add(coordinate.Value.Max(x => x.Position.X));
                yMax.Add(coordinate.Value.Max(y => y.Position.Y));

                Parallel.Invoke(
                    () =>
                    {
                        FBSThreaded(xMin[xMin.Count - 1] + halfMetric, yMin[yMin.Count - 1] + halfMetric, xMax[xMax.Count - 1] / 2, yMax[yMax.Count - 1] / 2);
                    },
                    () =>
                    {
                        FBSThreaded(xMax[xMax.Count - 1] / 2, yMin[yMin.Count - 1] + halfMetric, xMax[xMax.Count - 1] - halfMetric, yMax[yMax.Count - 1] / 2);
                    },
                    () =>
                    {
                        FBSThreaded(xMin[xMin.Count - 1] + halfMetric, yMax[yMax.Count - 1] / 2, xMax[xMax.Count - 1] / 2, yMax[yMax.Count - 1] - halfMetric);
                    },
                    () =>
                    {
                        FBSThreaded(xMax[xMax.Count - 1] / 2, yMax[yMax.Count - 1] / 2, xMax[xMax.Count - 1] - halfMetric, yMax[yMax.Count - 1] - halfMetric);
                    }
                );

                xMin.Clear(); yMin.Clear();
                xMax.Clear(); yMax.Clear();
            }

            lbm_Binary_FBS.UnlockBits();
            lbm_Gray_AD.UnlockBits();
            lbm_Binary_AD.UnlockBits();

            Transfer.Binary_FBS = Binary_FBS;
        }

        public void FBSThreaded(int xStart, int yStart, int xEnd, int yEnd)
        {
            int layer = -1;

            List<int> GrayIntensity = new List<int>();

            for (int i = xStart; i < xEnd; i++)
            {
                for (int j = yStart; j < yEnd; j++)
                {
                    int layer_AD = ((j * sizeX) + i) * step;

                    if (pixels_Binary_AD[layer_AD] == 0)
                    {
                        for (int k = -halfMetric; k <= halfMetric; k++)
                        {
                            for (int l = -halfMetric; l <= halfMetric; l++)
                            {
                                layer = ((j + l) * sizeX + (i + k)) * step;

                                GrayIntensity.Add(pixels_Gray_AD[layer]);
                            }
                        }

                        if (GrayIntensity.Max() >= T[i, j] + DS)
                        {
                            pixels_Binary_FBS[layer] = pixels_Binary_FBS[layer + 1] = pixels_Binary_FBS[layer + 2] = 255;
                        }

                        GrayIntensity.Clear();
                    }
                }
            }
        }

        public void LoadMorphologyValue()
        {
            Gray_AD = new Bitmap(Transfer.Gray_AD);

            Binary_AD = new Bitmap(Transfer.Binary_AD);
            Binary_FBS = new Bitmap(Transfer.Binary_AD);

            DS = Transfer.DS;
        }
    }
}