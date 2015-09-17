using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace 反射光除去処理
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new メイン画面());
        }
    }
}
