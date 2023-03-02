using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using StackExchange.Redis;

public sealed class RedisConnection
{
    private static Lazy<RedisConnection> lazy = new Lazy<RedisConnection>(() =>
    {
        if (config == null) throw new InvalidOperationException("Please call Init() first.");
        return new RedisConnection();
    });

    private static ConfigurationOptions config { get; set; }

    public readonly ConnectionMultiplexer ConnectionMultiplexer;

    public static RedisConnection Instance
    {
        get
        {
            return lazy.Value;
        }
    }

    private RedisConnection()
    {
        ConnectionMultiplexer = ConnectionMultiplexer.Connect(config);
    }

    public static void Init()
    {
        string IP = Environment.GetEnvironmentVariable("RedisIP");
        int Port = Convert.ToInt32(Environment.GetEnvironmentVariable("RedisPort"));
        config = new ConfigurationOptions
        {
            KeepAlive = 0,
            AllowAdmin = true,
            EndPoints = { $"{IP}:{Port}" },
            ConnectTimeout = 5000,
            ConnectRetry = 5,
            SyncTimeout = 5000,
            AbortOnConnectFail = false,
            Ssl = true,
            SslHost = IP,
            Password = Environment.GetEnvironmentVariable("RedisAuth"),
        };
        string path = Environment.GetEnvironmentVariable("RedisCA");
        config.CertificateSelection += delegate
        {
            return new X509Certificate2(path);
        };
        config.CertificateValidation += CheckServerCertificate;
        // 在 CommandMap 中將 SUBSCRIBE 設定為 false
        config.CommandMap = CommandMap.Create(new HashSet<string> { "SUBSCRIBE" }, false);
    }

    private static bool CheckServerCertificate(object sender, X509Certificate certificate,
        X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return true; // trust server
    }
}

public class RedisModel
{
    RedisConnection connection = RedisConnection.Instance;

    public async void setKey(string key, string value)
    {
        var redis = RedisConnection.Instance.ConnectionMultiplexer;
        var database = redis.GetDatabase();
        await database.StringSetAsync(key, value);
    }

    public async Task<string> getKey(string key)
    {
        var redis = RedisConnection.Instance.ConnectionMultiplexer;
        var database = redis.GetDatabase();
        return await database.StringGetAsync(key);
    }
}