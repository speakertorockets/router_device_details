# router_device_details

This dotnet console application scrapes the unsecured AT&T router webpage for details on the devices registered on the local network.  

This has been tested against the ATT provided Pace Plc router Model 5268AC, hardware version 260-2173300 and software version 11.14.1.533857-att.  This application scrapes the html from the web page using the AngleSharp library version 1.1.2.  

This application is intentionally written as simple as possible with hard coded ip address for the router (192.168.1.254 which is the default for this router) and it generates a json encoded text file in the current directory as the executable.   

The webpage url for the device details is also hard coded but it matches what the Att router provides in their current software.  Maybe in the future Att will take pity on those of us who are actually interested in this data and generate a json page so we don't need to scrape the html.  

