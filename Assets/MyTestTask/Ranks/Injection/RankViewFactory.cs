using MyTestTask.Abstraction.Injection;
using MyTestTask.Abstraction.Patterns;
using MyTestTask.Abstraction.View;
using MyTestTask.Abstraction.View.Layout;
using MyTestTask.Ranks.Model;
using MyTestTask.Ranks.View;
using UnityEngine;

namespace MyTestTask.Ranks.Injection
{
    public class RankViewFactory : MonoBehaviour, IFactory<IDataListFlexibleElement<Rank>>, IInstaller
    {
        [SerializeField] private RankView rankViewPrefab;
        
        public void InstallBindings(DiContainer container)
        {
            container.BindInstance<IFactory<IDataListFlexibleElement<Rank>>,RankViewFactory>(this);
        }
        
        public IDataListFlexibleElement<Rank> Create()
        {
            return Instantiate(rankViewPrefab);
        }
    }
}