using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace laba3_vote.Models
{
    public class VK
    {
        private CVK cvk;
        public VK(CVK cvk)
        {
            this.cvk = cvk;
        }

        public bool VoteDivider(string VoterId, byte[] Bulletin, byte[] sign, DSAParameters key)
        {
            var result = GetBulletin(VoterId, Bulletin, sign, key, out BulletinPart bulletine);

            if (result)
                cvk.BulletinDividers.Add(bulletine);

            return result;
        }

        public bool VoteApplicant(string VoterId, byte[] Bulletin, byte[] sign, DSAParameters key)
        {
            var result = GetBulletin(VoterId, Bulletin, sign, key, out BulletinPart bulletine);

            if (result)
                cvk.BulletinApplicants.Add(bulletine);

            return result;
        }

        private bool GetBulletin(string VoterId, byte[] Bulletin, byte[] sign, DSAParameters key, out BulletinPart bulletine)
        {
            bool signCheck = cvk.VerifySignedHash(Bulletin, sign, key);
            bulletine = new BulletinPart();

            if (!signCheck)
            {
                Console.WriteLine("sign is wrong!");
                return false;
            }

            var person = cvk.People.FirstOrDefault(x => x.Id == VoterId);

            if (person == null)
            {
                Console.WriteLine("Person doesn't exist");
                return false;
            }

            if (person.Voted)
            {
                Console.WriteLine("Person already voted!");
                return false;
            }

            bulletine = new BulletinPart() { Id = VoterId, bulletine = Bulletin };
            return true;
        }
    }
}
