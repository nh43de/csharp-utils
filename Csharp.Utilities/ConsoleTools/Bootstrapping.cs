using System;
using System.Linq;
using DataPowerTools.Extensions;

namespace Csharp.Utilities.ConsoleTools
{
    /// <summary>
    /// Calling this method in your program.main() will display all public static parameterless methods on the type provided,
    /// and allow the user to select one of them to execute. 
    /// //TODO: would be cool to integrate reflection + CL parser to be able to invoke non-parameterless methods and with CL args.
    /// </summary>
    public static class Bootstrapping
    {
        public static void Bootstrap(Type hostType)
        {
            while (true)
            {
                Console.WriteLine("Program modes:");
                Console.WriteLine("");
                Console.WriteLine("");
                
                //get parameterless static methods in me
                var methods = hostType.GetMethods().Where(m => m.IsPublic && m.IsStatic && m.GetParameters().Length == 0).ToArray();

                var i = 1;
                foreach (var methodInfo in methods)
                {
                    Console.WriteLine($"{i}. {methodInfo.Name}");
                    i++;
                }

                Console.WriteLine("");
                Console.WriteLine("");

                Console.WriteLine("Make a selection: ");

                var a = Console.ReadLine();

                int selectedInt;

                Console.WriteLine("");
                Console.WriteLine("");

                if (int.TryParse(a, out selectedInt) && selectedInt > 0 && selectedInt <= methods.Length)
                {
                    try
                    {
                        methods[selectedInt - 1].Invoke(null, null);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ConcatenateInners());
                        Console.WriteLine("Press any key to continue");
                        Console.Read();
                    }
                }
                else
                {
                    continue;
                }
                break;
            }
        }


    }
}
