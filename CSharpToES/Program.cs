
using TottiWatti.CSharpToES;
using System.Reflection;
using System.Diagnostics;

internal class Program
{
    static void Main(string[] args)
    {
        // write program name and version to console
        var versionString = Assembly.GetEntryAssembly()?
                                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                                    .InformationalVersion
                                    .ToString();
        Console.WriteLine($"CSharpToES v{versionString}");
        Console.WriteLine("-----------------");

        string? sourceDir = null;
        string? destDir = null;


        if (Debugger.IsAttached)
        {
            // debug mode
            DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());            
            // search TestInput directory by traversing up from current (debug) directory, copying test files to output directory hence unnessary
            while (string.IsNullOrEmpty(sourceDir) && string.IsNullOrEmpty(destDir))
            {
                DirectoryInfo? parentDir = di.Parent;
                if (parentDir != null )
                {
                    di = parentDir;
                    DirectoryInfo[] subdirs = parentDir.GetDirectories();
                    foreach(DirectoryInfo subdir in subdirs)
                    {
                        if (subdir.Name == "TestInput")
                        {
                            sourceDir = subdir.FullName;
                            destDir = sourceDir.Replace("TestInput", "TestOutput");
                        }
                    }
                }
                else
                {
                    break;
                }
            }            
        }
        else
        {
            // production mode
            if (args.Length > 1)
            {
                // get source and target directories from args
                sourceDir = args[0];
                destDir = args[1];

                if (string.IsNullOrEmpty(sourceDir))
                {
                    Console.WriteLine("Error: Source directory not specified");
                }
                else if (string.IsNullOrEmpty(destDir))
                {
                    Console.WriteLine("Error: Destination directory not specified");
                }                
            }
            else
            {
                // no args, show usage help
                Console.WriteLine("Usage:");
                Console.WriteLine("  csharptoes <C# source directory path> <es destination directory path>");
            }
        }

        // if source and destination directory specified run converter
        if (!string.IsNullOrEmpty(sourceDir) && !string.IsNullOrEmpty(destDir))
        {
            var result = StructureFilesParser.Parse(sourceDir, destDir);
        }
        
        // if started from Visual Studio get input key before exit to keep console visible
        if (Debugger.IsAttached)
        {
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}