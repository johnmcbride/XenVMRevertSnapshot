XenVMRevertSnapshot
==========
A simple tool to automate reverting a Citrix XenServer Virtual Machine back to a specified SnapShot.

Usage
------
once you have your configuration file updated you can run the program by calling the following command

XenVMrevertSnapshot.exe [path to xml config file]

Ex. XenVMrevertSnapshot.exe c:\utilitys\xenlist.xml

Configuration File
------
In order for this utility to work, you will need to define a configuration file.

The definition of the file is as follows.

You can have one or more &lt;Servers> elements.
You can also have one or more &lt;VM> elements under the &lt;Server> element.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Servers>
  <Server IPAddress="[XenServer Address]" Username="[Username]" Password="[Xen User Password]" Port="80">
    <VM Name="[Name of VM that your want to revert]">
      <RevertTo SSName="[Name of snapshot to revert to]"></RevertTo>
    </VM>
  </Server>
</Servers>
```
