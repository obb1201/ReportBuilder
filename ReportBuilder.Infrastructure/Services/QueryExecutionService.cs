using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ReportBuilder.Infrastructure.Services;

/// <summary>
/// Executes queries and returns results
/// </summary>
public class QueryExecutionService
{
    private readonly string _connectionString;
    private readonly SoqlToSqlConverter _soqlConverter;
    private readonly ILogger<QueryExecutionService> _logger;

    public QueryExecutionService(
        IConfiguration configuration,
        SoqlToSqlConverter soqlConverter,
        ILogger<QueryExecutionService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
        _soqlConverter = soqlConverter;
        _logger = logger;
    }

    /// <summary>
    /// Execute SOQL query and return results
    /// </summary>
    public async Task<QueryResult> ExecuteSoqlQueryAsync(string soqlQuery)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Validate SOQL
            if (!_soqlConverter.ValidateSoqlQuery(soqlQuery, out var errorMessage))
            {
                return QueryResult.CreateError(errorMessage);
            }
            
            // Convert to SQL
            var sqlQuery = _soqlConverter.ConvertToSql(soqlQuery);
            
            // Execute query
            var dataTable = await ExecuteSqlQueryAsync(sqlQuery);
            
            stopwatch.Stop();
            
            // Log execution
            await LogQueryExecutionAsync(soqlQuery, sqlQuery, stopwatch.ElapsedMilliseconds, dataTable.Rows.Count, true, null);
            
            return QueryResult.CreateSuccess(dataTable, sqlQuery, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error executing SOQL query: {Query}", soqlQuery);
            
            await LogQueryExecutionAsync(soqlQuery, string.Empty, stopwatch.ElapsedMilliseconds, 0, false, ex.Message);
            
            return QueryResult.CreateError(ex.Message);
        }
    }

    /// <summary>
    /// Execute SQL query and return DataTable
    /// </summary>
    private async Task<DataTable> ExecuteSqlQueryAsync(string sqlQuery)
    {
        var dataTable = new DataTable();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        using var command = new SqlCommand(sqlQuery, connection);
        command.CommandTimeout = 30; // 30 seconds timeout
        
        using var adapter = new SqlDataAdapter(command);
        await Task.Run(() => adapter.Fill(dataTable));
        
        return dataTable;
    }

    /// <summary>
    /// Log query execution
    /// </summary>
    private async Task LogQueryExecutionAsync(string soqlQuery, string sqlQuery, long executionTimeMs, int recordCount, bool success, string? errorMessage)
    {
        try
        {
            var sql = @"
                INSERT INTO QueryExecutionLog (SoqlQuery, SqlQuery, ExecutionTimeMs, RecordCount, ExecutedDate, Success, ErrorMessage)
                VALUES (@SoqlQuery, @SqlQuery, @ExecutionTimeMs, @RecordCount, @ExecutedDate, @Success, @ErrorMessage)";
            
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@SoqlQuery", soqlQuery);
            command.Parameters.AddWithValue("@SqlQuery", sqlQuery);
            command.Parameters.AddWithValue("@ExecutionTimeMs", executionTimeMs);
            command.Parameters.AddWithValue("@RecordCount", recordCount);
            command.Parameters.AddWithValue("@ExecutedDate", DateTime.Now);
            command.Parameters.AddWithValue("@Success", success);
            command.Parameters.AddWithValue("@ErrorMessage", (object?)errorMessage ?? DBNull.Value);
            
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging query execution");
        }
    }

    /// <summary>
    /// Get query execution history
    /// </summary>
    public async Task<List<QueryExecutionHistory>> GetQueryHistoryAsync(int top = 20)
    {
        var history = new List<QueryExecutionHistory>();
        
        var sql = $@"
            SELECT TOP ({top}) Id, SoqlQuery, SqlQuery, ExecutionTimeMs, RecordCount, ExecutedDate, Success, ErrorMessage
            FROM QueryExecutionLog
            ORDER BY ExecutedDate DESC";
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        using var command = new SqlCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            history.Add(new QueryExecutionHistory
            {
                Id = reader.GetInt32(0),
                SoqlQuery = reader.GetString(1),
                SqlQuery = reader.GetString(2),
                ExecutionTimeMs = reader.GetInt32(3),
                RecordCount = reader.GetInt32(4),
                ExecutedDate = reader.GetDateTime(5),
                Success = reader.GetBoolean(6),
                ErrorMessage = reader.IsDBNull(7) ? null : reader.GetString(7)
            });
        }
        
        return history;
    }
}

/// <summary>
/// Query execution result
/// </summary>
public class QueryResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DataTable? Data { get; set; }
    public string? SqlQuery { get; set; }
    public long ExecutionTimeMs { get; set; }
    public int RecordCount => Data?.Rows.Count ?? 0;

    public static QueryResult CreateSuccess(DataTable data, string sqlQuery, long executionTimeMs)
    {
        return new QueryResult
        {
            Success = true,
            Data = data,
            SqlQuery = sqlQuery,
            ExecutionTimeMs = executionTimeMs
        };
    }

    public static QueryResult CreateError(string errorMessage)
    {
        return new QueryResult
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Query execution history record
/// </summary>
public class QueryExecutionHistory
{
    public int Id { get; set; }
    public string SoqlQuery { get; set; } = string.Empty;
    public string SqlQuery { get; set; } = string.Empty;
    public int ExecutionTimeMs { get; set; }
    public int RecordCount { get; set; }
    public DateTime ExecutedDate { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}
