//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFrameX.Editor;
using GameFrameX.UGUI.Runtime;
using UnityEditor;

namespace GameFrameX.UGUI.Editor
{
    [CustomEditor(typeof(UGUIComponent))]
    internal sealed class UGUIComponentInspector : ComponentTypeComponentInspector
    {
        protected override void RefreshTypeNames()
        {
            RefreshComponentTypeNames(typeof(IUGUIManager));
        }
    }
}