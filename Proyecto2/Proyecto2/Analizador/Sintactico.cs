using Irony.Parsing;
using Proyecto2.Graficos;
using Proyecto2.Objetos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proyecto2.Analizador
{
    class Sintactico : Grammar
    {
        public ArrayList errores { get; set; }
        public List<Token> token { get; set; }
        public ParseTreeNode raiz { get; set; }

        public Sintactico()
        {
            errores = new ArrayList();
            token = new List<Token>();
        }

        public  ParseTreeNode analizar(String cadena, RichTextBox consola)
        {
            Gramatica gramatica = new Gramatica();
            LanguageData lenguaje = new LanguageData(gramatica);
            Parser parser = new Parser(lenguaje);
            ParseTree arbol = parser.Parse(cadena);
            /*ParseTreeNode*/
            raiz = arbol.Root;

            if (gramatica.Error.Count > 0 || raiz == null)
            {
                MessageBox.Show("hay un error");
                errores = gramatica.Error;
                return null;
            }
            else
            {
                token = arbol.Tokens;
                generarImagen(raiz);
                Accionar accion = new Accionar(consola);
                accion.errores = errores;
                accion.guardar(raiz);
                if (accion.errores.Count > 0)
                {
                    MessageBox.Show("hay un error");
                    errores = accion.errores;
                    return null;
                }
                else
                {
                    if(accion.principal!= null)
                    {
                        Entorno principe = accion.principal;
                        if (principe.HT.ContainsKey("main")) {                            
                            if (principe.nodoimplemento != null)
                            {
                                principe.implementos = accion.agregarEntorno(principe.nodoimplemento, ref principe );
                            }
                            Tipo main = (Tipo)principe.HT["main"];
                            accion.actual = main.entorno;
                            main.entorno.siguiente = principe;
                            Entorno ent = main.entorno;
                            accion.ejecutar(main.cuerpo, ref ent );
                        }else
                        {
                            Console.WriteLine("no hay main");
                        }
                    }
                    else
                    {
                        Console.WriteLine("no hay metodo main");
                    }
                    if (accion.errores.Count > 0)
                    {
                        MessageBox.Show("hay un error");
                        errores = accion.errores;
                        return null;
                    }
                }
                MessageBox.Show("imagen generada exitosamente");
                accion.imprimirTodo();
                return raiz;
            }


        }


        public static void generarImagen(ParseTreeNode raiz)
        {

            String grafoDOT = ControlDot.getDOT(raiz);
            File.Create("Grafica.dot").Dispose();
            TextWriter tw = new StreamWriter("Grafica.dot");
            tw.WriteLine(grafoDOT);
            tw.Close();

            ProcessStartInfo startinfo = new ProcessStartInfo("C:\\GraphViz\\bin\\dot.exe");
            Process Process;
            startinfo.RedirectStandardOutput = true;
            startinfo.UseShellExecute = false;
            startinfo.CreateNoWindow = true;
            startinfo.Arguments = "-Tpng Grafica.dot -o grafo.png";
            Process = Process.Start(startinfo);
            Process.Close();
            

        }

        public List<Token> Tokens
        {
            get { return token; }
        }
        public ArrayList Error
        {
            get { return errores; }
        }
    }
}
