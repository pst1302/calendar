using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CalanderTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            calendar1.CalendarDate = DateTime.Now;

            // 해당 날짜를 두번클릭할때 발생하는 이벤트 할당 예제
            calendar1.dateClickEventHandler += Date_Click;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            for(int i = 1; i < 30; i++)

                calendar1.AddWork("11시 00분", "18시 00분", "출장", new DateTime(2015, 12, i));

            calendar1.Refresh();
            
        }

        // 내가 작성한 이벤트 처리 함수
        private void Date_Click(DateTime time)
        {
            calendar1.AddWork("11시 00분", "18시 00분", "없음", time);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            calendar1.AddWorkStartTime("11시 00분", new DateTime(2015, 12, 7));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            calendar1.AddWorkStartTime("10시 20분", new DateTime(2015, 12, 4));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            calendar1.AddWorkEndTime("18시 00분", new DateTime(2015, 12, 4));
        }

        private void button5_Click(object sender, EventArgs e)
        {
            calendar1.AddWorkEndTime("15시 00분", new DateTime(2015, 12, 7));
        }

        private void button6_Click(object sender, EventArgs e)
        {
            calendar1.AddEtc("병가로 조퇴", new DateTime(2015, 12, 7));
        }
    }
}
