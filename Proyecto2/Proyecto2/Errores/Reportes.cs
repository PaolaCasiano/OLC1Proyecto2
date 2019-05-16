using Irony.Parsing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proyecto2.Errores
{
    class Reportes
    {

        public ArrayList token;
        public ArrayList des;
        public ArrayList TokenID;

        public Reportes()
        {
            token = new ArrayList();
            des = new ArrayList();
            TokenID = new ArrayList();
        }



        public  void Errores(ArrayList errores)
        {
            try
            {
                string archivo = "Errores.html";
                int num = 1;
                StreamWriter file = new StreamWriter(archivo);
                file.WriteLine("<html> \n");
                file.WriteLine("<head>\n");
                file.WriteLine("<title> Reporte de Errores </title>\n");
                file.WriteLine("<link rel=\"stylesheet\" type=\"text/css\" href=\"mystyle.css\">");
                file.WriteLine("</head>\n");
                file.WriteLine("<body>\n");
                file.WriteLine("<div class=\"table-title\">");
                file.WriteLine("<h3>Reporte de Errores</h3>");
                file.WriteLine("</div>");

                file.WriteLine("<table class=\"table-fill\">");
                file.WriteLine("<thead>");
                file.WriteLine("<tr>");
                file.WriteLine("<th class=\"text-left\">No.</th>");
                file.WriteLine("<th class=\"text-left\">Fila</th>");
                file.WriteLine("<th class=\"text-left\">Columna</th>");
                file.WriteLine("<th class=\"text-left\">Tipo</th>");
                file.WriteLine("<th class=\"text-left\">Descripcion</th>");
                file.WriteLine("</tr>");
                file.WriteLine("</thead>");

                file.WriteLine("<tbody class=\"table-hover\">");
                foreach (Error e in errores)
                {
                    file.WriteLine("<tr>");
                    file.WriteLine(("<td class=\"text-left\">" + num + "</td>"));
                    num++;

                    file.WriteLine(("<td class=\"text-left\">" + e.Linea.ToString() + "</td>"));
                    file.WriteLine(("<td class=\"text-left\">" + e.Columna.ToString() + "</td>"));
                    file.WriteLine(("<td class=\"text-left\">" + e.Tipo.ToString() + "</td>"));
                    file.WriteLine(("<td class=\"text-left\">" + e.Desc.ToString() + "</td>"));
                    file.WriteLine("</tr>");
                }


                file.WriteLine("</tbody>");
                file.WriteLine("</table>");
                file.WriteLine("</body>");
                file.WriteLine("</html>");
                file.Close();
                //MessageBox.Show("Se creo html", "HTML Errores", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                
            }
            catch (Exception)
            {
                MessageBox.Show("NO Sirvio D:", "My Application", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }

        }




        public static void final(int esquivados, int destruidos, int punteo, string tiempo)
        {
            try
            {
                string archivo = "Final.html";
                int num = 1;
                StreamWriter file = new StreamWriter(archivo);
                file.WriteLine("<html> \n");
                file.WriteLine("<head>\n");
                file.WriteLine("<title> Reporte de Errores </title>\n");
                file.WriteLine("<link rel=\"stylesheet\" type=\"text/css\" href=\"mystyle.css\">");
                file.WriteLine("</head>\n");
                file.WriteLine("<body>\n");
                file.WriteLine("<div class=\"table-title\">");
                file.WriteLine("<h3>Reporte de Errores</h3>");
                file.WriteLine("</div>");

                file.WriteLine("<table class=\"table-fill\">");
                file.WriteLine("<thead>");
                file.WriteLine("<tr>");
                file.WriteLine("<th class=\"text-left\">Esquivados</th>");
                file.WriteLine("<th class=\"text-left\">Destruidos</th>");
                file.WriteLine("<th class=\"text-left\">Punteo</th>");
                file.WriteLine("<th class=\"text-left\">Tiempo</th>");
                file.WriteLine("</tr>");
                file.WriteLine("</thead>");

                file.WriteLine("<tbody class=\"table-hover\">");

                file.WriteLine("<tr>");
                file.WriteLine(("<td class=\"text-left\">" + esquivados.ToString() + "</td>"));
                num++;

                file.WriteLine(("<td class=\"text-left\">" + destruidos.ToString() + "</td>"));
                file.WriteLine(("<td class=\"text-left\">" + punteo.ToString() + "</td>"));
                file.WriteLine(("<td class=\"text-left\">" + tiempo + "</td>"));
                file.WriteLine("</tr>");

                file.WriteLine("</tbody>");
                file.WriteLine("</table>");
                file.WriteLine("</body>");
                file.WriteLine("</html>");
                file.Close();
                MessageBox.Show("Se creo html", "HTML Errores", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                Process.Start("Final.html");
            }
            catch (Exception)
            {
                MessageBox.Show("NO Sirvio D:", "My Application", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }

        }


        public void reporteToken(List<Token> tokens)
        {
            string archivo = "Tokens.html";
            int num = 1;

            try
            {
                StreamWriter file = new StreamWriter(archivo);
                //Console.WriteLine(archivo);
                file.WriteLine("<html> \n");
                file.WriteLine("<head>\n");
                file.WriteLine("<title> Reporte de Tokens </title>\n");
                file.WriteLine("</head>\n");
                file.WriteLine("<body>\n");
                //--------------------css----------------
                file.WriteLine("<STYLE type=\"text/css\"> <!--  \n");
                file.WriteLine("HEADER {text-align:center; background: #6f9379; margin:10px;}\n");
                file.WriteLine("BODY {color:black; text-indent:1cm;}\n");
                file.WriteLine("#seccion{ background: black; float: left; width: 59%; margin: 10px; }\n");
                file.WriteLine("P{font-family: calibri; font-size: 14px, text-align: justify; color: white;margin: 10px}\n");
                file.WriteLine("#lateral{float:right; width: 30%; margin:10px;background: #ded4da} \n");
                file.WriteLine("LI{font-family: calibri; font-size: 14px, text-align: justify; color: white;margin: 10px}\n");

                file.WriteLine("H1{font-family: verdana; font-size: 16px; color: #6f9379;}\n");

                file.WriteLine("H2{font-size: 14px; font-family:Arial;}\n");

                file.WriteLine("TABLE{font-family: Calibri; font-size: 16px;}\n");

                file.WriteLine("// -->\n");
                file.WriteLine(" </STYLE>\n");
                //---------------------------------------------------- 
                file.WriteLine("<center>\n");
                file.WriteLine("<h1>Reporte De Tokens</h1>\n");
                DateTime thisDay = DateTime.Today;

                file.WriteLine("<h2>Dia de ejecucion:" + thisDay.ToString("D") + " </h2>");
                file.WriteLine("<h2>Dia de ejecucion:" + DateTime.Now.ToShortTimeString() + " </h2>");



                file.WriteLine("</center>\n");
                file.WriteLine("<table align=center>\n");
                file.WriteLine("<tr>\n");
                file.WriteLine("<td>No.</td>\n");
                file.WriteLine("<td>Lexema</td>\n");
                file.WriteLine("<td>Token</td>\n");
                file.WriteLine("<td>descripcion</td>\n");
                file.WriteLine("</tr>\n");

                int number = 1;

                foreach (Token item in tokens)
                {
                    file.WriteLine("<tr>\n");
                    file.WriteLine(("<td>" + number + "</td>\n"));
                    file.WriteLine(("<td>" + item.ValueString + "</td>\n"));
                    String data = " ";
                    try
                    {
                        data = item.KeyTerm.ToString();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("kaka");
                    }
                    if (!data.ElementAt(0).Equals('+'))
                    {
                        data = "";
                    }
                    file.WriteLine(("<td>" + data + "</td>\n"));
                    file.WriteLine(("<td>" + item.Category + "</td>\n"));
                    file.WriteLine("</tr>\n");
                    number++;
                }


                file.WriteLine("</table>\n");
                file.WriteLine("</body>\n");
                file.WriteLine("</html>");
                file.Close();
                MessageBox.Show("Se creo html", "HTML Tokens", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch (Exception e)
            {
                MessageBox.Show("Error creo html", "HTML Tokens", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }

        public void reporteerror(ArrayList Listaerror)
        {
            string archivo = "Errores.html";
            int num = 1;

            try
            {
                StreamWriter file = new StreamWriter(archivo);
                Console.WriteLine(archivo);
                file.WriteLine("<html> \n");
                file.WriteLine("<head>\n");
                file.WriteLine("<title> Reporte de Errores </title>\n");
                file.WriteLine("</head>\n");
                file.WriteLine("<body>\n");
                //--------------------css----------------
                file.WriteLine("<STYLE type=\"text/css\"> <!--  \n");
                file.WriteLine("HEADER {text-align:center; background: #6f9379; margin:10px;}\n");
                file.WriteLine("BODY {background-color: rgb(28, 52, 63); text-indent:1cm;}\n");
                file.WriteLine("#seccion{ background: black; float: left; width: 59%; margin: 10px; }\n");
                file.WriteLine("P{font-family: calibri; font-size: 14px, text-align: justify; color: white ;margin: 10px}\n");
                file.WriteLine("#lateral{float:right; width: 30%; margin:10px;background: #ded4da} \n");
                file.WriteLine("LI{font-family: calibri; font-size: 14px, text-align: justify; color: white;margin: 10px}\n");

                file.WriteLine("H1{font-family: verdana; font-size: 16px; color: #6f9379;}\n");

                file.WriteLine("H2{font-size: 14px; font-family:Arial;}\n");

                file.WriteLine("TABLE{font-family: Calibri; font-size: 16px ; border: 2px solid white; border - radius: 5px;color: white;}\n");

                file.WriteLine("// -->\n");
                file.WriteLine(" </STYLE>\n");
                //---------------------------------------------------- 
                file.WriteLine("<center>\n");
                file.WriteLine("<h1>Reporte De Errores</h1>\n");
                DateTime thisDay = DateTime.Today;

                file.WriteLine("<h2>Dia de ejecucion:" + thisDay.ToString("D") + " </h2>");
                file.WriteLine("<h2>Hora de ejecucion:" + DateTime.Now.ToShortTimeString() + " </h2>");
                file.WriteLine("</center>\n");
                file.WriteLine("<table align=center>\n");
                file.WriteLine("<tr>\n");
                file.WriteLine("<td>No.</td>\n");
                file.WriteLine("<td>Fila</td>\n");
                file.WriteLine("<td>Columna</td>\n");
                file.WriteLine("<td>Tipo</td>\n");
                file.WriteLine("<td>descripcion</td>\n");
                file.WriteLine("</tr>\n");


                if (Listaerror.Count != 0)
                {

                    foreach (Error er in Listaerror)
                    {
                        file.WriteLine("<tr>\n");
                        file.WriteLine(("<td>" + num + "</td>\n"));
                        num++;

                        file.WriteLine(("<td>" + er.Linea + "</td>\n"));
                        file.WriteLine(("<td>" + er.Columna + "</td>\n"));
                        file.WriteLine(("<td>" + er.Tipo + "</td>\n"));
                        file.WriteLine(("<td>" + er.Desc + "</td>\n"));
                        file.WriteLine("</tr>\n");


                        //Console.WriteLine(er.Linea + "  -  " + er.Columna + "   -   " + er.Desc + "  -  " + er.Tipo);
                    }
                }

                file.WriteLine("</table>\n");
                file.WriteLine("</body>\n");
                file.WriteLine("</html>");
                file.Close();
                MessageBox.Show("Se creo html", "HTML Errores", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                Process.Start("Errores.html");
            }
            catch (Exception)
            {
                MessageBox.Show("NO Sirvio D:", "My Application", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }
    }
}
