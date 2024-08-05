namespace GameFrameX.UGUI.Runtime
{
    public sealed class UGUIManager : GameFrameworkModule, IUGUIManager
    {
        public void Hide(string panelName)
        {
        }

        public void Show(string panelName)
        {
        }

        /// <summary>
        /// 全局配置管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        protected override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 关闭并清理全局配置管理器。
        /// </summary>
        protected override void Shutdown()
        {
        }
    }
}