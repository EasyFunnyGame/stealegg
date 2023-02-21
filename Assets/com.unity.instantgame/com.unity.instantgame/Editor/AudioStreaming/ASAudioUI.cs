#if IG_C106 || IG_C109 || IG_C201 || IG_C301 || IG_C302 // Auto generated by AddMacroForInstantGameFiles.exe

using System.Linq;
using UnityEngine;
using UnityEditor.IMGUI.Controls;
using UnityEditor;

namespace Unity.AutoStreaming
{
    internal class ASAudioUI : TabBase<ASAudioTreeView, ASAudioTreeDataItem>
    {
        protected override MultiColumnHeaderState CreateColumnHeaderState(float treeViewWidth)
        {
            return ASAudioTreeView.CreateDefaultMultiColumnHeaderState(treeViewWidth);
        }

        protected override void InitTreeView(MultiColumnHeader multiColumnHeader)
        {
            var treeModel = new TreeModelT<ASAudioTreeDataItem>(ASMainWindow.Instance.AudioData);
            m_TreeView = new ASAudioTreeView(m_TreeViewState, multiColumnHeader, treeModel);
        }

        protected override void OnToolbarGUI(Rect rect)
        {
            GUILayout.BeginArea(rect);

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(5);

                if (GUILayout.Button("Sync Audios", s_MiniButton, GUILayout.Width(100)))
                {
                    SyncAudios();
                }

                GUILayout.Space(5);
                m_TreeView.searchString = m_SearchField.OnToolbarGUI(m_TreeView.searchString, GUILayout.Width(200));

                GUILayout.FlexibleSpace();

                string statusReport = "";
                var allAudios = AutoStreamingSettings.audios;
                var onDemandDownloadItems = allAudios.Where(x => x.onDemandDownload);
                statusReport = string.Format("Placeholder: {0}/{1}, RT: {2}",
                    onDemandDownloadItems.Count(),
                    allAudios.Length,
                    EditorUtility.FormatBytes(onDemandDownloadItems.Select(x => (long)x.runtimeMemory).Sum()));
                GUILayout.Label(statusReport);
            }
            GUILayout.EndArea();
        }

        void SyncAudios()
        {
            ASUtilities.GenerateAddressablePathsText();
            AutoStreamingSettings.SyncAudios();
            ASMainWindow.Instance.AudioData = null;
            m_TreeViewInitialized = false;
        }
    }
}

#endif  // IG_C106 || IG_C109 || IG_C201 || IG_C301 || IG_C302, Auto generated by AddMacroForInstantGameFiles.exe
