using System;
using System.Threading.Tasks;
using System.Threading;
using Ookii.CommandLine;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;

namespace AdminDeploy
{
    class Program
    {
        //static variable for json data
        public static ConcurrentBag<string> json = new ConcurrentBag<string>();

        // function removes special characters from error messages returned from the OS
        // if these are not removed they can cause complications with JSON conversion
        private static string RemoveCharacters(string characterString)
        {
            characterString = characterString.Replace(System.Environment.NewLine, String.Empty);
            characterString = characterString.Replace("\\\\", "\\\\");
            characterString = characterString.Replace("\\", "\\\\");
            characterString = characterString.Replace("'", "");
            characterString = characterString.Replace("\"", "");
            characterString = characterString.TrimEnd('.');
            return characterString;
        }

        static void Main(string[] args)
        {
            // placeholder for json string


            CommandLineParser parser = new CommandLineParser(typeof(SwitchParameters));
            try
            {
                //Create instance for gathering ComputerName values
                SwitchParameters sp = (SwitchParameters)(parser.Parse(args));
                string[] computername = null;

                //default variable count for threads and timeouts
                int threadCount = 10;
                int wmiTimeout = 60;
                int threadTimeout = 120;

                //set thread count
                if (sp.ThreadCount != null)
                {
                    threadCount = Convert.ToInt32(sp.ThreadCount);
                }

                // set wmi timeout count
                if (sp.WmiTimeout != null)
                {
                    wmiTimeout = Convert.ToInt32(sp.WmiTimeout);
                }

                // set wmi timeout count
                if (sp.ThreadTimeout != null)
                {
                    threadTimeout = Convert.ToInt32(sp.ThreadTimeout);
                }

                // grab password
                // double quotes are automatically added in case there are contained quotes within the string
                // these quotes are stripped and trimmed when adding into the password field
                var password = "\"" + Encrypt.Unprotect() + "\"";

                //split computer collection into into array from commas if computerlist is null
                if (sp.ComputerList == null)
                {
                    computername = sp.ComputerName.Split(',');
                }

                //if not null then use computerlist
                else
                {
                    //path to text file
                    string path = @sp.ComputerList.ToString();

                    // This text is added only once to the file.
                    if (File.Exists(path))
                    {
                        // Open the file, convert to one line, and split at comma, removing new line
                        string readText = (File.ReadAllText(path)).Replace(Environment.NewLine, "");
                        computername = readText.Split(',');
                    }
                }

                //Grabs credentials from command line
                using (new Impersonator(sp.Username, sp.Domain, @password.ToString().Trim('"')))
                {
                    //set thread count
                    ThreadPool.SetMinThreads(threadCount, threadCount);
                    ThreadPool.SetMaxThreads(threadCount, threadCount);

                    Parallel.ForEach(computername, new ParallelOptions { MaxDegreeOfParallelism = threadCount }, cn =>
                        {
                            //This handles the timeouts for thread
                            Task task = Task.Factory.StartNew(() =>
                                    {

                                        try
                                        {
                                            //ping machine first
                                            //if it pings execute remote operations
                                            if (Remoting.Ping(cn) == true)
                                            {
                                                //variables for remoting
                                                var remoting = new Remoting();
                                                remoting.ComputerName = cn.ToString();
                                                remoting.SourceDirectory = @sp.SourceDirectory.ToString();
                                                remoting.Username = sp.Username.ToString();
                                                remoting.Domain = sp.Domain.ToString();
                                                remoting.Password = (@password.ToString()).Trim('"');
                                                remoting.ScriptToRun = sp.ScriptToRun.ToString();
                                                remoting.WmiTimeout = wmiTimeout.ToString();

                                                //capture json data from function
                                                var results = remoting.StartThreads();
                                                json.Add("{\"ComputerName\":\"" + cn + "\",\"HostStatus\":\"Online\",\"stdout\":\"OK\",\"Results\":[" + results + "]},");
                                                Console.WriteLine(cn + "...OK");
                                                results = null;

                                            }
                                            else
                                            {
                                                //if machine does not ping say that it is offline
                                                json.Add("{\"ComputerName\":\"" + cn + "\",\"HostStatus\":\"Offline\",\"stdout\":\"OK\",\"Results\":[]},");
                                                Console.WriteLine(cn + "...OFFLINE");
                                            }

                                        }
                                        catch (Exception e)
                                        {
                                            //if error errors 
                                            //catpure json data
                                            json.Add("{\"ComputerName\":\"" + cn + "\",\"HostStatus\":\"Online\",\"stdout\":\"" + RemoveCharacters(e.Message) + " on " + cn + "\",\"Results\":[]},");
                                            Console.WriteLine(RemoveCharacters(e.Message) + " on " + cn);
                                        }
                                    }); //end of task

                            //check if thread times out
                            try
                            {
                                if (task.Wait(threadTimeout * 1000))
                                {
                                    // do nothing
                                    // this theoretically should never be reached since the thread will finish with 
                                    // the time allotted
                                }
                                // if thread times out then throw exception and catch yourself
                                else
                                {
                                    // since thread is hanging throw exception
                                    // this does not remove temp files
                                    // reason for this is if it is an IO error the chances of it 
                                    // hanging are still there if an exception is not immediately thrown
                                    throw new Exception("Thread timeout");
                                }

                            }
                            catch (Exception e)
                            {
                                json.Add("{\"ComputerName\":\"" + cn + "\",\"HostStatus\":\"Online\",\"stdout\":\"" + RemoveCharacters(e.Message) + " on " + cn + "\",\"Results\":[]},");
                                Console.WriteLine(RemoveCharacters(e.Message) + " on " + cn);
                            }

                        }); //end of foreach parallel

                    try
                    {
                        //convert array to one long string
                        var formattedjson = string.Concat(json.ToArray());
                        json = null;

                        //remove trailing comma
                        formattedjson = "{\"Output\":[" + formattedjson.TrimEnd(',') + "]}";

                        //convert to single line
                        formattedjson = JObject.Parse(formattedjson).ToString(Formatting.None);

                        //indent
                        if (sp.FormatJSON == "yes" || sp.OutputLocation == null)
                        {
                            formattedjson = JObject.Parse(formattedjson).ToString(Formatting.Indented);
                        }

                        //output json file
                        if (sp.OutputLocation != null)
                        {
                            System.IO.File.WriteAllText(sp.OutputLocation, formattedjson);
                            formattedjson = null;
                        }
                        else
                        {
                            //output to console and clear string
                            Console.Clear();
                            Console.WriteLine(formattedjson);
                            formattedjson = null;
                        }

                    }
                    catch
                    {
                        Console.WriteLine("Unable to parse JSON data. Please make sure it is a valid JSON string.");
                    }

                }

            }
            catch (CommandLineArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                parser.WriteUsageToConsole();
            }
        }
    }
}
