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

            calendar1.ShowEventTooltips = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            calendar1.AddWork("근무함","6시간","출장", DateTime.Now);
            DateTime dt = new DateTime(2015, 11, 20, 0, 0, 0);
            calendar1.AddWork("근무함", "6시간", "없음",dt);
        }
    }
}
