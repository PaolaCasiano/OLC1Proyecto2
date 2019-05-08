using Irony.Parsing;
using Proyecto2.Errores;
using Proyecto2.Objetos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proyecto2.Analizador
{

    class Accionar
    {
        public Entorno principal = null;
        public RichTextBox consola;
        Hashtable listaEntornos = new Hashtable();
        public Entorno actual;
        Validacion val;
        bool ejecutando;

        public Accionar(RichTextBox console)
        {
            this.consola = console;
        }

        public ArrayList errores { get; set; }

        public void guardar(ParseTreeNode padre)
        {
            ejecutando = false;
            foreach (ParseTreeNode hijo in padre.ChildNodes)
            {
                switch (hijo.ToString())
                {
                    case "S":
                        guardar(hijo);
                        break;
                    case "INICIO":
                        guardar(hijo);
                        break;
                    case "LISTACLASES":
                        if (listaEntornos.ContainsKey("ent_" + hijo.ChildNodes[0].Token.Text)) 
                        {
                            errores.Add(new Error(hijo.ChildNodes[0].Token.Location.Line, hijo.ChildNodes[0].Token.Location.Column, "Semantico", "Ya existe una Clase llamada " + hijo.ChildNodes[0].Token.Text));
                        }
                        else
                        {
                            actual = new Entorno(hijo.ChildNodes[0].Token.Text.ToLower());
                            listaEntornos.Add("ent_" + hijo.ChildNodes[0].Token.Text.ToLower(), actual);
                            guardar(hijo);
                        }
                        
                        break;
                    case "CUERPO":
                        guardar(hijo);
                        break;
                    case "LISTACUERPO":
                        if (actual.siguiente != null)
                        {
                            actual = actual.siguiente;
                        }
                        guardar(hijo);
                        break;
                    case "DECLARACIONOBJETOS":
                        AgregarVariables(hijo, ref actual);
                        break;
                    case "DECLARACION":
                        AgregarVariables(hijo, ref actual);
                        break;
                    case "IMPORTAR":
                        actual.nodoimplemento = hijo.ChildNodes[0];
                        break;
                    case "MAIN":
                        

                        if (!actual.HT.ContainsKey("main"))
                        {                            
                            if (principal == null)
                            {
                                Tipo timain = new Tipo();
                                timain.nombre = "main";
                                timain.esmetodo = true;
                                timain.tipo = "main";
                                timain.entorno = new Entorno("main", actual);
                                //timain.entorno.siguiente = actual;
                                timain.cuerpo = hijo.ChildNodes[0];
                                actual.HT.Add("main", timain);
                                principal = actual;
                            }
                        }else
                        {
                            Console.WriteLine("No pueden haber dos metodos main");
                        }
                        break;
                    case "METODO":
                        declararMetosG(hijo, ref actual);
                        break;

                }
            }
        }


        private void declararMetosG(ParseTreeNode nodo, ref Entorno ent)
        {
            ParseTreeNodeList hijos = nodo.ChildNodes;   
            switch (nodo.ChildNodes.Count)
            {
                case 5://id + TIPOMETODO + OVERRIDE + PARAMETROS + CUERPOMETODO//5
                    guardarMetodoG(
                        "publico"
                        , hijos[0].Token.Text
                        , false
                        , hijos[1].ChildNodes[0].ChildNodes[0].Token.Text
                        , null
                        , hijos[2].ChildNodes.Count >0
                        , hijos[3] //HACER METODO QUE DEVUELVA UNA LISTA DE TIPOS
                        , hijos[4]
                        );
                    break;
                case 6://VISIBILIDAD + id + TIPOMETODO + OVERRIDE  + PARAMETROS +  CUERPOMETODO //6
                    guardarMetodoG(
                       hijos[0].ChildNodes[0].Token.Text
                       , hijos[1].Token.Text
                       , false
                       , hijos[2].ChildNodes[0].ChildNodes[0].Token.Text
                       , null
                       , hijos[3].ChildNodes.Count > 0
                       , hijos[4] //HACER METODO QUE DEVUELVA UNA LISTA DE TIPOS
                       , hijos[5]
                       );
                    break;
                case 7://id + arreglo + TIPOMETODO + TAMANO + OVERRIDE + PARAMETROS + CUERPOMETODO//7
                    guardarMetodoG(
                        "publico"
                        , hijos[0].Token.Text
                        , true
                        , hijos[1].ChildNodes[0].ChildNodes[0].Token.Text
                        , getTamano(hijos[3],new List<int>(), ref ent)
                        , hijos[4].ChildNodes.Count > 0
                        , hijos[5]
                        , hijos[6]
                        );
                    break;
                case 8://VISIBILIDAD + id + arreglo + TIPOMETODO + TAMANO + OVERRIDE + PARAMETROS + CUERPOMETODO//8
                    guardarMetodoG(
                        hijos[0].ChildNodes[0].Token.Text
                        , hijos[1].Token.Text
                        , true
                        , hijos[3].ChildNodes[0].ChildNodes[0].Token.Text
                        , getTamano(hijos[4], new List<int>(), ref ent)
                        , hijos[5].ChildNodes.Count > 0
                        , hijos[6]
                        , hijos[7]
                        );
                    break;
            }
        }

        private void guardarMetodoG(string visibilidad, string id, bool esArray, string tipo, List<int> tamano, bool sobreescribe, ParseTreeNode parametros, ParseTreeNode cuerpo)
        {
            Tipo metodo = new Tipo();
            tipo = changeTipo(tipo);
            metodo.esmetodo = true;
            metodo.visibilidad = visibilidad.ToLower();
            metodo.nombre = id;
            metodo.esArray = esArray;
            metodo.tipo = tipo;
            if (esArray)
            {
                metodo.tamano = tamano;
            }
            metodo.sobreescribe = sobreescribe;
            metodo.cuerpo = cuerpo;
            metodo.entorno = new Entorno(metodo.nombre, actual);
            if (!tipo.Equals("void", StringComparison.InvariantCultureIgnoreCase))
            {
                metodo.entorno.retorna = true;
            }
            agregarParametros(parametros, ref metodo);
            //metodo.entorno = entorno;
            actual.HT.Add("met_" + metodo.nombre.ToLower(), metodo);
            //Console.WriteLine("hola");

        }

        private void agregarParametros(ParseTreeNode parametros, ref Tipo metodo)
        {
            String nombre;
            switch (parametros.ChildNodes.Count)
            {
                case 3:
                    agregarParametros(parametros.ChildNodes[0],ref metodo);
                    nombre = parametros.ChildNodes[2].Token.Text;
                    if (!metodo.entorno.HT.Contains("var_" + nombre.ToLower()))
                    {
                        Tipo var = new Tipo();
                        var.nombre = nombre;
                        var.visibilidad = "publico";
                        var.tipo = changeTipo(parametros.ChildNodes[1].ChildNodes[0].Token.Text);
                        metodo.parametros.Add(nombre.ToLower());
                        metodo.entorno.HT.Add("var_" + nombre.ToLower(), var);
                        
                    }
                    else
                    {
                        errores.Add(new Error(parametros.ChildNodes[2].Token.Location.Line, parametros.ChildNodes[2].Token.Location.Column, "Semantico", "La Variable " + nombre + " ya existe en el entorno actual"));
                    }
                    break;
                case 2:
                    nombre = parametros.ChildNodes[1].Token.Text;
                    if (!metodo.entorno.HT.Contains("var_" + nombre.ToLower()))
                    {
                        Tipo var = new Tipo();
                        var.nombre = nombre;
                        var.visibilidad = "publico";
                        var.tipo = changeTipo(parametros.ChildNodes[0].ChildNodes[0].Token.Text);
                        metodo.entorno.HT.Add("var_" + nombre.ToLower(), var);
                        metodo.parametros.Add( nombre.ToLower());
                    }
                    else
                    {
                        errores.Add(new Error(parametros.ChildNodes[1].Token.Location.Line, parametros.ChildNodes[1].Token.Location.Column, "Semantico", "La Variable " + nombre + " ya existe en el entorno actual"));
                    }
                    break;
            }
        }

        public void ejecutar(ParseTreeNode padre, ref Entorno actual)
        {
            ejecutando = true;
            foreach (ParseTreeNode hijo in padre.ChildNodes)
            {
                switch (hijo.ToString())
                {
                    case "CUERPOMETODO":
                        ejecutar(hijo, ref actual);
                        break;
                    case "INICIO":
                        ejecutar(hijo, ref actual);
                        break;
                    case "FUNCIONMETODO":
                        ejecutar(hijo, ref actual);
                        break;
                    case "DECLARACIONOBJETO":
                        AgregarVariables(hijo, ref actual);
                        break;
                    case "DECLARACION":
                        AgregarVariables(hijo, ref actual);
                        break;
                    case "ASIGNACION":
                        ejecutarAsignacion(hijo, ref actual);
                        break;
                    case "LLAMAR":
                        ejecutarLlamada(hijo, false, ref actual);
                        break;
                    case "PRINT":
                        print(hijo, ref actual);
                        break;
                    case "SHOW":
                        show(hijo, ref actual);
                        break;
                    case "SI":
                        ejecutarIf(hijo, ref actual);
                        break;
                    case "RETORNAR":
                        Retornar(hijo, ref actual);
                        break;
                    case "PARA":
                        ejecutarFor(hijo, ref actual);
                        break;
                    case "REPETIR":
                        repetir(hijo, ref actual);
                        break;
                    case "MIENTRAS":
                        mientras(hijo, ref actual);
                        break;
                    case "COMPROBAR":
                        break;
                    case "HACERMIENTRAS":
                        hacerMientras(hijo, ref actual);
                        break;
                    case "AUMENTODECREMENTO":
                        aumentoDecremento(hijo, ref actual);
                        break;
                    case "LOCALMETODO":
                        ejecutarMetodoLocal(hijo, false, ref actual);//es una asignacion, siempre debe devolver algo 7n7
                        break;


                }
            }
            
        }

        public Entorno agregarEntorno(ParseTreeNode hijo, ref Entorno ent)
        {
            String nombre = hijo.ChildNodes[0].Token.Text.ToLower();
            switch (hijo.ChildNodes.Count)
            {
                case 1:
                    if (listaEntornos.ContainsKey("ent_" + nombre ))
                    {
                        
                        return (Entorno)listaEntornos["ent_" + nombre];
                    }else
                    {
                        errores.Add(new Error(hijo.ChildNodes[0].Token.Location.Line, hijo.ChildNodes[0].Token.Location.Column, "Semantico", "La clase " + hijo.ChildNodes[0].Token.Text + " No existe"));
                    }
                    break;
                case 2:
                    if (listaEntornos.ContainsKey("ent_" + nombre))
                    {
                        //string key = hijo.ChildNodes[1].Token.Text;
                        Entorno nuevo = (Entorno)listaEntornos["ent_" + nombre];
                        nuevo.siguiente = agregarEntorno(hijo.ChildNodes[0], ref ent);
                        return nuevo;
                    }
                    else
                    {
                        errores.Add(new Error(hijo.ChildNodes[1].Token.Location.Line, hijo.ChildNodes[1].Token.Location.Column, "Semantico", "La clase " + hijo.ChildNodes[1].Token.Text + " No existe"));
                    }
                    break;

            }
            return new Entorno();
        }

        private void AgregarVariables(ParseTreeNode Padre, ref Entorno ent)
        {
            switch (Padre.ChildNodes.Count)
            {
                case 2:
                    declararVariable("publico", Padre.ChildNodes[0].ChildNodes[0].Token.Text, false, Padre.ChildNodes[1], null, ref ent);
                    break;
                case 3:
                    if (Padre.ChildNodes[0].ToString().Equals("visibilidad", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // se declara el que es VISIBILIDAD TIPO IDS 
                        declararVariable(
                           Padre.ChildNodes[0].ChildNodes[0].Token.Text
                           , Padre.ChildNodes[1].ChildNodes[0].Token.Text
                           , false
                           , Padre.ChildNodes[2]
                           , null
                           , ref ent
                           );
                    }else
                    {
                        // se declara el que es TIPO IDS IGUALACION
                        declararVariableconValor(
                          "publico"
                          , Padre.ChildNodes[0].ChildNodes[0].Token.Text
                          , false
                          , Padre.ChildNodes[1]
                          , null
                          , Verificacion(Padre.ChildNodes[2].ChildNodes[0], ref ent)
                          , ref ent
                          );

                    }
                        
                    break;
                case 4:
                    if (!Padre.ChildNodes[0].ToString().Equals("visibilidad", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // se declara el que es TIPO ARRAY IDS TAMANO
                        declararVariable(
                            "publico"
                            , Padre.ChildNodes[0].ChildNodes[0].Token.Text
                            , true
                            , Padre.ChildNodes[2]
                            , getTamano(Padre.ChildNodes[3], new List<int>(), ref ent)
                            , ref ent
                            );
                    }
                    else
                    {
                        // se declara el que es VISIBILIDAD TIPO IDS IGUALACION
                        declararVariableconValor(
                           Padre.ChildNodes[0].ChildNodes[0].Token.Text
                          , Padre.ChildNodes[1].ChildNodes[0].Token.Text
                          , false
                          , Padre.ChildNodes[2]
                          , null
                          , Verificacion(Padre.ChildNodes[3].ChildNodes[0], ref ent)
                          , ref ent
                          );
                    }
                    break;
                case 5:
                    if (Padre.ChildNodes[0].ToString().Equals("visibilidad", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // se declara el que es VISIBILIDAD TIPO ARREGLO IDS TAMANO
                        declararVariable(
                            Padre.ChildNodes[0].ChildNodes[0].Token.Text
                            , Padre.ChildNodes[1].ChildNodes[0].Token.Text
                            , true
                            , Padre.ChildNodes[3]
                            , getTamano(Padre.ChildNodes[4], new List<int>(), ref ent)
                            , ref ent
                            );
                    }
                    else
                    {
                        // se declara el que es TIPO ARRAY VARIABLES TAMANO IGUALACION
                        declararVariableconValor(
                           "publico"
                          , Padre.ChildNodes[0].ChildNodes[0].Token.Text
                          , true
                          , Padre.ChildNodes[2]
                          , getTamano(Padre.ChildNodes[3], new List<int>(), ref ent)
                          , new Tipo()//Verificacion(Padre.ChildNodes[3].ChildNodes[0])
                          , ref ent
                          );
                    }
                    break;
                case 6:
                    //VISIBILIDAD TIPO ARRAY LISTAVAR TAMANO IGUALACION
                    declararVariableconValor(
                          Padre.ChildNodes[0].ChildNodes[0].Token.Text
                         , Padre.ChildNodes[1].ChildNodes[0].Token.Text
                         , true
                         , Padre.ChildNodes[3]
                         , getTamano(Padre.ChildNodes[4], new List<int>(), ref ent)
                         , new Tipo()//Verificacion(Padre.ChildNodes[3].ChildNodes[0])
                         , ref ent
                         );
                    break;
            }
        }

        private void declararVariable(string visibilidad, string tipo, bool arreglo, ParseTreeNode ids, List<int> tamano,  ref Entorno actual)
        {
            tipo = changeTipo(tipo);
            if (ids.ChildNodes.Count > 0)
            {
                foreach (ParseTreeNode id in ids.ChildNodes)
                {
                    declararVariable(visibilidad, tipo, arreglo, id, tamano, ref actual);
                }
            }else
            {
                String nombre = ids.Token.Text;
                if (!actual.HT.Contains("var_" + nombre.ToLower()))
                {
                    Tipo var = new Tipo();
                    var.nombre = nombre;
                    var.visibilidad = visibilidad;
                    var.tipo = tipo;
                    var.esArray = arreglo;
                    if (arreglo)
                    {
                        var.tamano = tamano;
                    }
                    actual.HT.Add("var_" + nombre.ToLower(), var);
                }else
                {
                    errores.Add(new Error(ids.Token.Location.Line, ids.Token.Location.Column, "Semantico", "La Variable "+nombre+" ya existe en el entorno actual"));
                }
            }
        }

        private void declararVariableconValor(string visibilidad, string tipo, bool arreglo, ParseTreeNode ids, List<int> tamano, Tipo value, ref Entorno actual)
        {
            tipo = changeTipo(tipo);
            if (ids.ChildNodes.Count > 0)
            {
                foreach (ParseTreeNode id in ids.ChildNodes)
                {
                    declararVariableconValor(visibilidad, tipo, arreglo, id, tamano,value, ref actual);
                }
            }
            else
            {
                if (tipo.Equals(value.tipo,StringComparison.InvariantCultureIgnoreCase))
                {
                    String nombre = ids.Token.Text;
                    if (!actual.HT.Contains("var_" + nombre.ToLower()))
                    {
                        Tipo var = value;
                        var.nombre = nombre;
                        var.visibilidad = visibilidad;
                        var.tipo = tipo;
                        var.esArray = arreglo;
                        if (arreglo)
                        {
                            var.tamano = tamano;
                        }
                        actual.HT.Add("var_" + nombre.ToLower(), var);
                    }
                    else
                    {
                        errores.Add(new Error(ids.Token.Location.Line, ids.Token.Location.Column, "Semantico", "La Variable " + nombre + " ya existe en el entorno actual"));
                    }
                }else
                {
                    errores.Add(new Error(ids.Token.Location.Line, ids.Token.Location.Column, "Semantico", "No se puede declarar a un tipo "+tipo+" un valor tipo "+value.tipo));
                }
               
            }
        }

        private List<int> getTamano(ParseTreeNode nodo, List<int> lista, ref Entorno ent)
        {
            Tipo numer;
            switch (nodo.ChildNodes.Count)
            {
                case 1:
                    numer = Verificacion(nodo.ChildNodes[0], ref ent);
                    if (numer.tipo.Equals("entero",StringComparison.InvariantCultureIgnoreCase))
                    {
                        lista.Add(numer.valorEntero);
                        return lista;
                    }else
                    {
                        errores.Add(new Error(numer.fila, numer.columna, "Semantico", "No se puede declarar a un tipo " + numer.tipo+ " como tamaño de un arreglo"));
                    }
                    break;
                case 2:
                    {
                        numer = Verificacion(nodo.ChildNodes[1], ref ent);
                        if (numer.tipo.Equals("entero"))
                        {
                            lista = getTamano(nodo.ChildNodes[0], lista, ref ent);
                            lista.Add(numer.valorEntero);
                            return lista;
                        }
                        else
                        {
                            errores.Add(new Error(numer.fila, numer.columna, "Semantico", "No se puede declarar a un tipo " + numer.tipo + " como tamaño de un arreglo"));
                        }
                        
                    }
                    break;
            }
            return lista;   
        }

        private Tipo Verificacion(ParseTreeNode nodo, ref Entorno actual)
        {
            switch (nodo.ToString())
            {
                case "L":
                    switch (nodo.ChildNodes.Count)
                    {
                        case 2:
                            if (nodo.ChildNodes[1].ToString().Equals("E"))
                            {
                                return comprobacionTipos(Verificacion(nodo.ChildNodes[1], ref actual), "-", null);
                            }else
                            {
                                //RETORNAR LA BUSQUEDA DE UNA VARIABLE DE UN OBJETO, COMO UN CLASE.ATRIBUTO.
                                Tipo variable = buscarVariable(nodo.ChildNodes[0].Token.Text, actual, false);
                                if (variable != null)
                                {
                                    if (variable.esObjeto)
                                    {
                                        Tipo ret = buscarVariable(nodo.ChildNodes[1].Token.Text, variable.entorno, false);
                                        if(ret!= null)
                                        {
                                            return ret;
                                        }else
                                        {
                                            errores.Add(
                                            new Error(nodo.ChildNodes[1].Token.Location.Line
                                            , nodo.ChildNodes[1].Token.Location.Column
                                            , "Semántico"
                                            , "No se encontro la variable " + nodo.ChildNodes[1].Token.Text));
                                        }
                                    }else
                                    {
                                        errores.Add(
                                           new Error(nodo.ChildNodes[1].Token.Location.Line
                                           , nodo.ChildNodes[1].Token.Location.Column
                                           , "Semántico"
                                           , "La variable " + nodo.ChildNodes[1].Token.Text + " no es un objeto"));
                                    }
                                    
                                }else
                                {
                                    errores.Add(
                                           new Error(nodo.ChildNodes[0].Token.Location.Line
                                           , nodo.ChildNodes[0].Token.Location.Column
                                           , "Semántico"
                                           , "No se encontro la variable " + nodo.ChildNodes[0].Token.Text));
                                }
                            }
                            break;
                        case 1:
                            switch (nodo.ChildNodes[0].ToString())
                            {
                                case "E":
                                    return Verificacion(nodo.ChildNodes[0], ref actual);
                                case "LLAMAR":
                                    if (ejecutando)
                                    {
                                        return ejecutarLlamada(nodo.ChildNodes[0], true, ref actual);
                                    }else
                                    {
                                        errores.Add(
                                            new Error(nodo.ChildNodes[0].ChildNodes[0].Token.Location.Line
                                            , nodo.ChildNodes[0].ChildNodes[0].Token.Location.Column
                                            , "Semántico"
                                            , "No se ejecutar una llamada fuera de un metodo"));
                                    }
                                    //LLAMAR A UN METODO DE UN OBJETO, VERIFICAR SI DEVUELVE 
                                    break;
                                case "NUEVACLASE":
                                    //CREAR UNA VARIABLE TIPO CLASE CON ATRIBUTOS Y TAL, ESE PUEDE SER UN ENTORNO?
                                    if (ejecutando)
                                    {
                                        return nuevoObjeto(nodo.ChildNodes[0].ChildNodes[0]);
                                    }
                                    else
                                    {
                                        errores.Add(
                                            new Error(nodo.ChildNodes[0].ChildNodes[0].Token.Location.Line
                                            , nodo.ChildNodes[0].ChildNodes[0].Token.Location.Column
                                            , "Semántico"
                                            , "No se puede inicializar una clase fuera de un metodo"));
                                    }
                                    break;
                                case "LOCALMETODO":
                                    //LLAMAR A UN METODO DE LA MISMA CLASE 
                                    if (ejecutando)
                                    {
                                        return ejecutarMetodoLocal(nodo.ChildNodes[0], false, ref actual); //no es necesario devolver un valor aca :3
                                    }
                                    else
                                    {
                                        errores.Add(
                                            new Error(nodo.ChildNodes[0].ChildNodes[0].Token.Location.Line
                                            , nodo.ChildNodes[0].ChildNodes[0].Token.Location.Column
                                            , "Semántico"
                                            , "No se puede ejecutar una llamada fuera de un metodo"));
                                    }
                                    break;
                                case "OBJETO":
                                    //CREAR UN OBJETO TIPO {{},{}}
                                    crearObjeto(nodo.ChildNodes[0]);
                                    break;
                                case "VALORARRAY":
                                    //BUSCAR UN ARRAY Y BUSCAR LA POSCION ARREGLO[POSICION]
                                    if (ejecutando)
                                    {
                                        
                                    }
                                    else
                                    {
                                        errores.Add(
                                            new Error(nodo.ChildNodes[0].ChildNodes[0].Token.Location.Line
                                            , nodo.ChildNodes[0].ChildNodes[0].Token.Location.Column
                                            , "Semántico"
                                            , "No se puede traer el valor de un arreglo fuera de un metodo"));
                                    }
                                    break;
                                case "AUMENTODECREMENTO":
                                    //BUSCAR UN VAR++ O VAR--
                                    return (aumentoDecremento(nodo.ChildNodes[0], ref actual));
                                default:
                                    Tipo nuevo = new Tipo() ;
                                    nuevo.tipo = "Error";
                                    nuevo.fila = nodo.ChildNodes[0].Token.Location.Line;
                                    nuevo.columna = nodo.ChildNodes[0].Token.Location.Column;
                                    switch (nodo.ChildNodes[0].Term.Name.ToLower())
                                    {
                                        case "id":
                                            String name = nodo.ChildNodes[0].Token.Text.ToLower();
                                            nuevo = buscarVariable(name, actual, false) ;
                                            if(nuevo == null)
                                            {
                                                nuevo = new Tipo();
                                                nuevo.fila = nodo.ChildNodes[0].Token.Location.Line;
                                                nuevo.columna = nodo.ChildNodes[0].Token.Location.Column;
                                                errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "No existe la variable " + name));
                                            }
                                            else
                                            {
                                                nuevo.fila = nodo.ChildNodes[0].Token.Location.Line;
                                                nuevo.columna = nodo.ChildNodes[0].Token.Location.Column;
                                            }
                                            //do
                                            //{
                                            //    if (temp.HT.ContainsKey("var_"+ name ))
                                            //    {
                                            //        nuevo = (Tipo)temp.HT["var_" + name];
                                            //        nuevo.fila = nodo.ChildNodes[0].Token.Location.Line;
                                            //        nuevo.columna = nodo.ChildNodes[0].Token.Location.Column;
                                            //        return nuevo;
                                            //    }
                                            //    if(temp.siguiente== null)
                                            //    {
                                            //        break;
                                            //    }

                                            //    temp = temp.siguiente;
                                            //} while (temp != null);

                                            //if(temp.implementos!= null)
                                            //{
                                            //    temp = temp.implementos;
                                            //    do
                                            //    {
                                            //        if (temp.HT.ContainsKey("var_" + name))
                                            //        {
                                            //            nuevo = (Tipo)temp.HT["var_" + name];
                                            //            nuevo.fila = nodo.ChildNodes[0].Token.Location.Line;
                                            //            nuevo.columna = nodo.ChildNodes[0].Token.Location.Column;
                                            //            return nuevo;
                                            //        }
                                            //        temp = temp.siguiente;

                                            //    } while (temp != null);
                                            //}

                                            
                                            break;
                                        case "entero":
                                            nuevo.tipo = "entero";
                                            nuevo.valorEntero = int.Parse(nodo.ChildNodes[0].Token.ValueString);
                                            nuevo.valorDouble = int.Parse(nodo.ChildNodes[0].Token.ValueString);
                                            nuevo.valorString = nodo.ChildNodes[0].Token.ValueString;
                                            break;
                                        case "cadena":
                                            nuevo.tipo = "cadena";
                                            nuevo.valorString = nodo.ChildNodes[0].Token.Text.Substring(1,nodo.ChildNodes[0].Token.Text.Length-2);
                                            break;
                                        case "decimal":
                                            nuevo.tipo = "decimal";
                                            nuevo.valorDouble = double.Parse(nodo.ChildNodes[0].Token.ValueString);
                                            nuevo.valorEntero = Convert.ToInt32(double.Parse(nodo.ChildNodes[0].Token.ValueString));
                                            nuevo.valorString = nodo.ChildNodes[0].Token.ValueString;
                                            break;
                                        case "char":
                                            nuevo.tipo = "char";
                                            nuevo.valorChar = nodo.ChildNodes[0].Token.ToString().ToCharArray()[1];
                                            nuevo.valorString = nodo.ChildNodes[0].Token.ToString().Substring(1, nodo.ChildNodes[0].Token.Text.Length - 1);
                                            nuevo.valorEntero = (int)nuevo.valorChar;
                                            nuevo.valorDouble = (int)nuevo.valorChar;
                                            break;
                                        case "verdadero":
                                            nuevo.tipo = "booleano";
                                            nuevo.valorBoleano = true;
                                            nuevo.valorEntero = 1;
                                            nuevo.valorDouble = 1;
                                            nuevo.valorString = "verdadero";
                                            break;
                                        case "falso":
                                            nuevo.tipo = "booleano";
                                            nuevo.valorBoleano = false;
                                            nuevo.valorEntero= 0;
                                            nuevo.valorDouble = 0;
                                            nuevo.valorString = "falso";
                                            break;

                                    }
                                    return nuevo;
                            }
                            break;
                                
                           
                    }
                    break;

                case "G":
                    switch (nodo.ChildNodes.Count)
                    {
                        case 1:
                            return Verificacion(nodo.ChildNodes[0], ref actual);
                        case 2:
                            return comprobacionTipos(
                                Verificacion(nodo.ChildNodes[1], ref actual)
                                , nodo.ChildNodes[0].Token.Text
                                , null);
                    }

                    break;
                default:
                    switch (nodo.ChildNodes.Count)
                    {
                        case 1:
                            return Verificacion(nodo.ChildNodes[0], ref actual);
                        case 3:
                            return comprobacionTipos(
                                Verificacion(nodo.ChildNodes[0], ref actual)
                                , nodo.ChildNodes[1].Token.Text
                                , Verificacion(nodo.ChildNodes[2], ref actual));
                        case 2:

                            return comprobacionTipos(Verificacion(nodo.ChildNodes[1], ref actual)
                               , nodo.ChildNodes[0].Token.Text
                               , null);
                            //return comprobacionTipos(
                            //    Verificacion(nodo.ChildNodes[0])
                            //    , nodo.ChildNodes[1].Token.Text
                            //    , null);
                    }
                    break;

            }
            Tipo terror = new Tipo();
            terror.tipo = "Error";
            return terror;//D:
        }

        private Tipo comprobacionTipos(Tipo v1, string op, Tipo v2)
        {
            Tipo retorno = new Tipo() ;
            retorno.tipo = "Error";
            switch (op)
            {
                case "||":
                    if(v1.tipo.Equals("booleano",StringComparison.InvariantCultureIgnoreCase) && v2.tipo.Equals("booleano", StringComparison.InvariantCultureIgnoreCase))
                    {
                        retorno.tipo = "booleano";
                        retorno.valorBoleano = v1.valorBoleano || v2.valorBoleano;
                        return retorno;
                    }else
                    {
                        errores.Add(new Error(v1.fila, v1.columna, "Semantico", "No se puede comparar un tipo "+v1.tipo + " y "+v2.tipo));
                    }
                    break;
                case "&&":
                    if (v1.tipo.Equals("booleano", StringComparison.InvariantCultureIgnoreCase) && v2.tipo.Equals("booleano", StringComparison.InvariantCultureIgnoreCase))
                    {
                        retorno.tipo = "booleano";
                        retorno.valorBoleano = v1.valorBoleano && v2.valorBoleano;
                        return retorno;
                    }
                    else
                    {
                        errores.Add(new Error(v1.fila, v1.columna, "Semantico", "No se puede comparar un tipo " + v1.tipo + " y " + v2.tipo));
                    }
                    break;
                case "!":
                    if (v1.tipo.Equals("booleano", StringComparison.InvariantCultureIgnoreCase))
                    {
                        retorno.tipo = "booleano";
                        retorno.valorBoleano = !v1.valorBoleano ;
                        return retorno;
                    }
                    else
                    {
                        errores.Add(new Error(v1.fila, v1.columna, "Semantico", "No se puede negar valor un tipo " + v1.tipo ));
                    }
                    break;
                case "+":
                    val = validoSuma(v1.tipo, v2.tipo);
                    if (val.valido)
                    {
                        retorno.tipo = val.tipo;
                        switch (val.tipo)
                        {
                            case "cadena":
                                retorno.valorString = v1.valorString + v2.valorString;
                                break;
                            case "decimal":
                                retorno.valorDouble = v1.valorDouble + v2.valorDouble;
                                retorno.valorEntero = Convert.ToInt32(retorno.valorDouble);
                                retorno.valorString = retorno.valorDouble.ToString();
                                break;
                            case "entero":
                                retorno.valorDouble = v1.valorEntero + v2.valorEntero;
                                retorno.valorEntero = v1.valorEntero + v2.valorEntero;
                                retorno.valorString = retorno.valorEntero.ToString();
                                break;
                            case "booleano":
                                retorno.valorBoleano = v1.valorBoleano || v2.valorBoleano;
                                retorno.valorEntero = retorno.valorBoleano?1:0 ;
                                retorno.valorDouble = retorno.valorEntero;
                                break;

                        }
                    }else
                    {
                        errores.Add(new Error(v1.fila, v1.columna, "Semantico", val.mensaje));
                    }
                    break;

                case "-":
                    val = validoResta(v1.tipo, v2.tipo);
                    if (val.valido)
                    {
                        retorno.tipo = val.tipo;
                        switch (val.tipo)
                        {
                            
                            case "decimal":
                                retorno.valorDouble = v1.valorDouble - v2.valorDouble;
                                retorno.valorEntero = Convert.ToInt32(retorno.valorDouble);
                                retorno.valorString = retorno.valorDouble.ToString();
                                break;
                            case "entero":
                                retorno.valorEntero = v1.valorEntero - v2.valorEntero;
                                retorno.valorDouble = retorno.valorEntero;                                
                                retorno.valorString = retorno.valorEntero.ToString();
                                break;
                        }
                    }
                    else
                    {
                        errores.Add(new Error(v1.fila, v1.columna, "Semantico", val.mensaje));
                    }
                    break;

                case "*":
                    val = validoMultiplicacion(v1.tipo, v2.tipo);
                    if (val.valido)
                    {
                        retorno.tipo = val.tipo;
                        switch (val.tipo)
                        {

                            case "decimal":
                                retorno.valorDouble = v1.valorDouble * v2.valorDouble;
                                retorno.valorEntero = Convert.ToInt32(retorno.valorDouble);
                                retorno.valorString = retorno.valorDouble.ToString();
                                break;
                            case "entero":
                                retorno.valorEntero = v1.valorEntero * v2.valorEntero;
                                retorno.valorDouble = retorno.valorEntero;
                                retorno.valorString = retorno.valorEntero.ToString();
                                break;
                            case "booleano":
                                retorno.valorBoleano = v1.valorBoleano && v2.valorBoleano;
                                retorno.valorEntero = retorno.valorBoleano ? 1 : 0;
                                retorno.valorDouble = retorno.valorEntero;
                                break;
                        }
                    }
                    else
                    {
                        errores.Add(new Error(v1.fila, v1.columna, "Semantico", val.mensaje));
                    }
                    break;
                case "/":
                    val = validoDivision(v1.tipo, v2.tipo, "dividir");
                    if (val.valido)
                    {
                        if(v2.valorDouble == 0)
                        {
                            errores.Add(new Error(v1.fila, v1.columna, "Semantico", "No se puede dividir entre 0 7n7"));
                        }
                        else
                        {
                            retorno.tipo = val.tipo;
                            switch (val.tipo)
                            {

                                case "decimal":
                                    retorno.valorDouble = v1.valorDouble / v2.valorDouble;
                                    retorno.valorEntero = Convert.ToInt32(retorno.valorDouble);
                                    retorno.valorString = retorno.valorDouble.ToString();
                                    break;
                                case "entero":
                                    retorno.valorEntero = v1.valorEntero / v2.valorEntero;
                                    retorno.valorDouble = retorno.valorEntero;
                                    retorno.valorString = retorno.valorEntero.ToString();
                                    break;
                            }
                        }
                        
                    }
                    else
                    {
                        errores.Add(new Error(v1.fila, v1.columna, "Semantico", val.mensaje));
                    }
                    break;
                case "^":
                    val = validoDivision(v1.tipo, v2.tipo, "elevar");
                    if (val.valido)
                    {
                        retorno.tipo = val.tipo;
                        switch (val.tipo)
                        {

                            case "decimal":
                                retorno.valorDouble = Math.Pow (v1.valorDouble , v2.valorDouble);
                                retorno.valorEntero = Convert.ToInt32(retorno.valorDouble);
                                retorno.valorString = retorno.valorDouble.ToString();
                                break;
                            case "entero":
                                retorno.valorEntero = Convert.ToInt32(Math.Pow(v1.valorEntero, v2.valorEntero));
                                retorno.valorDouble = retorno.valorEntero;
                                retorno.valorString = retorno.valorEntero.ToString();
                                break;
                        }
                    }
                    else
                    {
                        errores.Add(new Error(v1.fila, v1.columna, "Semantico", val.mensaje));
                    }
                    break;
                case "++":
                    if(v1.tipo.Equals("cadena")|| v1.tipo.Equals("booleano"))
                    {
                        errores.Add(new Error(v1.fila, v1.columna, "Semantico", "No se puede aumentar un tipo " + v1.tipo));
                    }else
                    {
                        retorno.tipo = v1.tipo;
                        retorno.valorEntero = v1.valorEntero++;
                        retorno.valorDouble = v1.valorDouble++;
                        if (v1.tipo.Equals("char"))
                        {
                            retorno.valorChar = Convert.ToChar(retorno.valorEntero);
                        }
                    }
                    break;
                case "--":
                    if (v1.tipo.Equals("cadena") || v1.tipo.Equals("booleano"))
                    {
                        errores.Add(new Error(v1.fila, v1.columna, "Semantico", "No se puede disminuir un tipo " + v1.tipo));
                    }
                    else
                    {
                        retorno.tipo = v1.tipo;
                        retorno.valorEntero = v1.valorEntero--;
                        retorno.valorDouble = v1.valorDouble--;
                        if (v1.tipo.Equals("char"))
                        {
                            retorno.valorChar = Convert.ToChar(retorno.valorEntero--);
                        }
                    }
                    break;
                case ">":
                    
                    if (v1.tipo.Equals("cadena") || v2.tipo.Equals("cadena"))
                    {
                        errores.Add(new Error(v1.fila, v1.columna, "Semantico", "No se puede comparar con > un tipo "+v1.tipo +" con un tipo "+ v2.tipo));
                    }
                    else
                    {
                        retorno.tipo = "booleano";
                        retorno.valorBoleano = v1.valorDouble > v2.valorDouble;
                    }
                    break;
                case "<":

                    if (v1.tipo.Equals("cadena") || v2.tipo.Equals("cadena"))
                    {
                        errores.Add(new Error(v1.fila, v1.columna, "Semantico", "No se puede comparar con < un tipo " + v1.tipo + " con un tipo " + v2.tipo));
                    }
                    else
                    {
                        retorno.tipo = "booleano";
                        retorno.valorBoleano = v1.valorDouble < v2.valorDouble;
                    }
                    break;
                case ">=":

                    if (v1.tipo.Equals("cadena") || v2.tipo.Equals("cadena"))
                    {
                        errores.Add(new Error(v1.fila, v1.columna, "Semantico", "No se puede comparar con >= un tipo " + v1.tipo + " con un tipo " + v2.tipo));
                    }
                    else
                    {
                        retorno.tipo = "booleano";
                        retorno.valorBoleano = v1.valorDouble >= v2.valorDouble;
                    }
                    break;
                case "<=":

                    if (v1.tipo.Equals("cadena") || v2.tipo.Equals("cadena"))
                    {
                        errores.Add(new Error(v1.fila, v1.columna, "Semantico", "No se puede comparar con <= un tipo " + v1.tipo + " con un tipo " + v2.tipo));
                    }
                    else
                    {
                        retorno.tipo = "booleano";
                        retorno.valorBoleano = v1.valorDouble > v2.valorDouble;
                    }
                    break;
                case "==":
                        retorno.tipo = "booleano";
                        retorno.valorBoleano = v1.valorString == v2.valorString;
                    break;
                case "!=":
                    retorno.tipo = "booleano";
                    retorno.valorBoleano = v1.valorString == v2.valorString;
                    break;
                default:
                    errores.Add(new Error(v1.fila, v1.columna, "Semantico", "No hay operacion" + op + " entre un tipo " + v1.tipo + " con un tipo " + v2.tipo));
                    break;
            }
            return retorno;
        }

        public Validacion validoSuma(string t1, string t2)
        {
            Validacion val = new Validacion();
            val.tipo = "Error";
            if (t1.Equals("Error") || t2.Equals("Error"))
            {
                val.valido = false;
                val.mensaje = "No se puede sumar un valor tipo " + t1 + " con  " + t2;
            }
            else if (t1.Equals("cadena") || t2.Equals("cadena"))
            {
                if (t1.Equals("booleano") || t2.Equals("booleano"))
                {
                    val.valido = false;
                    val.mensaje = "No se puede sumar un valor tipo " + t1 + " con  " + t2;
                }
                else
                {
                    val.valido = true;
                    val.tipo = "cadena";
                }
            }
            else if (t1.Equals("decimal") || t2.Equals("decimal"))
            {
                val.valido = true;
                val.tipo = "decimal";
            }
            else if (t1.Equals("entero") || t2.Equals("entero"))
            {
                val.valido = true;
                val.tipo = "entero";
            }
            else if (t1.Equals("char") || t2.Equals("char"))
            {
                val.valido = true;
                val.tipo = "entero";
            }
            else if (t1.Equals("booleano") || t2.Equals("booleano"))
            {
                val.valido = true;
                val.tipo = "booleano";
            }
            return val;
        }

        public Validacion validoResta(string t1, string t2)
        {
            Validacion val = new Validacion();
            if (t1.Equals("Error") || t2.Equals("Error"))
            {
                val.valido = false;
                val.mensaje = "No se puede restar un valor tipo " + t1 + " con  " + t2;
            }
            else if (t1.Equals("cadena") || t2.Equals("cadena"))
            {
                val.valido = false;
                val.mensaje = "No se puede restar un valor tipo " + t1 + " con  " + t2;
            }
            else if (t1.Equals("decimal") || t2.Equals("decimal"))
            {
                val.valido = true;
                val.tipo = "decimal";
            }
            else if (t1.Equals("entero") || t2.Equals("entero"))
            {
                val.valido = true;
                val.tipo = "entero";
            }
            else if (t1.Equals("char") || t2.Equals("char"))
            {
                if (t1.Equals("booleano") || t2.Equals("booleano"))
                {
                    val.valido = false;
                    val.mensaje = "No se puede restar un valor tipo " + t1 + " con  " + t2;
                }
                else
                {
                    val.valido = true;
                    val.tipo = "entero";
                }

            }
            else if (t1.Equals("booleano") || t2.Equals("booleano"))
            {
                val.valido = false;
                val.mensaje = "No se puede restar un valor tipo " + t1 + " con  " + t2;
            }

            return val;
        }

        public Validacion validoMultiplicacion(string t1, string t2)
        {
            Validacion val = new Validacion();
            val.tipo = "Error";
            if (t1.Equals("Error") || t2.Equals("Error"))
            {
                val.valido = false;
                val.mensaje = "No se puede multiplicar un valor tipo " + t1 + " con  " + t2;
            }
            else if (t1.Equals("cadena") || t2.Equals("cadena"))
            {
                val.valido = false;
                val.mensaje = "No se puede multiplicar un valor tipo " + t1 + " con  " + t2;
            }
            else if (t1.Equals("decimal") || t2.Equals("decimal"))
            {
                val.valido = true;
                val.tipo = "decimal";
            }
            else if (t1.Equals("entero") || t2.Equals("entero"))
            {
                val.valido = true;
                val.tipo = "entero";
            }
            else if (t1.Equals("char") || t2.Equals("char"))
            {
                val.valido = true;
                val.tipo = "entero";

            }
            else if (t1.Equals("booleano") || t2.Equals("booleano"))
            {
                val.valido = true;
                val.tipo = "booleano";
            }

            return val;
        }

        public Validacion validoDivision(string t1, string t2, string op)
        {
            Validacion val = new Validacion();
            val.tipo = "Error";
            if (t1.Equals("Error") || t2.Equals("Error"))
            {
                val.valido = false;
                val.mensaje = "No se puede " + op + " un valor tipo " + t1 + " con  " + t2;
            }
            else if (t1.Equals("cadena") || t2.Equals("cadena"))
            {
                val.valido = false;
                val.mensaje = "No se puede " + op + " un valor tipo " + t1 + " con  " + t2;
            }
            else if (t1.Equals("decimal") || t2.Equals("decimal"))
            {
                val.valido = true;
                val.tipo = "decimal";
            }
            else if (t1.Equals("entero") || t2.Equals("entero"))
            {
                if (t2.Equals("booleano"))
                {
                    val.valido = true;
                    val.tipo = "entero";
                }
                else
                {
                    val.valido = true;
                    val.tipo = "decimal";
                }

            }
            else if (t1.Equals("char") || t2.Equals("char"))
            {
                if (t2.Equals("booleano"))
                {
                    val.valido = true;
                    val.tipo = "entero";
                }
                else
                {
                    val.valido = true;
                    val.tipo = "decimal";
                }

            }
            else if (t1.Equals("booleano") || t2.Equals("booleano"))
            {
                val.valido = false;
                val.mensaje = "No se puede " + op + " un valor tipo " + t1 + " con  " + t2;
            }

            return val;
        }

        private string changeTipo(string tipo)
        {
            switch (tipo.ToLower())
            {
                case "int":
                    return "entero";
                case "string":
                    return "cadena";
                case "double":
                    return "decimal";
                case "char":
                    return "char";
                case "bool":
                    return "booleano";
                default:
                    return tipo;
            }
        }

        private void ejecutarAsignacion(ParseTreeNode nodo, ref Entorno temp)
        {

            String nombre = "";//nombre de la variable que se quiere cambiar
            Tipo tipo = new Tipo();// el tipo de objeto que se quiere asignar
            String tipoVar = "";// el tipo de objeto de la variable que se cambiara
            
            bool continuar = true;

            bool esOtraClase = false;
            Entorno entornoVariable = null;
            Tipo objeto = new Tipo();
            //Entorno temp = actual;
            switch (nodo.ChildNodes.Count)
            {
                case 2:
                    nombre = nodo.ChildNodes[0].Token.Text;
                    tipo = (Verificacion(nodo.ChildNodes[1].ChildNodes[0], ref temp));
                    break;
                case 3:
                    nombre = nodo.ChildNodes[0].Token.Text;
                    tipo = (Verificacion(nodo.ChildNodes[2].ChildNodes[0], ref temp));
                    break;
                case 4:
                    tipo = Verificacion(nodo.ChildNodes[0].ChildNodes[0], ref temp);
                    if (tipo.esObjeto)
                    {
                        // si es asignacion del tipo objeto.variable = algo
                        entornoVariable = tipo.entorno;//para buscar en el entorno del objeto
                        objeto = (Verificacion(nodo.ChildNodes[3].ChildNodes[0], ref entornoVariable));//el tipo que se quiere asignar
                        nombre = nodo.ChildNodes[1].Token.Text;
                    }else
                    {
                        continuar = false;
                        errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "La variable "+ objeto.nombre+ " no es un objeto" ));
                    }
                    break;
                case 5:
                    tipo = Verificacion(nodo.ChildNodes[0].ChildNodes[0], ref temp);
                    esOtraClase = true;
                    if (tipo.esObjeto)
                    {
                        entornoVariable = tipo.entorno;
                        objeto = Verificacion(nodo.ChildNodes[3].ChildNodes[0], ref entornoVariable);// 
                        nombre = nodo.ChildNodes[1].Token.Text;
                    }else
                    {
                        continuar = false;
                        errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "La variable " + objeto.nombre + " no es un objeto"));
                    }
                    break;
            }
            if (continuar)
            {
                if (esOtraClase)
                {
                    do
                    {
                        if (entornoVariable.HT.ContainsKey("var_" + nombre))
                        {
                            tipoVar = ((Tipo)temp.HT["var_" + nombre]).tipo;
                            break;
                        }
                        entornoVariable = entornoVariable.siguiente;

                    } while (entornoVariable!= null);

                    if (objeto.tipo.Equals(tipoVar, StringComparison.InvariantCultureIgnoreCase))
                    {
                        objeto.nombre = nombre;
                        entornoVariable.HT["var_" + nombre] = objeto;
                        tipo.entorno = entornoVariable;
                        temp.HT["var_" + nombre] = tipo;
                    }
                   
                    else
                    {
                        errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "No se puede asigar una variable " + tipo.tipo + " a una variable" + tipoVar));
                    }
                }
                else
                {
                    do
                    {
                        if (temp.HT.ContainsKey("var_" + nombre))
                        {
                            tipoVar = ((Tipo)temp.HT["var_" + nombre]).tipo;
                            break;
                        }
                        temp = temp.siguiente;

                    } while (temp != null);

                    if (tipo.tipo.Equals(tipoVar, StringComparison.InvariantCultureIgnoreCase))
                    {
                        tipo.nombre = nombre;
                        temp.HT["var_" + nombre] = tipo;
                    }
                    else
                    {
                        errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "No se puede asigar una variable " + tipo.tipo + " a una variable" + tipoVar));
                    }
                }
                
            }
           
        }
        
        private Tipo ejecutarLlamada(ParseTreeNode nodo, bool retorna, ref Entorno temp)
        {
            // ejecutar llama de un objeto, como objeto.llamarmetodo();
            Tipo objeto = new Tipo();
            Tipo metodo = new Tipo();
            Tipo retorno = new Tipo();
            //Entorno temp = actual;

            objeto = buscarVariable(nodo.ChildNodes[0].Token.Text, temp, false);
            if(objeto == null)
            {
                errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "La variable " + objeto.nombre + " no existe"));
                return retorno;
            }

            if (objeto.esObjeto)
            {
                //es un objeto tipo clase
                metodo = buscarMetodo(nodo.ChildNodes[1].Token.Text, objeto.entorno);     
                //buscar el metodo en la clase           
                if(metodo == null)
                {
                    errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "La variable " + objeto.nombre + " no es un objeto"));
                    return retorno;
                }
                temp = metodo.entorno;
                // es el entorno del metodo, cuyo siguiente = objeto.entorno i guess?
                if(retorna && !temp.retorna || !retorna && temp.retorna)
                {
                    errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "El metodo no retorna el dato requerido "));
                    return retorno;
                }
                switch (nodo.ChildNodes.Count)
                {
                    case 2://llamada sin parametros
                        if (metodo.parametros.Count > 0)
                        {
                            errores.Add(new Error(nodo.ChildNodes[1].Token.Location.Line, nodo.ChildNodes[1].Token.Location.Column, "Semantico", "El metodo " + metodo.nombre + " no recibe parametros"));
                        }else
                        {
                            Entorno delmetodo = metodo.entorno;
                            ejecutar(metodo.cuerpo, ref delmetodo);// ejecuta en el entorno del metodo, no en la clase del metodo
                            // aca no deberia volver a buscar, porque esta en el entorno del metodo, y cambia sus valores y la de la clase del metodo
                            metodo.entorno = delmetodo; // asi cambia con los valores que cambiaron al ejecutar
                            if (metodo.entorno.retorna)
                            {
                                retorno = metodo.entorno.Retorno;
                            }
                        }
                        break;
                    case 3://llamada con parametros
                        List<Tipo> parametros = new List<Tipo>() ;
                        bool correco = true;
                        parametros = obtenerParametros(nodo.ChildNodes[2], parametros, ref temp);
                        if(parametros.Count == metodo.parametros.Count)
                        {
                            for (int i = 0; i < parametros.Count; i++)
                            {
                                if (((Tipo)temp.HT["var_" + metodo.parametros[i]]).tipo.Equals(parametros[i].tipo))
                                {
                                    parametros[i].nombre = ((Tipo)temp.HT["var_" + metodo.parametros[i]]).nombre;
                                    temp.HT["var_" + metodo.parametros[i]] = parametros[i];
                                }else
                                {
                                    errores.Add(new Error(nodo.ChildNodes[1].Token.Location.Line, nodo.ChildNodes[1].Token.Location.Column, "Semantico", "El metodo " + metodo.nombre + " no recibe estos parametros"));
                                    correco = false;
                                    break;
                                }
                            }
                            if (correco)
                            {
                                Entorno delmetodo = metodo.entorno;
                                ejecutar(metodo.cuerpo, ref delmetodo);
                                //metodo = buscarMetodo(nodo.ChildNodes[1].Token.Text, objeto.entorno);
                                metodo.entorno = delmetodo;
                                if (metodo.entorno.retorna)
                                {
                                    retorno = metodo.entorno.Retorno;
                                }
                            }
                        }
                        break;
                }

            }else
            {
                errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "La variable " + objeto.nombre + " no es un objeto"));
            }

            return retorno;
            
        }

        public Tipo buscarMetodo(String id, Entorno entorno)
        {
            id = id.ToLower();
            if (entorno != null)
            {
                if (entorno.HT.ContainsKey("met_" + id))
                {
                    return (Tipo)entorno.HT["met_" + id];
                }
            }

            if (entorno.siguiente != null)
            {
                return buscarMetodo(id, entorno.siguiente); //busca en el entorno siguiente
            }

            if (entorno.implementos != null)
            {
                return buscarMetodo(id, entorno.implementos);//busca en sus extends

            }
            return null;
        }

        public Tipo buscarVariable(String id, Entorno entorno, bool esparametro)
        {
            id = id.ToLower();
            if (entorno != null)
            {
                if (entorno.HT.ContainsKey("var_" + id))
                {
                    return (Tipo)entorno.HT["var_" + id];
                }
            }

            if (!esparametro)
            {
                if (entorno.siguiente != null)
                {
                    return buscarVariable(id, entorno.siguiente, esparametro); //busca en el entorno siguiente
                }

                if (entorno.implementos!= null)
                {
                    return buscarVariable(id, entorno.implementos, esparametro);//busca en sus extends

                }
            }
            
            return null;
        }

        public List<Tipo> obtenerParametros(ParseTreeNode nodo, List <Tipo> lista, ref Entorno entorno)
        {
            switch (nodo.ChildNodes.Count)
            {
                case 2:
                    lista = obtenerParametros(nodo.ChildNodes[0], lista, ref entorno);
                    lista.Add(Verificacion(nodo.ChildNodes[1], ref entorno));
                    break;
                case 1:
                    lista.Add(Verificacion(nodo.ChildNodes[0], ref entorno));
                    break;
            }
            return lista;
        }

        private void print(ParseTreeNode nodo, ref Entorno entorno)
        {
            consola.Select(consola.TextLength, 0);
            consola.SelectionColor = Color.GreenYellow;
            Tipo ValorS = Verificacion(nodo.ChildNodes[0], ref entorno);
            if(ValorS != null)
            {
                if(!ValorS.tipo.Equals("Error"))
                    consola.AppendText(ValorS.valorString + "\n");
            }
            
        }

        private void show(ParseTreeNode nodo, ref Entorno entorno)
        {
            MessageBox.Show(Verificacion(nodo.ChildNodes[0], ref entorno).valorString, Verificacion(nodo.ChildNodes[1], ref entorno).valorString);
        }

        private void repetir(ParseTreeNode nodo, ref Entorno entorno)
        {
            Tipo cuenta = Verificacion(nodo.ChildNodes[0],ref entorno);
            if (cuenta.tipo.Equals("entero"))
            {
                Entorno entif = new Entorno();
                entif.siguiente = entorno;
                //actual = entif;
                for (int i = 0; i < cuenta.valorEntero; i++)
                {
                    ejecutar(nodo.ChildNodes[1], ref entif);
                }
            }else
            {
                errores.Add(new Error(cuenta.fila, cuenta.columna, "Semantico", "No se permite un tipo " + cuenta.tipo + " como argumento"));
            }
        }

        private void ejecutarIf(ParseTreeNode nodo, ref Entorno actual)
        {
            Tipo condicion = Verificacion(nodo.ChildNodes[0], ref actual);
            if (condicion.tipo.Equals("booleano"))
            {
                if (condicion.valorBoleano)
                {
                    Entorno entif = new Entorno();
                    entif.siguiente = actual;
                    //actual = entif;
                    ejecutar(nodo.ChildNodes[1], ref entif);
                } else if (nodo.ChildNodes.Count == 3)
                {
                    switch (nodo.ChildNodes[2].ToString())
                    {
                        case "SINOSI":
                            ejecutarIfElse(nodo.ChildNodes[2], ref actual);
                            break;
                        case "SINO":
                            Entorno entif = new Entorno();
                            entif.siguiente = actual;
                            //actual = entif;
                            ejecutar(nodo.ChildNodes[2].ChildNodes[0], ref entif);
                            break;
                    }
                } else if (nodo.ChildNodes.Count == 4)
                {
                    if (!ejecutarIfElse(nodo.ChildNodes[2], ref actual))
                    {
                        Entorno entif = new Entorno();
                        entif.siguiente = actual;
                        //actual = entif;
                        ejecutar(nodo.ChildNodes[3].ChildNodes[0], ref entif);
                    }
                }
                
            }else
            {
                errores.Add(new Error(condicion.fila, condicion.columna, "Semantico", "No se permite un tipo " + condicion.tipo + " como argumento"));
            }
        }

        private bool ejecutarIfElse(ParseTreeNode nodo, ref Entorno actual)
        {
            Tipo condicion = new Tipo();
            switch (nodo.ChildNodes.Count)
            {
                case 3:
                    if (!ejecutarIfElse(nodo.ChildNodes[0], ref actual))
                    {
                        condicion = Verificacion(nodo.ChildNodes[1], ref actual);
                        if (condicion.tipo.Equals("booleano"))
                        {
                            if (condicion.valorBoleano)
                            {
                                Entorno entif = new Entorno();
                                entif.siguiente = actual;
                                //actual = entif;
                                ejecutar(nodo.ChildNodes[2], ref entif);
                                return true;
                            }
                        }else
                        {
                            errores.Add(new Error(condicion.fila, condicion.columna, "Semantico", "No se permite un tipo " + condicion.tipo + " como argumento"));
                        }
                    }
                    break;
                case 2:
                    condicion = Verificacion(nodo.ChildNodes[0], ref actual);
                    if (condicion.tipo.Equals("booleano"))
                    {
                        if (condicion.valorBoleano)
                        {
                            Entorno entif = new Entorno();
                            entif.siguiente = actual;
                            //actual = entif;
                            ejecutar(nodo.ChildNodes[1], ref entif);
                            return true;
                        }
                    }else
                    {
                        errores.Add(new Error(condicion.fila, condicion.columna, "Semantico", "No se permite un tipo " + condicion.tipo + " como argumento"));
                    }
                    break;
            }
            return false;
        }

        private Tipo ValorArray(ParseTreeNode nodo, ref Entorno actual)
        {
            String varName = nodo.ChildNodes[0].Token.Text.ToLower();
            Tipo retorno = new Tipo();
            retorno.tipo = "Error";
            if (actual.HT.ContainsKey("var_" + varName))
            {
                retorno = (Tipo)actual.HT["var_" + varName];
                
                List<int> tamano = new List<int>();
                switch (nodo.ChildNodes.Count)
                {
                    case 2:
                        if (!retorno.esArray)
                        {
                            errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "La variable " + varName + " no es un arreglo"));
                            return retorno;
                        }
                        tamano = getTamano(nodo.ChildNodes[1], tamano, ref actual);
                        break;
                    case 3:
                        if (retorno.esObjeto)
                        {
                            if(retorno.entorno.HT.ContainsKey("var_"+ nodo.ChildNodes[1].Token.Text.ToLower()))
                            {
                                retorno = (Tipo)retorno.entorno.HT["var_" + nodo.ChildNodes[1].Token.Text.ToLower()];
                                if (retorno.esArray)
                                {
                                    tamano = getTamano(nodo.ChildNodes[1], tamano, ref actual);
                                }
                            }
                            
                        }
                        tamano = getTamano(nodo.ChildNodes[2], tamano, ref actual);
                        break;

                }
            }

            return retorno;
            
        }

        public void Retornar(ParseTreeNode nodo, ref Entorno actual)
        {
            if (actual.retorna)
            {
                actual.Retorno = Verificacion(nodo.ChildNodes[0], ref actual);
            }
        }

        private void mientras(ParseTreeNode nodo, ref Entorno actual)
        {
            Tipo validacion = Verificacion(nodo.ChildNodes[0], ref actual);
            if (validacion.tipo.Equals("booleano"))
            {
                Entorno entif = new Entorno();
                entif.siguiente = actual;
                entif.esCiclo = true;
                //actual = entif;
                while (Verificacion(nodo.ChildNodes[0], ref actual).valorBoleano)
                {
                    ejecutar(nodo.ChildNodes[1], ref entif);
                }
            }else
            {
                validacion = Verificacion(nodo.ChildNodes[0], ref actual);
                errores.Add(new Error(validacion.fila, validacion.columna, "Semantico", "No se permite un tipo " + validacion.tipo + " como argumento"));
            }
        }

        private void hacerMientras(ParseTreeNode nodo, ref Entorno actual)
        {
            Tipo condicion = Verificacion(nodo.ChildNodes[1], ref actual);
            if (condicion.tipo.Equals("booleano"))
            {
                Entorno entif = new Entorno();
                entif.siguiente = actual;
                entif.esCiclo = true;
                //actual = entif;
                ejecutar(nodo.ChildNodes[0], ref entif);
                while (Verificacion(nodo.ChildNodes[1], ref actual).valorBoleano)
                {
                    ejecutar(nodo.ChildNodes[0], ref entif);
                }
            }else
            {
                errores.Add(new Error(condicion.fila, condicion.columna, "Semantico", "No se permite un tipo " + condicion.tipo + " como argumento"));
            }
            
        }

        private Tipo aumentoDecremento(ParseTreeNode nodo, ref Entorno temp)
        {
            Tipo temporal = Verificacion(nodo.ChildNodes[0], ref temp);
            String signo = nodo.ChildNodes[1].Token.Text;
            if (!temporal.nombre.Equals(""))
            {   
                Tipo var;
                String name = temporal.nombre.ToLower();
                do
                {
                    if (temp.HT.ContainsKey("var_" + name))
                    {
                        var = (Tipo)temp.HT["var_" + name];
                        //temporal = var;
                        switch (var.tipo)
                        {
                            case "entero":
                                var.valorEntero = var.valorEntero + (signo.Equals("++")?1:-1);
                                var.valorDouble = var.valorEntero;
                                var.valorString = var.valorEntero.ToString();
                                temp.HT["var_" + name] = var;
                                break;
                            case "decimal":
                                var.valorDouble = var.valorDouble + (signo.Equals("++") ? 1 : -1);
                                var.valorEntero = (int)Math.Round(var.valorDouble);
                                var.valorString = var.valorDouble.ToString();
                                temp.HT["var_" + name] = var;
                                break;
                            case "char":
                                var.valorEntero = var.valorEntero + (signo.Equals("++") ? 1 : -1);
                                var.valorChar = (char)var.valorEntero;
                                var.valorDouble = var.valorEntero;
                                temp.HT["var_" + name] = var;
                                break;
                            default:
                                errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "No puede aumentar/disminuir una variable tipo  " + var.tipo));
                                break;

                        }


                        return temporal;

                    }
                    temp = temp.siguiente;

                } while (temp != null);

                errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "No existe la variable " + name));               
            }
            else
            {
                
                switch (temporal.tipo)
                {
                    case "entero":
                        temporal.valorEntero = temporal.valorEntero;
                        temporal.valorDouble = temporal.valorEntero;
                        temporal.valorString = temporal.valorEntero.ToString();
                        break;
                    case "decimal":
                        temporal.valorDouble = temporal.valorDouble;
                        temporal.valorEntero = (int)Math.Round(temporal.valorDouble);
                        temporal.valorString = temporal.valorDouble.ToString();
                        break;
                    case "char":
                        temporal.valorEntero = temporal.valorEntero;
                        temporal.valorChar = (char)temporal.valorEntero;
                        temporal.valorDouble = temporal.valorEntero;
                        break;
                    default:
                        errores.Add(new Error(temporal.fila, temporal.columna, "Semantico", "No puede aumentar/disminuir una variable tipo  " + temporal.tipo));
                        break;
                }
            }
            return temporal;

        }

        private Tipo ejecutarMetodoLocal(ParseTreeNode nodo, bool devuelve, ref Entorno ent)
        {
            Tipo temp = new Tipo();
            while(ent.siguiente!= null)
            {
                ent = ent.siguiente;//buscar la clase padre
            }
            temp = buscarMetodo(nodo.ChildNodes[0].Token.Text, ent);
            if(temp == null)
            {
                errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "No se encuentra el método "+ nodo.ChildNodes[0].Token.Text));
            }else
            {
                if (nodo.ChildNodes.Count == 2)
                {
                    if (temp.parametros.Count == 0)
                    {
                        errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "No se encuentra el método " + nodo.ChildNodes[0].Token.Text + " con el numero de parametros especificados "));
                    }else
                    {
                        if (!verificarParametros(nodo.ChildNodes[1], ref temp, ref ent))
                        {
                            errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "No se encuentra el método " + nodo.ChildNodes[0].Token.Text + " con los parametros especificos "));
                            temp.tipo = "Error";
                            return temp;
                        }
                    }
                }
                else
                {
                    if(temp.parametros.Count > 0)
                    {
                        errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "No se encuentra el método " + nodo.ChildNodes[0].Token.Text+ " sin parametros "));
                    }
                }
                //actual = temp.entorno;
                Entorno delmetodo = temp.entorno;
                ejecutar(temp.cuerpo, ref delmetodo);
                temp.entorno = delmetodo;
                
                
                if (temp.entorno.retorna && temp.entorno.Retorno!=null)
                {
                    temp = temp.entorno.Retorno;
                }
                //actual = guardaractual;
            }

            return temp;
        }

        private bool verificarParametros(ParseTreeNode nodo, ref Tipo metodo, ref Entorno ent)
        {
            List<Tipo> parametros = getListaParametros(nodo, ref ent );
            Tipo comp;
            if (metodo.parametros.Count == parametros.Count)
            {
                for (int i = 0; i < metodo.parametros.Count ; i++)
                {
                    comp = buscarVariable(metodo.parametros.ElementAt(i), metodo.entorno, true);
                    if (comp.tipo == parametros[i].tipo)
                    {
                        parametros[i].nombre = metodo.parametros[i];
                        metodo.entorno.HT["var_" + metodo.parametros[i].ToLower()] = parametros[i];
                    }else
                    {
                        errores.Add(new Error(parametros[i].fila, parametros[i].columna, "Semantico", "No puede asignar un tipo " + parametros[i].tipo + " a un tipo " + comp.tipo));
                        return false;
                    }
                }
                return true;

            }
            return false;
        }

        private List<Tipo> getListaParametros(ParseTreeNode nodo, ref Entorno ent)
        {
            List<Tipo> Lista;
            if (nodo.ChildNodes.Count==2)
            {
                Lista = getListaParametros(nodo.ChildNodes[0], ref ent);
                Lista.Add(Verificacion(nodo.ChildNodes[1], ref ent));
                return Lista;
            }else
            {
                Lista = new List<Tipo>();
                Lista.Add(Verificacion(nodo.ChildNodes[0], ref ent));
                return Lista;
            }
        }

        private void crearObjeto(ParseTreeNode nodo)
        {
            throw new NotImplementedException();
        }

        private void ejecutarFor(ParseTreeNode nodo, ref Entorno entorno)
        {
            string nombre = "";
            switch (nodo.ChildNodes[0].ChildNodes[0].ChildNodes.Count)
            {
                case 3:
                    nombre = nodo.ChildNodes[0].ChildNodes[0].ChildNodes[1].ChildNodes[0].Token.Text;
                    break;
                case 2:
                    nombre = nodo.ChildNodes[0].ChildNodes[0].ChildNodes[0].Token.Text;
                    break;
            }

            ejecutar(nodo.ChildNodes[0], ref entorno);//ejecuta declaracion/asignacion
            Tipo inicial = buscarVariable(nombre, entorno, false);//nombre de la variable de declaracion/asignacion
            if (inicial != null)
            {
                if (inicial.tipo.Equals("entero"))
                {
                    Entorno nuevo = new Entorno();
                    nuevo.siguiente = entorno;
                    nuevo.esCiclo = true;
                    nuevo.HT.Add("var_" + nombre.ToLower(), inicial);
                    inicial = Verificacion(nodo.ChildNodes[1], ref entorno);//verifica el valor booleano del segundo argumento
                    if (inicial.tipo.Equals("booleano"))
                    {
                        //actual = nuevo;
                        ParseTreeNode temp = new ParseTreeNode(null, new SourceSpan());
                        temp.ChildNodes.Add(nodo.ChildNodes[2]);
                        while (inicial.valorBoleano)
                        {
                            ejecutar(nodo.ChildNodes[3], ref entorno);
                            ejecutar(temp, ref entorno);
                            inicial = Verificacion(nodo.ChildNodes[1], ref nuevo);
                        }
                        //actual = nuevo.siguiente;
                    }else
                    {
                        errores.Add(new Error(inicial.fila, inicial.columna, "Semantico", "No puede utilizar una operacion de un tipo " + inicial.tipo));
                    }
                }else
                {
                    errores.Add(new Error(inicial.fila, inicial.columna, "Semantico", "No puede utilizar una variable un tipo " + inicial.tipo ));
                }
            }else
            {
                errores.Add(new Error(0, 0, "Semantico", "no se encontro la variable " + nombre));
            }

        }

        private Tipo nuevoObjeto(ParseTreeNode nodo)
        {
            Tipo retorno = new Tipo();
            String nombre = nodo.ChildNodes[0].Token.Text.ToLower();
            if (listaEntornos.ContainsKey("ent_" + nombre))
            {
                retorno.esObjeto = true;
                retorno.entorno = (Entorno)listaEntornos["ent_" + nombre];
                retorno.tipo = nombre;
            }else
            {
                errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "No se encuentra la clase con nombre " + nombre));
            }

            return retorno;
        }

        public void imprimirTodo()
        {
            Tipo actual = null;
            foreach (Entorno en in listaEntornos.Values)
            {
                Console.WriteLine("El entorno " + en.nombre + " Tiene " + en.HT.Count + " Elementos en su nombre");
                foreach (String k in en.HT.Keys)
                {
                    if (k.Substring(0, 4) == "var_")
                    {
                        actual = (Tipo)en.HT[k];
                        Console.WriteLine("key: " + k + " type: " + actual.tipo);
                    }
                    else
                    {
                        actual = (Tipo)en.HT[k];
                        Console.WriteLine("key: " + k + " type: " + actual.tipo + ".  metodo");
                    }
                }
            }
        }

        //comando ref voyage
    }
    
}
