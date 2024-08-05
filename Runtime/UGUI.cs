﻿using System;
using System.Collections.Generic;
using System.Linq;
using GameFrameX.ObjectPool;
using GameFrameX.Runtime;
using UnityEngine;

namespace GameFrameX.UGUI.Runtime
{
    public class UGUI : ObjectBase, IDisposable
    {
        protected object UserData { get; private set; }

        /// <summary>
        /// 界面显示之后触发
        /// </summary>
        public Action<UGUI> OnShowAction { get; set; }

        /// <summary>
        /// 界面隐藏之前触发
        /// </summary>
        public Action<UGUI> OnHideAction { get; set; }

        /// <summary>
        /// 记录初始化UI是否是显示的状态
        /// </summary>
        private bool IsInitVisible { get; }

        public UGUI(GameObject gObject, object userData = null, bool isRoot = false)
        {
            UserData      = userData;
            GObject       = gObject;
            IsInitVisible = GObject.activeSelf;
            IsRoot        = isRoot;
            InitView();
            // 在初始化的时候先隐藏UI。后续由声明周期控制
            // if (parent == null)
            // {
            // SetVisibleWithNoNotify(false);
            // }

            Init();

            // parent?.Add(this);

            if (gObject.name.IsNullOrWhiteSpace())
            {
                Name = GetType().Name;
            }
            else
            {
                Name = gObject.name;
            }
        }

        protected virtual void InitView()
        {
        }

        /// <summary>
        /// 界面添加到UI系统之前执行
        /// </summary>
        protected virtual void Init()
        {
            // Log.Info("Init " + Name);
        }

        /// <summary>
        /// 界面显示后执行
        /// </summary>
        protected virtual void OnShow()
        {
            // Log.Info("OnShow " + Name);
        }


        /// <summary>
        /// 界面显示之后执行，设置数据和多语言建议在这里设置
        /// </summary>
        public virtual void Refresh()
        {
            // Log.Info("Refresh " + Name);
        }

        /// <summary>
        /// 界面隐藏之前执行
        /// </summary>
        protected virtual void OnHide()
        {
            // Log.Info("OnHide " + Name);
        }

        /// <summary>
        /// UI 对象销毁之前执行
        /// </summary>
        protected virtual void OnDispose()
        {
            // Log.Info("OnDispose " + Name);
        }

        /// <summary>
        /// 显示UI
        /// </summary>
        public void Show(object userData = null)
        {
            UserData = userData;
            // Log.Info("Show " + Name);
            if (Visible)
            {
                OnShowAction?.Invoke(this);
                OnShow();
                Refresh();
                return;
            }

            Visible = true;
        }


        /// <summary>
        /// 隐藏UI
        /// </summary>
        public void Hide()
        {
            // Log.Info("Hide " + Name);
            if (!Visible)
            {
                OnHideAction?.Invoke(this);
                OnHide();
                return;
            }

            Visible = false;
        }

        /// <summary>
        /// UI 对象
        /// </summary>
        public GameObject GObject { get; }

        /// <summary>
        /// 是否是UI根
        /// </summary>
        public bool IsRoot { get; private set; }

        /// <summary>
        /// UI 变换对象
        /// </summary>
        public Transform Transform
        {
            get { return GObject?.transform; }
        }

        /// <summary>
        /// UI 名称
        /// </summary>
        public sealed override string Name
        {
            get
            {
                if (GObject == null)
                {
                    return string.Empty;
                }

                return GObject.name;
            }

            protected set
            {
                if (GObject == null)
                {
                    return;
                }

                if (GObject.name != null && GObject.name == value)
                {
                    return;
                }

                GObject.name = value;
            }
        }

        protected override void Release(bool isShutdown)
        {
        }

        /// <summary>
        /// 设置UI的显示状态，不发出事件
        /// </summary>
        /// <param name="value"></param>
        private void SetVisibleWithNoNotify(bool value)
        {
            if (GObject.activeSelf == value)
            {
                return;
            }

            GObject.SetActive(value);
        }

        /// <summary>
        /// 获取UI是否显示
        /// </summary>
        public bool IsVisible => Visible;

