using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EJEMPLO_DIBUJAR
{
    class Class1
    {
        public Draw drawForm = new Draw();

        public void dibujo()
        {
            drawForm.Show();
            drawForm.Size = new Size(800, 600);
            drawForm.Text = "Flor de Loto";

            DrawLine(0, 0, 200, 200, 0, "red");
            DrawLine(200, 0, 0, 200, 0, "red");

            DrawCircle(50, 200, 0, false, "#107800");

            DrawSquare(200, 200, 100, 150, false, "#0abde3");

            DrawSquare(350, 350, 50, 150, true, "#14f617");

            DrawLine(0, 0, 500, 500, 5, "#ff6700");
            DrawLine(0, 500, 500, 0, 5, "#005eff");

            DrawCircle(125, 190, 190, true, "#107800");
            DrawCircle(125, 315, 190, true, "#107800");
            DrawCircle(125, 190, 315, true, "#107800");
            DrawCircle(125, 315, 315, true, "#107800");

            DrawCircle(125, 250, 125, true, "#fff400");
            DrawCircle(125, 250, 375, true, "#fff400");
            DrawCircle(125, 375, 250, true, "#fff400");
            DrawCircle(125, 125, 250, true, "#fff400");

            DrawCircle(90, 250, 250, true, "Red");

            //DrawSquare(0, 0, 100, 100, false, "red");
            //DrawSquare(0, 0, 100, 100, true, "red");
            //DrawTriangle(100, 0, 0, 200, 200, 200, false, "red");
            //DrawTriangle(10, 0, 0, 20, 20, 20, true, "red");
        }

        public void DrawCircle(int Radio, int PosX, int PosY, bool relleno, string color)
        {
            Graphics g = drawForm.CreateGraphics();
            Color _color = System.Drawing.ColorTranslator.FromHtml(color);
            if (relleno == false)
            {

                System.Drawing.Pen myPen = new System.Drawing.Pen(_color);
                int widthEllipse = 2 * Radio;
                int heightEllipse = 2 * Radio;
                g.DrawEllipse(myPen, new Rectangle(PosX - Radio, PosY - Radio, widthEllipse, heightEllipse));
                myPen.Dispose();
                g.Dispose();
            }
            else if (relleno == true)
            {

                SolidBrush redBrush = new SolidBrush(_color);
                int widthEllipse = 2 * Radio;
                int heightEllipse = 2 * Radio;
                g.FillEllipse(redBrush, PosX - Radio, PosY - Radio, widthEllipse, heightEllipse);
            }
        }


        private void DrawSquare(int PosX, int PosY, int Width, int Height, bool relleno, string color)
        {

            Graphics g = drawForm.CreateGraphics();
            Color _color = System.Drawing.ColorTranslator.FromHtml(color);
            if (relleno == false)
            {
                System.Drawing.Pen myPen = new System.Drawing.Pen(_color);
                int PosXRec = Width / 2;
                int PosYRec = Height / 2;
                g.DrawRectangle(myPen, new Rectangle(PosX - PosXRec, PosY - PosYRec, Width, Height));
                myPen.Dispose();
                g.Dispose();
            }
            else if (relleno == true)
            {
                SolidBrush redBrush = new SolidBrush(_color);
                int PosXRec = Width / 2;
                int PosYRec = Height / 2;
                RectangleF rect = new RectangleF(PosX - PosXRec, PosY - PosYRec, Width, Height);
                g.FillRectangle(redBrush, rect);
            }
        }


        private void DrawTriangle(int PosX1, int PosY1, int PosX2, int PosY2, int PosX3, int PosY3, bool relleno, string color)
        {

            Graphics g = drawForm.CreateGraphics();
            Color _color = System.Drawing.ColorTranslator.FromHtml(color);
            if (relleno == false)
            {
                Pen blackPen = new Pen(_color, 0);
                PointF point1 = new PointF(PosX1, PosY1);
                PointF point2 = new PointF(PosX2, PosY2);
                PointF point3 = new PointF(PosX3, PosY3);
                PointF[] curvePoints =
                {
                    point1,
                    point2,
                    point3
                };
                g.DrawPolygon(blackPen, curvePoints);
            }
            else if (relleno == true)
            {
                SolidBrush blueBrush = new SolidBrush(_color);
                PointF point1 = new PointF(PosX1, PosY1);
                PointF point2 = new PointF(PosX2, PosY2);
                PointF point3 = new PointF(PosX3, PosY3);
                PointF[] curvePoints =
                {
                    point1,
                    point2,
                    point3
                };
                g.FillPolygon(blueBrush, curvePoints);
            }
        }

        private void DrawLine(int PosX1, int PosY1, int PosX2, int PosY2, int Thickness, string color)
        {

            Graphics g = drawForm.CreateGraphics();
            Color _color = System.Drawing.ColorTranslator.FromHtml(color);
            Pen blackPen = new Pen(_color, Thickness);

            PointF point1 = new PointF(PosX1, PosY1);
            PointF point2 = new PointF(PosX2, PosY2);
            PointF[] curvePoints =
                     {
                 point1,
                 point2

             };
            g.DrawPolygon(blackPen, curvePoints);
        }
    }
}
