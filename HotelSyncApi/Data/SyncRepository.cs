using Microsoft.Data.SqlClient;

namespace HotelSyncApi.Data;

public class SyncRepository
{
    private readonly string _connectionString;

    public SyncRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<int> CreateSyncRecordAsync(string operaResId, string hotelCode)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            INSERT INTO SyncRecords (OperaResId, HotelCode, SyncStatus, LastAttempt)
            VALUES (@OperaResId, @HotelCode, 'PENDING', GETUTCDATE());
            SELECT SCOPE_IDENTITY();";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@OperaResId", operaResId);
        command.Parameters.AddWithValue("@HotelCode", hotelCode);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    public async Task UpdateSyncStatusAsync(int id, string status, string? errorMessage = null)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            UPDATE SyncRecords 
            SET SyncStatus = @Status, 
                ErrorMessage = @ErrorMessage,
                UpdatedAt = GETUTCDATE()
            WHERE Id = @Id";

        using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Status", status);
        command.Parameters.AddWithValue("@ErrorMessage", (object?)errorMessage ?? DBNull.Value);
        command.Parameters.AddWithValue("@Id", id);

        await command.ExecuteNonQueryAsync();
    }
}