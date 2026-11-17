using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace HR.Tests;

public sealed class SqlServerFixture : IAsyncLifetime
{
    public bool IsAvailable { get; private set; }
    public string? SkipReason { get; private set; }
    public string PersonnelConnection { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        var configured = Environment.GetEnvironmentVariable("HR_TEST_SQL");
        if (string.IsNullOrWhiteSpace(configured))
        {
            IsAvailable = false;
            SkipReason = "متغیر HR_TEST_SQL تنظیم نشده؛ آزمون پایگاه رد می‌شود.";
            return;
        }

        var masterBuilder = new SqlConnectionStringBuilder(configured) { InitialCatalog = "master" };
        var master = masterBuilder.ConnectionString;

        try
        {
            await WaitForServerAsync(master);
            await using var connection = new SqlConnection(master);
            await connection.OpenAsync();
            await ExecuteAsync(connection, """
                IF DB_ID(N'Personnel') IS NOT NULL
                BEGIN
                    ALTER DATABASE Personnel SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE Personnel;
                END
                """);
            await ExecuteAsync(connection, "CREATE DATABASE Personnel;");

            var script = File.ReadAllText(FindScript());
            var batches = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            foreach (var batch in batches)
            {
                var sql = batch.Trim();
                if (sql.Length == 0 || ShouldSkip(sql)) continue;
                await ExecuteAsync(connection, sql);
            }

            IsAvailable = true;
            PersonnelConnection = new SqlConnectionStringBuilder(configured)
            {
                InitialCatalog = "Personnel",
                MultipleActiveResultSets = true
            }.ConnectionString;
        }
        catch (Exception ex)
        {
            IsAvailable = false;
            SkipReason = "SQL Server در دسترس نیست یا اسکریپت اجرا نشد: " + ex.GetBaseException().Message;
        }
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private static async Task WaitForServerAsync(string connectionString)
    {
        Exception? last = null;
        for (var attempt = 0; attempt < 30; attempt++)
        {
            try
            {
                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();
                return;
            }
            catch (Exception ex) when (ex is not TypeInitializationException)
            {
                last = ex;
                await Task.Delay(2000);
            }
        }

        throw last ?? new InvalidOperationException("اتصال به SQL Server برقرار نشد.");
    }

    private static async Task ExecuteAsync(SqlConnection connection, string sql)
    {
        await using var command = connection.CreateCommand();
        command.CommandTimeout = 120;
        command.CommandText = sql;
        try
        {
            await command.ExecuteNonQueryAsync();
        }
        catch (SqlException ex)
        {
            var preview = sql.Length > 300 ? sql[..300] : sql;
            throw new InvalidOperationException(ex.Message + "\n---\n" + preview, ex);
        }
    }

    private static bool ShouldSkip(string sql)
    {
        if (sql.Contains("CREATE DATABASE", StringComparison.OrdinalIgnoreCase)) return true;
        if (sql.Contains("ALTER DATABASE", StringComparison.OrdinalIgnoreCase)) return true;
        if (sql.Contains("sp_fulltext_database", StringComparison.OrdinalIgnoreCase)) return true;
        if (sql.Contains("sp_db_vardecimal_storage_format", StringComparison.OrdinalIgnoreCase)) return true;
        if (sql.Contains("QUERY_STORE", StringComparison.OrdinalIgnoreCase)) return true;
        return false;
    }

    private static string FindScript()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "HR", "DB", "LastPersonnel.sql");
            if (File.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }

        throw new FileNotFoundException("LastPersonnel.sql پیدا نشد.");
    }
}
