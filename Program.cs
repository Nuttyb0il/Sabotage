using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sabotage
{
    class Program
    {
        static int mapLength = 0;
        static long pauseTimestamp = 0;
        static long startTimestamp = 0;
        static bool paused = false;
        static Random rng = new Random();
        static void Main(string[] args)
        {
            Console.Title = "Sabotage";
            Console.WriteLine("Sabotage is ready, just start a map and play !");
            while (true)
            {
                Console.Title = "Sabotage - Waiting";
                mapLength = 0;
                pauseTimestamp = 0;
                startTimestamp = -1;
                paused = false;
                while (IsPlaying() != "")
                {
                    Console.Title = "Sabotage - Playing";
                    if(mapLength == 0)
                    {
                        startTimestamp = GetTimestamp();
                        mapLength = GetMapLength(IsPlaying());
                    }
                    if (mapLength != 0 && pauseTimestamp == 0) // map length has been acquired and no pause timestamps have been generated
                    {
                        pauseTimestamp = rng.Next(6000, mapLength); // generate a random timestamp between 3s and map length
                    }
                    while (UpdateClock() < pauseTimestamp) // while current map clock not pauseTimestamp
                    {
                        Console.WriteLine(pauseTimestamp.ToString() + " - " + UpdateClock());
                        if(IsPlaying() == "") // continues to check if player is playing
                        {
                            break;
                        }
                    } 
                    if(!paused && IsPlaying() != "")
                    {
                        Console.Beep();
                        SendKeys.SendWait("{ESCAPE}");
                        paused = true;
                    }
                    
                }
            }
        }
        /// <summary>
        /// This function tries to find if the user is playing
        /// </summary>
        /// <returns>Difficulty path</returns>
        static string IsPlaying()
        {
            string osuPath = $"C:\\Users\\{Environment.UserName}\\AppData\\Local\\osu!\\Songs\\";
            try
            {
                Process p = Process.GetProcessesByName("osu!")[0]; // get first occurence of osu
                string title = p.MainWindowTitle; // get window title
                if (title.Contains("-")) // Title of the window changes to osu! - <name of the map> + difficulty i guess ?
                {
                    string mapName = title.Split(new[] { "osu!  - " }, StringSplitOptions.None)[1].Split(new[] { " [" }, StringSplitOptions.None)[0]; // this should work, probably not the best solution
                    string diff_name = title.Split('[').Last().Replace("]", ""); // this should work, probably not the best solution
                    foreach (string song in Directory.EnumerateDirectories(osuPath))
                    {
                        if (song.Contains(mapName))
                        {
                            foreach (string difficulty in Directory.GetFiles(song))
                            {
                                if(difficulty.Contains(diff_name))
                                {
                                    return difficulty;
                                }
                            }
                        }
                    }
                }
                return "";
            }
            catch (Exception) {return "";}
        }
        /// <summary>
        /// This functions tries to get a map's length
        /// </summary>
        /// <returns>Last note timestamp</returns>
        static int GetMapLength(string path)
        {
            try
            { 
                string[] lines = File.ReadAllLines(path);
                // we're not gonna use the very last HitObject for map length so it won't pause at last circle or slider or whatever
                return Convert.ToInt32(lines[lines.Length - 10].Split(',')[2]); // timing is always 3rd value
            }
            catch (Exception) { return 0; }
            
        }
        /// <summary>
        /// I'm not using any ReadMemory to avoid bans
        /// </summary>
        /// <returns>Current MS since map started</returns>
        static long UpdateClock()
        {
            return GetTimestamp() - startTimestamp;
        }
        static long GetTimestamp()
        {
            return new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
        }
    }
}
