using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Runtime.Serialization;

namespace delGame
{
    class MenuScript
    {

        static readonly Sprite StartButton = new Sprite(new Texture("StartBtn.png"))
        { Position = new Vector2f(Game.window.Size.X / 2 - new Texture("StartBtn.png").Size.X / 2, Game.window.Size.Y / 1.5f) };

        static readonly Text MainText = new Text("Поддавки", new Font("ofont.ru_Times New Roman.ttf"))
        { Position = new Vector2f(Game.window.Size.X / 2 - 200, Game.window.Size.Y / 8), CharacterSize = 100, Color = Color.Black };

        public static void Draw(RenderWindow window)
        {
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
            if (Game.Screen != 0)
                return;

            var MouseArgs = (MouseButtonEventArgs)e;
            if (MouseArgs.Button != Mouse.Button.Left)
                return;

            if (StartButton.GetGlobalBounds().Contains(MouseArgs.X, MouseArgs.Y))
                Run = false;
        }
    }
}
