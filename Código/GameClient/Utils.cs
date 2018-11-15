namespace GameClient
{
    public class Utils
    {
        public static string GetSeparator()
        {
            return "_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ ";
        }
        public static string ToLwr(string cmd)
        {
            return cmd.ToLower();
        }

        public static string GetClientAvailableCmds()
        {
            return "Available commands: <newplayer> - <connect> - <enter> - <exit>";
        }
    }
}
