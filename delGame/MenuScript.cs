using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.IO;

namespace delGame
{
    class MenuScript
    {

        static readonly Sprite StartButton = new Sprite(new Texture("StartBtn.png"))
        { Position = new Vector2f(Game.window.Size.X / 2 - new Texture("StartBtn.png").Size.X / 2, Game.window.Size.Y / 1.5f) };

        static readonly Text MainText = new Text("Поддавки", new Font("ofont.ru_Times New Roman.ttf"))
        { Position = new Vector2f(Game.window.Size.X / 2 - 200, Game.window.Size.Y / 8), CharacterSize = 100, Color = Color.Black };
        static readonly RectangleShape LoadBtn = new RectangleShape((Vector2f)StartButton.Texture.Size / 2)
        { Position = new Vector2f(Game.window.Size.X / 2 - 90, Game.window.Size.Y / 2), FillColor = Color.Red };

        static readonly Text LoadText = new Text("Продолжить", new Font("Athelas-Regular.ttf"))
        { Position = LoadBtn.Position + new Vector2f(27, 15), CharacterSize = 20 };
        public static void Draw(RenderWindow window)
        {
            if (File.Exists("Save"))//если файл существует, появляется кнопка продолжить
            {
                window.Draw(LoadBtn);
                window.Draw(LoadText);
            }
            window.Draw(MainText);
            window.Draw(StartButton);
        }

        public bool Run = true;
        public MenuScript(RenderWindow window)
        {
            window.MouseButtonPressed += Control;
        }

        void Control(object o, EventArgs e)
        {

            var MouseArgs = (MouseButtonEventArgs)e;
            if (MouseArgs.Button != Mouse.Button.Left)
                return;

            if (StartButton.GetGlobalBounds().Contains(MouseArgs.X, MouseArgs.Y))
            {
                File.Delete("Save");//удаляется файл сохранения если нажали старт, когда есть кнопка продолжить
                Game.Screen = 1;
                Run = false;
                ((RenderWindow)o).MouseButtonPressed -= Control;
            }
            else if (File.Exists("Save") && LoadBtn.GetGlobalBounds().Contains(MouseArgs.X, MouseArgs.Y))
            {
                Game.Screen = 2;//если загрузка сразу на карту
                Run = false;
                ((RenderWindow)o).MouseButtonPressed -= Control;//при выходе с экрана убирает контроль данного экрана
            }
        }
    }
}
