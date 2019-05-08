using Irony.Parsing;
using Proyecto2.Analizador;
using Proyecto2.Errores;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proyecto2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Error.consola = this.rtbConsola;
            tabs.Add(tab0);
        }
        Sintactico analizr;
        ArrayList ListaErrores = new ArrayList();
        List<Token> ListaTokens;
        ParseTreeNode resultado;

        List<TabPage> tabs = new List<TabPage>();
        List<string> paths = new List<string>();

        String fileContent = string.Empty;

        private void btnAnalizar_Click(object sender, EventArgs e)
        {
            ListaErrores = new ArrayList();
            analizr = new Sintactico();
            FastColoredTextBoxNS.FastColoredTextBox seleccion  = pestanas.SelectedTab.Controls.OfType<FastColoredTextBoxNS.FastColoredTextBox>().Reverse().FirstOrDefault();

            resultado = analizr.analizar(seleccion.Text.Replace("\\", "\\\\"), rtbConsola);
            if (resultado != null)
            {
                ListaTokens = analizr.Tokens;
                Console.WriteLine("Correcto! :D se feliz ");

            }
            else
            {
                //ListaErrores = analizr.Error;
                //Console.WriteLine("Hay un pinshi error XD");

                //foreach (Error error in ListaErrores)
                //{
                //    Console.WriteLine(error.Linea + "  -  " + error.Columna + "   -   " + error.Desc + "  -  " + error.Tipo);
                //}
            }
        }

        private void abrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Code from Microsoft examples :3 
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "fi files (*.fi)|*.fi|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 0;
                openFileDialog.RestoreDirectory = true;

                int index = pestanas.SelectedIndex;
                int numero = pestanas.TabCount + 1;
                


                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    String newpath = openFileDialog.FileName;
                    //paths.Add(newpath);
                    var fileStream = openFileDialog.OpenFile();

                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        //Console.WriteLine(seleccionada
                        fileContent = reader.ReadToEnd();
                        
                        TabPage myTabPage = nuevaPagina(Path.GetFileName(newpath), newpath, fileContent, numero);
                        pestanas.TabPages.Add(myTabPage);
                        tabs.Add(myTabPage);
                        pestanas.SelectedTab = myTabPage;
                    }
                }
            }
        }

        private void aSTToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(@"grafo.png");
            }
            catch (Exception)
            {
                MessageBox.Show("Error al abrir imagen");
            }
            
            //Process photoViewer = new Process();
            //photoViewer.StartInfo.FileName = @"The photo viewer file path";
            //photoViewer.StartInfo.Arguments = @"Your image file path";
            //photoViewer.Start();
        }

        private void reporteDeErroresToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void nuevoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int numero = pestanas.TabCount + 1;
            TabPage myTabPage = nuevaPagina("Nuevo " + numero, "", "", numero);
            pestanas.TabPages.Add(myTabPage);
            tabs.Add(myTabPage);            
            pestanas.SelectedTab = myTabPage;
        }

        private void guardarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }


        private TabPage nuevaPagina(String titulo, String ruta, String texto, int indice)
        {
            FastColoredTextBoxNS.FastColoredTextBox nuevo = new FastColoredTextBoxNS.FastColoredTextBox();
            TabPage myTabPage = new System.Windows.Forms.TabPage();

            nuevo.AutoScrollMinSize = new System.Drawing.Size(25, 15);
            nuevo.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            nuevo.Cursor = System.Windows.Forms.Cursors.IBeam;
            nuevo.Dock = System.Windows.Forms.DockStyle.Fill;
            nuevo.FoldingIndicatorColor = System.Drawing.Color.Black;
            nuevo.ForeColor = System.Drawing.Color.Black;
            nuevo.Location = new System.Drawing.Point(3, 3);
            nuevo.Name = "cajaTexto"+indice;
            nuevo.ServiceLinesColor = System.Drawing.Color.Orange;
            nuevo.Size = new System.Drawing.Size(1098, 313);
            nuevo.Text = fileContent;

            myTabPage.Controls.Add(nuevo);
            myTabPage.Location = new System.Drawing.Point(4, 23);
            myTabPage.Name = "tab"+indice;
            myTabPage.Padding = new System.Windows.Forms.Padding(3);
            myTabPage.Size = new System.Drawing.Size(1104, 319);
            myTabPage.TabIndex = 0;
            myTabPage.Text = titulo;
            myTabPage.UseVisualStyleBackColor = true;

            paths.Add(ruta);
            

            return myTabPage;
        }
    }
}
