using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime;
using System.Reflection;
using System.ComponentModel;

namespace Potty
{
    internal abstract class CliProgramBase
    {
        protected readonly string[] SysArgs;

        public CliProgramBase(string[] args)
        {
            SysArgs = args;

            // parse arguments
            Dictionary<string, string> arguments = CliProgramBase.ParseArgs(args);

            // get fields defined on child class and set their values
            foreach (FieldInfo field in this.GetType().GetFields())
            {
                object[] attribs = field.GetCustomAttributes(typeof(CliArgAttribute), false);
                if (attribs.Length == 0) continue;

                foreach (object obj in attribs)
                {
                    CliArgAttribute a = (CliArgAttribute)obj;
                    string name = a.Name == null ? field.Name.ToUpper() : a.Name.ToUpper();

                    string val = arguments.ContainsKey(name) ? arguments[name] : null;

                    if (arguments.ContainsKey(name))
                    {
                        if (field.FieldType == typeof(bool) && string.IsNullOrWhiteSpace(arguments[name]))
                        {
                            field.SetValue(this, true);
                        }
                        else
                        {
                            field.SetValue(this, TypeDescriptor.GetConverter(field.FieldType).ConvertFromInvariantString(arguments[name]));
                        }
                    }
                    else if (a.Required)
                    {
                        throw new ArgumentException("A required command-line argument was not provided.", name);
                    }
                }
            }
        }

        /// <summary>
        /// Parse command line args into a Dictionary. (--format=like --this)
        /// </summary>
        /// 
        /// <param name="args">The args from the system.</param>
        /// 
        /// <returns>
        /// A dictionary containing the args.
        /// </returns>
        private static Dictionary<string, string> ParseArgs(string[] args)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();

            foreach (string arg in args)
            {
                if (arg == null) continue;

                string key = arg;
                string val = null;

                if (key.StartsWith("-"))
                {
                    key = arg.TrimStart('-');
                }

                if (key.Contains('='))
                {
                    key = key.Substring(0, key.IndexOf('='));
                    val = arg.IndexOf('=') + 1 >= arg.Length ? string.Empty : arg.Substring(arg.IndexOf('=') + 1);
                }

                key = key.ToUpper();

                if (values.ContainsKey(key))
                {
                    values[key] = val;
                }
                else
                {
                    values.Add(key, val);
                }
            }

            return values;
        }

        /// <summary>
        /// Generate a generic-ish usage string based on the HelpText values specified on the fields of this type.
        /// </summary>
        protected string GetCliUsageString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (FieldInfo field in GetType().GetFields())
            {
                object[] attribs = field.GetCustomAttributes(typeof(CliArgAttribute), false);
                if (attribs.Length == 0) continue;

                foreach (object obj in attribs)
                {
                    CliArgAttribute a = (CliArgAttribute)obj;
                    if (a.HelpText == null) continue;
                    if (string.IsNullOrWhiteSpace(a.Name)) a.Name = field.Name;

                    string type = field.FieldType.Name.ToLower();
                    if (field.FieldType.IsEnum)
                    {

                        type = $"{field.FieldType.Name} <{string.Join(", ", Enum.GetNames(field.FieldType))}>";
                    }

                    sb.AppendLine($"{a.Name}: {type}");
                    sb.AppendLine($"  {a.HelpText}");
                    //sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        protected abstract bool ValidateCliArgs();
        protected abstract void CliMain();
    }
}
