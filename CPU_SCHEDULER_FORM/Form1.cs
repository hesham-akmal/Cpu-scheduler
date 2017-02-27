using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CPU_SCHEDULER_FORM
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.g = this.panel1.CreateGraphics();
        }

       

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(burstTbox.Text))
                return;

            Process p = new Process(Convert.ToInt32(burstTbox.Text));
            p.arriveTime = SCH.currentTime;
            p.id = SCH.i++;

           if (SCH.State == 4 || SCH.State == 5)
               p.priority = Convert.ToInt32(Prioritybox.Text);
          
            SCH.allProcesses.Add(p);

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(SCH.speed > 0)
            SCH.speed -= 200;

            label2.Text = "Running speed: " + SCH.speed + "ms";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (SCH.speed < 10000)
                SCH.speed += 200;

            label2.Text = "Running speed: " + SCH.speed + "ms";
        }

    
      
    }
}
