using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TottiWatti.CSharpToES
{
    /// <summary>
    /// Parses .cs files to models and then writes models to .js file in ES format
    /// </summary>
    public static class StructureFilesParser
    {
        /// <summary>
        /// Parses .cs files to models and then writes models to .js file in ES format
        /// </summary>
        /// <param name="sourceDirectory">.cs files shared with ES source directory</param>
        /// <param name="destinationDirectory">destination directory for generated ES equivalents of .cs files</param>
        /// <returns></returns>
        public static int Parse(string sourceDirectory, string destinationDirectory)
        {
            try
            {
                
                var files = Directory.GetFiles(sourceDirectory, "*.cs", SearchOption.AllDirectories);
               
                if (files.Length == 0)
                {
                    Console.WriteLine($"Warning: Source Directory '{sourceDirectory}' does not contain any files");
                }

                // load last conversion source file last write times
                string fileInfoFile = Path.Combine(destinationDirectory, "CSharpToES.SourceFileModifyDateTimes.json");
                string? fileInfoJson = null; 
                if (File.Exists(fileInfoFile))
                {                    
                    fileInfoJson = File.ReadAllText(fileInfoFile);
                }
                Dictionary<string, System.DateTime>? LastFileWriteTimes = null;
                if (!String.IsNullOrEmpty(fileInfoJson))
                {
                    LastFileWriteTimes = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, System.DateTime>>(fileInfoJson);
                }                               
                var FileInfos = new List<System.IO.FileInfo>();
                var MakeConversion = false;

                // source file changed?
                foreach (var file in files)
                {
                    System.IO.FileInfo fi = new System.IO.FileInfo(file);
                    DateTime? LastWriteTime = null;
                    if (LastFileWriteTimes != null && LastFileWriteTimes.ContainsKey(fi.FullName))
                    {
                        LastWriteTime = LastFileWriteTimes[fi.FullName];
                        string hasChanged = fi.LastWriteTime != LastWriteTime ? "" : "not ";
                        Console.WriteLine($"Source file '{file}' has {hasChanged} changed since last conversion");
                    }
                    else
                    {
                        Console.WriteLine($"Source file '{file}' has not been previously converted");
                    }
                    
                    MakeConversion = (MakeConversion || LastWriteTime == null || fi.LastWriteTime != LastWriteTime);
                    FileInfos.Add(fi);
                }

                // when debugging always create js output files
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    MakeConversion = true;
                }

                if (!MakeConversion) { return 1; }

                var structureFiles = new List<StructureFileModel>();
                var structures = new List<StructureModel>();
                var enums = new List<EnumModel>();

                // read all class models from files
                foreach (var file in files)
                {
                    string code;
                    using (var sr = new StreamReader(file))
                    {
                        code = sr.ReadToEnd();
                    }

                    StructureFileModel cf = new StructureFileModel();
                    cf.SourceFile = file;                    
                    cf.DestinationFile =  Path.ChangeExtension(destinationDirectory + cf.SourceFile.Replace(sourceDirectory, ""), "js");                   
                    cf.RelativeDestinationFile = "." + Path.ChangeExtension(cf.SourceFile, "js").Replace(sourceDirectory, "").Replace("\\", "/");
                   
                    ModelsParser.Parse(cf, code);

                    structures.AddRange(cf.ClassModels);
                    enums.AddRange(cf.EnumModels);                

                    structureFiles.Add(cf);
                }                

                // cross imports between files
                foreach (var classFile in structureFiles)
                {
                    foreach (var cm in classFile.ClassModels)
                    {
                        // class model prpoperty is a class or enum defined in another file?
                        foreach (var p in cm.Properties)
                        {
                            if (!p.IsNative)
                            {
                                string typeName;
                                
                                if (p.IsList && p.ListType != null)
                                {
                                    typeName = p.ListType.Type;
                                }
                                else if (p.IsArray && p.ArrayType != null)
                                {
                                    typeName = p.ArrayType.Type;
                                }
                                else if (p.IsDictionary && p.DictionaryValueType != null)
                                {
                                    typeName = p.DictionaryValueType.Type;
                                }
                                else
                                {
                                    typeName = p.Type;
                                }

                                p.Class = structures.Where(x => x.Name == typeName).FirstOrDefault();
                                p.Enum = enums.Where(x => x.Name == typeName).FirstOrDefault();

                                var elName = p.Class != null ? p.Class.Name : (p.Enum != null ? p.Enum.Name : null);
                                if (elName != null)
                                {
                                    var importClassFile = structureFiles.FindAll(cf => cf.ClassModels.Where(cm => cm.Name == elName).FirstOrDefault() != null).FirstOrDefault();
                                    if (importClassFile is null)
                                    {
                                        importClassFile = structureFiles.FindAll(cf => cf.EnumModels.Where(em => em.Name == elName).FirstOrDefault() != null).FirstOrDefault();
                                    }
                                    if (importClassFile != null && !object.ReferenceEquals(classFile, importClassFile))
                                    {
                                        var import = classFile.Imports.Where(i => i.FileName == importClassFile.RelativeDestinationFile).FirstOrDefault();
                                        if (import != null)
                                        {
                                            if (!import.Elements.Contains(elName))
                                            {
                                                import.Elements.Add(elName);
                                            }                                            
                                        }
                                        else
                                        {
                                            var i = new Import();
                                            i.FileName = importClassFile.RelativeDestinationFile;
                                            i.Elements.Add(elName);
                                            classFile.Imports.Add(i);
                                        }
                                    }
                                }
                            }
                        }

                        // class model inherits from a class defined in annother file?
                        foreach (var parent in cm.InheritsFrom)
                        {
                            foreach (var importClassFile in structureFiles)
                            {
                                if (!object.ReferenceEquals(cm, importClassFile))
                                {
                                    var icm = importClassFile.ClassModels.Where(f => f.Name == parent.Name).FirstOrDefault();
                                    if (icm != null)
                                    {
                                        parent.Model = icm;
                                        parent.FileName = importClassFile.RelativeDestinationFile;
                                        var elName = parent.Name;
                                        var import = classFile.Imports.Where(i => i.FileName == importClassFile.RelativeDestinationFile).FirstOrDefault();
                                        if (import != null)
                                        {
                                            if (!import.Elements.Contains(elName))
                                            {
                                                import.Elements.Add(elName);
                                            }
                                        }
                                        else
                                        {
                                            var i = new Import();
                                            i.FileName = importClassFile.RelativeDestinationFile;
                                            i.Elements.Add(elName);
                                            classFile.Imports.Add(i);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // write to js files
                foreach (var classFile in structureFiles)
                {
                    //var sb = new System.Text.StringBuilder();
                    var fw = new EsFileWriter();

                    foreach (var i in classFile.Imports)
                    {                        
                        fw.AppendLine($"import {{ {String.Join(", ", i.Elements.ToArray())} }} from '{i.FileName}';");
                    }
                    fw.AppendLine();

                    foreach (var em in classFile.EnumModels)
                    {
                        em.GenerateEs6FileLines(fw);
                    }

                    foreach (var cm in classFile.ClassModels)
                    {
                        cm.GenerateEs6FileLines(fw);
                    }                    
                                       
                    fw.Write(classFile.DestinationFile);

                }

                // save source file write times
                var fileWriteTimes = new Dictionary<string, DateTime>(); 
                foreach (var FileInfo in FileInfos)
                {
                    fileWriteTimes.Add(FileInfo.FullName, FileInfo.LastWriteTime);
                }                               
                File.WriteAllText(fileInfoFile, System.Text.Json.JsonSerializer.Serialize(fileWriteTimes));
                
            }
            catch (Exception e)
            {
                //var ee = e;
                ConsoleException.Write($"ClassFilesParser.Parse(sourceDirectory := {sourceDirectory}, destinationDirectory := {destinationDirectory})", e);
                return 0;
            }

            return 1;
        }
    }
}
