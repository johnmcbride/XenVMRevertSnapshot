using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XenVMrevertSnapshot
{
    class Program
    {
        static void Main(string[] args)
        {
            string _configFile = null;
            //defining the log file name. Log file will be in the same location as the exe'

            string _logFileName = string.Format(@"{1}\revertss_run_{0}.txt", DateTime.Now.ToString("MMddyyyy_hhmmss"), Environment.CurrentDirectory);
            //opening a the log file for writing
            //writing to the log file
            StreamWriter _logFile = File.AppendText(_logFileName);
            _logFile.WriteLine(string.Format("{0},Starting reverting snapshot run", DateTime.Now));
            _logFile.WriteLine(string.Format("{0},Parsing command line paramaters", DateTime.Now));
            //looping through any command line parameters passed in
            for ( int i=0;i<args.Length;i++)
            {
                if ( args[i].Contains("configfile="))
                {
                    _logFile.WriteLine(string.Format("{0},Found config file command line parameter", DateTime.Now));
                    string[] _elements = args[i].Split('=');
                    _configFile = _elements[1];
                }
            }
            //couldn't find the config file command line parameter. log it and exit
            if ( _configFile == null)
            {
                _logFile.WriteLine(string.Format("{0},Could not find config file command line parameter", DateTime.Now));
                Console.WriteLine(@"You must enter a config file for this utility to run.\nPlease use configfile=<FQDN of file>\n\nEx: configfile=c:\serverss.xml");
                Environment.Exit(0);
            }


            try
            {
                //Loading up the xml configuration file
                XmlDocument _configDoc = new XmlDocument();
                _logFile.WriteLine(string.Format("{0},Loading configuration file", DateTime.Now));
                _configDoc.Load(_configFile);
                _logFile.WriteLine(string.Format("{0},Obtaining servers", DateTime.Now));
                //define the xpath for pulling out each server element from the xml file
                XmlNodeList _servers = _configDoc.SelectNodes("/Servers/Server");
                //loop through each server element found
                foreach (XmlNode _server in _servers)
                {
                    string _ip = null;
                    string _uname = null;
                    string _password = null;
                    string _port = null;

                    _logFile.WriteLine(string.Format("{0},Getting the server attributes from the elements", DateTime.Now));
                    //get the values from the xml file for connection to the xen servers referenced
                    _ip = _server.Attributes.GetNamedItem("IPAddress").Value.ToString();
                    _uname = _server.Attributes.GetNamedItem("Username").Value.ToString();
                    _password = _server.Attributes.GetNamedItem("Password").Value.ToString();
                    _port = _server.Attributes.GetNamedItem("Port").Value.ToString();

                    _logFile.WriteLine(string.Format("{0},Trying to authenticate to the Xen Server", DateTime.Now));
                    //authenticate to the xen server
                    XenAPI.Session s = new XenAPI.Session(_ip, Convert.ToInt32(_port));
                    //login
                    try
                    {
                        s.login_with_password(_uname, _password);
                    }
                    catch (Exception loginErr)
                    {
                        _logFile.WriteLine(string.Format("{0},Login threw an exception. Aborting program\n\n StackTrace: {1}", DateTime.Now, loginErr.StackTrace));
                        _logFile.Close();
                        Environment.Exit(1);
                    }

                    //get the list of VMs that we need to revert back to a snapshot
                    XmlNodeList _vmNodes = _server.SelectNodes("VM");

                    _logFile.WriteLine(string.Format("{0},Obtaining the VM elements under the server elements", DateTime.Now));
                    //loop trough each VM child element under the server element
                    foreach (XmlNode _vmNode in _vmNodes)
                    {
                        _logFile.WriteLine(string.Format("{0},Getting the vm attributes and children from the element", DateTime.Now));
                        string _vmName = _vmNode.Attributes.GetNamedItem("Name").Value;
                        XmlNode _revertToNode = _vmNode.SelectSingleNode("RevertTo");
                        string _snapshotName = _revertToNode.Attributes.GetNamedItem("SSName").Value;


                        _logFile.WriteLine(string.Format("{0},Querying Xen to find the VM ({1}) by name", DateTime.Now, _vmName));
                        foreach (var _vm in XenAPI.VM.get_by_name_label(s, _vmName))
                        {
                            XenAPI.VM _vmprops = XenAPI.VM.get_record(s, _vm);

                            _logFile.WriteLine(string.Format("{0},Checking if its a template", DateTime.Now));

                            if (!_vmprops.is_a_template)
                            {
                                _logFile.WriteLine(string.Format("{0},Checking if the VM name we found is the one we need to process", DateTime.Now));
                                if (_vmprops.name_label == _vmName)
                                {
                                    _logFile.WriteLine(string.Format("{0},Getting the snapshot list for VM '{1}", DateTime.Now, _vmName));
                                    List<XenAPI.XenRef<XenAPI.VM>> _refs = _vmprops.snapshots;

                                    foreach (XenAPI.XenRef<XenAPI.VM> _vmRefSS in _refs)
                                    {
                                        XenAPI.VM _vmss = XenAPI.VM.get_record(s, _vmRefSS);
                                        //check if the VM returned is a template.
                                        if (_vmss.is_a_snapshot)
                                        {
                                            if (_vmss.name_label == _snapshotName)
                                            {
                                                _logFile.WriteLine(string.Format("{0},Reverting to snapshot '{1}' for VM '{2}'", DateTime.Now, _snapshotName, _vmName));
                                                XenAPI.VM.revert(s, _vmRefSS);
                                                _logFile.WriteLine(string.Format("{0},Starting up the VM '{1}'", DateTime.Now, _vmName));
                                                XenAPI.VM.start(s, _vm, false, true);
                                                break;
                                            }
                                        }
                                    }

                                }
                            }

                        }

                    }
                }
                //close the File
                _logFile.Close();
            }
            catch (Exception generalError)
            {
                _logFile.WriteLine(string.Format("{0},Gernal error has occurred.\n\n StackTrack:{1}", DateTime.Now, generalError.StackTrace));
                //close the File
                _logFile.Close();

            }
           


            
        }
    }
}
