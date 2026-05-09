namespace NetSend.Core.Config;

public static class EnvConfig
{
    public static string LocalDatabaseBasePath { get; } = Environment.GetEnvironmentVariable("NETSEND_LOCAL_DB_ROOT")
                                                          ?? throw new NullReferenceException(
                                                              "Set NETSEND_LOCAL_DB_ROOT environment variable");
}