using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using GameFrameX.Asset.Runtime;
using UnityEngine;
using GameFrameX.Runtime;
using Object = UnityEngine.Object;

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
        private IAssetManager m_assetManager;
        private UGUI _root;
        private UGUI HiddenRoot;
        private UGUI FloorRoot;
        private UGUI NormalRoot;
        private UGUI FixedRoot;
        private UGUI WindowRoot;
        private UGUI TipRoot;
        private UGUI BlackBoardRoot;
        private UGUI DialogueRoot;
        private UGUI GuideRoot;
        private UGUI LoadingRoot;
        private UGUI NotifyRoot;
        private UGUI SystemRoot;
        private readonly Dictionary<UGUILayer, Dictionary<string, UGUI>> _dictionary = new Dictionary<UGUILayer, Dictionary<string, UGUI>>(16);

        private void Start()
        {
            m_assetManager = GameFrameworkEntry.GetModule<IAssetManager>();
            if (m_assetManager == null)
            {
                Log.Fatal("Asset manager is invalid.");
                return;
            }
        }

        protected override void Awake()
        {
            ImplementationComponentType = Type.GetType(componentType);
            InterfaceComponentType = typeof(IUGUIManager);
            base.Awake();
            m_uiManager = GameFrameworkEntry.GetModule<IUGUIManager>();
            if (m_uiManager == null)
            {
                Log.Fatal("UI manager is invalid.");
                return;
            }


            _root = new UGUI(gameObject, null, true);
            _root.Show();
            HiddenRoot = CreateNode(_root.Transform, UGUILayer.Hidden);
            FloorRoot = CreateNode(_root.Transform, UGUILayer.Floor);
            NormalRoot = CreateNode(_root.Transform, UGUILayer.Normal);
            FixedRoot = CreateNode(_root.Transform, UGUILayer.Fixed);
            WindowRoot = CreateNode(_root.Transform, UGUILayer.Window);
            TipRoot = CreateNode(_root.Transform, UGUILayer.Tip);
            BlackBoardRoot = CreateNode(_root.Transform, UGUILayer.BlackBoard);
            DialogueRoot = CreateNode(_root.Transform, UGUILayer.Dialogue);
            GuideRoot = CreateNode(_root.Transform, UGUILayer.Guide);
            LoadingRoot = CreateNode(_root.Transform, UGUILayer.Loading);
            NotifyRoot = CreateNode(_root.Transform, UGUILayer.Notify);
            SystemRoot = CreateNode(_root.Transform, UGUILayer.System);


            _dictionary[UGUILayer.Hidden] = new Dictionary<string, UGUI>(64);
            _dictionary[UGUILayer.Floor] = new Dictionary<string, UGUI>(64);
            _dictionary[UGUILayer.Normal] = new Dictionary<string, UGUI>(64);
            _dictionary[UGUILayer.Fixed] = new Dictionary<string, UGUI>(64);
            _dictionary[UGUILayer.Window] = new Dictionary<string, UGUI>(64);
            _dictionary[UGUILayer.Tip] = new Dictionary<string, UGUI>(64);
            _dictionary[UGUILayer.BlackBoard] = new Dictionary<string, UGUI>(64);
            _dictionary[UGUILayer.Dialogue] = new Dictionary<string, UGUI>(64);
            _dictionary[UGUILayer.Guide] = new Dictionary<string, UGUI>(64);
            _dictionary[UGUILayer.Loading] = new Dictionary<string, UGUI>(64);
            _dictionary[UGUILayer.Notify] = new Dictionary<string, UGUI>(64);
            _dictionary[UGUILayer.System] = new Dictionary<string, UGUI>(64);
        }

        UGUI CreateNode(Transform root, UGUILayer layer)
        {
            GameObject component = new GameObject();
            var ui = new UGUI(component, null, true);
            component.transform.SetParent(root, true);
            component.transform.AddLocalPositionZ((int)layer * 1000);
            component.transform.localScale = Vector3.one;

            component.SetLayerRecursively(LayerMask.NameToLayer("UI"));

            var canvasRenderer = component.GetOrAddComponent<CanvasRenderer>();
            canvasRenderer.cullTransparentMesh = true;

            var canvasGroup = component.GetOrAddComponent<CanvasGroup>();
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            var comName = layer.ToString();

            component.name = comName;
            ui.MakeFullScreen();
            ui.Show();
            return ui;
        }


        /// <summary>
        /// 添加全屏UI对象
        /// </summary>
        /// <param name="descFilePath">UI目录</param>
        /// <param name="layer">目标层级</param>
        /// <param name="userData">用户自定义数据</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>返回创建后的UI对象</returns>
        public T AddToFullScreen<T>(string descFilePath, UGUILayer layer, object userData = null) where T : UGUI
        {
            return Add<T>(descFilePath, layer, true, userData);
        }

        /// <summary>
        /// 异步添加UI 对象
        /// </summary>
        /// <param name="descFilePath">UI目录</param>
        /// <param name="layer">目标层级</param>
        /// <param name="isFullScreen">是否全屏</param>
        /// <param name="userData">用户自定义数据</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>返回创建后的UI对象</returns>
        public async UniTask<T> AddAsync<T>(string descFilePath, UGUILayer layer, bool isFullScreen = false, object userData = null) where T : UGUI
        {
            GameFrameworkGuard.NotNull(descFilePath, nameof(descFilePath));
            var assetHandle = await m_assetManager.LoadAssetAsync<Object>(descFilePath);
            var obj = assetHandle.InstantiateSync();
            T ui = (T)new UGUI(obj, userData);
            Add(ui, layer);
            if (isFullScreen)
            {
                ui.MakeFullScreen();
            }

            return ui;
        }

        /// <summary>
        /// 添加UI对象
        /// </summary>
        /// <param name="creator">UI创建器</param>
        /// <param name="descFilePath">UI目录</param>
        /// <param name="layer">目标层级</param>
        /// <param name="isFullScreen">是否全屏</param>
        /// <param name="userData">用户自定义数据</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>返回创建后的UI对象</returns>
        /// <exception cref="ArgumentNullException">创建器不存在,引发参数异常</exception>
        public T Add<T>(string descFilePath, UGUILayer layer, bool isFullScreen = false, object userData = null) where T : UGUI
        {
            GameFrameworkGuard.NotNull(descFilePath, nameof(descFilePath));
            var assetHandle = m_assetManager.LoadAssetSync<Object>(descFilePath);
            assetHandle.InstantiateSync();
            T ui = (T)new UGUI(assetHandle.InstantiateSync(), userData);
            Add(ui, layer);
            if (isFullScreen)
            {
                ui.MakeFullScreen();
            }

            return ui;
        }


        /// <summary>
        /// 从UI管理列表中删除所有的UI
        /// </summary>
        public void RemoveAll()
        {
            var tempKv = new Dictionary<string, UGUI>(32);
            foreach (var kv in _dictionary)
            {
                tempKv.Clear();
                foreach (var fui in kv.Value)
                {
                    tempKv[fui.Key] = fui.Value;
                }

                foreach (var fui in tempKv)
                {
                    Remove(fui.Key);
                    fui.Value.Dispose();
                }

                kv.Value.Clear();
            }

            tempKv.Clear();
            _dictionary.Clear();
        }

        private UGUI Add(UGUI ui, UGUILayer layer)
        {
            GameFrameworkGuard.NotNull(ui, nameof(ui));
            if (_dictionary[layer].ContainsKey(ui.Name))
            {
                return _dictionary[layer][ui.Name];
            }

            _dictionary[layer][ui.Name] = ui;
            switch (layer)
            {
                case UGUILayer.Hidden:
                    HiddenRoot.Add(ui);
                    break;
                case UGUILayer.Floor:
                    FloorRoot.Add(ui);
                    break;
                case UGUILayer.Normal:
                    NormalRoot.Add(ui);
                    break;
                case UGUILayer.Fixed:
                    FixedRoot.Add(ui);
                    break;
                case UGUILayer.Window:
                    WindowRoot.Add(ui);
                    break;
                case UGUILayer.Tip:
                    TipRoot.Add(ui);
                    break;
                case UGUILayer.BlackBoard:
                    BlackBoardRoot.Add(ui);
                    break;
                case UGUILayer.Dialogue:
                    DialogueRoot.Add(ui);
                    break;
                case UGUILayer.Guide:
                    GuideRoot.Add(ui);
                    break;
                case UGUILayer.Loading:
                    LoadingRoot.Add(ui);
                    break;
                case UGUILayer.Notify:
                    NotifyRoot.Add(ui);
                    break;
                case UGUILayer.System:
                    SystemRoot.Add(ui);
                    break;
            }

            return ui;
        }

        /// <summary>
        /// 根据UI名称从UI管理列表中移除
        /// </summary>
        /// <param name="uiName"></param>
        /// <returns></returns>
        public bool Remove(string uiName)
        {
            GameFrameworkGuard.NotNullOrEmpty(uiName, nameof(uiName));
            if (SystemRoot.Remove(uiName))
            {
                _dictionary[UGUILayer.System].Remove(uiName);

                return true;
            }

            if (NotifyRoot.Remove(uiName))
            {
                _dictionary[UGUILayer.Notify].Remove(uiName);
                return true;
            }

            if (HiddenRoot.Remove(uiName))
            {
                _dictionary[UGUILayer.Hidden].Remove(uiName);
                return true;
            }

            if (FloorRoot.Remove(uiName))
            {
                _dictionary[UGUILayer.Floor].Remove(uiName);
                return true;
            }

            if (NormalRoot.Remove(uiName))
            {
                _dictionary[UGUILayer.Normal].Remove(uiName);
                return true;
            }

            if (FixedRoot.Remove(uiName))
            {
                _dictionary[UGUILayer.Fixed].Remove(uiName);
                return true;
            }

            if (WindowRoot.Remove(uiName))
            {
                _dictionary[UGUILayer.Window].Remove(uiName);
                return true;
            }

            if (TipRoot.Remove(uiName))
            {
                _dictionary[UGUILayer.Tip].Remove(uiName);
                return true;
            }

            if (BlackBoardRoot.Remove(uiName))
            {
                _dictionary[UGUILayer.BlackBoard].Remove(uiName);
                return true;
            }

            if (DialogueRoot.Remove(uiName))
            {
                _dictionary[UGUILayer.Dialogue].Remove(uiName);
                return true;
            }

            if (GuideRoot.Remove(uiName))
            {
                _dictionary[UGUILayer.Guide].Remove(uiName);
                return true;
            }

            if (LoadingRoot.Remove(uiName))
            {
                _dictionary[UGUILayer.Loading].Remove(uiName);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 根据UI名称和层级从UI管理列表中移除
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <param name="layer">层级</param>
        /// <returns></returns>
        public void Remove(string uiName, UGUILayer layer)
        {
            GameFrameworkGuard.NotNullOrEmpty(uiName, nameof(uiName));
            switch (layer)
            {
                case UGUILayer.Hidden:
                    HiddenRoot.Remove(uiName);
                    _dictionary[UGUILayer.Hidden].Remove(uiName);
                    break;
                case UGUILayer.Floor:
                    FloorRoot.Remove(uiName);
                    _dictionary[UGUILayer.Floor].Remove(uiName);
                    break;
                case UGUILayer.Normal:
                    NormalRoot.Remove(uiName);
                    _dictionary[UGUILayer.Normal].Remove(uiName);
                    break;
                case UGUILayer.Fixed:
                    FixedRoot.Remove(uiName);
                    _dictionary[UGUILayer.Fixed].Remove(uiName);
                    break;
                case UGUILayer.Window:
                    WindowRoot.Remove(uiName);
                    _dictionary[UGUILayer.Window].Remove(uiName);
                    break;
                case UGUILayer.Tip:
                    TipRoot.Remove(uiName);
                    _dictionary[UGUILayer.Tip].Remove(uiName);
                    break;
                case UGUILayer.BlackBoard:
                    BlackBoardRoot.Remove(uiName);
                    _dictionary[UGUILayer.BlackBoard].Remove(uiName);
                    break;
                case UGUILayer.Dialogue:
                    DialogueRoot.Remove(uiName);
                    _dictionary[UGUILayer.Dialogue].Remove(uiName);
                    break;
                case UGUILayer.Guide:
                    GuideRoot.Remove(uiName);
                    _dictionary[UGUILayer.Guide].Remove(uiName);
                    break;
                case UGUILayer.Loading:
                    LoadingRoot.Remove(uiName);
                    _dictionary[UGUILayer.Loading].Remove(uiName);
                    break;
                case UGUILayer.Notify:
                    NotifyRoot.Remove(uiName);
                    _dictionary[UGUILayer.Notify].Remove(uiName);
                    break;
                case UGUILayer.System:
                    SystemRoot.Remove(uiName);
                    _dictionary[UGUILayer.System].Remove(uiName);
                    break;
            }
        }

        /// <summary>
        /// 判断UI名称是否在UI管理列表
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <returns></returns>
        public bool Has(string uiName)
        {
            GameFrameworkGuard.NotNullOrEmpty(uiName, nameof(uiName));
            return Get(uiName) != null;
        }

        /// <summary>
        /// 判断UI是否在UI管理列表，如果存在则返回对象，不存在返回空值
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <param name="fui"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Has<T>(string uiName, out T fui) where T : UGUI
        {
            GameFrameworkGuard.NotNullOrEmpty(uiName, nameof(uiName));
            var ui = Get(uiName);
            fui = ui as T;
            return fui != null;
        }

        /// <summary>
        /// 根据UI名称获取UI对象
        /// </summary>
        /// <param name="uiName">UI名称</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>(string uiName) where T : UGUI
        {
            T fui = default;
            GameFrameworkGuard.NotNullOrEmpty(uiName, nameof(uiName));
            foreach (var kv in _dictionary)
            {
                if (kv.Value.TryGetValue(uiName, out var ui))
                {
                    fui = ui as T;
                    break;
                }
            }

            return fui;
        }

        /// <summary>
        /// 根据UI名称获取UI对象
        /// </summary>
        /// <param name="uiName"></param>
        /// <returns></returns>
        public UGUI Get(string uiName)
        {
            GameFrameworkGuard.NotNullOrEmpty(uiName, nameof(uiName));
            foreach (var kv in _dictionary)
            {
                if (kv.Value.TryGetValue(uiName, out var ui))
                {
                    return ui;
                }
            }

            return null;
        }

        private void OnDestroy()
        {
            RemoveAll();
            _root.Dispose();
            _root = null;
        }
    }
}