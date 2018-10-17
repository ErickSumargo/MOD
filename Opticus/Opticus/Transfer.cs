using System;
using System.Collections.Generic;
using System.Drawing;

namespace Opticus
{
    class Transfer
    {
        /*----------------------------------------Declaring Local Variables-----------------------------------------*/

        public static int sizeX, sizeY;
        public static int N;
        public static int DS;
        public static int SS;
        public static int MF_W;
        public static int FBS_W;
        public static int LA;
        public static int[,] TD;
        public static int[,] TS;

        public static double ρ;
        public static double δ;
        public static double τs;
        public static double τh;
        public static double PC;
        public static double alpha = 1.0;

        public static double TP_LOTS;
        public static double FP_LOTS;
        public static double correct_LOTS;
        public static double insertion_LOTS;
        public static double errorClassification_LOTS;
        public static double FAR_LOTS;

        public static double TP_Shadow;
        public static double FP_Shadow;
        public static double correct_Shadow;
        public static double insertion_Shadow;
        public static double η;

        public static double TP_Sobel;
        public static double FP_Sobel;
        public static double correct_Sobel;
        public static double insertion_Sobel;
        public static double errorClassification_Sobel;
        public static double FAR_Sobel;

        public static double TP_FBS;
        public static double FP_FBS;
        public static double correct_FBS;
        public static double insertion_FBS;
        public static double errorClassification_FBS;
        public static double FAR_FBS;

        public static double TP_Sobel_FBS;
        public static double FP_Sobel_FBS;
        public static double correct_Sobel_FBS;
        public static double insertion_Sobel_FBS;
        public static double errorClassification_Sobel_FBS;
        public static double FAR_Sobel_FBS;

        public static double TP_Merge;
        public static double FP_Merge;
        public static double correct_Merge;
        public static double insertion_Merge;
        public static double errorClassification_Merge;
        public static double FAR_Merge;

        public static bool initialization_shadowDetection = false;
        public static bool initialization_Sobel = false;
        public static bool initialization_FBS = false;
        public static bool sh_Activated;
        public static bool cf_Activated;
        public static bool ls_Activated;

        public static bool checkingLOTS = false;
        public static bool checkingShadow = false;
        public static bool checkingCamouflage = false;
        public static bool checkingSobel = false;
        public static bool checkingFBS = false;
        public static bool checkingSobel_FBS = false;
        public static bool checkingMerge = false;

        /*----------------------------------------------------------------------------------------------------------*/

        /*-------------------------------------------Declaring SubClasses-------------------------------------------*/

        public static Bitmap RGB_B;
        public static Bitmap HSL_B;
        public static Bitmap Gray_B;

        public static Bitmap RF;
        public static Bitmap RGB_RF;
        public static Bitmap Gray_RF;
        public static Bitmap HSL_RF;

        public static Bitmap Gray_AD;
        public static Bitmap Binary_AD;

        public static Bitmap Gray_S;
        public static Bitmap Binary_S_N;
        public static Bitmap Binary_S;
        public static Bitmap Binary_FBS;
        public static Bitmap Binary_CL;

        public static Bitmap Binary_DS;
        public static Bitmap Binary_SR;
        public static Bitmap Binary_SL;

        public static Bitmap Binary_M;
        public static Bitmap Binary_CCL;

        public static Bitmap PF;

        public static List<Bitmap> RGBFrames;
        public static List<Bitmap> GrayFrames;

        public static List<double> timeProcess_System_List;
        public static List<double> errorClassification_List;
        public static List<double> FAR_List;
        public static List<double> η_List;
        public static List<double> PC_List;

        /*----------------------------------------------------------------------------------------------------------*/
    }
}