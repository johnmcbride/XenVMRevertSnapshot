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
```xml
<?xml version="1.0" encoding="utf-8" ?>
<Configuration>
  <EmailNotification Enabled="[true,false]" SMTPServer="[Your SMTP Server]" Port="25">
    <!-- You can have multiple Notify elements -->
    <Notify>address="[Email address to send log to]"</Notify>
  </EmailNotification>
  <Servers>
    <Server IPAddress="[XenServer Address]" Username="root" Password="[Xen User Password]" Port="80">
      <VM Name="[Name of VM that your want to revert]">
        <RevertTo SSName="[Name of snapshot to revert to]"></RevertTo>
      </VM>
    </Server>
  </Servers>
</Configuration>
```

#####Email Notification
You can enable email notification after each run of the snapshot tool.

If you would like to enable auto email notifications after each run, set the "Enabled" attribute to "true" on the &lt;EmailNotification> element.

You will also need to add you SMTP server to the config file. Update the &lt;SMTPServer> node with your SMTP server information including the port attribute.

For each recipient you would like to notify you will add a &lt;Notify> element with the email address of recipient as the text of the element. You can have multiple &lt;Notify> elements. 

For Example:&lt;Notify>address="test@test.com"&lt;/Notify>


#####Server/Snapshot Information

You can have one or more &lt;Server> elements.
You can also have one or more &lt;VM> elements under the &lt;Server> element.


