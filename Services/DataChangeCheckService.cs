using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace bikey.Services
{
    public interface IDataChangeCheckService
    {
        string GenerateChecksum<T>(IEnumerable<T> data) where T : class;
        string GenerateChecksum(object data);
    }

    public class DataChangeCheckService : IDataChangeCheckService
    {
        public string GenerateChecksum<T>(IEnumerable<T> data) where T : class
        {
            try
            {
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                return GenerateHash(json);
            }
            catch
            {
                return Guid.NewGuid().ToString();
            }
        }

        public string GenerateChecksum(object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                return GenerateHash(json);
            }
            catch
            {
                return Guid.NewGuid().ToString();
            }
        }

        private string GenerateHash(string data)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
