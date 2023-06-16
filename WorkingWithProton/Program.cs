using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Utilities;
using System.Text;
using Tuvi.Proton.Client;
using Tuvi.Proton.Primitive.Exceptions;
using TuviPgpLib.Entities;
using TuviPgpLib;
using TuviPgpLibImpl;
using Moq;
using MimeKit;
using static WorkingWithProton.Exceptions;
using MimeKit.IO;
using WorkingWithProton.Entities;

namespace WorkingWithProton
{
    internal class MyTuviContext : TuviPgpContext
    {
        public MyTuviContext(IKeyStorage storage) : base(storage)
        {
            passwordsDictionary = new Dictionary<long, string>();
        }

        public MyTuviContext(IKeyStorage storage, Dictionary<long, string> passwordsDictionary) : base(storage)
        {
            this.passwordsDictionary = passwordsDictionary;
        }

        private readonly Dictionary<long, string> passwordsDictionary;

        protected override string GetPasswordForKey(PgpSecretKey key)
        {
            return passwordsDictionary[key.KeyId];
        }

        public void AddKeyPassword(long keyId, string password)
        {
            passwordsDictionary.Add(keyId, password);
        }
    }

    internal class MockPgpKeyStorage
    {
        private PgpPublicKeyBundle? PublicKeyStorage;
        private PgpSecretKeyBundle? SecretKeyStorage;

        private readonly Mock<IKeyStorage> MockInstance;

        public MockPgpKeyStorage()
        {
            PublicKeyStorage = null;
            SecretKeyStorage = null;

            MockInstance = new Mock<IKeyStorage>();
            MockInstance.Setup(a => a.GetPgpPublicKeysAsync(default)).ReturnsAsync(PublicKeyStorage);
            MockInstance.Setup(a => a.GetPgpSecretKeysAsync(default)).ReturnsAsync(SecretKeyStorage);
            MockInstance.Setup(a => a.SavePgpPublicKeys(It.IsAny<PgpPublicKeyBundle>()))
                                       .Callback<PgpPublicKeyBundle>((bundle) => PublicKeyStorage = bundle);
            MockInstance.Setup(a => a.SavePgpSecretKeys(It.IsAny<PgpSecretKeyBundle>()))
                                       .Callback<PgpSecretKeyBundle>((bundle) => SecretKeyStorage = bundle);
        }

        public IKeyStorage Get()
        {
            return MockInstance.Object;
        }
    }

    public class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");
            
            var proton = new Session(
                httpClient: new HttpClient(),
                host: new Uri("https://mail-api.proton.me"))
            {
                AppVersion = "Other",
                RedirectUri = new Uri("https://protonmail.ch")
            };

