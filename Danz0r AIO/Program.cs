
using EnsoulSharp;
using EnsoulSharp.SDK;
using Danz0r_AIO.Champions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Danz0r_AIO
{
    
     class Program
    {
        static void Main(string[] args)
        {
            GameEvent.OnGameLoad += On_LoadGame;
        }
        private static void On_LoadGame()
        {

            if (ObjectManager.Player.CharacterName == ObjectManager.Player.CharacterName)
            {
                Morgana.OnLoad();
                Chat.PrintChat("<font color=\"#e02db4\">[</font><font color=\"#e67d1c\">Morgana</font><font color=\"#e02db4\">]</font><font color=\"#e02db4\"> - <u>The Best Morgan Script</u>  </font>- Danz0r");
            }
        }
    }
}
