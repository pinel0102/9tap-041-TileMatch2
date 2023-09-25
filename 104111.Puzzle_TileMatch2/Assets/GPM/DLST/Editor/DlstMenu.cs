using UnityEditor;

namespace Gpm.Dlst
{
    public static class DlstMenu
    {
        [MenuItem("Tools/GPM/Duplicate Library Search Tool(DLST)")]
        private static void LibrarySearchTool()
        {
            DlstWindow.ShowWindow();
        }
    }
}