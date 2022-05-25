using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using FuncaoTemp;
using MinhaBiblioteca;

namespace Emuladores
{
    public partial class Form1 : Form
    {
        public Temperatura tp;

        public void Inicializacao()
        {
            string[] baudrate = { "9600", "115200" };
            Parity[] paridades = { Parity.Even, Parity.Odd, Parity.None };
            int[] databits = { 7, 8 };
            StopBits[] sb = { StopBits.One, StopBits.OnePointFive, StopBits.Two };

            comboBox1.Items.AddRange(SerialPort.GetPortNames());
            comboBox6.Items.AddRange(SerialPort.GetPortNames());
            comboBox2.Items.AddRange(baudrate);
            comboBox7.Items.AddRange(baudrate);

            foreach (Parity P in paridades)
            {
                comboBox5.Items.Add(P.ToString());
                comboBox10.Items.Add(P.ToString());
            }

            foreach (StopBits S in sb)
            {
                comboBox4.Items.Add(S.ToString());
                comboBox9.Items.Add(S.ToString());
            }

            foreach (int db in databits)
            {
                comboBox3.Items.Add(db.ToString());
                comboBox8.Items.Add(db.ToString());
            }
        } 

        public Form1()
        {
            InitializeComponent();
            this.Inicializacao();
            tp = new Temperatura(25.0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                s1.BaudRate = Convert.ToInt32(comboBox2.SelectedItem);
                s1.PortName = comboBox1.SelectedItem.ToString();
                s1.Parity = (Parity)Enum.Parse(typeof(Parity), comboBox5.SelectedItem.ToString());
                s1.DataBits = Convert.ToInt32((comboBox3.SelectedItem.ToString()));
                s1.StopBits = (StopBits)Enum.Parse(typeof(StopBits), comboBox4.SelectedItem.ToString());

                s2.BaudRate = Convert.ToInt32(comboBox7.SelectedItem);
                s2.PortName = comboBox6.SelectedItem.ToString();
                s2.Parity = (Parity)Enum.Parse(typeof(Parity), comboBox10.SelectedItem.ToString());
                s2.DataBits = Convert.ToInt32((comboBox8.SelectedItem.ToString()));
                s2.StopBits = (StopBits)Enum.Parse(typeof(StopBits), comboBox9.SelectedItem.ToString());

                MessageBox.Show("Portas seriais configuradas com sucesso");
            }
            catch
            {
                MessageBox.Show("Erro ao configurar as portas");
            }
        }

        public void disable()
        {
            try
            {
                this.s1.WriteLine("FDT");
                this.s1.Close();
                this.timer1.Enabled = false;
                this.button2.Text = "INICIAR";
            }
            catch
            {
                MessageBox.Show("Falha ao desligar o sensor");
            }
        }

        public void enable()
        {
            try
            {
                s1.Open();
                s2.Open();
            }
            catch
            {
                MessageBox.Show("Erro ao iniciar a conexão");
                return;
            }
            this.button2.Text = "PARAR";
            this.timer1.Enabled = true;
        }

        public void TratamentodeDados(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort SP = (SerialPort)sender;
            string indado = SP.ReadLine();
            SP.DiscardInBuffer();

            try
            {
                if (indado == "FDT")
                {
                    Console.WriteLine("");
                }
                else
                {
                    tp.t = Convert.ToDouble(indado);
                }
            }
            catch
            {
                Console.WriteLine("");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.button2.Text == "INICIAR")
            {
                s2.DataReceived += new SerialDataReceivedEventHandler(TratamentodeDados);
                this.enable();
            }
            else
            {
                this.disable();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.tp.calculaTemperatura();
            this.tp.contador = (this.tp.contador + 1) % 11;

            try
            {
                s1.WriteLine(this.tp.temperatura.ToString());
            }
            catch
            {
                MessageBox.Show("Falha ao enviar dados");
                this.disable();
            }
        }
    }
}
