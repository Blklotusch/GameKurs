using SFML.Graphics;
using SFML.Window;
using System;
using System.Threading;

namespace delGame
{
    class Game
    {
        public static RenderWindow window;
        public static byte[,] MapForLoad = new byte[8, 8];

        public static byte Screen = 0;
        static bool IsRun = true;
        Thread UpdateThread;
        public Game(uint _width, uint _height, uint FPS = 60)
        {
            window = new RenderWindow(new VideoMode(_width, _height), "Поддавки", Styles.Close);
            window.SetFramerateLimit(FPS);
            Initialization();
        }
        private void WhenClose(object o, EventArgs e)
        {
            IsRun = false;
            window.Close();
        }


        static void Update()
        {
            while (IsRun)
            {
                switch (Screen)
                {
                    case 0:
                        var _MenuScript = new MenuScript(window);
                        while (_MenuScript.Run) { }
                        break;
                    case 1:
                        var _LevelScript = new LevelsScript(window);
                        while (_LevelScript.Run) { }
                        Screen = 2;
                        break;
                    case 2:
                        GameScript.Load = true;
                        var _GameScript = new GameScript(window, MapForLoad);
                        while (_GameScript.Run)
                        {
                            _GameScript.Update(window);
                        }
                        Screen = 0;
                        break;
                }
            }
        }

        public void DrawUpdate()
        {
            while (IsRun)
            {
                window.DispatchEvents();

                if (!window.HasFocus())
                    continue;

                switch (Screen)
                {
                    case 0:
                        MenuScript.Draw(window);
                        break;
                    case 1:
                        LevelsScript.Draw(window);
                        break;
                    case 2:
                        while (GameScript.Load) { }
                        GameScript.Draw(window);
                        break;
                }

                window.Display();
                window.Clear(Color.White);
            }
        }

        private void Initialization()
        {
            window.Closed += WhenClose;
            UpdateThread = new Thread(Update);
            UpdateThread.IsBackground = true;
            UpdateThread.Start();
        }
    }
}
