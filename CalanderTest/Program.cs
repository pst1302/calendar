using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CalanderTest
{

    enum mod
    {
        Test,       // 테스트를 위한 폼
        Excute      // 실제 실행을 위한 폼
    }
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            mod mode = mod.Test;
            
            if(mode == mod.Test)
            {
                Application.Run(new TestForm());
            }
            else if(mode == mod.Excute)
            {
                Application.Run(new Form1());
            }
        }
    }
}
