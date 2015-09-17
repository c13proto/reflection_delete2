using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCvSharp;

namespace 反射光除去処理
{
    class manualCV
    {
        public void 自作反射光除去(Mat[] images, ref Mat DST)
        {
            int width = images[0].Width;
            int height = images[0].Height;
            //var indexer = DST.GetGenericIndexer<Vec3b>();

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {//medianX,Y SG完成
                    Vec3b color = DST.Get<Vec3b>(y, x);//indexer[y, x];
                    double[] vals = { 0, 0, 0, 0 };
                    for (int num = 0; num < 4; num++) vals[num] = images[num].At<Vec3b>(y,x).Item0;
                    Array.Sort(vals);//並び替えを行う．min=vals[0]
                    color.Item0 =(byte)( (vals[0] + vals[1] + vals[2]) / 3.0);
                    DST.Set<Vec3b>(y, x, color);
                    //indexer[y, x] = color;

                }

        }
        public void 吉岡反射光除去処理(Mat[] images, ref Mat DST,int th_l,int th_h)
        {
            int width = images[0].Width;
            int height = images[0].Height;
            //var indexer = DST.GetGenericIndexer<Vec3b>();

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {//medianX,Y SG完成
                    Vec3b color = DST.Get<Vec3b>(y, x);//indexer[y, x];
                    double[] vals = { 0, 0, 0, 0 };
                    for (int num = 0; num < 4; num++) vals[num] = images[num].At<Vec3b>(y, x).Item0;
                    Array.Sort(vals);//並び替えを行う．min=vals[0]
                    for (int i = 1; i < 3;i++ ) if (vals[i] < th_l || vals[i]>th_h) vals[i] = 255;                    
                    color.Item0 = (byte)((vals[1] + vals[2]) / 2.0);
                    DST.Set<Vec3b>(y, x, color);
                    //indexer[y, x] = color;

                }

        }
        public void コントラスト調整_シグモイド(ref Mat src, double 倍率)
        {
            int width = src.Width;
            int height = src.Height;
            //var indexer = src.GetGenericIndexer<Vec3b>();

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    Vec3b color = src.Get<Vec3b>(y, x); //indexer[y, x];
                    color.Item0 = (byte)(255.0 / (1 + Math.Exp(-倍率 * (color.Item0 - 128) / 255)));
                    src.Set<Vec3b>(y, x, color);//indexer[y, x] = color;

                }

        }
        public void コントラスト調整(ref Mat src, double 倍率)
        {
            int width = src.Width;
            int height = src.Height;
            //var indexer = src.GetGenericIndexer<Vec3b>();
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    Vec3b color = src.Get<Vec3b>(y, x);//indexer[y, x];
                    double val = color.Item0 * 倍率;
                    if (val > 255) color.Item0 = 255;
                    else if (val < 0) color.Item0 = 0;
                    else color.Item0 = (byte)val;
                    src.Set<Vec3b>(y, x, color);//indexer[y, x] = color;
                }

        }
        public void Median(Mat[] images, ref Mat DST)
        {
            int width = images[0].Width;
            int height = images[0].Height;
            //var indexer = DST.GetGenericIndexer<Vec3b>();
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {//medianX,Y SG完成
                    double[] vals = { 0, 0, 0, 0 };
                    Vec3b color = DST.Get<Vec3b>(y, x);//indexer[y, x];
                    for (int num = 0; num < 4; num++) vals[num] = images[num].At<Vec3b>(y, x).Item0;
                    Array.Sort(vals);//並び替えを行う．min=vals[0]
                    color.Item0 = (byte)(( vals[1] + vals[2]) / 2);
                    DST.Set<Vec3b>(y, x, color);//indexer[y, x] = color;
                }

        }
        public void brightness(ref Mat img, double 目標)
        {//中心近くの9ピクセルから輝度調整

            int width = img.Width;
            int height = img.Height;
            int center_x = width /5;
            int center_y = height /5;
            //var indexer = img.GetGenericIndexer<Vec3b>();
            
            double[] vals= new double[9];
            double average=0;
            double diff = 0;

            vals[0] = img.At<Vec3b>(center_y - 10, center_x - 10).Item0; vals[3] = img.At<Vec3b>(center_y - 10, center_x).Item0; vals[6] = img.At<Vec3b>(center_y - 10, center_x + 10).Item0;
            vals[1] = img.At<Vec3b>(center_y, center_x - 10).Item0; vals[4] = img.At<Vec3b>(center_y, center_x).Item0; vals[7] = img.At<Vec3b>(center_y, center_x + 10).Item0;
            vals[2] = img.At<Vec3b>(center_y + 10, center_x - 10).Item0; vals[5] = img.At<Vec3b>(center_y + 10, center_x).Item0; vals[8] = img.At<Vec3b>(center_y + 10, center_x + 10).Item0;

            for (int num = 0; num < 9; num++) average += vals[num];
            average = average / 9.0;
            diff = 目標 - average;

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    Vec3b color = img.Get<Vec3b>(y, x);//indexer[y, x];
                    double val = color.Item0 + diff;
                    if (val > 255) color.Item0 = 255;
                    else if(val<0) color.Item0 =0;
                    else color.Item0 = (byte)val;
                    img.Set<Vec3b>(y, x, color);//indexer[y, x] = color;
                }
        }
        public void infilterX(ref Mat DST,Mat SRC)
        {
            int width = SRC.Width;
            int height = SRC.Height;
            //var indexer = DST.GetGenericIndexer<Vec3b>();
            for (int x = 0; x < width - 1; x++)
                for (int y = 0; y < height; y++)
                {
                    Vec3b color = DST.Get<Vec3b>(y, x);//indexer[y, x];
                    double val=(SRC.At<Vec3b>(y, x + 1).Item0 - SRC.At<Vec3b>(y, x).Item0);
                    if (val > 255) color.Item0 = 255;
                    else if (val < 0) color.Item0 = 0;
                    else color.Item0 = (byte)val;
                    DST.Set<Vec3b>(y, x, color);//indexer[y, x] = color;
                }
            //SRC.Dispose();
        }
        public void infilterY(ref Mat DST,Mat SRC)
        {
            int width = SRC.Width;
            int height = SRC.Height;
            //var indexer = DST.GetGenericIndexer<Vec3b>();
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height - 1; y++)
                {
                    Vec3b color = DST.Get<Vec3b>(y, x);//indexer[y, x];
                    double val = (SRC.At<Vec3b>(y+1, x).Item0 - SRC.At<Vec3b>(y, x).Item0);
                    if (val > 255) color.Item0 = 255;
                    else if (val < 0) color.Item0 = 0;
                    else color.Item0 = (byte)val;
                    DST.Set<Vec3b>(y, x, color);//indexer[y, x] = color;
                }
        }


        //public void CvDct(ref Mat DST, ref Mat SRC, int N)
        //{
        //    Mat dct, idct;
        //    Mat dct2, dct3;
        //    int width = 640;//N;
        //    int height = 480;//N;
            

        //    //DCT,IDCT用の行列作成(double)
        //    dct = Cv.CreateImage(new CvSize(width, height), BitDepth.F64, 1);
        //    idct = Cv.CreateImage(new CvSize(width, height), BitDepth.F64, 1);

        //    dct2 = Cv.CreateImage(new CvSize(width, height), BitDepth.F64, 1);
        //    dct3 = Cv.CreateImage(new CvSize(width, height), BitDepth.F64, 1);

        //    //行列dctに画像データをコピー
        //    //double fcos;
        //    for (int x = 0; x < width; x++)
        //        for (int y = 0; y < height; y++)
        //        {
        //            CvScalar cs = Cv.Get2D(SRC, y, x) / 256.0;
        //            Cv.Set2D(dct, y, x, cs);
        //        }

        //    //DCT…dctをコサイン変換してdct2を作成します
        //    Cv.DCT(dct, dct2, DCTFlag.Forward);

        //    //dct2をDenomで割りdct3を作成します
        //    PerformDenom(dct3, dct2, width, height);

        //    //IDCT…dct3を逆コサイン変換します
        //    Cv.DCT(dct3, idct, DCTFlag.Inverse);

        //    //逆変換用画像にデータをコピー
        //    for (int x = 0; x < width; x++)
        //        for (int y = 0; y < height; y++)
        //        {
        //            CvScalar cs = Cv.Get2D(idct, y, x) * 256.0;
        //            Cv.Set2D(DST, y, x, cs);
        //        }

        //    //正規化
        //    double min, max;
        //    min = 4000000000000;
        //    max = -4000000000000;
        //    double offset = 0.0;

        //    for (int x = 0; x < width; x++)
        //        for (int y = 0; y < height; y++)
        //        {
        //            CvScalar cs = Cv.Get2D(DST, y, x);
        //            double data = cs.Val0;
        //            if (data < min) min = data;
        //            if (data > max) max = data;
        //        }

        //    for (int x = 0; x < width; x++)
        //        for (int y = 0; y < height; y++)
        //        {
        //            CvScalar cs = Cv.Get2D(DST, y, x);
        //            double data = cs.Val0;

        //            if (data < min + offset) data = min + offset;
        //            cs.Val0 = (((data / (max - min + offset))) * 255.0) - (((min + offset) / (max - min + offset)) * 255.0);
        //            Cv.Set2D(DST, y, x, cs);
        //        }
        //    //DST = idct.Clone();

        //    //行列メモリを開放します
        //    dct.Dispose();
        //    dct2.Dispose();
        //    dct3.Dispose();
        //    idct.Dispose();
        //}

        //private void PerformDenom(Mat DST, Mat SRC, int width, int height)
        //{
        //    double PI = 3.1416;
        //    //XとYを準備
        //    double[,] meshX = new double[width, height];
        //    double[,] meshY = new double[width, height];
        //    double[,] denom = new double[width, height];

        //    //メッシュグリッドの作成
        //    for (int x = 0; x < width; x++)
        //        for (int y = 0; y < height; y++)
        //        {
        //            meshX[x,y] = x;
        //            meshY[x,y] = y;
        //        }

        //    //固有値計算
        //    for (int x = 0; x < width; x++)
        //        for (int y = 0; y < height; y++)
        //            denom[x, y] = (2.0 * Math.Cos(PI * (double)x / ((double)width)) - 2.0) + (2.0 * Math.Cos(PI * (double)y / ((double)height)) - 2.0);
        //    //計算
        //    for (int x = 0; x < width; x++)
        //        for (int y = 0; y < height; y++)
        //        {
        //            //data_d[j][i] = data_s[j][i] / denom[j][i];
        //            CvScalar cs = Cv.Get2D(SRC, y, x);
        //            //ゼロ割防止
        //            if (!(x == 0 && y == 0)) cs= cs/denom[x, y];

        //            Cv.Set2D(DST, y, x, cs);
        //        }
        //}
    }
}
