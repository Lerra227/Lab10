namespace Lab10
{
    public class DatabaseConfig
    {
        private const string Dsn = "Host=localhost;Port=5432;Database=Lab10;Username=postgres;Password=labysql";

        public static string GetDsn()
        {
            return Dsn;
        }
    }
}
