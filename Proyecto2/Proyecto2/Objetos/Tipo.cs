using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto2.Objetos
{
    class Tipo
    {
        public bool sobreescribe { get; set; }
        public bool esArray { get; set; }
        public bool esmetodo { get; set; }
        public bool esObjeto { get; set; }

        public Entorno entorno { get; set; }
        public ParseTreeNode cuerpo { get; set; }

        public String tipo { get; set; }
        public String nombre { get; set; }
        public String visibilidad { get; set; }

        public List<int> tamano { get; set; }
        public List<String> parametros { get; set; }

        

        public int fila { get; set; }
        public int columna { get; set; }

        public String valorString { get; set; }
        public bool valorBoleano { get; set; }
        public int valorEntero { get; set; }
        public double valorDouble { get; set; }
        public char valorChar { get; set; }
        public List<Tipo> arreglo { get; set; }

        public Tipo()
        {
            this.esmetodo = false;
            this.esObjeto = false;
            this.entorno = null;
            this.parametros = new List<string>();
            this.tipo = "Error";
            this.nombre = "";
        }

    }
}
