using MyTestTask.Abstraction.Injection;
using MyTestTask.Abstraction.Model;
using MyTestTask.Ranks.Model;
using UnityEngine;

namespace MyTestTask.Ranks.Injection
{
    public class RanksListInstaller : MonoBehaviour, IInstaller
    {
        [SerializeField] private RanksList ranksList;
        public void InstallBindings(DiContainer container)
        {
            container.BindInstance<IDataSource<Rank>,RanksList>(ranksList);
        }
    }
}