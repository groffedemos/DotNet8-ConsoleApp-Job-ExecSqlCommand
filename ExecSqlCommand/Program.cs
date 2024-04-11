using Microsoft.Data.SqlClient;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

const string APPLICATION_ERROR = "Aplicacao finalizada com erros.";

var logger = new LoggerConfiguration()
    .WriteTo.Console(theme: AnsiConsoleTheme.Literate)
    .CreateLogger();
logger.Information("Execucao de comando em uma base SQL Server...");

var connectionString = Environment.GetEnvironmentVariable("ConnectionString");
if (string.IsNullOrWhiteSpace(connectionString))
{
    logger.Error("Variavel de ambiente 'ConnectionString' nao foi definida.");
    logger.Error(APPLICATION_ERROR);
    Environment.ExitCode = 2;
    return Environment.ExitCode;
}

var sqlCommand = Environment.GetEnvironmentVariable("SqlCommand");
if (string.IsNullOrWhiteSpace(sqlCommand))
{
    logger.Error("Variavel de ambiente 'SqlCommand' nao foi definida.");
    logger.Error(APPLICATION_ERROR);
    Environment.ExitCode = 3;
    return Environment.ExitCode;
}

var commandTimeoutValue = Environment.GetEnvironmentVariable("CommandTimeout");
if (!int.TryParse(commandTimeoutValue, out var commandTimeout))
{
    logger.Error("Variavel de ambiente 'CommandTimeout' nao foi definida ou nao e um numero inteiro.");
    logger.Error(APPLICATION_ERROR);
    Environment.ExitCode = 4;
    return Environment.ExitCode;
}

try
{
    using var connection = new SqlConnection(connectionString);
    await connection.OpenAsync();
    logger.Information("Conexao realizada com sucesso!");

    var command = connection.CreateCommand();
    command.CommandText = sqlCommand;
    command.CommandTimeout = commandTimeout;
    logger.Information($"Iniciando a execucao do comando: {sqlCommand}");
    var result = await command.ExecuteNonQueryAsync();
    logger.Information($"No. de rows afetadas: {result}");
    
    await connection.CloseAsync();
    logger.Information("Conexao fechada com sucesso!");
    logger.Information("Aplicacao executada com sucesso!");
}
catch (Exception ex)
{
    logger.Error($"Erro: {ex.Message} | {ex.GetType().FullName}");
    if (Environment.ExitCode == 0)
        Environment.ExitCode = 1; // Erro generico
   logger.Error(APPLICATION_ERROR);
}

return Environment.ExitCode;