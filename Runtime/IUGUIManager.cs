namespace GameFrameX.UGUI.Runtime
{
    /// <summary>
    /// UGUI接口。
    /// </summary>
    public interface IUGUIManager
    {
        /// <summary>
        /// 打开UI界面
        /// </summary>
        /// <param name="panelName"></param>
        void Show(string panelName);

        /// <summary>
        /// 关闭UI界面
        /// </summary>
        /// <param name="panelName"></param>
        void Hide(string panelName);
    }
}