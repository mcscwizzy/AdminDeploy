using System.Net.NetworkInformation;
using System.Threading;


namespace AdminDeploy
{
    class Remoting
    {
        public string ComputerName { get; set; }
        public string SourceDirectory { get; set; }
        public string Username { get; set; }
        public string Domain { get; set; }
        public string Password { get; set; }
        public string ScriptToRun { get; set; }
        public string WmiTimeout { get; set; }

        //verify host is online
        public static bool Ping(string nameOraddress)
        {
            bool pingable = false;

            try
            {
                Ping myPing = new Ping();
                PingReply reply = myPing.Send(nameOraddress, 5000);

                if (reply.Status == IPStatus.Success)
                {
                    pingable = true;
                }
                else
                {
                    pingable = false;
                }

            }
            catch
            {
                pingable = false;
            }

            return pingable;
        }

        //start threads
        public string StartThreads()
        {
            //set parameters to remoting
            CreateWmiProcess process = new CreateWmiProcess();
            process.ComputerName = ComputerName;
            process.Username = Username;
            process.Password = Password;
            process.Domain = Domain;
            process.WmiTimeout = WmiTimeout;

            //create temp directory
            process.CreateDirectory(@"\\" + ComputerName + @"\c$\deploy_temp");

            //copy files to temp directory
            process.CopyDirectory(SourceDirectory, @"\\" + ComputerName + @"\c$\deploy_temp");

            //execute remote command
            process.InvokeCommand(@"C:\Windows\System32\WindowsPowerShell\v1.0\Powershell.exe -ExecutionPolicy Bypass -File C:\deploy_temp\" + ScriptToRun);

            //grab json files
            string json = process.ImportJSON();

            //remove temp directory
            process.RemoveDirectory(@"\\" + ComputerName + @"\c$\deploy_temp");

            //null process object
            process = null;

            //return json data
            return json;
        }

    }
}
