using System.Data;
using Microsoft.EntityFrameworkCore;

namespace IntegracionNiubizDemo.Persistence.Data;

/// <summary>
/// Aplica cambios de esquema idempotentes sin depender del flujo completo de EF Core Migrations
/// (este proyecto no usa `dotnet ef migrations`). Si la base de datos SQLite ya existe (por ejemplo
/// el <c>app.db</c> de desarrollo con datos previos), agrega únicamente las columnas nuevas que falten,
/// preservando los datos ya almacenados. Si la base de datos no existe, <see cref="Database.EnsureCreatedAsync"/>
/// la crea directamente con el esquema completo (incluyendo las columnas nuevas) y no hay nada más que hacer.
/// </summary>
public static class SchemaUpgrader
{
    private static readonly (string Column, string SqlType)[] OrderAddressColumns =
    {
        ("CustomerAddress", "TEXT"),
        ("CustomerCity", "TEXT"),
        ("CustomerState", "TEXT"),
        ("CustomerCountry", "TEXT"),
        ("CustomerPostalCode", "TEXT"),
        ("CustomerPhone", "TEXT"),
    };

    public static async Task EnsureOrderAddressColumnsAsync(AppDbContext db, CancellationToken ct = default)
    {
        await db.Database.EnsureCreatedAsync(ct);

        var connection = db.Database.GetDbConnection();
        var openedHere = connection.State != ConnectionState.Open;
        if (openedHere) await connection.OpenAsync(ct);
        try
        {
            var existingColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var pragmaCmd = connection.CreateCommand())
            {
                pragmaCmd.CommandText = "PRAGMA table_info(Orders);";
                using var reader = await pragmaCmd.ExecuteReaderAsync(ct);
                var nameOrdinal = -1;
                while (await reader.ReadAsync(ct))
                {
                    if (nameOrdinal < 0) nameOrdinal = reader.GetOrdinal("name");
                    existingColumns.Add(reader.GetString(nameOrdinal));
                }
            }

            foreach (var (column, sqlType) in OrderAddressColumns)
            {
                if (existingColumns.Contains(column)) continue;

                using var alterCmd = connection.CreateCommand();
                alterCmd.CommandText = $"ALTER TABLE Orders ADD COLUMN {column} {sqlType} NULL;";
                await alterCmd.ExecuteNonQueryAsync(ct);
            }
        }
        finally
        {
            if (openedHere) await connection.CloseAsync();
        }
    }
}
