using MyTestTask.Abstraction.Injection;
using MyTestTask.Abstraction.Model;
using MyTestTask.Ranks.Model;
using MyTestTask.Ranks.View;
using UnityEngine;

namespace MyTestTask.Ranks.Injection
{
    public class RanksListViewRunner : MonoBehaviour, IInjectionTarget
    {
        [SerializeField] private RanksListView ranksListView;
        private IDataSource<Rank> _ranksDataSource;
        
        public void Inject(DiContainer container)
        {
            _ranksDataSource = container.Resolve<IDataSource<Rank>>();
        }

        private void Start()
        {
            ranksListView.SetDataSource(_ranksDataSource);
        }
    }
}