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

            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            calendar1.AddWork("11시 10분", "18시 15분", "", new DateTime(2015, 12, 1));
            calendar1.AddWork("10시 56분","17시 55분","", DateTime.Now);
            calendar1.AddWork("10시 40분","18시 05분","", new DateTime(2015,12,3));
            calendar1.AddWork("10시 48분", "19시 00분", "출장", new DateTime(2015, 12, 4));
            
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
    }
}
