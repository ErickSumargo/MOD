using System;
using System.Threading.Tasks;
using System.Drawing;

namespace Opticus
{
    class ShadowDetection
    {
        /*----------------------------------------Declaring Local Variables-----------------------------------------*/

        int sizeX, sizeY;
        int step;

        byte[] pixels_Binary_DS;
        byte[] pixels_Binary_SR;
        byte[] pixels_Binary_AD;
        byte[] pixels_HSL_B;
        byte[] pixels_HSL_RF;

        double ρ, δ, τs, τh;

        /*----------------------------------------------------------------------------------------------------------*/

        /*-------------------------------------------Declaring SubClasses-------------------------------------------*/

        Bitmap HSL_B, HSL_RF, Binary_AD, Binary_DS, Binary_SR;

        LockBitmap lbm_HSL_B, lbm_HSL_RF, lbm_Binary_AD, lbm_Binary_DS, lbm_Binary_SR;

        Canvas canvas;

        /*----------------------------------------------------------------------------------------------------------*/

        public ShadowDetection()
        {
            sizeX = Transfer.sizeX;
            sizeY = Transfer.sizeY;

            HSL_B = new Bitmap(Transfer.HSL_B);

            canvas = new Canvas();
        }

        public void Detect()
        {
            LoadShadowDetectionValue();

            lbm_Binary_DS = new LockBitmap(Binary_DS);
            lbm_Binary_DS.LockBits();

            lbm_Binary_SR = new LockBitmap(Binary_SR);
            lbm_Binary_SR.LockBits();

            lbm_Binary_AD = new LockBitmap(Binary_AD);
            lbm_Binary_AD.LockBits();

            lbm_HSL_B = new LockBitmap(HSL_B);
            lbm_HSL_B.LockBits();

            lbm_HSL_RF = new LockBitmap(HSL_RF);
            lbm_HSL_RF.LockBits();

            step = lbm_Binary_DS.step;

            pixels_Binary_DS = lbm_Binary_DS.Pixels;
            pixels_Binary_SR = lbm_Binary_SR.Pixels;
            pixels_Binary_AD = lbm_Binary_AD.Pixels;
            pixels_HSL_RF = lbm_HSL_RF.Pixels;
            pixels_HSL_B = lbm_HSL_B.Pixels;

            Parallel.Invoke(
                () =>
                {
                    DetectThreaded(0, 0, sizeX / 2, sizeY / 2);
                },
                () =>
                {
                    DetectThreaded(sizeX / 2, 0, sizeX, sizeY / 2);
                },
                () =>
                {
                    DetectThreaded(0, sizeY / 2, sizeX / 2, sizeY);
                },
                () =>
                {
                    DetectThreaded(sizeX / 2, sizeY / 2, sizeX, sizeY);
                }
            );

            lbm_Binary_DS.UnlockBits();
            lbm_Binary_SR.UnlockBits();
            lbm_Binary_AD.UnlockBits();
            lbm_HSL_RF.UnlockBits();
            lbm_HSL_B.UnlockBits();

            Transfer.Binary_DS = Binary_DS;
            Transfer.Binary_SR = Binary_SR;
        }

        public void DetectThreaded(int xStart, int yStart, int xEnd, int yEnd)
        {
            double illuminationBackground = 255;

            for (int i = xStart; i < xEnd; i++)
            {
                for (int j = yStart; j < yEnd; j++)
                {
                    int layer = ((j * sizeX) + i) * step;

                    if (pixels_Binary_AD[layer] == 255)
                    {
                        if (pixels_HSL_B[layer] > 0)
                        {
                            illuminationBackground = pixels_HSL_B[layer];
                        }

                        if (((pixels_HSL_RF[layer] / illuminationBackground >= ρ) && (pixels_HSL_RF[layer] / illuminationBackground <= δ))
                              && (Math.Abs(pixels_HSL_RF[layer + 1] - pixels_HSL_B[layer + 1]) <= τs)
                              && (Math.Abs(pixels_HSL_RF[layer + 2] - pixels_HSL_B[layer + 2]) <= τh))
                        {
                            pixels_Binary_DS[layer + 2] = 255;
                            pixels_Binary_DS[layer] = pixels_Binary_DS[layer + 1] = 0;

                            pixels_Binary_SR[layer] = pixels_Binary_SR[layer + 1] = pixels_Binary_SR[layer + 2] = 255;
                        }
                    }
                }
            }
        }

        public void LoadShadowDetectionValue()
        {
            ρ = Transfer.ρ;
            δ = Transfer.δ;
            τs = Transfer.τs;
            τh = Transfer.τh;

            Binary_SR = canvas.Blank(sizeX, sizeY);
            Binary_DS = new Bitmap(Transfer.Binary_AD);
            Binary_AD = new Bitmap(Transfer.Binary_AD);

            HSL_RF = new Bitmap(Transfer.HSL_RF);
        }
    }
}