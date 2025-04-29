using Cysharp.Threading.Tasks;

namespace AppBase.LoadingDeath
{
    public class BaseProgress
    {
        public LoadingController controller;
        public float Weight  {set; get;}
        public float Progress // 0->1
        {
            get => _progress;
            set
            {
                _progress = value;
                controller.Update();
            }
        }
        private float _progress;
        
        protected BaseProgress(float weight)
        {
            Weight = weight;
        }

        public void SetProgress(float progress)
        {
            Progress = progress;
        }

        public virtual async UniTask Process()
        {
            
        }
    }
}