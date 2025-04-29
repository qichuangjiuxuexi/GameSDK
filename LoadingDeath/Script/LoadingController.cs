using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace AppBase.LoadingDeath
{
    public class LoadingController
    {
        public List<BaseProgress> progress;
        public Action<float> OnProcess;
        public int CurrentIndex;
        public float totalWeight;
        public float finishWeight;
        public float lastProgress;

        public LoadingController(List<BaseProgress> progress)
        {
            this.progress = progress;
        }


        //开始流程
        public void Start()
        {
            Process().Forget();
        }

        private async UniTask Process()
        {
            progress.ForEach(p => totalWeight+=p.Weight);
            lastProgress = -1;
            for (CurrentIndex = 0; CurrentIndex < progress.Count; CurrentIndex++)
            {
                var nowProgress = progress[CurrentIndex];
                if (nowProgress == null) continue;
                nowProgress.controller = this;
                await nowProgress.Process();
                nowProgress.Progress = 1;
                finishWeight += nowProgress.Weight;
            }
            
            
        }

        /// <summary>
        /// 当前进度
        /// </summary>
        public float Progress
        {
            get
            {
                var nowProgress = progress[CurrentIndex];
                if (nowProgress == null)
                {
                    return finishWeight / totalWeight;
                }

                return (finishWeight + (nowProgress.Progress * nowProgress.Weight)) / totalWeight;
            }
        }

        public void Update()
        {
            //进度和上次记录的一样 滚
            if (Progress == lastProgress) return;
            lastProgress = Progress;
            OnProcess?.Invoke(Progress);
        }
    }
}