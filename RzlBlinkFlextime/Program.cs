using NLog;
using System;
using System.Drawing;

namespace RzlBlinkFlextime
{
    class Program
    {
        #region Member

        private Logger log;
        private IniFileHandler settings;
        private BlinkOneHandler blinkOneHandler;
        private Flextime flextime;
        private DateTime lastSet;

        #endregion

        static void Main(string[] args)
        {
            Program p = new Program();
            p.Init();

            Console.WriteLine("Drücke ESC um abzubrechen");

            do
            {
                p.CheckFlextime();
                System.Threading.Thread.Sleep(5000);

                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape) break;
                }
            } while (true);

            p.WriteInfoMessage("Programm beendet");
            p.blinkOneHandler.Dispose();
        }

        private void Init()
        {
            log = NLog.LogManager.GetCurrentClassLogger();

            System.IO.FileInfo fi = new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location);
            string settingsPath = System.IO.Path.Combine(fi.DirectoryName, "Settings.ini");
            settings = new IniFileHandler(settingsPath);
            //If the field FirstRunThisDay is empty, write the current DateTime.
            if (string.IsNullOrEmpty(settings.getValue("Genaral", "FirstRunThisDay")))
            {
                settings.setValue("Genaral", "FirstRunThisDay", DateTime.Now.ToString());
                settings.Save();
            }

            blinkOneHandler = new BlinkOneHandler();
            WriteInfoMessage(string.Format("RZL-Blink mit ID {0} gefunden.", blinkOneHandler.ID));

            flextime = new Flextime(
                int.Parse(settings.getValue("Flextime", "MinutesToWork")),
                int.Parse(settings.getValue("Flextime", "Offset")),
                int.Parse(settings.getValue("Flextime", "DaylyGrow")));

            lastSet = DateTime.Parse(settings.getValue("Genaral", "FirstRunThisDay"));
        }

        private void CheckFlextime()
        {
            //Check if the last set Date is today
            if (lastSet.Year == DateTime.Now.Year &&
                lastSet.Month == DateTime.Now.Month &&
                lastSet.Day == DateTime.Now.Day)
            {
                if (!flextime.TimeToGo(lastSet)) blinkOneHandler.SetColorTo(Color.Firebrick);

                if (flextime.TimeToGo(lastSet) && !flextime.ExtraTimeReached(lastSet))
                {
                    blinkOneHandler.SetColorTo(Color.GreenYellow);
                    WriteInfoMessage("Kannst gehen, Pflicht erfüllt");
                }

                if (flextime.ExtraTimeReached(lastSet))
                {
                    blinkOneHandler.SetColorTo(Color.ForestGreen);
                    WriteInfoMessage(@"\o/ Yeah! Du hast deine Wunschzeit für heute aufgebaut!");
                }
            }
            else
            {
                //if not, set the new date.
                settings.setValue("Genaral", "FirstRunThisDay", DateTime.Now.ToString());
                lastSet = DateTime.Now;
                settings.Save();
                //A new work day. Set the color to red.
                blinkOneHandler.SetColorTo(Color.Firebrick);
                WriteInfoMessage("Moin! Schönen Arbeitstag.");
                WriteInfoMessage(string.Format("Du bist um {0} gekommen und kannst um {1} gehen", lastSet.ToString("HH:mm:ss"), flextime.EndTime(lastSet)));
            }
        }

        private string lastInfoMessage;
        private void WriteInfoMessage(string Message)
        {
            if (!Message.Equals(lastInfoMessage))
            {
                lastInfoMessage = Message;
                Console.WriteLine(Message);
                log.Info(Message);
            }
        }

    }
}