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
    public partial class WorkEnterForm : Form
    {
        

        public WorkType t;
        public int workTime;
        public string note;


        public WorkEnterForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            t = WorkType.NotWorking;
            if (comboBox1.SelectedItem.ToString().Equals("근무"))
            {
                t = WorkType.Working;
            }
            else if(comboBox1.SelectedItem.ToString().Equals("미 근무"))
            {
                t = WorkType.NotWorking;
            }
            else if(comboBox1.SelectedItem.ToString().Equals("부분 근무"))
            {
                t = WorkType.incompleteWorking;
            }

            if (textBox1.Text != "")
            {
                workTime = int.Parse(textBox1.Text);
            }

            note = textBox5.Text;

            //System.Diagnostics.Debug.WriteLine(workTime.ToString());
            //System.Diagnostics.Debug.WriteLine(note);

            textBox1.Clear();
            textBox5.Clear();

            this.Hide();
        }
    }
}
