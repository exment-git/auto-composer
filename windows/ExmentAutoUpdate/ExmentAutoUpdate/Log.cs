using System;
using System.Collections.Generic;
using System.Text;

namespace AutoComposer
{
    class Log
    {
        public static void Debug(string log)
        {
            message("Debug", log);
        }

        public static void Error(string log)
        {
            message("Error", log);
        }

        public static void Error(Exception ex)
        {
            message("Error", ex.Message + ex.StackTrace);
        }

        private static void message(string level, string message)
        {
            Console.WriteLine(string.Format("{0}    {1}    {2}", DateTime.Now.ToString(), level, message));
        }
    }
}
