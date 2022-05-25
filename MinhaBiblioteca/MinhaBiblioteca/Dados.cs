using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace MinhaBiblioteca
{
    [Serializable]
    public class Dados
    {
        public string valor { get; set; }
        public string tempo { get; set; }
        public Dados() { }
    }
}
