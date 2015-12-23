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

            
            calenderEx1.DateClickEvent += dtClickEvent;
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
                "출장");
            }
        }
    }
}
