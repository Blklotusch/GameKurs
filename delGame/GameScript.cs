using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace delGame
{
    class GameScript
    {
        public static bool Load = true;
        public bool Run = true;
        public static bool IsEnd = false;
        public static byte[,] MapCords = new byte[8, 8];
        public static byte[,] MapCordstmp = new byte[8, 8];
        public static bool NowIsPlayer = true;
        static List<Vector2i> blacks = new List<Vector2i>();
        static List<Vector2i> white = new List<Vector2i>();
        List<Vector2i> KingWhite = new List<Vector2i>();
        List<Vector2i> KingBlacks = new List<Vector2i>();

        public GameScript(RenderWindow window, byte[,] _LoadMap)
        {
            MapCords = _LoadMap;//загрузка карты
            window.MouseButtonPressed += Control;
            window.MouseWheelScrolled += Scroll;

            SelectedWhite = new Vector2i(-1, -1);

            ListsUpdate();

            Anim = new Thread(AnimThread);
            Anim.IsBackground = true;
            Anim.Start();

            StartTimer = DateTime.Now;
            NowIsPlayer = true;
            if (!File.Exists("Log.txt"))
                File.WriteAllText("Log.txt", null);
            StartLog = 1;
            Load = false;
        }//Иницилизация

        #region Отрисовка
        static Sprite MapSprite = new Sprite(new Texture("Map.png"));
        static CircleShape Cir = new CircleShape(37.5f);
        static CircleShape KingCir = new CircleShape(18) { FillColor = Color.Yellow };
        static RectangleShape Rectangle = new RectangleShape(new Vector2f(-75, -75)) { FillColor = new Color(67, 129, 186, 130) };
        static RectangleShape MBRect = new RectangleShape(Rectangle) { FillColor = new Color(255, 0, 0, 220) };
        static RectangleShape LogRect = new RectangleShape(new Vector2f(180, 440)) { Position = new Vector2f(650, 150), FillColor = new Color(84, 84, 84) };
        static RectangleShape White = new RectangleShape(new Vector2f(840, 640)) { FillColor = new Color(84, 84, 84, 180) };
        static Text text = new Text("", new Font("Athelas-Regular.ttf")) { Position = new Vector2f(660, 50), FillColor = Color.Black, CharacterSize = 44 };
        static Text LogText = new Text("", new Font("ofont.ru_Times New Roman.ttf"));
        static Text EndText = new Text(LogText) { Position = new Vector2f(230, 190), CharacterSize = 70 };
        static DateTime StartTimer;
        static Vector2f AnimPos = new Vector2f(-75, -75);
        static byte TypeAnim = 1;
        static int StartLog = 1;
        public static void Draw(RenderWindow window)//реализация отрисовки игры
        {
            window.Draw(MapSprite);

            for (int x = 0; x < 8; x++)//Отрисовка шашек на поле
            {
                for (int y = 0; y < 8; y++)
                {
                    if (MapCords[x, y] == 0)
                        continue;

                    Cir.FillColor = (MapCords[x, y] % 2 != 0) ? new Color(190, 190, 190) : new Color(128, 64, 48);//Выбор цвета шашки
                    Cir.Position = new Vector2f(75 * x, 75 * y);//Расчёт координаты в окне
                    window.Draw(Cir);

                    if (MapCords[x, y] - 2 > 0)//Отрисока короны если шашка это дамка 
                    {
                        KingCir.Position = new Vector2f(75 * x + (75 / 2 - 18), 75 * y + (75 / 2 - 18));
                        window.Draw(KingCir);
                    }
                }
            }

            if (SelectedWhite != new Vector2i(-1, -1) && !IsAnimated)//Отрисока того куда можно походить игроку
            {
                window.Draw(Rectangle);
                if (CanEat(SelectedWhite, out var Maby))
                {
                    foreach (var pos in Maby)
                    {
                        MBRect.Position = ((Vector2f)pos + new Vector2f(1, 1)) * 75;
                        window.Draw(MBRect);
                    }
                }
                else if (CanMove(SelectedWhite, out var Maby2))
                {
                    foreach (var pos in Maby2)
                    {
                        MBRect.Position = ((Vector2f)pos + new Vector2f(1, 1)) * 75;
                        window.Draw(MBRect);
                    }
                }
            }

            if (AnimPos != new Vector2f(-75, -75))//Отрисовка анимации передвижения шашки
            {
                Cir.FillColor = (TypeAnim % 2 != 0) ? new Color(190, 190, 190) : new Color(128, 64, 48);
                Cir.Position = AnimPos;
                window.Draw(Cir);
                if (TypeAnim - 2 > 0)
                {
                    KingCir.Position = new Vector2f(AnimPos.X + (75 / 2 - 18), AnimPos.Y + (75 / 2 - 18));
                    window.Draw(KingCir);
                }
            }

            if (!IsEnd)//Таймер
            {
                string buf = (DateTime.Now - StartTimer).ToString();
                text.DisplayedString = buf.Remove(buf.LastIndexOf('.'));
            }
            window.Draw(text);

            window.Draw(LogRect);//Фон для логов
            for (int i = StartLog; i < Log.Count + 1 && i < 13 + StartLog; i++)//Сами логи
            {
                LogText.DisplayedString = Log[Log.Count - i];
                LogText.Position = LogRect.Position + new Vector2f(5, (i - StartLog) * 30);
                window.Draw(LogText);
            }

            if (IsEnd)//Отображение кто победил
            {
                window.Draw(White);
                EndText.DisplayedString = white.Count < blacks.Count ? "Вы победили" : "Вы проиграли";
                window.Draw(EndText);
                LogText.Position = EndText.Position + new Vector2f(130, 130);
                LogText.DisplayedString = "Нажмите Enter";
                window.Draw(LogText);
            }
        }
        #endregion

        #region Логика игры
        public static int Agres = 0;//Уровень агресии
        static Random rnd = new Random();
        public static List<string> Log = new List<string>();
        public void Update(RenderWindow window)//часть кода определяющая логику игры
        {
            if (IsAnimated)
                return;

            if (white.Count == 0 || blacks.Count == 0)//Конец игры
            {
                StopThread = true;
                IsEnd = true;
                File.AppendAllText("Log.txt", (white.Count < blacks.Count ? "Победил игрок!" : "Победил компьютер!") + "\nВремя игры: " + (DateTime.Now - StartTimer).ToString() + "\n\n\n");
                Log.Clear();
                window.MouseButtonPressed -= Control;//убираем контроль с карты если игра закончена
                window.MouseWheelScrolled -= Scroll;
                while (!Keyboard.IsKeyPressed(Keyboard.Key.Enter)){}
                File.Delete("Save");
                IsEnd = false;
                Run = false;
                return;
            }

            if (NowIsPlayer)
                return; 

            ListsUpdate();//Обновление списков шашек которые могут ходить

            if (blacks.Any(CanEat))//Если шашка может есть
            {
                foreach (var black in blacks)
                {
                    List<Vector2i> Mabys;
                    if (!CanEat(black, out Mabys))
                        continue;

                    MoveOrEat(black, (Mabys.Count > 1) ? Mabys[rnd.Next(0, Mabys.Count - 1)] : Mabys[0]);
                    return;
                }
            }
            else if (!blacks.Any(CanMove))
                return;

            int i = 0;
            MapCordstmp = (byte[,])MapCords.Clone();
            Priora = new Dictionary<int, List<Vector2i>>();

            Filtr(blacks, i, MapCordstmp, new List<Vector2i>());//Рекурсия расчёта наиболее выгодных ходов

            switch (Agres)//Сложность
            {
                case 2://Сложная
                    MoveOrEat(Priora[Priora.Keys.Min()][0], Priora[Priora.Keys.Min()][1]);
                    break;
                case 1://нормальная
                    if (rnd.Next(2) == 0)
                        MoveOrEat(Priora[Priora.Keys.Min()][0], Priora[Priora.Keys.Min()][1]);
                    else
                    {
                        var op = rnd.Next(Priora.Count);
                        MoveOrEat(Priora.ElementAt(op).Value[0], Priora.ElementAt(op).Value[1]);
                    }
                    break;
                case 0://Лёгкая
                    var op2 = blacks[rnd.Next(blacks.Count)];
                    CanMove(op2, out var Maby);
                    MoveOrEat(op2, Maby[rnd.Next(Maby.Count)]);
                    break;
            }
        }
        #endregion

        #region Логика игрока
        static Vector2i SelectedWhite = new Vector2i(-1, -1);//Шашка выбраная игроком
        void Control(object o, EventArgs e)//Управление игрока
        {

            if (!NowIsPlayer || IsAnimated || IsEnd)
                return;

            var MouseArgs = (MouseButtonEventArgs)e;
            if (MouseArgs.Button != Mouse.Button.Left)
                return;

            Vector2i SelectedPos = new Vector2i(MouseArgs.X, MouseArgs.Y) / 75;//Получаем шашку на которую нажал игрок

            if (SelectedPos.X > 7 || SelectedPos.Y > 7)//Чтобы работало только в пределах поля
                return;

            if (SelectedPos == SelectedWhite)
                return;

            if (MapCords[SelectedPos.X, SelectedPos.Y] % 2 != 0)//Если шашка белого цвета
            {
                if (white.Any(CanEat) && !CanEat(SelectedPos))//Если кто-то может есть а белая шашка не может
                {
                    SelectedWhite = new Vector2i(-1, -1);
                    return;
                }
                SelectedWhite = SelectedPos;
                return;
            }


            if (SelectedWhite != new Vector2i(-1, -1))
            {
                if (CanEat(SelectedWhite, out var Maby2) && !Maby2.Contains(SelectedPos))//Если выбраная шашка может есть но игрок нажал не туда
                    return;
                else if ((!CanMove(SelectedWhite) || (CanMove(SelectedWhite, out var MabyM) && !MabyM.Contains(SelectedPos))) && !CanEat(SelectedWhite))//Если шашка не может есть или игрок нажал не туда
                    return;

                MoveOrEat(SelectedWhite, SelectedPos);
            }
        }

        void Scroll(object o, EventArgs e)//Скролинг логов
        {

            int Delta = -(int)((MouseWheelScrollEventArgs)e).Delta;

            if (Delta > 0 && Log.Count - (StartLog + Delta) > 11 )
            {
                StartLog += Delta;
            }
            else if (Delta < 0 && StartLog + Delta >= 1)
            {
                StartLog += Delta;
            }
        }
        #endregion

        #region Анимация
        Thread Anim;//Объект потока анимации
        sbyte AnimSel = -1;
        bool StopThread = false;
        void AnimThread()//Анимация мигания
        {
            while (true)
            {
                if (StopThread)//Остановка
                {
                    AnimSel = -1;
                    StopThread = false;
                    return;
                }

                if (Rectangle.FillColor.A == 130 && AnimSel != -1)
                    AnimSel = -1;
                else if (Rectangle.FillColor.A == 0 && AnimSel != 1)
                    AnimSel = 1;

                Rectangle.Position = ((Vector2f)SelectedWhite + new Vector2f(1, 1)) * 75;
                Rectangle.FillColor = new Color(Rectangle.FillColor) { A = (byte)(Rectangle.FillColor.A + AnimSel) };
                Thread.Sleep(2);
            }
        }
        #endregion

        #region ИИ фильтр
        public static Dictionary<int, List<Vector2i>> Priora = new Dictionary<int, List<Vector2i>>();
        const int Max = 5;
        
        static void Filtr(List<Vector2i> blacks, int i , byte[,] Map,List<Vector2i>Steps, byte Type = 2)///рекурсия для выбора ходов компьютера
        {
            i++;
            if (i > Max)
            {
                if (!Priora.ContainsKey(i + 2))
                    Priora.Add(i + 2, new List<Vector2i>(Steps));//Если не может ходить понижаем приоритет
                return;
            }
            foreach (var black in blacks)//Берём одну из чёрных
            {
                Steps.Add(black);
                if (i == 1)//Сохраняем тип
                    Type = MapCords[black.X, black.Y];
                else//моделируем шаг
                {   
                    if (blacks.IndexOf(black) != 0)
                        Map[blacks[blacks.IndexOf(black) - 1].X, blacks[blacks.IndexOf(black) - 1].Y] = 0;
                    Map[black.X, black.Y] = Type;
                }
                if (i!=1&&CanIDie(black, Type, Map))//Если может умереть
                {
                    if (!Priora.ContainsKey(i))
                        Priora.Add(i, new List<Vector2i>(Steps));
                    Steps.RemoveAt(Steps.LastIndexOf(black));
                    return;
                }
                else
                {
                    if(CanMoveForEnemy(black, Type, out var Maby))//Если может ходить
                    {   
                        if (Maby.Contains(black))
                            Maby.Remove(black);
                        Map[black.X, black.Y] = 0;
                        Filtr(Maby, i, (byte[,])Map.Clone(), Steps, Type);//Рекурсия
                        Map[black.X, black.Y] = Type;
                    }
                    else
                    {
                        if (!Priora.ContainsKey(i + 2)) 
                            Priora.Add(i + 2, new List<Vector2i>(Steps));//Если не может ходить понижаем приоритет
                    }                    
                }
                Steps.RemoveAt(Steps.LastIndexOf(black));
            }
        }
        #endregion

        #region Helpers

        #region CanIDie(И все её перегрузки)
        static bool CanIDie(Vector2i position)
        {
            return CanIDie(position, MapCords[position.X, position.Y], MapCordstmp);
        }
        static bool CanIDie(Vector2i position, int Type, byte[,] Map)//функция проверяет может ли шашка съесть
        {
            if (Type == 0)
                throw new Exception("Это пустая клетка");

            for (int x = -1; x < 2; x += 2)//Проверка вокруг одной шашки
            {
                for (int y = -1; y < 2; y += 2)
                {
                    try
                    {
                        if ((Map[position.X + x, position.Y + y] != Type && Map[position.X + x, position.Y + y] != 0)
                        && Map[position.X - x, position.Y - y] == 0)
                            return true;
                    }
                    catch { }
                }
            }
            return false;
        }
        #endregion

        #region CanMove(И все её перегрузки)
        static bool CanMove(Vector2i position)
        {
            return CanMove(position, out var lol);
        }
        static bool CanMove(Vector2i position, out List<Vector2i> MabyStep)
        {
            return CanMove(position, MapCords[position.X, position.Y], out MabyStep);
        }
        static bool CanMove(Vector2i position, int Type)
        {
            return CanMove(position, MapCords[position.X, position.Y], out var Maby);
        }
        static bool CanMove(Vector2i position, int Type, out List<Vector2i> MabyStep)//функция проверяет может ли шашка двигаться
        {
            if (Type == 0)
                throw new Exception("Это пустая клетка");

            MabyStep = new List<Vector2i>();

            for (int x = -1; x < 2; x += 2)//Проверка вокруг одной шашки
            {
                if (Type / 3 == 1)//Если это дамка
                {
                    for (int y = -1; y < 2; y += 2)
                    {
                        try
                        {
                            if (MapCords[position.X + x, position.Y + y] == 0)
                                MabyStep.Add(position + new Vector2i(x, y));
                        }
                        catch { }
                    }
                    continue;
                }

                try
                {
                    if (MapCords[position.X + x, position.Y + ( Type == 2 ? 1 : -1)] == 0)
                        MabyStep.Add(position + new Vector2i(x, Type == 2 ? 1 : -1));
                }
                catch { }
            }

            if (MabyStep.Count != 0)
                return true;
            return false;
        }
        static bool CanMoveForEnemy(Vector2i position, int Type, out List<Vector2i> MabyStep)//функция проверяет может ли шашка двигаться
        {
            if (Type == 0)
                throw new Exception("Это пустая клетка");

            MabyStep = new List<Vector2i>();

            for (int x = -1; x < 2; x += 2)
            {
                if (Type / 3 == 1)
                {
                    for (int y = -1; y < 2; y += 2)
                    {
                        try
                        {
                            if (MapCordstmp[position.X + x, position.Y + y] == 0)
                                MabyStep.Add(position + new Vector2i(x, y));
                        }
                        catch { }
                    }
                    continue;
                }

                try
                {
                    if (MapCordstmp[position.X + x, position.Y + (Type == 2 ? 1 : -1)] == 0)
                        MabyStep.Add(position + new Vector2i(x, Type == 2 ? 1 : -1));
                }
                catch { }
            }

            if (MabyStep.Count != 0)
                return true;
            return false;
        }
        #endregion

        #region CanEat(И все её перегрузки)
        static bool CanEat(Vector2i position)
        {
            return CanEat(position, out var list);
        }
        static bool CanEat(Vector2i position, out List<Vector2i> MabyStep)
        {
            return CanEat(position, MapCords[position.X, position.Y], out MabyStep);
        }
        static bool CanEat(Vector2i position, int Type)
        {
            return CanEat(position, Type, out var Maby);
        }
        static bool CanEat(Vector2i position, int Type, out List<Vector2i> MabyStep)//функция проверяет может ли шашка съесть
        {
            if (Type == 0)
                throw new Exception($"Это пустая клетка ({position})");

            MabyStep = new List<Vector2i>();

            for (int x = -1; x < 2; x += 2)//Проверка вокруг одной шашки
            {
                for (int y = -1; y < 2; y += 2)
                {
                    try
                    {
                        if ((MapCords[position.X + x, position.Y + y] % 2 != Type % 2 && MapCords[position.X + x, position.Y + y] != 0) &&
                        (MapCords[position.X + x * 2, position.Y + y * 2] == 0))
                            MabyStep.Add(new Vector2i(position.X + x * 2, position.Y + y * 2));
                    }
                    catch { }
                }
            }
            if (MabyStep.Count != 0)
                return true;
            return false;
        }
        #endregion

        #region ListUpdate
        void ListsUpdate()
        {
            var StartTimer = DateTime.Now;
            blacks.Clear();
            white.Clear();
            for (int x = 0; x < 8; x++)//Проверяет по X
            {
                XCheck(x);
            }
        }
        void XCheck(int x)
        {
            for (int y = (x % 2 == 0 ? 1 : 0); y < 8; y += 2)
            {
                if (MapCords[x, y] == 0)
                    continue;

                YCheck(x, y);
            }
        }
        void YCheck(int x, int y)
        {
            if (!CanEat(new Vector2i(x, y)) && !CanMove(new Vector2i(x, y)))
                return;

            (MapCords[x, y] % 2 != 0 ? white : blacks).Add(new Vector2i(x, y));
        }
        #endregion

        #region MoveOrEat
        public static bool IsAnimated = false;
        void MoveOrEat(Vector2i Start, Vector2i End)// Функция реализующая движение и поедание шашек
        {
            if (MapCords[Start.X, Start.Y] == 0)
                throw new Exception("Это пустая клетка");

            if (MapCords[End.X, End.Y] != 0)
                throw new Exception("Невозможно перейти на занятую клетку");

            Thread Anim2 = new Thread(() => MoveOrEatAnim(Start, End, (Vector2f)(End - Start), MapCords[Start.X, Start.Y]));//Создаём поток
            IsAnimated = true;
            Anim2.Start();
        }
        Dictionary<int, char> Alph = new Dictionary<int, char>() { {0, 'A'}, {1, 'B'}, { 2, 'C' }, { 3, 'D' }, { 4, 'E' }, { 5, 'F' }, { 6, 'G' }, { 7, 'H' } };
        async void MoveOrEatAnim(Vector2i Start, Vector2i End, Vector2f Delta, byte Type)
        {
            Log.Add(Alph[Start.X].ToString() + (8 - Start.Y ).ToString() + ":" + Alph[End.X].ToString() + (8 - End.Y).ToString());//Запись в логи
            File.AppendAllText("Log.txt", Alph[Start.X] + (9 - Start.Y + 1) + ":" + Alph[End.X] + (9 - End.Y + 1) + "\n");//Запись в файл

            if (Type % 2 != 0)//Если это белая сбрасываем выбраную шашку, чтобы избежать ошибок
                SelectedWhite = new Vector2i(-1, -1);
            MapCords[Start.X, Start.Y] = 0;
            (Type % 2 == 0 ? blacks : white).Remove(Start);
            if (Type - 2 > 0)
                (Type % 2 == 0 ? KingBlacks : KingWhite).Remove(Start);

            TypeAnim = Type;
            bool Eating = false;
            for (float i = 0; i <= 1; i += 0.05f)
            {
                AnimPos = ((Vector2f)Start + (Delta * i)) * 75;

                if (i > 0.5f && (Start.X + End.X) % 2 == 0 && (Start.Y + End.Y) % 2 == 0 && MapCords[(Start.X + End.X) / 2, (Start.Y + End.Y) / 2] != 0)//Если между началом и концом есть шашка противоположного цвета
                {
                    if (MapCords[(Start.X + End.X) / 2, (Start.Y + End.Y) / 2] - 2 > 0)
                        (MapCords[(Start.X + End.X) / 2, (Start.Y + End.Y) / 2] - 2 == 2 ? KingBlacks : KingWhite).Remove((Start + End) / 2);

                    (MapCords[(Start.X + End.X) / 2, (Start.Y + End.Y) / 2] % 2 == 0 ? blacks : white).Remove((Start + End) / 2);
                    MapCords[(Start.X + End.X) / 2, (Start.Y + End.Y) / 2] = 0;
                    Eating = true;
                }
                Thread.Sleep(10);
            }

            AnimPos = new Vector2f(-75, -75);

            if (End.Y == (Type % 2 == 0 ? 7 : 0) && !(Type - 2 == 2 ? KingBlacks : KingWhite).Contains(End))//Если шашка дошла до противоположного конца карты и не является дамкой
            {
                (Type - 2 == 2 ? KingBlacks : KingWhite).Add(End);
                MapCords[End.X, End.Y] = (byte)(Type + 2);
            }
            else
                MapCords[End.X, End.Y] = Type;

            if (Type % 2 == 0)
                await Task.Run(() => Save(MapCords));//запуск асинхронного метода для записи игры 

            (Type % 2 == 0 ? blacks : white).Add(End);
            if (Type - 2 > 0) 
                (Type % 2 == 0 ? KingBlacks : KingWhite).Add(End);

            if (CanEat(End) && Eating)//Если шашка только что ела и всё ещё может есть, то мы не меняем сторону которая ходит ход
            {
                if(Type % 2 != 0)
                    SelectedWhite = End;
                ListsUpdate();
            }
            else
                NowIsPlayer = !NowIsPlayer;
            IsAnimated = false;
        }
        #endregion

        static async void Save(byte[,] Map)//асинхронный метод сохранения карты и логов на случай преждевременного закрытия
        {
            await File.WriteAllTextAsync("Save", null);
            for (var x = 0; x < 8; x++)
            {
                for (var y = 0; y < 8; y++)
                {
                    await File.AppendAllTextAsync("Save", Map[x, y].ToString() + (y == 7 ? "" : ","));
                }
                await File.AppendAllTextAsync("Save", x == 7 ? "\n" + Agres + "\n" : "\n");
            }
            foreach (var l in Log)
            {
                await File.AppendAllTextAsync("Save", l + (Log.IndexOf(l) == Log.Count - 1 ? "" : "\n"));
            }
            Console.WriteLine("Файл сохранён");
        }
        #endregion
    }
}

