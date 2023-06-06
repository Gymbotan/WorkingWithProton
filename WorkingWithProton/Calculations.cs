using Org.BouncyCastle.Crypto.Generators;
using System.Text;
using Org.BouncyCastle.Math;
using WorkingWithProton.Entities;
using Org.BouncyCastle.Utilities.Encoders;
using static WorkingWithProton.Exceptions;

namespace WorkingWithProton
{
    public class Calculations
    {
        private const int cost = 10; // it shows how many reps(cycles) bcrypt will do during calculations (2^cost times)

        public static byte[] SaltForKey(Salt[] salts, byte[] keyPass, string id)
        {            
            Salt keySalt = salts.First(salt => salt.Id == id);
            if (keySalt.KeySalt == null)
            {
                throw new RequestException("Key salt is null here.");
            }
            return SaltForKey(keySalt.KeySalt, keyPass);
        }

        public static byte[] SaltForKey(string salt, byte[] keyPass)
        {
            byte[] prefix = new byte[] { 36, 50, 121, 36, 49, 48, 36 }; // "$2y$10$"

            var decodedSalt = Base64.Decode(salt);
            var base64Salt = ProtonBase64Lib.ProtonBase64.ToBase64String(decodedSalt); // Salt into ProtonBase64

            var hashedPassword = MailboxPassword(decodedSalt, keyPass); // Using bcrypt to calculate hash

            var base64HashedPassword = ProtonBase64Lib.ProtonBase64.ToBase64String(hashedPassword[..^1]); // Hashed password without last byte into ProtonBase64

            var result = prefix.Concat(Encoding.ASCII.GetBytes(base64Salt)).
                Concat(Encoding.ASCII.GetBytes(base64HashedPassword)).ToArray();

            // Cut off last 31 bytes
            return result[(result.Length - 31)..];
        }

        public static byte[] MailboxPassword(byte[] salt, byte[] password)
        {
            var newPassword = password.Concat(new byte[] { 0 }).ToArray(); // Function HashBytes, file https://github.com/ProtonMail/bcrypt/blob/master/bcrypt.go, row 173
            var hashedPassword = BCrypt.Generate(newPassword, salt, cost);
            return hashedPassword;
        }
    }
}
