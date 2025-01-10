namespace Plugins.CityCommon.Server
{
    public class GameServerConfig
    {
        public const float TIME_SCALE = 1;
        public const float AGENT_BASE_SPEED = 3;
        public const int MAX_AGENT_MEET_A_DAY = 3;
        
        public const string URL_GATE_DEV = "ws://localhost2:3014";
        public const string URL_GATE_LOCAL = "ws://127.0.0.1:3014";
        public const string URL_GATE = URL_GATE_DEV;
        public const string IMG_BASE_URL = "http://localhost2:3001/";
        public const string CHANNEL_CITY_SERVER = "CITY_SERVER";
        public const string CHANNEL_CITY = "CITY";

        public const string USER_NAME_GAME_SERVER = "_GameServer";
        public const string USER_NAME_LLM = "_LLMServer";

        public const string REDIS_IP = "localhost2";
        public const string REDIS_PORT = "6379";
        public const string MYSQL_IP = "localhost2";
        public const string MYSQL_PORT = "3306";
        public const string MYSQL_CONNECT_STR = "server=" + MYSQL_IP + ";database=game;username=dev;password=Civil_dev_123;";

        public const int CELL_SIZE_IN_PIXEL = 32;
        public const int SPEAK_DURATION_SECONDS_MAX = 3 * 60;
        public const int SPEAK_INTERVAL_MAX = 3;

        public const float mood_threshold = 0.8f;
        public const int DEFAULT_MOOD = 80;
        public const int AGENT_MAX_COUNT = 50;
    }
}