using Irony.Parsing;
using Proyecto2.Errores;
using Proyecto2.Graficos;
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
        public RichTextBox consola, variables;
        Hashtable listaEntornos = new Hashtable();
        public Entorno actual;
        Validacion val;
        bool ejecutando;
        public List<Figura> figuras = new List<Figura>();

        public Accionar(RichTextBox console, RichTextBox variables)
        {
            this.consola = console;
            this.variables = variables;
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
                        if(hijo.ChildNodes[0].Token != null)
                        {
                            if (listaEntornos.ContainsKey("ent_" + hijo.ChildNodes[0].Token.Text.ToLower()))
                            {
                                errores.Add(new Error(hijo.ChildNodes[0].Token.Location.Line, hijo.ChildNodes[0].Token.Location.Column, "Semantico", "Ya existe una Clase llamada " + hijo.ChildNodes[0].Token.Text));
                            }
                            else
                            {
                                actual = new Entorno(hijo.ChildNodes[0].Token.Text.ToLower());
                                listaEntornos.Add("ent_" + hijo.ChildNodes[0].Token.Text.ToLower(), actual);
                                guardar(hijo);
                            }
                        }else
                        {
                            if (listaEntornos.ContainsKey("ent_" + hijo.ChildNodes[1].Token.Text.ToLower()))
                            {
                                errores.Add(new Error(hijo.ChildNodes[1].Token.Location.Line, hijo.ChildNodes[1].Token.Location.Column, "Semantico", "Ya existe una Clase llamada " + hijo.ChildNodes[1].Token.Text));
                            }
                            else
                            {
                                actual = new Entorno(hijo.ChildNodes[1].Token.Text.ToLower());
                                actual.visibilidad = hijo.ChildNodes[0].ChildNodes[0].Token.Text.ToLower();
                                listaEntornos.Add("ent_" + hijo.ChildNodes[1].Token.Text.ToLower(), actual);
                                guardar(hijo);
                            }
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
                                timain.entorno.metodoPrincipal = timain.entorno;
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
            metodo.entorno.metodoPrincipal = metodo.entorno;
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
            if(actual.metodoPrincipal!= null)
            {
                if(actual.metodoPrincipal.Retorno != null) { return; }
            }
            if( !actual.salir)
            {

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
                            comprobar(hijo, ref actual);
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
                        case "SALIR":
                            if (actual.esCiclo)
                            {
                                actual.salir = true;
                            }else
                            {
                                errores.Add(new Error(hijo.ChildNodes[0].ChildNodes[0].Token.Location.Line, hijo.ChildNodes[0].ChildNodes[0].Token.Location.Column, "Semantico", "Sentencia Salir en ambito incorrecto "));
                            }
                            
                            break;
                        case "ADDFIGURE":
                            ejecutar(hijo, ref actual);
                            break;
                        case "LFIGURA":
                            ejecutar(hijo, ref actual);
                            break;
                        case "CIRCLE":
                            anadirCirculo(hijo, ref actual);
                            break;
                        case "TRIANGLE":
                            anadirTriangulo(hijo, ref actual);
                            break;
                        case "SQUARE":
                            anadirCuadrado(hijo, ref actual);
                            break;
                        case "LINE":
                            anadirLinea(hijo, ref actual);
                            break;
                        case "FIGURE":
                            pintarFiguras(hijo, ref actual);
                            break;

                    }
                }
            }
            
            
        }

        private void pintarFiguras(ParseTreeNode hijo, ref Entorno actual)
        {
            Tipo nombre = Verificacion(hijo.ChildNodes[0], ref actual);
            if(nombre.tipo.Equals("cadena", StringComparison.InvariantCultureIgnoreCase))
            {
                if (errores.Count == 0)
                {
                    Dibujar dibujar = new Dibujar();
                    dibujar.dibujo(figuras, nombre.valorString);
                    figuras.Clear();
                }
                
            }else
            {
                errores.Add(new Error(nombre.fila, nombre.columna, "Semantico", "El nombre del grafico debe ser tipo cadena "));
            }

        }

        private void anadirLinea(ParseTreeNode hijo, ref Entorno actual)
        {
            Tipo color = Verificacion(hijo.ChildNodes[0], ref actual);
            Tipo posx = Verificacion(hijo.ChildNodes[1], ref actual);
            Tipo posy = Verificacion(hijo.ChildNodes[2], ref actual);
            Tipo posfx = Verificacion(hijo.ChildNodes[3], ref actual);
            Tipo posfy = Verificacion(hijo.ChildNodes[4], ref actual);
            Tipo thickness = Verificacion(hijo.ChildNodes[5], ref actual);
            Figura linea = new Figura();
            linea.tipo = "linea";

            if (color.tipo.Equals("cadena", StringComparison.InvariantCultureIgnoreCase))
            {
                linea.color = color.valorString;
            }
            else
            {
                errores.Add(new Error(color.fila, color.columna, "Semantico", "El color de una linea debe ser de tipo cadena "));
                return;
            }

            if (thickness.tipo.Equals("entero", StringComparison.InvariantCultureIgnoreCase) || thickness.tipo.Equals("decimal", StringComparison.InvariantCultureIgnoreCase))
            {
                linea.grosor = thickness.valorEntero;
            }
            else
            {
                errores.Add(new Error(thickness.fila, thickness.columna, "Semantico", "el dato de grosor de una linea debe ser entero"));
                return;
            }

            if (posx.tipo.Equals("entero", StringComparison.InvariantCultureIgnoreCase) || posx.tipo.Equals("decimal", StringComparison.InvariantCultureIgnoreCase))
            {
                linea.posx = posx.valorEntero;
            }
            else
            {
                errores.Add(new Error(posx.fila, posx.columna, "Semantico", "La posicion X de una linea debe ser un entero"));
                return;
            }

            if (posy.tipo.Equals("entero", StringComparison.InvariantCultureIgnoreCase)|| posy.tipo.Equals("decimal", StringComparison.InvariantCultureIgnoreCase))
            {
                linea.posy = posy.valorEntero;
            }
            else
            {
                errores.Add(new Error(posy.fila, posy.columna, "Semantico", "La posicion y de una linea debe ser un entero"));
                return;
            }

            if (posfx.tipo.Equals("entero", StringComparison.InvariantCultureIgnoreCase) || posfx.tipo.Equals("decimal", StringComparison.InvariantCultureIgnoreCase))
            {
                linea.posxf = posfx.valorEntero;
            }
            else
            {
                errores.Add(new Error(posfx.fila, posfx.columna, "Semantico", "La posicion final X de una linea debe ser un entero"));
                return;
            }

            if (posfy.tipo.Equals("entero", StringComparison.InvariantCultureIgnoreCase) || posfy.tipo.Equals("decimal", StringComparison.InvariantCultureIgnoreCase))
            {
                linea.posyf = posfy.valorEntero;
            }
            else
            {
                errores.Add(new Error(posfy.fila, posfy.columna, "Semantico", "La posicion final y de una linea debe ser un entero"));
                return;
            }
            figuras.Add(linea);
        }

        private void anadirCuadrado(ParseTreeNode hijo, ref Entorno actual)
        {
            Tipo color = Verificacion(hijo.ChildNodes[0], ref actual);
            Tipo solido = Verificacion(hijo.ChildNodes[1], ref actual);
            Tipo posx = Verificacion(hijo.ChildNodes[2], ref actual);
            Tipo posy = Verificacion(hijo.ChildNodes[3], ref actual);
            Tipo weight = Verificacion(hijo.ChildNodes[4], ref actual);
            Tipo height = Verificacion(hijo.ChildNodes[5], ref actual);
            Figura cuadrado = new Figura();
            cuadrado.tipo = "cuadrado";
            if (color.tipo.Equals("cadena", StringComparison.InvariantCultureIgnoreCase))
            {
                cuadrado.color = color.valorString;
            }
            else
            {
                errores.Add(new Error(color.fila, color.columna, "Semantico", "El color de un cuadrado debe ser de tipo cadena "));
                return;
            }

            if (solido.tipo.Equals("booleano", StringComparison.InvariantCultureIgnoreCase))
            {
                cuadrado.solida = solido.valorBoleano;
            }
            else
            {
                errores.Add(new Error(solido.fila, solido.columna, "Semantico", "el dato solido de un cuadrado debe ser booleano "));
                return;
            }

            if (posx.tipo.Equals("entero", StringComparison.InvariantCultureIgnoreCase) || posx.tipo.Equals("decimal", StringComparison.InvariantCultureIgnoreCase))
            {
                cuadrado.posx = posx.valorEntero;
            }
            else
            {
                errores.Add(new Error(posx.fila, posx.columna, "Semantico", "La posicion X de un cuadrado debe ser un entero"));
                return;
            }

            if (posy.tipo.Equals("entero", StringComparison.InvariantCultureIgnoreCase) || posy.tipo.Equals("decimal", StringComparison.InvariantCultureIgnoreCase))
            {
                cuadrado.posy = posy.valorEntero;
            }
            else
            {
                errores.Add(new Error(posy.fila, posy.columna, "Semantico", "La posicion y de un cuadrado debe ser un entero"));
                return;
            }

            if (weight.tipo.Equals("entero", StringComparison.InvariantCultureIgnoreCase))
            {
                cuadrado.width = weight.valorEntero;
            }
            else
            {
                errores.Add(new Error(weight.fila, weight.columna, "Semantico", "El ancho de un cuadrado debe ser un entero"));
                return;
            }

            if (height.tipo.Equals("entero", StringComparison.InvariantCultureIgnoreCase) || height.tipo.Equals("decimal", StringComparison.InvariantCultureIgnoreCase))
            {
                cuadrado.height = height.valorEntero;
            }
            else
            {
                errores.Add(new Error(height.fila, height.columna, "Semantico", "El largo de un cuadrado debe ser un entero"));
                return;
            }
            figuras.Add(cuadrado);

        }

        private void anadirTriangulo(ParseTreeNode hijo, ref Entorno actual)
        {
            Tipo color = Verificacion(hijo.ChildNodes[0], ref actual);
            Tipo solido = Verificacion(hijo.ChildNodes[1], ref actual);
            Tipo posx = Verificacion(hijo.ChildNodes[2], ref actual);
            Tipo posy = Verificacion(hijo.ChildNodes[3], ref actual);
            Tipo pos2x = Verificacion(hijo.ChildNodes[4], ref actual);
            Tipo pos2y = Verificacion(hijo.ChildNodes[5], ref actual);
            Tipo pos3x = Verificacion(hijo.ChildNodes[6], ref actual);
            Tipo pos3y = Verificacion(hijo.ChildNodes[7], ref actual);
            Figura triangulo = new Figura();
            triangulo.tipo = "triangulo";

            if (color.tipo.Equals("cadena", StringComparison.InvariantCultureIgnoreCase))
            {
                triangulo.color = color.valorString;
            }
            else
            {
                errores.Add(new Error(color.fila, color.columna, "Semantico", "El color de un triangulo debe ser de tipo cadena "));
                return;
            }

            if (solido.tipo.Equals("booleano", StringComparison.InvariantCultureIgnoreCase))
            {
                triangulo.solida = solido.valorBoleano;
            }
            else
            {
                errores.Add(new Error(solido.fila, solido.columna, "Semantico", "el dato solido de un triangulo debe ser booleano "));
                return;
            }

            if (posx.tipo.Equals("entero", StringComparison.InvariantCultureIgnoreCase) || posx.tipo.Equals("decimal", StringComparison.InvariantCultureIgnoreCase))
            {
                triangulo.vx1 = posx.valorEntero;
            }
            else
            {
                errores.Add(new Error(posx.fila, posx.columna, "Semantico", "La posicion X de un triangulo debe ser un entero"));
                return;
            }

            if (posy.tipo.Equals("entero", StringComparison.InvariantCultureIgnoreCase) ||posy.tipo.Equals("decimal", StringComparison.InvariantCultureIgnoreCase))
            {
                triangulo.vy1 = posy.valorEntero;
            }
            else
            {
                errores.Add(new Error(posy.fila, posy.columna, "Semantico", "La posicion y de un triangulo debe ser un entero"));
                return;
            }

            if (pos2x.tipo.Equals("entero", StringComparison.InvariantCultureIgnoreCase) || pos2x.tipo.Equals("decimal", StringComparison.InvariantCultureIgnoreCase))
            {
                triangulo.vx2 = pos2x.valorEntero;
            }
            else
            {
                errores.Add(new Error(pos2x.fila, pos2x.columna, "Semantico", "La posicion X de un triangulo debe ser un entero"));
                return;
            }

            if (pos2y.tipo.Equals("entero", StringComparison.InvariantCultureIgnoreCase) || pos2y.tipo.Equals("decimal", StringComparison.InvariantCultureIgnoreCase))
            {
                triangulo.vy2 = pos2y.valorEntero;
            }
            else
            {
                errores.Add(new Error(pos2y.fila, pos2y.columna, "Semantico", "La posicion y de un triangulo debe ser un entero"));
                return;
            }

            if (pos3x.tipo.Equals("entero", StringComparison.InvariantCultureIgnoreCase) || pos3x.tipo.Equals("decimal", StringComparison.InvariantCultureIgnoreCase))
            {
                triangulo.vx3 = pos3x.valorEntero;
            }
            else
            {
                errores.Add(new Error(pos3x.fila, pos3x.columna, "Semantico", "La posicion X de un triangulo debe ser un entero"));
                return;
            }

            if (pos3y.tipo.Equals("entero", StringComparison.InvariantCultureIgnoreCase) || pos3y.tipo.Equals("decimal", StringComparison.InvariantCultureIgnoreCase))
            {
                triangulo.vy3 = pos3y.valorEntero;
            }
            else
            {
                errores.Add(new Error(pos3y.fila, pos3y.columna, "Semantico", "La posicion y de un triangulo debe ser un entero"));
                return;
            }
            figuras.Add(triangulo);


        }

        private void anadirCirculo(ParseTreeNode hijo, ref Entorno actual)
        {
            Tipo color = Verificacion(hijo.ChildNodes[0], ref actual);
            Tipo radio = Verificacion(hijo.ChildNodes[1], ref actual);
            Tipo solido = Verificacion(hijo.ChildNodes[2], ref actual);
            Tipo posx = Verificacion(hijo.ChildNodes[3], ref actual);
            Tipo posy = Verificacion(hijo.ChildNodes[4], ref actual);
            Figura circulo = new Figura();
            circulo.tipo = "circulo";

            if (color.tipo.Equals("cadena", StringComparison.InvariantCultureIgnoreCase))
            {
                circulo.color = color.valorString;
            }
            else
            {
                errores.Add(new Error(color.fila, color.columna, "Semantico", "El color de un circulo debe ser de tipo cadena "));
                return;
            }

            if (radio.tipo.Equals("entero", StringComparison.InvariantCultureIgnoreCase) || radio.tipo.Equals("decimal", StringComparison.InvariantCultureIgnoreCase))
            {
                circulo.radio = radio.valorEntero;
            }
            else
            {
                errores.Add(new Error(radio.fila, radio.columna, "Semantico", "El radio de un circulo debe ser tipo entero "));
                return;
            }

            if (solido.tipo.Equals("booleano", StringComparison.InvariantCultureIgnoreCase))
            {
                circulo.solida = solido.valorBoleano;
            }
            else
            {
                errores.Add(new Error(solido.fila, solido.columna, "Semantico", "el dato solido de un circulo debe ser booleano "));
                return;
            }

            if (posx.tipo.Equals("entero", StringComparison.InvariantCultureIgnoreCase)|| posx.tipo.Equals("decimal", StringComparison.InvariantCultureIgnoreCase))
            {
                circulo.posx = posx.valorEntero;
            }
            else
            {
                errores.Add(new Error(posx.fila, posx.columna, "Semantico", "La posicion X de un circulo debe ser un entero"));
                return;
            }

            if (posy.tipo.Equals("entero", StringComparison.InvariantCultureIgnoreCase) || posy.tipo.Equals("decimal", StringComparison.InvariantCultureIgnoreCase))
            {
                circulo.posy = posy.valorEntero;
            }
            else
            {
                errores.Add(new Error(posy.fila, posy.columna, "Semantico", "La posicion y de un circulo debe ser un entero"));
                return;
            }
            figuras.Add(circulo);
        }

        private void comprobar(ParseTreeNode hijo, ref Entorno actual)
        {
            Tipo switchon = Verificacion(hijo.ChildNodes[0], ref actual);
            if (switchon.esArray)
            {
                errores.Add(new Error(hijo.ChildNodes[0].Token.Location.Line, hijo.ChildNodes[0].Token.Location.Column, "Semantico", "No puede compararse un arreglo en un COMPROBAR " ));
                return;
            }
            ejecutarcaso(hijo.ChildNodes[1], switchon, ref actual);
           
        }

        private bool ejecutarcaso(ParseTreeNode nodo, Tipo switchon, ref Entorno ent)
        {
            switch (nodo.ChildNodes.Count)
            {
                case 1:
                    return continuarCaso(nodo.ChildNodes[0], switchon, ref actual);
                case 2:
                    if(nodo.ChildNodes[1].ChildNodes.Count == 1)
                    {
                        ent.defecto = nodo.ChildNodes[1].ChildNodes[0];
                        return ejecutarcaso(nodo.ChildNodes[0], switchon, ref actual);

                        //return true;
                    }
                    bool seguir = ejecutarcaso(nodo.ChildNodes[0], switchon, ref actual);
                    if (seguir)
                    {
                        seguir = continuarCaso(nodo.ChildNodes[1], switchon, ref actual);
                        return seguir;
                    }
                    break;
            }
            ejecutar(ent.defecto, ref ent);
            return false;
        }

        private bool continuarCaso(ParseTreeNode nodo, Tipo swichon, ref Entorno ent)
        {
            if (ent.salir)
            {
                return false;
            }
            Tipo caso = Verificacion(nodo.ChildNodes[0], ref ent);
            if(caso.tipo != swichon.tipo)
            {
                errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "No puede compararse una variable tipo  "+ swichon.tipo + " con una variable " + caso.tipo));
                return false;
            }
            if (caso.esArray)
            {
                errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "Un arreglo no p uede tratarse como caso"));
                return false;
            }

            if(caso.valorString.Equals(swichon.valorString, StringComparison.InvariantCultureIgnoreCase))
            {
                Entorno comp = new Entorno("switchcase", ent);
                ejecutar(nodo.ChildNodes[1], ref comp);
                return !comp.salir;
            }

            return true;


        }

        public Entorno agregarEntorno(ParseTreeNode hijo, ref Entorno ent)
        {
            String nombre="";
            switch (hijo.ChildNodes.Count)
            {
                case 1:
                    nombre =  hijo.ChildNodes[0].Token.Text.ToLower();
                    if (listaEntornos.ContainsKey("ent_" + nombre ))
                    {
                        
                        return (Entorno)listaEntornos["ent_" + nombre];
                    }else
                    {
                        errores.Add(new Error(hijo.ChildNodes[0].Token.Location.Line, hijo.ChildNodes[0].Token.Location.Column, "Semantico", "La clase " + hijo.ChildNodes[0].Token.Text + " No existe"));
                    }
                    break;
                case 2:
                    nombre = hijo.ChildNodes[1].Token.Text.ToLower();
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
                          , Verificacion(Padre.ChildNodes[4].ChildNodes[0], ref ent)
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
                         , Verificacion(Padre.ChildNodes[5].ChildNodes[0], ref ent)
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

                Validacion validar = compatibles(tipo, value.tipo);
                //if (tipo.Equals(value.tipo,StringComparison.InvariantCultureIgnoreCase))
                if(validar.valido)
                {
                    String nombre = ids.Token.Text;
                    if (!actual.HT.Contains("var_" + nombre.ToLower()))
                    {
                        if (arreglo != value.esArray)
                        {
                            errores.Add(new Error(
                                ids.Token.Location.Line
                                , ids.Token.Location.Column
                                , "Semantico"
                                , "No se puede asignar a " + (arreglo?"un arreglo tipo ":"una variable tipo ") + tipo + (value.esArray?" un arreglo tipo ": " una variable tipo " + value.tipo )));
                        }
                        Tipo var = value;
                        var.nombre = nombre;
                        var.visibilidad = visibilidad;
                        var.tipo = validar.tipo;
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
                    errores.Add(new Error(ids.Token.Location.Line, ids.Token.Location.Column, "Semantico", "1. No se puede declarar a un tipo "+tipo+" un valor tipo "+value.tipo));
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
                        errores.Add(new Error(numer.fila, numer.columna, "Semantico", "2. No se puede declarar a un tipo " + numer.tipo+ " como tamaño de un arreglo"));
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
                            errores.Add(new Error(numer.fila, numer.columna, "Semantico", "3. No se puede declarar a un tipo " + numer.tipo + " como tamaño de un arreglo"));
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
                                            if (ret.visibilidad.Equals("privado", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                errores.Add(
                                                new Error(nodo.ChildNodes[1].Token.Location.Line
                                                , nodo.ChildNodes[1].Token.Location.Column
                                                , "Semántico"
                                                , "La variable " + nodo.ChildNodes[1].Token.Text + " es privada "));
                                            }
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
                                        return ejecutarMetodoLocal(nodo.ChildNodes[0], true, ref actual); //no es necesario devolver un valor aca :3
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
                                    Tipo retor = crearObjeto(nodo.ChildNodes[0], ref actual);
                                    Console.WriteLine(retor.tipo);
                                    return retor;
                                case "VALORARRAY":
                                    //BUSCAR UN ARRAY Y BUSCAR LA POSCION ARREGLO[POSICION]
                                    if (ejecutando)
                                    {
                                        return valorArreglo(nodo.ChildNodes[0], ref actual);
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
                                                errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "1. No existe la variable " + name));
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
                                        case "true":
                                            nuevo.tipo = "booleano";
                                            nuevo.valorBoleano = true;
                                            nuevo.valorEntero = 1;
                                            nuevo.valorDouble = 1;
                                            nuevo.valorString = "verdadero";
                                            break;
                                        case "false":
                                            nuevo.tipo = "booleano";
                                            nuevo.valorBoleano = false;
                                            nuevo.valorEntero = 0;
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

        private Tipo valorArreglo(ParseTreeNode nodo, ref Entorno actual)
        {
            Tipo arreglo = buscarVariable(nodo.ChildNodes[0].Token.Text,  actual, false);
            

            if (arreglo == null)
            {
                errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "No se encontro la variable " + nodo.ChildNodes[0].Token.Text));
                return new Tipo();
            }
            int fila = arreglo.fila, columna = arreglo.columna;
            if (arreglo.esArray)
            {
                List<int> posicion = getTamano(nodo.ChildNodes[1], new List<int>() , ref actual);
                if(arreglo.arreglo.Count > 0)
                {
                    for (int i = 0; i < posicion.Count; i++)
                    {
                        if(arreglo.arreglo.Count > posicion[i])
                        {
                            arreglo = arreglo.arreglo[posicion[i]];
                        }else
                        {
                            errores.Add(new Error(fila, columna, "Semantico", "Indice sobrepasa los limites "));
                            break;
                        }
                    }
                }
            }else
            {
                errores.Add(new Error(fila, columna, "Semantico", "La variable " + arreglo.nombre + " no es un arreglo"));
            }

            return arreglo;
        }

        private Tipo comprobacionTipos(Tipo v1, string op, Tipo v2)
        {
            Tipo retorno = new Tipo() ;
            retorno.tipo = "Error";
            if(v2 != null)
            {
                if(v2.esArray)
                    errores.Add(new Error(v2.fila, v1.columna, "Semantico", "No se pueden operar Arreglos"));
            }
            if(v1.esArray )
            {
                errores.Add(new Error(v1.fila, v1.columna, "Semantico", "No se pueden operar Arreglos"));
            }
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
                    if(v2 == null)
                    {
                        retorno.tipo = v1.tipo;
                        switch (v1.tipo)
                        {
                            
                            case "decimal":
                                retorno.valorDouble = v1.valorDouble * -1;
                                retorno.valorEntero = Convert.ToInt32(retorno.valorDouble);
                                retorno.valorString = retorno.valorDouble.ToString();
                                break;
                            case "entero":
                                retorno.valorEntero = v1.valorEntero * -1;
                                retorno.valorDouble = retorno.valorEntero;
                                retorno.valorString = retorno.valorEntero.ToString();
                                break;
                            default:
                                retorno.tipo = "Error";
                                errores.Add(new Error(v1.fila, v1.columna, "Semantico", "No puede operar un negativo de un tipo "+v1.tipo));
                                break;

                        }
                    }else
                    {
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
                        retorno.valorBoleano = v1.valorDouble <= v2.valorDouble;
                    }
                    break;
                case "==":
                        retorno.tipo = "booleano";
                        retorno.valorBoleano = v1.valorString == v2.valorString;
                    break;
                case "!=":
                    retorno.tipo = "booleano";
                    retorno.valorBoleano = v1.valorString != v2.valorString;
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
                    if ((nodo.ChildNodes[1].Token != null))
                    {

                        tipo = buscarVariableRef(nodo.ChildNodes[0].Token.Text, ref temp, false); //(Verificacion(nodo.ChildNodes[0], ref temp));//igual a algo
                        if (tipo.esObjeto)
                        {
                            // si es asignacion del tipo objeto.variable = algo
                            entornoVariable = tipo.entorno;//para buscar en el entorno del objeto
                            objeto = (Verificacion(nodo.ChildNodes[2].ChildNodes[0], ref entornoVariable));//el tipo que se quiere asignar
                            nombre = nodo.ChildNodes[1].Token.Text;
                            esOtraClase = true;
                        }
                        else
                        {
                            continuar = false;
                            errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "La variable " + objeto.nombre + " no es un objeto"));
                        }
                    }else
                    {
                        tipo = (Verificacion(nodo.ChildNodes[2].ChildNodes[0], ref temp));//igual a algo
                    }
                   
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
                    Tipo busqueda = buscarVariableRef(nombre, ref entornoVariable, false);
                    if (busqueda == null)
                    {
                        errores.Add(new Error(tipo.fila, tipo.columna, "Semantico", ".La variable " + nombre + " no existe"));
                        return;
                    }
                    tipoVar = busqueda.tipo;
                    //do
                    //{
                    //    if (entornoVariable.HT.ContainsKey("var_" + nombre))
                    //    {
                    //        tipoVar = ((Tipo)temp.HT["var_" + nombre]).tipo;
                    //        break;
                    //    }
                    //    entornoVariable = entornoVariable.siguiente;

                    //} while (entornoVariable!= null);

                    //if (objeto.tipo.Equals(tipoVar, StringComparison.InvariantCultureIgnoreCase))
                    Validacion valido = compatibles(tipoVar, objeto.tipo);
                    if (valido.valido)
                    {
                        objeto.nombre = nombre;
                        entornoVariable.HT["var_" + nombre] = objeto;
                        //tipo.entorno = entornoVariable;
                        //temp.HT["var_" + nombre] = valido.tipo;
                    }
                   
                    else
                    {
                        errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "No se puede asigar una variable " + tipo.tipo + " a una variable" + tipoVar));
                    }
                }
                else
                {

                    Tipo busqueda = buscarVariableRef(nombre, ref temp, false);
                    if (busqueda == null)
                    {
                        errores.Add(new Error(tipo.fila, tipo.columna, "Semantico", ".La variable " + nombre + " no existe"));
                        return;
                    }
                    tipoVar = busqueda.tipo;
                    //int veces = 0;
                    //if (temp.HT.ContainsKey("var_" + nombre))
                    //{
                    //    tipoVar = ((Tipo)temp.HT["var_" + nombre]).tipo;
                    //}else
                    //{
                    //    while (temp.siguiente != null)
                    //    {
                    //        if (temp.HT.ContainsKey("var_" + nombre))
                    //        {
                    //            tipoVar = ((Tipo)temp.HT["var_" + nombre]).tipo;
                    //            break;
                    //        }
                    //        if (temp.siguiente != null)
                    //        {
                    //            temp.siguiente.anterior = temp;
                    //        }

                    //        temp = temp.siguiente;
                    //        veces++;
                    //    }
                    //}

                    Validacion valido = compatibles(tipo.tipo, tipoVar);
                    //if (tipo.tipo.Equals(tipoVar, StringComparison.InvariantCultureIgnoreCase))
                    if(valido.valido)
                    {
                        tipo.nombre = nombre;
                        temp.HT["var_" + nombre] = tipo ;

                        //for (int i = 0; i < veces; i++)
                        //{
                        //    temp = temp.anterior;
                        //    temp.siguiente.anterior = null;
                        //}
                        
                    }
                    else
                    {
                        errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "No se puede asigar una variable " + tipo.tipo + " a una variable" + tipoVar));
                    }
                }
                
            }
           
        }

        public Tipo buscarVariableRef(String id, ref Entorno entorno, bool esparametro)
        {
            id = id.ToLower();
            if (entorno != null)
            {
                if (entorno.HT.ContainsKey("var_" + id))
                {
                    return (Tipo)entorno.HT["var_" + id];
                }
            }
            else
            {
                errores.Add(new Error(-100, -100, "Semantico", "No se ha enviado entorno"));
                return null;
            }

            if (!esparametro)
            {
                if (entorno.siguiente != null)
                {
                    return buscarVariable(id, entorno.siguiente, esparametro); //busca en el entorno siguiente
                }

                if (entorno.implementos != null)
                {
                    return buscarVariable(id, entorno.implementos, esparametro);//busca en sus extends

                }
            }

            return null;
        }

        private Tipo ejecutarLlamada(ParseTreeNode nodo, bool retorna, ref Entorno entt)
        {
            // ejecutar llama de un objeto, como objeto.llamarmetodo();
            Tipo objeto = new Tipo();
            Tipo metodo = new Tipo();
            Tipo retorno = new Tipo();
            //Entorno temp = actual;

            objeto = buscarVariable(nodo.ChildNodes[0].Token.Text, entt, false);
            if(objeto == null)
            {
                errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "La variable " + nodo.ChildNodes[0].Token.Text + " no existe"));
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
                if(metodo.visibilidad.Equals("privado", StringComparison.InvariantCultureIgnoreCase))
                {
                    errores.Add(new Error(nodo.ChildNodes[1].Token.Location.Line, nodo.ChildNodes[1].Token.Location.Column, "Semantico", "El metodo "+metodo.nombre + " de " + objeto.nombre + " es privado"));
                    return retorno;
                }
                metodo = new Tipo(metodo);
                metodo.limpiarTabla();
                Entorno temp = metodo.entorno;
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
                            delmetodo.metodoPrincipal.Retorno = null;
                            if (metodo.visibilidad.Equals("privado", StringComparison.InvariantCultureIgnoreCase))
                            {
                                errores.Add(new Error(nodo.ChildNodes[1].Token.Location.Line, nodo.ChildNodes[1].Token.Location.Column, "Semantico", "El metodo " + metodo.nombre + " no recibe parametros"));
                            }
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
                        Validacion valido = new Validacion();
                        if (verificarParametros(nodo.ChildNodes[2],ref  metodo,ref  entt))
                        {
                            //for (int i = 0; i < parametros.Count; i++)
                            //{
                            //    valido = compatibles(((Tipo)temp.HT["var_" + metodo.parametros[i]]).tipo, parametros[i].tipo);
                            //    //if (((Tipo)temp.HT["var_" + metodo.parametros[i]]).tipo.Equals(parametros[i].tipo))
                            //    if(valido.valido)
                            //    {
                            //        parametros[i].nombre = ((Tipo)temp.HT["var_" + metodo.parametros[i]]).nombre;
                            //        temp.HT["var_" + metodo.parametros[i]] = parametros[i];
                            //    }else
                            //    {
                            //        errores.Add(new Error(nodo.ChildNodes[1].Token.Location.Line, nodo.ChildNodes[1].Token.Location.Column, "Semantico", "El metodo " + metodo.nombre + " no recibe estos parametros"));
                            //        correco = false;
                            //        break;
                            //    }
                            //}
                            if (correco)
                            {
                                Entorno delmetodo = metodo.entorno;
                                delmetodo.metodoPrincipal.Retorno = null;
                                ejecutar(metodo.cuerpo, ref delmetodo);
                                metodo.entorno = delmetodo;
                                if (metodo.entorno.retorna)
                                {
                                    retorno = metodo.entorno.metodoPrincipal.Retorno;
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
            }else
            {
                errores.Add(new Error(-100, -100, "Semantico", "No se ha enviado entorno"));
                return null;
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
                entif.metodoPrincipal = entorno.metodoPrincipal;
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
                    entif.metodoPrincipal = actual.metodoPrincipal;
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
                            entif.metodoPrincipal = actual.metodoPrincipal;
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
                        entif.metodoPrincipal = actual.metodoPrincipal;
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
                                entif.metodoPrincipal = actual.metodoPrincipal;
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
                            entif.metodoPrincipal = actual.metodoPrincipal;
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
            //int entro = 0;
            Tipo retorno = Verificacion(nodo.ChildNodes[0], ref actual); 
            
            if (actual.metodoPrincipal.retorna)
            {
                actual.metodoPrincipal.Retorno = retorno;
            }else
            {
                errores.Add(new Error(retorno.fila, retorno.columna, "Semantico", "El metodo " + actual.metodoPrincipal.nombre + " no debe retornar"));
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
                entif.metodoPrincipal = actual.metodoPrincipal;
                //actual = entif;
                while (Verificacion(nodo.ChildNodes[0], ref actual).valorBoleano && !actual.salir)
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
                entif.metodoPrincipal = actual.metodoPrincipal;
                //actual = entif;
                ejecutar(nodo.ChildNodes[0], ref entif);
                while (Verificacion(nodo.ChildNodes[1], ref actual).valorBoleano && !actual.salir)
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

                errores.Add(new Error(temporal.fila, temporal.columna, "Semantico", "2. No existe la variable " + temporal.nombre));               
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
            Entorno aux = ent;
            while(aux.siguiente!= null)
            {
                aux = aux.siguiente;//buscar la clase padre
            }
            temp = buscarMetodo(nodo.ChildNodes[0].Token.Text, aux);// busca el metodo
            if(temp == null)
            {
                errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "No se encuentra el método "+ nodo.ChildNodes[0].Token.Text));
            }else
            {
                Tipo nmetodo = new Tipo(temp);//aca deberia crear otro entorno solo con los parametros
                nmetodo.limpiarTabla();
                //temp.entorno.metodoPrincipal = new Entorno(temp.entorno.metodoPrincipal);
                if (nodo.ChildNodes.Count == 2)//llamada con parametros
                {
                    if (nmetodo.parametros.Count == 0)
                    {
                        errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "No se encuentra el método " + nodo.ChildNodes[0].Token.Text + " con el numero de parametros especificados "));
                    }else
                    {
                        if (!verificarParametros(nodo.ChildNodes[1], ref nmetodo, ref ent))
                        {
                            errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "No se encuentra el método " + nodo.ChildNodes[0].Token.Text + " con los parametros especificos "));
                            nmetodo.tipo = "Error";
                            return nmetodo;
                        }
                    }
                }
                else
                {
                    if(nmetodo.parametros.Count > 0)
                    {
                        errores.Add(new Error(nodo.ChildNodes[0].Token.Location.Line, nodo.ChildNodes[0].Token.Location.Column, "Semantico", "No se encuentra el método " + nodo.ChildNodes[0].Token.Text+ " sin parametros "));
                    }
                }
                //actual = temp.entorno;
                Entorno delmetodo = nmetodo.entorno;
                delmetodo.metodoPrincipal.Retorno = null;
                ejecutar(nmetodo.cuerpo, ref delmetodo);
                nmetodo.entorno = delmetodo;
                
                
                if (nmetodo.entorno.metodoPrincipal.retorna && nmetodo.entorno.metodoPrincipal.Retorno!=null)
                {
                    temp = nmetodo.entorno.metodoPrincipal.Retorno;
                }
                //actual = guardaractual;
            }

            return temp;
        }

        private Validacion compatibles(String t1, String t2)
        {
            Validacion valido = new Validacion();
            if (t1.Equals(t2, StringComparison.InvariantCultureIgnoreCase))
            {
                valido.valido = true;
                valido.tipo = t1;
            }
            else if (t1.Equals("decimal", StringComparison.InvariantCultureIgnoreCase) && t2.Equals("entero", StringComparison.InvariantCultureIgnoreCase))
            {
                valido.valido = true;
                valido.tipo = t1;
            }
            else if ((t1.Equals("entero", StringComparison.InvariantCultureIgnoreCase)|| t1.Equals("decimal", StringComparison.InvariantCultureIgnoreCase)) && t2.Equals("char", StringComparison.InvariantCultureIgnoreCase))
            {
                valido.valido = true;
                valido.tipo = t1;
            }else
            {
                valido.valido = false;
                valido.tipo = "Error";
                valido.mensaje = "No se puede asignar un " + t2 + " a un  " + t1;
            }
            return valido;

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
                        //parametros[i].nombre = metodo.parametros[i];
                        metodo.entorno.HT["var_" + metodo.parametros[i].ToLower()] = new Tipo(parametros[i]);
                        ((Tipo)metodo.entorno.HT["var_" + metodo.parametros[i].ToLower()]).nombre = metodo.parametros[i];
                    }
                    else
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

        private Tipo crearObjeto(ParseTreeNode nodo, ref Entorno entorno)
        {
            Tipo retorno = new Tipo();
            switch (nodo.ToString())
            {
                case "OBJETO":
                    retorno  = crearObjeto(nodo.ChildNodes[0], ref entorno);
                    retorno.esArray = true;
                    break;
                case "LISTAOBJETOS":
                    switch (nodo.ChildNodes.Count)
                    {
                        case 2:
                            retorno = crearObjeto(nodo.ChildNodes[0], ref entorno);
                            Tipo segundo = Verificacion(nodo.ChildNodes[1], ref entorno);
                            if (retorno.arreglo.Count > 0)//significa que es un arreglo
                            {
                                if (retorno.esArray)//si salio de un nodo OBJETO
                                {   
                                    if (segundo.tipo.Equals(retorno.tipo, StringComparison.InvariantCultureIgnoreCase)){
                                        Tipo arreglo = new Tipo();
                                        arreglo.tipo = retorno.tipo;
                                        arreglo.esArray = true;
                                        arreglo.arreglo.Add(retorno);
                                        arreglo.arreglo.Add(new Tipo(segundo));
                                        return arreglo;
                                    }
                                    
                                }else
                                {
                                    retorno.arreglo.Add(new Tipo(segundo));
                                }
                            }else
                            {

                                if (retorno.tipo.Equals(segundo.tipo))
                                {
                                    Tipo arreglo = new Tipo();
                                    arreglo.tipo = retorno.tipo;
                                    //arreglo.esArray = true;
                                    arreglo.arreglo.Add(retorno);
                                    arreglo.arreglo.Add(new Tipo(segundo));
                                    return arreglo;
                                }else
                                {
                                    errores.Add(new Error(retorno.fila, retorno.columna, "Semantico", "No puede agregar un arreglo un tipo " + retorno.tipo + " junto a un tipo " + segundo.tipo));
                                }
                               
                            }
                            break;
                        case 1:
                            retorno = Verificacion(nodo.ChildNodes[0], ref entorno);
                            Console.WriteLine(retorno.nombre);
                            return new Tipo(retorno);
                    }
                    break;
            }
            return retorno;
        }

        private void ejecutarFor(ParseTreeNode nodo, ref Entorno entorno)
        {
            string nombre = "";
            switch (nodo.ChildNodes[0].ChildNodes[0].ChildNodes.Count)
            {
                case 3:
                    nombre = nodo.ChildNodes[0].ChildNodes[0].ChildNodes[1].ChildNodes[0].Token.Text;
                    if (entorno.HT.ContainsKey("var_" + nombre))
                    {
                        entorno.HT.Remove("var_" + nombre);
                        //((Tipo)entorno.HT["var_" + nombre]).nombre = "";
                    }
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
                    Entorno entFor = new Entorno();
                    entFor.siguiente = entorno;
                    entFor.metodoPrincipal = actual.metodoPrincipal;
                    entFor.nombre = "cicloFor";
                    entFor.esCiclo = true;
                    entFor.HT.Add("var_" + nombre.ToLower(), inicial);
                    inicial = Verificacion(nodo.ChildNodes[1], ref entorno);//verifica el valor booleano del segundo argumento
                    if (inicial.tipo.Equals("booleano"))
                    {
                        //actual = nuevo;
                        //Entorno aux = entFor;
                        ParseTreeNode temp = new ParseTreeNode(null, new SourceSpan());
                        temp.ChildNodes.Add(nodo.ChildNodes[2]);

                        while (inicial.valorBoleano)
                        {
                            ejecutar(nodo.ChildNodes[3], ref entFor);
                            ejecutar(temp, ref entFor);
                            inicial = Verificacion(nodo.ChildNodes[1], ref entFor);
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
            //variables.Select(consola.TextLength, 0);
            //variables.SelectionColor = Color.AliceBlue;
            
            foreach (Entorno en in listaEntornos.Values)
            {
                variables.AppendText("El entorno " + en.nombre + " Tiene " + en.HT.Count + " Elementos en su nombre \n");
                foreach (String k in en.HT.Keys)
                {
                    if (k.Substring(0, 4) == "var_")
                    {
                        actual = (Tipo)en.HT[k];
                        variables.AppendText("key: " + k + " type: " + actual.tipo + " Valor: " + actual.valorString +  "\n");
                    }
                    else
                    {
                        actual = (Tipo)en.HT[k];
                        variables.AppendText("key: " + k + " type: " + actual.tipo + ".  metodo \n");
                    }
                }
            }
        }

        //comando ref voyage
    }
    
}
