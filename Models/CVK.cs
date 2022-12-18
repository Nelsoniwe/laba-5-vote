using laba3_vote.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace laba3_vote.Models
{
    public class CVK
    {
        UnicodeEncoding byteConverter = new UnicodeEncoding();
        public List<Person> People { get; set; } = new List<Person>();
        private static RSACryptoServiceProvider rsa;

        private List<Vote> Votes { get; set; } = new List<Vote>();

        public List<BulletinPart> BulletinDividers { get; set; } = new List<BulletinPart>();
        public List<BulletinPart> BulletinApplicants { get; set; } = new List<BulletinPart>();

        private string personsPath;
        private string votesPath;
        private string bulletinDividersPath;
        private string bulletinApplicantsPath;

        public CVK()
        {
            personsPath = @"Persons.json";
            votesPath = @"Votes.json";
            bulletinDividersPath = @"BulletinDividers.json";
            bulletinApplicantsPath = @"BulletinApplicants.json";
        }

        public RSAParameters GetPublicKey()
        {
            rsa = new RSACryptoServiceProvider(512);
            rsa.FromXmlString(@"<RSAKeyValue>
  <Modulus>zg9hDWCkWigxa8X8rJBl5iTjfYE5gAdf2NvSBoM8pyVMCUsrI02KhV8W/cj7r8lXkLMVx6lH93Wr3JKFZUbsAQ==</Modulus>
  <Exponent>AQAB</Exponent>
  <P>5xKWUkv3ePTr7opGVYJ4P51Tfbd4Lrnf8Pg8NDUHM1c=</P>
  <Q>5EoIpAUV4mHhHeFsW43/XkUsOg7dnAn9s48VIlfCXGc=</Q>
  <DP>NpDZFo4B3npXzHiyqzaoFr2cHa/ZnY8fJtQ3w0xSavk=</DP>
  <DQ>nIV+IycxeAPwG1Khvqw/ON1ok23517Cp9+DUdrWBF2U=</DQ>
  <InverseQ>PjqGl8vNmitqoL/+nPoUxxkay7pLLqt8qyI/Ff/WTG4=</InverseQ>
  <D>Lam0zR0cbqo3gXWHb8oz+pM0ImzPjDKWJ91WpoDQoQ06B4nFzs3QZtaQJx1vfezDY/oYG90fgp5vCZJ/ShRFrQ==</D>
</RSAKeyValue>");

            return rsa.ExportParameters(false);
        }

        public List<Result> GetResults()
        {
            if (BulletinApplicants.Count != BulletinApplicants.Count)
            {
                Console.WriteLine("Count of bulletins is wrong");
            }

            var results = new List<Result>();

            foreach (var person in People)
            {
                var bulletinDivider = BulletinDividers.FirstOrDefault(x => x.Id == person.Id);
                var bulletinApplicant = BulletinApplicants.FirstOrDefault(x => x.Id == person.Id);

                if (bulletinDivider == null || bulletinApplicant == null)
                {
                    continue;
                }

                var message = Encoding.UTF8.GetString(Decrypt(bulletinApplicant.bulletine));
                var message2 = Encoding.UTF8.GetString(Decrypt(bulletinDivider.bulletine));

                var applicantId = Convert.ToInt32(message) / Convert.ToInt32(message2);
                var applicant = People.FirstOrDefault(x => x.Id == applicantId.ToString());
                results.Add(new Result { voterId = person.Id ,Id = applicant.Id, Name = applicant.Name, Surname = applicant.Surname });
            }

            return results;
        }

        public static byte[] Decrypt(byte[] cipher)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(512);
            rsa.FromXmlString(@"<RSAKeyValue>
  <Modulus>zg9hDWCkWigxa8X8rJBl5iTjfYE5gAdf2NvSBoM8pyVMCUsrI02KhV8W/cj7r8lXkLMVx6lH93Wr3JKFZUbsAQ==</Modulus>
  <Exponent>AQAB</Exponent>
  <P>5xKWUkv3ePTr7opGVYJ4P51Tfbd4Lrnf8Pg8NDUHM1c=</P>
  <Q>5EoIpAUV4mHhHeFsW43/XkUsOg7dnAn9s48VIlfCXGc=</Q>
  <DP>NpDZFo4B3npXzHiyqzaoFr2cHa/ZnY8fJtQ3w0xSavk=</DP>
  <DQ>nIV+IycxeAPwG1Khvqw/ON1ok23517Cp9+DUdrWBF2U=</DQ>
  <InverseQ>PjqGl8vNmitqoL/+nPoUxxkay7pLLqt8qyI/Ff/WTG4=</InverseQ>
  <D>Lam0zR0cbqo3gXWHb8oz+pM0ImzPjDKWJ91WpoDQoQ06B4nFzs3QZtaQJx1vfezDY/oYG90fgp5vCZJ/ShRFrQ==</D>
</RSAKeyValue>");

            var decryptedResult = rsa.Decrypt(cipher, false);
            
            return decryptedResult;
        }

        public void Load()
        {
            FileInfo fi = new FileInfo(personsPath);
            FileStream fs = fi.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);

            using (StreamReader r = new StreamReader(fs))
            {
                string json = r.ReadToEnd();
                if (json != "")
                {
                    People = JsonSerializer.Deserialize<List<Person>>(json);
                }
            }

            fi = new FileInfo(votesPath);
            fs = fi.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);

            using (StreamReader r = new StreamReader(fs))
            {
                string json = r.ReadToEnd();
                if (json != "")
                {
                    Votes = JsonSerializer.Deserialize<List<Vote>>(json);
                }
            }

            fi = new FileInfo(bulletinDividersPath);
            fs = fi.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);

            using (StreamReader r = new StreamReader(fs))
            {
                string json = r.ReadToEnd();
                if (json != "")
                {
                    BulletinDividers = JsonSerializer.Deserialize<List<BulletinPart>>(json);
                }
            }

            fi = new FileInfo(bulletinApplicantsPath);
            fs = fi.Open(FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);

            using (StreamReader r = new StreamReader(fs))
            {
                string json = r.ReadToEnd();
                if (json != "")
                {
                    BulletinApplicants = JsonSerializer.Deserialize<List<BulletinPart>>(json);
                }
            }
        }

        public void Save()
        {
            string jsonPeople = JsonSerializer.Serialize(People);
            File.WriteAllText(personsPath, jsonPeople);

            string jsonVotes = JsonSerializer.Serialize(Votes);
            File.WriteAllText(votesPath, jsonVotes);

            string bulletinDividers = JsonSerializer.Serialize(BulletinDividers);
            File.WriteAllText(bulletinDividersPath, bulletinDividers);

            string bulletinApplicants = JsonSerializer.Serialize(BulletinApplicants);
            File.WriteAllText(bulletinApplicantsPath, bulletinApplicants);
        }

        public int GetVotesById(string id)
        {
            var applicant = People.FirstOrDefault(x => x.Id == id && x.Role == Role.applicant);
            if (applicant != null)
                return Votes.Where(x => x.ForWho == id).Count();
            return 0;
        }

        public bool VerifySignedHash(byte[] DataToVerify, byte[] SignedData, DSAParameters Key)
        {
            try
            {
                DSACryptoServiceProvider DSAalg = new DSACryptoServiceProvider();

                DSAalg.ImportParameters(Key);

                return DSAalg.VerifyData(DataToVerify, SignedData);
            }
            catch (CryptographicException e)
            {
                return false;
            }
        }

        public byte[] HashAndSignBytes(byte[] DataToSign, DSAParameters Key)
        {
            try
            {
                DSACryptoServiceProvider DSAalg = new DSACryptoServiceProvider();

                DSAalg.ImportParameters(Key);

                return DSAalg.SignData(DataToSign, HashAlgorithmName.SHA1);
            }
            catch (CryptographicException e)
            {
                return null;
            }
        }

        public byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }
    }
}
