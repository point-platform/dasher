using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace MsgPack.Strict.SchemaGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Environment.ExitCode = (int)new Program().GenerateSchema(args);
            }
            catch (SchemaGenerationException e)
            {
                // Visual Studio parseable format
                Console.WriteLine("MsgPack.Strict.SchemaGenerator.exe : error: " + "Error generating schema for " + e.TargetType + ": " + e.Message);
                Environment.ExitCode = (int)ReturnCode.EXIT_ERROR;
            }
            catch (Exception e)
            {
                // Visual Studio parseable format
                Console.WriteLine("MsgPack.Strict.SchemaGenerator.exe : error: " + e.ToString());
                Environment.ExitCode = (int)ReturnCode.EXIT_ERROR;
            }
        }

        private enum ReturnCode
        {
            EXIT_SUCCESS = 0,
            EXIT_ERROR = 1
        };

        private string targetPath = null;
        private string targetDir = null;
        private string projectDir = null;
        private bool writeToAppMessagesFile = false;
        private bool debug = false;
        private bool help = false;

        private ReturnCode GenerateSchema(string[] args)
        {
            var optionSet = new OptionSet() {
                                { "targetPath=", o => targetPath = o },
                                { "targetDir=",  o => targetDir = o },
                                { "projectDir=",  o => projectDir = o },
                                { "writeToAppMessagesFile",  v => writeToAppMessagesFile = v != null },
                                { "debug",   v => debug = v != null },
                                { "h|?|help",   v => help = v != null },
                                };

            List<string> extra = optionSet.Parse(args);


            if (help)
            {
                Usage();
                return ReturnCode.EXIT_SUCCESS;
            }

            if (debug)
                Debugger.Launch();

            if (targetPath == null || targetDir == null || projectDir == null)
            {
                // This format makes it show up properly in the VS Error window.
                Console.WriteLine("MsgPack.Strict.SchemaGenerator.exe : error: Incorrect command line arguments.");
                Usage();
                return ReturnCode.EXIT_ERROR;
            }

            var assembly = Assembly.LoadFrom(targetPath);

            var sendMessageTypes = new HashSet<Type>();
            var receiveMessageTypes = new HashSet<Type>();

            // TODO this is verbose.  Tidy up using Linq?
            foreach (Type t in assembly.GetTypes())
            {
                foreach (var attribute in t.GetCustomAttributes())
                {
                    if (attribute is SendMessageAttribute)
                    {
                        sendMessageTypes.Add(t);
                    }
                    if (attribute is ReceiveMessageAttribute)
                    {
                        receiveMessageTypes.Add(t);
                    }
                }
            }

            // In both cases we write to the project (ie source) dir and also the target (ie bin) dir.
            // This makes sure that the updated file is included in the source code and also deployed.
            if (writeToAppMessagesFile)
            {
                writeMessageFile(targetDir + "App.messages", sendMessageTypes, receiveMessageTypes);
                writeMessageFile(projectDir + "App.messages", sendMessageTypes, receiveMessageTypes);
            }
            else
            {
                writeToAppManifest(targetDir, sendMessageTypes, receiveMessageTypes);
                writeToAppManifest(projectDir, sendMessageTypes, receiveMessageTypes);
            }

            return ReturnCode.EXIT_SUCCESS;
        }

        private void writeToAppManifest(string dirName, HashSet<Type> sendMessageTypes, HashSet<Type> receiveMessageTypes)
        {
            XDocument doc;
            // Create a file with a root element if the manifest doesn't exist.
            // This should only happen in testing.
            string appManifestFileName = dirName + "App.manifest";
            XElement appElement;
            if (File.Exists(appManifestFileName))
            {
                doc = XDocument.Load(appManifestFileName);
                appElement = doc.Element("App");
                if(appElement == null)
                {
                    appElement = new XElement("App");
                    doc.AddFirst(appElement);
                }
                if(appElement.Element("SendsMessages") == null)
                {
                    appElement.Add(new XElement("SendsMessages"));
                }
                if (appElement.Element("ReceivesMessages") == null)
                {
                    appElement.Add(new XElement("ReceivesMessages"));
                }
            }
            else
            {
                doc = new XDocument(new XElement("App"));
                appElement = doc.Element("App");
                appElement.Add(new XElement("SendsMessages"));
                appElement.Add(new XElement("ReceivesMessages"));
            }

            var sendsMessagesElement = new XElement("SendsMessages");
            var receivesMessagesElement = new XElement("ReceivesMessages");
            foreach (var sendMessageType in sendMessageTypes)
            {
                var message = XMLSchemaGenerator.GenerateSchema(sendMessageType);
                sendsMessagesElement.AddFirst(message);
            }
            foreach (var receiveMessageType in receiveMessageTypes)
            {
                var message = XMLSchemaGenerator.GenerateSchema(receiveMessageType);
                receivesMessagesElement.AddFirst(message);
            }

            appElement.Element("SendsMessages").ReplaceWith(sendsMessagesElement);
            appElement.Element("ReceivesMessages").ReplaceWith(receivesMessagesElement);

            doc.Save(appManifestFileName);
        }

        private void writeMessageFile(string path, HashSet<Type> sendMessageTypes, HashSet<Type> receiveMessageTypes)
        {
            // Delete the file if it exists.
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            //Create the file.
            using (FileStream fs = File.Create(path))
            {
                var bytes = new UTF8Encoding(true).GetBytes("Sends:\r\n");
                fs.Write(bytes, 0, bytes.Length);
                foreach (var sendMessageType in sendMessageTypes)
                {
                    string schema = SchemaGenerator.GenerateSchema(sendMessageType);
                    bytes = new UTF8Encoding(true).GetBytes(schema);
                    fs.Write(bytes, 0, bytes.Length);
                }
                bytes = new UTF8Encoding(true).GetBytes("Receives:\r\n");
                fs.Write(bytes, 0, bytes.Length);
                foreach (var receiveMessageType in receiveMessageTypes)
                {
                    string schema = SchemaGenerator.GenerateSchema(receiveMessageType);
                    bytes = new UTF8Encoding(true).GetBytes(schema);
                    fs.Write(bytes, 0, bytes.Length);
                }
            }
        }

        private static void Usage()
        {
            Console.WriteLine("Usage: MsgPack.Strict.SchemaGenerator.exe --targetDir=TARGETDIR --targetName=TARGETNAME --projectDir=PROJECTDIR [--debug] [--help|-h|-?");
            Console.WriteLine("TARGETDIR is the output directory of the project.  TARGETNAME is the full path of the project target.  PROJECTDIR is the root dir of the project, where the app.messages file will be written.");
        }
    }
}
