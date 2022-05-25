using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO.Ports;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using MinhaBiblioteca;

namespace Servidor
{
    class Program
    {
        private static bool ServidorAtivo = false;
        public static List<Dados> DADOS = new List<Dados>();
        public static SerialPort S1 = new SerialPort("COM13", 9600, Parity.None, 8);
        public static SerialPort S2 = new SerialPort("COM11", 9600, Parity.None, 8);

        private static object _lock = new object();

        public static void enviaAtuador(object obj)
        {
            lock (_lock)
            {
                S2.WriteLine((string)obj);
                Thread.Sleep(10000);
            }
        }

        public static void TratamentodeDados(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort SP = (SerialPort)sender;
            string valor = SP.ReadLine();
            Dados dado = new Dados();
            dado.valor = valor;
            dado.tempo = Convert.ToString(System.DateTime.Now);
            SP.DiscardInBuffer();

            try
            {
                if (valor == "FDT")
                {
                    Console.WriteLine("");
                }
                else
                {
                    if (DADOS.Count > 100)
                        DADOS.Clear();
                    DADOS.Add(dado);
                }
            }
            catch
            {
                Console.WriteLine("");
            }
        }

        public static void trataClientes(object Objcliente)
        {
            TcpClient cliente = (TcpClient)Objcliente;

            ParameterizedThreadStart enviaAtuadorDel = new ParameterizedThreadStart(enviaAtuador);
            Thread Thread1 = new Thread(enviaAtuadorDel);  

            NetworkStream stream;
            byte[] mensagem_in = new byte[1024];
            byte[] mensagem_out = new byte[1024];
            string mensagemS;
            string DadosCliente = cliente.Client.RemoteEndPoint.ToString();
         
            Console.WriteLine(System.DateTime.Now.ToString() + " - Cliente " + DadosCliente + " - Conectao\r\n" + "Atendendo requisições pela Thread " + Thread.CurrentThread.Name);
            stream = cliente.GetStream();

            while (cliente.Connected && ServidorAtivo)
            {
                try
                {
                    Array.Clear(mensagem_in, 0, mensagem_in.Length);
                    stream.Read(mensagem_in, 0, mensagem_in.Length);
                    mensagemS = Encoding.ASCII.GetString(mensagem_in).TrimEnd('\0');
                    string[] tokens = mensagemS.Split(' ');
                    if (tokens[0] == "QRD")
                    {
                        mensagem_out = Encoding.ASCII.GetBytes(DADOS.ElementAt(DADOS.Count-1).valor);
                        stream.Write(mensagem_out, 0, mensagem_out.Length);
                    }
                    else if (tokens[0] == "QRG")
                    {
                        BinaryFormatter bf = new BinaryFormatter();
                        MemoryStream ms = new MemoryStream();
                        bf.Serialize(ms, DADOS);
                        stream.Write(ms.ToArray(), 0, ms.ToArray().Length);
                    }
                    else if (tokens[0] == "ATR")
                    {
                        if (!Thread1.IsAlive)
                        {
                            Thread1 = new Thread(enviaAtuadorDel);
                            Thread1.Start(tokens[1]);
                        }
                    }
                }
                catch (SystemException ex)
                {
                    if (cliente.Connected)
                        Console.WriteLine("Erro: " + ex.Message);
                    cliente.Close();
                    stream.Close();
                } 
            }
            if (cliente.Connected)
            {
                cliente.Close();
                stream.Close();
            }
            Console.WriteLine();
        }

        public static void servidor()
        {
            TcpListener servidor = new TcpListener(IPAddress.Any, 9900);
            TcpClient cliente;
            servidor.Start();
            List<Thread> ListThread = new List<Thread>();

            ParameterizedThreadStart ThreadTrataCliente = new ParameterizedThreadStart(trataClientes);
            Console.WriteLine(System.DateTime.Now.ToString() + " - Aguardando Clientes ...");

            while (ServidorAtivo)
            {
                if (servidor.Pending())
                {
                    cliente = servidor.AcceptTcpClient();
                    ListThread.RemoveAll(IsDead);
                    ListThread.Add(new Thread(ThreadTrataCliente));
                    ListThread[ListThread.Count - 1].Name = ListThread[ListThread.Count - 1].ManagedThreadId.ToString();
                    ListThread[ListThread.Count - 1].Start(cliente);
                }
            }
            servidor.Stop();
        }

        private static bool IsDead(Thread T)
        {
            return !T.IsAlive;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Servidor... digite CTRL+C para sair");

            S1.DataReceived += new SerialDataReceivedEventHandler(TratamentodeDados);

            try
            {
                S1.Open();
                S1.DiscardInBuffer();
                S2.Open();
                S2.DiscardInBuffer();
            }
            catch
            {
                Console.WriteLine("");
            }

            Thread ServidorT = new Thread(servidor);
            ServidorAtivo = true;
            ServidorT.Start();

            while (!Console.KeyAvailable) { }
            ServidorAtivo = false;
            if (S1.IsOpen)
            {
                S1.Close();
            }

            if (S2.IsOpen)
            {
                S2.Close();
            }
            Environment.Exit(0);
        }
    }
}
