using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;

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

            public static int minRadius { get; set; }
            public static int maxRadius { get; set; }
            public static double minDistance { get; set; }
        }

        public struct Move
        {
            public char column { get; set; }
            public int row { get; set; }
        }
        #endregion
        private int _size;
        private List<Field> _fields;

        private CircleF[] _pawnsPlayer1;
        private CircleF[] _pawnsPlayer2;

        private List<string> _moves;

        /// <summary>
        /// Metoda kalibrująca zmiany położenia pól szachownicy względem kamery oraz zmiany ułożenia pionów na szachownicy
        /// </summary>
        /// <param name="img">Przeszukiwany obraz</param>
        /// <param name="draw">Rysuje punkty wskazujące położenie pól szachownicy</param>
        /// <returns></returns>
        public void Calibration(Image<Bgr, byte> img, bool drawFields = false, bool drawPawnsPlayer1 = false, bool drawPawnsPlayer2 = false)
        {
            #region Kalibracja kamery
            Size patternSize = new Size((_size - 1), (_size - 1));

            VectorOfPointF corners = new VectorOfPointF();
            bool found = CvInvoke.FindChessboardCorners(img, patternSize, corners);
            #endregion
            #region Aktualizacja położenia pól i pionów
            if (corners.Size == Convert.ToInt32(Math.Pow((_size - 1), 2)))
            {
                _fields.Clear();

                char column = 'A';
                int row = 1;

                for (int i = 0; i < corners.Size - (_size - 1); i++)
                {
                    if ((i + 1) % (_size - 1) == 0) { column = (char)(Convert.ToUInt16(column) + 1); continue; }
                    _fields.Add(new Field(column, row++, 0, new Point((int)corners[i].X, (int)corners[i].Y), new Point((int)corners[i + 1].X, (int)corners[i + 1].Y), new Point((int)corners[i + (_size - 1)].X, (int)corners[i + (_size - 1)].Y), new Point((int)corners[i + _size].X, (int)corners[i + _size].Y)));

                    if (row == (_size - 1)) row = 1;
                }

                
            }
            #endregion
            #region Aktualizacja położenia pionów
            Image<Gray, byte> gray1 = img.InRange(PawnsInfo.minColorRange1, PawnsInfo.maxColorRange1);
            Image<Gray, byte> gray2 = img.InRange(PawnsInfo.minColorRange2, PawnsInfo.maxColorRange2);

            _pawnsPlayer1 = gray1.HoughCircles(new Gray(85), new Gray(30), 2, PawnsInfo.minDistance, PawnsInfo.minRadius, PawnsInfo.maxRadius)[0];
            _pawnsPlayer2 = gray2.HoughCircles(new Gray(85), new Gray(30), 2, PawnsInfo.minDistance, PawnsInfo.minRadius, PawnsInfo.maxRadius)[0];

            foreach (CircleF pawn in _pawnsPlayer1)
            {
                Point position = new Point((int)pawn.Center.X, (int)pawn.Center.Y);

                int search = _fields.FindIndex(field => (position.X >= field.leftUp.X && position.X <= field.rightUp.X) && (position.Y >= field.leftUp.Y && position.Y <= field.leftDown.Y));
                if (search >= 0) _fields[search] = new Field(_fields[search].column, _fields[search].row, 1, _fields[search].leftUp, _fields[search].rightUp, _fields[search].leftDown, _fields[search].rightDown);
            }

            foreach (CircleF pawn in _pawnsPlayer2)
            {
                Point position = new Point((int)pawn.Center.X, (int)pawn.Center.Y);

                int search = _fields.FindIndex(field => (position.X >= field.leftUp.X && position.X <= field.rightUp.X) && (position.Y >= field.leftUp.Y && position.Y <= field.leftDown.Y));
                if (search >= 0) _fields[search] = new Field(_fields[search].column, _fields[search].row, 2, _fields[search].leftUp, _fields[search].rightUp, _fields[search].leftDown, _fields[search].rightDown);
            }
            #endregion

            #region Wyświetlanie pól i pionów
            if (drawFields) CvInvoke.DrawChessboardCorners(img, patternSize, corners, found);

            if (drawPawnsPlayer1 && _pawnsPlayer1 != null)
                foreach (CircleF pawn in _pawnsPlayer1) img.Draw(pawn, new Bgr(0, 255, 0), 2);
            if (drawPawnsPlayer2 && _pawnsPlayer2 != null)
                foreach (CircleF pawn in _pawnsPlayer2) img.Draw(pawn, new Bgr(0, 255, 0), 2);
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
        public Chessboard(double minDistance, int minRadius, int maxRadius, int size = 10)
        {
            _size = size;
            _fields = new List<Field>();
            _moves = new List<string>();

            #region Uzupełnienie danych na temat pionów graczy
            PawnsInfo.minDistance = minDistance;
            PawnsInfo.minRadius = minRadius;
            PawnsInfo.maxRadius = maxRadius;
            #endregion
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
