using System;
using System.IO;

namespace delGame
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try//обработка исключения, если файла с сохраненной игрой не открывается
            {
                var Save = File.ReadAllLines("Save");
                for (var x = 0; x < Save.Length; x++)
                {
                    if (x < 8)
                    {
                        for (var y = 0; y < 8; y++)
                        {
                            var h = Save[x].Split();
                            Game.MapForLoad[x, y] = byte.Parse(Save[x].Split(',')[y]);
                        }
                    }
                    else if (x == 8)
                    {
                        if (Save[x].Length != 1)
                            throw new Exception("Файл сохранения повреждён");
                        GameScript.Agres = byte.Parse(Save[x]);
                    }
                    else
                    {
                        GameScript.Log.Add(Save[x]);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());//вывод на консоль ошибки в считывания файла
            }
            new Game(840, 640).DrawUpdate();
        }
    }
}
