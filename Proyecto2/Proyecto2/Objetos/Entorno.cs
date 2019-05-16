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
        public Entorno metodoPrincipal { get; set; }
        public Entorno anterior { get; set; }


        public Tipo Retorno { get; set; }
        public string visibilidad { get; set; }

        public ParseTreeNode nodoimplemento { get; set; }
        public ParseTreeNode defecto { get; set;}
        
        public bool retorna = false;
        public bool esCase = false;
        public bool esCiclo = false;
        public bool salir = false;
        
        public Entorno()
        {
            HT = new Hashtable();
            nombre = "";
            visibilidad = "publico";
            siguiente = null;
        }

        public Entorno(String nombre)
        {
            HT = new Hashtable();
            this.nombre = nombre;
            siguiente = null;
            visibilidad = "publico";
        }


        public Entorno(String nombre, Entorno siguiente)
        {
            HT = new Hashtable();
            this.siguiente = siguiente;
            this.nombre = nombre;
            visibilidad = "publico";
        }

        public Entorno(Entorno clon)
        {
            this.HT = clon.HT;
            this.nombre = clon.nombre;
            this.implementos = clon.implementos;
            this.siguiente = clon.siguiente;
            this.metodoPrincipal = clon.metodoPrincipal;
            this.Retorno = clon.Retorno;
            this.nodoimplemento = clon.nodoimplemento;
            this.retorna = clon.retorna;
            this.esCase = clon.esCase;
            this.esCiclo = clon.esCiclo;
            this.salir = clon.salir;
            this.visibilidad = clon.visibilidad;
        }

        public void limpiarTabla(List<string> prm)
        {
            Hashtable copy = new Hashtable();
            foreach (String name in prm)
            {
                if (HT.Contains("var_" + name))
                {
                    copy.Add("var_" + name, (Tipo)HT["var_" + name]);
                }
            }

            this.HT = copy;
        }
    }
}
