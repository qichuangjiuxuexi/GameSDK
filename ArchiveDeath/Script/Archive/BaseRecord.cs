using AppBase;
using AppBase.Module;

namespace AppBase.ArchiveDeath
{
    public class BaseRecord<T> : ModuleBase where T : BaseArchiveData, new()
    {
        /// <summary>
        /// 存档名称
        /// </summary>
        private string archiveName;
        public string ArchiveName
        {
            get => archiveName ?? GetType().Name;
            protected set => archiveName = value;
        }

        private T archiveData;
        public T ArchiveData => archiveData;

        public bool IsNewRecord;

        protected override void OnInit()
        {
            base.OnInit();
            archiveData = GameBase.Instance.GetModule<ArchiveManager>().GetArchiveData<T>(ArchiveName);
            if (archiveData == null)
            {
                IsNewRecord = true;
                archiveData = new T();
                GameBase.Instance.GetModule<ArchiveManager>().UpdateArchiveData(ArchiveName, archiveData);
                OnNewRecord();
            }
            else
            {
                IsNewRecord = false;
                OnLoadRecord();
            }
        }
        /// <summary>
        /// 当新建存档时调用
        /// </summary>
        protected virtual void OnNewRecord()
        {
        }
        /// <summary>
        /// 当加载存档时调用
        /// </summary>
        private void OnLoadRecord()
        {
        }

        public void Save()
        {
            GameBase.Instance.GetModule<ArchiveManager>().Save(ArchiveName);
        }
    }
}
