using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenCvSharp;

namespace 反射光除去処理
{
    public partial class メイン画面 : Form
    {
        Mat[] 入力画像= new Mat[4];
        Mat[] Gx = new Mat[4];
        Mat[] Gy = new Mat[4];
        Mat SGx;
        Mat SGy;
        Mat Gxx;
        Mat Gyy;

        Mat 出力画像;
        Boolean is4Image = false;
        manualCV mCV = new manualCV();

        public メイン画面()
        {
            InitializeComponent();
        }
        private void 自作プロセス実行()
        {
            System.Diagnostics.Debug.WriteLine("自作プロセス開始");
            if (is4Image)
            {
                int width = 入力画像[0].Width;
                int height = 入力画像[0].Height;
                出力画像 = new OpenCvSharp.Mat(height,width,MatType.CV_8UC1);
                mCV.自作反射光除去(入力画像, ref 出力画像);
                if (!(int.Parse(textBox_Gaus.Text) == 0)) Cv2.GaussianBlur(出力画像, 出力画像, new OpenCvSharp.Size(int.Parse(textBox_Gaus.Text), int.Parse(textBox_Gaus.Text)),0,0,BorderTypes.Default);//ガウシアンフィルタ

                mCV.コントラスト調整(ref 出力画像, double.Parse(textBox_Cont.Text));
                //mCV.コントラスト調整_シグモイド(ref 出力画像, double.Parse(textBox_Cont.Text));
                if (textBox_Bright.Text != "0") mCV.brightness(ref 出力画像, double.Parse(textBox_Bright.Text));

                pictureBoxIpl1.ImageIpl = 出力画像;
            }
            else System.Diagnostics.Debug.WriteLine("no 4 images");
            System.Diagnostics.Debug.WriteLine("自作プロセス終了");
 
        }
        private void OnClick自作(object sender, EventArgs e)
        {
            自作プロセス実行();
        }
        private void OnClick実行(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("OnClick実行　開始");
            if (is4Image)
            {

                int width = 入力画像[0].Width;
                int height = 入力画像[0].Height;

                SGx = new OpenCvSharp.Mat(height,width, MatType.CV_8UC1);
                SGy = new OpenCvSharp.Mat(height, width, MatType.CV_8UC1);

                for (int num = 0; num < 4; num++)
                {
                    Gx[num] = 入力画像[num].Clone();
                    Gy[num] = 入力画像[num].Clone();
                }


                for (int num = 0; num < 4; num++)
                {//infilterX,Y
                    mCV.infilterX(ref Gx[num], 入力画像[num]);
                    mCV.infilterY(ref Gy[num], 入力画像[num]);
                }
                mCV.Median(Gx, ref SGx);
                mCV.Median(Gy, ref SGy);

                //Gxxを作る．とりあえず外周1ピクセルやらない方向で．
                Gxx = new OpenCvSharp.Mat(height, width, MatType.CV_8UC1);
                Gyy = new OpenCvSharp.Mat(height, width, MatType.CV_8UC1);

                mCV.infilterX(ref Gxx, SGx);
                mCV.infilterY(ref Gyy, SGy);

                //SP作成（仮の出力画像）
                Mat SP = new OpenCvSharp.Mat(height,width, MatType.CV_8UC1);
                var indexer_sp = new MatOfByte3(SP).GetIndexer();
                var indexer_Gxx= new MatOfByte3(Gxx).GetIndexer();
                var indexer_Gyy = new MatOfByte3(Gyy).GetIndexer();
                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                    {
                        Vec3b color = indexer_sp[y, x];
                        double val = indexer_Gxx[y, x].Item0+ indexer_Gyy[y, x].Item0;
                        if (val > 255) color.Item0 = 255;
                        else if (val < 0) color.Item0 = 0;
                        else color.Item0 = (byte)val;
                        indexer_sp[y, x] = color;
 
                    }
                indexer_sp  =null;
                indexer_Gxx =null;
                indexer_Gyy = null;
                


                Mat DCT_dst = new OpenCvSharp.Mat(height, width, MatType.CV_8UC1);
                mCV.CvDct(ref DCT_dst,SP, 1024);//第3引数使われてない件
                //DCT_dst = SP.Clone();
                //mCV.吉岡反射光除去処理(入力画像, ref DCT_dst,int.Parse(textBox_Gaus.Text),int.Parse(textBox_Bright.Text));
                出力画像=DCT_dst.Clone();
                
            }
            else System.Diagnostics.Debug.WriteLine("no 4 images");
            System.Diagnostics.Debug.WriteLine("OnClick実行　終了");
        }

