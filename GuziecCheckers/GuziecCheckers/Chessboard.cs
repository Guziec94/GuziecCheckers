using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Shapes;

namespace GuziecCheckers
{
    public class Chessboard
    {
        #region Struktury itp
        public struct Field
        {
            public char column { get; set; }
            public int row { get; set; }

            public int ownership { get; set; }

            public Point leftUp { get; set; }
            public Point leftDown { get; set; }
            public Point rightUp { get; set; }
            public Point rightDown { get; set; }

            public Field(char column, int row, int ownership, Point leftUp, Point rightUp, Point leftDown, Point rightDown)
            {
                this.column = column;
                this.row = row;

                this.ownership = ownership;

                this.leftUp = leftUp;
                this.rightUp = rightUp;
                this.leftDown = leftDown;
                this.rightDown = rightDown;
            }
        }

        public struct PawnsInfo
        {
            public static Bgr minColorRange1 { get; set; }
            public static Bgr maxColorRange1 { get; set; }

            public static Bgr minColorRange2 { get; set; }
            public static Bgr maxColorRange2 { get; set; }
        }

        public struct Move
        {
            public char column { get; set; }
            public int row { get; set; }
        }
        #endregion
        private int _size;
        private List<string> _moves;

        public List<Field> _fields;
        
        public static Capture kamera = new Capture(1);

        /// <summary>
        /// Metoda kalibrująca zmiany położenia pól szachownicy względem kamery oraz zmiany ułożenia pionów na szachownicy
        /// </summary>
        /// <param name="img">Przeszukiwany obraz</param>
        /// <param name="draw">Rysuje punkty wskazujące położenie pól szachownicy</param>
        /// <returns></returns>
        public void Calibration(Image<Bgr, byte> img, bool drawFields = false)
        {
            #region Kalibracja kamery
            Size patternSize = new Size((_size - 1), (_size - 1));

            VectorOfPointF corners = new VectorOfPointF();
            bool found = CvInvoke.FindChessboardCorners(img, patternSize, corners);
            #endregion
            #region Aktualizacja położenia pól
            if (corners.Size == Convert.ToInt32(Math.Pow((_size - 1), 2)))
            {
                Bitmap bitmapa = img.ToBitmap();

                _fields.Clear();

                char column = 'A';
                int row = 1;

                for (int i = 0; i < corners.Size - (_size - 1); i++)
                {
                    if ((i + 1) % (_size - 1) == 0) { column = (char)(Convert.ToUInt16(column) + 1); continue; }

                    Point leftUp = new Point((int)corners[i].X, (int)corners[i].Y);
                    Point rightUp = new Point((int)corners[i + 1].X, (int)corners[i + 1].Y);
                    Point leftDown = new Point((int)corners[i + (_size - 1)].X, (int)corners[i + (_size - 1)].Y);
                    Point rightDown = new Point((int)corners[i + _size].X, (int)corners[i + _size].Y);

                    Field f = new Field(column, row++, 0, leftUp, rightUp, leftDown, rightDown);

                    int countP1 = 0, countP2 = 0;
                    for(int j = f.leftUp.X; j < f.rightUp.X; j++)
                    {
                        for(int k = f.leftUp.Y; k < f.leftDown.Y; k++)
                        {
                            Color pixel = bitmapa.GetPixel(j, k);

                            if((pixel.R >= PawnsInfo.minColorRange1.Red && pixel.G >= PawnsInfo.minColorRange1.Green && pixel.B >= PawnsInfo.minColorRange1.Blue) &&
                               (pixel.R <= PawnsInfo.maxColorRange1.Red && pixel.G <= PawnsInfo.maxColorRange1.Green && pixel.B <= PawnsInfo.maxColorRange1.Blue)) countP1++;

                            if ((pixel.R >= PawnsInfo.minColorRange2.Red && pixel.G >= PawnsInfo.minColorRange2.Green && pixel.B >= PawnsInfo.minColorRange2.Blue) &&
                               (pixel.R <= PawnsInfo.maxColorRange2.Red && pixel.G <= PawnsInfo.maxColorRange2.Green && pixel.B <= PawnsInfo.maxColorRange2.Blue)) countP2++;
                        }
                    }

                    double p1 = ((double)countP1 / ((f.rightUp.X - f.leftUp.X) * (f.leftDown.Y - f.leftUp.Y)));
                    double p2 = ((double)countP2 / ((f.rightUp.X - f.leftUp.X) * (f.leftDown.Y - f.leftUp.Y)));

                    if (p1 > 0.1) f.ownership = 1;
                    else if (p2 > 0.1) f.ownership = 2;

                    _fields.Add(f);

                    if (row == (_size - 1)) row = 1;
                }
            }
            #endregion

            #region Wyświetlanie pól
            if (drawFields) CvInvoke.DrawChessboardCorners(img, patternSize, corners, found);
            #endregion
        }
        
