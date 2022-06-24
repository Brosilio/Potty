using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;

namespace Potty
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class CliArgAttribute : Attribute
    {
        public string Name { get; set; }
        public bool Required { get; set; }
        public string HelpText { get; set; }

        public CliArgAttribute()
        {

        }

        public CliArgAttribute(string argName)
        {
            this.Name = argName;
        }
    }

    public static class CmdArg
    {
        /// <summary>
        /// Create an instance of the class and paint the args on it. See <see cref="Paint{T}(string[], T)"/>
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="args">The arguments to parse.</param>
        /// <returns></returns>
        public static T Create<T>(string[] args) where T : class, new()
        {
            T target = new T();
            Paint<T>(args, target);
            return target;
        }

        /// <summary>
        /// "Paint" command line arguments (<paramref name="args"/>) to an object (<paramref name="target"/>).
        /// </summary>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="args">The command-line args.</param>
        /// <param name="target">The object to paint the args over.</param>
        /// <returns>true if all required args are set, false if not</returns>
        public static bool Paint<T>(string[] args, T target) where T : class
        {
            if (args == null) throw new ArgumentNullException(nameof(args));
            if (target == null) throw new ArgumentNullException(nameof(target));

            bool success = true;

            Dictionary<string, string> values = new Dictionary<string, string>();
            foreach (string str in args)
            {
                if (str == null) continue;

                string arg = str;
                string val = null;

                if (arg.StartsWith("-"))
                {
                    arg = str.TrimStart('-');
                }

                if (arg.Contains('='))
                {
                    arg = arg.Substring(0, arg.IndexOf('='));
                    val = str.IndexOf('=') + 1 >= str.Length ? string.Empty : str.Substring(str.IndexOf('=') + 1);
                }

                if (values.ContainsKey(arg))
                {
                    values[arg] = val;
                }
                else
                {
                    values.Add(arg, val);
                }
            }

            foreach (FieldInfo field in target.GetType().GetFields())
            {
                object[] attribs = field.GetCustomAttributes(typeof(CliArgAttribute), false);
                if (attribs.Length == 0) continue;

                foreach (object obj in attribs)
                {
                    CliArgAttribute a = (CliArgAttribute)obj;
                    if (a.Name == null) a.Name = field.Name;

                    string val = values.ContainsKey(a.Name) ? values[a.Name] : null;

                    if (values.ContainsKey(a.Name))
                    {
                        if (field.FieldType == typeof(bool) && string.IsNullOrWhiteSpace(values[a.Name]))
                        {
                            field.SetValue(target, true);
                        }
                        else
                        {
                            field.SetValue(target, TypeDescriptor.GetConverter(field.FieldType).ConvertFromInvariantString(values[a.Name]));
                        }
                    }
                    else if (a.Required)
                    {
                        success = false;
                    }
                }
            }

            return success;
        }

        /// <summary>
        /// Generate a generic-ish usage string based on the HelpText values specified on the fields of a type.
        /// </summary>
        /// <typeparam name="T">The type containing the fields.</typeparam>
        /// <returns></returns>
        public static string GetUsageString<T>() where T : class
        {
            StringBuilder sb = new StringBuilder();

            foreach (FieldInfo field in typeof(T).GetFields())
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

                    sb.AppendLine("--" + a.Name + " : " + type);
                    sb.AppendLine("\t" + a.HelpText);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }
    }
}
