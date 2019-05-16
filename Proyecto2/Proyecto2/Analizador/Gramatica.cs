using Irony.Parsing;
using Proyecto2.Errores;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proyecto2.Analizador
{
    class Gramatica : Grammar
    {

        public ArrayList errores = new ArrayList();

        public Gramatica() : base(false)
        {
            #region ER

            IdentifierTerminal id = new IdentifierTerminal("id"); //token para un id :'D
            RegexBasedTerminal numero = new RegexBasedTerminal("entero", "[0-9]+");
            RegexBasedTerminal ndecimal = new RegexBasedTerminal("decimal", "[0-9]+([.][0-9]+)");
            RegexBasedTerminal vchar = new RegexBasedTerminal("char", "'[\x00-\x7F]'");
            //RegexBasedTerminal valorbool = new RegexBasedTerminal("boleano", "\b(verdadero|falso)\b");
            StringLiteral cadena = new StringLiteral("cadena", "\"", StringOptions.IsTemplate);

            //RegexBasedTerminal comitex = new RegexBasedTerminal("comTex", "#\"");
            var comentario = new CommentTerminal("comentario", ">>", "\n", "\r\n");
            var Mcomentario = new CommentTerminal("Mcomentario", "<-", "->");
            base.NonGrammarTerminals.Add(comentario);
            base.NonGrammarTerminals.Add(Mcomentario);


            #endregion

            #region TERMINALS
            KeyTerm
                mas = ToTerm("+", "tknSuma"),
                menos = ToTerm("-", "tknMenos"),
                por = ToTerm("*", "tknPor"),
                divide = ToTerm("/", "tknDividido"),
                potencia = ToTerm("^", "tknPotencia"),
                igual = ToTerm("=", "tkIguual"),

                masmas = ToTerm ("++","masmas"),
                menosmenos = ToTerm("--", "menosmenos"),

                coma = ToTerm(",", "Coma"),
                semco = ToTerm(";", "semiColon"),
                punto = ToTerm(".", "punto"),
                dospuntos = ToTerm(":","dosPuntos"),

                pareA = ToTerm("(", "parea"),
                pareC = ToTerm(")", "pareC"),
                llavA = ToTerm("{", "llaveA"),
                llavC = ToTerm("}", "llaveC"),
                cora = ToTerm("[", "Cora<3"),
                corc = ToTerm("]", "Corc"),

                importar = ToTerm("importar", "importar"),
                clase = ToTerm("clase", "Clase")
                , publico = ToTerm("publico", "publico")
                , privado = ToTerm("privado", "privado")
                , entero = ToTerm("int", "int")
                , decima = ToTerm("double", "double")
                , boleano = ToTerm("bool", "bool")
                , caracter = ToTerm("char", "char")
                , stringo = ToTerm("string", "string")
                , arreglo = ToTerm("array", "arreglo")
                , verdadero1 = ToTerm("true","true")
                , falso1 = ToTerm("false")
                , nueva = ToTerm("new", "nueva")
                , metodo = ToTerm("void", "void")
                , sobreescribir = ToTerm("override", "override")
                , print = ToTerm("print","print")
                , show = ToTerm("show","show")
                , si = ToTerm("if")
                , sino = ToTerm("else")
                , main = ToTerm("main")
                , retornar = ToTerm("return")
                , para = ToTerm("for")
                , repetir = ToTerm("repeat")
                , mientras = ToTerm("while")
                , comprobar = ToTerm("comprobar")
                , caso = ToTerm("caso")
                , defecto = ToTerm("defecto")
                , salir = ToTerm("salir")
                , hacer = ToTerm("hacer")
                , verdadero = ToTerm("verdadero")
                , falso = ToTerm("falso")
                , continuar = ToTerm("continuar")
                , circle = ToTerm("circle")
                , triangle = ToTerm("triangle")
                , square = ToTerm("square")
                , line = ToTerm("line")
                , addfigure = ToTerm("addfigure")
                , figure = ToTerm("figure")

                , or = ToTerm("||", "OR")
                , and = ToTerm("&&", "AND")
                , not = ToTerm("!", "NOT")

                , mayorq = ToTerm(">", "mayorQue")
                , menorq = ToTerm("<", "menorQue")
                , mayorIgual = ToTerm(">=", "mayorIgual")
                , menorIgual = ToTerm("<=", "menorIgual")
                , igualigual = ToTerm("==", "igualIgual")
                , notIgual = ToTerm("!=", "notIgual")
                


                ;



            #endregion

            #region nonTerminals
            NonTerminal S = new NonTerminal("S"),// ACEPTACIOM
                      INICIO = new NonTerminal("INICIO"),
                      LISTACLASES = new NonTerminal("LISTACLASES"),
                      IMPORTAR = new NonTerminal("IMPORTAR"),
                      CUERPO = new NonTerminal("CUERPO"),
                      LISTACUERPO = new NonTerminal("LISTACUERPO"),
                      DECLARACION = new NonTerminal("DECLARACION"),
                      ASIGNACION = new NonTerminal("ASIGNACION"),
                      LISTAVARIABLES = new NonTerminal("LISTAVARIABLES"),
                      IGUALACION = new NonTerminal("IGUALACION"),
                      VISIBILIDAD = new NonTerminal("VISIBILIDAD"),
                      E = new NonTerminal("E")
                      , F = new NonTerminal("F")
                      , G = new NonTerminal("G")
                      , H = new NonTerminal("H")
                      , I = new NonTerminal("I")
                      , J = new NonTerminal("J")
                      , K = new NonTerminal("K")
                      , L = new NonTerminal("L")
                      , M = new NonTerminal("M")
                      , TIPO = new NonTerminal("TIPO")
                      , LLAMAR = new NonTerminal("LLAMAR")
                      , LISTAOBJETOS = new NonTerminal("LISTAOBJETOS")
                      , TAMANO = new NonTerminal("TAMANO")
                      , NUEVACLASE = new NonTerminal("NUEVACLASE")
                      , LOCALMETODO = new NonTerminal("LOCALMETODO")
                      , OBJETO = new NonTerminal("OBJETO")
                      , VALORARRAY = new NonTerminal("VALORARRAY")
                      , METODO = new NonTerminal("METODO")
                      , TIPOMETODO = new NonTerminal("TIPOMETODO")
                      , TMETODO = new NonTerminal("TMETODO")
                      , OVERRIDE = new NonTerminal("OVERRIDE")
                      , PARAMETROS = new NonTerminal("PARAMETROS")
                      , CUERPOMETODO = new NonTerminal("CUERPOMETODO")
                      , FUNCIONMETODO = new NonTerminal("FUNCIONMETODO")
                      , PRINT = new NonTerminal("PRINT")
                      , SHOW = new NonTerminal("SHOW")
                      , SI = new NonTerminal("SI")
                      , SINO = new NonTerminal("SINO")
                      , SINOSI = new NonTerminal("SINOSI")
                      , MAIN = new NonTerminal("MAIN")
                      , RETORNAR = new NonTerminal("RETORNAR")
                      , PARA = new NonTerminal("PARA")
                      , VARINICIAL = new NonTerminal("VARINICIAL")
                      , REPETIR = new NonTerminal("REPETIR")
                      , MIENTRAS = new NonTerminal("MIENTRAS")
                      , COMPROBAR = new NonTerminal("COMPROBAR")
                      , CASO = new NonTerminal("CASO")
                      , DEFECTO = new NonTerminal("DEFECTO")
                      , SALIR = new NonTerminal("SALIR")
                      , LISTACASO = new NonTerminal("LISTACASO")
                      , HACERMIENTRAS = new NonTerminal("HACERMIENTRAS")
                      , AUMENTODECREMENTO = new NonTerminal("AUMENTODECREMENTO")
                      , CONTINUAR = new NonTerminal("CONTINUAR")
                      , ADDFIGURE = new NonTerminal("ADDFIGURE")
                      , CIRCLE = new NonTerminal("CIRCLE")
                      , TRIANGLE = new NonTerminal("TRIANGLE")
                      , SQUARE = new NonTerminal("SQUARE")
                      , LINE = new NonTerminal("LINE")
                      , LFIGURA = new NonTerminal("LFIGURA")
                      , FIGURE = new NonTerminal("FIGURE")
                      , DECLARACIONOBJETO = new NonTerminal("DECLARACIONOBJETO")
                      , AUX = new NonTerminal("AUX")

                       ;
            NonTerminal a = new NonTerminal("a");
            #endregion

            #region GRAMMAR
            S.Rule = INICIO;

            INICIO.Rule = INICIO + LISTACLASES
                           | LISTACLASES
                           ;

            LISTACLASES.Rule = VISIBILIDAD +  clase + id + IMPORTAR + llavA + CUERPO + llavC
                                | VISIBILIDAD + clase + id + llavA + CUERPO + llavC
                                | clase + id + IMPORTAR + llavA + CUERPO + llavC
                                | clase + id + llavA + CUERPO + llavC
                                ;

            IMPORTAR.Rule = importar + LISTAVARIABLES;

            CUERPO.Rule = CUERPO + LISTACUERPO
                           | LISTACUERPO
                           ;
            LISTACUERPO.Rule = DECLARACION + semco
                                | DECLARACIONOBJETO + semco
                                | METODO
                                | MAIN
                                ;
            DECLARACION.Rule =
                VISIBILIDAD + TIPO + arreglo + LISTAVARIABLES + TAMANO
                | VISIBILIDAD + TIPO + arreglo + LISTAVARIABLES + TAMANO + IGUALACION
                | VISIBILIDAD + TIPO + LISTAVARIABLES
                | VISIBILIDAD + TIPO + LISTAVARIABLES + IGUALACION
                | TIPO + arreglo + LISTAVARIABLES + TAMANO
                | TIPO + arreglo + LISTAVARIABLES + TAMANO + IGUALACION
                | TIPO + LISTAVARIABLES
                | TIPO + LISTAVARIABLES + IGUALACION
                ;

            DECLARACIONOBJETO.Rule =
                VISIBILIDAD + AUX + arreglo + LISTAVARIABLES + TAMANO
                | VISIBILIDAD + AUX + arreglo + LISTAVARIABLES + TAMANO + IGUALACION
                | VISIBILIDAD + AUX + LISTAVARIABLES
                | VISIBILIDAD + AUX + LISTAVARIABLES + IGUALACION
                | AUX + arreglo + LISTAVARIABLES + TAMANO
                | AUX + arreglo + LISTAVARIABLES + TAMANO + IGUALACION
                | AUX + LISTAVARIABLES
                | AUX + LISTAVARIABLES + IGUALACION;

            AUX.Rule = id;

            DECLARACION.ErrorRule = SyntaxError + ";";


            TAMANO.Rule = TAMANO + cora + E + corc
                | cora + E + corc;


            LISTAVARIABLES.Rule =
                LISTAVARIABLES + coma + id
                | id;


            ASIGNACION.Rule = id + IGUALACION
                | id + TAMANO + IGUALACION
                | id + punto + id + IGUALACION
                | id + punto + id + TAMANO + IGUALACION
                ;

            TIPO.Rule =  entero | decima | caracter | boleano | stringo ;

            VISIBILIDAD.Rule = publico | privado ;

            IGUALACION.Rule = igual + E;

            NUEVACLASE.Rule = nueva + LOCALMETODO;

            LOCALMETODO.Rule =
                id + pareA + LISTAOBJETOS + pareC
                | id + pareA + pareC
                ;

            LLAMAR.Rule =
                id + punto + id + pareA + LISTAOBJETOS + pareC
                | id + punto + id + pareA + pareC
                ;

            LISTAOBJETOS.Rule = LISTAOBJETOS + coma + E
                | E;

            OBJETO.Rule = llavA + LISTAOBJETOS + llavC;

            VALORARRAY.Rule = id + TAMANO
                | id + punto + id + TAMANO;

            METODO.Rule = VISIBILIDAD + id + TIPOMETODO + OVERRIDE + pareA + PARAMETROS + pareC + llavA + CUERPOMETODO + llavC
                | VISIBILIDAD + id + arreglo + TIPOMETODO + TAMANO + OVERRIDE + pareA + PARAMETROS + pareC + llavA + CUERPOMETODO + llavC
                | id + TIPOMETODO + OVERRIDE + pareA + PARAMETROS + pareC + llavA + CUERPOMETODO + llavC
                | id + arreglo + TIPOMETODO + TAMANO + OVERRIDE + pareA + PARAMETROS + pareC + llavA + CUERPOMETODO + llavC
                ;

            TIPOMETODO.Rule = TIPO | TMETODO;

            TMETODO.Rule = metodo;

            OVERRIDE.Rule = sobreescribir | Empty;

            PARAMETROS.Rule = PARAMETROS + coma + TIPO + id
                | TIPO + id
                | PARAMETROS + coma + AUX + id
                | AUX + id
                | Empty;

            CUERPOMETODO.Rule = CUERPOMETODO + FUNCIONMETODO
                | FUNCIONMETODO
                ;

            FUNCIONMETODO.Rule =
                DECLARACION + semco
                | ASIGNACION + semco
                | DECLARACIONOBJETO + semco
                | LLAMAR + semco
                | PRINT + semco
                | SHOW + semco
                | SI
                | RETORNAR + semco
                | PARA
                | REPETIR
                | MIENTRAS
                | COMPROBAR
                | HACERMIENTRAS + semco
                | AUMENTODECREMENTO + semco
                | LOCALMETODO + semco
                | SALIR
                | CONTINUAR
                | ADDFIGURE
                | FIGURE
                ;

            PRINT.Rule = print + pareA + E + pareC;

            SHOW.Rule = show + pareA + E + coma + E + pareC;

            SI.Rule = 
                si + pareA + E + pareC + llavA + CUERPOMETODO + llavC + SINOSI + SINO
                | si + pareA + E + pareC + llavA + CUERPOMETODO + llavC + SINOSI
                | si + pareA + E + pareC + llavA + CUERPOMETODO + llavC + SINO
                | si + pareA + E + pareC + llavA + CUERPOMETODO + llavC ;

            SINOSI.Rule =
                SINOSI + sino + si + pareA + E + pareC + llavA + CUERPOMETODO + llavC
                | sino + si + pareA + E + pareC + llavA + CUERPOMETODO + llavC;

            SINO.Rule =
                sino + llavA + CUERPOMETODO + llavC;

            MAIN.Rule = main + pareA + pareC + llavA + CUERPOMETODO + llavC;

            RETORNAR.Rule = retornar + E;

            PARA.Rule = para + pareA + VARINICIAL + semco + E + semco + AUMENTODECREMENTO + pareC + llavA + CUERPOMETODO + llavC;

            VARINICIAL.Rule = DECLARACION | ASIGNACION;

            REPETIR.Rule = repetir + pareA + E + pareC + llavA + CUERPOMETODO + llavC;

            MIENTRAS.Rule = mientras + pareA + E + pareC + llavA + CUERPOMETODO + llavC;

            COMPROBAR.Rule = comprobar + pareA + E + pareC + llavA + LISTACASO + llavC;

            LISTACASO.Rule = LISTACASO + CASO | CASO;

            CASO.Rule = caso + E + dospuntos + CUERPOMETODO
                | defecto + dospuntos + CUERPOMETODO;

            SALIR.Rule = salir + semco;

            CONTINUAR.Rule = CONTINUAR + semco;

            HACERMIENTRAS.Rule = hacer + llavA + CUERPOMETODO + llavC + mientras + pareA + E + pareC;

            AUMENTODECREMENTO.Rule = 
                L + masmas 
                | L + menosmenos;

            ADDFIGURE.Rule = addfigure + pareA + LFIGURA + pareC + semco;

            LFIGURA.Rule = CIRCLE
                | TRIANGLE
                | SQUARE
                | LINE;

            CIRCLE.Rule = circle + pareA + E + coma + E + coma + E + coma + E + coma + E + pareC;

            TRIANGLE.Rule = triangle + pareA + E + coma + E + coma + E + coma + E + coma + E + coma + E + coma + E + coma + E + pareC;

            SQUARE.Rule = square + pareA + E + coma + E + coma + E + coma + E + coma + E + coma + E + pareC;

            LINE.Rule = line + pareA + E + coma + E + coma + E + coma + E + coma + E + coma + E + pareC;

            FIGURE.Rule = figure + pareA + E + pareC + semco;

            E.Rule =
                E + or + F
                | F;

            F.Rule =
                F + and + G
                | G;

            G.Rule = not + H
                | H;

            H.Rule = H + igualigual + I
                | H + notIgual + I
                | H + menorq + I
                | H + menorIgual + I
                | H + mayorq + I
                | H + mayorIgual + I
                | I
                ;

            I.Rule = I + mas + J
                | I + menos + J
                | J;

            J.Rule = J + por + K
                | J + divide + K
                | K;

            K.Rule = menos + M
                | M;

            M.Rule = M + potencia + L
                | L;

            L.Rule =  pareA + E + pareC
                | id
                | id + punto + id
                | numero
                | cadena
                | ndecimal
                | vchar
                | verdadero
                | falso
                | verdadero1
                | falso1
                | LLAMAR
                | NUEVACLASE
                | LOCALMETODO
                | OBJETO
                | VALORARRAY
                | AUMENTODECREMENTO
                ;

            #endregion


            #region preferences 
            this.Root = S;
            MarkPunctuation(igual, coma, semco, punto, dospuntos, pareA, pareC, llavA, llavC, cora, corc);//<para quitar nodos basura :3
            MarkPunctuation(addfigure, circle, triangle, square, line, figure);
            MarkPunctuation(retornar, show, para, si, sino, print, repetir, mientras, comprobar, clase, importar, main, hacer, nueva, caso);

            #endregion
        }

        public override void ReportParseError(ParsingContext context)
        {

            String error = (String)context.CurrentToken.ValueString;


            String type;
            int fila, columna;

            if (error.Contains("Invalid character"))
            {
                type = "Error Lexico";
                string DStr = "";
                char[] delimit = DStr.ToCharArray();
                string[] div = error.Split(delimit, 2);
                div = div[1].Split('.');
                error = "Caracter Invalido: " + div[0];

            }
            else
            {
                type = "Error sintactico";
            }
            fila = context.Source.Location.Line;
            columna = context.Source.Location.Column;
            Error nEr = new Error(fila, columna, type, error);

            this.errores.Add(nEr);
            base.ReportParseError(context);
        }


        public ArrayList Error
        {
            get { return errores; }

        }

    }
}