        private void OnClick開く(object sender, EventArgs e)
        {
            for (int i = 0; i < 4; i++)
            {
                if (入力画像[i] != null) 入力画像[i].Dispose();
                入力画像[i] = new OpenCvSharp.Mat(480, 640, MatType.CV_8UC1);
            }
            if (出力画像 != null)
            {
                出力画像.Dispose();
                出力画像=new OpenCvSharp.Mat(480,640, MatType.CV_8UC1); 
            }
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Multiselect = true,  // 複数選択の可否
                Filter =  // フィルタ
                "画像ファイル|*.bmp;*.gif;*.jpg;*.png|全てのファイル|*.*",
            };
            //ダイアログを表示
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {

                //OKボタンがクリックされたとき
                //選択されたファイル名をすべて表示する
                foreach (var file in dialog.FileNames.Select((value, index) => new { value, index }))
                {
                    入力画像[file.index] = new Mat(file.value, ImreadModes.GrayScale);
                }
                if (dialog.FileNames.Length == 4) is4Image = true;
                else is4Image = false;
                // ファイル名をタイトルバーに設定
                this.Text = dialog.FileNames[0];

                pictureBoxIpl1.Size = pictureBoxIplのサイズ調整(入力画像[0]);
                pictureBoxIpl1.ImageIpl = 入力画像[0];
            }
        }

        public static System.Drawing.Size pictureBoxIplのサイズ調整(Mat sample)
        {
            return new System.Drawing.Size(sample.Width, sample.Height);
        }

        private void TextChanged_x(object sender, EventArgs e)
        {
            カーソル移動操作();
        }

        private void TextChanged_y(object sender, EventArgs e)
        {
            カーソル移動操作();
        }
        private void カーソル移動操作()
        {
            if (pictureBoxIpl1.ImageIpl != null)
            {
                double isnumber_x, isnumber_y;
                if (double.TryParse(textBox_x.Text, out isnumber_x) && double.TryParse(textBox_y.Text, out isnumber_y))
                    if ((isnumber_x >= 0 && isnumber_x <= pictureBoxIpl1.ImageIpl.Width) && (isnumber_y >= 0 && isnumber_y <= pictureBoxIpl1.ImageIpl.Height))
                    {
                        //CvColor c = pictureBoxIpl1.ImageIpl[(int)isnumber_y, (int)isnumber_x];
                        //label_color.Text = "" + c.B;
                        label_color.Text = pictureBoxIpl1.ImageIpl.At<Vec3b>((int)isnumber_y, (int)isnumber_x).Item0.ToString();
                        //クライアント座標を画面座標に変換する
                        System.Drawing.Point mp = this.PointToScreen(new System.Drawing.Point((int)isnumber_x + pictureBoxIpl1.Location.X, (int)isnumber_y + pictureBoxIpl1.Location.Y));
                        //マウスポインタの位置を設定する
                        System.Windows.Forms.Cursor.Position = mp;
                    }
            }
        }

        private void MouseMove_pictureBoxIpl1(object sender, MouseEventArgs e)
        {
            System.Drawing.Point sp = System.Windows.Forms.Cursor.Position;
            System.Drawing.Point cp = this.PointToClient(sp);
            label_座標.Text = "(" + (cp.X - pictureBoxIpl1.Location.X) + "," + (cp.Y - pictureBoxIpl1.Location.Y) + ")";
        }

