using System;
using UnityEngine;
using GameFrameX.Runtime;

namespace GameFrameX.UGUI.Runtime
{
    /// <summary>
    /// UGUI组件
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/UGUI")]
    public sealed class UGUIComponent : GameFrameworkComponent
    {
        private IUGUIManager m_uiManager = null;

        protected override void Awake()
        {
            ImplementationComponentType = Type.GetType(componentType);
            InterfaceComponentType      = typeof(IUGUIManager);
            base.Awake();
            m_uiManager = GameFrameworkEntry.GetModule<IUGUIManager>();
            if (m_uiManager == null)
            {
                Log.Fatal("UI manager is invalid.");
                return;
            }
        }

        private void Start()
        {
        }

        public void ShowPanel(string panelName)
        {
            Log.Debug($"ShowPanel:{panelName}");
            m_uiManager.Show(panelName);
        }

        public void HidePanel(string panelName)
        {
            Log.Debug($"HidePanel:{panelName}");
            m_uiManager?.Hide(panelName);
        }
    }
}