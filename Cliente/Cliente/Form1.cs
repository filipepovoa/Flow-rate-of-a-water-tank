using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using MinhaBiblioteca;

namespace Cliente
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            label2.Text = "25°C";
        }

        private NetworkStream stream;
        TcpClient cliente;
        bool b = true;

        public void desconectar()
        {
            button1.Text = "CONECTAR";
            checkBox1.Checked = false;
            checkBox2.Checked = false;
            stream.Close();
        }

        private void EnviaDado(string s)
        {
            try
            {
                string pacote;
                string pacote_in;
                byte[] pacoteb = new byte[1024];
                byte[] pacoteb_in = new byte[1024];
                BinaryFormatter bf = new BinaryFormatter();
                byte[] buffer;
                List<Dados> Lst;

                pacote = s;
                pacoteb = Encoding.ASCII.GetBytes(pacote);
                stream.Write(pacoteb, 0, pacoteb.Length);

                System.Threading.Thread.Sleep(100);
                
                if (s == "QRD ")
                {
                    stream.Read(pacoteb_in, 0, pacoteb_in.Length);
                    pacote_in = Encoding.ASCII.GetString(pacoteb_in);

                    label1.Text = pacote_in + "°C";
                }
                else if (s == "QRG ")
                {
                    try
                    {
                        if (cliente.Available > 0)
                        {
                            buffer = new byte[cliente.Available];
                            stream.Read(buffer, 0, cliente.Available);

                            MemoryStream ms = new MemoryStream(buffer);
                            Lst = bf.Deserialize(ms) as List<Dados>;

                            chart1.Series[0].Points.Clear();

                            for (int i = 0; i < Lst.Count; i++)
                            {
                                chart1.Series[0].Points.AddXY(Lst[i].tempo, Convert.ToDouble(Lst[i].valor));
                            }
                        }
                    }
                    catch (SystemException ex)
                    {
                        MessageBox.Show("Erro: " + ex.Message);
                    }
                }
            }
            catch
            {
                b = false;
                this.desconectar();
                MessageBox.Show("Erro ao comunicar com o servidor");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (button1.Text == "CONECTAR")
            {
                try
                {
                    cliente = new TcpClient();  
                    cliente.Connect(IPAddress.Parse("10.15.30.25"), 9900);  
                    stream = cliente.GetStream();                             

                    button1.Text = "DESCONECTAR";                        
                }
                catch
                {
                    MessageBox.Show("Falha ao conectar com o servidor");               
                }
            }
            else
            {
                this.desconectar();           
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            EnviaDado("QRD ");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string[] tokens = label2.Text.Split('°');
            EnviaDado("ATR " + tokens[0]);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                string[] tokens = label2.Text.Split('°');
                double aux = Convert.ToDouble(tokens[0]);
                label2.Text = Convert.ToString(aux + 1) + "°C";
            }
            catch
            {
                MessageBox.Show("Erro ao converter para double");
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                string[] tokens = label2.Text.Split('°');
                double aux = Convert.ToDouble(tokens[0]);
                label2.Text = Convert.ToString(aux - 1) + "°C";
            }
            catch
            {
                MessageBox.Show("Erro ao converter para double");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            EnviaDado("QRG ");
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                timer1.Enabled = true;
            }
            else
            {
                timer1.Enabled = false;
            }
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                timer2.Enabled = true;
            }
            else
            {
                timer2.Enabled = false;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (b)
                EnviaDado("QRD ");
            else
                timer1.Enabled = false;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (b)
                EnviaDado("QRG ");
            else
                timer2.Enabled = false;
        }
    }
}
