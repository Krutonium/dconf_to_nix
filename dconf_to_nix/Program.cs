using System;
using System.Collections.Generic;
using System.IO;

namespace dconf_to_nix
{
    class Program
    {
        static void Main(string[] args)
        {
            if (File.Exists(args[0]))
            {
                bool notfirstThing = true;
                foreach (var line in File.ReadAllLines(args[0]))
                {
                    if (line.Trim() == String.Empty)
                    {
                        CreateNixFile(args[1], "");
                        continue;
                    }
                    if (line.StartsWith("["))
                    {
                        //This is a path to a setting
                        var path = line.Trim().Replace("[", "").Replace("]", "");
                        path = "    \"" + path + "\" = {";
                        if (notfirstThing == false)
                        {
                            CreateNixFile(args[1], "    };" + Environment.NewLine);
                        }
                        notfirstThing = false;
                        CreateNixFile(args[1], path + Environment.NewLine);
                    }
                    else
                    {
                        var variable = line.Replace("=", " = ");
                        variable = "      " + variable;
                        variable += ";";
                        variable = variable.Replace("'", "\"");
                        variable = variable.Replace(",", " ");
                        if (variable.Contains("@as") | variable.Contains("@av"))
                        {
                            Console.WriteLine("Skipped: " + variable);
                            continue;
                            //I have no idea how to actually do this so we'll just skip it for now
                        }

                        if (variable.Contains("animate-appicon-hover-animation-extent"))
                        {
                            Console.WriteLine("Skipped: " + variable + " because I don't know how to parse it");
                            continue;
                            //I have no idea how to actually do this so we'll just skip it for now
                            //It produces invalid output
                        }
                            
                        CreateNixFile(args[1], variable + Environment.NewLine);
                    }
                }
                CloseNixFile(args[1]);
            }
        }

        static void CreateNixFile(string location, string toAdd)
        {
            if (File.Exists(location) == false)
            {
                //Create initial boilerplate then write it to disk
                //Create List of string
                List<string> lines = new List<string>();
                lines.Add("{ lib, ... }:");
                lines.Add("let");
                lines.Add("  mkTuple = lib.hm.gvariant.mkTuple;");
                lines.Add("in");
                lines.Add("{");
                lines.Add("  dconf.settings = {" );
                File.WriteAllLines(location, lines);
            }
            //Open File at location and add to it
            File.AppendAllText(location, toAdd);
        }

        static void CloseNixFile(string location)
        {
            //Open File at location then Add "};" to file.
            File.AppendAllText(location, "    };" + Environment.NewLine);
            File.AppendAllText(location, "  };" + Environment.NewLine);
            File.AppendAllText(location, "}" + Environment.NewLine);
        }
    }
}