        /// <summary>
        /// Konstruktor klasy reprezentującej obiekt szachownicy
        /// </summary>
        /// <param name="minColorRange1">Dolny zakres koloru pionów pierwszego gracza</param>
        /// <param name="minColorRange2">Dolny zakres koloru pionów drugiego gracza</param>
        /// <param name="maxColorRange1">Górny zakres koloru pionów pierwszego gracza</param>
        /// <param name="maxColorRange2">Górny zakres koloru pionów drugiego gracza</param>
        /// <param name="minDistance1">Minimalny dystans pomiędzy pionami pierwszego gracza</param>
        /// <param name="minDistance2">Minimalny dystans pomiędzy pionami drugiego gracza</param>
        /// <param name="minRadius1">Dolny zakres długości promienia pionów pierwszego gracza</param>
        /// <param name="minRadius2">Dolny zakres długości promienia pionów drugiego gracza</param>
        /// <param name="maxRadius1">Górny zakres długości promienia pionów pierwszego gracza</param>
        /// <param name="maxRadius2">Górny zakres długości promienia pionów drugiego gracza</param>
        /// <param name="size"></param>
        public Chessboard(int size = 10)
        {
            try
            {                
                _size = size;

                _fields = new List<Field>();
                _moves = new List<string>();
            }
            catch (Exception ex) { System.Windows.MessageBox.Show(ex.Message); }
        }

        /// <summary>
        /// Funkcja zwracająca listę stringów reprezentujących możliwe sekwencje ruchów dla wskazanego w parametrze gracza
        /// </summary>
        /// <param name="n">Numer gracza (1 lub 2)</param>
        /// <returns>Lista stringów reprezentujących możliwe sekwencje ruchów gracza</returns>
        public List<string> FindMoves(int n)
        {
            _moves.Clear();

            if (n == 1 || n == 2)
            {
                List<Field> fields = new List<Field>(_fields);
                FindMoves(n, fields);

                if (n == 1) _moves.RemoveAll(m => m.Length == 5 && m[1] > m[4] && m[1] - m[4] == 1);
                else _moves.RemoveAll(m => m.Length == 5 && m[4] > m[1] && m[4] - m[1] == 1);
            }

            return _moves;
        }

