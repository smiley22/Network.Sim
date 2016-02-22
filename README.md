### Introduction

Network.Sim is a software for modeling and simulating computer networks. It implements a full network
stack up to the IP-layer and provides a simulation of the IEEE 802.3 data link and physical layers
including MAC/LLC and address resolution (ARP). It can be easily extended to support simulation of
LAN technologies other than Ethernet such as IEEE 802.5 (token ring) or IEE 802.4 (token bus).


### Usage & Examples

The program comes with a couple of example scenarios demonstrating some of the basic principles
of local area networking such as network address resolution and the CSMA/CD media access control
method for resolving collisions during frame transmissions.

A scenario is a simple C# class that inherits from the Scenario Base class. On startup the program
will look for any C# source files in the /Scenarios directory and compile them on-the-fly, so in
order to create your own scenario you can simply copy and adapt one of the existing C# scripts.

After selecting a scenario you can run/single step through it in the command-line interpreter. The
interpreter also provides a couple of other commands that let you examine a host's ARP and routing
tables, etc. Type 'help' for a list of all available commands.

Source code documentation can be found [here](http://smiley22.github.io/Network.Sim/Documentation/).

<p align="center">
 <img src="/Network.Sim.1.png?raw=true" />
</p>
<p align="center">
 <img src="/Network.Sim.2.png?raw=true" />
</p>

### Credits

This program is copyright © 2013 Torben Könke.


### License

This program is released under the [GPL license](https://github.com/smiley22/Network.Sim/blob/master/License.md).
