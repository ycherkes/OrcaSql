using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using ICSharpCode.TextEditor.Document;

namespace OrcaSql.OSMS.SQLEditor
{
    public class AppSyntaxModeProvider : ISyntaxModeFileProvider
    {
        private readonly List<SyntaxMode> _syntaxModes;

        public ICollection<SyntaxMode> SyntaxModes => _syntaxModes;

        public AppSyntaxModeProvider()
        {
            var assembly = Assembly.GetExecutingAssembly();

            //enumerate resource names if need
            //foreach (string resourceName in assembly.GetManifestResourceNames()){}

            //load modes list
            using (var syntaxModeStream = assembly.GetManifestResourceStream("OrcaSql.OSMS.SQLEditor.Resources.SyntaxModes.xml"))
            {
                _syntaxModes = syntaxModeStream != null ? SyntaxMode.GetSyntaxModes(syntaxModeStream) : new List<SyntaxMode>();
            }
        }

        public XmlTextReader GetSyntaxModeFile(SyntaxMode syntaxMode)
        {
            var assembly = Assembly.GetExecutingAssembly();

            // load syntax schema
            var stream = assembly.GetManifestResourceStream("OrcaSql.OSMS.SQLEditor.Resources." + syntaxMode.FileName);
            return new XmlTextReader(stream);
        }

        public void UpdateSyntaxModeList()
        {
            // resources don't change during runtime
        }
    }
}
