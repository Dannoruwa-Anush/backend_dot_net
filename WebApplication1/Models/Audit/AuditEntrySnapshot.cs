using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace WebApplication1.Models.Audit
{
    internal sealed class AuditEntrySnapshot
    {
        public string Action { get; }
        public string EntityName { get; }
        public int? EntityId { get; private set; }
        public string Changes { get; }

        private readonly EntityEntry _entry;

        public AuditEntrySnapshot(EntityEntry entry)
        {
            _entry = entry;
            Action = entry.State.ToString();
            EntityName = entry.Entity.GetType().Name;
            Changes = SerializeChanges(entry);
        }

        public void ResolvePrimaryKey()
        {
            var key = _entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            EntityId = key?.CurrentValue as int?;
        }

        private static string SerializeChanges(EntityEntry entry)
        {
            var changes = new Dictionary<string, object?>();

            foreach (var prop in entry.Properties)
            {
                var name = prop.Metadata.Name;

                if (SensitiveFields.Contains(name, StringComparer.OrdinalIgnoreCase))
                {
                    changes[name] = "REDACTED";
                    continue;
                }

                if (entry.State == EntityState.Added)
                    changes[name] = prop.CurrentValue;

                else if (entry.State == EntityState.Modified && prop.IsModified)
                    changes[name] = new { Old = prop.OriginalValue, New = prop.CurrentValue };

                else if (entry.State == EntityState.Deleted)
                    changes[name] = prop.OriginalValue;
            }

            return JsonSerializer.Serialize(changes);
        }

        private static readonly string[] SensitiveFields =
        {
            "Password",
            "PasswordHash",
            "Token",
            "RefreshToken",
            "SecretKey"
        };
    }
}