        /// <summary>
        /// Funkcja wykorzystywana przy wyznaczaniu możliwych do wykonania przez gracza ruchów
        /// </summary>
        /// <param name="n">Numer gracza (1 lub 2)</param>
        /// <param name="fields">Lista pól reprezentujących szachownicę</param>
        /// <param name="recurrence">Zmienna pomocnicza, używana w rekurencji</param>
        private void FindMoves(int n, List<Field> fields, bool recurrence = false)
        {
            foreach (Field field in fields)
            {
                if (field.ownership == n)
                {
                    string current = field.column.ToString() + field.row;
                    int index = _moves.FindIndex(m => m.IndexOf(current) == m.Length - current.Length);

                    #region Lewy górny róg
                    Predicate<Field> upLeft = f => f.column == (field.column - 1) && f.row == (field.row + 1);

                    if (fields.Exists(upLeft))
                    {
                        Field next = fields.Find(upLeft);

                        if (next.ownership == 0 && !recurrence)
                        {
                            string move = next.column.ToString() + next.row;

                            if (index >= 0) _moves.Add(_moves[index] + " " + move);
                            else _moves.Add(current + " " + move);
                        }
                        else if (next.ownership > 0 && next.ownership != field.ownership)
                        {
                            upLeft = f => f.column == (next.column - 1) && f.row == (next.row + 1);

                            if (_fields.Exists(upLeft))
                            {
                                Field doubleNext = _fields.Find(upLeft);

                                if (doubleNext.ownership == 0)
                                {
                                    string move = doubleNext.column.ToString() + doubleNext.row;

                                    if (index >= 0) _moves.Add(_moves[index] + " " + move);
                                    else _moves.Add(current + " " + move);

                                    List<Field> copy = new List<Field>(fields);

                                    copy[copy.IndexOf(field)] = new Field(field.column, field.row, 0, field.leftUp, field.rightUp, field.leftDown, field.rightDown);
                                    copy[copy.IndexOf(next)] = new Field(next.column, next.row, 0, next.leftUp, next.rightUp, next.leftDown, next.rightDown);
                                    copy[copy.IndexOf(doubleNext)] = new Field(doubleNext.column, doubleNext.row, n, doubleNext.leftUp, doubleNext.rightUp, doubleNext.leftDown, doubleNext.rightDown);

                                    FindMoves(n, copy, true);
                                }
                            }
                        }
                    }
                    #endregion
                    #region Prawy górny róg
                    Predicate<Field> upRight = f => f.column == (field.column + 1) && f.row == (field.row + 1);

                    if (fields.Exists(upRight))
                    {
                        Field next = fields.Find(upRight);

                        if (next.ownership == 0 && !recurrence)
                        {
                            string move = next.column.ToString() + next.row;

                            if (index >= 0) _moves.Add(_moves[index] + " " + move);
                            else _moves.Add(current + " " + move);
                        }
                        else if (next.ownership > 0 && next.ownership != field.ownership)
                        {
                            upRight = f => f.column == (next.column + 1) && f.row == (next.row + 1);

                            if (_fields.Exists(upRight))
                            {
                                Field doubleNext = _fields.Find(upRight);

                                if (doubleNext.ownership == 0)
                                {
                                    string move = doubleNext.column.ToString() + doubleNext.row;

                                    if (index >= 0) _moves.Add(_moves[index] + " " + move);
                                    else _moves.Add(current + " " + move);

                                    List<Field> copy = new List<Field>(fields);

                                    copy[copy.IndexOf(field)] = new Field(field.column, field.row, 0, field.leftUp, field.rightUp, field.leftDown, field.rightDown);
                                    copy[copy.IndexOf(next)] = new Field(next.column, next.row, 0, next.leftUp, next.rightUp, next.leftDown, next.rightDown);
                                    copy[copy.IndexOf(doubleNext)] = new Field(doubleNext.column, doubleNext.row, n, doubleNext.leftUp, doubleNext.rightUp, doubleNext.leftDown, doubleNext.rightDown);

                                    FindMoves(n, copy, true);
                                }
                            }
                        }
                    }
                    #endregion
                    #region Prawy dolny róg
                    Predicate<Field> downRight = f => f.column == (field.column + 1) && f.row == (field.row - 1);

                    if (fields.Exists(downRight))
                    {
                        Field next = fields.Find(downRight);

                        if (next.ownership == 0 && !recurrence)
                        {
                            string move = next.column.ToString() + next.row;

                            if (index >= 0) _moves.Add(_moves[index] + " " + move);
                            else _moves.Add(current + " " + move);
                        }
                        else if (next.ownership > 0 && next.ownership != field.ownership)
                        {
                            downRight = f => f.column == (next.column + 1) && f.row == (next.row - 1);

                            if (_fields.Exists(downRight))
                            {
                                Field doubleNext = _fields.Find(downRight);

                                if (doubleNext.ownership == 0)
                                {
                                    string move = doubleNext.column.ToString() + doubleNext.row;

                                    if (index >= 0) _moves.Add(_moves[index] + " " + move);
                                    else _moves.Add(current + " " + move);

                                    List<Field> copy = new List<Field>(fields);

                                    copy[copy.IndexOf(field)] = new Field(field.column, field.row, 0, field.leftUp, field.rightUp, field.leftDown, field.rightDown);
                                    copy[copy.IndexOf(next)] = new Field(next.column, next.row, 0, next.leftUp, next.rightUp, next.leftDown, next.rightDown);
                                    copy[copy.IndexOf(doubleNext)] = new Field(doubleNext.column, doubleNext.row, n, doubleNext.leftUp, doubleNext.rightUp, doubleNext.leftDown, doubleNext.rightDown);

                                    FindMoves(n, copy, true);
                                }
                            }
                        }
                    }
                    #endregion
                    #region Lewy dolny róg
                    Predicate<Field> downLeft = f => f.column == (field.column - 1) && f.row == (field.row - 1);

                    if (fields.Exists(downLeft))
                    {
                        Field next = fields.Find(downLeft);

                        if (next.ownership == 0 && !recurrence)
                        {
                            string move = next.column.ToString() + next.row;

                            if (index >= 0) _moves.Add(_moves[index] + " " + move);
                            else _moves.Add(current + " " + move);
                        }
                        else if (next.ownership > 0 && next.ownership != field.ownership)
                        {
                            downLeft = f => f.column == (next.column - 1) && f.row == (next.row - 1);

                            if (_fields.Exists(downLeft))
                            {
                                Field doubleNext = _fields.Find(downLeft);

                                if (doubleNext.ownership == 0)
                                {
                                    string move = doubleNext.column.ToString() + doubleNext.row;

                                    if (index >= 0) _moves.Add(_moves[index] + " " + move);
                                    else _moves.Add(current + " " + move);

                                    List<Field> copy = new List<Field>(fields);

                                    copy[copy.IndexOf(field)] = new Field(field.column, field.row, 0, field.leftUp, field.rightUp, field.leftDown, field.rightDown);
                                    copy[copy.IndexOf(next)] = new Field(next.column, next.row, 0, next.leftUp, next.rightUp, next.leftDown, next.rightDown);
                                    copy[copy.IndexOf(doubleNext)] = new Field(doubleNext.column, doubleNext.row, n, doubleNext.leftUp, doubleNext.rightUp, doubleNext.leftDown, doubleNext.rightDown);

                                    FindMoves(n, copy, true);
                                }
                            }
                        }
                    }
                    #endregion
                }
            }
        }
    }
}
