using System;

namespace ElevensXNA
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Elevens game = new Elevens())
            {
                game.Run();
            }
        }
    }
#endif
}

