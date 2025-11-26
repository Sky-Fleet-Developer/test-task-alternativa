using System;
using UnityEngine;

namespace MyTestTask.Ranks.Model
{
    [Serializable]
    public class Rank
    {
        [SerializeField] private string id;
        public string Id => id;
        private int _sequenceIndex;
        public int SequenceIndex => _sequenceIndex;
        public void SetSequenceIndex(int index) => _sequenceIndex = index;
    }
}