            string username = "";
            string password = "";
            #region try
            try
            {
                // 0. Login
                await proton.LoginAsync(
                    username: username,
                    password: password);

                // 1. Request for user info from "/core/v4/users" API
                var userResponse = await proton.RequestAsync<UsersResponse>(
                    endpoint: new Uri("/core/v4/users", UriKind.Relative),
                    method: HttpMethod.Get,
                    headers: null) ?? throw new RequestException("User response is null.");

                // 2. Request for salts info
                var saltResponse = await proton.RequestAsync<SaltsResponse>(
                    endpoint: new Uri("/core/v4/keys/salts", UriKind.Relative),
                    method: HttpMethod.Get,
                    headers: null) ?? throw new RequestException("Salt response is null.");

                // 3. Request for address info (key for encrypting messages)
                var addressResponse = await proton.RequestAsync<AddressesResponse>(
                   endpoint: new Uri("/core/v4/addresses", UriKind.Relative),
                   method: HttpMethod.Get,
                   headers: null);

                #region Null checkings
                //if (saltResponse == null)
                //{
                //    throw new RequestException("Salt response is null.");
                //}

                if (saltResponse.KeySalts == null)
                {
                    throw new RequestException("KeySalts in salt response is null.");
                }

                //if (userResponse == null)
                //{
                //    throw new RequestException("User response is null.");
                //}

                if (userResponse.User == null)
                {
                    throw new RequestException("User in user response is null.");
                }

                if (userResponse.User.Value.Id == null)
                {
                    throw new RequestException("User Id in user response is null.");
                }

                if (userResponse.User.Value.Keys == null)
                {
                    throw new RequestException("User keys in user response are null.");
                }

                if (addressResponse == null)
                {
                    throw new RequestException("Address response is null.");
                }

                if (addressResponse.Addresses == null)
                {
                    throw new RequestException("Addresses in address response are null.");
                }

                if (addressResponse.Addresses.First() == null)
                {
                    throw new RequestException("Address keys in address response are null.");
                }

                if (addressResponse.Addresses.First().Keys == null)
                {
                    throw new RequestException("Address keys in address response are null.");
                }
                #endregion

                byte[] keyPass = Encoding.ASCII.GetBytes(password);
                // TODO: Two factor authentification. What the keyPass is?

                // 4. Get salted pass
                var saltedKeyPass = Calculations.SaltForKey(saltResponse.KeySalts.ToArray(), keyPass, 
                    userResponse.User.Value.Keys.First(key => key.IsActive).Id ?? throw new RequestException("User key id is null."));
                var saltedKeyPassAsString = Encoding.ASCII.GetString(saltedKeyPass);

                // Plain text and public key
                string text = "Plain text: some text some text some text";
                string pubKey = "-----BEGIN PGP PUBLIC KEY BLOCK-----\r\n\r\nxjMEZFELYBYJKwYBBAHaRw8BAQdAcMeIC1IYRPeW730xxpe9zaohU3xyEeva\r\nlhHX+v0QmoTNJ2d5bWJvdGFuQHByb3Rvbi5tZSA8Z3ltYm90YW5AcHJvdG9u\r\nLm1lPsKMBBAWCgA+BYJkUQtgBAsJBwgJkG7GajeMYonuAxUICgQWAAIBAhkB\r\nApsDAh4BFiEEcRIla6oSJW2LajAibsZqN4xiie4AAJFPAQCW7Q0s0Q0soqSj\r\nPitWXPy1jSpcNt0/aMiGyUUtn9jpYQD8Dk7JzY13jJbHHGMBQLLmetWqD9Gk\r\nFnxYBjmQxOO2FAvOOARkUQtgEgorBgEEAZdVAQUBAQdA4Z7wwSuIMYVlaL1S\r\nC43fR3hUnRbxkLn3JP9XXK44b00DAQgHwngEGBYIACoFgmRRC2AJkG7GajeM\r\nYonuApsMFiEEcRIla6oSJW2LajAibsZqN4xiie4AAGErAQDPf2AkFJ/27YQy\r\nmeMDRynTuB6URyhuNLvG9DzqwWyKTgD/WzHWJkAlCcbgI9NC8siNyWgQBh+/\r\nPdOiCdktIucnHw4=\r\n=d8Dl\r\n-----END PGP PUBLIC KEY BLOCK-----";
                await Console.Out.WriteLineAsync("Initial text: ");
                await Console.Out.WriteLineAsync(text);

                // Text encryption
                PgpPublicKeyRingBundle pubRings;
                MimePart signedMime;

                using Stream encryptedData = new MemoryStream();
                using (TuviPgpContext encryptContext = new TuviPgpContext(new MockPgpKeyStorage().Get()))
                {
                    var recipients = new List<MailboxAddress> { new MailboxAddress("gymbo", "gymbotan@proton.me") };

                    using (ArmoredInputStream keyIn = new ArmoredInputStream(
                        new MemoryStream(Encoding.ASCII.GetBytes(pubKey))))
                    {
                        pubRings = new PgpPublicKeyRingBundle(keyIn);
                    }

                    encryptContext.Import(pubRings); //Add public key to the context

                    using Stream inputData = new MemoryStream();

                    using var messageBody = new TextPart() { Text = text };
                    messageBody.WriteTo(inputData); // write text into a stream
                    inputData.Position = 0;

                    signedMime = encryptContext.Encrypt(recipients, inputData);
                    signedMime.WriteTo(encryptedData);
                    encryptedData.Position = 0;
                }

                await Console.Out.WriteLineAsync("\nEncrypted message: \n");
                await Console.Out.WriteLineAsync(signedMime.ToString());

                //PgpPublicKey keyForSigning;
                
                    // Decryption
                PgpSecretKeyRingBundle pgpRing;
                using (MyTuviContext context = new MyTuviContext(new MockPgpKeyStorage().Get()))
                {
                    // get user key from response
                    string userKey = userResponse.User.Value.Keys.First(key => key.IsActive).PrivateKey ?? throw new RequestException("User key is null."); 
                    using (ArmoredInputStream keyIn = new ArmoredInputStream(
                            new MemoryStream(Encoding.ASCII.GetBytes(userKey))))
                    {
                        pgpRing = new PgpSecretKeyRingBundle(keyIn);
                        var key = pgpRing.GetKeyRings().First().GetSecretKeys().First(key => key.IsSigningKey == false); // Find encripting key
                        context.AddKeyPassword(key.KeyId, Strings.FromByteArray(saltedKeyPass)); // Add password of this key to the context

                        var key2 = pgpRing.GetKeyRings().First().GetSecretKeys().First(key => key.IsSigningKey == true); // Find encripting key
                        //context.AddKeyPassword(key2.KeyId, Strings.FromByteArray(saltedKeyPass));
                        //keyForSigning = key2.PublicKey;
                        var publicSigningKeys = pgpRing.GetSecretKeyRing(key2.KeyId).GetPublicKeys();


                        // Adding public signing keys for checking signatures
                        using var str = new MemoryStream();
                        foreach (var pubSigKey in publicSigningKeys)
                        {
                            pubSigKey.Encode(str);
                        }
                        str.Position = 0;
                        var pkRing = new PgpPublicKeyRing(str);
                        context.Import(pkRing);
                    }

                    
                    //var a = new PgpPublicKeyRing()

                    context.Import(pgpRing);
                    context.Import(pubRings);

                    // Getting addrKey password from Token
                    string tokenText = addressResponse.Addresses.First().Keys?.First(key => key.IsActive).Token ?? throw new RequestException("Token is null."); 
                    // TODO: What to do if there are will be a lot of keys
                    // TODO: Check is key Active? Is it enough of checkings?

                    // Token is encrypted password for address key. So we forming encrypted message
                    //MimePart mp = new MimePart()
                    //{
                    //    ContentDisposition = new ContentDisposition("attachment"),
                    //    Content = new MimeContent(new MemoryStream(Encoding.ASCII.GetBytes(tokenText)))
                    //};

                    string addrKeyPassword;

                    using (MemoryStream stream = new MemoryStream(Encoding.ASCII.GetBytes(tokenText)))
                    {
                        //mp.WriteTo(stream);
                        //stream.Position = 0;
                        using var decryptedData = new MemoryBlockStream();
                        context.DecryptTo(stream, decryptedData);
                        decryptedData.Position = 0;
                        using StreamReader reader = new StreamReader(decryptedData);
                        addrKeyPassword = reader.ReadToEnd(); // Password for address key
                    }




                    string signature = addressResponse.Addresses.First().Keys?.First(key => key.IsActive).Signature ?? throw new RequestException("Signature is null.");
                    // TODO: What to do if there are will be a lot of keys
                    // TODO: Check is key Active? Is it enough of checkings?

                    //Another version of signature checking
                    using (var armored = new ArmoredInputStream(new MemoryStream(Encoding.ASCII.GetBytes(signature))))
                    {
                        PgpObjectFactory factory = new PgpObjectFactory(armored);

                        PgpSignature? sign = ((PgpSignatureList)factory.NextPgpObject())[0];
                        
                        var pubSignKey = context.EnumeratePublicKeys().First(key => key.KeyId == sign.KeyId);//.GetPublicKeys(recipients);
                        
                        sign.InitVerify(pubSignKey);
                        sign.Update(Encoding.ASCII.GetBytes(addrKeyPassword));
                        if (sign.Verify() != true)
                        {
                            throw new Exception("Token signature is incorrect.");
                        }
                    }



              //      // Token is encrypted password for address key. So we forming encrypted message
              //      MimePart mp2 = new MimePart()
              //      {
              //          ContentDisposition = new ContentDisposition("attachment"),
              //          Content = new MimeContent(new MemoryStream(Encoding.ASCII.GetBytes(signature)))
              //      };

              //      //string addrKeyPassword;

              //      using (MemoryStream stream = new MemoryStream())
              //      {
              //          using Stream inputData = new MemoryStream();
              //          //using Stream encryptedData = new MemoryStream();
              //          using var messageBody = new TextPart() { Text = addrKeyPassword };
              //          messageBody.WriteTo(inputData);
              //          inputData.Position = 0;
              //          //pgpRing.GetSecretKey()
              ////          var sigMime = context.Sign(pgpRing.GetSecretKey(5138869812945562109), DigestAlgorithm.Sha512, inputData);


              //          mp2.WriteTo(stream);
              //          stream.Position = 0;
              //          //using var decryptedData = new MemoryBlockStream();
              //          //context.DecryptTo(stream, decryptedData);
              //          //decryptedData.Position = 0;
              //          //using StreamReader reader = new StreamReader(decryptedData);
              //          //addrKeyPassword = reader.ReadToEnd(); // Password for address key


              //          var signatures = context.Verify(inputData, stream);

              //          foreach (IDigitalSignature sig in signatures)
              //          {
              //              var res = sig.Verify();
              //          }
              //      }




                    // Add addrKey and addrKeyPassword to context 
                    string addrKey = addressResponse.Addresses.First().Keys?.First(key => key.IsActive).PrivateKey ?? throw new RequestException("Address key is null.");
                    using (ArmoredInputStream keyIn = new ArmoredInputStream(
                            new MemoryStream(Encoding.ASCII.GetBytes(addrKey))))
                    {
                        pgpRing = new PgpSecretKeyRingBundle(keyIn);
                        var key = pgpRing.GetKeyRings().First().GetSecretKeys().First(key => key.IsSigningKey == false); // Find encripting key
                        context.AddKeyPassword(key.KeyId, addrKeyPassword); // Add password of this key to the context
                    }

                    context.Import(pgpRing);

                    // Message decrypting
                    var mime = context.Decrypt(encryptedData);
                    var decryptedBody = mime as TextPart;
                    var result = decryptedBody?.Text;

                    await Console.Out.WriteLineAsync("\nDecrypted text:");
                    await Console.Out.WriteLineAsync(result);

                    // TODO: Signatures checking
                }
            }
            catch (ProtonRequestException ex)
            {
                Console.WriteLine($"""
                ProtonRequestException:

                Proton Code: {ex.ErrorInfo.Code};
                Proton Error: {ex.ErrorInfo.Error};

                HttpStatusCode: {ex.HttpStatusCode};
                InnerException: {ex.InnerException};
                """);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            #endregion

            await proton.LogoutAsync();
        }
    }
}