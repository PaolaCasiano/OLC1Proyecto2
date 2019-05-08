using Irony.Parsing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto2.Objetos
{
    class Entorno
    {
        public Hashtable HT { get; set; }
        public String nombre { get; set; }

        public Entorno implementos { get; set; }
        public Entorno siguiente { get; set; }

        public Tipo Retorno { get; set; }

        public ParseTreeNode nodoimplemento { get; set; }
        
        public bool retorna = false;
        public bool esCase = false;
        public bool esCiclo = false;
        
        public Entorno()
        {
            HT = new Hashtable();
            nombre = "";
            siguiente = null;
        }

        public Entorno(String nombre)
        {
            HT = new Hashtable();
            this.nombre = nombre;
            siguiente = null;
        }


        public Entorno(String nombre, Entorno siguiente)
        {
            HT = new Hashtable();
            this.siguiente = siguiente;
            this.nombre = nombre;
        }
    }
}
