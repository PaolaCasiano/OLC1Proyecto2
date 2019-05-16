using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto2.Objetos
{
    class Figura
    {
        public string tipo { get; set; }
        public int posx { get; set; }
        public int posy { get; set; }
        public string color { get; set; }

        //circulo y rectangulo
        public bool solida { get; set; }

        //circulo
        public int radio { get; set; }

        //rectangulo
        public int width { get; set; }
        public int height { get; set; }

        //triangulo
        public int vx1 { get; set; }
        public int vx2 { get; set; }
        public int vx3 { get; set; }
        public int vy1 { get; set; }
        public int vy2 { get; set; }
        public int vy3 { get; set; }


        //linnea
        public int posxf { get; set; }
        public int posyf { get; set; }
        public int grosor { get; set; }
    }
}
