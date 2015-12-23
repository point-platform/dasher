using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

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
        private bool debug = false;
        private bool help = false;

        private ReturnCode GenerateSchema(string[] args)
        {
            var optionSet = new OptionSet() {
                                { "targetPath=", o => targetPath = o },
                                { "targetDir=",  o => targetDir = o },
                                { "projectDir=",  o => projectDir = o },
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

            writeMessageFile(targetDir + "App.messages", sendMessageTypes, receiveMessageTypes);
            writeMessageFile(projectDir + "App.messages", sendMessageTypes, receiveMessageTypes);

            return ReturnCode.EXIT_SUCCESS;
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
                foreach (var sendMessageType in receiveMessageTypes)
                {
                    string schema = SchemaGenerator.GenerateSchema(sendMessageType);
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
