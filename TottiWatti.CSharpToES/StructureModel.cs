using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TottiWatti.CSharpToES
{   
    /// <summary>
    /// C# class model
    /// </summary>
    public class StructureModel
    {
        /// <summary>
        /// Class name
        /// </summary>
        public string Name { get; set; } = "";
        
        /// <summary>
        /// Other classes this class inherits from. Note! ES supports only single inheritance so multiple inheritance is not supported
        /// </summary>
        public List<Parent> InheritsFrom = new List<Parent>();

        /// <summary>
        /// Namespace of class
        /// </summary>
        public string Namespace { get; set; } = "";

        /// <summary>
        /// Class comment trivia
        /// </summary>
        public List<string> TriviaLines { get; set; } = new List<string>();        

        /// <summary>
        /// Class public properties and fields models
        /// </summary>
        public List<StructureProperty> Properties { get; set; } = new List<StructureProperty>();          

        /// <summary>
        /// Writes class model in ES format
        /// </summary>
        /// <param name="sb">Stringbuilder instance to write to</param>
        /// <param name="tabs">Initial tabulator at point of destination file class model is written to</param>
        public void GenerateEs6FileLines(EsFileWriter fw) 
        {            
            try
            {
                // class trivia
                fw.AppendTrivia(this.TriviaLines);

                // class name
                string extends = "";
                if (this.InheritsFrom.Count > 0)
                {
                    extends = $"extends {this.InheritsFrom[0].Name}";
                }
                fw.AppendLine($"export class {this.Name} {extends}{{");
                fw.AppendLine();
                fw.Indent(); //tabs++;

                // privates                
                fw.AppendLine("// private values");
                foreach (var p in this.Properties)
                {                    
                    fw.AppendLine($"/** @type {{{p.JsType}}} */ #{p.Name};");
                }
                fw.AppendLine();

                // constructor
                fw.AppendTrivia(this.TriviaLines);
                fw.AppendLine($"constructor() {{");
                fw.Indent();
                if (this.InheritsFrom.Count > 0)
                {                       
                    fw.AppendLine("super();");
                }
                foreach (var p in this.Properties)
                {
                    if (p.IsDictionary)
                    {
                        fw.AppendLine($"this.#{p.Name} = new Map();");
                    }
                    else if (p.IsList || p.IsArray)
                    {
                        fw.AppendLine($"this.#{p.Name} = {(!String.IsNullOrEmpty(p.InitialValue) ? p.InitialValue.ToString() : "[]")};");
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(p.InitialValue)) 
                        {
                            fw.AppendLine($"this.#{p.Name} = {p.InitialValue};");
                        }
                        else if (!String.IsNullOrEmpty(p.BaseTypeDefaultValue))
                        {
                            fw.AppendLine($"this.#{p.Name} = {p.BaseTypeDefaultValue};");
                        }                        
                    }
                }
                fw.Outdent();
                fw.AppendLine("}");
                fw.AppendLine();

                // property getters and setters
                foreach (var p in this.Properties)
                {
                    var trivia = new List<string>();
                    trivia.AddRange(p.TriviaLines);
                    string pType = p.Type;
                    if (p.IsArray && p.ArrayDimensions.Count > 0 && p.ArrayType != null)
                    {                       
                        var dims = p.ArrayDimensions.Where(dim => dim != 0);
                        pType = $"{p.ArrayType.Type}[{String.Join(',',dims)}]";
                        if (p.ArrayDimensions.Count() > dims.Count())
                        {
                            // jagged array
                            pType += "[]";
                        }
                    }
                    
                    var typeSpecifier = "";
                    var range = "";
                    if (p.Class is not null)
                    {
                        typeSpecifier = "class ";
                    }
                    if (p.Enum is not null)
                    {
                        typeSpecifier = "enum ";
                        range = $" values [{String.Join(",",p.Enum.Values)}]";
                    }                   
                    if (p.IsNumeric)
                    {
                        if (p.HasRange)
                        {
                            range = $" custom range {p.RangeMin} ... {p.RangeMax}";
                        }
                        else
                        {
                            range = $" range {p.TypeMin} ... {p.TypeMax}";
                        }
                        
                    }                   

                    trivia.Add($"Server type {(p.IsNullable ? "nullable " : "")}{typeSpecifier}'{pType}'{range}");
                    trivia.Add($"@type {{{p.JsType}}}");
                    fw.AppendTrivia(trivia);
                    fw.AppendLine($"get {p.Name}() {{");
                    fw.Indent();
                    fw.AppendLine($"return this.#{p.Name};");
                    fw.Outdent();
                    fw.AppendLine("}");
                    fw.AppendLine($"set {p.Name}(val) {{");
                    fw.Indent();

                    string setter = $"this.#{p.Name} = val;"; 

                    if ((p.IsNumeric && (p.Type != "double" && p.Type != "decimal") || p.HasRange) && p.Enum is null)
                    {
                        var min = p.HasRange ? p.RangeMin : p.TypeMin;
                        var max = p.HasRange ? p.RangeMax : p.TypeMax; ;
                        setter = $"this.#{p.Name} = (val < {min} ? {min} : (val > {max} ? {max} : {(p.IsInteger ? "Math.round(val)" : "val")}))";
                    }

                    if (p.IsNullable)
                    {
                        fw.AppendLine("if (val == null) {");
                        fw.Indent();
                        fw.AppendLine($"this.#{p.Name} = null;");
                        fw.Outdent();
                        fw.AppendLine("}");
                        fw.AppendLine("else {");
                        fw.Indent();
                    }

                    if (p.IsDateTime)
                    {
                        fw.AppendLine($"if (val instanceof Date) {{");
                    }
                    else if (p.Enum is not null)
                    {
                        fw.AppendLine($"if ([{String.Join(",", p.Enum.Values)}].includes(val)) {{");
                    }
                    else if (p.IsArray || p.IsList)
                    {
                        fw.AppendLine($"if (Array.isArray(val)) {{");
                    }
                    else if (p.IsDictionary)
                    {
                        fw.AppendLine($"if (val instanceof Map) {{");
                    }                    
                    else if (!p.IsNative && p.Enum is null)
                    {
                        fw.AppendLine($"if (val instanceof {p.Type}) {{");
                    }
                    else
                    {
                        if (p.IsNative || p.JsType == "object")
                        {
                            fw.AppendLine($"if (typeof val === '{p.JsType}') {{");
                        }                        
                    }

                    fw.Indent();
                    fw.AppendLine(setter);
                    fw.Outdent();
                    fw.AppendLine("}");

                    fw.Outdent();
                    fw.AppendLine("}");                    

                    if (p.IsNullable)
                    {
                        fw.Outdent();
                        fw.AppendLine("}");
                    }

                    fw.AppendLine();
                }                

                // json serializer
                fw.AppendTrivia(new List<string>() {$"{this.Name} JSON serializer. Called automatically by JSON.stringify()." });
                fw.AppendLine("toJSON() {");
                fw.Indent();
                if(this.InheritsFrom.Count > 0)
                {
                    fw.AppendLine("return Object.assign(super.toJSON(), {");
                }
                else
                {
                    fw.AppendLine("return {");
                }
                fw.Indent();
                for (int i=0; i<this.Properties.Count; i++)
                {
                    var p = this.Properties[i];
                    if (p.IsDictionary)
                    {
                        fw.AppendLine($"\'{p.Name}\': Object.fromEntries(this.#{p.Name}){(i != this.Properties.Count-1 ? "," : "")}");
                    }
                    else
                    {
                        fw.AppendLine($"\'{p.Name}\': this.#{p.Name}{(i != this.Properties.Count-1 ? "," : "")}");
                    }                    
                }
                fw.Outdent();
                if (this.InheritsFrom.Count > 0)
                {
                    fw.AppendLine("})");
                }
                else
                {
                    fw.AppendLine("}");
                }
                fw.Outdent();
                fw.AppendLine("}");
                fw.AppendLine();

                // json deserializer
                fw.AppendTrivia(new List<string>() {
                    $"Deserializes json to instance of {this.Name}.",
                    $"@param {{string}} json json serialized {this.Name} instance",
                    $"@returns {{{this.Name}}} deserialized {this.Name} class instance"
                });
                fw.AppendLine("static fromJSON(json) {");
                fw.Indent();
                fw.AppendLine("let o = JSON.parse(json);");
                fw.AppendLine($"return {this.Name}.fromObject(o);");
                fw.Outdent();
                fw.AppendLine("}");
                fw.AppendLine();

                // object mapper object -> class instance
                fw.AppendTrivia(new List<string>() { 
                    $"Maps object to instance of {this.Name}.",
                    $"@param {{object}} o object to map instance of {this.Name} from",
                    $"@returns {{{this.Name}}} mapped {this.Name} class instance"
                });
                fw.AppendLine("static fromObject(o) {");
                fw.Indent();
                fw.AppendLine("if (o != null) {");
                fw.Indent();
                fw.AppendLine($"let val = new {this.Name}();");
                                
                if (this.InheritsFrom.Count > 0)
                {
                    fw.AppendLine($"// values from super '{this.InheritsFrom[0].Name}'");
                    var im = this.InheritsFrom[0].Model;
                    if (im != null)
                    {
                        foreach (var p in im.Properties)
                        {
                            _AppendEs6PropertyDeserializer(p, "val", fw); 
                        }                        
                    }
                    fw.AppendLine($"// values from '{this.Name}'");
                }                

                foreach (var p in this.Properties)
                {
                    _AppendEs6PropertyDeserializer(p, "val", fw); 
                }
                fw.AppendLine("return val;");
                fw.Outdent();
                fw.AppendLine("}");
                fw.AppendLine("return null;");
                fw.Outdent();
                fw.AppendLine("}");
                fw.AppendLine();

                // json array deserializer
                fw.AppendTrivia(new List<string>() {
                    $"Deserializes json to array of {this.Name}.",
                    $"@param {{string}} json json serialized {this.Name} array",
                    $"@returns {{{this.Name}[]}} deserialized {this.Name} array"
                });
                fw.AppendLine("static fromJSONArray(json) {");
                fw.Indent();
                fw.AppendLine("let arr = JSON.parse(json);");
                fw.AppendLine($"return {this.Name}.fromObjectArray(arr);");
                fw.Outdent();
                fw.AppendLine("}");
                fw.AppendLine();

                // object mapper object[] -> instance[]
                fw.AppendTrivia(new List<string>() {
                    $"Maps array of objects to array of {this.Name}.",
                    $"@param {{object[]}} arr object array to map WeatherForecast array from",
                    $"@returns {{{this.Name}[]}} mapped {this.Name} array"
                });
                fw.AppendLine("static fromObjectArray(arr) {");
                fw.Indent();
                fw.AppendLine("if (arr != null) {");
                fw.Indent();
                fw.AppendLine($"let /** @type {{{this.Name}[]}} */ val = [];");
                fw.AppendLine($"arr.forEach(function (f) {{ val.push({this.Name}.fromObject(f)); }});");
                fw.AppendLine("return val;");
                fw.Outdent();
                fw.AppendLine("}");
                fw.AppendLine("return null;");
                fw.Outdent();
                fw.AppendLine("}");
                fw.AppendLine();

                // class close
                fw.Outdent();
                fw.AppendLine("}");
            }
            catch (Exception e)
            {
                //throw;
                //var ee = e;
                ConsoleException.Write($"ClassModel.GenerateEs6FileLines(fw := {fw})", e);
            }            
        }        

        private void _AppendEs6PropertyDeserializer(StructureProperty p, string jsVarName, EsFileWriter fw) //System.Text.StringBuilder sb, int tabs)
        {
            if (p.IsDateTime)
            {
                fw.AppendLine($"if (o.hasOwnProperty('{p.Name}')) {{ {jsVarName}.{p.Name} = new Date(o.{p.Name}); }}");
            }
            else if (p.IsDictionary)
            {                
                fw.AppendLine($"if (o.hasOwnProperty('{p.Name}')) {{");
                fw.Indent();
                if (p.IsNullable)
                {
                    fw.AppendLine($"if (o.{p.Name} == null) {{ {jsVarName}.{p.Name} = null; }}");
                    fw.AppendLine("else {");
                    fw.Indent();
                }
                fw.AppendLine($"for (const entry of Object.entries(o.{p.Name})) {{ ");
                fw.Indent();
                if (p.DictionaryValueType != null)
                {
                    if (p.DictionaryValueType.IsNative)
                    {
                        fw.AppendLine($"val.{p.Name}.set(entry[0], entry[1]);");
                    }
                    else
                    {
                        fw.AppendLine($"val.{p.Name}.set(entry[0], {p.DictionaryValueType.Type}.fromObject(entry[1]));");
                    }
                }
                fw.Outdent();
                fw.AppendLine("}");
                if (p.IsNullable)
                {
                    fw.Outdent();
                    fw.AppendLine("}");
                }
                fw.Outdent();
                fw.AppendLine("}");
            }
            else if (p.Class is not null)
            {
                if (p.IsList && p.ListType != null)
                {
                    fw.AppendLine($"if (o.hasOwnProperty('{p.Name}')) {{ {jsVarName}.{p.Name} = {p.ListType.Type}.fromObjectArray(o.{p.Name}); }}");
                }
                else if (p.IsArray && p.ArrayType != null)
                {
                    fw.AppendLine($"if (o.hasOwnProperty('{p.Name}')) {{ {jsVarName}.{p.Name} = {p.ArrayType.Type}.fromObjectArray(o.{p.Name}); }}");
                }
                else
                {
                    fw.AppendLine($"if (o.hasOwnProperty('{p.Name}')) {{ {jsVarName}.{p.Name} = {p.Type}.fromObject(o.{p.Name}); }}");
                }                
            }
            else
            {
                fw.AppendLine($"if (o.hasOwnProperty('{p.Name}')) {{ {jsVarName}.{p.Name} = o.{p.Name}; }}");
            }
        }       

    }
    
}
