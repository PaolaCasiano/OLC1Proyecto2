using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proyecto2.Errores
{
    class Error
    {
        public static RichTextBox consola;
        int linea;
        int columna;
        string tipo;
        string desc;

        public Error(int fila, int columna, string tipo, string descr)
        {
            this.linea = fila + 1;
            this.columna = columna;
            this.tipo = tipo;
            this.desc = descr;
            consola.Select(consola.TextLength, 0);
            consola.SelectionColor = Color.Red;
            consola.AppendText(this.linea + "  -  " + this.columna + "   -   " + this.desc + "  -  " + this.tipo + "\n");
        }

        public int Linea
        {
            get { return linea; }
        }

        public int Columna
        {
            get { return columna; }
        }
        public string Tipo
        {
            get { return tipo; }
        }
        public string Desc
        {
            get { return desc; }
        }
    }
}
