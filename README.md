# AdminDeploy
A powerful CLI utility used to deploy Powershell scripts. These scripts can be used to install sofware, gather information, make changes to the OS, etc. 

The uniqueness of this utility lies in the way it is made. It copies an entire folder to the target machine(s), then runs the script, outputs the results from script, then bring it back and outputs the results in a JSON file. This file can then be consumed by an API service or whereever you need this information to go. This can even be used as a one-to-many monitoring software. The possibilites are endless. 

This CLI utility does all of this in parallel. The number of threads are set in the command line. Depending on your script, I was able to hit 3500+ servers in under 3 minutes with 1000 threads. So you can do what you need to do fast. If PSRemoting is not enabled on your servers, that will not be a problem due to how the program is run. The scripts are copied locally and invoked through WMI. Whatever account you use the password is encrypted using DPAPI. Run the EncryptPassword.exe and it will encrypt the password for whatever service account you are using to run the software. This is what account it will use to execute the script you are wanting to deploy. 

Any Powershell script can be used with this CLI utiltiy. To get detailed results from your Powershell scripst back make sure to convert the output to JSON. e.g. get-service | convertto-json

I have not gotten full documentation up on this yet. If you have any questions please feel free to contact me. This little utility is EXTREMELY powerful and is in my opinion something every sysadmin should have in their toolbox. Heck, even a developer or engineer for that matter. 

