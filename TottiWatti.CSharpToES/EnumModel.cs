using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TottiWatti.CSharpToES
{
    /// <summary>
    /// C# enum model
    /// </summary>
    public class EnumModel
    {
        /// <summary>
        /// Enum name
        /// </summary>
        public string Name { get; set; } = "";  

        /// <summary>
        /// Comments
        /// </summary>
        public List<string> TriviaLines { get; set; } = new List<string>();

        /// <summary>
        /// Enum members
        /// </summary>
        public List<EnumMember> Members { get; set; } = new List<EnumMember>();

        /// <summary>
        /// Enum member values as list
        /// </summary>
        public List<string> Values { get; set; } = new List<string>();

        /// <summary>
        /// Writes enum model to ES format
        /// </summary>
        /// <param name="sb">Stringbuilder to write ES model to</param>
        /// <param name="tabs">Intiatial tabulator in point of ES file enum model is written to</param>
        public void GenerateEs6FileLines(EsFileWriter fw)
        {
            try
            {     
                var enumTrivia = new List<string>();
                enumTrivia.AddRange(this.TriviaLines);
                enumTrivia.Add("@readonly");
                enumTrivia.Add("@enum {number}");
                foreach(EnumMember em in this.Members)
                {
                    string oneLineTrivia = ""; 
                    if (em.TriviaLines.Count > 0)
                    {
                        oneLineTrivia = string.Join(" ", em.TriviaLines);
                    }
                    enumTrivia.Add($"@property {{number}} {em.Name} {oneLineTrivia}");
                }
                
                fw.AppendTrivia(enumTrivia);
                fw.AppendLine($"export const {this.Name} = {{");
                fw.Indent(); //tabs++;
                for (int i = 0; i < this.Members.Count; i++)
                {
                    EnumMember em = this.Members[i];
                    fw.AppendTrivia(em.TriviaLines);
                    fw.AppendLine($"{em.Name}: {em.Value}{(i < this.Members.Count -1 ? "," : "")}");
                }
                fw.Outdent();
                fw.AppendLine("}");
                fw.AppendLine();
            }
            catch (Exception e)
            {
                //var ee = e;
                ConsoleException.Write($"EnumModel.GenerateEs6FileLines(fw := {fw})", e);
                //throw;
            }
        }        
    }

    /// <summary>
    /// Enum memner model
    /// </summary>
    public class EnumMember
    {
        /// <summary>
        /// Enum member name
        /// </summary>
        public string Name { get; set; } = "";        

        /// <summary>
        /// Enum member comments
        /// </summary>
        public List<string> TriviaLines { get; set; } = new List<string>();

        /// <summary>
        /// Ebnum member value
        /// </summary>
        public object Value { get; set; } = 0;
    }
}
