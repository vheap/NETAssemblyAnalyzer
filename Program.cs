using System.Reflection;
using System.Text;

namespace NETAssemblyAnalyzer
{
    public class Program
    {
        static void Main(string[] args)
        {
            string AsseemblyPath = @"";
            if(File.Exists(AsseemblyPath))
            {
                AnalyzeAssembly(AsseemblyPath);
            }
        }
         static void AnalyzeAssembly(string AssemblyPath)
        {
            try
            {
                var assembly = Assembly.LoadFrom(AssemblyPath);

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"Assembly: {assembly.FullName}\n");

                // Extract Imports and Functions
                var imports = new Dictionary<string, List<string>>();
                foreach (var type in assembly.GetTypes())
                {
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
                    {
                        if (!imports.ContainsKey(method.DeclaringType.FullName))
                            imports[method.DeclaringType.FullName] = new List<string>();
                        imports[method.DeclaringType.FullName].Add(method.Name);
                    }
                }

                Console.WriteLine("Imports and Functions:");
                Console.ForegroundColor = ConsoleColor.White;

                foreach (var import in imports)
                {
                    Console.WriteLine($" Key: - {import.Key}");
                    foreach (var method in import.Value)
                        Console.WriteLine($"  Value: - {method}");
                }

                // Extract Exports
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\nExports:");
                Console.ForegroundColor = ConsoleColor.White;

                foreach (var type in assembly.GetExportedTypes())
                {
                    Console.WriteLine($"- {type.FullName}");
                }

                // Extract Strings
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\nStrings:");
                Console.ForegroundColor = ConsoleColor.White;

                List<string> strings = ExtractStringsFromAssembly(AssemblyPath);
                foreach (var str in strings)
                {
                    Console.WriteLine($"- {str}");
                }

                // Extract Embedded Files and Executables
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\nEmbedded Resources:");
                Console.ForegroundColor = ConsoleColor.White;

                foreach (var resourceName in assembly.GetManifestResourceNames())
                {
                    using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (resourceStream != null)
                        {
                            string fileName = Path.Combine(Directory.GetCurrentDirectory(), resourceName);
                            using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                            {
                                resourceStream.CopyTo(fileStream);
                                Console.WriteLine($"- Extracted: {fileName}");
                            }
                        }
                    }
                }
            }
            catch(Exception ex) { Console.WriteLine("Error: " + ex.Message); }
        }

        static List<string> ExtractStringsFromAssembly(string assemblyPath)
        {
            List<string> strings = new List<string>();

            try
            {
                using (FileStream fs = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader reader = new BinaryReader(fs))
                    {
                        var sb = new StringBuilder();
                        while (reader.BaseStream.Position != reader.BaseStream.Length)
                        {
                            byte b = reader.ReadByte();
                            if (b >= 32 && b <= 126) // ASCII printable characters
                            {
                                sb.Append((char)b);
                            }
                            else
                            {
                                if (sb.Length > 3) // Filter out very short strings
                                {
                                    strings.Add(sb.ToString());
                                }
                                sb.Clear();
                            }
                        }
                        if (sb.Length > 3)
                        {
                            strings.Add(sb.ToString());
                        }
                    }
                }
            }
            catch { }     
            return strings;
        }

    }
    
    
}
