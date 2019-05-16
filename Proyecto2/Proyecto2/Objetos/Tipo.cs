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
        public bool completaObjeto { get; set; }

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
            this.completaObjeto = false;
            this.entorno = null;
            this.parametros = new List<string>();
            this.tipo = "Error";
            this.nombre = "";
            this.arreglo = new List<Tipo>();
            this.visibilidad = "publico";
        }

        public Tipo(Tipo clon)
        {
            this.sobreescribe = clon.sobreescribe;
            this.esArray = clon.esArray;
            this.esmetodo = clon.esmetodo;
            this.completaObjeto = clon.completaObjeto;
            this.esObjeto = clon.esObjeto;
            if (clon.entorno != null)
            {
            this.entorno = new Entorno (clon.entorno);
            }else
            {
                this.entorno = null ;
            }
            this.cuerpo = clon.cuerpo;
            this.tipo = clon.tipo;
            this.nombre = clon.nombre;
            this.visibilidad = clon.visibilidad;
            this.fila = clon.fila;
            this.columna = clon.columna;
            this.valorString = clon.valorString;
            this.valorBoleano = clon.valorBoleano;
            this.valorEntero = clon.valorEntero;
            this.valorDouble = clon.valorDouble;
            this.valorChar = clon.valorChar;

            this.arreglo = clon.arreglo;
            this.tamano = clon.tamano;
            this.parametros = clon.parametros;

        }

        public void limpiarTabla()
        {
            if (this.esmetodo)
            {
                if (this.entorno != null)
                {
                    this.entorno.limpiarTabla(parametros);
                }
                
            }
        }

    }
}
