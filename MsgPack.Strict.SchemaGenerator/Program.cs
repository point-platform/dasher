using NDesk.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace MsgPack.Strict.SchemaGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Debugger.Launch();
            new Program().GenerateSchema(args);
        }

        private string targetName = null;
        private string targetDir = null;

        private void GenerateSchema(string[] args)
        { 
            var optionSet = new OptionSet() {
                                { "targetName=", o => targetName = o },
                                { "targetDir=",  o => targetDir = o },
                                };
            List<string> extra = optionSet.Parse(args);
            if (targetName == null || targetDir == null)
            {
                // This format makes it show up properly in the VS Error window.
                Console.WriteLine("MsgPack.Strict.SchemaGenerator.exe : error: Incorrect command line arguments.");
                Usage();
                Environment.ExitCode = 1;
            }

            AppDomain.CurrentDomain.AssemblyResolve += (sender, eventArgs) => { return Assembly.LoadFrom(targetDir + eventArgs.Name); };
            var assembly = Assembly.LoadFrom(targetDir + targetName);

            foreach (Type t in assembly.GetTypes())
            {
                Debug.Print(t.ToString());
            }

            Type msgPackPackerType = null;
            foreach (var file in Directory.GetFiles(targetDir, "*.dll"))
            {
                assembly = Assembly.LoadFrom(file);
                msgPackPackerType = assembly.GetType("MsgPack.Strict.StrictSerialiser");
                
                foreach (Type t in assembly.GetTypes())
                {
                    Debug.Print(file + " => " + t.ToString());

                    DisplayTypeInfo(t);
                }


                //if (msgPackPackerType != null)
                //    break;
            }

            //Type[] typeParameters = msgPackPackerType.GetGenericArguments();

            Environment.ExitCode = 0;
        }

        private static void DisplayTypeInfo(Type t)
        {
            Debug.Print("\r\n{0}", t);
            Debug.Print("\tIs this a generic type definition? {0}",
                t.IsGenericTypeDefinition);
            Debug.Print("\tIs it a generic type? {0}",
                t.IsGenericType);
            Type[] typeArguments = t.GetGenericArguments();
            Debug.Print("\tList type arguments ({0}):", typeArguments.Length);
            foreach (Type tParam in typeArguments)
            {
                Debug.Print("\t\t{0}", tParam);
            }
        }

        private static void Usage()
        {
            Console.WriteLine("Usage: MsgPack.Strict.SchemaGenerator.exe --targetDir=TARGETDIR --targetName=TARGETNAME");
        }
    }
}
