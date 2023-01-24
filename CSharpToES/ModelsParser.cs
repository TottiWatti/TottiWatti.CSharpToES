using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TottiWatti.CSharpToES
{
    /// <summary>
    /// Parses .cs file class and enum models using Roslyn code analysis
    /// </summary>
    public static class ModelsParser
    {
        /// <summary>
        /// Parses .cs file class and enum models using Roslyn code analysis
        /// </summary>
        /// <param name="cf">Class file model to store parsed classes and enums</param>
        /// <param name="content">.cs file as string to parse</param>
        public static void Parse(StructureFileModel cf, string content)
        {
            try
            {
                var tree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(content);
                var members = tree.GetRoot().DescendantNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.MemberDeclarationSyntax>();

                // enums
                foreach (var member in members)
                {
                    if (member is Microsoft.CodeAnalysis.CSharp.Syntax.EnumDeclarationSyntax enumDeclaration)
                    {
                        var p = enumDeclaration.ToString().Split("\r\n");
                        if (p.Length > 0 && !p[0].Contains("private"))
                        {
                            var e = new EnumModel();
                            e.Name = enumDeclaration.Identifier.ValueText;

                            if (enumDeclaration.HasLeadingTrivia)
                            {
                                e.TriviaLines.AddRange(_GetXmlCommentTriviaLines(enumDeclaration.GetLeadingTrivia().Select(i => i.GetStructure()).OfType<Microsoft.CodeAnalysis.CSharp.Syntax.DocumentationCommentTriviaSyntax>().FirstOrDefault()));
                            }

                            int enumAutoValue = 0;
                            foreach (var enumMember in enumDeclaration.Members)
                            {
                                var em = new EnumMember();
                                em.Name = enumMember.Identifier.ValueText;
                                if (enumMember.EqualsValue != null)
                                {
                                    var token = enumMember.EqualsValue.Value.GetFirstToken();
                                    if (token.Value != null)
                                    {
                                        em.Value = token.Value;
                                    }
                                }
                                else
                                {
                                    em.Value = enumAutoValue;
                                    enumAutoValue++;
                                }
                                if (enumMember.HasLeadingTrivia)
                                {
                                    em.TriviaLines.AddRange(_GetXmlCommentTriviaLines(enumMember.GetLeadingTrivia().Select(i => i.GetStructure()).OfType<Microsoft.CodeAnalysis.CSharp.Syntax.DocumentationCommentTriviaSyntax>().FirstOrDefault()));
                                }
                                var val = em.Value.ToString();
                                if (val != null)
                                {
                                    e.Values.Add(val);
                                }
                                e.Members.Add(em);
                            }

                            cf.EnumModels.Add(e);
                        }

                    }
                }


                // structures (class, struct, record)
                foreach (var member in members)
                {
                    Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax? declaration = null;

                    if (member is Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax structureDeclaration)
                    {
                        declaration = structureDeclaration;
                    }                    

                    if (declaration != null)
                    {
                        var cls = new StructureModel();
                        cls.Name = declaration.Identifier.ValueText;

                        // inheritance
                        if (declaration.BaseList != null)
                        {
                            var ss = declaration.BaseList.Types.ToString().Split(',');
                            foreach (var t in ss)
                            {
                                var p = new Parent();
                                p.Name = t.Trim();
                                cls.InheritsFrom.Add(p);
                            }
                        }

                        if (declaration.Parent != null && declaration.Parent is Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax)
                        {
                            var ns = (Microsoft.CodeAnalysis.CSharp.Syntax.NamespaceDeclarationSyntax)declaration.Parent;
                            cls.Namespace = ns.Name.ToString();
                        }

                        if (declaration.HasLeadingTrivia)
                        {
                            cls.TriviaLines.AddRange(_GetXmlCommentTriviaLines(declaration.GetLeadingTrivia().Select(i => i.GetStructure()).OfType<Microsoft.CodeAnalysis.CSharp.Syntax.DocumentationCommentTriviaSyntax>().FirstOrDefault()));
                        }

                        cf.ClassModels.Add(cls);
                    }
                }               

                // structure methods and properties
                foreach (var member in members)
                {
                    if (member is Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax property)
                    {
                        if (property.Modifiers.ToString().Contains("public"))
                        {
                            var p = new StructureProperty();
                            p.Name = property.Identifier.ValueText;
                            p.Type = property.Type.ToString().Replace("?", "").Trim();
                            p.IsNullable = property.Type.Kind().ToString().Contains("Nullable");

                            if (property.HasLeadingTrivia)
                            {
                                p.TriviaLines.AddRange(_GetXmlCommentTriviaLines(property.GetLeadingTrivia().Select(i => i.GetStructure()).OfType<Microsoft.CodeAnalysis.CSharp.Syntax.DocumentationCommentTriviaSyntax>().FirstOrDefault()));
                            }

                            if (property.AttributeLists.Count > 0)
                            {
                                foreach (Microsoft.CodeAnalysis.CSharp.Syntax.AttributeListSyntax atrls in property.AttributeLists)
                                {
                                    var s = atrls.ToString();

                                    if (s.StartsWith("[Range("))
                                    {
                                        var ss = s.Split('(', ',', ')');
                                        if (ss.Length > 2)
                                        {
                                            p.HasRange = true;
                                            p.RangeMin = ss[1];
                                            p.RangeMax = ss[2];
                                        }

                                    }

                                }
                            }

                            if (property.Initializer != null)
                            {
                                p.InitialValue = _GetInitialValue(p, property.Initializer.Value);
                            }

                            // find parent class and ad to it's properties
                            if (property.Parent != null && property.Parent is Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax)
                            {
                                Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax strucureDec = (Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax)property.Parent;
                                var cls = cf.ClassModels.Where(c => c.Name == strucureDec.Identifier.ValueText).FirstOrDefault();
                                if (cls != null)
                                {
                                    cls.Properties.Add(p);
                                }
                            }
                        }
                    }

                    if (member is Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax field)
                    {
                        if (field.Modifiers.ToString().Contains("public"))
                        {
                            if (field.Declaration.Variables.Count > 0)
                            {
                                var dec = field.Declaration.Variables[0];
                                if (dec != null)
                                {
                                    var p = new StructureProperty();
                                    p.Name = dec.Identifier.ValueText;
                                    p.Type = field.Declaration.Type.ToString().Replace("?", "").Trim();
                                    p.IsNullable = field.Declaration.Type.Kind().ToString().Contains("Nullable");

                                    var var1 = field.Declaration.Variables[0];

                                    if (field.HasLeadingTrivia)
                                    {
                                        p.TriviaLines.AddRange(_GetXmlCommentTriviaLines(field.GetLeadingTrivia().Select(i => i.GetStructure()).OfType<Microsoft.CodeAnalysis.CSharp.Syntax.DocumentationCommentTriviaSyntax>().FirstOrDefault()));
                                    }

                                    if (dec.Initializer != null)
                                    {
                                        p.InitialValue = _GetInitialValue(p, dec.Initializer.Value);
                                    }

                                    // find parent class and ad to it's properties
                                    if (field.Parent != null && field.Parent is Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax)
                                    {
                                        Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax clsDec = (Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax)field.Parent;
                                        var cls = cf.ClassModels.Where(c => c.Name == clsDec.Identifier.ValueText).FirstOrDefault();
                                        if (cls != null)
                                        {
                                            cls.Properties.Add(p);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

                ConsoleException.Write($"ModelsParser.Parse(cf := {cf}, content := {content})", e);
            }            
        }

        private static List<string> _GetXmlCommentTriviaLines(Microsoft.CodeAnalysis.CSharp.Syntax.DocumentationCommentTriviaSyntax? xmlTrivia)
        {
            try
            {

                if (xmlTrivia != null)
                {
                    var xmlElements = xmlTrivia!.ChildNodes().OfType<Microsoft.CodeAnalysis.CSharp.Syntax.XmlElementSyntax>();
                    if (xmlElements != null)
                    {
                        foreach (var xmlElement in xmlElements)
                        {
                            if (xmlElement.StartTag.ToString() == "<summary>")
                            {
                                var xmlElementSummaryLines = xmlElement.Content.ToString().Split("///").Select(line => line.Trim()).ToList();
                                xmlElementSummaryLines!.RemoveAll(line => String.IsNullOrEmpty(line));
                                if (xmlElementSummaryLines != null)
                                {
                                    return xmlElementSummaryLines;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                var ee = e;
            }
            return new List<string>();

        }

        private static string _GetInitialValue(StructureProperty p, Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionSyntax val)
        {
            try
            {
                if (val != null)
                {
                    var s = val.ToString();                    

                    // some values have characters unnessary here, get value string from token
                    if (val.Kind() == Microsoft.CodeAnalysis.CSharp.SyntaxKind.NumericLiteralExpression || val.Kind() == Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralExpression)
                    {
                        s =  val.GetFirstToken().ValueText;
                    }

                    if (s.EndsWith(".MinValue") && p.TypeMin != null)
                    {
                        return p.TypeMin;
                    }
                    
                    if (s.EndsWith(".MaxValue") && p.TypeMax != null)
                    {
                        return p.TypeMax;
                    }

                    if (p.Type == "bool")
                    {
                        return s;
                    }
                    
                    if (p.Type == "string")
                    {
                        return "'" + s + "'";
                    }
                    
                    if (p.Type == "DateTime" || p.Type == "System.DateTime")
                    {                        
                        if (s.EndsWith("Now")) return "new Date()";

                        var ni = s.IndexOf("new");
                        var pi = s.IndexOf("Parse");
                        var asi = s.LastIndexOf("(");
                        var aei = s.LastIndexOf(")");                        
                        if (ni >= 0 && asi >= 0 && aei >= 0 && ni < asi && asi < aei)
                        {
                            var di = s.Substring(asi+1,aei-asi-1);
                            if (!String.IsNullOrEmpty(di))
                            {
                                return $"new Date({di})";
                            }                            
                        }
                        if (pi >= 0 && asi >= 0 && aei >= 0 && pi < asi && asi < aei)
                        {
                            var di = s.Substring(asi + 1, aei - asi - 1);
                            if (!String.IsNullOrEmpty(di))
                            {
                                return $"new Date({di})";
                            }
                        }
                    }                  

                    if(p.IsArray)
                    {
                        var sbsi = s.IndexOf('[');
                        var sbse = s.IndexOf(']');                       
                        if (sbsi != -1 && sbse != -1 && sbse-sbsi > 1)
                        {                            
                            string[]? ais = s.Substring(sbsi+1, sbse-sbsi-1).Split(',');
                            if (ais != null && ais.Length > 0) {                                
                                p.ArrayDimensions.AddRange(ais.Select(ai => int.Parse(ai)).ToArray());
                            }                            
                        }
                        if (p.ArrayDimensions.Count > 0)
                        {
                            var count = s.Count(c => c == '[');
                            if (count > p.ArrayDimensions.Count)
                            {
                                // jagged array
                                p.ArrayDimensions.Add(0);
                            }
                        }
                        
                    }

                    if (p.IsList || p.IsArray)
                    {
                        var si = s.IndexOf('{');
                        var ei = s.LastIndexOf('}');
                        if (si != -1 && ei != -1 && ei-si > 0)
                        {
                            return s.Substring(si, ei-si+1).Replace('{', '[').Replace('}', ']').Replace('"', '\'').Replace(" ", String.Empty);
                        }
                        else
                        {                           
                            return "[]";
                        }
                    }                    

                    return s;
                }
            }
            catch (Exception e)
            {
                var ee = e; ;
            }
            return "";
        }        
    }
}
