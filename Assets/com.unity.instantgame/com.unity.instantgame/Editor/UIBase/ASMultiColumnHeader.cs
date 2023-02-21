#if IG_C106 || IG_C109 || IG_C201 || IG_C301 || IG_C302 // Auto generated by AddMacroForInstantGameFiles.exe

using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Unity.AutoStreaming
{
    internal class ASMultiColumnHeader : MultiColumnHeader
    {
        public enum Mode
        {
            LargeHeader,
            DefaultHeader,
            MinimumHeaderWithoutSorting
        }

        Mode m_Mode;

        public Mode HeaderMode
        {
            get
            {
                return m_Mode;
            }
            set
            {
                m_Mode = value;
                switch (m_Mode)
                {
                    case Mode.LargeHeader:
                        canSort = true;
                        height = 37f;
                        break;
                    case Mode.DefaultHeader:
                        canSort = true;
                        height = DefaultGUI.defaultHeight;
                        break;
                    case Mode.MinimumHeaderWithoutSorting:
                        canSort = false;
                        height = DefaultGUI.minimumHeight;
                        break;
                }
            }
        }

        public ASMultiColumnHeader(MultiColumnHeaderState state)
            : base(state)
        {
            HeaderMode = Mode.DefaultHeader;
        }

        protected override void ColumnHeaderGUI(MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex)
        {
            // Default column header gui
            base.ColumnHeaderGUI(column, headerRect, columnIndex);

            // Add additional info for large header
            if (m_Mode == Mode.LargeHeader)
            {
                // Show example overlay stuff on some of the columns
                if (columnIndex > 2)
                {
                    headerRect.xMax -= 3f;
                    var oldAlignment = EditorStyles.largeLabel.alignment;
                    EditorStyles.largeLabel.alignment = TextAnchor.UpperRight;
                    GUI.Label(headerRect, 36 + columnIndex + "%", EditorStyles.largeLabel);
                    EditorStyles.largeLabel.alignment = oldAlignment;
                }
            }
        }
    }
}

#endif  // IG_C106 || IG_C109 || IG_C201 || IG_C301 || IG_C302, Auto generated by AddMacroForInstantGameFiles.exe
