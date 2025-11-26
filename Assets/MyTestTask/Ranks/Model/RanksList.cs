using System;
using System.Collections.Generic;
using MyTestTask.Abstraction.Model;
using UnityEngine;

namespace MyTestTask.Ranks.Model
{
    [CreateAssetMenu(menuName = "Ranks/Ranks List")]
    public class RanksList : ScriptableObject, IDataSource<Rank>
    {
        [SerializeField] private Rank[] ranks;
        [SerializeField] private int lastRankExpToNextLevel = 200000;
        public int Count => ranks.Length;

        private void OnEnable()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            for (var i = 0; i < ranks.Length; i++)
            {
                ranks[i].SetSequenceIndex(i);
            }
            int exp = 0;
            for (var i = 1; i < ranks.Length; i++)
            {
                int expToNextLevel = ranks[i].ExperienceLevel - exp;
                ranks[i-1].SetExtToNextLevel(expToNextLevel);
                exp = ranks[i].ExperienceLevel;
            }
            ranks[^1].SetExtToNextLevel(lastRankExpToNextLevel);
        }

        public IEnumerable<Rank> EnumerateRange(int start, int count)
        {
            for (int i = start; i < start + count; i++)
            {
                yield return ranks[i];
            }
        }

        public Rank Get(int index)
        {
            return ranks[index];
        }
    }
}