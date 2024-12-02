namespace AnkiNet.Infrastructure.Persistence.AnkiFile.Database.Model;

internal record grave(
    long usn,
    long oid,
    long type
);