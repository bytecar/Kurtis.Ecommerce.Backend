using System.Security.Cryptography;
namespace ClientSecretKeyGenerator
{
    public class GenerateClientID
    {
        public static void Main(string[] args)
        {
            // Generate 32 random bytes (256 bits)
            byte[] keyBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(keyBytes);
            }

            // Convert to Base64 string
            string base64Key = Convert.ToBase64String(keyBytes);

            Console.WriteLine("Generated 256-bit Base64 key:");
            Console.WriteLine(base64Key);
            Console.ReadKey();
        }
    }
}