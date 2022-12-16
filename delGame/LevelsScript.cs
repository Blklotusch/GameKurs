using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;

namespace delGame
{
    class LevelsScript
    {
        static RectangleShape Rect = new RectangleShape(new Vector2f(300, 80))
        { FillColor = Color.Red, Position = new Vector2f(Game.window.Size.X / 2 - 300 / 2, Game.window.Size.Y * 0.75f) };

        static RectangleShape Rect2 = new RectangleShape(new Vector2f(300, 80))
        { FillColor = Color.Red, Position = new Vector2f(Game.window.Size.X / 2 - 300 / 2, Game.window.Size.Y * 0.5f) };

        static RectangleShape Rect3 = new RectangleShape(new Vector2f(300, 80))
        { FillColor = Color.Red, Position = new Vector2f(Game.window.Size.X / 2 - 300 / 2, Game.window.Size.Y * 0.25f) };

        static Text MainText = new Text("", new Font("Athelas-Regular.ttf"));
        static RectangleShape[] rectangles = new RectangleShape[3] {Rect3, Rect2, Rect};

        public static void Draw(RenderWindow window)
        {
            MainText.Color = Color.Black;

            MainText.DisplayedString = "Сложно"; 
            MainText.Position = Rect.Position + new Vector2f(80, 13);
            MainText.CharacterSize = 40;
            window.Draw(Rect);
            window.Draw(MainText);

            MainText.DisplayedString = "Нормально";
            MainText.Position = Rect2.Position + new Vector2f(48, 13);
            MainText.CharacterSize = 40;
            window.Draw(Rect2);
            window.Draw(MainText);

            MainText.DisplayedString = "Легко";
            MainText.Position = Rect3.Position + new Vector2f(100, 13);
            MainText.CharacterSize = 40;
            window.Draw(Rect3);
            window.Draw(MainText);

            MainText.DisplayedString = "Сложность";
            MainText.Position = new Vector2f(Game.window.Size.X / 2 - 160, Game.window.Size.Y * 0.05f);
            MainText.CharacterSize = 70;
            window.Draw(MainText);
        }

        public bool Run = true;
        public LevelsScript(RenderWindow window)
        {
            window.MouseButtonPressed += Control;
        }

        void Control(object o, EventArgs e)
        {
            var MouseArgs = (MouseButtonEventArgs)e;
            if (MouseArgs.Button != Mouse.Button.Left)
                return;

            for (int i = 0; i < rectangles.Length; i++)
            {
                if (rectangles[i].GetGlobalBounds().Contains(MouseArgs.X, MouseArgs.Y))//если выбираешь новую игру загружается карта
                {
                    GameScript.Agres = i;
                    Game.MapForLoad = new byte[8, 8]
                    {
                        {0, 2, 0, 0, 0, 1, 0, 1},
                        {2, 0, 2, 0, 0, 0, 1, 0},
                        {0, 2, 0, 0, 0, 1, 0, 1},
                        {2, 0, 2, 0, 0, 0, 1, 0},
                        {0, 2, 0, 0, 0, 1, 0, 1},
                        {2, 0, 2, 0, 0, 0, 1, 0},
                        {0, 2, 0, 0, 0, 1, 0, 1},
                        {2, 0, 2, 0, 0, 0, 1, 0}
                    };
                    Run = false;
                    ((RenderWindow)o).MouseButtonPressed -= Control;//удаляется контроль других экранов
                }
            }
        }
    }
}
