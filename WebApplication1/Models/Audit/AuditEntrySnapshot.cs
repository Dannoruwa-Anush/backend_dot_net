using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace WebApplication1.Models.Audit
{
    internal sealed class AuditEntrySnapshot
    {
        public EntityEntry Entry { get; }
        public string Action { get; }
        public string EntityName { get; }
        public int? EntityId { get; private set; }
        public string Changes => JsonSerializer.Serialize(GetChangedProperties());

        public AuditEntrySnapshot(EntityEntry entry)
        {
            Entry = entry;
            Action = entry.State.ToString();
            EntityName = entry.Entity.GetType().Name;
        }

        public void ResolvePrimaryKey()
        {
            var key = Entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
            EntityId = key?.CurrentValue as int?;
        }

        private Dictionary<string, object?> GetChangedProperties()
        {
            var changes = new Dictionary<string, object?>();

            foreach (var prop in Entry.Properties)
            {
                var name = prop.Metadata.Name;

                if (SensitiveFields.Contains(name, StringComparer.OrdinalIgnoreCase))
                {
                    changes[name] = Entry.State == EntityState.Modified
                        ? new { Old = "REDACTED", New = "REDACTED" }
                        : "REDACTED";
                    continue;
                }

                if (Entry.State == EntityState.Added)
                {
                    changes[name] = prop.CurrentValue;
                }
                else if (Entry.State == EntityState.Modified && prop.IsModified)
                {
                    changes[name] = new { Old = prop.OriginalValue, New = prop.CurrentValue };
                }
                else if (Entry.State == EntityState.Deleted)
                {
                    changes[name] = prop.OriginalValue;
                }
            }

            return changes;
        }

        private static readonly string[] SensitiveFields = new[]
        {
            "Password", "PasswordHash", "Token", "RefreshToken", "SecretKey"
        };
    }
}