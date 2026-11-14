// Stream A. The three shared backup Bytes (PRD 4.5). The only place that raises Snatched, BackupsChanged
// and Collapsed: job shifts, complaints failures and Day 3 leaks all report through ReportSnatch.
using UnityEngine;

namespace ByteWorld
{
    public class BackupNest : MonoBehaviour, IBackupNest
    {
        [SerializeField, Min(0)] int max = 3;

        public int Left { get; private set; }
        public int Max => max;

        void Awake()
        {
            Left = max;
            Services.Register<IBackupNest>(this);
        }

        void OnDestroy() => Services.Unregister<IBackupNest>(this);

        // Let the top bar draw the starting count.
        void Start() => GameEvents.RaiseBackupsChanged(Left, Max);

        public SnatchResult ReportSnatch(SnatchInfo info)
        {
            // A Byte snatched with the nest already empty collapses Byte World.
            if (Left <= 0)
            {
                Debug.Log($"[BackupNest] {info.job} snatched by {info.attacker} with no backups left: collapse");
                GameEvents.RaiseCollapsed(Services.Flow.Day);
                return SnatchResult.Collapsed;
            }

            Left--;
            info.backupsLeft = Left;
            Debug.Log($"[BackupNest] {info.job} snatched by {info.attacker} ({info.source}); backups left {Left}/{Max}");
            GameEvents.RaiseSnatched(info);
            GameEvents.RaiseBackupsChanged(Left, Max);
            return SnatchResult.BackupUsed;
        }

        public void Refill()
        {
            Left = max;
            GameEvents.RaiseBackupsChanged(Left, Max);
        }
    }
}