        private bool Visible
        {
            get
            {
                if (GObject == null)
                {
                    return false;
                }

                return GObject.activeSelf;
            }
            set
            {
                if (GObject == null)
                {
                    return;
                }

                if (GObject.activeSelf == value)
                {
                    return;
                }

                if (value == false)
                {
                    foreach (var child in this._children)
                    {
                        child.Value.Visible = value;
                    }

                    OnHideAction?.Invoke(this);
                    OnHide();
                }

                GObject.SetActive(value);
                if (value)
                {
                    foreach (var child in this._children)
                    {
                        child.Value.Visible = value;
                    }

                    OnShowAction?.Invoke(this);
                    OnShow();
                    Refresh();
                }
            }
        }


        /// <summary>
        /// 界面对象是否为空
        /// </summary>
        public bool IsEmpty
        {
            get { return GObject == null; }
        }

        private readonly Dictionary<string, UGUI> _children = new Dictionary<string, UGUI>();

        /// <summary>
        /// 是否从对象池获取
        /// </summary>
        protected bool IsFromPool { get; set; }


        protected bool IsDisposed;

        /// <summary>
        /// 销毁UI对象
        /// </summary>
        public virtual void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
            // 删除所有的孩子
            DisposeChildren();

            // 删除自己的UI
            /*if (!IsRoot)
            {
                RemoveFromParent();
            }*/

            // 释放UI
            OnDispose();
            // 删除自己的UI
            /*if (!IsRoot)
            {
                GObject.DestroyObject();
            }*/

            OnShowAction = null;
            OnHideAction = null;
            Parent       = null;
            // isFromFGUIPool = false;
        }

        /// <summary>
        /// 添加UI对象到子级列表
        /// </summary>
        /// <param name="ui"></param>
        /// <param name="index">添加到的目标UI层级索引位置</param>
        /// <exception cref="Exception"></exception>
        public void Add(UGUI ui, int index = -1)
        {
            if (ui == null || ui.IsEmpty)
            {
                throw new Exception($"ui can not be empty");
            }

            if (string.IsNullOrWhiteSpace(ui.Name))
            {
                throw new Exception($"ui.Name can not be empty");
            }

            if (_children.ContainsKey(ui.Name))
            {
                throw new Exception($"ui.Name({ui.Name}) already exist");
            }

            _children.Add(ui.Name, ui);
            if (index < 0 || index > _children.Count)
            {
                ui.Transform.SetParent(Transform);
            }
            else
            {
                ui.Transform.SetParent(Transform);
                ui.Transform.SetSiblingIndex(index);
            }

            ui.Parent = this;

            if (ui.IsInitVisible)
            {
                // 显示UI
                ui.Show(ui.UserData);
            }
        }

        /// <summary>
        /// UI 父级对象
        /// </summary>
        public UGUI Parent { get; protected set; }

        /// <summary>
        /// 设置当前UI对象为全屏
        /// </summary>
        public void MakeFullScreen()
        {
            var rectTransform = GObject.GetOrAddComponent<RectTransform>();
            rectTransform.anchorMin        = Vector2.zero;
            rectTransform.anchorMax        = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta        = Vector2.zero;
        }

        /// <summary>
        /// 将自己从父级UI对象删除
        /// </summary>
        public void RemoveFromParent()
        {
            Parent?.Remove(Name);
        }

        /// <summary>
        /// 删除指定UI名称的UI对象
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Remove(string name)
        {
            if (_children.TryGetValue(name, out var ui))
            {
                _children.Remove(name);

                if (ui != null)
                {
                    ui.RemoveChildren();

                    ui.Hide();

                    // if (IsComponent)
                    // {
                    //     GObject.asCom.RemoveChild(ui.GObject);
                    // }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 销毁所有自己对象
        /// </summary>
        public void DisposeChildren()
        {
            if (_children.Count > 0)
            {
                var children = GetAll();
                foreach (var child in children)
                {
                    child.Dispose();
                }

                _children.Clear();
            }
        }

        /// <summary>
        /// 删除所有子级UI对象
        /// </summary>
        public void RemoveChildren()
        {
            if (_children.Count > 0)
            {
                var children = GetAll();

                foreach (var child in children)
                {
                    child.RemoveFromParent();
                }

                _children.Clear();
            }
        }

        /// <summary>
        /// 根据 UI名称 获取子级UI对象
        /// </summary>
        /// <param name="name">UI名称</param>
        /// <returns></returns>
        public UGUI Get(string name)
        {
            if (_children.TryGetValue(name, out var child))
            {
                return child;
            }

            return null;
        }

        /// <summary>
        /// 获取所有的子级UI，非递归
        /// </summary>
        /// <returns></returns>
        public UGUI[] GetAll()
        {
            return _children.Values.ToArray();
        }
    }
}