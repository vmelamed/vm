using EnvDTE;
using EnvDTE80;
using System.Collections;
using System.IO;
using System.Linq;

namespace vm.Aspects.Visix.AddRelatedClasses
{
    class CachedItems
    {
        public CachedItems()
        {
            SourceProjectItem = ((IEnumerable)RootCommand.Dte2
                                                         .ToolWindows
                                                         .SolutionExplorer
                                                         .SelectedItems)
                                            .OfType<UIHierarchyItem>()
                                            .FirstOrDefault().Object as ProjectItem;
            if (SourceProjectItem == null)
                return;

            SourceCodeModel = SourceProjectItem.FileCodeModel as FileCodeModel2;

            if (SourceCodeModel?.Language != CodeModelLanguageConstants.vsCMLanguageCSharp)
                return;

            SourcePathName = SourceProjectItem.Document.FullName;

            if (!FindFirstClassOrInterface(SourceCodeModel.CodeElements))
                return;

            TargetPathName    = Path.Combine(
                                        Path.GetDirectoryName(SourcePathName),
                                        Path.GetFileNameWithoutExtension(SourcePathName) + ".Metadata.cs");
        }

        private bool FindFirstClassOrInterface(
            CodeElements codeElements)
        {
            foreach (CodeElement2 element in codeElements)
            {
                SourceClass = element as CodeClass2;

                if (SourceClass != null)
                    return true;

                SourceInterface = element as CodeInterface2;

                if (SourceInterface != null)
                    return true;

                var nameSpace = element as CodeNamespace;

                if (nameSpace != null)
                    if (FindFirstClassOrInterface(nameSpace.Children))
                    {
                        SourceNameSpace = nameSpace;
                        return true;
                    }
            }

            return false;
        }

        public string SourcePathName { get; set; }
        public ProjectItem SourceProjectItem { get; set; }
        public CodeNamespace SourceNameSpace { get; set; }
        public FileCodeModel2 SourceCodeModel { get; set; }
        public CodeClass2 SourceClass { get; set; }
        public CodeInterface2 SourceInterface { get; set; }

        public string TargetPathName { get; set; }

        public bool HasClass
        {
            get { return SourceClass != null; }
        }

        public bool HasInterface
        {
            get { return SourceInterface != null; }
        }
    }
}
