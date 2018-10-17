using System;
using System.Windows.Forms;

namespace Opticus
{
    public partial class Welcome : Form
    {
        /*------------------------------------------Declaring Main Classes------------------------------------------*/

        Main main;

        /*----------------------------------------------------------------------------------------------------------*/

        public Welcome()
        {
            InitializeComponent();
        }

        private void button_continue_Click(object sender, EventArgs e)
        {
            main = new Main();
            main.Show();
        }

        private void button_exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}