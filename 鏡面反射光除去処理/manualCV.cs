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

            MatIndexer<Vec3b>[] indexers = new MatIndexer<Vec3b>[4];
            var indexer = new MatOfByte3(DST).GetIndexer();

            for (int i = 0; i < 4; i++) indexers[i] = new MatOfByte3(images[i]).GetIndexer();

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    Vec3b[] colors = new Vec3b[4];
                    Vec3b color = indexer[y, x];
                    for (int i = 0; i < 4; i++) colors[i] = indexers[i][y, x];
                    double[] vals = { 0, 0, 0, 0 };
                    for (int num = 0; num < 4; num++) vals[num] = colors[num].Item0;
                    Array.Sort(vals);//並び替えを行う．min=vals[0]
                    color.Item0 = (byte)((vals[0] + vals[1] + vals[2]) / 3.0);
                    indexer[y, x] = color;

                    colors = null;
                }
            indexers = null;
            indexer = null;

        }
        public void 吉岡反射光除去処理(Mat[] images, ref Mat DST,int th_l,int th_h)
        {
            int width = images[0].Width;
            int height = images[0].Height;
            MatIndexer<Vec3b>[] indexers = new MatIndexer<Vec3b>[4];
            var indexer = new MatOfByte3(DST).GetIndexer();

            for (int i = 0; i < 4; i++) indexers[i] = new MatOfByte3(images[i]).GetIndexer();

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {//medianX,Y SG完成
                    Vec3b[] colors = new Vec3b[4];
                    Vec3b color = indexer[y, x];
                    for (int i = 0; i < 4; i++) colors[i] = indexers[i][y, x];
                    double[] vals = { 0, 0, 0, 0 };
                    for (int num = 0; num < 4; num++) vals[num] = colors[num].Item0;
                    Array.Sort(vals);//並び替えを行う．min=vals[0]
                    for (int i = 1; i < 3; i++)
                    {
                        //if (vals[i] < th_l || vals[i] > th_h) vals[i] = 255;
                        
                        //普通こっちでは？
                        //if (vals[i] < th_l) vals[i] = 0;
                        //if (vals[i] > th_h) vals[i] = 255;

                    }
                    color.Item0 = (byte)((vals[1] + vals[2]) / 2.0);
                    indexer[y, x] = color;

                    colors = null;
                }
            indexers = null;
            indexer = null;
        }
        public void コントラスト調整_シグモイド(ref Mat src, double 倍率)
        {
            int width = src.Width;
            int height = src.Height;
            var indexer = new MatOfByte3(src).GetIndexer();

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    Vec3b color = indexer[y, x];
                    color.Item0 = (byte)(255.0 / (1 + Math.Exp(-倍率 * (color.Item0 - 128) / 255)));
                    indexer[y, x] = color;
                }
            indexer = null;
        }
        public void コントラスト調整(ref Mat src, double 倍率)
        {
            int width = src.Width;
            int height = src.Height;

            var indexer = new MatOfByte3(src).GetIndexer();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vec3b color = indexer[y, x];
                    double val = color.Item0 * 倍率;
                    if (val > 255) color.Item0 = 255;
                    else if (val < 0) color.Item0 = 0;
                    else color.Item0 = (byte)val;
                    indexer[y, x] = color;
                }
            }
            indexer = null;

        }
        public void Median(Mat[] images, ref Mat DST)
        {
            int width = images[0].Width;
            int height = images[0].Height;
            MatIndexer<Vec3b>[] indexers = new MatIndexer<Vec3b>[4];
            var indexer = new MatOfByte3(DST).GetIndexer();
            for (int i = 0; i < 4; i++) indexers[i] = new MatOfByte3(images[i]).GetIndexer();

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {//medianX,Y SG完成

                    Vec3b[] colors = new Vec3b[4];
                    Vec3b color = indexer[y, x];
                    for (int i = 0; i < 4; i++) colors[i] = indexers[i][y, x];
                    double[] vals = { 0, 0, 0, 0 };

                    for (int num = 0; num < 4; num++) vals[num] = colors[num].Item0;
                    Array.Sort(vals);//並び替えを行う．min=vals[0]
                    color.Item0 = (byte)(( vals[1] + vals[2]) / 2);
                    indexer[y, x] = color;

                    colors = null;
                }
            indexers = null;
            indexer = null;

        }
        public void brightness(ref Mat img, double 目標)
        {//中心近くの9ピクセルから輝度調整

            int width = img.Width;
            int height = img.Height;
            int center_x = width / 5;
            int center_y = height / 5;

            double[] vals = new double[9];
            double average = 0;
            double diff = 0;
            var indexer = new MatOfByte3(img).GetIndexer();

            vals[0] = indexer[center_y - 10, center_x - 10].Item0; vals[3] = indexer[center_y - 10, center_x].Item0; vals[6] = indexer[center_y - 10, center_x + 10].Item0;
            vals[1] = indexer[center_y, center_x - 10].Item0; vals[4] = indexer[center_y, center_x].Item0; vals[7] = indexer[center_y, center_x + 10].Item0;
            vals[2] = indexer[center_y + 10, center_x - 10].Item0; vals[5] = indexer[center_y + 10, center_x].Item0; vals[8] = indexer[center_y + 10, center_x + 10].Item0;

            for (int num = 0; num < 9; num++) average += vals[num];
            average = average / 9.0;
            diff = 目標 - average;

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    Vec3b color = indexer[y, x];
                    double val = color.Item0 + diff;
                    if (val > 255) color.Item0 = 255;
                    else if (val < 0) color.Item0 = 0;
                    else color.Item0 = (byte)val;
                    indexer[y, x] = color;
                }
            indexer = null;
        }
        public void infilterX(ref Mat DST,Mat SRC)
        {
            int width = SRC.Width;
            int height = SRC.Height;
            DST = SRC.Clone();
            var indexer = new MatOfByte3(DST).GetIndexer();

            for (int x = 0; x < width - 1; x++)
                for (int y = 0; y < height; y++)
                {
                    Vec3b color = indexer[y, x];
                    double val=indexer[y, x + 1].Item0 - indexer[y,x].Item0;
                    if (val > 255) color.Item0 = 255;
                    else if (val < 0) color.Item0 = 0;
                    else color.Item0 = (byte)val;
                    indexer[y, x] = color;                    
                }
            indexer = null;
        }
        public void infilterY(ref Mat DST,Mat SRC)
        {
            int width = SRC.Width;
            int height = SRC.Height;
            DST = SRC.Clone();
            var indexer = new MatOfByte3(DST).GetIndexer();

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height - 1; y++)
                {
                    Vec3b color = indexer[y, x];
                    double val = indexer[y+1, x].Item0 - indexer[y, x].Item0;
                    if (val > 255) color.Item0 = 255;
                    else if (val < 0) color.Item0 = 0;
                    else color.Item0 = (byte)val;
                    indexer[y, x] = color;
                }
            indexer = null;
        }


        public void CvDct(ref Mat DST, Mat SRC, int N)
        {
            Mat dct, idct;
            Mat dct2, dct3;
            int width = SRC.Width;//N;
            int height = SRC.Height;//N;

            DST = SRC.Clone();            

            //DCT,IDCT用の行列作成(double)
            dct =  new Mat(height, width, MatType.CV_64FC1);
            idct = new Mat(height, width, MatType.CV_64FC1);

            dct2 = new Mat(height, width, MatType.CV_64FC1);
            dct3 = new Mat(height, width, MatType.CV_64FC1);



            var indexer_DST = new MatOfByte3(DST).GetIndexer();
            var indexer_dct = new MatOfDouble3(dct).GetIndexer();
            

            //行列dctに画像データをコピー
            //double fcos;
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    Vec3d color = indexer_dct[y, x];
                    color.Item0= indexer_DST[y, x].Item0 / 256.0;
                    indexer_dct[y,x] = color;
                }
            //DCT…dctをコサイン変換してdct2を作成します
            Cv2.Dct(dct, dct2, DctFlags.None);

            //dct2をDenomで割りdct3を作成します
            PerformDenom(ref dct3, dct2);

            //IDCT…dct3を逆コサイン変換します
            Cv2.Dct(dct3, idct, DctFlags.Inverse);
            var indexer_idct = new MatOfDouble3(idct).GetIndexer();

            //逆変換用画像にデータをコピー
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    Vec3b color = indexer_DST[y, x];
                    color.Item0=  (byte)(indexer_idct[y,x].Item0 * 256.0);
                    indexer_DST[y,x]=color;
                }

            ////正規化
            //double min, max;
            //min = 4000000000000;
            //max = -4000000000000;
            //double offset = 0.0;

            ////輝度値の最大と最小を取得
            //DST.MinMaxIdx(out min, out max);
            ////for (int x = 0; x < width; x++)
            ////    for (int y = 0; y < height; y++)
            ////    {
            ////        double data = indexer_DST[y,x].Item0;
            ////        if (data < min) min = data;
            ////        if (data > max) max = data;
            ////    }

            //for (int x = 0; x < width; x++)
            //    for (int y = 0; y < height; y++)
            //    {
            //        Vec3b color = indexer_DST[y, x];
            //        double data = indexer_DST[y, x].Item0;

            //        if (data < min + offset) data = min + offset;
            //        color.Item0 = (byte)( (((data / (max - min + offset))) * 255.0) - (((min + offset) / (max - min + offset)) * 255.0) );
            //        indexer_DST[y,x] = color;
            //    }
            ////DST = idct.Clone();

            //行列メモリを開放します
            dct.Dispose();
            dct2.Dispose();
            dct3.Dispose();
            idct.Dispose();

            indexer_dct = null;
            indexer_DST = null;
            indexer_idct = null;
        }

        private void PerformDenom(ref Mat DST, Mat SRC)
        {
            double PI = 3.1416;
            int width = SRC.Width;
            int height = SRC.Height;
            //XとYを準備
            double[,] meshX = new double[height,width];
            double[,] meshY = new double[height,width];
            double[,] denom = new double[height,width];

            DST = SRC.Clone();
            var indexer = new MatOfDouble3(DST).GetIndexer();

            //メッシュグリッドの作成
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    meshX[y, x] = x;
                    meshY[y, x] = y;
                }

            //固有値計算
            for (int y = 0; y < height; y++)
                for (int x = 0;x < width; x++)
                    denom[y, x] = (2.0 * Math.Cos(PI * (double)x / ((double)width)) - 2.0) + (2.0 * Math.Cos(PI * (double)y / ((double)height)) - 2.0);
            //計算
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    //data_d[j][i] = data_s[j][i] / denom[j][i];


                    Vec3d color = indexer[y, x];
                    color.Item0 = indexer[y, x].Item0;
                    //ゼロ割防止
                    if (!(x == 0 && y == 0)) color.Item0 = color.Item0 / denom[y, x];
                    indexer[y, x] = color;
                }
            indexer = null;
        }
    }
}
