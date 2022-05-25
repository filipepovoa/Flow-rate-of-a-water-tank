using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FuncaoTemp
{
    public class Temperatura
    {
        public int contador { get; set; }
        public double a { get; set; }
        public double b { get; set; }
        public double t { get; set; }
        public double temperatura { get; set; }
        public Temperatura(double T0)
        {
            temperatura = T0-0.5;
            t = T0;
        }
        public void calculaTemperatura()
        {
            if (temperatura > t + 0.5)
            {
                temperatura = temperatura - 0.1;
                b = t - 0.5;
            }
            else if (temperatura < t - 0.5)
            {
                temperatura = temperatura + 0.1;
                b = t + 0.5;
            }
            else if ((temperatura <= t + 0.5) && (temperatura >= t - 0.5))
            {
                if (contador == 0)
                {
                    if (temperatura == t - 0.5)
                    {
                        a = 0.1;
                        b = t - 0.5;
                    }
                    else if (temperatura == t + 0.5)
                    {
                        a = -0.1;
                        b = t + 0.5;
                    }
                }
                temperatura = a * contador + b;
            }
        }
    }
}
