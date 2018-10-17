using System;
using System.Threading.Tasks;
using System.Drawing;

namespace Opticus
{
    class Segmentation
    {
        /*----------------------------------------Declaring Local Variables-----------------------------------------*/

        int sizeX, sizeY;
        int MF_W;
        int step;

        byte[] pixels_Gray_AD;
        byte[] pixels_Gray_S;

        /*----------------------------------------------------------------------------------------------------------*/

        /*------------------------------------------Declaring Main Classes------------------------------------------*/

        ColorConversion colorConversion;

        Restoration restoration;

        /*----------------------------------------------------------------------------------------------------------*/

        /*-------------------------------------------Declaring SubClasses-------------------------------------------*/

        Bitmap Gray_AD, Gray_S, Binary_S_N, Binary_S;

        LockBitmap lbm_Gray_AD, lbm_Gray_S;

        Canvas canvas;

        /*----------------------------------------------------------------------------------------------------------*/

        public Segmentation()
        {
            sizeX = Transfer.sizeX; sizeY = Transfer.sizeY;
            MF_W = Transfer.MF_W;

            colorConversion = new ColorConversion();

            restoration = new Restoration();

            canvas = new Canvas();
        }

        public void Sobel()
        {
            LoadSegmentationValue();

            lbm_Gray_S = new LockBitmap(Gray_S);
            lbm_Gray_S.LockBits();

            lbm_Gray_AD = new LockBitmap(Gray_AD);
            lbm_Gray_AD.LockBits();

            step = lbm_Gray_S.step;

            pixels_Gray_S = lbm_Gray_S.Pixels;
            pixels_Gray_AD = lbm_Gray_AD.Pixels;

            Parallel.Invoke(
                () =>
                {
                    SobelThreaded(1, 1, sizeX / 2, sizeY / 2);
                },
                () =>
                {
                    SobelThreaded(sizeX / 2, 1, sizeX - 1, sizeY / 2);
                },
                () =>
                {
                    SobelThreaded(1, sizeY / 2, sizeX / 2, sizeY - 1);
                },
                () =>
                {
                    SobelThreaded(sizeX / 2, sizeY / 2, sizeX - 1, sizeY - 1);
                }
            );

            lbm_Gray_S.UnlockBits();
            lbm_Gray_AD.UnlockBits();

            Binary_S = new Bitmap(Gray_S);
            colorConversion.BinaryScale_Segmentation(Binary_S);

            Binary_S_N = new Bitmap(Binary_S);

            Restoration();

            Transfer.Gray_S = Gray_S;
            Transfer.Binary_S_N = Binary_S_N;
            Transfer.Binary_S = Binary_S;
        }

        private void SobelThreaded(int xStart, int yStart, int xEnd, int yEnd)
        {
            for (int i = xStart; i < xEnd; i++)
            {
                for (int j = yStart; j < yEnd; j++)
                {
                    int layer0 = (j * sizeX + i) * step;
                    int layer1 = ((j - 1) * sizeX + (i + 1)) * step;
                    int layer2 = (j * sizeX + (i + 1)) * step;
                    int layer3 = ((j + 1) * sizeX + (i + 1)) * step;
                    int layer4 = ((j - 1) * sizeX + (i - 1)) * step;
                    int layer5 = (j * sizeX + (i - 1)) * step;
                    int layer6 = ((j + 1) * sizeX + (i - 1)) * step;
                    int layer7 = ((j - 1) * sizeX + i) * step;
                    int layer8 = ((j + 1) * sizeX + i) * step;

                    int sobel = (int)Math.Sqrt(Math.Pow(pixels_Gray_AD[layer1] + 2 * pixels_Gray_AD[layer2] + pixels_Gray_AD[layer3]
                                                - pixels_Gray_AD[layer4] - 2 * pixels_Gray_AD[layer5] - pixels_Gray_AD[layer6], 2)
                                                + Math.Pow(pixels_Gray_AD[layer4] + 2 * pixels_Gray_AD[layer7] + pixels_Gray_AD[layer1]
                                                - pixels_Gray_AD[layer6] - 2 * pixels_Gray_AD[layer8] - pixels_Gray_AD[layer3], 2));

                    if (sobel > 255)
                    {
                        sobel = pixels_Gray_AD[layer0];
                    }

                    pixels_Gray_S[layer0] = pixels_Gray_S[layer0 + 1] = pixels_Gray_S[layer0 + 2] = (byte)sobel;
                }
            }
        }

        public void LoadSegmentationValue()
        {
            Gray_S = canvas.Blank(sizeX, sizeY);
            Binary_S = canvas.Blank(sizeX, sizeY);

            Gray_AD = new Bitmap(Transfer.Gray_AD);
        }

        public void Restoration()
        {
            restoration.MedianFilter(ref Binary_S, MF_W);
        }
    }
}