        private void OnClick_pictureBoxIpl1(object sender, MouseEventArgs e)
        {
            if (pictureBoxIpl1.ImageIpl != null)
            {
                System.Drawing.Point sp = System.Windows.Forms.Cursor.Position;
                System.Drawing.Point cp = this.PointToClient(sp);

                label_color.Text = pictureBoxIpl1.ImageIpl.At<Vec3b>(cp.Y - pictureBoxIpl1.Location.Y, cp.X - pictureBoxIpl1.Location.X).Item0.ToString();//(Cv.Get2D(pictureBoxIpl1.ImageIpl, cp.Y - pictureBoxIpl1.Location.Y, cp.X - pictureBoxIpl1.Location.X)).ToString();
                textBox_x.Text = "" + (cp.X - pictureBoxIpl1.Location.X);
                textBox_y.Text = "" + (cp.Y - pictureBoxIpl1.Location.Y);
            }
        }

        private void OnScroll_trackBar_選択(object sender, EventArgs e)
        {
            int val = trackBar_選択.Value;
            if (val < 4)
            {
                if (入力画像[val] != null) pictureBoxIpl1.ImageIpl = 入力画像[val];
                if (checkBox_Gx.Checked) pictureBoxIpl1.ImageIpl = Gx[val];
                if (checkBox_Gy.Checked) pictureBoxIpl1.ImageIpl = Gy[val];
                if (checkBox_SG.Checked)
                {
                    if (val == 0) pictureBoxIpl1.ImageIpl = SGx;
                    else if (val == 1) pictureBoxIpl1.ImageIpl = SGy;
                }
                if (checkBox_G2.Checked) 
                {
                    if (val == 0) pictureBoxIpl1.ImageIpl = Gxx;
                    else if (val == 1) pictureBoxIpl1.ImageIpl = Gyy;
                }
            }
            else
            {
                if (出力画像 != null) pictureBoxIpl1.ImageIpl = 出力画像;
            }
            

        }

        private void OnClick保存(object sender, EventArgs e)
        {
            System.IO.Directory.CreateDirectory(@"result");//resultフォルダの作成
            SaveFileDialog sfd = new SaveFileDialog();//SaveFileDialogクラスのインスタンスを作成
            sfd.FileName =  textBox_Gaus.Text+"_"+textBox_Bright.Text+"_"+textBox_Cont.Text;//はじめのファイル名を指定する
            sfd.InitialDirectory = @"result\";//はじめに表示されるフォルダを指定する
            sfd.Filter ="画像ファイル|*.bmp;*.gif;*.jpg;*.png|全てのファイル|*.*";//[ファイルの種類]に表示される選択肢を指定する
            sfd.FilterIndex = 1;//[ファイルの種類]ではじめに「画像ファイル」が選択されているようにする
            sfd.Title = "保存先のファイルを選択してください";//タイトルを設定する
            sfd.RestoreDirectory = true;//ダイアログボックスを閉じる前に現在のディレクトリを復元するようにする
            sfd.OverwritePrompt = true;//既に存在するファイル名を指定したとき警告する．デフォルトでTrueなので指定する必要はない
            sfd.CheckPathExists = true;//存在しないパスが指定されたとき警告を表示する．デフォルトでTrueなので指定する必要はない

            //ダイアログを表示する
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                //OKボタンがクリックされたとき
                //選択されたファイル名を表示する
                System.Diagnostics.Debug.WriteLine(sfd.FileName);
                pictureBoxIpl1.ImageIpl.SaveImage(sfd.FileName);
            }
        }

        private void ValueChanged_cont(object sender, EventArgs e)
        {
            textBox_Cont.Text = (trackBar_cont.Value / 10.0).ToString();
            自作プロセス実行();
        }


        private void TextChanged_cont(object sender, EventArgs e)
        {
            double isnumber;
            if (double.TryParse(textBox_Cont.Text, out isnumber))
                if (isnumber * 10 >= trackBar_cont.Minimum && isnumber * 10 <= trackBar_cont.Maximum)
                    trackBar_cont.Value = (int)(isnumber * 10);
        }




    }
}
