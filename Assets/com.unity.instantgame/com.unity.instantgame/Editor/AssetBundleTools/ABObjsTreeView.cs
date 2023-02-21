#if IG_C106 || IG_C109 || IG_C201 || IG_C301 || IG_C302 // Auto generated by AddMacroForInstantGameFiles.exe

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Linq;
using UnityEngine.Assertions;

namespace Unity.AutoStreaming
{
    internal class ABObjDataItem : TreeDataItem
    {
        public string objName { get; set; }
        public int objInstanceId { get; set; }
        public int objSize { get; set; }
        public string objType { get; set; }
        // The asset which depends on this object. It will be empty for scene AssetBundle.
        public string assetPath { get; set; }
        public int localFileId { get; set; }
        public ABObjDataItem(string inObjName, int depth, int id)
            : base(inObjName, depth, id)
        {
            objName = inObjName;
        }
        public override bool MatchesSearch(string search)
        {
            return objName.Contains(search);
        }
    }

    internal class ABObjsTreeView : TreeViewBaseT<ABObjDataItem>
    {
        enum MyColumns
        {
            ObjectName = 0,
            ObjectType,
            ObjectSize,
            AssetPath
        }
        enum SortOption
        {
            ObjectName = 0,
            ObjectType,
            ObjectSize,
            AssetPath
        }

        SortOption[] m_SortOptions =
        {
            SortOption.ObjectName,
            SortOption.ObjectType,
            SortOption.ObjectSize,
            SortOption.AssetPath,
        };

        public ABObjsTreeView(TreeViewState state, MultiColumnHeader multicolumnHeader, TreeModelT<ABObjDataItem> model) : base(state, multicolumnHeader, model)
        {
            // Custom setup
            rowHeight = k_RowHeights;

            columnIndexForTreeFoldouts = 0;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            customFoldoutYOffset = (k_RowHeights - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI

            multicolumnHeader.sortingChanged += OnSortingChanged;

            Reload();
        }

        protected override void SortByMultipleColumns()
        {
            var sortedColumns = multiColumnHeader.state.sortedColumns;

            if (sortedColumns.Length == 0)
                return;

            var myTypes = rootItem.children.Cast<TreeViewItemBaseT<ABObjDataItem>>();
            var orderedQuery = InitialOrder(myTypes, sortedColumns);
            for (int i = 1; i < sortedColumns.Length; i++)
            {
                SortOption sortOption = m_SortOptions[sortedColumns[i]];
                bool ascending = multiColumnHeader.IsSortedAscending(sortedColumns[i]);

                switch (sortOption)
                {
                    case SortOption.ObjectName:
                        orderedQuery = orderedQuery.ThenBy(l => l.Data.Name, ascending);
                        break;
                    case SortOption.ObjectType:
                        orderedQuery = orderedQuery.ThenBy(l => l.Data.objType, ascending);
                        break;
                    case SortOption.ObjectSize:
                        orderedQuery = orderedQuery.ThenBy(l => l.Data.objSize, ascending);
                        break;
                    case SortOption.AssetPath:
                        orderedQuery = orderedQuery.ThenBy(l => l.Data.assetPath, ascending);
                        break;
                }
            }

            rootItem.children = orderedQuery.Cast<TreeViewItem>().ToList();
        }

        IOrderedEnumerable<TreeViewItemBaseT<ABObjDataItem>> InitialOrder(IEnumerable<TreeViewItemBaseT<ABObjDataItem>> myTypes, int[] history)
        {
            SortOption sortOption = m_SortOptions[history[0]];
            bool ascending = multiColumnHeader.IsSortedAscending(history[0]);
            switch (sortOption)
            {
                case SortOption.ObjectName:
                    return myTypes.Order(l => l.Data.Name, ascending);
                case SortOption.ObjectType:
                    return myTypes.Order(l => l.Data.objType, ascending);
                case SortOption.ObjectSize:
                    return myTypes.Order(l => l.Data.objSize, ascending);
                case SortOption.AssetPath:
                    return myTypes.Order(l => l.Data.assetPath, ascending);
                default:
                    Assert.IsTrue(false, "Unhandled enum");
                    break;
            }

            // default
            return myTypes.Order(l => l.Data.Name, ascending);
        }

        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth)
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Name"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 250,
                    minWidth = 100,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Type"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 150,
                    minWidth = 50,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Size"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 80,
                    minWidth = 80,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Asset"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 450,
                    minWidth = 100,
                    autoResize = false,
                    allowToggleVisibility = false
                },
            };

            var state = new MultiColumnHeaderState(columns);
            return state;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TreeViewItemBaseT<ABObjDataItem>)args.item;

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (MyColumns)args.GetColumn(i), ref args);
            }
        }

        void CellGUI(Rect cellRect, TreeViewItemBaseT<ABObjDataItem> item, MyColumns column, ref RowGUIArgs args)
        {
            // Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch (column)
            {
                case MyColumns.ObjectName:
                {
                    string value = item.Data.objName;
                    DefaultGUI.Label(cellRect, value, args.selected, args.focused);
                }
                break;
                case MyColumns.ObjectType:
                {
                    string value = item.Data.objType;
                    DefaultGUI.Label(cellRect, value, args.selected, args.focused);
                }
                break;
                case MyColumns.ObjectSize:
                {
                    string value = EditorUtility.FormatBytes(item.Data.objSize);
                    DefaultGUI.Label(cellRect, value, args.selected, args.focused);
                }
                break;
                case MyColumns.AssetPath:
                {
                    string value = item.Data.assetPath;
                    DefaultGUI.Label(cellRect, value, args.selected, args.focused);
                }
                break;
            }
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds.Count == 1)
            {
                var dataItem = TreeModel.Find(selectedIds[0]);
                Selection.activeInstanceID = dataItem.objInstanceId;
            }
        }

        protected override bool DoesItemMatchSearch(TreeViewItem item, string search)
        {
            return base.DoesItemMatchSearch(item, search);
        }
    }
}

#endif  // IG_C106 || IG_C109 || IG_C201 || IG_C301 || IG_C302, Auto generated by AddMacroForInstantGameFiles.exe
