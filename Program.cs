using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using laba3_vote.Models;
using System.Runtime.ConstrainedExecution;

namespace laba3_vote
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CVK cvk = new CVK();
            cvk.Load();

            VK vk1 = new VK(cvk);
            VK vk2 = new VK(cvk);

            Person currentUser = null;
            bool exit = false;
            while (!exit)
            {
                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("1. Choose Person\n2. Create Person\n3. Delete Person\n4. Watch Results\n5. exit");
                    string action = Console.ReadLine();

                    if (action == "1")
                    {
                        Console.Clear();

                        if (cvk.People.FindAll(x => x.Role == Role.voter).Count > 0)
                        {

                            var people = cvk.People.FindAll(x => x.Role == Role.voter);
                            foreach (var item in people)
                            {
                                Console.WriteLine($"{item.Id} Role: {item.Role} Name: {item.Name} Surname: {item.Surname}");
                            }

                            Console.WriteLine("Choose person");
                            var id = Console.ReadLine();
                            var result = cvk.People.FirstOrDefault(x => x.Id == id);
                            if (result != null && result.Role == Role.voter)
                            {
                                currentUser = result;
                                break;
                            }
                            else
                            {
                                Console.WriteLine("Person don't exist");
                                Console.ReadLine();
                            }

                        }
                        else
                        {
                            Console.WriteLine("People don't exist");
                            Console.ReadLine();
                        }
                    }
                    if (action == "2")
                    {
                        Console.Clear();
                        Console.WriteLine("Write a name:");
                        var name = Console.ReadLine();
                        Console.WriteLine("Write a surname:");
                        var surname = Console.ReadLine();

                        Console.WriteLine($"Write a role: ({Role.applicant}, {Role.voter})");
                        var role = Console.ReadLine();

                        if (name != "" && surname != "" && Enum.IsDefined(typeof(Role), role))
                        {
                            var random = new Random();
                            string id = random.Next(1000000).ToString();

                            while (cvk.People.FirstOrDefault(x => x.Id == id) != null)
                            {
                                id = random.Next(1000000).ToString();
                            }

                            cvk.People.Add(new Person(id, name, surname, (Role)Enum.Parse(typeof(Role), role)));
                            cvk.Save();
                            continue;
                        }
                    }
                    if (action == "3")
                    {
                        Console.Clear();
                        var people = cvk.People;

                        if (people.Count == 0)
                        {
                            Console.WriteLine("People don't exist");
                            Console.ReadLine();
                            continue;
                        }

                        foreach (var item in people)
                        {
                            Console.WriteLine($"{item.Id} Role: {item.Role} Name: {item.Name} Surname: {item.Surname}");
                        }

                        Console.WriteLine("Choose person");

                        var id = Console.ReadLine();
                        var result = cvk.People.FirstOrDefault(x => x.Id == id);
                        if (result != null)
                        {
                            cvk.People.Remove(result);
                            continue;
                        }
                        else
                        {
                            Console.WriteLine("Person doesn't exist");
                            Console.ReadLine();
                        }
                    }
                    if (action == "4")
                    {
                        var results = cvk.GetResults();

                        var applicants = cvk.People.Where(x => x.Role == Role.applicant);
                        foreach (var applicant in applicants)
                        {
                            Console.WriteLine($"applicant id: {applicant.Id}; name: {applicant.Name}; votes: {results.Count(x => x.Id==applicant.Id)}");
                        }


                        Console.WriteLine();

                        foreach (var item in results)
                        {
                            Console.WriteLine($"voter id: {item.voterId} applicant id: {item.Id}");
                        }

                        Console.ReadLine();
                        continue;
                    }
                    if (action == "5")
                    {
                        exit = true;
                        break;
                    }
                }

                while (true)
                {
                    Console.Clear();
                    if (currentUser == null)
                    {
                        Console.WriteLine("Current user doesn't exist");
                        break;
                    }

                    if (currentUser.Voted == true && currentUser.Permission != true)
                    {
                        Console.WriteLine("Current user can't vote");
                        break;
                    }

                    Console.WriteLine("Write id of the applicant you want to vote for");

                    var people = cvk.People.FindAll(x => x.Role == Role.applicant);

                    foreach (var item in people)
                    {
                        Console.WriteLine($"Id: {item.Id} Name: {item.Name} Surname: {item.Surname}");
                    }

                    var id = Console.ReadLine();
                    var choosenApplicant = cvk.People.FirstOrDefault(x => x.Id == id);

                    if (choosenApplicant == null)
                    {
                        Console.WriteLine("Applicant doesn't exist");
                        Console.ReadLine();
                        break;
                    }

                    var random = new Random();
                    var multiplier = random.Next(100);

                    var dividedId = (Convert.ToInt32(choosenApplicant.Id) * multiplier);

                    RSACryptoServiceProvider publicKey = new RSACryptoServiceProvider();
                    publicKey.ImportParameters(cvk.GetPublicKey());

                    var bulletin1 = publicKey.Encrypt(Encoding.UTF8.GetBytes(multiplier.ToString()), false);
                    var bulletin2 = publicKey.Encrypt(Encoding.UTF8.GetBytes(dividedId.ToString()), false);

                    DSACryptoServiceProvider DSA = new DSACryptoServiceProvider();
                    DSAParameters privateSignKey = DSA.ExportParameters(true);
                    DSAParameters publicSignKey = DSA.ExportParameters(false);
                    byte[] sign1 = cvk.HashAndSignBytes(bulletin1, privateSignKey);
                    byte[] sign2 = cvk.HashAndSignBytes(bulletin2, privateSignKey);

                    var result = vk1.VoteDivider(currentUser.Id, bulletin1, sign1, publicSignKey);
                    var result2 = vk2.VoteApplicant(currentUser.Id, bulletin2, sign2, publicSignKey);

                    cvk.People.FirstOrDefault(x => x.Id == currentUser.Id).Voted = true;

                    if (!result || !result2)
                        Console.WriteLine("Something went wrong");
                    else
                        Console.WriteLine("Success");

                    Console.ReadLine();
                    break;
                }

                cvk.Save();

            }


        }
    }
}
