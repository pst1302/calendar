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
    public partial class TestForm : Form
    {

        public WorkType wt;
        public int workingTime;
        public string note;
        WorkEnterForm wef;

        public TestForm()
        {
            InitializeComponent();
            wef = new WorkEnterForm();

            // Date 클릭에 대한 함수 추가 부분
            calenderEx1.DateClickEvent += dtClickEvent;

            // 다음달로 이동할때 추가할 작업 수행 예시
            calenderEx1.addMonthClick += AddMonthEvent;

            // 이전달로 이동할때 추가할 작업 수행 예시
            calenderEx1.reduceMonthClick += reduceMonthEvent;


        }

        private void AddMonthEvent()
        {
            MessageBox.Show("다음달로 이동할때 작업 추가 예시");
        }

        private void reduceMonthEvent()
        {
            MessageBox.Show("이전달로 이동할때 작업 추가 예시");
        }

        private void dtClickEvent(DateTime dt)
        {
            wef.ShowDialog();

            ((DatePanel)calenderEx1.Controls[dt.Day - 1]).AddWork(wef.t,
                wef.workTime,
                wef.note);

            wef.t = WorkType.NotWorking;
            wef.workTime = 0;
            wef.note = null;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            calenderEx1.AddMonth();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            calenderEx1.reduceMonth();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            for(int i = 0;i < calenderEx1.Controls.Count; i++)
            {
                ((DatePanel)calenderEx1.Controls[i]).AddWork(WorkType.Working,
                6,
                "없음");
            }
        }
    }
}
